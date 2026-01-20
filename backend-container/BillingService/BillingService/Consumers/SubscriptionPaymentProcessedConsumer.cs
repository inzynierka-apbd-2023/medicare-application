using BillingService.Data;
using BillingService.Models;
using MassTransit;
using Medicare.Messaging.Contracts;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Consumers;

public class SubscriptionPaymentProcessedConsumer : IConsumer<ISubscriptionPaymentProcessed>
{
    private readonly BillingDbContext _db;
    private readonly ILogger<SubscriptionPaymentProcessedConsumer> _logger;

    public SubscriptionPaymentProcessedConsumer(BillingDbContext db, ILogger<SubscriptionPaymentProcessedConsumer> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ISubscriptionPaymentProcessed> context)
    {
        var evt = context.Message;
        if (!evt.IsPaid) return;

        _logger.LogInformation("Processing subscription payment for {SubscriptionId}", evt.SubscriptionId);

        var contract = await _db.SubscriptionContracts.FindAsync(evt.SubscriptionId);
        if (contract == null)
        {
            _logger.LogWarning("Subscription contract {SubscriptionId} not found", evt.SubscriptionId);
            return;
        }

        var plan = await _db.Plans.FindAsync(contract.PlanCode);
        if (plan == null)
        {
            _logger.LogWarning("Plan {PlanCode} not found for subscription {SubscriptionId}", contract.PlanCode, evt.SubscriptionId);
            return;
        }

        // Extend the period
        var now = DateTime.UtcNow;
        var baseDate = contract.PeriodEnd > now ? contract.PeriodEnd : now;
        
        contract.PeriodEnd = plan.BillingPeriod == "yearly" 
            ? baseDate.AddYears(1) 
            : baseDate.AddMonths(1);
        
        contract.Status = SubscriptionStatus.Active;

        await _db.SaveChangesAsync(context.CancellationToken);
        
        _logger.LogInformation("Subscription {SubscriptionId} extended until {NewEnd}", evt.SubscriptionId, contract.PeriodEnd);
    }
}
