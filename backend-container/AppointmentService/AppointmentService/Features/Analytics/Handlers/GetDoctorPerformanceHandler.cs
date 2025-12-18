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

        var doctorsQuery = _context.Doctors.AsQueryable();

        if (!string.IsNullOrEmpty(request.DoctorId))
            doctorsQuery = doctorsQuery.Where(d => d.Id == request.DoctorId);

        if (!string.IsNullOrEmpty(request.Specialization))
        {
            doctorsQuery = doctorsQuery
                .Join(_context.DoctorSpecializations, d => d.Id, ds => ds.Doctor_Id, (d, ds) => new { d, ds })
                .Join(_context.Specializations, x => x.ds.Specialization_Id, s => s.Id, (x, s) => new { x.d, s })
                .Where(x => x.s.Name == request.Specialization)
                .Select(x => x.d);
        }

        var doctors = await doctorsQuery.ToListAsync(cancellationToken);
        var performanceList = new List<DoctorPerformanceDto>();

        foreach (var doctor in doctors)
        {
            var doctorProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(up => up.User_Id == doctor.Id, cancellationToken);

            var appointments = await _context.ScheduleAppointments
                .Where(sa => sa.Doctor_User_Id == doctor.Id)
                .Where(sa => sa.Day >= startDate && sa.Day <= endDate)
                .ToListAsync(cancellationToken);

            var totalAppointments = appointments.Count;

            var completedAppointments = await _context.ScheduleAppointments
                .Join(_context.ScheduleAppointmentStatuses, sa => sa.Schedule_Appointment_Status_Id, status => status.Id,
                    (sa, status) => new { sa, status })
                .Where(x => x.sa.Doctor_User_Id == doctor.Id)
                .Where(x => x.sa.Day >= startDate && x.sa.Day <= endDate && x.status.Name == "Completed")
                .CountAsync(cancellationToken);

            var cancelledAppointments = await _context.ScheduleAppointments
                .Join(_context.ScheduleAppointmentStatuses, sa => sa.Schedule_Appointment_Status_Id, status => status.Id,
                    (sa, status) => new { sa, status })
                .Where(x => x.sa.Doctor_User_Id == doctor.Id)
                .Where(x => x.sa.Day >= startDate && x.sa.Day <= endDate && x.status.Name == "Cancelled")
                .CountAsync(cancellationToken);

            var noShowAppointments = await _context.ScheduleAppointments
                .Join(_context.ScheduleAppointmentStatuses, sa => sa.Schedule_Appointment_Status_Id, status => status.Id,
                    (sa, status) => new { sa, status })
                .Where(x => x.sa.Doctor_User_Id == doctor.Id)
                .Where(x => x.sa.Day >= startDate && x.sa.Day <= endDate && x.status.Name == "NoShow")
                .CountAsync(cancellationToken);

            var ratings = await _context.Rates
                .Where(r => r.Doctor_User_Id == doctor.Id)
                .Where(r => r.Rated_At >= startDate && r.Rated_At <= endDate)
                .ToListAsync(cancellationToken);

            var averageRating = ratings.Any() ? (ratings.Average(r => (double?)r.Rate_Value) ?? 0) : 0;
            var totalRatings = ratings.Count;

            var revenue = await _context.AppointmentPayments
                .Join(_context.ScheduleAppointments, ap => ap.Schedule_Appointment_Id, sa => sa.Id,
                    (ap, sa) => new { ap, sa })
                .Where(x => x.sa.Doctor_User_Id == doctor.Id)
                .Where(x => x.sa.Day >= startDate && x.sa.Day <= endDate && x.ap.Status == "Paid")
                .SumAsync(x => x.ap.Amount ?? 0m, cancellationToken);

            // Calculate utilization rate based on total working hours vs booked appointments
            var totalWorkingMinutes = (endDate - startDate).Days * 8 * 60; // Assuming 8 hours per day
            var bookedMinutes = appointments.Sum(a => a.Duration_Minutes ?? 0);
            var utilizationRate = totalWorkingMinutes > 0 ? ((double)bookedMinutes / totalWorkingMinutes) * 100 : 0;

            var specialization = await _context.DoctorSpecializations
                .Join(_context.Specializations, ds => ds.Specialization_Id, s => s.Id, (ds, s) => new { ds, s })
                .Where(x => x.ds.Doctor_Id == doctor.Id)
                .Select(x => x.s.Name)
                .FirstOrDefaultAsync(cancellationToken) ?? "General";

            performanceList.Add(new DoctorPerformanceDto
            {
                Id = doctor.Id,
                Name = doctorProfile != null ? $"Dr. {doctorProfile.FirstName ?? "Unknown"} {doctorProfile.LastName ?? ""}" : "Unknown",
                Specialization = specialization,
                TotalAppointments = totalAppointments,
                CompletedAppointments = completedAppointments,
                CancelledAppointments = cancelledAppointments,
                NoShowAppointments = noShowAppointments,
                AverageRating = averageRating,
                TotalRatings = totalRatings,
                Revenue = revenue,
                UtilizationRate = utilizationRate
            });
        }

        return performanceList.OrderByDescending(p => p.TotalAppointments);
    }
}
