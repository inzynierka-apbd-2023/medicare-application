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
    private IModel? _channel;

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
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(exchange: "practitioner.events", type: ExchangeType.Topic, durable: true);
            var queue = _channel.QueueDeclare(queue: "archive.doctor", durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind(queue: queue.QueueName, exchange: "practitioner.events", routingKey: "doctor.remove.requested");
            _logger.LogInformation("DoctorArchiveConsumer connected to RabbitMQ");
        }
        catch (Exception ex) 
        {
            _logger.LogError(ex, "Failed to setup RabbitMQ topology");
            // Retry loop for channel/topology if needed (omitted for brevity, relying on Aspire connection resilience mostly, but simple retry is good)
             while (!stoppingToken.IsCancellationRequested && _channel == null)
            {
                try
                {
                     await Task.Delay(5000, stoppingToken);
                     _channel = _connection.CreateModel();
                     // Re-declare...
                }
                catch { }
            }
        }

        if (_channel == null) return;


        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (_, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var evt = JsonSerializer.Deserialize<DoctorRemovalRequested>(json);
                if (evt == null) { _channel!.BasicAck(ea.DeliveryTag, false); return; }

                using var scope = _sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ArchiveDbContext>();

                var existing = await db.ArchivedDoctors.FindAsync(evt.DoctorId);
                if (existing != null)
                {
                    _logger.LogInformation("Doctor {DoctorId} already archived", evt.DoctorId);
                    _channel!.BasicAck(ea.DeliveryTag, false);
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
                await db.SaveChangesAsync();

                var archivedEvent = new { DoctorId = evt.DoctorId, DoctorUserId = evt.DoctorUserId, Type = "DoctorArchived", At = DateTime.UtcNow };
                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(archivedEvent));
                _channel.BasicPublish(exchange: "practitioner.events", routingKey: "doctor.archived", basicProperties: null, body: body);

                _logger.LogInformation("Archived doctor {DoctorId}", evt.DoctorId);
                _channel!.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to archive doctor");
                _channel!.BasicNack(ea.DeliveryTag, false, true);
            }
        };

        _channel.BasicConsume(queue: "archive.doctor", autoAck: false, consumer: consumer);
        
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override void Dispose()
    {
        _channel?.Close(); _channel?.Dispose();
        base.Dispose();
    }
}
