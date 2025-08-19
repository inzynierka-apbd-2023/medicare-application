using AppointmentService.Features.Metrics.DTOs;

namespace AppointmentService.Features.Metrics.Services;

public interface IAppointmentMetricsService
{
    Task<AppointmentMetricsResponse> GetMetricsAsync(DateTime start, DateTime end, CancellationToken ct);
}
