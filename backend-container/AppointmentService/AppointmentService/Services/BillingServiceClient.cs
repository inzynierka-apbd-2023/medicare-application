using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace AppointmentService.Services;

public interface IBillingServiceClient
{
    Task<(bool Success, string Error)> RecordMockPaymentAsync(Guid appointmentId, Guid patientId, string method);
}

public class BillingServiceClient : IBillingServiceClient
{
    private readonly IConnection _mqConnection;
    private readonly ILogger<BillingServiceClient> _logger;

    public BillingServiceClient(IConnection mqConnection, ILogger<BillingServiceClient> logger)
    {
        _mqConnection = mqConnection;
        _logger = logger;
    }

    public async Task<(bool Success, string Error)> RecordMockPaymentAsync(Guid appointmentId, Guid patientId, string method)
    {
        try
        {
            await using var channel = await _mqConnection.CreateChannelAsync();
            await channel.ExchangeDeclareAsync("billing.events", ExchangeType.Topic, durable: true);

            var evt = new
            {
                AppointmentId = appointmentId,
                PatientId = patientId,
                PaymentMethod = method,
                Timestamp = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(evt);
            var body = Encoding.UTF8.GetBytes(json);
            var props = new BasicProperties { Persistent = true };

            await channel.BasicPublishAsync(
                exchange: "billing.events",
                routingKey: "billing.payment_initiated", // specific key
                mandatory: false,
                basicProperties: props,
                body: body);
            
            _logger.LogInformation("Published billing.payment_initiated for Appt {Id}", appointmentId);
            return (true, string.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish payment initiation event");
            return (false, "Failed to initiate payment: " + ex.Message);
        }
    }
}
