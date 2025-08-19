using System;
using System.Collections.Generic;

namespace PatientService.Features.Metrics.Validators;

public static class PatientMetricsRequestValidator
{
    /// <summary>
    /// Validates request parameters for patient metrics endpoint.
    /// </summary>
    /// <param name="startDate">Optional start date.</param>
    /// <param name="endDate">Optional end date.</param>
    /// <returns>Collection of validation error messages (empty if valid).</returns>
    public static IEnumerable<string> Validate(DateTime? startDate, DateTime? endDate)
    {
        if (startDate.HasValue && endDate.HasValue && startDate > endDate)
            yield return "StartDate cannot be after EndDate.";

        // Guard against excessively large range (business rule placeholder: max 365 days)
        if (startDate.HasValue && endDate.HasValue && (endDate.Value - startDate.Value).TotalDays > 365)
            yield return "Date range cannot exceed 365 days.";

        // Future dates not allowed
        var now = DateTime.UtcNow.Date.AddDays(1); // allow today
        if (startDate.HasValue && startDate.Value > now)
            yield return "StartDate cannot be in the future.";
        if (endDate.HasValue && endDate.Value > now)
            yield return "EndDate cannot be in the future.";
    }
}
