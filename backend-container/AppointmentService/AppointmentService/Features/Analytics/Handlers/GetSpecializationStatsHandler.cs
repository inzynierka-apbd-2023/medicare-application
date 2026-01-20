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
    private readonly MassTransit.IRequestClient<Medicare.Messaging.Contracts.IGetDoctors> _doctorClient;
    private readonly MassTransit.IRequestClient<Medicare.Messaging.Contracts.IGetAppointmentPayments> _paymentClient;
    private readonly ILogger<GetSpecializationStatsHandler> _logger;

    public GetSpecializationStatsHandler(
        AppointmentDbContext context,
        MassTransit.IRequestClient<Medicare.Messaging.Contracts.IGetDoctors> doctorClient,
        MassTransit.IRequestClient<Medicare.Messaging.Contracts.IGetAppointmentPayments> paymentClient,
        ILogger<GetSpecializationStatsHandler> logger)
    {
        _context = context;
        _doctorClient = doctorClient;
        _paymentClient = paymentClient;
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

        _logger.LogInformation("[SpecializationStats] Fetching profiles for {Count} doctors", doctorIds.Count);
        List<DoctorProfileDto> profiles = new();
        if (doctorIds.Any())
        {
            var resp = await _doctorClient.GetResponse<Medicare.Messaging.Contracts.IDoctorProfiles>(new { DoctorIds = doctorIds }, cancellationToken);
            profiles = resp.Message.Profiles.Select(p => new DoctorProfileDto
            {
                DoctorId = p.DoctorId,
                UserId = p.UserId,
                FirstName = p.FirstName,
                LastName = p.LastName,
                SpecializationNames = p.SpecializationNames
            }).ToList();
        }
        
        var doctorSpecializationMap = new Dictionary<Guid, string>();
        foreach (var p in profiles)
        {
            var spec = p.SpecializationNames.Split(',', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.Trim() ?? "General";
            doctorSpecializationMap[p.DoctorId] = spec;
            if (p.UserId != p.DoctorId)
            {
                doctorSpecializationMap[p.UserId] = spec;
            }
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
        }

        rates = await _context.Rates.AsNoTracking()
            .Where(r => r.Appointment_Id.HasValue && appointmentIds.Contains(r.Appointment_Id.Value))
            .ToListAsync(cancellationToken);

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
