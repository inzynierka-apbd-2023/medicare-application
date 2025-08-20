namespace BillingService.Features.RevenueMetrics.Validators;

public static class YearlyRevenueRequestValidator
{
    /// <summary>
    /// Validates request parameters for yearly revenue endpoint.
    /// </summary>
    /// <param name="year">The year for which to get revenue metrics.</param>
    /// <returns>Collection of validation error messages (empty if valid).</returns>
    public static IEnumerable<string> Validate(int year)
    {
        // Future years not allowed
        var currentYear = DateTime.UtcNow.Year;
        if (year > currentYear)
            yield return "Year cannot be in the future.";
            
        // Don't allow years too far in the past (business rule placeholder: max 10 years)
        var minYear = currentYear - 10;
        if (year < minYear)
            yield return "Year cannot be more than 10 years in the past.";
            
        // Validate year range
        if (year < 2000)
            yield return "Year must be 2000 or later.";
    }
}
