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
        var query = _db.ScheduleAppointments.AsNoTracking().Where(a => a.Day >= start && a.Day <= end);
        var total = await query.CountAsync(ct);
        var month = end.Month; var year = end.Year;
        var thisMonth = await query.Where(a => a.Day.Month == month && a.Day.Year == year).CountAsync(ct);

        var statuses = _db.ScheduleAppointmentStatuses.AsNoTracking();
        var joined = from a in query
                     join s in statuses on a.Schedule_Appointment_Status_Id equals s.Id
                     select new { a.Id, a.Duration_Minutes, a.Doctor_User_Id, a.Patient_User_Id, Status = s.Name, a.Total_Cost };

        var completed = await joined.CountAsync(x => x.Status == "completed", ct);
        var cancelled = await joined.CountAsync(x => x.Status == "cancelled", ct);
        var noShow = await joined.CountAsync(x => x.Status == "no-show" || x.Status == "no_show", ct);

        decimal completionRate = total == 0 ? 0 : (decimal)completed / total * 100m;
        var activeDoctors = await joined.Select(j => j.Doctor_User_Id).Distinct().CountAsync(ct);
        var uniquePatients = await joined.Select(j => j.Patient_User_Id).Distinct().CountAsync(ct);
        var avgDuration = total == 0 ? 0 : await joined.AverageAsync(j => (double)j.Duration_Minutes, ct);
        var totalRevenue = await joined.Where(j => j.Total_Cost != null).SumAsync(j => j.Total_Cost ?? 0m, ct);
        var avgRevenue = total == 0 ? 0 : totalRevenue / total;

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
