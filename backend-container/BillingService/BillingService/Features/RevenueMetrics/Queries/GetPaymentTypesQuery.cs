using MediatR;
using BillingService.Features.RevenueMetrics.DTOs;

namespace BillingService.Features.RevenueMetrics.Queries;

public class GetPaymentTypesQuery : IRequest<PaymentTypesResponse>
{
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
}
