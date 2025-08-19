using System;
using System.Collections.Generic;

namespace AppointmentService.Features.Metrics.Validators;

public static class AppointmentMetricsRequestValidator
{
    public static IEnumerable<string> Validate(DateTime? startDate, DateTime? endDate)
    {
        if (startDate.HasValue && endDate.HasValue && startDate > endDate)
            yield return "StartDate cannot be after EndDate.";
        if (startDate.HasValue && endDate.HasValue && (endDate.Value - startDate.Value).TotalDays > 365)
            yield return "Date range cannot exceed 365 days.";
        var today = DateTime.UtcNow.Date.AddDays(1);
        if (startDate.HasValue && startDate.Value > today)
            yield return "StartDate cannot be in the future.";
        if (endDate.HasValue && endDate.Value > today)
            yield return "EndDate cannot be in the future.";
    }
}
