namespace BillingService.Features.RevenueMetrics.DTOs;

/// <summary>
/// Response model for payment types breakdown used by Owner dashboard.
/// </summary>
public class PaymentTypesResponse
{
    /// <summary>Breakdown of payments by payment type.</summary>
    public List<PaymentTypeBreakdown> PaymentTypes { get; set; } = new();
    
    /// <summary>Total revenue across all payment types.</summary>
    public decimal TotalRevenue { get; set; }
    
    /// <summary>Total number of payments across all types.</summary>
    public int TotalPaymentCount { get; set; }
    
    /// <summary>Date range for which metrics are calculated.</summary>
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
}

public class PaymentTypeBreakdown
{
    public string PaymentType { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int PaymentCount { get; set; }
    public decimal Percentage { get; set; }
}
