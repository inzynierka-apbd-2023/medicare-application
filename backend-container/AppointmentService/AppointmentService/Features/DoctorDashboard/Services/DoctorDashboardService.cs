using Microsoft.EntityFrameworkCore;
using AppointmentService.Data;
using AppointmentService.Features.DoctorDashboard.DTOs;

namespace AppointmentService.Features.DoctorDashboard.Services;

public class DoctorDashboardService : IDoctorDashboardService
{
    private readonly AppointmentDbContext _context;

    public DoctorDashboardService(AppointmentDbContext context)
    {
        _context = context;
    }

    public async Task<DoctorQuickStatsResponse> GetQuickStatsAsync(Guid doctorId, CancellationToken cancellationToken = default)
    {
        var doctorIdStr = doctorId.ToString();
        var today = DateTime.UtcNow.Date;
        var startOfMonth = new DateTime(today.Year, today.Month, 1);

        var patientsToday = await _context.Appointments
            .Where(a => a.DoctorId == doctorIdStr && a.ScheduledAt.Date == today)
            .Select(a => a.PatientId)
            .Distinct()
            .CountAsync(cancellationToken);

        var totalPatients = await _context.Appointments
            .Where(a => a.DoctorId == doctorIdStr)
            .Select(a => a.PatientId)
            .Distinct()
            .CountAsync(cancellationToken);

        var visitsThisMonth = await _context.Appointments
            .Where(a => a.DoctorId == doctorIdStr && 
                       a.ScheduledAt >= startOfMonth && 
                       a.Status == "Completed")
            .CountAsync(cancellationToken);

        var unreadMessages = 0;

        var stats = new List<DoctorQuickStatsDto>
        {
            new() { Label = "Patients Today", Value = patientsToday },
            new() { Label = "Total Patients", Value = totalPatients },
            new() { Label = "Visits this Month", Value = visitsThisMonth },
            new() { Label = "Unread Messages", Value = unreadMessages }
        };

        return new DoctorQuickStatsResponse { Stats = stats };
    }
}
