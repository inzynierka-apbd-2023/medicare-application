namespace BillingService.Features.RevenueMetrics.Queries;

/// <summary>
/// Response model for payment types breakdown used by the service layer.
/// </summary>
public class PaymentTypesBreakdownResponse
{
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public decimal TotalRevenue { get; set; }
    public PaymentTypeData AppointmentPayments { get; set; } = new();
    public PaymentTypeData SubscriptionPayments { get; set; } = new();
}

public class PaymentTypeData
{
    public decimal Revenue { get; set; }
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}
