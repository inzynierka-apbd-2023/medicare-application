using MediatR;
using AppointmentService.Features.Analytics.DTOs;
using AppointmentService.Features.Analytics.Queries;
using AppointmentService.Features.Metrics.Services;

namespace AppointmentService.Features.Analytics.Handlers;

public class GetAppointmentMetricsHandler : IRequestHandler<GetAppointmentMetricsQuery, IEnumerable<AppointmentMetricDto>>
{
    private readonly IAppointmentMetricsService _metricsService;

    public GetAppointmentMetricsHandler(IAppointmentMetricsService metricsService)
    {
        _metricsService = metricsService;
    }

    public async Task<IEnumerable<AppointmentMetricDto>> Handle(GetAppointmentMetricsQuery request, CancellationToken cancellationToken)
    {
        var end = request.EndDate ?? DateTime.UtcNow;
        var start = request.StartDate ?? end.AddDays(-30);
        
        var duration = end - start;
        var prevStart = start - duration;
        var prevEnd = start;

        var current = await _metricsService.GetMetricsAsync(start, end, cancellationToken);
        var previous = await _metricsService.GetMetricsAsync(prevStart, prevEnd, cancellationToken);
        
        var metrics = new List<AppointmentMetricDto>
        {
            new() {
                 Id = Guid.NewGuid(),
                 Title = "Total Appointments",
                 Value = current.TotalAppointments,
                 Change = CalculateChange(current.TotalAppointments, previous.TotalAppointments),
                 Period = "vs last period",
                 Icon = "calendar"
            },
            new() {
                 Id = Guid.NewGuid(),
                 Title = "Completion Rate",
                 Value = (int)current.CompletionRate,
                 Change = CalculateChange((double)current.CompletionRate, (double)previous.CompletionRate),
                 Period = "vs last period",
                 Icon = "activity" 
            },
            new() {
                 Id = Guid.NewGuid(),
                 Title = "Total Revenue",
                 Value = (int)current.TotalRevenue,
                 Change = CalculateChange((double)current.TotalRevenue, (double)previous.TotalRevenue),
                 Period = "vs last period",
                 Icon = "dollar-sign"
            },
             new() {
                 Id = Guid.NewGuid(),
                 Title = "Active Patients",
                 Value = current.UniquePatientsInPeriod,
                 Change = CalculateChange(current.UniquePatientsInPeriod, previous.UniquePatientsInPeriod),
                 Period = "vs last period",
                 Icon = "users"
            }
        };

        return metrics;
    }

    private double CalculateChange(double current, double previous)
    {
        if (previous == 0) return current > 0 ? 100 : 0;
        return Math.Round(((current - previous) / previous) * 100, 1);
    }
}
