using MediatR;
using Microsoft.EntityFrameworkCore;
using AppointmentService.Data;
using AppointmentService.Features.Analytics.DTOs;
using AppointmentService.Features.Analytics.Queries;

namespace AppointmentService.Features.Analytics.Handlers;

public class GetAppointmentMetricsHandler : IRequestHandler<GetAppointmentMetricsQuery, IEnumerable<AppointmentMetricDto>>
{
    private readonly AppointmentDbContext _context;

    public GetAppointmentMetricsHandler(AppointmentDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AppointmentMetricDto>> Handle(GetAppointmentMetricsQuery request, CancellationToken cancellationToken)
    {
        var endDate = request.EndDate ?? DateTime.UtcNow;
        var startDate = request.StartDate ?? endDate.AddDays(-30);
        var previousPeriodStart = startDate.AddDays(-(endDate - startDate).Days);

        // Use the actual Appointments table instead of ScheduleAppointments
        var query = _context.Appointments
            .Where(a => a.ScheduledAt >= startDate && a.ScheduledAt <= endDate);

        if (request.DoctorId.HasValue)
            query = query.Where(a => a.DoctorId == request.DoctorId);

        var appointments = await query.ToListAsync(cancellationToken);
        
        var previousAppointments = await _context.Appointments
            .Where(a => a.ScheduledAt >= previousPeriodStart && a.ScheduledAt < startDate)
            .Where(a => !request.DoctorId.HasValue || a.DoctorId == request.DoctorId)
            .ToListAsync(cancellationToken);

        var totalAppointments = appointments.Count;
        var previousTotal = previousAppointments.Count;
        var totalChange = previousTotal > 0 ? ((double)(totalAppointments - previousTotal) / previousTotal) * 100 : 0;

        // Use Status field directly instead of joining with ScheduleAppointmentStatuses
        var completedAppointments = appointments.Count(a => a.Status == "Completed");
        var previousCompleted = previousAppointments.Count(a => a.Status == "Completed");
        var completedChange = previousCompleted > 0 ? ((double)(completedAppointments - previousCompleted) / previousCompleted) * 100 : 0;

        var activePatients = appointments.Select(a => a.PatientId).Distinct().Count();
        var previousActivePatients = previousAppointments.Select(a => a.PatientId).Distinct().Count();
        var patientsChange = previousActivePatients > 0 ? ((double)(activePatients - previousActivePatients) / previousActivePatients) * 100 : 0;

        // Calculate average duration from ScheduledAt and ScheduledEndAt
        var avgDuration = appointments.Any() 
            ? appointments.Average(a => (a.ScheduledEndAt - a.ScheduledAt).TotalMinutes) 
            : 0;
        var previousAvgDuration = previousAppointments.Any()
            ? previousAppointments.Average(a => (a.ScheduledEndAt - a.ScheduledAt).TotalMinutes)
            : 0;
        var durationChange = previousAvgDuration > 0 ? ((avgDuration - previousAvgDuration) / previousAvgDuration) * 100 : 0;

        // Revenue and ratings are not available in the Appointments table
        // Return placeholder values for now
        var totalRevenue = 0m;
        var revenueChange = 0.0;
        var avgRating = 0.0;
        var ratingChange = 0.0;

        return new List<AppointmentMetricDto>
        {
            new() { Id = Guid.NewGuid(), Title = "Total Appointments", Value = totalAppointments, Change = totalChange, Period = "vs last period", Icon = "calendar" },
            new() { Id = Guid.NewGuid(), Title = "Completed", Value = completedAppointments, Change = completedChange, Period = "vs last period", Icon = "trending" },
            new() { Id = Guid.NewGuid(), Title = "Active Patients", Value = activePatients, Change = patientsChange, Period = "vs last period", Icon = "users" },
            new() { Id = Guid.NewGuid(), Title = "Avg Duration", Value = (int)avgDuration, Change = durationChange, Period = "minutes", Icon = "clock" },
            new() { Id = Guid.NewGuid(), Title = "Total Revenue", Value = (int)totalRevenue, Change = revenueChange, Period = "vs last period", Icon = "dollar" },
            new() { Id = Guid.NewGuid(), Title = "Avg Rating", Value = (int)(avgRating * 10), Change = ratingChange, Period = "vs last period", Icon = "star" }
        };
    }
}
