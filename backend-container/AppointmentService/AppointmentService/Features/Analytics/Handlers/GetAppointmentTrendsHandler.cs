using MediatR;
using Microsoft.EntityFrameworkCore;
using AppointmentService.Data;
using AppointmentService.Features.Analytics.DTOs;
using AppointmentService.Features.Analytics.Queries;

namespace AppointmentService.Features.Analytics.Handlers;

public class GetAppointmentTrendsHandler : IRequestHandler<GetAppointmentTrendsQuery, IEnumerable<TrendDataDto>>
{
    private readonly AppointmentDbContext _context;

    public GetAppointmentTrendsHandler(AppointmentDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TrendDataDto>> Handle(GetAppointmentTrendsQuery request, CancellationToken cancellationToken)
    {
        var endDate = request.EndDate ?? DateTime.UtcNow;
        var startDate = request.StartDate ?? endDate.AddDays(-request.Days);
        
        var trends = new List<TrendDataDto>();
        
        for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
        {
            var nextDate = date.AddDays(1);
            
            var appointments = await _context.ScheduleAppointments
                .Where(sa => sa.Day >= date && sa.Day < nextDate)
                .Where(sa => !request.DoctorId.HasValue || sa.Doctor_User_Id == request.DoctorId)
                .CountAsync(cancellationToken);

            var completed = await _context.ScheduleAppointments
                .Join(_context.ScheduleAppointmentStatuses, sa => sa.Schedule_Appointment_Status_Id, status => status.Id,
                    (sa, status) => new { sa, status })
                .Where(x => x.sa.Day >= date && x.sa.Day < nextDate && x.status.Name == "Completed")
                .Where(x => !request.DoctorId.HasValue || x.sa.Doctor_User_Id == request.DoctorId)
                .CountAsync(cancellationToken);

            var cancelled = await _context.ScheduleAppointments
                .Join(_context.ScheduleAppointmentStatuses, sa => sa.Schedule_Appointment_Status_Id, status => status.Id,
                    (sa, status) => new { sa, status })
                .Where(x => x.sa.Day >= date && x.sa.Day < nextDate && x.status.Name == "Cancelled")
                .Where(x => !request.DoctorId.HasValue || x.sa.Doctor_User_Id == request.DoctorId)
                .CountAsync(cancellationToken);

            var noShow = await _context.ScheduleAppointments
                .Join(_context.ScheduleAppointmentStatuses, sa => sa.Schedule_Appointment_Status_Id, status => status.Id,
                    (sa, status) => new { sa, status })
                .Where(x => x.sa.Day >= date && x.sa.Day < nextDate && x.status.Name == "No Show")
                .Where(x => !request.DoctorId.HasValue || x.sa.Doctor_User_Id == request.DoctorId)
                .CountAsync(cancellationToken);

            var revenue = await _context.AppointmentPayments
                .Join(_context.ScheduleAppointments, ap => ap.Schedule_Appointment_Id, sa => sa.Id,
                    (ap, sa) => new { ap, sa })
                .Where(x => x.sa.Day >= date && x.sa.Day < nextDate && x.ap.Status == "Paid")
                .Where(x => !request.DoctorId.HasValue || x.sa.Doctor_User_Id == request.DoctorId)
                .SumAsync(x => x.ap.Amount ?? 0m, cancellationToken);

            trends.Add(new TrendDataDto
            {
                Date = date.ToString("yyyy-MM-dd"),
                Appointments = appointments,
                Completed = completed,
                Cancelled = cancelled,
                NoShow = noShow,
                Revenue = revenue
            });
        }

        return trends.OrderBy(t => t.Date);
    }
}
