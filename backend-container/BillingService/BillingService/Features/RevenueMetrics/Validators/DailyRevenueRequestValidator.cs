namespace BillingService.Features.RevenueMetrics.Validators;

public static class DailyRevenueRequestValidator
{
    /// <summary>
    /// Validates request parameters for daily revenue endpoint.
    /// </summary>
    /// <param name="date">The date for which to get revenue metrics.</param>
    /// <returns>Collection of validation error messages (empty if valid).</returns>
    public static IEnumerable<string> Validate(DateTime date)
    {
        // Future dates not allowed
        var now = DateTime.UtcNow.Date.AddDays(1); // allow today
        if (date > now)
            yield return "Date cannot be in the future.";
            
        // Don't allow dates too far in the past (business rule placeholder: max 5 years)
        var minDate = DateTime.UtcNow.Date.AddYears(-5);
        if (date < minDate)
            yield return "Date cannot be more than 5 years in the past.";
    }
}
