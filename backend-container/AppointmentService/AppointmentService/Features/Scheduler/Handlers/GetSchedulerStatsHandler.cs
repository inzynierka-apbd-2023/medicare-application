using MediatR;
using Microsoft.EntityFrameworkCore;
using AppointmentService.Data;
using AppointmentService.Features.Scheduler.DTOs;
using AppointmentService.Features.Scheduler.Queries;

namespace AppointmentService.Features.Scheduler.Handlers;

public class GetSchedulerStatsHandler : IRequestHandler<GetSchedulerStatsQuery, SchedulerStatsResponse>
{
    private readonly AppointmentDbContext _context;

    public GetSchedulerStatsHandler(AppointmentDbContext context)
    {
        _context = context;
    }

    public async Task<SchedulerStatsResponse> Handle(GetSchedulerStatsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Appointments.AsQueryable();

        if (request.DoctorId.HasValue)
        {
            query = query.Where(a => a.DoctorId == request.DoctorId.Value);
        }

        if (request.PatientId.HasValue)
        {
            query = query.Where(a => a.PatientId == request.PatientId.Value);
        }
        
        // Cache base query to avoid repeated DB calls if possible, or execute separate counts efficiently
        // Separate counts are clearer to read and usually fine for this scale.

        var total = await query.CountAsync(cancellationToken);
        
        var today = DateTime.UtcNow.Date;
        var todaysCount = await query
            .Where(a => a.ScheduledAt.Date == today)
            .CountAsync(cancellationToken);

        var confirmed = await query
            .Where(a => a.Status == "Confirmed")
            .CountAsync(cancellationToken);
            
        var cancelled = await query
            .Where(a => a.Status == "Cancelled")
            .CountAsync(cancellationToken);

        return new SchedulerStatsResponse
        {
            TotalAppointments = total,
            TodaysAppointments = todaysCount,
            ConfirmedAppointments = confirmed,
            CancelledAppointments = cancelled
        };
    }
}
