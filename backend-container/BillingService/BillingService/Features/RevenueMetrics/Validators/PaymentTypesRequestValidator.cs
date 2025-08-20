namespace BillingService.Features.RevenueMetrics.Validators;

public static class PaymentTypesRequestValidator
{
    /// <summary>
    /// Validates request parameters for payment types endpoint.
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
            
        // Don't allow dates too far in the past (business rule placeholder: max 5 years)
        var minDate = DateTime.UtcNow.Date.AddYears(-5);
        if (startDate.HasValue && startDate.Value < minDate)
            yield return "StartDate cannot be more than 5 years in the past.";
        if (endDate.HasValue && endDate.Value < minDate)
            yield return "EndDate cannot be more than 5 years in the past.";
    }
}
