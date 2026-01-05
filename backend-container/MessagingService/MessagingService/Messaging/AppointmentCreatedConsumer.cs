using MessagingService.Data;
using MessagingService.Models;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace MessagingService.Messaging;

/// <summary>
/// Event raised by AppointmentService when an appointment is created.
/// Contains data needed to establish patient-doctor messaging relationship.
/// </summary>
public record AppointmentCreatedEvent(
    Guid AppointmentId,
    Guid PatientId,
    Guid DoctorId,
    DateTime ScheduledAt,
    DateTime OccurredAt,
    // Optional enriched data (may be null if not provided by publisher)
    string? DoctorName = null,
    string? DoctorSpecialization = null
);

/// <summary>
/// Consumes appointment.created events from RabbitMQ and maintains the
/// PatientDoctorContact table for messaging recipient lookup.
/// </summary>
public class AppointmentCreatedConsumer : BackgroundService
{
    private readonly ILogger<AppointmentCreatedConsumer> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConnection _connection;
    private IChannel? _channel;

    private const string ExchangeName = "appointment.events";
    private const string QueueName = "messaging.appointment.created";
    private const string RoutingKey = "appointment.created";

    public AppointmentCreatedConsumer(
        ILogger<AppointmentCreatedConsumer> logger,
        IServiceProvider serviceProvider,
        IConnection connection)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _connection = connection;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Setup RabbitMQ channel and topology
        try
        {
            _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);
            
            await _channel.ExchangeDeclareAsync(
                exchange: ExchangeName,
                type: ExchangeType.Topic,
                durable: true,
                cancellationToken: stoppingToken);
            
            var queueResult = await _channel.QueueDeclareAsync(
                queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: stoppingToken);
            
            await _channel.QueueBindAsync(
                queue: queueResult.QueueName,
                exchange: ExchangeName,
                routingKey: RoutingKey,
                arguments: null,
                cancellationToken: stoppingToken);
            
            _logger.LogInformation("AppointmentCreatedConsumer connected to RabbitMQ, queue: {Queue}", QueueName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to setup RabbitMQ topology for AppointmentCreatedConsumer");
            
            // Retry loop
            while (!stoppingToken.IsCancellationRequested && _channel == null)
            {
                try
                {
                    await Task.Delay(5000, stoppingToken);
                    _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);
                    await _channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Topic, durable: true, cancellationToken: stoppingToken);
                    var q = await _channel.QueueDeclareAsync(QueueName, durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);
                    await _channel.QueueBindAsync(q.QueueName, ExchangeName, RoutingKey, cancellationToken: stoppingToken);
                    _logger.LogInformation("AppointmentCreatedConsumer reconnected to RabbitMQ");
                }
                catch (Exception retryEx)
                {
                    _logger.LogWarning(retryEx, "Failed to reconnect to RabbitMQ, will retry...");
                }
            }
        }

        if (_channel == null)
        {
            _logger.LogError("Could not establish RabbitMQ channel, consumer will not start");
            return;
        }

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, eventArgs) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
                _logger.LogDebug("Received appointment.created event: {Json}", json);
                
                var evt = JsonSerializer.Deserialize<AppointmentCreatedEvent>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                if (evt == null)
                {
                    _logger.LogWarning("Failed to deserialize appointment.created event");
                    await _channel.BasicAckAsync(eventArgs.DeliveryTag, false, stoppingToken);
                    return;
                }

                await ProcessEventAsync(evt, stoppingToken);
                await _channel.BasicAckAsync(eventArgs.DeliveryTag, false, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing appointment.created event");
                // Nack and requeue for retry
                await _channel.BasicNackAsync(eventArgs.DeliveryTag, false, true, stoppingToken);
            }
        };

        await _channel.BasicConsumeAsync(
            queue: QueueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        _logger.LogInformation("AppointmentCreatedConsumer started consuming messages");
        
        // Keep running until cancelled
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task ProcessEventAsync(AppointmentCreatedEvent evt, CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MessagingDbContext>();

        // Check if relationship already exists
        var existing = await db.PatientDoctorContacts
            .FirstOrDefaultAsync(c => c.PatientUserId == evt.PatientId && c.DoctorUserId == evt.DoctorId, ct);

        var now = DateTime.UtcNow;

        // Fetch doctor name from shared database (User_Profile table) if not in event
        string? doctorName = evt.DoctorName;
        string? doctorSpecialization = evt.DoctorSpecialization;
        
        if (string.IsNullOrEmpty(doctorName))
        {
            try
            {
                // Query the shared user.User_Profile table directly
                // Since all services share the same DB, we can access it
                var userProfile = await db.Database.SqlQueryRaw<UserProfileDto>(
                    "SELECT FirstName, LastName FROM [user].[User_Profile] WHERE Id = {0}", 
                    evt.DoctorId)
                    .FirstOrDefaultAsync(ct);
                
                if (userProfile != null)
                {
                    doctorName = $"Dr. {userProfile.FirstName} {userProfile.LastName}".Trim();
                    _logger.LogInformation("Fetched doctor name from User_Profile: {DoctorName}", doctorName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not fetch doctor name from User_Profile for {DoctorId}", evt.DoctorId);
            }
        }

        if (existing != null)
        {
            // Update last contact time
            existing.LastContactAt = now;
            existing.UpdatedAt = now;
            
            // Update name/specialization if we have values and current is empty
            if (!string.IsNullOrEmpty(doctorName) && string.IsNullOrEmpty(existing.DoctorName))
            {
                existing.DoctorName = doctorName;
            }
            if (!string.IsNullOrEmpty(doctorSpecialization) && string.IsNullOrEmpty(existing.DoctorSpecialization))
            {
                existing.DoctorSpecialization = doctorSpecialization;
            }
            
            _logger.LogInformation("Updated PatientDoctorContact for Patient {PatientId} -> Doctor {DoctorId} ({Name})", 
                evt.PatientId, evt.DoctorId, existing.DoctorName);
        }
        else
        {
            // Create new relationship
            var contact = new PatientDoctorContact
            {
                Id = Guid.NewGuid(),
                PatientUserId = evt.PatientId,
                DoctorUserId = evt.DoctorId,
                DoctorName = doctorName ?? "Doctor",
                DoctorSpecialization = doctorSpecialization ?? "General",
                FirstContactAt = now,
                LastContactAt = now,
                CreatedAt = now,
                UpdatedAt = now
            };
            
            db.PatientDoctorContacts.Add(contact);
            _logger.LogInformation("Created PatientDoctorContact for Patient {PatientId} -> Doctor {DoctorId} ({Name})", 
                evt.PatientId, evt.DoctorId, contact.DoctorName);
        }

        await db.SaveChangesAsync(ct);
    }

    // DTO for querying user profile
    private record UserProfileDto(string? FirstName, string? LastName);

    public override void Dispose()
    {
        _channel?.Dispose();
        base.Dispose();
    }
}
