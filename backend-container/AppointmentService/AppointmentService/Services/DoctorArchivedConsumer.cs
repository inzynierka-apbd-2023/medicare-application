using AppointmentService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace AppointmentService.Services;

public class DoctorArchivedConsumer : BackgroundService
{
    private readonly ILogger<DoctorArchivedConsumer> _logger;
    private readonly IServiceProvider _sp;
    private readonly IConnection _conn;
    private IChannel? _ch;

    public DoctorArchivedConsumer(ILogger<DoctorArchivedConsumer> logger, IServiceProvider sp, IConnection conn)
    {
        _logger = logger;
        _sp = sp;
        _conn = conn;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Aspire connection used
                _ch = await _conn.CreateChannelAsync(cancellationToken: stoppingToken);
                await _ch.ExchangeDeclareAsync("practitioner.events", ExchangeType.Topic, durable: true, cancellationToken: stoppingToken);
                var q = await _ch.QueueDeclareAsync("appointment.purge.on-doctor-archived", durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: stoppingToken);
                await _ch.QueueBindAsync(q.QueueName, "practitioner.events", "doctor.archived", arguments: null, cancellationToken: stoppingToken);

                var consumer = new AsyncEventingBasicConsumer(_ch);
                consumer.ReceivedAsync += async (_, ea) =>
                {
                    try
                    {
                        var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                        var doc = JsonDocument.Parse(json);
                        if (!doc.RootElement.TryGetProperty("DoctorId", out var idEl)) 
                        { 
                            await _ch!.BasicAckAsync(ea.DeliveryTag, false, stoppingToken); 
                            return; 
                        }
                        var archivedDoctorEntityId = idEl.GetGuid();
                        Guid? doctorUserId = null;
                        if (doc.RootElement.TryGetProperty("DoctorUserId", out var userIdEl) && userIdEl.ValueKind == JsonValueKind.String)
                        {
                            var userIdStr = userIdEl.GetString();
                            if (Guid.TryParse(userIdStr, out var g)) doctorUserId = g;
                        }
                        using var scope = _sp.CreateScope();
                        var db = scope.ServiceProvider.GetRequiredService<AppointmentDbContext>();
                        var appts = await db.Appointments.Where(a => a.DoctorId == archivedDoctorEntityId || (doctorUserId.HasValue && a.DoctorId == doctorUserId.Value)).ToListAsync(stoppingToken);
                        if (appts.Any())
                        {
                            db.Appointments.RemoveRange(appts);
                            await db.SaveChangesAsync(stoppingToken);
                            _logger.LogInformation("Purged {Count} appointments for archived doctor entity {DoctorEntityId}", appts.Count, archivedDoctorEntityId);
                        }
                        await _ch!.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to purge appointments on doctor archived");
                        await _ch!.BasicNackAsync(ea.DeliveryTag, false, true, stoppingToken);
                    }
                };
                await _ch.BasicConsumeAsync(q.QueueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);

                // Keep running until cancelled
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DoctorArchivedConsumer connection failure; retrying in 5s");
                await Task.Delay(5000, stoppingToken);
            }
        }
    }

    public override void Dispose()
    {
        // _ch?.Dispose();
        // Do not dispose injected _conn!
        base.Dispose();
    }
}
