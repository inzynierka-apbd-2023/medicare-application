using MediatR;
using BillingService.Features.RevenueMetrics.DTOs;
using BillingService.Features.RevenueMetrics.Queries;
using BillingService.Services;

namespace BillingService.Features.RevenueMetrics.Handlers;

public class GetPaymentTypesHandler : IRequestHandler<GetPaymentTypesQuery, PaymentTypesResponse>
{
    private readonly IRevenueMetricsService _revenueMetricsService;
    private readonly ILogger<GetPaymentTypesHandler> _logger;

    public GetPaymentTypesHandler(IRevenueMetricsService revenueMetricsService, ILogger<GetPaymentTypesHandler> logger)
    {
        _revenueMetricsService = revenueMetricsService;
        _logger = logger;
    }

    public async Task<PaymentTypesResponse> Handle(GetPaymentTypesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling GetPaymentTypesQuery for period: {StartDate} to {EndDate}", request.StartDate, request.EndDate);
        
        var breakdown = await _revenueMetricsService.GetPaymentTypesBreakdownAsync(request.StartDate, request.EndDate, cancellationToken);
        
        var paymentTypes = new List<PaymentTypeBreakdown>
        {
            new PaymentTypeBreakdown
            {
                PaymentType = "Appointment Payments",
                Revenue = breakdown.AppointmentPayments.Revenue,
                PaymentCount = breakdown.AppointmentPayments.Count,
                Percentage = breakdown.AppointmentPayments.Percentage
            },
            new PaymentTypeBreakdown
            {
                PaymentType = "Subscription Payments",
                Revenue = breakdown.SubscriptionPayments.Revenue,
                PaymentCount = breakdown.SubscriptionPayments.Count,
                Percentage = breakdown.SubscriptionPayments.Percentage
            }
        };
        
        return new PaymentTypesResponse
        {
            PaymentTypes = paymentTypes,
            TotalRevenue = breakdown.TotalRevenue,
            TotalPaymentCount = breakdown.AppointmentPayments.Count + breakdown.SubscriptionPayments.Count,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };
    }
}
