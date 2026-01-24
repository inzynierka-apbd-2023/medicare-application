using MediatR;
using Microsoft.EntityFrameworkCore;
using AppointmentService.Data;
using AppointmentService.Features.Analytics.DTOs;
using AppointmentService.Features.Analytics.Queries;
using AppointmentService.Services;

namespace AppointmentService.Features.Analytics.Handlers;

public class GetDoctorPerformanceHandler : IRequestHandler<GetDoctorPerformanceQuery, IEnumerable<DoctorPerformanceDto>>
{
    private readonly AppointmentDbContext _context;
    private readonly MassTransit.IRequestClient<Medicare.Messaging.Contracts.IGetDoctors> _doctorClient;
    private readonly MassTransit.IRequestClient<Medicare.Messaging.Contracts.IGetAppointmentPayments> _paymentClient;
    private readonly MassTransit.IRequestClient<Medicare.Messaging.Contracts.IGetAppointmentRatings> _ratingsClient;
    private readonly ILogger<GetDoctorPerformanceHandler> _logger;

    public GetDoctorPerformanceHandler(
        AppointmentDbContext context, 
        MassTransit.IRequestClient<Medicare.Messaging.Contracts.IGetDoctors> doctorClient,
        MassTransit.IRequestClient<Medicare.Messaging.Contracts.IGetAppointmentPayments> paymentClient,
        MassTransit.IRequestClient<Medicare.Messaging.Contracts.IGetAppointmentRatings> ratingsClient,
        ILogger<GetDoctorPerformanceHandler> logger)
    {
        _context = context;
        _doctorClient = doctorClient;
        _paymentClient = paymentClient;
        _ratingsClient = ratingsClient;
        _logger = logger;
    }

    public async Task<IEnumerable<DoctorPerformanceDto>> Handle(GetDoctorPerformanceQuery request, CancellationToken cancellationToken)
    {
        var endDate = request.EndDate ?? DateTime.UtcNow;
        var startDate = request.StartDate ?? endDate.AddDays(-30);

        // Get all appointments in the date range
        var query = _context.Appointments.AsNoTracking()
            .Where(a => a.ScheduledAt >= startDate && a.ScheduledAt <= endDate);

        if (request.DoctorId.HasValue)
            query = query.Where(a => a.DoctorId == request.DoctorId);

        var appointments = await query.ToListAsync(cancellationToken);

        if (appointments.Count == 0)
        {
            _logger.LogInformation("[Analytics] No appointments found in date range {Start} to {End}", startDate, endDate);
            return Enumerable.Empty<DoctorPerformanceDto>();
        }

        var doctorIds = appointments.Select(a => a.DoctorId).Distinct().ToList();
        _logger.LogInformation("[Analytics] Getting profiles for {Count} doctors via RabbitMQ", doctorIds.Count);

        List<AppointmentService.Features.Analytics.DTOs.DoctorProfileDto> profiles = new();

        var resp = await _doctorClient.GetResponse<Medicare.Messaging.Contracts.IDoctorProfiles>(new { DoctorIds = doctorIds }, cancellationToken);
        profiles = resp.Message.Profiles.Select(p => new AppointmentService.Features.Analytics.DTOs.DoctorProfileDto
        {
            DoctorId = p.DoctorId,
            UserId = p.UserId,
            FirstName = p.FirstName,
            LastName = p.LastName,
            SpecializationNames = p.SpecializationNames
        }).ToList();

        var profileMap = profiles.ToDictionary(
            p => p.DoctorId, 
            p => (Name: $"{p.FirstName} {p.LastName}".Trim(), Specialization: p.SpecializationNames));
        
        foreach (var p in profiles.Where(p => p.UserId != p.DoctorId))
        {
            profileMap[p.UserId] = (Name: $"{p.FirstName} {p.LastName}".Trim(), Specialization: p.SpecializationNames);
        }

        _logger.LogInformation("[Analytics] Received {Count} doctor profiles", profiles.Count);

        var appointmentIds = appointments.Select(a => a.Id).ToList();
        
        var payments = new List<AppointmentPaymentDto>();

        var response = await _paymentClient.GetResponse<Medicare.Messaging.Contracts.IAppointmentPayments>(new { AppointmentIds = appointmentIds }, cancellationToken);
        payments = response.Message.Payments.Select(p => new AppointmentPaymentDto 
        { 
            AppointmentId = p.AppointmentId, 
            AmountCents = (int)p.AmountCents, 
            Status = p.Status 
        }).ToList();

        // Get ratings via RabbitMQ from PractitionerService
        var ratingsResponse = await _ratingsClient.GetResponse<Medicare.Messaging.Contracts.IAppointmentRatings>(new { AppointmentIds = appointmentIds }, cancellationToken);
        var ratings = ratingsResponse.Message.Ratings.Select(r => new RatingDto
        {
            AppointmentId = r.AppointmentId,
            RateValue = r.RateValue
        }).ToList();

        var doctorGroups = appointments.GroupBy(a => a.DoctorId);
        var performanceList = new List<DoctorPerformanceDto>();

        foreach (var group in doctorGroups)
        {
            var doctorId = group.Key;
            var doctorAppointments = group.ToList();
            var docApptIds = doctorAppointments.Select(a => a.Id).ToHashSet();

            var totalAppointments = doctorAppointments.Count;
            var completedAppointments = doctorAppointments.Count(a => string.Equals(a.Status, "Completed", StringComparison.OrdinalIgnoreCase));
            var cancelledAppointments = doctorAppointments.Count(a => string.Equals(a.Status, "Cancelled", StringComparison.OrdinalIgnoreCase));
            var noShowAppointments = doctorAppointments.Count(a => 
                string.Equals(a.Status, "NoShow", StringComparison.OrdinalIgnoreCase) || 
                string.Equals(a.Status, "Overdue", StringComparison.OrdinalIgnoreCase));

            // Calculate utilization
            var totalWorkingMinutes = (endDate - startDate).Days * 8 * 60;
            var bookedMinutes = doctorAppointments.Sum(a => (a.ScheduledEndAt - a.ScheduledAt).TotalMinutes);
            var utilizationRate = totalWorkingMinutes > 0 ? (bookedMinutes / totalWorkingMinutes) * 100 : 0;

            // Revenue
            var docRevenue = payments
                .Where(p => docApptIds.Contains(p.AppointmentId))
                .Sum(p => p.AmountCents) / 100.0m;

            // Ratings from RabbitMQ response
            var docRatings = ratings.Where(r => docApptIds.Contains(r.AppointmentId)).ToList();
            var avgRating = docRatings.Any() ? docRatings.Average(r => r.RateValue) : 0;

            // Get name and specialization from RabbitMQ response
            var (name, specialization) = profileMap.TryGetValue(doctorId, out var profile)
                ? profile
                : ($"Doctor {doctorId.ToString()[..8]}", "General");

            // Fallback if name is empty
            if (string.IsNullOrWhiteSpace(name))
                name = $"Doctor {doctorId.ToString()[..8]}";
            if (string.IsNullOrWhiteSpace(specialization))
                specialization = "General";

            performanceList.Add(new DoctorPerformanceDto
            {
                Id = doctorId,
                Name = name,
                Specialization = specialization,
                TotalAppointments = totalAppointments,
                CompletedAppointments = completedAppointments,
                CancelledAppointments = cancelledAppointments,
                NoShowAppointments = noShowAppointments,
                AverageRating = (double)avgRating,
                TotalRatings = docRatings.Count,
                Revenue = docRevenue,
                UtilizationRate = utilizationRate
            });
        }

        return performanceList.OrderByDescending(p => p.TotalAppointments);
    }
}

