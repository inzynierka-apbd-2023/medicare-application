using MediatR;
using Microsoft.EntityFrameworkCore;
using AppointmentService.Data;
using AppointmentService.Features.Analytics.DTOs;
using AppointmentService.Features.Analytics.Queries;
using AppointmentService.Models;
using AppointmentService.Services;

namespace AppointmentService.Features.Analytics.Handlers;

public class GetAppointmentAnalyticsHandler : IRequestHandler<GetAppointmentAnalyticsQuery, AppointmentAnalyticsResponse>
{
    private readonly AppointmentDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly MassTransit.IRequestClient<Medicare.Messaging.Contracts.IGetDoctors> _doctorClient;
    private readonly MassTransit.IRequestClient<Medicare.Messaging.Contracts.IGetAppointmentPayments> _paymentClient;

    public GetAppointmentAnalyticsHandler(
        AppointmentDbContext context, 
        INotificationService notificationService,
        MassTransit.IRequestClient<Medicare.Messaging.Contracts.IGetDoctors> doctorClient,
        MassTransit.IRequestClient<Medicare.Messaging.Contracts.IGetAppointmentPayments> paymentClient)
    {
        _context = context;
        _notificationService = notificationService;
        _doctorClient = doctorClient;
        _paymentClient = paymentClient;
    }

    public async Task<AppointmentAnalyticsResponse> Handle(GetAppointmentAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var endDate = request.EndDate ?? DateTime.UtcNow;
        var startDate = request.StartDate ?? endDate.AddDays(-30);

        var metrics = await GetMetricsAsync(startDate, endDate, request.DoctorId, cancellationToken);
        var trends = await GetTrendsAsync(startDate, endDate, request.DoctorId, cancellationToken);
        var doctorPerformance = await GetDoctorPerformanceAsync(startDate, endDate, request.DoctorId, cancellationToken);
        var specializationStats = await GetSpecializationStatsAsync(startDate, endDate, cancellationToken);
        var timeAnalysis = await GetTimeSlotAnalysisAsync(startDate, endDate, request.DoctorId, cancellationToken);

        await _notificationService.CreateNotificationAsync(new CreateNotificationRequest
        {
            RecipientUserId = "system",
            Description = "Appointment analytics dashboard accessed",
            Type = 1,
            SourceService = "AppointmentService",
            Priority = "Low"
        });

        return new AppointmentAnalyticsResponse
        {
            Metrics = metrics,
            Trends = trends,
            DoctorPerformance = doctorPerformance,
            SpecializationStats = specializationStats,
            TimeAnalysis = timeAnalysis
        };
    }

    private async Task<IEnumerable<AppointmentMetricDto>> GetMetricsAsync(DateTime startDate, DateTime endDate, Guid? doctorId, CancellationToken cancellationToken)
    {
        var query = _context.Appointments.AsNoTracking()
            .Where(a => a.ScheduledAt >= startDate && a.ScheduledAt <= endDate);

        if (doctorId.HasValue)
            query = query.Where(a => a.DoctorId == doctorId);

        var appointments = await query.ToListAsync(cancellationToken);
        
        var previousPeriodStart = startDate.AddDays(-(endDate - startDate).Days);
        var prevQuery = _context.Appointments.AsNoTracking()
            .Where(a => a.ScheduledAt >= previousPeriodStart && a.ScheduledAt < startDate);
             
        if (doctorId.HasValue)
            prevQuery = prevQuery.Where(a => a.DoctorId == doctorId);
            
        var previousAppointments = await prevQuery.ToListAsync(cancellationToken);

        var appointmentIds = appointments.Select(a => a.Id).ToList();
        
        var payments = new List<AppointmentPaymentDto>();
        var rates = new List<Rate>();
        var prevPayments = new List<AppointmentPaymentDto>();
        var prevRates = new List<Rate>();

        if (appointmentIds.Any())
        {
            var response = await _paymentClient.GetResponse<Medicare.Messaging.Contracts.IAppointmentPayments>(new { AppointmentIds = appointmentIds }, cancellationToken);
            payments = response.Message.Payments.Select(p => new AppointmentPaymentDto 
            { 
                AppointmentId = p.AppointmentId, 
                AmountCents = (int)p.AmountCents, 
                Status = p.Status
            }).ToList();

            rates = await _context.Rates.AsNoTracking()
                .Where(r => r.Appointment_Id.HasValue && appointmentIds.Contains(r.Appointment_Id.Value))
                .ToListAsync(cancellationToken);
        }
            
        var prevAppointmentIds = previousAppointments.Select(a => a.Id).ToList();
        if (prevAppointmentIds.Any())
        {
            var response = await _paymentClient.GetResponse<Medicare.Messaging.Contracts.IAppointmentPayments>(new { AppointmentIds = prevAppointmentIds }, cancellationToken);
            prevPayments = response.Message.Payments.Select(p => new AppointmentPaymentDto 
            { 
                AppointmentId = p.AppointmentId, 
                AmountCents = (int)p.AmountCents, 
                Status = p.Status 
            }).ToList();

            prevRates = await _context.Rates.AsNoTracking()
                .Where(r => r.Appointment_Id.HasValue && prevAppointmentIds.Contains(r.Appointment_Id.Value))
                .ToListAsync(cancellationToken);
        }

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

        var totalRevenue = payments.Sum(p => p.AmountCents) / 100.0m; 
        var prevRevenue = prevPayments.Sum(p => p.AmountCents) / 100.0m;
        var revenueChange = prevRevenue > 0 ? (double)((totalRevenue - prevRevenue) / prevRevenue) * 100 : 0;

        var avgRating = rates.Any() ? rates.Average(r => r.Rate_Value ?? 0) : 0;
        var prevAvgRating = prevRates.Any() ? prevRates.Average(r => r.Rate_Value ?? 0) : 0;
        var finalRating = avgRating; 
        var ratingChange = prevAvgRating > 0 ? ((avgRating - prevAvgRating) / prevAvgRating) * 100 : 0;

        return new List<AppointmentMetricDto>
        {
            new() { Id = Guid.NewGuid(), Title = "Total Appointments", Value = totalAppointments, Change = totalChange, Period = "vs last period", Icon = "calendar" },
            new() { Id = Guid.NewGuid(), Title = "Completed", Value = completedAppointments, Change = completedChange, Period = "vs last period", Icon = "trending" },
            new() { Id = Guid.NewGuid(), Title = "Active Patients", Value = activePatients, Change = patientsChange, Period = "vs last period", Icon = "users" },
            new() { Id = Guid.NewGuid(), Title = "Avg Duration", Value = (int)avgDuration, Change = durationChange, Period = "minutes", Icon = "clock" },
            new() { Id = Guid.NewGuid(), Title = "Total Revenue", Value = (int)totalRevenue, Change = revenueChange, Period = "vs last period", Icon = "dollar" },
            new() { Id = Guid.NewGuid(), Title = "Avg Rating", Value = (int)finalRating, Change = ratingChange, Period = "vs last period", Icon = "star" }
        };
    }

    private async Task<IEnumerable<TrendDataDto>> GetTrendsAsync(DateTime startDate, DateTime endDate, Guid? doctorId, CancellationToken cancellationToken)
    {
        var query = _context.Appointments.AsNoTracking()
            .Where(a => a.ScheduledAt >= startDate && a.ScheduledAt <= endDate);

        if (doctorId.HasValue)
            query = query.Where(a => a.DoctorId == doctorId);

        var appointments = await query.ToListAsync(cancellationToken);
        
        var appointmentIds = appointments.Select(a => a.Id).ToList();
        var payments = new List<AppointmentPaymentDto>();
        

        if (appointmentIds.Any())
        {
            var response = await _paymentClient.GetResponse<Medicare.Messaging.Contracts.IAppointmentPayments>(new { AppointmentIds = appointmentIds }, cancellationToken);
            payments = response.Message.Payments.Select(p => new AppointmentPaymentDto 
            { 
                AppointmentId = p.AppointmentId, 
                AmountCents = (int)p.AmountCents, 
                Status = p.Status 
            }).ToList();
        }
        
        var trends = new List<TrendDataDto>();
        
        for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
        {
            var nextDate = date.AddDays(1);
            var dayAppointments = appointments.Where(a => a.ScheduledAt >= date && a.ScheduledAt < nextDate).ToList();
            var dayApptIds = dayAppointments.Select(a => a.Id).ToHashSet();
            
            var dayRevenue = payments
                .Where(p => dayApptIds.Contains(p.AppointmentId))
                .Sum(p => p.AmountCents) / 100.0m;

            trends.Add(new TrendDataDto
            {
                Date = date.ToString("yyyy-MM-dd"),
                Appointments = dayAppointments.Count,
                Completed = dayAppointments.Count(a => a.Status == "Completed"),
                Cancelled = dayAppointments.Count(a => a.Status == "Cancelled"),
                NoShow = dayAppointments.Count(a => a.Status == "NoShow" || a.Status == "Overdue"),
                Revenue = dayRevenue
            });
        }

        return trends;
    }

    private async Task<IEnumerable<DoctorPerformanceDto>> GetDoctorPerformanceAsync(DateTime startDate, DateTime endDate, Guid? doctorId, CancellationToken cancellationToken)
    {
        var query = _context.Appointments.AsNoTracking()
            .Where(a => a.ScheduledAt >= startDate && a.ScheduledAt <= endDate);

        if (doctorId.HasValue)
            query = query.Where(a => a.DoctorId == doctorId);

        var appointments = await query.ToListAsync(cancellationToken);
        
        var appointmentIds = appointments.Select(a => a.Id).ToList();
        
        var payments = new List<AppointmentPaymentDto>();
        var rates = new List<Rate>();
        
        var doctorIds = appointments.Select(a => a.DoctorId).Distinct().ToList();
        
        List<DoctorProfileDto> doctorProfiles = new();
        if (doctorIds.Any())
        {
            var resp = await _doctorClient.GetResponse<Medicare.Messaging.Contracts.IDoctorProfiles>(new { DoctorIds = doctorIds }, cancellationToken);
            doctorProfiles = resp.Message.Profiles.Select(p => new AppointmentService.Features.Analytics.DTOs.DoctorProfileDto
            {
                DoctorId = p.DoctorId,
                UserId = p.UserId,
                FirstName = p.FirstName,
                LastName = p.LastName,
                SpecializationNames = p.SpecializationNames
            }).ToList();
        }

        var profileMap = doctorProfiles.ToDictionary(
            p => p.DoctorId,
            p => (Name: $"{p.FirstName} {p.LastName}".Trim(), Spec: p.SpecializationNames)
        );

        foreach(var p in doctorProfiles.Where(p => p.UserId != p.DoctorId))
            profileMap[p.UserId] = ($"{p.FirstName} {p.LastName}".Trim(), p.SpecializationNames);

        if (appointmentIds.Any())
        {
            var response = await _paymentClient.GetResponse<Medicare.Messaging.Contracts.IAppointmentPayments>(new { AppointmentIds = appointmentIds }, cancellationToken);
            payments = response.Message.Payments.Select(p => new AppointmentPaymentDto 
            { 
                AppointmentId = p.AppointmentId, 
                AmountCents = (int)p.AmountCents, 
                Status = p.Status 
            }).ToList();

            rates = await _context.Rates.AsNoTracking()
                .Where(r => r.Appointment_Id.HasValue && appointmentIds.Contains(r.Appointment_Id.Value))
                .ToListAsync(cancellationToken);
        }

        var doctorGroups = appointments.GroupBy(a => a.DoctorId);
        var performance = new List<DoctorPerformanceDto>();

        foreach (var group in doctorGroups)
        {
            var docId = group.Key;
            var docAppointments = group.ToList();
            var docApptIds = docAppointments.Select(a => a.Id).ToHashSet();
            
            var totalWorkingMinutes = (endDate - startDate).Days * 8 * 60;
            var bookedMinutes = docAppointments.Sum(a => (a.ScheduledEndAt - a.ScheduledAt).TotalMinutes);
            
            var docRevenue = payments
                .Where(p => docApptIds.Contains(p.AppointmentId))
                .Sum(p => p.AmountCents) / 100.0m;

            var docRates = rates
                .Where(r => r.Appointment_Id.HasValue && docApptIds.Contains(r.Appointment_Id.Value))
                .ToList();
            
            var avgRating = docRates.Any() ? docRates.Average(r => r.Rate_Value ?? 0) : 0;
            
            var (name, spec) = profileMap.TryGetValue(docId, out var p) ? p : ($"Doctor {docId.ToString()[..8]}", "General");
            if (string.IsNullOrWhiteSpace(name)) name = $"Doctor {docId.ToString()[..8]}";
            if (string.IsNullOrWhiteSpace(spec)) spec = "General";

            performance.Add(new DoctorPerformanceDto
            {
                Id = docId,
                Name = name,
                Specialization = spec,
                TotalAppointments = docAppointments.Count,
                CompletedAppointments = docAppointments.Count(a => a.Status == "Completed"),
                CancelledAppointments = docAppointments.Count(a => a.Status == "Cancelled"),
                NoShowAppointments = docAppointments.Count(a => a.Status == "NoShow" || a.Status == "Overdue"),
                AverageRating = (double)avgRating,
                TotalRatings = docRates.Count,
                Revenue = docRevenue,
                UtilizationRate = totalWorkingMinutes > 0 ? (bookedMinutes / totalWorkingMinutes) * 100 : 0
            });
        }

        return performance.OrderByDescending(p => p.TotalAppointments);
    }

    private async Task<IEnumerable<SpecializationStatsDto>> GetSpecializationStatsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var appointments = await _context.Appointments.AsNoTracking()
            .Where(a => a.ScheduledAt >= startDate && a.ScheduledAt <= endDate)
            .ToListAsync(cancellationToken);
            
        var appointmentIds = appointments.Select(a => a.Id).ToList();
        var doctorIds = appointments.Select(a => a.DoctorId).Distinct().ToList();

        List<DoctorProfileDto> profiles = new();
        if (doctorIds.Any())
        {
            var resp = await _doctorClient.GetResponse<Medicare.Messaging.Contracts.IDoctorProfiles>(new { DoctorIds = doctorIds }, cancellationToken);
            profiles = resp.Message.Profiles.Select(p => new AppointmentService.Features.Analytics.DTOs.DoctorProfileDto
            {
                DoctorId = p.DoctorId,
                UserId = p.UserId,
                FirstName = p.FirstName,
                LastName = p.LastName,
                SpecializationNames = p.SpecializationNames
            }).ToList();
        }

        var docSpecMap = new Dictionary<Guid, string>();
        foreach (var p in profiles)
        {
             var spec = p.SpecializationNames.Split(',', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.Trim() ?? "General";
             docSpecMap[p.DoctorId] = spec;
             if (p.UserId != p.DoctorId) docSpecMap[p.UserId] = spec;
        }

        var payments = new List<AppointmentPaymentDto>();
        var rates = new List<Rate>();

        if (appointmentIds.Any())
        {
            var response = await _paymentClient.GetResponse<Medicare.Messaging.Contracts.IAppointmentPayments>(new { AppointmentIds = appointmentIds }, cancellationToken);
            payments = response.Message.Payments.Select(p => new AppointmentPaymentDto 
            { 
                AppointmentId = p.AppointmentId, 
                AmountCents = (int)p.AmountCents, 
                Status = p.Status 
            }).ToList();
                
            rates = await _context.Rates.AsNoTracking()
                .Where(r => r.Appointment_Id.HasValue && appointmentIds.Contains(r.Appointment_Id.Value))
                .ToListAsync(cancellationToken);
        }

        var groupedByType = appointments
            .GroupBy(a => 
            {
                if (docSpecMap.TryGetValue(a.DoctorId, out var spec)) return spec;
                return "General";
            })
            .Select(g => 
            {
                 var groupApptIds = g.Select(a => a.Id).ToHashSet();
                 var groupRevenue = payments.Where(p => groupApptIds.Contains(p.AppointmentId)).Sum(p => p.AmountCents) / 100.0m;
                 var groupRates = rates.Where(r => r.Appointment_Id.HasValue && groupApptIds.Contains(r.Appointment_Id.Value)).ToList();
                 var groupAvgRating = groupRates.Any() ? groupRates.Average(r => r.Rate_Value ?? 0) : 0;
                 
                 return new SpecializationStatsDto
                 {
                    Specialization = g.Key,
                    TotalAppointments = g.Count(),
                    TotalPatients = g.Select(a => a.PatientId).Distinct().Count(),
                    TotalDoctors = g.Select(a => a.DoctorId).Distinct().Count(),
                    AverageAppointmentDuration = g.Any() ? g.Average(a => (a.ScheduledEndAt - a.ScheduledAt).TotalMinutes) : 0,
                    Revenue = groupRevenue,
                    CompletionRate = g.Count() > 0 ? (double)g.Count(a => a.Status == "Completed") / g.Count() * 100 : 0,
                    AverageRating = (double)groupAvgRating
                 };
            })
            .ToList();

        return groupedByType;
    }

    private async Task<TimeSlotAnalysisDto> GetTimeSlotAnalysisAsync(DateTime startDate, DateTime endDate, Guid? doctorId, CancellationToken cancellationToken)
    {
        var query = _context.Appointments.AsNoTracking()
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
                AverageRevenue = 0, // Revenue calculation for time slots complex without joining payments, skipping for now
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

}
