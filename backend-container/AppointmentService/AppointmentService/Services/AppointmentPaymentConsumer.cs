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
    private IModel? _channel;

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
            _channel = _conn.CreateModel();
            _channel.ExchangeDeclare("billing.events", ExchangeType.Topic, durable: true);
            _channel.QueueDeclare("appointment.billing.updates", durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind("appointment.billing.updates", "billing.events", "billing.appointment_payment_processed");
            
            Console.WriteLine("[ApptPaymentConsumer] ✅ Connected and bound to billing.appointment_payment_processed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to setup RabbitMQ");
            return;
        }

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
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
                    await UpdateAppointmentAsync(evt);
                }
                
                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message");
                _channel.BasicNack(ea.DeliveryTag, false, false);
            }
        };

        _channel.BasicConsume("appointment.billing.updates", false, consumer);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task UpdateAppointmentAsync(AppointmentBillingProcessed evt)
    {
        using var scope = _sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppointmentDbContext>();

        var appt = await db.Appointments.FindAsync(evt.AppointmentId);
        if (appt == null)
        {
            _logger.LogWarning("Appointment {Id} not found during payment update", evt.AppointmentId);
            return;
        }

        appt.IsPaid = evt.IsPaid;
        appt.PaymentProcessed = true; // Use this to stop showing "Pending" if we wanted
        appt.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        _logger.LogInformation("Updated Appointment {Id} -> IsPaid: {IsPaid}", appt.Id, appt.IsPaid);
    }
    
    public override void Dispose()
    {
        _channel?.Dispose();
        base.Dispose();
    }
}
