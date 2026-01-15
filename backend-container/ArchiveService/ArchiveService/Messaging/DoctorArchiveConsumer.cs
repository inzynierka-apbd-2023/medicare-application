using ArchiveService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace ArchiveService.Messaging;

public record DoctorRemovalRequested(Guid DoctorId, Guid? DoctorUserId, string? FullName, string? Email, string? Phone, string? SnapshotJson);

public class DoctorArchiveConsumer : BackgroundService
{
    private readonly ILogger<DoctorArchiveConsumer> _logger;
    private readonly IConfiguration _config;
    private readonly IServiceProvider _sp;
    private readonly IConnection _connection;
    private IChannel? _channel;

    public DoctorArchiveConsumer(ILogger<DoctorArchiveConsumer> logger, IServiceProvider sp, IConfiguration config, IConnection connection)
    {
        _logger = logger;
        _sp = sp;
        _config = config;
        _connection = connection;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Aspire connection is injected; just create channel
        try
        {
            _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);
            await _channel.ExchangeDeclareAsync(exchange: "practitioner.events", type: ExchangeType.Topic, durable: true, cancellationToken: stoppingToken);
            var queue = await _channel.QueueDeclareAsync(queue: "archive.doctor", durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: stoppingToken);
            await _channel.QueueBindAsync(queue: queue.QueueName, exchange: "practitioner.events", routingKey: "doctor.remove.requested", arguments: null, cancellationToken: stoppingToken);
            _logger.LogInformation("DoctorArchiveConsumer connected to RabbitMQ");
        }
        catch (Exception ex) 
        {
            _logger.LogError(ex, "Failed to setup RabbitMQ topology");
            // Retry loop for channel/topology omitted for brevity.
             while (!stoppingToken.IsCancellationRequested && _channel == null)
            {
                try
                {
                     await Task.Delay(5000, stoppingToken);
                     _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);
                     // Re-declare...
                }
                catch { }
            }
        }

        if (_channel == null) return;


        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var evt = JsonSerializer.Deserialize<DoctorRemovalRequested>(json);
                if (evt == null) { await _channel!.BasicAckAsync(ea.DeliveryTag, false, stoppingToken); return; }

                using var scope = _sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ArchiveDbContext>();

                var existing = await db.ArchivedDoctors.FindAsync(evt.DoctorId, stoppingToken);
                if (existing != null)
                {
                    _logger.LogInformation("Doctor {DoctorId} already archived", evt.DoctorId);
                    await _channel!.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
                    return;
                }

                var archived = new ArchiveService.Models.ArchivedDoctor
                {
                    DoctorId = evt.DoctorId,
                    UserId = evt.DoctorUserId,
                    FullName = evt.FullName ?? string.Empty,
                    Email = evt.Email,
                    Phone = evt.Phone,
                    ArchivedAtUtc = DateTime.UtcNow,
                    SnapshotJson = evt.SnapshotJson
                };
                db.ArchivedDoctors.Add(archived);
                await db.SaveChangesAsync(stoppingToken);

                var archivedEvent = new { DoctorId = evt.DoctorId, DoctorUserId = evt.DoctorUserId, Type = "DoctorArchived", At = DateTime.UtcNow };
                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(archivedEvent));
                
                var props = new BasicProperties();
                await _channel.BasicPublishAsync(exchange: "practitioner.events", routingKey: "doctor.archived", mandatory: false, basicProperties: props, body: body, cancellationToken: stoppingToken);

                _logger.LogInformation("Archived doctor {DoctorId}", evt.DoctorId);
                await _channel!.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to archive doctor");
                await _channel!.BasicNackAsync(ea.DeliveryTag, false, true, stoppingToken);
            }
        };

        await _channel.BasicConsumeAsync(queue: "archive.doctor", autoAck: false, consumer: consumer, cancellationToken: stoppingToken);
        
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override void Dispose()
    {
        // _channel?.Dispose(); // Not sync disposable
        base.Dispose();
    }
}
