using MediatR;
using PractitionerService.Features.PerformanceMetrics.DTOs;
using PractitionerService.Features.PerformanceMetrics.Queries;

namespace PractitionerService.Features.PerformanceMetrics.Handlers;

public class GetDoctorPerformanceSummaryHandler : IRequestHandler<GetDoctorPerformanceSummaryQuery, DoctorPerformanceSummaryResponse>
{
	public Task<DoctorPerformanceSummaryResponse> Handle(GetDoctorPerformanceSummaryQuery request, CancellationToken cancellationToken)
	{
		var resp = new DoctorPerformanceSummaryResponse
		{
			TotalDoctors = 0,
			AverageAppointmentsPerDoctor = 0,
			TopRatedDoctor = "N/A",
			DoctorAverageRating = 0,
			StartDate = request.StartDate,
			EndDate = request.EndDate,
			IsStub = true
		};
		return Task.FromResult(resp);
	}
}
