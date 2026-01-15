using MediatR;
using Microsoft.EntityFrameworkCore;
using PractitionerService.Data;
using PractitionerService.Models;
using PractitionerService.Features.PerformanceMetrics.DTOs;
using PractitionerService.Features.PerformanceMetrics.Queries;

namespace PractitionerService.Features.PerformanceMetrics.Handlers;

public class GetDoctorPerformanceSummaryHandler : IRequestHandler<GetDoctorPerformanceSummaryQuery, DoctorPerformanceSummaryResponse>
{
	private readonly PractitionerDbContext _context;

	public GetDoctorPerformanceSummaryHandler(PractitionerDbContext context)
	{
		_context = context;
	}

	public async Task<DoctorPerformanceSummaryResponse> Handle(GetDoctorPerformanceSummaryQuery request, CancellationToken cancellationToken)
	{
		var activeDoctors = await _context.Doctors.CountAsync(d => d.IsActive, cancellationToken);
		var allStats = await _context.DoctorStatistics.ToListAsync(cancellationToken);

		var totalAppointments = allStats.Sum(s => s.TotalAppointments);
		var avg = activeDoctors > 0 ? (decimal)totalAppointments / activeDoctors : 0;
		
		var topDocId = allStats.OrderByDescending(s => s.TotalRatingCount > 0 ? (decimal)s.TotalRatingSum / s.TotalRatingCount : 0).FirstOrDefault()?.DoctorId;
		var topDocName = "N/A";
		
		if (topDocId.HasValue)
		{
			var docDir = await _context.Set<DoctorDirectory>().FirstOrDefaultAsync(d => d.DoctorId == topDocId.Value, cancellationToken);
			if (docDir != null && !string.IsNullOrEmpty(docDir.FirstName))
			{
				topDocName = $"Dr. {docDir.FirstName} {docDir.LastName}";
			}
            else
            {
                topDocName = "Top Doctor";
            }
		}

        var totalSum = allStats.Sum(s => s.TotalRatingSum);
        var totalCount = allStats.Sum(s => s.TotalRatingCount);
        var overallAvg = totalCount > 0 ? (decimal)totalSum / totalCount : 0;

		var resp = new DoctorPerformanceSummaryResponse
		{
			TotalDoctors = activeDoctors,
			AverageAppointmentsPerDoctor = Math.Round(avg, 1),
			TopRatedDoctor = topDocName,
			DoctorAverageRating = Math.Round(overallAvg, 2),
			StartDate = request.StartDate,
			EndDate = request.EndDate,
			IsStub = false
		};
		return resp;
	}
}
