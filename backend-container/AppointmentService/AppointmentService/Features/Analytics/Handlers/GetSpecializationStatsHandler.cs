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

        var specializations = await _context.Specializations
            .Where(s => s.Is_Active)
            .ToListAsync(cancellationToken);

        var stats = new List<SpecializationStatsDto>();

        foreach (var specialization in specializations)
        {
            var doctorIds = await _context.DoctorSpecializations
                .Where(ds => ds.Specialization_Id == specialization.Id)
                .Select(ds => ds.Doctor_Id)
                .ToListAsync(cancellationToken);

            if (!doctorIds.Any())
                continue;

            var appointments = await _context.ScheduleAppointments
                .Where(sa => doctorIds.Contains(sa.Doctor_User_Id))
                .Where(sa => sa.Day >= startDate && sa.Day <= endDate)
                .ToListAsync(cancellationToken);

            var totalAppointments = appointments.Count;

            var completedAppointments = await _context.ScheduleAppointments
                .Join(_context.ScheduleAppointmentStatuses, sa => sa.Schedule_Appointment_Status_Id, status => status.Id,
                    (sa, status) => new { sa, status })
                .Where(x => doctorIds.Contains(x.sa.Doctor_User_Id))
                .Where(x => x.sa.Day >= startDate && x.sa.Day <= endDate && x.status.Name == "Completed")
                .CountAsync(cancellationToken);

            var totalPatients = await _context.ScheduleAppointments
                .Where(sa => doctorIds.Contains(sa.Doctor_User_Id))
                .Where(sa => sa.Day >= startDate && sa.Day <= endDate)
                .Select(sa => sa.Patient_User_Id)
                .Distinct()
                .CountAsync(cancellationToken);

            var totalDoctors = doctorIds.Count;

            var averageDuration = appointments.Any() ? appointments.Average(a => a.Duration_Minutes ?? 0) : 0;

            var revenue = await _context.AppointmentPayments
                .Join(_context.ScheduleAppointments, ap => ap.Schedule_Appointment_Id, sa => sa.Id,
                    (ap, sa) => new { ap, sa })
                .Where(x => doctorIds.Contains(x.sa.Doctor_User_Id))
                .Where(x => x.sa.Day >= startDate && x.sa.Day <= endDate && x.ap.Status == "Paid")
                .SumAsync(x => x.ap.Amount ?? 0m, cancellationToken);

            var completionRate = totalAppointments > 0 ? ((double)completedAppointments / totalAppointments) * 100 : 0;

            var averageRating = await _context.Rates
                .Where(r => doctorIds.Contains(r.Doctor_User_Id))
                .Where(r => r.Rated_At >= startDate && r.Rated_At <= endDate)
                .AverageAsync(r => (double?)r.Rate_Value, cancellationToken) ?? 0;

            stats.Add(new SpecializationStatsDto
            {
                Specialization = specialization.Name,
                TotalAppointments = totalAppointments,
                TotalPatients = totalPatients,
                TotalDoctors = totalDoctors,
                AverageAppointmentDuration = averageDuration,
                Revenue = revenue,
                CompletionRate = completionRate,
                AverageRating = averageRating
            });
        }

        return stats.OrderByDescending(s => s.TotalAppointments);
    }
}
