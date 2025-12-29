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

        // Execute all analytics queries in parallel
        // Execute all analytics queries in parallel with error handling
        var metricsTask = SafeExecuteListAsync(() => GetMetricsAsync(startDate, endDate, request.DoctorId, cancellationToken), "Metrics");
        var trendsTask = SafeExecuteListAsync(() => GetTrendsAsync(startDate, endDate, request.DoctorId, cancellationToken), "Trends");
        var doctorPerformanceTask = SafeExecuteListAsync(() => GetDoctorPerformanceAsync(startDate, endDate, request.DoctorId, request.Specialization, cancellationToken), "DoctorPerformance");
        var specializationStatsTask = SafeExecuteListAsync(() => GetSpecializationStatsAsync(startDate, endDate, cancellationToken), "SpecializationStats");
        var timeAnalysisTask = SafeExecuteObjectAsync(() => GetTimeSlotAnalysisAsync(startDate, endDate, request.DoctorId, cancellationToken), "TimeAnalysis");

        await Task.WhenAll(metricsTask, trendsTask, doctorPerformanceTask, specializationStatsTask, timeAnalysisTask);

        try
        {
            // Create notification for analytics access
            await _notificationService.CreateNotificationAsync(new CreateNotificationRequest
            {
                RecipientUserId = "system", // This would be the actual user ID in production
                Description = "Appointment analytics dashboard accessed",
                Type = 1, // Info notification
                SourceService = "AppointmentService",
                Priority = "Low"
            });
        }
        catch (Exception)
        {
            // Ignore notification failures to prevent blocking analytics response
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

        var totalAppointments = appointments.Count();
        var previousTotal = previousAppointments.Count();
        var totalChange = previousTotal > 0 ? ((double)(totalAppointments - previousTotal) / previousTotal) * 100 : 0;

        var completedAppointments = await _context.Appointments
            .Where(a => a.ScheduledAt >= startDate && a.ScheduledAt <= endDate && a.Status == "Completed")
            .Where(a => !doctorId.HasValue || a.DoctorId == doctorId)
            .CountAsync(cancellationToken);

        var previousCompleted = await _context.Appointments
            .Where(a => a.ScheduledAt >= previousPeriodStart && a.ScheduledAt < startDate && a.Status == "Completed")
            .Where(a => !doctorId.HasValue || a.DoctorId == doctorId)
            .CountAsync(cancellationToken);

        var completedChange = previousCompleted > 0 ? ((double)(completedAppointments - previousCompleted) / previousCompleted) * 100 : 0;

        var activePatients = await _context.Appointments
            .Where(a => a.ScheduledAt >= startDate && a.ScheduledAt <= endDate)
            .Where(a => !doctorId.HasValue || a.DoctorId == doctorId)
            .Select(a => a.PatientId)
            .Distinct()
            .CountAsync(cancellationToken);

        var previousActivePatients = await _context.Appointments
            .Where(a => a.ScheduledAt >= previousPeriodStart && a.ScheduledAt < startDate)
            .Where(a => !doctorId.HasValue || a.DoctorId == doctorId)
            .Select(a => a.PatientId)
            .Distinct()
            .CountAsync(cancellationToken);

        var patientsChange = previousActivePatients > 0 ? ((double)(activePatients - previousActivePatients) / previousActivePatients) * 100 : 0;

        var avgDuration = appointments.Any() ? appointments.Average(a => (a.ScheduledEndAt - a.ScheduledAt).TotalMinutes) : 0;
        var previousAvgDuration = previousAppointments.Any() ? previousAppointments.Average(a => (a.ScheduledEndAt - a.ScheduledAt).TotalMinutes) : 0;
        var durationChange = previousAvgDuration > 0 ? ((avgDuration - previousAvgDuration) / previousAvgDuration) * 100 : 0;

        var totalRevenue = await _context.AppointmentPayments
            .Join(_context.Appointments, ap => ap.Schedule_Appointment_Id, a => a.Id,
                (ap, a) => new { ap, a })
            .Where(x => x.a.ScheduledAt >= startDate && x.a.ScheduledAt <= endDate && x.ap.Status == "Paid")
            .Where(x => !doctorId.HasValue || x.a.DoctorId == doctorId)
            .SumAsync(x => x.ap.Amount ?? 0m, cancellationToken);

        var previousRevenue = await _context.AppointmentPayments
            .Join(_context.Appointments, ap => ap.Schedule_Appointment_Id, a => a.Id,
                (ap, a) => new { ap, a })
            .Where(x => x.a.ScheduledAt >= previousPeriodStart && x.a.ScheduledAt < startDate && x.ap.Status == "Paid")
            .Where(x => !doctorId.HasValue || x.a.DoctorId == doctorId)
            .SumAsync(x => x.ap.Amount ?? 0m, cancellationToken);

        var revenueChange = previousRevenue > 0 ? ((double)(totalRevenue - previousRevenue) / (double)previousRevenue) * 100 : 0;

        var avgRating = await _context.Rates
            .Where(r => !doctorId.HasValue || r.Doctor_User_Id == doctorId)
            .Where(r => r.Rated_At >= startDate && r.Rated_At <= endDate)
            .AverageAsync(r => (double?)r.Rate_Value, cancellationToken) ?? 0;

        var previousAvgRating = await _context.Rates
            .Where(r => !doctorId.HasValue || r.Doctor_User_Id == doctorId)
            .Where(r => r.Rated_At >= previousPeriodStart && r.Rated_At < startDate)
            .AverageAsync(r => (double?)r.Rate_Value, cancellationToken) ?? 0;

        var ratingChange = previousAvgRating > 0 ? ((avgRating - previousAvgRating) / previousAvgRating) * 100 : 0;

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

    private async Task<IEnumerable<TrendDataDto>> GetTrendsAsync(DateTime startDate, DateTime endDate, Guid? doctorId, CancellationToken cancellationToken)
    {
        var trends = new List<TrendDataDto>();
        
        for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
        {
            var nextDate = date.AddDays(1);
            
            var dayAppointments = await _context.Appointments
                .Where(a => a.ScheduledAt >= date && a.ScheduledAt < nextDate)
                .Where(a => !doctorId.HasValue || a.DoctorId == doctorId)
                .ToListAsync(cancellationToken);

            var completed = await _context.Appointments
                .Where(a => a.ScheduledAt >= date && a.ScheduledAt < nextDate && a.Status == "Completed")
                .Where(a => !doctorId.HasValue || a.DoctorId == doctorId)
                .CountAsync(cancellationToken);

            var cancelled = await _context.Appointments
                .Where(a => a.ScheduledAt >= date && a.ScheduledAt < nextDate && a.Status == "Cancelled")
                .Where(a => !doctorId.HasValue || a.DoctorId == doctorId)
                .CountAsync(cancellationToken);

            var noShow = await _context.Appointments
                .Where(a => a.ScheduledAt >= date && a.ScheduledAt < nextDate && a.Status == "NoShow")
                .Where(a => !doctorId.HasValue || a.DoctorId == doctorId)
                .CountAsync(cancellationToken);

            var revenue = await _context.AppointmentPayments
                .Join(_context.Appointments, ap => ap.Schedule_Appointment_Id, a => a.Id,
                    (ap, a) => new { ap, a })
                .Where(x => x.a.ScheduledAt >= date && x.a.ScheduledAt < nextDate && x.ap.Status == "Paid")
                .Where(x => !doctorId.HasValue || x.a.DoctorId == doctorId)
                .SumAsync(x => x.ap.Amount ?? 0m, cancellationToken);

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

    private async Task<IEnumerable<DoctorPerformanceDto>> GetDoctorPerformanceAsync(DateTime startDate, DateTime endDate, Guid? doctorId, string? specialization, CancellationToken cancellationToken)
    {
        var query = from d in _context.Doctors
                    join u in _context.Users on d.Id equals u.Id
                    join up in _context.UserProfiles on u.Id equals up.User_Id
                    join ds in _context.DoctorSpecializations on d.Id equals ds.Doctor_Id into docSpecs
                    from ds in docSpecs.DefaultIfEmpty()
                    join s in _context.Specializations on ds.Specialization_Id equals s.Id into specs
                    from s in specs.DefaultIfEmpty()
                    where u.Is_Active
                    select new { Doctor = d, Profile = up, Specialization = s != null ? s.Name : "General" };

        if (doctorId.HasValue)
            query = query.Where(x => x.Doctor.Id == doctorId);

        if (!string.IsNullOrEmpty(specialization))
            query = query.Where(x => x.Specialization == specialization);

        var doctors = await query.ToListAsync(cancellationToken);
        var performance = new List<DoctorPerformanceDto>();

        foreach (var doctor in doctors)
        {
            var appointments = await _context.Appointments
                .Where(a => a.DoctorId == doctor.Doctor.Id && a.ScheduledAt >= startDate && a.ScheduledAt <= endDate)
                .ToListAsync(cancellationToken);

            var completed = await _context.Appointments
                .Where(a => a.DoctorId == doctor.Doctor.Id && a.ScheduledAt >= startDate && a.ScheduledAt <= endDate && a.Status == "Completed")
                .CountAsync(cancellationToken);

            var cancelled = await _context.Appointments
                .Where(a => a.DoctorId == doctor.Doctor.Id && a.ScheduledAt >= startDate && a.ScheduledAt <= endDate && a.Status == "Cancelled")
                .CountAsync(cancellationToken);

            var noShow = await _context.Appointments
                .Where(a => a.DoctorId == doctor.Doctor.Id && a.ScheduledAt >= startDate && a.ScheduledAt <= endDate && a.Status == "NoShow")
                .CountAsync(cancellationToken);

            var ratings = await _context.Rates
                .Where(r => r.Doctor_User_Id == doctor.Doctor.Id && r.Rated_At >= startDate && r.Rated_At <= endDate)
                .ToListAsync(cancellationToken);

            var revenue = await _context.AppointmentPayments
                .Join(_context.Appointments, ap => ap.Schedule_Appointment_Id, a => a.Id,
                    (ap, a) => new { ap, a })
                .Where(x => x.a.DoctorId == doctor.Doctor.Id && x.a.ScheduledAt >= startDate && x.a.ScheduledAt <= endDate && x.ap.Status == "Paid")
                .SumAsync(x => x.ap.Amount ?? 0m, cancellationToken);

            performance.Add(new DoctorPerformanceDto
            {
                Id = doctor.Doctor.Id,
                Name = $"Dr. {doctor.Profile.FirstName ?? "Unknown"} {doctor.Profile.LastName ?? ""}",
                Specialization = doctor.Specialization,
                TotalAppointments = appointments.Count,
                CompletedAppointments = completed,
                CancelledAppointments = cancelled,
                NoShowAppointments = noShow,
                AverageRating = ratings.Any() ? (ratings.Average(r => (double?)r.Rate_Value) ?? 0) : 0,
                TotalRatings = ratings.Count,
                Revenue = revenue,
                UtilizationRate = appointments.Count > 0 ? (double)completed / appointments.Count * 100 : 0
            });
        }

        return performance;
    }

    private async Task<IEnumerable<SpecializationStatsDto>> GetSpecializationStatsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var flatData = await (from s in _context.Specializations
                                        join ds in _context.DoctorSpecializations on s.Id equals ds.Specialization_Id
                                        join d in _context.Doctors on ds.Doctor_Id equals d.Id
                                        where s.Is_Active
                                        select new
                                        {
                                            SpecializationName = s.Name,
                                            DoctorId = d.Id
                                        }).ToListAsync(cancellationToken);

        var specializationData = flatData
            .GroupBy(x => x.SpecializationName)
            .Select(g => new
            {
                SpecializationName = g.Key,
                DoctorIds = g.Select(x => x.DoctorId).Distinct().ToList()
            })
            .ToList();

        var stats = new List<SpecializationStatsDto>();

        foreach (var spec in specializationData)
        {
            var appointments = await _context.Appointments
                .Where(a => spec.DoctorIds.Contains(a.DoctorId) && a.ScheduledAt >= startDate && a.ScheduledAt <= endDate)
                .ToListAsync(cancellationToken);

            var completed = await _context.Appointments
                .Where(a => spec.DoctorIds.Contains(a.DoctorId) && a.ScheduledAt >= startDate && a.ScheduledAt <= endDate && a.Status == "Completed")
                .CountAsync(cancellationToken);

            var patients = await _context.Appointments
                .Where(a => spec.DoctorIds.Contains(a.DoctorId) && a.ScheduledAt >= startDate && a.ScheduledAt <= endDate)
                .Select(a => a.PatientId)
                .Distinct()
                .CountAsync(cancellationToken);

            var revenue = await _context.AppointmentPayments
                .Join(_context.Appointments, ap => ap.Schedule_Appointment_Id, a => a.Id,
                    (ap, a) => new { ap, a })
                .Where(x => spec.DoctorIds.Contains(x.a.DoctorId) && x.a.ScheduledAt >= startDate && x.a.ScheduledAt <= endDate && x.ap.Status == "Paid")
                .SumAsync(x => x.ap.Amount ?? 0m, cancellationToken);

            var avgRating = await _context.Rates
                .Where(r => spec.DoctorIds.Contains(r.Doctor_User_Id) && r.Rated_At >= startDate && r.Rated_At <= endDate)
                .AverageAsync(r => (double?)r.Rate_Value, cancellationToken) ?? 0;

            stats.Add(new SpecializationStatsDto
            {
                Specialization = spec.SpecializationName,
                TotalAppointments = appointments.Count,
                TotalPatients = patients,
                TotalDoctors = spec.DoctorIds.Count,
                AverageAppointmentDuration = appointments.Any() ? appointments.Average(a => (a.ScheduledEndAt - a.ScheduledAt).TotalMinutes) : 0,
                Revenue = revenue,
                CompletionRate = appointments.Count > 0 ? (double)completed / appointments.Count * 100 : 0,
                AverageRating = avgRating
            });
        }

        return stats;
    }

    private async Task<TimeSlotAnalysisDto> GetTimeSlotAnalysisAsync(DateTime startDate, DateTime endDate, Guid? doctorId, CancellationToken cancellationToken)
    {
        var appointments = await _context.Appointments
            .Where(a => a.ScheduledAt >= startDate && a.ScheduledAt <= endDate)
            .Where(a => !doctorId.HasValue || a.DoctorId == doctorId)
            .ToListAsync(cancellationToken);

        var timeSlots = new List<TimeSlotDataDto>();
        var weeklyData = new List<DayDataDto>();

        // Group by hour for time slot analysis
        var hourlyData = appointments
            .GroupBy(a => a.ScheduledAt.Hour)
            .Where(g => g.Key >= 8 && g.Key <= 18) // Business hours
            .OrderBy(g => g.Key);

        foreach (var hourGroup in hourlyData)
        {
            var hour = hourGroup.Key;
            var hourAppointments = hourGroup.ToList();
            
            var timeSlotData = new TimeSlotDataDto
            {
                Hour = hour,
                TimeSlot = $"{hour:D2}:00-{(hour + 1):D2}:00",
                TotalAppointments = hourAppointments.Count
            };

            // Count by day of week
            timeSlotData.Monday = hourAppointments.Count(a => a.ScheduledAt.DayOfWeek == DayOfWeek.Monday);
            timeSlotData.Tuesday = hourAppointments.Count(a => a.ScheduledAt.DayOfWeek == DayOfWeek.Tuesday);
            timeSlotData.Wednesday = hourAppointments.Count(a => a.ScheduledAt.DayOfWeek == DayOfWeek.Wednesday);
            timeSlotData.Thursday = hourAppointments.Count(a => a.ScheduledAt.DayOfWeek == DayOfWeek.Thursday);
            timeSlotData.Friday = hourAppointments.Count(a => a.ScheduledAt.DayOfWeek == DayOfWeek.Friday);
            timeSlotData.Saturday = hourAppointments.Count(a => a.ScheduledAt.DayOfWeek == DayOfWeek.Saturday);
            timeSlotData.Sunday = hourAppointments.Count(a => a.ScheduledAt.DayOfWeek == DayOfWeek.Sunday);

            timeSlots.Add(timeSlotData);
        }

        // Group by day of week for weekly analysis
        var weekDays = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
        
        for (int i = 0; i < 7; i++)
        {
            var dayOfWeek = (DayOfWeek)((i + 1) % 7); // Adjust for Sunday = 0
            var dayAppointments = appointments.Where(a => a.ScheduledAt.DayOfWeek == dayOfWeek).ToList();
            
            var peakHour = dayAppointments
                .GroupBy(a => a.ScheduledAt.Hour)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault();

            weeklyData.Add(new DayDataDto
            {
                Day = weekDays[i],
                TotalAppointments = dayAppointments.Count,
                PeakHour = peakHour != null ? $"{peakHour.Key:D2}:00-{(peakHour.Key + 1):D2}:00" : "-",
                UtilizationRate = dayAppointments.Count > 0 ? Math.Min(100, dayAppointments.Count * 10) : 0 // Simple calculation
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
            // Log error
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
            // Log error
            Console.WriteLine($"[ERROR] Failed to retrieve {componentName}: {ex.Message}");
            return new T();
        }
    }
}
