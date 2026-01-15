using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AppointmentService.Services;

/// <summary>
/// RabbitMQ RPC client to request patient profile data from PatientService
/// </summary>
public interface IPatientProfileClient
{
    Task<List<PatientProfileDto>> GetPatientProfilesAsync(IEnumerable<Guid> patientIds, CancellationToken ct = default);
}

public class PatientProfileClient : IPatientProfileClient, IDisposable
{
    private readonly IConnection _connection;
    private readonly ILogger<PatientProfileClient> _logger;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> _pendingRequests = new();
    private IChannel? _channel;
    private string? _replyQueueName;
    private bool _initialized;
    private readonly SemaphoreSlim _initLock = new(1, 1);

    private const string ExchangeName = "patient.rpc";
    private const string RoutingKey = "patient.profile.request";
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(10);

    public PatientProfileClient(IConnection connection, ILogger<PatientProfileClient> logger)
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
            _logger.LogInformation("[PatientProfileClient] Initialized with reply queue {Queue}", _replyQueueName);
        }
        finally
        {
            _initLock.Release();
        }
    }

    public async Task<List<PatientProfileDto>> GetPatientProfilesAsync(IEnumerable<Guid> patientIds, CancellationToken ct = default)
    {
        var patientIdList = patientIds.ToList();
        if (patientIdList.Count == 0)
        {
            return new List<PatientProfileDto>();
        }

        try
        {
            await EnsureInitializedAsync(ct);

            var correlationId = Guid.NewGuid().ToString();
            var tcs = new TaskCompletionSource<string>();
            _pendingRequests[correlationId] = tcs;

            var request = new { PatientIds = patientIdList };
            var requestJson = JsonSerializer.Serialize(request);
            var requestBytes = Encoding.UTF8.GetBytes(requestJson);

            var props = new BasicProperties
            {
                CorrelationId = correlationId,
                ReplyTo = _replyQueueName,
                ContentType = "application/json"
            };

            _logger.LogInformation("[PatientProfileClient] Requesting profiles for {Count} patients", patientIdList.Count);

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
                var response = JsonSerializer.Deserialize<PatientProfileResponse>(responseJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                _logger.LogInformation("[PatientProfileClient] Received {Count} profiles", response?.Profiles?.Count ?? 0);
                return response?.Profiles ?? new List<PatientProfileDto>();
            }
            catch (OperationCanceledException)
            {
                _pendingRequests.TryRemove(correlationId, out _);
                _logger.LogWarning("[PatientProfileClient] Request timed out after {Timeout}s", RequestTimeout.TotalSeconds);
                return new List<PatientProfileDto>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PatientProfileClient] Failed to get patient profiles");
            return new List<PatientProfileDto>();
        }
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _initLock.Dispose();
    }
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
