using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PractitionerService.Data;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PractitionerService.Services;

/// <summary>
/// Handles RPC requests for doctor profile data from other services (e.g., AppointmentService analytics)
/// </summary>
public class DoctorProfileRequestHandler : BackgroundService
{
    private readonly IConnection _connection;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DoctorProfileRequestHandler> _logger;
    private IChannel? _channel;

    private const string ExchangeName = "practitioner.rpc";
    private const string RequestQueueName = "practitioner.doctor-profile.requests";

    public DoctorProfileRequestHandler(IConnection connection, IServiceProvider serviceProvider, ILogger<DoctorProfileRequestHandler> logger)
    {
        _connection = connection;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);
                
                // Declare exchange and queue
                await _channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Direct, durable: true, cancellationToken: stoppingToken);
                await _channel.QueueDeclareAsync(RequestQueueName, durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);
                await _channel.QueueBindAsync(RequestQueueName, ExchangeName, "doctor.profile.request", cancellationToken: stoppingToken);
                await _channel.BasicQosAsync(0, 10, false, stoppingToken);

                _logger.LogInformation("[DoctorProfileRequestHandler] Listening on queue {Queue}", RequestQueueName);

                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.ReceivedAsync += async (_, ea) =>
                {
                    var replyProps = new BasicProperties
                    {
                        CorrelationId = ea.BasicProperties?.CorrelationId
                    };

                    try
                    {
                        var requestJson = Encoding.UTF8.GetString(ea.Body.ToArray());
                        _logger.LogInformation("[DoctorProfileRequestHandler] Received request: {Request}", requestJson);

                        var request = JsonSerializer.Deserialize<DoctorProfileRequest>(requestJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        
                        var response = await ProcessRequest(request, stoppingToken);
                        var responseJson = JsonSerializer.Serialize(response);
                        var responseBytes = Encoding.UTF8.GetBytes(responseJson);

                        // Send reply
                        if (!string.IsNullOrEmpty(ea.BasicProperties?.ReplyTo))
                        {
                            await _channel.BasicPublishAsync(
                                exchange: "",
                                routingKey: ea.BasicProperties.ReplyTo,
                                mandatory: false,
                                basicProperties: replyProps,
                                body: responseBytes,
                                cancellationToken: stoppingToken);
                            _logger.LogInformation("[DoctorProfileRequestHandler] Sent reply with {Count} profiles", response.Profiles.Count);
                        }

                        await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[DoctorProfileRequestHandler] Error processing request");
                        await _channel.BasicNackAsync(ea.DeliveryTag, false, false, stoppingToken);
                    }
                };

                await _channel.BasicConsumeAsync(RequestQueueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);
                
                // Keep running
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DoctorProfileRequestHandler] Connection failure; retrying in 5s");
                await Task.Delay(5000, stoppingToken);
            }
        }
    }

    private async Task<DoctorProfileResponse> ProcessRequest(DoctorProfileRequest? request, CancellationToken ct)
    {
        var response = new DoctorProfileResponse();

        if (request?.DoctorIds == null || request.DoctorIds.Count == 0)
        {
            return response;
        }

        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PractitionerDbContext>();

        // Query DoctorDirectory view
        var directories = await db.Set<Models.DoctorDirectory>()
            .AsNoTracking()
            .Where(d => request.DoctorIds.Contains(d.DoctorId) || request.DoctorIds.Contains(d.UserId))
            .ToListAsync(ct);

        foreach (var d in directories)
        {
            response.Profiles.Add(new DoctorProfileDto
            {
                DoctorId = d.DoctorId,
                UserId = d.UserId,
                FirstName = d.FirstName ?? "",
                LastName = d.LastName ?? "",
                Email = d.Email ?? "",
                SpecializationNames = d.Specializations ?? ""
            });
        }

        return response;
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        base.Dispose();
    }
}

public class DoctorProfileRequest
{
    public List<Guid> DoctorIds { get; set; } = new();
}

public class DoctorProfileResponse
{
    public List<DoctorProfileDto> Profiles { get; set; } = new();
}

public class DoctorProfileDto
{
    public Guid DoctorId { get; set; }
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public string SpecializationNames { get; set; } = "";
}
