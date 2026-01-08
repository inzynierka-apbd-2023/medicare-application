using MediatR;
using Microsoft.EntityFrameworkCore;
using AppointmentService.Data;
using AppointmentService.Features.Analytics.DTOs;
using AppointmentService.Features.Analytics.Queries;
using AppointmentService.Services;

namespace AppointmentService.Features.Analytics.Handlers;

public class GetAppointmentAnalyticsHandler : IRequestHandler<GetAppointmentAnalyticsQuery, AppointmentAnalyticsResponse>
{
    private readonly AppointmentDbContext _context;
    private readonly INotificationService _notificationService;

    public GetAppointmentAnalyticsHandler(AppointmentDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<AppointmentAnalyticsResponse> Handle(GetAppointmentAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var endDate = request.EndDate ?? DateTime.UtcNow;
        var startDate = request.StartDate ?? endDate.AddDays(-30);

        // Execute all analytics queries in parallel with error handling
        var metricsTask = SafeExecuteListAsync(() => GetMetricsAsync(startDate, endDate, request.DoctorId, cancellationToken), "Metrics");
        var trendsTask = SafeExecuteListAsync(() => GetTrendsAsync(startDate, endDate, request.DoctorId, cancellationToken), "Trends");
        var doctorPerformanceTask = SafeExecuteListAsync(() => GetDoctorPerformanceAsync(startDate, endDate, request.DoctorId, cancellationToken), "DoctorPerformance");
        var specializationStatsTask = SafeExecuteListAsync(() => GetSpecializationStatsAsync(startDate, endDate, cancellationToken), "SpecializationStats");
        var timeAnalysisTask = SafeExecuteObjectAsync(() => GetTimeSlotAnalysisAsync(startDate, endDate, request.DoctorId, cancellationToken), "TimeAnalysis");

        await Task.WhenAll(metricsTask, trendsTask, doctorPerformanceTask, specializationStatsTask, timeAnalysisTask);

        try
        {
            await _notificationService.CreateNotificationAsync(new CreateNotificationRequest
            {
                RecipientUserId = "system",
                Description = "Appointment analytics dashboard accessed",
                Type = 1,
                SourceService = "AppointmentService",
                Priority = "Low"
            });
        }
        catch (Exception)
        {
            // Ignore notification failures
        }

        return new AppointmentAnalyticsResponse
        {
            Metrics = await metricsTask,
            Trends = await trendsTask,
            DoctorPerformance = await doctorPerformanceTask,
            SpecializationStats = await specializationStatsTask,
            TimeAnalysis = await timeAnalysisTask
        };
    }

    private async Task<IEnumerable<AppointmentMetricDto>> GetMetricsAsync(DateTime startDate, DateTime endDate, Guid? doctorId, CancellationToken cancellationToken)
    {
        var query = _context.Appointments
            .Where(a => a.ScheduledAt >= startDate && a.ScheduledAt <= endDate);

        if (doctorId.HasValue)
            query = query.Where(a => a.DoctorId == doctorId);

        var appointments = await query.ToListAsync(cancellationToken);
        var previousPeriodStart = startDate.AddDays(-(endDate - startDate).Days);
        
        var previousAppointments = await _context.Appointments
            .Where(a => a.ScheduledAt >= previousPeriodStart && a.ScheduledAt < startDate)
            .Where(a => !doctorId.HasValue || a.DoctorId == doctorId)
            .ToListAsync(cancellationToken);

        var totalAppointments = appointments.Count;
        var previousTotal = previousAppointments.Count;
        var totalChange = previousTotal > 0 ? ((double)(totalAppointments - previousTotal) / previousTotal) * 100 : 0;

        var completedAppointments = appointments.Count(a => a.Status == "Completed");
        var previousCompleted = previousAppointments.Count(a => a.Status == "Completed");
        var completedChange = previousCompleted > 0 ? ((double)(completedAppointments - previousCompleted) / previousCompleted) * 100 : 0;

        var activePatients = appointments.Select(a => a.PatientId).Distinct().Count();
        var previousActivePatients = previousAppointments.Select(a => a.PatientId).Distinct().Count();
        var patientsChange = previousActivePatients > 0 ? ((double)(activePatients - previousActivePatients) / previousActivePatients) * 100 : 0;

        var avgDuration = appointments.Any() ? appointments.Average(a => (a.ScheduledEndAt - a.ScheduledAt).TotalMinutes) : 0;
        var previousAvgDuration = previousAppointments.Any() ? previousAppointments.Average(a => (a.ScheduledEndAt - a.ScheduledAt).TotalMinutes) : 0;
        var durationChange = previousAvgDuration > 0 ? ((avgDuration - previousAvgDuration) / previousAvgDuration) * 100 : 0;

        return new List<AppointmentMetricDto>
        {
            new() { Id = Guid.NewGuid(), Title = "Total Appointments", Value = totalAppointments, Change = totalChange, Period = "vs last period", Icon = "calendar" },
            new() { Id = Guid.NewGuid(), Title = "Completed", Value = completedAppointments, Change = completedChange, Period = "vs last period", Icon = "trending" },
            new() { Id = Guid.NewGuid(), Title = "Active Patients", Value = activePatients, Change = patientsChange, Period = "vs last period", Icon = "users" },
            new() { Id = Guid.NewGuid(), Title = "Avg Duration", Value = (int)avgDuration, Change = durationChange, Period = "minutes", Icon = "clock" },
            new() { Id = Guid.NewGuid(), Title = "Total Revenue", Value = 0, Change = 0, Period = "vs last period", Icon = "dollar" },
            new() { Id = Guid.NewGuid(), Title = "Avg Rating", Value = 0, Change = 0, Period = "vs last period", Icon = "star" }
        };
    }

    private async Task<IEnumerable<TrendDataDto>> GetTrendsAsync(DateTime startDate, DateTime endDate, Guid? doctorId, CancellationToken cancellationToken)
    {
        var query = _context.Appointments
            .Where(a => a.ScheduledAt >= startDate && a.ScheduledAt <= endDate);

        if (doctorId.HasValue)
            query = query.Where(a => a.DoctorId == doctorId);

        var appointments = await query.ToListAsync(cancellationToken);
        var trends = new List<TrendDataDto>();
        
        for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
        {
            var nextDate = date.AddDays(1);
            var dayAppointments = appointments.Where(a => a.ScheduledAt >= date && a.ScheduledAt < nextDate).ToList();

            trends.Add(new TrendDataDto
            {
                Date = date.ToString("yyyy-MM-dd"),
                Appointments = dayAppointments.Count,
                Completed = dayAppointments.Count(a => a.Status == "Completed"),
                Cancelled = dayAppointments.Count(a => a.Status == "Cancelled"),
                NoShow = dayAppointments.Count(a => a.Status == "NoShow" || a.Status == "Overdue"),
                Revenue = 0
            });
        }

        return trends;
    }

    private async Task<IEnumerable<DoctorPerformanceDto>> GetDoctorPerformanceAsync(DateTime startDate, DateTime endDate, Guid? doctorId, CancellationToken cancellationToken)
    {
        var query = _context.Appointments
            .Where(a => a.ScheduledAt >= startDate && a.ScheduledAt <= endDate);

        if (doctorId.HasValue)
            query = query.Where(a => a.DoctorId == doctorId);

        var appointments = await query.ToListAsync(cancellationToken);
        var doctorGroups = appointments.GroupBy(a => a.DoctorId);
        var performance = new List<DoctorPerformanceDto>();

        foreach (var group in doctorGroups)
        {
            var docId = group.Key;
            var docAppointments = group.ToList();
            var totalWorkingMinutes = (endDate - startDate).Days * 8 * 60;
            var bookedMinutes = docAppointments.Sum(a => (a.ScheduledEndAt - a.ScheduledAt).TotalMinutes);

            performance.Add(new DoctorPerformanceDto
            {
                Id = docId,
                Name = $"Doctor {docId.ToString()[..8]}",
                Specialization = "General",
                TotalAppointments = docAppointments.Count,
                CompletedAppointments = docAppointments.Count(a => a.Status == "Completed"),
                CancelledAppointments = docAppointments.Count(a => a.Status == "Cancelled"),
                NoShowAppointments = docAppointments.Count(a => a.Status == "NoShow" || a.Status == "Overdue"),
                AverageRating = 0,
                TotalRatings = 0,
                Revenue = 0,
                UtilizationRate = totalWorkingMinutes > 0 ? (bookedMinutes / totalWorkingMinutes) * 100 : 0
            });
        }

        return performance.OrderByDescending(p => p.TotalAppointments);
    }

    private async Task<IEnumerable<SpecializationStatsDto>> GetSpecializationStatsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var appointments = await _context.Appointments
            .Where(a => a.ScheduledAt >= startDate && a.ScheduledAt <= endDate)
            .ToListAsync(cancellationToken);

        // Group by Category or AppointmentType since we don't have specialization data
        var groupedByType = appointments
            .GroupBy(a => a.Category ?? a.AppointmentType ?? "General")
            .Select(g => new SpecializationStatsDto
            {
                Specialization = g.Key,
                TotalAppointments = g.Count(),
                TotalPatients = g.Select(a => a.PatientId).Distinct().Count(),
                TotalDoctors = g.Select(a => a.DoctorId).Distinct().Count(),
                AverageAppointmentDuration = g.Any() ? g.Average(a => (a.ScheduledEndAt - a.ScheduledAt).TotalMinutes) : 0,
                Revenue = 0,
                CompletionRate = g.Count() > 0 ? (double)g.Count(a => a.Status == "Completed") / g.Count() * 100 : 0,
                AverageRating = 0
            })
            .ToList();

        return groupedByType;
    }

    private async Task<TimeSlotAnalysisDto> GetTimeSlotAnalysisAsync(DateTime startDate, DateTime endDate, Guid? doctorId, CancellationToken cancellationToken)
    {
        var query = _context.Appointments
            .Where(a => a.ScheduledAt >= startDate && a.ScheduledAt <= endDate);

        if (doctorId.HasValue)
            query = query.Where(a => a.DoctorId == doctorId);

        var appointments = await query.ToListAsync(cancellationToken);
        var timeSlots = new List<TimeSlotDataDto>();

        for (int hour = 8; hour < 18; hour++)
        {
            var hourAppointments = appointments.Where(a => a.ScheduledAt.Hour == hour).ToList();
            
            timeSlots.Add(new TimeSlotDataDto
            {
                Hour = hour,
                TimeSlot = $"{hour:D2}:00 - {(hour + 1):D2}:00",
                Monday = hourAppointments.Count(a => a.ScheduledAt.DayOfWeek == DayOfWeek.Monday),
                Tuesday = hourAppointments.Count(a => a.ScheduledAt.DayOfWeek == DayOfWeek.Tuesday),
                Wednesday = hourAppointments.Count(a => a.ScheduledAt.DayOfWeek == DayOfWeek.Wednesday),
                Thursday = hourAppointments.Count(a => a.ScheduledAt.DayOfWeek == DayOfWeek.Thursday),
                Friday = hourAppointments.Count(a => a.ScheduledAt.DayOfWeek == DayOfWeek.Friday),
                Saturday = hourAppointments.Count(a => a.ScheduledAt.DayOfWeek == DayOfWeek.Saturday),
                Sunday = hourAppointments.Count(a => a.ScheduledAt.DayOfWeek == DayOfWeek.Sunday),
                TotalAppointments = hourAppointments.Count,
                AverageRevenue = 0,
                CompletionRate = hourAppointments.Count > 0 
                    ? (double)hourAppointments.Count(a => a.Status == "Completed") / hourAppointments.Count * 100 
                    : 0
            });
        }

        var weeklyData = new List<DayDataDto>();
        foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
        {
            var dayAppointments = appointments.Where(a => a.ScheduledAt.DayOfWeek == day).ToList();
            var peakHour = dayAppointments
                .GroupBy(a => a.ScheduledAt.Hour)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault()?.Key ?? 9;

            weeklyData.Add(new DayDataDto
            {
                Day = day.ToString(),
                TotalAppointments = dayAppointments.Count,
                PeakHour = $"{peakHour:D2}:00",
                Revenue = 0,
                UtilizationRate = 0
            });
        }

        return new TimeSlotAnalysisDto
        {
            TimeSlots = timeSlots,
            WeeklyData = weeklyData
        };
    }

    private async Task<IEnumerable<T>> SafeExecuteListAsync<T>(Func<Task<IEnumerable<T>>> action, string componentName)
    {
        try
        {
            return await action();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to retrieve {componentName}: {ex.Message}");
            return Enumerable.Empty<T>();
        }
    }

    private async Task<T> SafeExecuteObjectAsync<T>(Func<Task<T>> action, string componentName) where T : new()
    {
        try
        {
            return await action();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to retrieve {componentName}: {ex.Message}");
            return new T();
        }
    }
}
