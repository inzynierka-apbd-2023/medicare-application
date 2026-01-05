using MediatR;
using Microsoft.EntityFrameworkCore;
using BillingService.Data;
using BillingService.Features.Plans.DTOs;
using BillingService.Features.Plans.Queries;
using BillingService.Features.Plans.Commands;
using BillingService.Models;

namespace BillingService.Features.Plans.Handlers;

/// <summary>
/// Handler for GetAllPlansQuery
/// </summary>
public class GetAllPlansHandler : IRequestHandler<GetAllPlansQuery, GetAllPlansResponse>
{
    private readonly BillingDbContext _db;
    private readonly ILogger<GetAllPlansHandler> _logger;

    public GetAllPlansHandler(BillingDbContext db, ILogger<GetAllPlansHandler> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<GetAllPlansResponse> Handle(GetAllPlansQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling GetAllPlansQuery");

        var plans = await _db.Plans
            .Where(p => p.IsActive)
            .OrderBy(p => p.SortOrder)
            .Select(p => new PlanDto
            {
                Code = p.Code,
                Name = p.Name,
                Description = p.Description,
                PriceCents = p.PriceCents,
                Currency = p.Currency,
                BillingPeriod = p.BillingPeriod,
                FreeVisitsPerMonth = p.FreeVisitsPerMonth,
                HasMessaging = p.HasMessaging,
                HasPrescriptions = p.HasPrescriptions,
                HasDocuments = p.HasDocuments
            })
            .ToListAsync(cancellationToken);

        return new GetAllPlansResponse { Plans = plans };
    }
}

/// <summary>
/// Handler for GetPlanByCodeQuery
/// </summary>
public class GetPlanByCodeHandler : IRequestHandler<GetPlanByCodeQuery, GetPlanResponse>
{
    private readonly BillingDbContext _db;
    private readonly ILogger<GetPlanByCodeHandler> _logger;

    public GetPlanByCodeHandler(BillingDbContext db, ILogger<GetPlanByCodeHandler> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<GetPlanResponse> Handle(GetPlanByCodeQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling GetPlanByCodeQuery for code: {Code}", request.Code);

        var plan = await _db.Plans
            .Where(p => p.Code == request.Code && p.IsActive)
            .Select(p => new PlanDto
            {
                Code = p.Code,
                Name = p.Name,
                Description = p.Description,
                PriceCents = p.PriceCents,
                Currency = p.Currency,
                BillingPeriod = p.BillingPeriod,
                FreeVisitsPerMonth = p.FreeVisitsPerMonth,
                HasMessaging = p.HasMessaging,
                HasPrescriptions = p.HasPrescriptions,
                HasDocuments = p.HasDocuments
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (plan == null)
        {
            return new GetPlanResponse
            {
                Found = false,
                ErrorMessage = $"Plan '{request.Code}' not found"
            };
        }

        return new GetPlanResponse { Plan = plan, Found = true };
    }
}

/// <summary>
/// Handler for GetPatientPlanQuery
/// </summary>
public class GetPatientPlanHandler : IRequestHandler<GetPatientPlanQuery, GetPatientPlanResponse>
{
    private readonly BillingDbContext _db;
    private readonly ILogger<GetPatientPlanHandler> _logger;

    public GetPatientPlanHandler(BillingDbContext db, ILogger<GetPatientPlanHandler> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<GetPatientPlanResponse> Handle(GetPatientPlanQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling GetPatientPlanQuery for patient: {PatientId}", request.PatientId);

        var subscription = await _db.SubscriptionContracts
            .Where(s => s.PatientId == request.PatientId && s.Status == Models.SubscriptionStatus.Active)
            .OrderByDescending(s => s.PeriodEnd)
            .FirstOrDefaultAsync(cancellationToken);

        if (subscription == null)
        {
            // Return FREE plan for users without subscription
            var freePlan = await _db.Plans
                .Where(p => p.Code == "FREE")
                .Select(p => new PlanDto
                {
                    Code = p.Code,
                    Name = p.Name,
                    Description = p.Description,
                    PriceCents = p.PriceCents,
                    Currency = p.Currency,
                    BillingPeriod = p.BillingPeriod,
                    FreeVisitsPerMonth = p.FreeVisitsPerMonth,
                    HasMessaging = p.HasMessaging,
                    HasPrescriptions = p.HasPrescriptions,
                    HasDocuments = p.HasDocuments
                })
                .FirstOrDefaultAsync(cancellationToken);

            return new GetPatientPlanResponse 
            { 
                Plan = freePlan,
                HasActiveSubscription = false 
            };
        }

        var plan = await _db.Plans
            .Where(p => p.Code == subscription.PlanCode)
            .Select(p => new PlanDto
            {
                Code = p.Code,
                Name = p.Name,
                Description = p.Description,
                PriceCents = p.PriceCents,
                Currency = p.Currency,
                BillingPeriod = p.BillingPeriod,
                FreeVisitsPerMonth = p.FreeVisitsPerMonth,
                HasMessaging = p.HasMessaging,
                HasPrescriptions = p.HasPrescriptions,
                HasDocuments = p.HasDocuments
            })
            .FirstOrDefaultAsync(cancellationToken);

        return new GetPatientPlanResponse
        {
            Plan = plan,
            Subscription = new SubscriptionInfo
            {
                Id = subscription.Id,
                PeriodStart = subscription.PeriodStart,
                PeriodEnd = subscription.PeriodEnd,
                Status = subscription.Status.ToString()
            },
            HasActiveSubscription = true
        };
    }
}

/// <summary>
/// Handler for UpdateSubscriptionCommand - allows upgrade/downgrade of plans
/// </summary>
public class UpdateSubscriptionHandler : IRequestHandler<UpdateSubscriptionCommand, UpdateSubscriptionResponse>
{
    private readonly BillingDbContext _db;
    private readonly ILogger<UpdateSubscriptionHandler> _logger;

    public UpdateSubscriptionHandler(BillingDbContext db, ILogger<UpdateSubscriptionHandler> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<UpdateSubscriptionResponse> Handle(UpdateSubscriptionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling UpdateSubscriptionCommand for patient: {PatientId}, new plan: {NewPlanCode}", 
            request.PatientId, request.NewPlanCode);

        // Validate new plan exists
        var newPlan = await _db.Plans
            .Where(p => p.Code == request.NewPlanCode && p.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        if (newPlan == null)
        {
            return new UpdateSubscriptionResponse
            {
                Success = false,
                ErrorMessage = $"Plan '{request.NewPlanCode}' not found"
            };
        }

        // Get current subscription
        var currentSubscription = await _db.SubscriptionContracts
            .Where(s => s.PatientId == request.PatientId && s.Status == SubscriptionStatus.Active)
            .FirstOrDefaultAsync(cancellationToken);

        var currentPlanCode = currentSubscription?.PlanCode ?? "FREE";

        // Check if trying to update to same plan
        if (currentPlanCode == request.NewPlanCode)
        {
            return new UpdateSubscriptionResponse
            {
                Success = false,
                ErrorMessage = "You already have this plan. Please select a different plan."
            };
        }

        var now = DateTime.UtcNow;
        var isYearly = request.NewPlanCode.Contains("YEARLY", StringComparison.OrdinalIgnoreCase);
        var periodEnd = isYearly ? now.AddYears(1) : now.AddMonths(1);

        if (currentSubscription != null)
        {
            // Update existing subscription
            if (request.NewPlanCode == "FREE")
            {
                // Downgrading to FREE - cancel subscription
                currentSubscription.Status = SubscriptionStatus.Canceled;
                currentSubscription.PlanCode = "FREE";
                _logger.LogInformation("Cancelled subscription for patient {PatientId}, downgraded to FREE", request.PatientId);
            }
            else
            {
                // Upgrade/change to different paid plan
                currentSubscription.PlanCode = request.NewPlanCode;
                currentSubscription.PeriodStart = now;
                currentSubscription.PeriodEnd = periodEnd;
                _logger.LogInformation("Updated subscription for patient {PatientId} from {OldPlan} to {NewPlan}", 
                    request.PatientId, currentPlanCode, request.NewPlanCode);
            }
        }
        else
        {
            // No subscription - create new one
            if (request.NewPlanCode != "FREE")
            {
                var newSubscription = new SubscriptionContract
                {
                    Id = Guid.NewGuid(),
                    PatientId = request.PatientId,
                    PlanCode = request.NewPlanCode,
                    PeriodStart = now,
                    PeriodEnd = periodEnd,
                    Status = SubscriptionStatus.Active
                };
                _db.SubscriptionContracts.Add(newSubscription);
                _logger.LogInformation("Created new subscription for patient {PatientId} with plan {NewPlan}", 
                    request.PatientId, request.NewPlanCode);
            }
        }

        await _db.SaveChangesAsync(cancellationToken);

        return new UpdateSubscriptionResponse
        {
            Success = true,
            NewPlanCode = request.NewPlanCode,
            NewPlanName = newPlan.Name
        };
    }
}

