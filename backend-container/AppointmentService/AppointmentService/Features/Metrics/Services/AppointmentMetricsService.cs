using AppointmentService.Data;
using AppointmentService.Features.Metrics.DTOs;
using Microsoft.EntityFrameworkCore;
using AppointmentService.Models;

namespace AppointmentService.Features.Metrics.Services;

public class AppointmentMetricsService : IAppointmentMetricsService
{
    private readonly AppointmentDbContext _db;
    private readonly MassTransit.IRequestClient<Medicare.Messaging.Contracts.IGetAppointmentPayments> _paymentClient;

    public AppointmentMetricsService(AppointmentDbContext db, MassTransit.IRequestClient<Medicare.Messaging.Contracts.IGetAppointmentPayments> paymentClient)
    {
        _db = db;
        _paymentClient = paymentClient;
    }

    public async Task<AppointmentMetricsResponse> GetMetricsAsync(DateTime start, DateTime end, CancellationToken ct)
    {
        var query = _db.Appointments.AsNoTracking().Where(a => a.ScheduledAt >= start && a.ScheduledAt <= end);
        var total = await query.CountAsync(ct);
        var month = end.Month; var year = end.Year;
        var thisMonth = await query.Where(a => a.ScheduledAt.Month == month && a.ScheduledAt.Year == year).CountAsync(ct);

        var completed = await query.CountAsync(x => x.Status.ToLower() == "completed", ct);
        var cancelled = await query.CountAsync(x => x.Status.ToLower() == "cancelled", ct);
        var noShow = await query.CountAsync(x => x.Status.ToLower() == "noshow" || x.Status.ToLower() == "no-show", ct);

        decimal completionRate = total == 0 ? 0 : (decimal)completed / total * 100m;
        
        var activeDoctors = await query.Select(j => j.DoctorId).Distinct().CountAsync(ct);
        var uniquePatients = await query.Select(j => j.PatientId).Distinct().CountAsync(ct);
        
        var durations = await query.Select(a => EF.Functions.DateDiffMinute(a.ScheduledAt, a.ScheduledEndAt)).ToListAsync(ct);
        var avgDuration = total == 0 ? 0 : durations.Average();

        var apptIds = await query.Select(a => a.Id).ToListAsync(ct);
        var payments = new List<AppointmentService.Features.Analytics.DTOs.AppointmentPaymentDto>();
        
        if (apptIds.Any())
        {
            var response = await _paymentClient.GetResponse<Medicare.Messaging.Contracts.IAppointmentPayments>(new { AppointmentIds = apptIds }, ct);
            payments = response.Message.Payments.Select(p => new AppointmentService.Features.Analytics.DTOs.AppointmentPaymentDto
            {
                AppointmentId = p.AppointmentId,
                AmountCents = (long)p.AmountCents,
                Status = p.Status
            }).ToList();
        }

        var totalRevenue = payments.Sum(p => p.AmountCents) / 100m;
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
