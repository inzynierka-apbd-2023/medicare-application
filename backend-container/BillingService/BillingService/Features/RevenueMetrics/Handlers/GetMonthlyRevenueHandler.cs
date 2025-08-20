using MediatR;
using BillingService.Features.RevenueMetrics.DTOs;
using BillingService.Features.RevenueMetrics.Queries;
using BillingService.Services;
using System.Globalization;

namespace BillingService.Features.RevenueMetrics.Handlers;

public class GetMonthlyRevenueHandler : IRequestHandler<GetMonthlyRevenueQuery, MonthlyRevenueResponse>
{
    private readonly IRevenueMetricsService _revenueMetricsService;
    private readonly ILogger<GetMonthlyRevenueHandler> _logger;

    public GetMonthlyRevenueHandler(IRevenueMetricsService revenueMetricsService, ILogger<GetMonthlyRevenueHandler> logger)
    {
        _revenueMetricsService = revenueMetricsService;
        _logger = logger;
    }

    public async Task<MonthlyRevenueResponse> Handle(GetMonthlyRevenueQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling GetMonthlyRevenueQuery for year: {Year}, month: {Month}", request.Year, request.Month);
        
        return await _revenueMetricsService.GetMonthlyRevenueAsync(request.Year, request.Month, cancellationToken);
    }
}
