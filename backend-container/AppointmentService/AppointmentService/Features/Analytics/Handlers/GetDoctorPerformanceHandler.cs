using MediatR;
using Microsoft.EntityFrameworkCore;
using AppointmentService.Data;
using AppointmentService.Features.Analytics.DTOs;
using AppointmentService.Features.Analytics.Queries;

namespace AppointmentService.Features.Analytics.Handlers;

public class GetDoctorPerformanceHandler : IRequestHandler<GetDoctorPerformanceQuery, IEnumerable<DoctorPerformanceDto>>
{
    private readonly AppointmentDbContext _context;

    public GetDoctorPerformanceHandler(AppointmentDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<DoctorPerformanceDto>> Handle(GetDoctorPerformanceQuery request, CancellationToken cancellationToken)
    {
        var endDate = request.EndDate ?? DateTime.UtcNow;
        var startDate = request.StartDate ?? endDate.AddDays(-30);

        // Get all appointments in the date range, grouped by doctor
        var query = _context.Appointments
            .Where(a => a.ScheduledAt >= startDate && a.ScheduledAt <= endDate);

        if (request.DoctorId.HasValue)
            query = query.Where(a => a.DoctorId == request.DoctorId);

        var appointments = await query.ToListAsync(cancellationToken);

        // Group by doctor
        var doctorGroups = appointments.GroupBy(a => a.DoctorId);
        var performanceList = new List<DoctorPerformanceDto>();

        foreach (var group in doctorGroups)
        {
            var doctorId = group.Key;
            var doctorAppointments = group.ToList();
            var totalAppointments = doctorAppointments.Count;
            var completedAppointments = doctorAppointments.Count(a => a.Status == "Completed");
            var cancelledAppointments = doctorAppointments.Count(a => a.Status == "Cancelled");
            var noShowAppointments = doctorAppointments.Count(a => a.Status == "NoShow" || a.Status == "Overdue");

            // Calculate utilization rate based on working hours
            var totalWorkingMinutes = (endDate - startDate).Days * 8 * 60; // Assuming 8 hours per day
            var bookedMinutes = doctorAppointments.Sum(a => (a.ScheduledEndAt - a.ScheduledAt).TotalMinutes);
            var utilizationRate = totalWorkingMinutes > 0 ? (bookedMinutes / totalWorkingMinutes) * 100 : 0;

            performanceList.Add(new DoctorPerformanceDto
            {
                Id = doctorId,
                Name = $"Doctor {doctorId.ToString()[..8]}", // Placeholder name since we can't access user profiles
                Specialization = "General", // Placeholder since we can't access practitioner service
                TotalAppointments = totalAppointments,
                CompletedAppointments = completedAppointments,
                CancelledAppointments = cancelledAppointments,
                NoShowAppointments = noShowAppointments,
                AverageRating = 0, // Not available
                TotalRatings = 0, // Not available
                Revenue = 0, // Not available
                UtilizationRate = utilizationRate
            });
        }

        return performanceList.OrderByDescending(p => p.TotalAppointments);
    }
}
