using MediatR;
using PatientService.Features.Metrics.DTOs;
using PatientService.Features.Metrics.Queries;
using PatientService.Features.Metrics.Services;

namespace PatientService.Features.Metrics.Handlers;

public class GetPatientMetricsHandler : IRequestHandler<GetPatientMetricsQuery, PatientMetricsResponse>
{
    private readonly IPatientMetricsService _service;
    public GetPatientMetricsHandler(IPatientMetricsService service) => _service = service;

    public async Task<PatientMetricsResponse> Handle(GetPatientMetricsQuery request, CancellationToken cancellationToken)
    {
        var end = request.EndDate ?? DateTime.UtcNow.Date;
        var start = request.StartDate ?? end.AddDays(-30);
        return await _service.GetMetricsAsync(start, end, cancellationToken);
    }
}
