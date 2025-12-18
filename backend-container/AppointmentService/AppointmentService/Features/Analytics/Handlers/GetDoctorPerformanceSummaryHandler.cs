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

        var appts = _context.ScheduleAppointments.Where(a => a.Day >= start && a.Day <= end);

        var totalDoctors = await _context.Doctors.CountAsync(cancellationToken);

        var apptCounts = await appts
            .GroupBy(a => a.Doctor_User_Id)
            .Select(g => new { DoctorId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        decimal avgPerDoctor = 0;
        if (totalDoctors > 0)
        {
            var totalAppts = apptCounts.Sum(x => x.Count);
            avgPerDoctor = totalAppts == 0 ? 0 : (decimal)totalAppts / totalDoctors;
        }

        var ratings = await _context.Rates
            .Where(r => r.Rated_At >= start && r.Rated_At <= end)
            .GroupBy(r => r.Doctor_User_Id)
            .Select(g => new { DoctorId = g.Key, Avg = g.Average(x => x.Rate_Value), Count = g.Count() })
            .ToListAsync(cancellationToken);

        decimal doctorAverageRating = 0;
        string topRatedDoctor = "N/A";
        if (ratings.Count > 0)
        {
            doctorAverageRating = (decimal)(ratings.Average(r => r.Avg) ?? 0);
            var top = ratings.OrderByDescending(r => r.Avg).ThenByDescending(r => r.Count).First();
            var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.User_Id == top.DoctorId, cancellationToken);
            if (profile != null)
                topRatedDoctor = $"{profile.FirstName} {profile.LastName}".Trim();
        }

        return new DoctorPerformanceSummaryDto
        {
            TotalDoctors = totalDoctors,
            AverageAppointmentsPerDoctor = Math.Round(avgPerDoctor, 2),
            TopRatedDoctor = topRatedDoctor,
            DoctorAverageRating = Math.Round(doctorAverageRating, 2),
            StartDate = start,
            EndDate = end
        };
    }
}
