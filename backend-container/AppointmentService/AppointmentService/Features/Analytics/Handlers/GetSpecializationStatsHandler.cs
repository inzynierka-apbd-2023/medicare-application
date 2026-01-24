using MediatR;
using Microsoft.EntityFrameworkCore;
using AppointmentService.Data;
using AppointmentService.Features.Analytics.DTOs;
using AppointmentService.Features.Analytics.Queries;
using AppointmentService.Services;

namespace AppointmentService.Features.Analytics.Handlers;

public class GetSpecializationStatsHandler : IRequestHandler<GetSpecializationStatsQuery, IEnumerable<SpecializationStatsDto>>
{
    private readonly AppointmentDbContext _context;
    private readonly MassTransit.IRequestClient<Medicare.Messaging.Contracts.IGetDoctors> _doctorClient;
    private readonly MassTransit.IRequestClient<Medicare.Messaging.Contracts.IGetAppointmentPayments> _paymentClient;
    private readonly MassTransit.IRequestClient<Medicare.Messaging.Contracts.IGetAppointmentRatings> _ratingsClient;
    private readonly ILogger<GetSpecializationStatsHandler> _logger;

    public GetSpecializationStatsHandler(
        AppointmentDbContext context,
        MassTransit.IRequestClient<Medicare.Messaging.Contracts.IGetDoctors> doctorClient,
        MassTransit.IRequestClient<Medicare.Messaging.Contracts.IGetAppointmentPayments> paymentClient,
        MassTransit.IRequestClient<Medicare.Messaging.Contracts.IGetAppointmentRatings> ratingsClient,
        ILogger<GetSpecializationStatsHandler> logger)
    {
        _context = context;
        _doctorClient = doctorClient;
        _paymentClient = paymentClient;
        _ratingsClient = ratingsClient;
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
        var ratings = new List<RatingDto>();

        if (appointmentIds.Any())
        {
            var response = await _paymentClient.GetResponse<Medicare.Messaging.Contracts.IAppointmentPayments>(new { AppointmentIds = appointmentIds }, cancellationToken);
            payments = response.Message.Payments.Select(p => new AppointmentPaymentDto 
            { 
                AppointmentId = p.AppointmentId, 
                AmountCents = (int)p.AmountCents, 
                Status = p.Status 
            }).ToList();

            // Get ratings via RabbitMQ from PractitionerService
            var ratingsResponse = await _ratingsClient.GetResponse<Medicare.Messaging.Contracts.IAppointmentRatings>(new { AppointmentIds = appointmentIds }, cancellationToken);
            ratings = ratingsResponse.Message.Ratings.Select(r => new RatingDto
            {
                AppointmentId = r.AppointmentId,
                RateValue = r.RateValue
            }).ToList();
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
                    
                 var groupRatings = ratings.Where(r => groupApptIds.Contains(r.AppointmentId)).ToList();
                 var groupAvgRating = groupRatings.Any() ? groupRatings.Average(r => r.RateValue) : 0;
                 
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

