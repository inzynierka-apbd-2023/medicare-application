using AppointmentService.Data;
using MassTransit;
using Medicare.Messaging.Contracts;
using Microsoft.EntityFrameworkCore;

namespace AppointmentService.Services.Messaging.Consumers;

public class DoctorArchivedConsumer : IConsumer<IDoctorArchived>
{
    private readonly ILogger<DoctorArchivedConsumer> _logger;
    private readonly AppointmentDbContext _db;

    public DoctorArchivedConsumer(ILogger<DoctorArchivedConsumer> logger, AppointmentDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    public async Task Consume(ConsumeContext<IDoctorArchived> context)
    {
        var msg = context.Message;
        var archivedDoctorEntityId = msg.DoctorId;
        var doctorUserId = msg.DoctorUserId;

        var appts = await _db.Appointments.Where(a => a.DoctorId == archivedDoctorEntityId || (doctorUserId.HasValue && a.DoctorId == doctorUserId.Value)).ToListAsync(context.CancellationToken);
        
        if (appts.Any())
        {
            _db.Appointments.RemoveRange(appts);
            await _db.SaveChangesAsync(context.CancellationToken);
            _logger.LogInformation("Purged {Count} appointments for archived doctor entity {DoctorEntityId}", appts.Count, archivedDoctorEntityId);
        }
    }
}
