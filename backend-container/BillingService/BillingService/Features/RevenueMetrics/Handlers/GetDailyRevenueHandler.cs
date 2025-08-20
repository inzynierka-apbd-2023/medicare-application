using MediatR;
using BillingService.Features.RevenueMetrics.DTOs;
using BillingService.Features.RevenueMetrics.Queries;
using BillingService.Services;

namespace BillingService.Features.RevenueMetrics.Handlers;

public class GetDailyRevenueHandler : IRequestHandler<GetDailyRevenueQuery, DailyRevenueResponse>
{
    private readonly IRevenueMetricsService _revenueMetricsService;
    private readonly ILogger<GetDailyRevenueHandler> _logger;

    public GetDailyRevenueHandler(IRevenueMetricsService revenueMetricsService, ILogger<GetDailyRevenueHandler> logger)
    {
        _revenueMetricsService = revenueMetricsService;
        _logger = logger;
    }

    public async Task<DailyRevenueResponse> Handle(GetDailyRevenueQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling GetDailyRevenueQuery for date: {Date}", request.Date);
        
        return await _revenueMetricsService.GetDailyRevenueAsync(request.Date, cancellationToken);
    }
}
