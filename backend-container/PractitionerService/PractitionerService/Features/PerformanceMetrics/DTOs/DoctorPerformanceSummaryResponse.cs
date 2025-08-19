namespace PractitionerService.Features.PerformanceMetrics.DTOs;

/// <summary>
/// Summary metrics for doctor performance displayed on Owner dashboard.
/// Logic/population intentionally stubbed – values will be populated by future implementation.
/// </summary>
public class DoctorPerformanceSummaryResponse
{
	public int TotalDoctors { get; set; }
	public decimal AverageAppointmentsPerDoctor { get; set; }
	public string TopRatedDoctor { get; set; } = "N/A";
	public decimal DoctorAverageRating { get; set; }
	public DateTime? StartDate { get; set; }
	public DateTime? EndDate { get; set; }
	public bool IsStub { get; set; } = true;
}
