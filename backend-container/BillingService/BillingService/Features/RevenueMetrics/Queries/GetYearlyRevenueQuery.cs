using MediatR;
using BillingService.Features.RevenueMetrics.DTOs;

namespace BillingService.Features.RevenueMetrics.Queries;

public class GetYearlyRevenueQuery : IRequest<YearlyRevenueResponse>
{
    public int Year { get; set; }
}
