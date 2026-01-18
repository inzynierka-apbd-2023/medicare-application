using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PractitionerService.Data;
using PractitionerService.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PractitionerService.Services;

public class AppointmentEventListener : BackgroundService
{
    private readonly IConnection _connection;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AppointmentEventListener> _logger;
    private IChannel? _channel;

    public AppointmentEventListener(IConnection connection, IServiceProvider serviceProvider, ILogger<AppointmentEventListener> logger)
    {
        _connection = connection;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);
        await _channel.ExchangeDeclareAsync("appointment.events", ExchangeType.Topic, durable: true, cancellationToken: stoppingToken);
        
        var queueName = (await _channel.QueueDeclareAsync(durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken)).QueueName;
        
        await _channel.QueueBindAsync(queueName, "appointment.events", "appointment.created", cancellationToken: stoppingToken);
        await _channel.QueueBindAsync(queueName, "appointment.events", "appointment.updated", cancellationToken: stoppingToken);
        await _channel.QueueBindAsync(queueName, "appointment.events", "appointment.rated", cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var routingKey = ea.RoutingKey;

                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<PractitionerDbContext>();

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                if (routingKey == "appointment.created")
                {
                    var evt = JsonSerializer.Deserialize<AppointmentCreatedEvent>(message, options);
                    if (evt != null) await HandleAppointmentCreated(db, evt);
                }
                else if (routingKey == "appointment.updated")
                {
                    var evt = JsonSerializer.Deserialize<AppointmentUpdatedEvent>(message, options);
                    if (evt != null) await HandleAppointmentUpdated(db, evt);
                }
                else if (routingKey == "appointment.rated")
                {
                    var evt = JsonSerializer.Deserialize<AppointmentRatedEvent>(message, options);
                    if (evt != null) await HandleAppointmentRated(db, evt);
                }

                await _channel.BasicAckAsync(ea.DeliveryTag, false, cancellationToken: stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message");
                // Requeue or Dead Letter logic could go here
                await _channel.BasicNackAsync(ea.DeliveryTag, false, false, cancellationToken: stoppingToken);
            }
        };

        await _channel.BasicConsumeAsync(queueName, false, consumer, cancellationToken: stoppingToken);
        
        // Keep running until cancellation
        try 
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // Graceful shutdown
        }
    }

    private async Task HandleAppointmentCreated(PractitionerDbContext db, AppointmentCreatedEvent evt)
    {
        var stats = await db.DoctorStatistics.FindAsync(evt.DoctorId);
        if (stats == null)
        {
            stats = new DoctorStatistics { DoctorId = evt.DoctorId, TotalAppointments = 1 };
            db.DoctorStatistics.Add(stats);
        }
        else
        {
            stats.TotalAppointments++;
            stats.UpdatedAt = DateTime.UtcNow;
        }
        await db.SaveChangesAsync();
    }

    private async Task HandleAppointmentUpdated(PractitionerDbContext db, AppointmentUpdatedEvent evt)
    {
        if (string.Equals(evt.Status, "Completed", StringComparison.OrdinalIgnoreCase))
        {
            var stats = await db.DoctorStatistics.FindAsync(evt.DoctorId);
            if (stats == null)
            {
                // Should exist ideally, but create if missed
                stats = new DoctorStatistics { DoctorId = evt.DoctorId, TotalAppointments = 1, CompletedAppointments = 1 };
                db.DoctorStatistics.Add(stats);
            }
            else
            {
                stats.CompletedAppointments++;
                stats.UpdatedAt = DateTime.UtcNow;
            }
            await db.SaveChangesAsync();
        }
    }

    private async Task HandleAppointmentRated(PractitionerDbContext db, AppointmentRatedEvent evt)
    {
        var stats = await db.DoctorStatistics.FindAsync(evt.DoctorId);
        if (stats == null)
        {
            stats = new DoctorStatistics { DoctorId = evt.DoctorId, TotalAppointments = 1, CompletedAppointments = 1, TotalRatingCount = 1, TotalRatingSum = evt.Rating };
            db.DoctorStatistics.Add(stats);
        }
        else
        {
            stats.TotalRatingCount++;
            stats.TotalRatingSum += evt.Rating;
            stats.UpdatedAt = DateTime.UtcNow;
        }
        await db.SaveChangesAsync();
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        base.Dispose();
    }
}

public class AppointmentCreatedEvent
{
    public Guid DoctorId { get; set; }
}

public class AppointmentUpdatedEvent
{
    public Guid DoctorId { get; set; }
    public string? Status { get; set; }
}

public class AppointmentRatedEvent
{
    public Guid DoctorId { get; set; }
    public int Rating { get; set; }
}
