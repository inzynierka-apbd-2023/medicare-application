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
        try 
        {
            return await _service.GetMetricsAsync(start, end, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MetricsError] Failed to fetch patient metrics: {ex.Message}");
            return new PatientMetricsResponse 
            {
                StartDate = start,
                EndDate = end,
                TotalActivePatients = 0,
                NewPatients = 0,
                RetentionRate = 0,
                AverageRating = 0,
                TotalRatings = 0,
                IsStub = true
            };
        }
    }
}
