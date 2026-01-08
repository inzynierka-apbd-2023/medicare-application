using MediatR;
using Microsoft.EntityFrameworkCore;
using AppointmentService.Data;
using AppointmentService.Features.Analytics.DTOs;
using AppointmentService.Features.Analytics.Queries;

namespace AppointmentService.Features.Analytics.Handlers;

public class GetSpecializationStatsHandler : IRequestHandler<GetSpecializationStatsQuery, IEnumerable<SpecializationStatsDto>>
{
    private readonly AppointmentDbContext _context;

    public GetSpecializationStatsHandler(AppointmentDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SpecializationStatsDto>> Handle(GetSpecializationStatsQuery request, CancellationToken cancellationToken)
    {
        var endDate = request.EndDate ?? DateTime.UtcNow;
        var startDate = request.StartDate ?? endDate.AddDays(-30);

        // Get all appointments in the date range
        var appointments = await _context.Appointments
            .Where(a => a.ScheduledAt >= startDate && a.ScheduledAt <= endDate)
            .ToListAsync(cancellationToken);

        // Since we don't have access to specializations, group by AppointmentType or Category as a proxy
        var groupedByType = appointments
            .GroupBy(a => a.Category ?? a.AppointmentType ?? "General")
            .Select(g => new SpecializationStatsDto
            {
                Specialization = g.Key,
                TotalAppointments = g.Count(),
                TotalPatients = g.Select(a => a.PatientId).Distinct().Count(),
                TotalDoctors = g.Select(a => a.DoctorId).Distinct().Count(),
                AverageAppointmentDuration = g.Any() ? g.Average(a => (a.ScheduledEndAt - a.ScheduledAt).TotalMinutes) : 0,
                Revenue = 0, // Not available
                CompletionRate = g.Count() > 0 ? (double)g.Count(a => a.Status == "Completed") / g.Count() * 100 : 0,
                AverageRating = 0 // Not available
            })
            .ToList();

        return groupedByType;
    }
}
