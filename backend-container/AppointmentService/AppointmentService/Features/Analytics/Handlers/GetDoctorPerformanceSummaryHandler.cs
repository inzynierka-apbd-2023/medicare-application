using MediatR;
using Microsoft.EntityFrameworkCore;
using AppointmentService.Data;
using AppointmentService.Features.Analytics.DTOs;
using AppointmentService.Features.Analytics.Queries;
using AppointmentService.Services;

namespace AppointmentService.Features.Analytics.Handlers;

public class GetDoctorPerformanceSummaryHandler : IRequestHandler<GetDoctorPerformanceSummaryQuery, DoctorPerformanceSummaryDto>
{
    private readonly AppointmentDbContext _context;
    private readonly IDoctorProfileClient _doctorProfileClient;

    public GetDoctorPerformanceSummaryHandler(AppointmentDbContext context, IDoctorProfileClient doctorProfileClient)
    {
        _context = context;
        _doctorProfileClient = doctorProfileClient;
    }

    public async Task<DoctorPerformanceSummaryDto> Handle(GetDoctorPerformanceSummaryQuery request, CancellationToken cancellationToken)
    {
        var end = request.EndDate ?? DateTime.UtcNow.Date;
        var start = request.StartDate ?? end.AddDays(-30);

        var appts = await _context.Appointments.AsNoTracking()
            .Where(a => a.ScheduledAt >= start && a.ScheduledAt <= end)
            .ToListAsync(cancellationToken);

        var rates = await _context.Rates.AsNoTracking()
            .Where(r => r.Rated_At >= start && r.Rated_At <= end)
            .ToListAsync(cancellationToken);

        var doctorGroups = appts.GroupBy(a => a.DoctorId).ToList();
        var totalDoctors = doctorGroups.Count;

        decimal avgPerDoctor = totalDoctors > 0 ? (decimal)appts.Count / totalDoctors : 0;

        string topRatedDoctor = "N/A";
        double doctorAverageRating = 0;

        if (rates.Any())
        {
            doctorAverageRating = rates.Average(r => r.Rate_Value ?? 0);

            // Find top rated doctor
            var topDocGroup = rates
                .GroupBy(r => r.Doctor_User_Id)
                .Select(g => new { DoctorId = g.Key, AvgRating = g.Average(r => r.Rate_Value ?? 0), Count = g.Count() })
                .OrderByDescending(x => x.AvgRating)
                .ThenByDescending(x => x.Count)
                .FirstOrDefault();

            if (topDocGroup != null)
            {
                 try 
                 {
                     var profiles = await _doctorProfileClient.GetDoctorProfilesAsync(new[] { topDocGroup.DoctorId }, cancellationToken);
                     if (profiles.Any())
                     {
                         topRatedDoctor = $"{profiles.First().FirstName} {profiles.First().LastName}".Trim();
                     }
                     else
                     {
                         topRatedDoctor = $"Doctor {topDocGroup.DoctorId.ToString()[..8]}";
                     }
                 }
                 catch (Exception)
                 {
                     topRatedDoctor = $"Doctor {topDocGroup.DoctorId.ToString()[..8]}";
                 }
            }
        }
        else if (doctorGroups.Any())
        {
             // Fallback to most active doctor if no ratings
             var topDoc = doctorGroups.OrderByDescending(g => g.Count()).First();
             try
             {
                 var profiles = await _doctorProfileClient.GetDoctorProfilesAsync(new[] { topDoc.Key }, cancellationToken);
                 if (profiles.Any())
                 {
                     topRatedDoctor = $"{profiles.First().FirstName} {profiles.First().LastName}".Trim();
                 }
                 else
                 {
                     topRatedDoctor = $"Doctor {topDoc.Key.ToString()[..8]}";
                 }
             }
             catch
             {
                 topRatedDoctor = $"Doctor {topDoc.Key.ToString()[..8]}";
             }
        }

        return new DoctorPerformanceSummaryDto
        {
            TotalDoctors = totalDoctors,
            AverageAppointmentsPerDoctor = Math.Round(avgPerDoctor, 2),
            TopRatedDoctor = topRatedDoctor,
            DoctorAverageRating = (decimal)Math.Round(doctorAverageRating, 2),
            StartDate = start,
            EndDate = end
        };
    }
}
