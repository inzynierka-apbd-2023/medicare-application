using MediatR;
using BillingService.Features.RevenueMetrics.DTOs;
using BillingService.Features.RevenueMetrics.Queries;
using BillingService.Services;
using System.Globalization;

namespace BillingService.Features.RevenueMetrics.Handlers;

public class GetYearlyRevenueHandler : IRequestHandler<GetYearlyRevenueQuery, YearlyRevenueResponse>
{
    private readonly IRevenueMetricsService _revenueMetricsService;
    private readonly ILogger<GetYearlyRevenueHandler> _logger;

    public GetYearlyRevenueHandler(IRevenueMetricsService revenueMetricsService, ILogger<GetYearlyRevenueHandler> logger)
    {
        _revenueMetricsService = revenueMetricsService;
        _logger = logger;
    }

    public async Task<YearlyRevenueResponse> Handle(GetYearlyRevenueQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling GetYearlyRevenueQuery for year: {Year}", request.Year);
        
        return await _revenueMetricsService.GetYearlyRevenueAsync(request.Year, cancellationToken);
    }
}
