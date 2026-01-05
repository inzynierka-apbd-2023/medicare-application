using AppointmentService.Data;
using AppointmentService.Models;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace AppointmentService.Services;

public record AppointmentBillingProcessed(Guid AppointmentId, bool IsPaid, long AmountCents, string? PlanCode);

public class AppointmentPaymentConsumer : BackgroundService
{
    private readonly IServiceProvider _sp;
    private readonly ILogger<AppointmentPaymentConsumer> _logger;
    private readonly IConnection _conn;
    private IChannel? _channel;

    public AppointmentPaymentConsumer(IServiceProvider sp, IConnection conn, ILogger<AppointmentPaymentConsumer> logger)
    {
        _sp = sp;
        _conn = conn;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("[ApptPaymentConsumer] 🔌 Connecting...");
        try
        {
            _channel = await _conn.CreateChannelAsync(cancellationToken: stoppingToken);
            await _channel.ExchangeDeclareAsync("billing.events", ExchangeType.Topic, durable: true, cancellationToken: stoppingToken);
            await _channel.QueueDeclareAsync("appointment.billing.updates", durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: stoppingToken);
            await _channel.QueueBindAsync("appointment.billing.updates", "billing.events", "billing.appointment_payment_processed", arguments: null, cancellationToken: stoppingToken);
            
            Console.WriteLine("[ApptPaymentConsumer] ✅ Connected and bound to billing.appointment_payment_processed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to setup RabbitMQ");
            return;
        }

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var msg = Encoding.UTF8.GetString(body);
                Console.WriteLine($"[ApptPaymentConsumer] 📨 Received update: {msg}");
                
                var evt = JsonSerializer.Deserialize<AppointmentBillingProcessed>(msg, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (evt != null)
                {
                    Console.WriteLine($"[ApptPaymentConsumer] 🔄 Updating Appt {evt.AppointmentId} -> IsPaid={evt.IsPaid}");
                    _logger.LogInformation("Processing payment update for appt {Id}: IsPaid={IsPaid}", evt.AppointmentId, evt.IsPaid);
                    await UpdateAppointmentAsync(evt, stoppingToken);
                }
                
                await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message");
                await _channel.BasicNackAsync(ea.DeliveryTag, false, false, stoppingToken);
            }
        };

        await _channel.BasicConsumeAsync("appointment.billing.updates", false, consumer, cancellationToken: stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task UpdateAppointmentAsync(AppointmentBillingProcessed evt, CancellationToken stoppingToken)
    {
        using var scope = _sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppointmentDbContext>();

        var appt = await db.Appointments.FindAsync(new object[] { evt.AppointmentId }, stoppingToken);
        if (appt == null)
        {
            _logger.LogWarning("Appointment {Id} not found during payment update", evt.AppointmentId);
            return;
        }

        appt.IsPaid = evt.IsPaid;
        appt.PaymentProcessed = true; // Use this to stop showing "Pending" if we wanted
        appt.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(stoppingToken);
        _logger.LogInformation("Updated Appointment {Id} -> IsPaid: {IsPaid}", appt.Id, appt.IsPaid);
    }
    
    public override void Dispose()
    {
        // _channel?.Dispose();
        base.Dispose();
    }
}
