namespace PractitionerService.Features.PerformanceMetrics.Validators;

public static class DoctorPerformanceRequestValidator
{
	public static IEnumerable<string> Validate(DateTime? startDate, DateTime? endDate)
	{
		if (startDate.HasValue && endDate.HasValue && startDate > endDate)
			yield return "StartDate cannot be after EndDate.";

		if (startDate.HasValue && endDate.HasValue && (endDate.Value - startDate.Value).TotalDays > 365)
			yield return "Date range cannot exceed 365 days.";

		var now = DateTime.UtcNow.Date.AddDays(1);
		if (startDate.HasValue && startDate.Value > now)
			yield return "StartDate cannot be in the future.";
		if (endDate.HasValue && endDate.Value > now)
			yield return "EndDate cannot be in the future.";
	}
}
