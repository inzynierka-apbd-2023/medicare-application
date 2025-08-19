using MediatR;
using AppointmentService.Features.Metrics.DTOs;
using AppointmentService.Features.Metrics.Queries;
using AppointmentService.Features.Metrics.Services;

namespace AppointmentService.Features.Metrics.Handlers;

public class GetAppointmentMetricsHandler : IRequestHandler<GetAppointmentMetricsQuery, AppointmentMetricsResponse>
{
    private readonly IAppointmentMetricsService _service;
    public GetAppointmentMetricsHandler(IAppointmentMetricsService service) => _service = service;

    public async Task<AppointmentMetricsResponse> Handle(GetAppointmentMetricsQuery request, CancellationToken cancellationToken)
    {
        var end = request.EndDate ?? DateTime.UtcNow.Date;
        var start = request.StartDate ?? end.AddDays(-30);
        return await _service.GetMetricsAsync(start, end, cancellationToken);
    }
}
