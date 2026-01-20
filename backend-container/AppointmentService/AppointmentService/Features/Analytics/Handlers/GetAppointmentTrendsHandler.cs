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
    private readonly MassTransit.IRequestClient<Medicare.Messaging.Contracts.IGetAppointmentPayments> _paymentClient;

    public GetAppointmentTrendsHandler(AppointmentDbContext context, MassTransit.IRequestClient<Medicare.Messaging.Contracts.IGetAppointmentPayments> paymentClient)
    {
        _context = context;
        _paymentClient = paymentClient;
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
        
        var appointmentIds = appointments.Select(a => a.Id).ToList();
        var payments = new List<AppointmentPaymentDto>();

        if (appointmentIds.Any())
        {

            var response = await _paymentClient.GetResponse<Medicare.Messaging.Contracts.IAppointmentPayments>(new { AppointmentIds = appointmentIds }, cancellationToken);
            payments = response.Message.Payments.Select(p => new AppointmentPaymentDto 
            { 
                AppointmentId = p.AppointmentId, 
                AmountCents = (int)p.AmountCents, 
                Status = p.Status 
            }).ToList();
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
