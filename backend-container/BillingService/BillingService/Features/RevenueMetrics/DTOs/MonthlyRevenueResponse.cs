namespace BillingService.Features.RevenueMetrics.DTOs;

/// <summary>
/// Response model for monthly revenue metrics used by Owner dashboard.
/// </summary>
public class MonthlyRevenueResponse
{
    /// <summary>Year and month for which metrics are calculated.</summary>
    public int Year { get; set; }
    public int Month { get; set; }
    
    /// <summary>Total revenue for the specified month.</summary>
    public decimal TotalRevenue { get; set; }
    
    /// <summary>Revenue from appointment payments.</summary>
    public decimal AppointmentRevenue { get; set; }
    
    /// <summary>Revenue from subscription payments.</summary>
    public decimal SubscriptionRevenue { get; set; }
    
    /// <summary>Number of transactions processed in the specified month.</summary>
    public int TransactionCount { get; set; }
    
    /// <summary>Daily breakdown of revenue for the month.</summary>
    public List<DailyRevenueItem> DailyBreakdown { get; set; } = new();
}

public class DailyRevenueItem
{
    public int Day { get; set; }
    public decimal Revenue { get; set; }
    public int TransactionCount { get; set; }
}
