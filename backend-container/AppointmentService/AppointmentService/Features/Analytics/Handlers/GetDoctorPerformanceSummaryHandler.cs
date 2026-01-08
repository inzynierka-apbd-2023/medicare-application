using MediatR;
using Microsoft.EntityFrameworkCore;
using AppointmentService.Data;
using AppointmentService.Features.Analytics.DTOs;
using AppointmentService.Features.Analytics.Queries;

namespace AppointmentService.Features.Analytics.Handlers;

public class GetDoctorPerformanceSummaryHandler : IRequestHandler<GetDoctorPerformanceSummaryQuery, DoctorPerformanceSummaryDto>
{
    private readonly AppointmentDbContext _context;

    public GetDoctorPerformanceSummaryHandler(AppointmentDbContext context) => _context = context;

    public async Task<DoctorPerformanceSummaryDto> Handle(GetDoctorPerformanceSummaryQuery request, CancellationToken cancellationToken)
    {
        var end = request.EndDate ?? DateTime.UtcNow.Date;
        var start = request.StartDate ?? end.AddDays(-30);

        // Use Appointments table instead of ScheduleAppointments
        var appts = await _context.Appointments
            .Where(a => a.ScheduledAt >= start && a.ScheduledAt <= end)
            .ToListAsync(cancellationToken);

        var doctorGroups = appts.GroupBy(a => a.DoctorId).ToList();
        var totalDoctors = doctorGroups.Count;

        decimal avgPerDoctor = 0;
        if (totalDoctors > 0)
        {
            var totalAppts = appts.Count;
            avgPerDoctor = totalAppts == 0 ? 0 : (decimal)totalAppts / totalDoctors;
        }

        // Find top performing doctor by completed appointments
        string topRatedDoctor = "N/A";
        if (doctorGroups.Any())
        {
            var topDoctor = doctorGroups
                .OrderByDescending(g => g.Count(a => a.Status == "Completed"))
                .First();
            topRatedDoctor = $"Doctor {topDoctor.Key.ToString()[..8]}";
        }

        // Calculate completion rate as a proxy for "rating"
        decimal doctorAverageRating = 0;
        if (appts.Any())
        {
            var completedCount = appts.Count(a => a.Status == "Completed");
            doctorAverageRating = Math.Round((decimal)completedCount / appts.Count * 5, 2); // Scale to 0-5
        }

        return new DoctorPerformanceSummaryDto
        {
            TotalDoctors = totalDoctors,
            AverageAppointmentsPerDoctor = Math.Round(avgPerDoctor, 2),
            TopRatedDoctor = topRatedDoctor,
            DoctorAverageRating = doctorAverageRating,
            StartDate = start,
            EndDate = end
        };
    }
}
