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
    private readonly IConfiguration _config;
    private IConnection? _conn;
    private IModel? _ch;

    public DoctorArchivedConsumer(ILogger<DoctorArchivedConsumer> logger, IServiceProvider sp, IConfiguration config)
    {
        _logger = logger;
        _sp = sp;
        _config = config;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var host = _config["RABBITMQ:HOST"] ?? "rabbitmq";
            var user = _config["RABBITMQ:USERNAME"] ?? "medicare";
            var pass = _config["RABBITMQ:PASSWORD"] ?? "medicare";
            var factory = new ConnectionFactory { HostName = host, UserName = user, Password = pass, DispatchConsumersAsync = true };
            _conn = factory.CreateConnection();
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
                    // cancel or delete appointments of the doctor; support either stored identifier
                    var appts = db.Appointments.Where(a => a.DoctorId == archivedDoctorEntityId || (doctorUserId != null && a.DoctorId == doctorUserId));
                    db.Appointments.RemoveRange(appts);
                    await db.SaveChangesAsync();
                    _logger.LogInformation("Purged appointments for archived doctor entity {DoctorEntityId} (userId={DoctorUserId})", archivedDoctorEntityId, doctorUserId);
                    _ch!.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to purge appointments on doctor archived");
                    _ch!.BasicNack(ea.DeliveryTag, false, true);
                }
            };
            _ch.BasicConsume(q.QueueName, autoAck: false, consumer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DoctorArchivedConsumer startup failure");
        }
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _ch?.Close(); _ch?.Dispose();
        _conn?.Close(); _conn?.Dispose();
        base.Dispose();
    }
}
