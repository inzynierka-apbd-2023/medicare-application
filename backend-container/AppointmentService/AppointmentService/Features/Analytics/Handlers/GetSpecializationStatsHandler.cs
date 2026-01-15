using MediatR;
using Microsoft.EntityFrameworkCore;
using AppointmentService.Data;
using AppointmentService.Features.Analytics.DTOs;
using AppointmentService.Features.Analytics.Queries;
using AppointmentService.Services;
using AppointmentService.Models;

namespace AppointmentService.Features.Analytics.Handlers;

public class GetSpecializationStatsHandler : IRequestHandler<GetSpecializationStatsQuery, IEnumerable<SpecializationStatsDto>>
{
    private readonly AppointmentDbContext _context;
    private readonly IDoctorProfileClient _doctorProfileClient;
    private readonly ILogger<GetSpecializationStatsHandler> _logger;

    public GetSpecializationStatsHandler(
        AppointmentDbContext context,
        IDoctorProfileClient doctorProfileClient,
        ILogger<GetSpecializationStatsHandler> logger)
    {
        _context = context;
        _doctorProfileClient = doctorProfileClient;
        _logger = logger;
    }

    public async Task<IEnumerable<SpecializationStatsDto>> Handle(GetSpecializationStatsQuery request, CancellationToken cancellationToken)
    {
        var endDate = request.EndDate ?? DateTime.UtcNow;
        var startDate = request.StartDate ?? endDate.AddDays(-30);

        var appointments = await _context.Appointments.AsNoTracking()
            .Where(a => a.ScheduledAt >= startDate && a.ScheduledAt <= endDate)
            .ToListAsync(cancellationToken);

        if (!appointments.Any())
        {
            return Enumerable.Empty<SpecializationStatsDto>();
        }
            
        var appointmentIds = appointments.Select(a => a.Id).ToList();
        var doctorIds = appointments.Select(a => a.DoctorId).Distinct().ToList();

        // Fetch doctor profiles to get specializations
        _logger.LogInformation("[SpecializationStats] Fetching profiles for {Count} doctors", doctorIds.Count);
        var profiles = await _doctorProfileClient.GetDoctorProfilesAsync(doctorIds, cancellationToken);
        
        var doctorSpecializationMap = new Dictionary<Guid, string>();
        foreach (var p in profiles)
        {
            // Use primary specialization or first one if comma separated
            var spec = p.SpecializationNames.Split(',', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.Trim() ?? "General";
            doctorSpecializationMap[p.DoctorId] = spec;
            if (p.UserId != p.DoctorId)
            {
                doctorSpecializationMap[p.UserId] = spec;
            }
        }

        var payments = new List<AppointmentPayment>();
        var rates = new List<Rate>();

        try
        {
            payments = await _context.AppointmentPayments.AsNoTracking()
                .Where(p => appointmentIds.Contains(p.AppointmentId))
                .ToListAsync(cancellationToken);

            rates = await _context.Rates.AsNoTracking()
                .Where(r => r.Appointment_Id.HasValue && appointmentIds.Contains(r.Appointment_Id.Value))
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
             _logger.LogError(ex, "[AnalyticsWarning] SpecializationStats failed to fetch cross-context data");
        }

        // Group by Specialization (determined from Doctor)
        var groupedBySpec = appointments
            .GroupBy(a => 
            {
                if (doctorSpecializationMap.TryGetValue(a.DoctorId, out var spec))
                    return spec;
                return "General";
            })
            .Select(g => 
            {
                 var groupApptIds = g.Select(a => a.Id).ToHashSet();
                 var groupRevenue = payments
                    .Where(p => groupApptIds.Contains(p.AppointmentId))
                    .Sum(p => p.AmountCents) / 100.0m;
                    
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
                    CompletionRate = g.Count() > 0 ? (double)g.Count(a => string.Equals(a.Status, "Completed", StringComparison.OrdinalIgnoreCase)) / g.Count() * 100 : 0,
                    AverageRating = (double)groupAvgRating
                 };
            })
            .OrderByDescending(s => s.TotalAppointments)
            .ToList();

        return groupedBySpec;
    }
}
