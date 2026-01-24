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
    private readonly MassTransit.IRequestClient<Medicare.Messaging.Contracts.IGetDoctors> _doctorClient;
    private readonly MassTransit.IRequestClient<Medicare.Messaging.Contracts.IGetAppointmentRatings> _ratingsClient;

    public GetDoctorPerformanceSummaryHandler(
        AppointmentDbContext context, 
        MassTransit.IRequestClient<Medicare.Messaging.Contracts.IGetDoctors> doctorClient,
        MassTransit.IRequestClient<Medicare.Messaging.Contracts.IGetAppointmentRatings> ratingsClient)
    {
        _context = context;
        _doctorClient = doctorClient;
        _ratingsClient = ratingsClient;
    }

    public async Task<DoctorPerformanceSummaryDto> Handle(GetDoctorPerformanceSummaryQuery request, CancellationToken cancellationToken)
    {
        var end = request.EndDate ?? DateTime.UtcNow.Date;
        var start = request.StartDate ?? end.AddDays(-30);

        var appts = await _context.Appointments.AsNoTracking()
            .Where(a => a.ScheduledAt >= start && a.ScheduledAt <= end)
            .ToListAsync(cancellationToken);

        var appointmentIds = appts.Select(a => a.Id).ToList();
        
        // Get ratings via RabbitMQ from PractitionerService
        var ratings = new List<RatingDto>();
        if (appointmentIds.Any())
        {
            var ratingsResponse = await _ratingsClient.GetResponse<Medicare.Messaging.Contracts.IAppointmentRatings>(new { AppointmentIds = appointmentIds }, cancellationToken);
            ratings = ratingsResponse.Message.Ratings.Select(r => new RatingDto
            {
                AppointmentId = r.AppointmentId,
                RateValue = r.RateValue
            }).ToList();
        }

        var doctorGroups = appts.GroupBy(a => a.DoctorId).ToList();
        var totalDoctors = doctorGroups.Count;

        decimal avgPerDoctor = totalDoctors > 0 ? (decimal)appts.Count / totalDoctors : 0;

        string topRatedDoctor = "N/A";
        double doctorAverageRating = 0;

        if (ratings.Any())
        {
            doctorAverageRating = ratings.Average(r => r.RateValue);

            // Find top rated doctor by grouping ratings by appointment -> doctor
            var apptToDoctorMap = appts.ToDictionary(a => a.Id, a => a.DoctorId);
            var doctorRatings = ratings
                .Where(r => apptToDoctorMap.ContainsKey(r.AppointmentId))
                .GroupBy(r => apptToDoctorMap[r.AppointmentId])
                .Select(g => new { DoctorId = g.Key, AvgRating = g.Average(r => r.RateValue), Count = g.Count() })
                .OrderByDescending(x => x.AvgRating)
                .ThenByDescending(x => x.Count)
                .FirstOrDefault();

            if (doctorRatings != null)
            {
                var resp = await _doctorClient.GetResponse<Medicare.Messaging.Contracts.IDoctorProfiles>(new { DoctorIds = new[] { doctorRatings.DoctorId } }, cancellationToken);
                var profiles = resp.Message.Profiles;
                if (profiles.Any())
                {
                    topRatedDoctor = $"{profiles.First().FirstName} {profiles.First().LastName}".Trim();
                }
                else
                {
                    topRatedDoctor = $"Doctor {doctorRatings.DoctorId.ToString()[..8]}";
                }
            }
        }
        else if (doctorGroups.Any())
        {
             var topDoc = doctorGroups.OrderByDescending(g => g.Count()).First();
             var resp = await _doctorClient.GetResponse<Medicare.Messaging.Contracts.IDoctorProfiles>(new { DoctorIds = new[] { topDoc.Key } }, cancellationToken);
             var profiles = resp.Message.Profiles;
             if (profiles.Any())
             {
                 topRatedDoctor = $"{profiles.First().FirstName} {profiles.First().LastName}".Trim();
             }
             else
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

