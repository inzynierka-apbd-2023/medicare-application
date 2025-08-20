using MediatR;
using BillingService.Features.RevenueMetrics.DTOs;

namespace BillingService.Features.RevenueMetrics.Queries;

public class GetDailyRevenueQuery : IRequest<DailyRevenueResponse>
{
    public DateOnly Date { get; set; }
}
