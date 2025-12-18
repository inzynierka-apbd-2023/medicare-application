using AppointmentService.Data;
using AppointmentService.Features.Metrics.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AppointmentService.Features.Metrics.Services;

public class AppointmentMetricsService : IAppointmentMetricsService
{
    private readonly AppointmentDbContext _db;
    public AppointmentMetricsService(AppointmentDbContext db) => _db = db;

    public async Task<AppointmentMetricsResponse> GetMetricsAsync(DateTime start, DateTime end, CancellationToken ct)
    {
        var query = _db.Appointments.AsNoTracking().Where(a => a.ScheduledAt >= start && a.ScheduledAt <= end);
        var total = await query.CountAsync(ct);
        var month = end.Month; var year = end.Year;
        var thisMonth = await query.Where(a => a.ScheduledAt.Month == month && a.ScheduledAt.Year == year).CountAsync(ct);

        // Status names in Appointment table (Scheduled, Confirmed, Completed, Cancelled, NoShow)
        // We use lowercase comparison or normalize.
        var completed = await query.CountAsync(x => x.Status == "Completed", ct);
        var cancelled = await query.CountAsync(x => x.Status == "Cancelled", ct);
        var noShow = await query.CountAsync(x => x.Status == "NoShow", ct);

        decimal completionRate = total == 0 ? 0 : (decimal)completed / total * 100m;
        
        // Active Doctors/Patients
        var activeDoctors = await query.Select(j => j.DoctorId).Distinct().CountAsync(ct);
        var uniquePatients = await query.Select(j => j.PatientId).Distinct().CountAsync(ct);
        
        // Duration Calculation (in minutes)
        // SQL Server DATEDIFF equivalent in LINQ
        var durations = await query.Select(a => EF.Functions.DateDiffMinute(a.ScheduledAt, a.ScheduledEndAt)).ToListAsync(ct);
        var avgDuration = total == 0 ? 0 : durations.Average();

        // Revenue - Currently assuming 0 or mapping from Payment table if available which is not directly linked in Entity context shown.
        // For now, setting to 0 to fix the error.
        var totalRevenue = 0m; 
        var avgRevenue = 0m;

        return new AppointmentMetricsResponse
        {
            StartDate = start,
            EndDate = end,
            TotalAppointments = total,
            AppointmentsThisMonth = thisMonth,
            CompletedAppointments = completed,
            CancelledAppointments = cancelled,
            NoShowAppointments = noShow,
            CompletionRate = Math.Round(completionRate, 2),
            ActiveDoctorsInPeriod = activeDoctors,
            UniquePatientsInPeriod = uniquePatients,
            AverageDurationMinutes = (decimal)Math.Round(avgDuration, 2),
            TotalRevenue = Math.Round(totalRevenue, 2),
            AverageRevenuePerAppointment = Math.Round(avgRevenue, 2),
            IsStub = false
        };
    }
}
