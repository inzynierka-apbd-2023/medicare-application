using PatientService.Features.Metrics.DTOs;

namespace PatientService.Features.Metrics.Services;

public interface IPatientMetricsService
{
    Task<PatientMetricsResponse> GetMetricsAsync(DateTime startDate, DateTime endDate, CancellationToken ct);
}
