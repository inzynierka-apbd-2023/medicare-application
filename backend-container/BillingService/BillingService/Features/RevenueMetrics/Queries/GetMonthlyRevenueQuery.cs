using MediatR;
using BillingService.Features.RevenueMetrics.DTOs;

namespace BillingService.Features.RevenueMetrics.Queries;

public class GetMonthlyRevenueQuery : IRequest<MonthlyRevenueResponse>
{
    public int Year { get; set; }
    public int Month { get; set; }
}
