using BillingService.Data;
using BillingService.Models;
using MassTransit;
using Medicare.Messaging.Contracts;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Consumers;

public class UserRegisteredConsumer : IConsumer<IUserRegistered>
{
    private readonly BillingDbContext _db;
    private readonly ILogger<UserRegisteredConsumer> _logger;

    public UserRegisteredConsumer(BillingDbContext db, ILogger<UserRegisteredConsumer> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<IUserRegistered> context)
    {
        var evt = context.Message;
        _logger.LogInformation("Processing UserRegistered for {UserId}", evt.UserId);

        var exists = await _db.SubscriptionContracts.AnyAsync(s => s.PatientId == evt.UserId, context.CancellationToken);
        if (exists)
        {
            _logger.LogInformation("Subscription for user {UserId} already exists. Skipping.", evt.UserId);
            return;
        }

        var planCode = evt.PlanId ?? "FREE"; 
        
        var plan = await _db.Plans.FindAsync(new object[] { planCode }, context.CancellationToken);
        
        if (plan == null)
        {
            _logger.LogWarning("Plan {PlanCode} not found. Defaulting to FREE.", planCode);
            planCode = "FREE";
            plan = await _db.Plans.FindAsync(new object[] { "FREE" }, context.CancellationToken);
        }

        var now = DateTime.UtcNow;
        var end = plan?.BillingPeriod == "yearly" ? now.AddYears(1) : now.AddMonths(1);

        var sub = new SubscriptionContract
        {
            Id = Guid.NewGuid(),
            PatientId = evt.UserId,
            PlanCode = planCode,
            PeriodStart = now,
            PeriodEnd = end,
            Status = SubscriptionStatus.Active, 
            DefaultPaymentMethodId = null 
        };

        _db.SubscriptionContracts.Add(sub);
        await _db.SaveChangesAsync(context.CancellationToken);
        
        _logger.LogInformation("Created subscription {SubId} for user {UserId} with plan {PlanCode}", sub.Id, evt.UserId, planCode);
    }
}
