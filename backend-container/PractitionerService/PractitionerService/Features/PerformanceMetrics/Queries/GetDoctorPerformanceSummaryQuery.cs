using MediatR;
using PractitionerService.Features.PerformanceMetrics.DTOs;

namespace PractitionerService.Features.PerformanceMetrics.Queries;

public class GetDoctorPerformanceSummaryQuery : IRequest<DoctorPerformanceSummaryResponse>
{
	public DateTime? StartDate { get; set; }
	public DateTime? EndDate { get; set; }
}
