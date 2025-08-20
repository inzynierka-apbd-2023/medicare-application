using BillingService.Features.RevenueMetrics.Queries;
using BillingService.Features.RevenueMetrics.DTOs;

namespace BillingService.Services;

public interface IRevenueMetricsService
{
    Task<DailyRevenueResponse> GetDailyRevenueAsync(DateOnly date, CancellationToken cancellationToken = default);
    Task<MonthlyRevenueResponse> GetMonthlyRevenueAsync(int year, int month, CancellationToken cancellationToken = default);
    Task<YearlyRevenueResponse> GetYearlyRevenueAsync(int year, CancellationToken cancellationToken = default);
    Task<PaymentTypesBreakdownResponse> GetPaymentTypesBreakdownAsync(DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default);
}
