using MediatR;
using PatientService.Features.Metrics.DTOs;

namespace PatientService.Features.Metrics.Queries;

public class GetPatientMetricsQuery : IRequest<PatientMetricsResponse>
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
