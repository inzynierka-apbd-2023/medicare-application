namespace BillingService.Features.RevenueMetrics.Validators;

public static class MonthlyRevenueRequestValidator
{
    /// <summary>
    /// Validates request parameters for monthly revenue endpoint.
    /// </summary>
    /// <param name="year">The year for which to get revenue metrics.</param>
    /// <param name="month">The month for which to get revenue metrics.</param>
    /// <returns>Collection of validation error messages (empty if valid).</returns>
    public static IEnumerable<string> Validate(int year, int month)
    {
        // Validate month range
        if (month < 1 || month > 12)
            yield return "Month must be between 1 and 12.";
            
        // Future dates not allowed
        var now = DateTime.UtcNow;
        var requestedDate = new DateTime(year, month, 1);
        if (requestedDate > new DateTime(now.Year, now.Month, 1).AddMonths(1))
            yield return "Month cannot be in the future.";
            
        // Don't allow dates too far in the past (business rule placeholder: max 10 years)
        var minDate = DateTime.UtcNow.AddYears(-10);
        if (requestedDate < new DateTime(minDate.Year, minDate.Month, 1))
            yield return "Month cannot be more than 10 years in the past.";
            
        // Validate year range
        if (year < 2000 || year > DateTime.UtcNow.Year + 1)
            yield return "Year must be between 2000 and current year.";
    }
}
