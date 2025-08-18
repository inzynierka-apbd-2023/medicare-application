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

        var query = _context.ScheduleAppointments
            .Where(sa => sa.Day >= startDate && sa.Day <= endDate);

        if (!string.IsNullOrEmpty(request.DoctorId))
            query = query.Where(sa => sa.Doctor_User_Id == request.DoctorId);

        var appointments = await query.ToListAsync(cancellationToken);
        
        var previousAppointments = await _context.ScheduleAppointments
            .Where(sa => sa.Day >= previousPeriodStart && sa.Day < startDate)
            .Where(sa => string.IsNullOrEmpty(request.DoctorId) || sa.Doctor_User_Id == request.DoctorId)
            .ToListAsync(cancellationToken);

        var totalAppointments = appointments.Count;
        var previousTotal = previousAppointments.Count;
        var totalChange = previousTotal > 0 ? ((double)(totalAppointments - previousTotal) / previousTotal) * 100 : 0;

        var completedAppointments = await _context.ScheduleAppointments
            .Join(_context.ScheduleAppointmentStatuses, sa => sa.Schedule_Appointment_Status_Id, status => status.Id,
                (sa, status) => new { sa, status })
            .Where(x => x.sa.Day >= startDate && x.sa.Day <= endDate && x.status.Name == "completed")
            .Where(x => string.IsNullOrEmpty(request.DoctorId) || x.sa.Doctor_User_Id == request.DoctorId)
            .CountAsync(cancellationToken);

        var previousCompleted = await _context.ScheduleAppointments
            .Join(_context.ScheduleAppointmentStatuses, sa => sa.Schedule_Appointment_Status_Id, status => status.Id,
                (sa, status) => new { sa, status })
            .Where(x => x.sa.Day >= previousPeriodStart && x.sa.Day < startDate && x.status.Name == "completed")
            .Where(x => string.IsNullOrEmpty(request.DoctorId) || x.sa.Doctor_User_Id == request.DoctorId)
            .CountAsync(cancellationToken);

        var completedChange = previousCompleted > 0 ? ((double)(completedAppointments - previousCompleted) / previousCompleted) * 100 : 0;

        var activePatients = await _context.ScheduleAppointments
            .Where(sa => sa.Day >= startDate && sa.Day <= endDate)
            .Where(sa => string.IsNullOrEmpty(request.DoctorId) || sa.Doctor_User_Id == request.DoctorId)
            .Select(sa => sa.Patient_User_Id)
            .Distinct()
            .CountAsync(cancellationToken);

        var previousActivePatients = await _context.ScheduleAppointments
            .Where(sa => sa.Day >= previousPeriodStart && sa.Day < startDate)
            .Where(sa => string.IsNullOrEmpty(request.DoctorId) || sa.Doctor_User_Id == request.DoctorId)
            .Select(sa => sa.Patient_User_Id)
            .Distinct()
            .CountAsync(cancellationToken);

        var patientsChange = previousActivePatients > 0 ? ((double)(activePatients - previousActivePatients) / previousActivePatients) * 100 : 0;

        var avgDuration = appointments.Any() ? appointments.Average(a => a.Duration_Minutes) : 0;
        var previousAvgDuration = previousAppointments.Any() ? previousAppointments.Average(a => a.Duration_Minutes) : 0;
        var durationChange = previousAvgDuration > 0 ? ((avgDuration - previousAvgDuration) / previousAvgDuration) * 100 : 0;

        var totalRevenue = await _context.AppointmentPayments
            .Join(_context.ScheduleAppointments, ap => ap.Schedule_Appointment_Id, sa => sa.Id,
                (ap, sa) => new { ap, sa })
            .Where(x => x.sa.Day >= startDate && x.sa.Day <= endDate && x.ap.Status == "Paid")
            .Where(x => string.IsNullOrEmpty(request.DoctorId) || x.sa.Doctor_User_Id == request.DoctorId)
            .SumAsync(x => x.ap.Amount, cancellationToken);

        var previousRevenue = await _context.AppointmentPayments
            .Join(_context.ScheduleAppointments, ap => ap.Schedule_Appointment_Id, sa => sa.Id,
                (ap, sa) => new { ap, sa })
            .Where(x => x.sa.Day >= previousPeriodStart && x.sa.Day < startDate && x.ap.Status == "Paid")
            .Where(x => string.IsNullOrEmpty(request.DoctorId) || x.sa.Doctor_User_Id == request.DoctorId)
            .SumAsync(x => x.ap.Amount, cancellationToken);

        var revenueChange = previousRevenue > 0 ? ((double)(totalRevenue - previousRevenue) / (double)previousRevenue) * 100 : 0;

        var avgRating = await _context.Rates
            .Where(r => string.IsNullOrEmpty(request.DoctorId) || r.Doctor_User_Id == request.DoctorId)
            .Where(r => r.Rated_At >= startDate && r.Rated_At <= endDate)
            .AverageAsync(r => (double?)r.Rate_Value, cancellationToken) ?? 0;

        var previousAvgRating = await _context.Rates
            .Where(r => string.IsNullOrEmpty(request.DoctorId) || r.Doctor_User_Id == request.DoctorId)
            .Where(r => r.Rated_At >= previousPeriodStart && r.Rated_At < startDate)
            .AverageAsync(r => (double?)r.Rate_Value, cancellationToken) ?? 0;

        var ratingChange = previousAvgRating > 0 ? ((avgRating - previousAvgRating) / previousAvgRating) * 100 : 0;

        return new List<AppointmentMetricDto>
        {
            new() { Id = "1", Title = "Total Appointments", Value = totalAppointments, Change = totalChange, Period = "vs last period", Icon = "calendar" },
            new() { Id = "2", Title = "Completed", Value = completedAppointments, Change = completedChange, Period = "vs last period", Icon = "trending" },
            new() { Id = "3", Title = "Active Patients", Value = activePatients, Change = patientsChange, Period = "vs last period", Icon = "users" },
            new() { Id = "4", Title = "Avg Duration", Value = (int)avgDuration, Change = durationChange, Period = "minutes", Icon = "clock" },
            new() { Id = "5", Title = "Total Revenue", Value = (int)totalRevenue, Change = revenueChange, Period = "vs last period", Icon = "dollar" },
            new() { Id = "6", Title = "Avg Rating", Value = (int)(avgRating * 10), Change = ratingChange, Period = "vs last period", Icon = "star" }
        };
    }
}

public class GetAppointmentTrendsHandler : IRequestHandler<GetAppointmentTrendsQuery, IEnumerable<TrendDataDto>>
{
    private readonly AppointmentDbContext _context;

    public GetAppointmentTrendsHandler(AppointmentDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TrendDataDto>> Handle(GetAppointmentTrendsQuery request, CancellationToken cancellationToken)
    {
        var endDate = request.EndDate ?? DateTime.UtcNow;
        var startDate = request.StartDate ?? endDate.AddDays(-request.Days);
        
        var trends = new List<TrendDataDto>();
        
        for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
        {
            var nextDate = date.AddDays(1);
            
            var dayAppointments = await _context.ScheduleAppointments
                .Where(sa => sa.Day >= date && sa.Day < nextDate)
                .Where(sa => string.IsNullOrEmpty(request.DoctorId) || sa.Doctor_User_Id == request.DoctorId)
                .ToListAsync(cancellationToken);

            var completed = await _context.ScheduleAppointments
                .Join(_context.ScheduleAppointmentStatuses, sa => sa.Schedule_Appointment_Status_Id, status => status.Id,
                    (sa, status) => new { sa, status })
                .Where(x => x.sa.Day >= date && x.sa.Day < nextDate && x.status.Name == "completed")
                .Where(x => string.IsNullOrEmpty(request.DoctorId) || x.sa.Doctor_User_Id == request.DoctorId)
                .CountAsync(cancellationToken);

            var cancelled = await _context.ScheduleAppointments
                .Join(_context.ScheduleAppointmentStatuses, sa => sa.Schedule_Appointment_Status_Id, status => status.Id,
                    (sa, status) => new { sa, status })
                .Where(x => x.sa.Day >= date && x.sa.Day < nextDate && x.status.Name == "cancelled")
                .Where(x => string.IsNullOrEmpty(request.DoctorId) || x.sa.Doctor_User_Id == request.DoctorId)
                .CountAsync(cancellationToken);

            var noShow = await _context.ScheduleAppointments
                .Join(_context.ScheduleAppointmentStatuses, sa => sa.Schedule_Appointment_Status_Id, status => status.Id,
                    (sa, status) => new { sa, status })
                .Where(x => x.sa.Day >= date && x.sa.Day < nextDate && x.status.Name == "no-show")
                .Where(x => string.IsNullOrEmpty(request.DoctorId) || x.sa.Doctor_User_Id == request.DoctorId)
                .CountAsync(cancellationToken);

            var revenue = await _context.AppointmentPayments
                .Join(_context.ScheduleAppointments, ap => ap.Schedule_Appointment_Id, sa => sa.Id,
                    (ap, sa) => new { ap, sa })
                .Where(x => x.sa.Day >= date && x.sa.Day < nextDate && x.ap.Status == "Paid")
                .Where(x => string.IsNullOrEmpty(request.DoctorId) || x.sa.Doctor_User_Id == request.DoctorId)
                .SumAsync(x => x.ap.Amount, cancellationToken);

            trends.Add(new TrendDataDto
            {
                Date = date.ToString("yyyy-MM-dd"),
                Appointments = dayAppointments.Count,
                Completed = completed,
                Cancelled = cancelled,
                NoShow = noShow,
                Revenue = revenue
            });
        }

        return trends;
    }
}
