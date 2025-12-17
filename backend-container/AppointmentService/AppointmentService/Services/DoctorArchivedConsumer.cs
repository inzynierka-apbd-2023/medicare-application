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
    private IModel? _ch;

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
                _ch = _conn.CreateModel();
                _ch.ExchangeDeclare("practitioner.events", ExchangeType.Topic, durable: true);
                var q = _ch.QueueDeclare("appointment.purge.on-doctor-archived", durable: true, exclusive: false, autoDelete: false);
                _ch.QueueBind(q.QueueName, "practitioner.events", "doctor.archived");

                var consumer = new AsyncEventingBasicConsumer(_ch);
                consumer.Received += async (_, ea) =>
                {
                    try
                    {
                        var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                        var doc = JsonDocument.Parse(json);
                        if (!doc.RootElement.TryGetProperty("DoctorId", out var idEl)) 
                        { 
                            _ch!.BasicAck(ea.DeliveryTag, false); 
                            return; 
                        }
                        var archivedDoctorEntityId = idEl.GetGuid().ToString();
                        string? doctorUserId = null;
                        if (doc.RootElement.TryGetProperty("DoctorUserId", out var userIdEl) && userIdEl.ValueKind == JsonValueKind.String)
                        {
                            doctorUserId = userIdEl.GetString();
                            if (Guid.TryParse(doctorUserId, out var g)) doctorUserId = g.ToString();
                        }
                        using var scope = _sp.CreateScope();
                        var db = scope.ServiceProvider.GetRequiredService<AppointmentDbContext>();
                        var appts = await db.Appointments.Where(a => a.DoctorId == archivedDoctorEntityId || (doctorUserId != null && a.DoctorId == doctorUserId)).ToListAsync(stoppingToken);
                        if (appts.Any())
                        {
                            db.Appointments.RemoveRange(appts);
                            await db.SaveChangesAsync(stoppingToken);
                            _logger.LogInformation("Purged {Count} appointments for archived doctor entity {DoctorEntityId}", appts.Count, archivedDoctorEntityId);
                        }
                        _ch!.BasicAck(ea.DeliveryTag, false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to purge appointments on doctor archived");
                        _ch!.BasicNack(ea.DeliveryTag, false, true);
                    }
                };
                _ch.BasicConsume(q.QueueName, autoAck: false, consumer);

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
        _ch?.Close(); _ch?.Dispose();
        // Do not dispose injected _conn!
        base.Dispose();
    }
}
