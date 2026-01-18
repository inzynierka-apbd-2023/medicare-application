using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using PatientService.Data;
using PatientService.Models;

namespace PatientService.Infrastructure.Messaging;

public class PatientDetailsRequestConsumer : BackgroundService
{
    private readonly IServiceProvider _sp;
    private readonly RabbitMQ.Client.IConnection _connection;
    private IChannel? _channel;
    private const string ExchangeName = "patient.rpc";
    private const string QueueName = "patient.profile.requests";
    private const string RoutingKey = "patient.profile.request";

    public PatientDetailsRequestConsumer(IServiceProvider sp, RabbitMQ.Client.IConnection connection)
    {
        _sp = sp;
        _connection = connection;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);
            
            await _channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Direct, durable: true, cancellationToken: stoppingToken);
            await _channel.QueueDeclareAsync(QueueName, durable: false, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);
            await _channel.QueueBindAsync(QueueName, ExchangeName, RoutingKey, cancellationToken: stoppingToken);
            
            await _channel.BasicQosAsync(0, 10, false, stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (_, ea) =>
            {
                try
                {
                    var props = ea.BasicProperties;
                    var replyTo = props?.ReplyTo;
                    var correlationId = props?.CorrelationId;

                    if (string.IsNullOrEmpty(replyTo) || string.IsNullOrEmpty(correlationId))
                    {
                        // Cannot reply
                        await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
                        return;
                    }

                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var request = JsonSerializer.Deserialize<PatientProfileRequest>(message, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (request == null || request.PatientIds == null || !request.PatientIds.Any())
                    {
                        // Empty response
                        await SendReplyAsync(new PatientProfileResponse(), replyTo, correlationId, stoppingToken);
                        await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
                        return;
                    }

                    using var scope = _sp.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<PatientDbContext>();

                    var profiles = await db.Set<PatientOverview>()
                        .Where(p => request.PatientIds.Contains(p.PatientId))
                        .ToListAsync(stoppingToken);

                    var response = new PatientProfileResponse
                    {
                        Profiles = profiles.Select(p => new PatientProfileDto
                        {
                            PatientId = p.PatientId,
                            UserId = p.UserId,
                            FirstName = p.FirstName ?? "",
                            LastName = p.LastName ?? "",
                            Email = p.Email ?? "",
                            Phone = p.Phone ?? "",
                            DateOfBirth = p.DateOfBirth
                        }).ToList()
                    };

                    await SendReplyAsync(response, replyTo, correlationId, stoppingToken);
                    await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[PatientDetailsRequestConsumer] Error processing request: {ex.Message}");
                    // Ack to prevent endless loops on bad message format, or Nack if transient
                    await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
                }
            };

            await _channel.BasicConsumeAsync(QueueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);
            
            // Keep running
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PatientDetailsRequestConsumer] Startup failed: {ex.Message}");
        }
    }

    private async Task SendReplyAsync(PatientProfileResponse response, string replyTo, string correlationId, CancellationToken ct)
    {
        if (_channel == null || _channel.IsClosed) return;

        var json = JsonSerializer.Serialize(response);
        var body = Encoding.UTF8.GetBytes(json);
        var props = new BasicProperties
        {
            CorrelationId = correlationId,
            ContentType = "application/json"
        };

        await _channel.BasicPublishAsync(exchange: "", routingKey: replyTo, mandatory: false, basicProperties: props, body: body, cancellationToken: ct);
    }
}

public class PatientProfileRequest
{
    public List<Guid> PatientIds { get; set; } = new();
}

public class PatientProfileResponse
{
    public List<PatientProfileDto> Profiles { get; set; } = new();
}

public class PatientProfileDto
{
    public Guid PatientId { get; set; }
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public DateTime? DateOfBirth { get; set; }
}
