using MediatR;
using BillingService.Features.Plans.DTOs;

namespace BillingService.Features.Plans.Queries;

/// <summary>
/// Query to get all active plans
/// </summary>
public record GetAllPlansQuery : IRequest<GetAllPlansResponse>;

/// <summary>
/// Query to get a specific plan by code
/// </summary>
public record GetPlanByCodeQuery : IRequest<GetPlanResponse>
{
    public string Code { get; init; } = default!;
}

/// <summary>
/// Query to get a patient's current plan
/// </summary>
public record GetPatientPlanQuery : IRequest<GetPatientPlanResponse>
{
    public Guid PatientId { get; init; }
}
