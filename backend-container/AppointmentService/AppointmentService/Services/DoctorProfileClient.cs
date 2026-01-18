using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AppointmentService.Services;

/// <summary>
/// RabbitMQ RPC client to request doctor profile data from PractitionerService
/// </summary>
public interface IDoctorProfileClient
{
    Task<List<DoctorProfileDto>> GetDoctorProfilesAsync(IEnumerable<Guid> doctorIds, CancellationToken ct = default);
}

public class DoctorProfileClient : IDoctorProfileClient, IDisposable
{
    private readonly IConnection _connection;
    private readonly ILogger<DoctorProfileClient> _logger;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> _pendingRequests = new();
    private IChannel? _channel;
    private string? _replyQueueName;
    private bool _initialized;
    private readonly SemaphoreSlim _initLock = new(1, 1);

    private const string ExchangeName = "practitioner.rpc";
    private const string RoutingKey = "doctor.profile.request";
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(10);

    public DoctorProfileClient(IConnection connection, ILogger<DoctorProfileClient> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    private async Task EnsureInitializedAsync(CancellationToken ct)
    {
        if (_initialized) return;

        await _initLock.WaitAsync(ct);
        try
        {
            if (_initialized) return;

            _channel = await _connection.CreateChannelAsync(cancellationToken: ct);
            await _channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Direct, durable: true, cancellationToken: ct);

            // Create exclusive reply queue
            var queueResult = await _channel.QueueDeclareAsync(queue: "", durable: false, exclusive: true, autoDelete: true, cancellationToken: ct);
            _replyQueueName = queueResult.QueueName;

            // Set up consumer for replies
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += (_, ea) =>
            {
                var correlationId = ea.BasicProperties?.CorrelationId;
                if (!string.IsNullOrEmpty(correlationId) && _pendingRequests.TryRemove(correlationId, out var tcs))
                {
                    var responseJson = Encoding.UTF8.GetString(ea.Body.ToArray());
                    tcs.TrySetResult(responseJson);
                }
                return Task.CompletedTask;
            };

            await _channel.BasicConsumeAsync(_replyQueueName, autoAck: true, consumer: consumer, cancellationToken: ct);
            _initialized = true;
            _logger.LogInformation("[DoctorProfileClient] Initialized with reply queue {Queue}", _replyQueueName);
        }
        finally
        {
            _initLock.Release();
        }
    }

    public async Task<List<DoctorProfileDto>> GetDoctorProfilesAsync(IEnumerable<Guid> doctorIds, CancellationToken ct = default)
    {
        var doctorIdList = doctorIds.ToList();
        if (doctorIdList.Count == 0)
        {
            return new List<DoctorProfileDto>();
        }

        try
        {
            await EnsureInitializedAsync(ct);

            var correlationId = Guid.NewGuid().ToString();
            var tcs = new TaskCompletionSource<string>();
            _pendingRequests[correlationId] = tcs;

            var request = new { DoctorIds = doctorIdList };
            var requestJson = JsonSerializer.Serialize(request);
            var requestBytes = Encoding.UTF8.GetBytes(requestJson);

            var props = new BasicProperties
            {
                CorrelationId = correlationId,
                ReplyTo = _replyQueueName,
                ContentType = "application/json"
            };

            _logger.LogInformation("[DoctorProfileClient] Requesting profiles for {Count} doctors", doctorIdList.Count);

            await _channel!.BasicPublishAsync(
                exchange: ExchangeName,
                routingKey: RoutingKey,
                mandatory: false,
                basicProperties: props,
                body: requestBytes,
                cancellationToken: ct);

            // Wait for response with timeout
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(RequestTimeout);

            try
            {
                var responseJson = await tcs.Task.WaitAsync(cts.Token);
                var response = JsonSerializer.Deserialize<DoctorProfileResponse>(responseJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                _logger.LogInformation("[DoctorProfileClient] Received {Count} profiles", response?.Profiles?.Count ?? 0);
                return response?.Profiles ?? new List<DoctorProfileDto>();
            }
            catch (OperationCanceledException)
            {
                _pendingRequests.TryRemove(correlationId, out _);
                _logger.LogWarning("[DoctorProfileClient] Request timed out after {Timeout}s", RequestTimeout.TotalSeconds);
                return new List<DoctorProfileDto>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[DoctorProfileClient] Failed to get doctor profiles");
            return new List<DoctorProfileDto>();
        }
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _initLock.Dispose();
    }
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
