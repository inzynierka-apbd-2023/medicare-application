namespace BillingService.Features.RevenueMetrics.DTOs;

/// <summary>
/// Response model for yearly revenue metrics used by Owner dashboard.
/// </summary>
public class YearlyRevenueResponse
{
    /// <summary>Year for which metrics are calculated.</summary>
    public int Year { get; set; }
    
    /// <summary>Total revenue for the specified year.</summary>
    public decimal TotalRevenue { get; set; }
    
    /// <summary>Revenue from appointment payments.</summary>
    public decimal AppointmentRevenue { get; set; }
    
    /// <summary>Revenue from subscription payments.</summary>
    public decimal SubscriptionRevenue { get; set; }
    
    /// <summary>Number of transactions processed in the specified year.</summary>
    public int TransactionCount { get; set; }
    
    /// <summary>Monthly breakdown of revenue for the year.</summary>
    public List<MonthlyRevenueItem> MonthlyBreakdown { get; set; } = new();
}

public class MonthlyRevenueItem
{
    public int Month { get; set; }
    public decimal Revenue { get; set; }
    public int TransactionCount { get; set; }
}
