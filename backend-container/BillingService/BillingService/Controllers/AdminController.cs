using BillingService.Data;
using BillingService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Controllers;

[ApiController]
[Route("api/billing/admin")]
[Authorize(Roles = "Owner,Admin")]
public class AdminController : ControllerBase
{
    private readonly BillingDbContext _db;
    private readonly IWebHostEnvironment _env;
    public AdminController(BillingDbContext db, IWebHostEnvironment env) { _db = db; _env = env; }

    [HttpPost("purge")]
    public async Task<ActionResult> Purge()
    {
        if (_env.IsProduction()) return Forbid("Not allowed in production.");
        var strategy = _db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                await _db.Database.ExecuteSqlRawAsync(@"IF OBJECT_ID(N'billing.Outbox_Event') IS NOT NULL DELETE FROM billing.Outbox_Event;");
                await _db.Database.ExecuteSqlRawAsync(@"IF OBJECT_ID(N'billing.Psp_Webhook_Event') IS NOT NULL DELETE FROM billing.Psp_Webhook_Event;");
                await _db.Database.ExecuteSqlRawAsync(@"IF OBJECT_ID(N'billing.Subscription_Payment') IS NOT NULL DELETE FROM billing.Subscription_Payment;");
                await _db.Database.ExecuteSqlRawAsync(@"IF OBJECT_ID(N'billing.Appointment_Payment') IS NOT NULL DELETE FROM billing.Appointment_Payment;");
                await _db.Database.ExecuteSqlRawAsync(@"IF OBJECT_ID(N'billing.Payment_Transaction') IS NOT NULL DELETE FROM billing.Payment_Transaction;");
                await _db.Database.ExecuteSqlRawAsync(@"IF OBJECT_ID(N'billing.Payment_Intent') IS NOT NULL DELETE FROM billing.Payment_Intent;");
                await _db.Database.ExecuteSqlRawAsync(@"IF OBJECT_ID(N'billing.Subscription_Contract') IS NOT NULL DELETE FROM billing.Subscription_Contract;");
                await _db.Database.ExecuteSqlRawAsync(@"IF OBJECT_ID(N'billing.Payment_Method') IS NOT NULL DELETE FROM billing.Payment_Method;");
                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        });
        return NoContent();
    }

    [HttpPost("purge-and-seed")]
    public async Task<ActionResult<object>> PurgeAndSeed()
    {
        if (_env.IsProduction()) return Forbid("Not allowed in production.");
        var strategy = _db.Database.CreateExecutionStrategy();
        Guid patientId = Guid.NewGuid();
        Guid contractId = Guid.Empty;
        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                await _db.Database.ExecuteSqlRawAsync(@"IF OBJECT_ID(N'billing.Outbox_Event') IS NOT NULL DELETE FROM billing.Outbox_Event;");
                await _db.Database.ExecuteSqlRawAsync(@"IF OBJECT_ID(N'billing.Psp_Webhook_Event') IS NOT NULL DELETE FROM billing.Psp_Webhook_Event;");
                await _db.Database.ExecuteSqlRawAsync(@"IF OBJECT_ID(N'billing.Subscription_Payment') IS NOT NULL DELETE FROM billing.Subscription_Payment;");
                await _db.Database.ExecuteSqlRawAsync(@"IF OBJECT_ID(N'billing.Appointment_Payment') IS NOT NULL DELETE FROM billing.Appointment_Payment;");
                await _db.Database.ExecuteSqlRawAsync(@"IF OBJECT_ID(N'billing.Payment_Transaction') IS NOT NULL DELETE FROM billing.Payment_Transaction;");
                await _db.Database.ExecuteSqlRawAsync(@"IF OBJECT_ID(N'billing.Payment_Intent') IS NOT NULL DELETE FROM billing.Payment_Intent;");
                await _db.Database.ExecuteSqlRawAsync(@"IF OBJECT_ID(N'billing.Subscription_Contract') IS NOT NULL DELETE FROM billing.Subscription_Contract;");
                await _db.Database.ExecuteSqlRawAsync(@"IF OBJECT_ID(N'billing.Payment_Method') IS NOT NULL DELETE FROM billing.Payment_Method;");

                var pm = new PaymentMethod { PatientId = patientId, Provider = "mock", ProviderToken = "tok_123", Last4 = "4242", Brand = "VISA", IsDefault = true };
                _db.PaymentMethods.Add(pm);
                var today = DateTime.UtcNow.Date;
                var periodStart = new DateTime(today.Year, today.Month, 1);
                var periodEnd = periodStart.AddMonths(1).AddDays(-1);
                var contract = new SubscriptionContract { PatientId = patientId, PlanCode = "BASIC", PeriodStart = periodStart, PeriodEnd = periodEnd, Status = SubscriptionStatus.Active, DefaultPaymentMethodId = pm.Id };
                _db.SubscriptionContracts.Add(contract);
                await _db.SaveChangesAsync();
                contractId = contract.Id;

                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        });

        return Ok(new { PatientId = patientId, ContractId = contractId });
    }

    [HttpPost("seed-realistic")]
    public async Task<ActionResult<object>> SeedRealistic()
    {
        if (_env.IsProduction()) return Forbid("Not allowed in production.");
        var strategy = _db.Database.CreateExecutionStrategy();
        var now = DateTime.UtcNow;
        var ids = new List<object>();
        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                // Create two patients and default payment methods
                var patientA = Guid.NewGuid();
                var patientB = Guid.NewGuid();
                var pmA = new PaymentMethod { PatientId = patientA, Provider = "mock", ProviderToken = "tok_a", Last4 = "4242", Brand = "VISA", IsDefault = true };
                var pmB = new PaymentMethod { PatientId = patientB, Provider = "mock", ProviderToken = "tok_b", Last4 = "1881", Brand = "MC", IsDefault = true };
                _db.PaymentMethods.AddRange(pmA, pmB);
                await _db.SaveChangesAsync();

                // Active subscription for A, past-due for B
                DateTime pStart = new DateTime(now.Year, now.Month, 1);
                DateTime pEnd = pStart.AddMonths(1).AddDays(-1);
                var subA = new SubscriptionContract { PatientId = patientA, PlanCode = "BASIC", PeriodStart = pStart, PeriodEnd = pEnd, Status = SubscriptionStatus.Active, DefaultPaymentMethodId = pmA.Id };
                var subB = new SubscriptionContract { PatientId = patientB, PlanCode = "PLUS", PeriodStart = pStart, PeriodEnd = pEnd, Status = SubscriptionStatus.PastDue, DefaultPaymentMethodId = pmB.Id };
                _db.SubscriptionContracts.AddRange(subA, subB);
                await _db.SaveChangesAsync();

                // Appointment payment flow for A
                var appointmentId = Guid.NewGuid();
                var intentAppt = new PaymentIntent
                {
                    Kind = PaymentIntentKind.Appointment,
                    SubjectId = appointmentId,
                    PatientId = patientA,
                    Provider = "mock",
                    AmountCents = 7500,
                    Currency = "USD",
                    Status = PaymentIntentStatus.RequiresPaymentMethod,
                    ClientSecret = Guid.NewGuid().ToString("N")
                };
                _db.PaymentIntents.Add(intentAppt);
                await _db.SaveChangesAsync();
                _db.PaymentTransactions.Add(new PaymentTransaction
                {
                    PaymentIntentId = intentAppt.Id,
                    Type = TransactionType.Capture,
                    AmountCents = 7500,
                    Currency = "USD",
                    ProviderChargeId = "ch_123"
                });
                intentAppt.Status = PaymentIntentStatus.Succeeded;
                _db.AppointmentPayments.Add(new AppointmentPayment
                {
                    AppointmentId = appointmentId,
                    PatientId = patientA,
                    AmountCents = 7500,
                    Currency = "USD",
                    PaymentIntentId = intentAppt.Id
                });

                // Subscription renewal intent for A and payment record
                var intentSub = new PaymentIntent
                {
                    Kind = PaymentIntentKind.Subscription,
                    SubjectId = subA.Id,
                    PatientId = patientA,
                    Provider = "mock",
                    AmountCents = 1999,
                    Currency = "USD",
                    Status = PaymentIntentStatus.Succeeded,
                    ClientSecret = Guid.NewGuid().ToString("N")
                };
                _db.PaymentIntents.Add(intentSub);
                await _db.SaveChangesAsync();
                _db.PaymentTransactions.Add(new PaymentTransaction
                {
                    PaymentIntentId = intentSub.Id,
                    Type = TransactionType.Capture,
                    AmountCents = 1999,
                    Currency = "USD",
                    ProviderChargeId = "ch_456"
                });
                _db.SubscriptionPayments.Add(new SubscriptionPayment
                {
                    SubscriptionContractId = subA.Id,
                    PatientId = patientA,
                    PlanCode = subA.PlanCode,
                    PeriodStart = pStart,
                    PeriodEnd = pEnd,
                    AmountCents = 1999,
                    Currency = "USD",
                    PaymentIntentId = intentSub.Id
                });

                // PSP webhook inbox example
                _db.PspWebhookEvents.Add(new PspWebhookEvent
                {
                    Id = "evt_abc",
                    Provider = "mock",
                    PayloadJson = "{\"type\":\"payment.succeeded\"}",
                    Processed = true
                });

                // Outbox examples
                _db.OutboxEvents.AddRange(
                    new OutboxEvent { Type = Infrastructure.Events.BillingEvents.AppointmentPaid, PayloadJson = System.Text.Json.JsonSerializer.Serialize(new { IntentId = intentAppt.Id, AmountCents = 7500 }) },
                    new OutboxEvent { Type = Infrastructure.Events.BillingEvents.SubscriptionPaid, PayloadJson = System.Text.Json.JsonSerializer.Serialize(new { IntentId = intentSub.Id, AmountCents = 1999 }) }
                );

                await _db.SaveChangesAsync();

                ids.Add(new { PatientA = patientA, PatientB = patientB, AppointmentIntentId = intentAppt.Id, SubscriptionIntentId = intentSub.Id, SubscriptionA = subA.Id, SubscriptionB = subB.Id });

                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        });
        return Ok(ids.First());
    }

    [HttpGet("outbox")] 
    public async Task<ActionResult<IEnumerable<object>>> Outbox()
    {
        if (_env.IsProduction()) return Forbid();
        var items = await _db.OutboxEvents.OrderByDescending(o => o.OccurredAt).Take(20)
            .Select(o => new { o.Type, o.OccurredAt, o.PublishedAt, o.PayloadJson }).ToListAsync();
        return items;
    }
}

public record ProcessAppointmentRequest(Guid AppointmentId, Guid PatientId);
