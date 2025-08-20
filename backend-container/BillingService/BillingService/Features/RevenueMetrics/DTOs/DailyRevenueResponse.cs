namespace BillingService.Features.RevenueMetrics.DTOs;

/// <summary>
/// Response model for daily revenue metrics used by Owner dashboard.
/// </summary>
public class DailyRevenueResponse
{
    /// <summary>Date for which metrics are calculated.</summary>
    public DateOnly Date { get; set; }
    
    /// <summary>Total revenue for the specified day.</summary>
    public decimal TotalRevenue { get; set; }
    
    /// <summary>Revenue from appointment payments.</summary>
    public decimal AppointmentRevenue { get; set; }
    
    /// <summary>Revenue from subscription payments.</summary>
    public decimal SubscriptionRevenue { get; set; }
    
    /// <summary>Number of transactions processed on the specified day.</summary>
    public int TransactionCount { get; set; }
}
