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

        // Use the actual Appointments table
        var query = _context.Appointments
            .Where(a => a.ScheduledAt >= startDate && a.ScheduledAt <= endDate);

        if (request.DoctorId.HasValue)
            query = query.Where(a => a.DoctorId == request.DoctorId);

        var appointments = await query.ToListAsync(cancellationToken);

        var trends = new List<TrendDataDto>();

        for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
        {
            var nextDate = date.AddDays(1);
            var dayAppointments = appointments.Where(a => a.ScheduledAt >= date && a.ScheduledAt < nextDate).ToList();

            trends.Add(new TrendDataDto
            {
                Date = date.ToString("yyyy-MM-dd"),
                Appointments = dayAppointments.Count,
                Completed = dayAppointments.Count(a => a.Status == "Completed"),
                Cancelled = dayAppointments.Count(a => a.Status == "Cancelled"),
                NoShow = dayAppointments.Count(a => a.Status == "NoShow" || a.Status == "Overdue"),
                Revenue = 0 // Revenue not available in Appointments table
            });
        }

        return trends;
    }
}
