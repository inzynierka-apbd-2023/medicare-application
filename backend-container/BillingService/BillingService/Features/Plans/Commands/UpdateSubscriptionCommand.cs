using MediatR;

namespace BillingService.Features.Plans.Commands;

/// <summary>
/// Command to update a patient's subscription plan
/// </summary>
public record UpdateSubscriptionCommand : IRequest<UpdateSubscriptionResponse>
{
    public Guid PatientId { get; init; }
    public string NewPlanCode { get; init; } = string.Empty;
}

/// <summary>
/// Response for subscription update
/// </summary>
public record UpdateSubscriptionResponse
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public string? NewPlanCode { get; init; }
    public string? NewPlanName { get; init; }
}

/// <summary>
/// Request DTO for updating subscription
/// </summary>
public class UpdateSubscriptionRequest
{
    public string NewPlanCode { get; set; } = string.Empty;
}
