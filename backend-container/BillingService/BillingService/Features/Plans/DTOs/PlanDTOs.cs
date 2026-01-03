namespace BillingService.Features.Plans.DTOs;

/// <summary>
/// DTO for plan information
/// </summary>
public record PlanDto
{
    public string Code { get; init; } = default!;
    public string Name { get; init; } = default!;
    public string? Description { get; init; }
    public int PriceCents { get; init; }
    public string Currency { get; init; } = "PLN";
    public string BillingPeriod { get; init; } = "monthly";
    public int FreeVisitsPerMonth { get; init; }
    public bool HasMessaging { get; init; }
    public bool HasPrescriptions { get; init; }
    public bool HasDocuments { get; init; }
}

/// <summary>
/// Response for listing all plans
/// </summary>
public record GetAllPlansResponse
{
    public List<PlanDto> Plans { get; init; } = new();
}

/// <summary>
/// Response for getting a single plan
/// </summary>
public record GetPlanResponse
{
    public PlanDto? Plan { get; init; }
    public bool Found { get; init; }
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Response for patient's current subscription and plan
/// </summary>
public record GetPatientPlanResponse
{
    public PlanDto? Plan { get; init; }
    public SubscriptionInfo? Subscription { get; init; }
    public bool HasActiveSubscription { get; init; }
}

public record SubscriptionInfo
{
    public Guid Id { get; init; }
    public DateTime PeriodStart { get; init; }
    public DateTime PeriodEnd { get; init; }
    public string Status { get; init; } = default!;
}
