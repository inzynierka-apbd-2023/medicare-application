using MediatR;
using Microsoft.EntityFrameworkCore;
using AppointmentService.Data;
using AppointmentService.Features.Analytics.DTOs;
using AppointmentService.Features.Analytics.Queries;
using AppointmentService.Models;

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

        var query = _context.Appointments.AsNoTracking()
            .Where(a => a.ScheduledAt >= startDate && a.ScheduledAt <= endDate);

        if (request.DoctorId.HasValue)
            query = query.Where(a => a.DoctorId == request.DoctorId);

        var appointments = await query.ToListAsync(cancellationToken);
        
        // Fetch revenue data with fallback
        var appointmentIds = appointments.Select(a => a.Id).ToList();
        var payments = new List<AppointmentPayment>();

        if (appointmentIds.Any())
        {
            payments = await _context.AppointmentPayments.AsNoTracking()
                .Where(p => appointmentIds.Contains(p.AppointmentId))
                .ToListAsync(cancellationToken);
        }

        var trends = new List<TrendDataDto>();

        for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
        {
            var nextDate = date.AddDays(1);
            var dayAppointments = appointments.Where(a => a.ScheduledAt >= date && a.ScheduledAt < nextDate).ToList();
            var dayApptIds = dayAppointments.Select(a => a.Id).ToHashSet();
            
            var dayRevenue = payments
                .Where(p => dayApptIds.Contains(p.AppointmentId))
                .Sum(p => p.AmountCents) / 100.0m;

            trends.Add(new TrendDataDto
            {
                Date = date.ToString("yyyy-MM-dd"),
                Appointments = dayAppointments.Count,
                Completed = dayAppointments.Count(a => string.Equals(a.Status, "Completed", StringComparison.OrdinalIgnoreCase)),
                Cancelled = dayAppointments.Count(a => string.Equals(a.Status, "Cancelled", StringComparison.OrdinalIgnoreCase)),
                NoShow = dayAppointments.Count(a => string.Equals(a.Status, "NoShow", StringComparison.OrdinalIgnoreCase) || string.Equals(a.Status, "Overdue", StringComparison.OrdinalIgnoreCase)),
                Revenue = dayRevenue
            });
        }

        return trends;
    }
}
