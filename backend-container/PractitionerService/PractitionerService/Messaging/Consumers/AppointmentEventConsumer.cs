using MassTransit;
using Medicare.Messaging.Contracts;
using PractitionerService.Data;
using PractitionerService.Models;
using Microsoft.EntityFrameworkCore;

namespace PractitionerService.Messaging.Consumers;

public class AppointmentEventConsumer : 
    IConsumer<IAppointmentCreated>,
    IConsumer<IAppointmentUpdated>,
    IConsumer<IAppointmentRated>
{
    private readonly ILogger<AppointmentEventConsumer> _logger;
    private readonly PractitionerDbContext _db;

    public AppointmentEventConsumer(ILogger<AppointmentEventConsumer> logger, PractitionerDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    public async Task Consume(ConsumeContext<IAppointmentCreated> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Processing AppointmentCreated for Doctor {DoctorId}", msg.DoctorId);

        var stats = await _db.DoctorStatistics.FindAsync(new object[] { msg.DoctorId }, context.CancellationToken);
        if (stats == null)
        {
            stats = new DoctorStatistics { DoctorId = msg.DoctorId, TotalAppointments = 1 };
            _db.DoctorStatistics.Add(stats);
        }
        else
        {
            stats.TotalAppointments++;
            stats.UpdatedAt = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync(context.CancellationToken);
    }

    public async Task Consume(ConsumeContext<IAppointmentUpdated> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Processing AppointmentUpdated for Doctor {DoctorId}", msg.DoctorId);

        if (string.Equals(msg.Status, "Completed", StringComparison.OrdinalIgnoreCase))
        {
            var stats = await _db.DoctorStatistics.FindAsync(new object[] { msg.DoctorId }, context.CancellationToken);
            if (stats == null)
            {
                stats = new DoctorStatistics { DoctorId = msg.DoctorId, TotalAppointments = 1, CompletedAppointments = 1 };
                _db.DoctorStatistics.Add(stats);
            }
            else
            {
                stats.CompletedAppointments++;
                stats.UpdatedAt = DateTime.UtcNow;
            }
            await _db.SaveChangesAsync(context.CancellationToken);
        }
    }

    public async Task Consume(ConsumeContext<IAppointmentRated> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Processing AppointmentRated for Doctor {DoctorId}", msg.DoctorId);

        var rate = new Rate
        {
            Id = Guid.NewGuid(),
            Rate_Value = (byte)msg.Rating,
            Description = msg.Description,
            Patient_User_Id = msg.PatientId,
            Doctor_User_Id = msg.DoctorId,
            Appointment_Id = msg.AppointmentId,
            Rated_At = msg.OccurredAt,
            Is_Anonymous = false
        };

        _db.Rates.Add(rate);

        var stats = await _db.DoctorStatistics.FindAsync(new object[] { msg.DoctorId }, context.CancellationToken);
        if (stats == null)
        {
            stats = new DoctorStatistics { DoctorId = msg.DoctorId, TotalAppointments = 1, CompletedAppointments = 1, TotalRatingCount = 1, TotalRatingSum = msg.Rating };
            _db.DoctorStatistics.Add(stats);
        }
        else
        {
            stats.TotalRatingCount++;
            stats.TotalRatingSum += msg.Rating;
            stats.UpdatedAt = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync(context.CancellationToken);
    }

}
