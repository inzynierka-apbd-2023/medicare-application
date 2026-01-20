using BillingService.Data;
using BillingService.Models;
using BillingService.Infrastructure.Events;
using MassTransit;
using Medicare.Messaging.Contracts;
using Medicare.ServiceDefaults.Webhooks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly BillingDbContext _db;
    private readonly IPublishEndpoint _publishEndpoint;

    public PaymentsController(BillingDbContext db, IPublishEndpoint publishEndpoint)
    {
        _db = db;
        _publishEndpoint = publishEndpoint;
    }

    [HttpPost("intents")]
    public async Task<ActionResult<PaymentIntent>> CreateIntent([FromBody] CreateIntentRequest req)
    {
        if (req.AmountCents <= 0) return BadRequest("amount required");
        var intent = new PaymentIntent
        {
            Kind = req.Kind,
            SubjectId = req.SubjectId,
            PatientId = req.PatientId,
            Provider = req.Provider ?? "mock",
            AmountCents = req.AmountCents,
            Currency = req.Currency ?? "USD",
            Status = PaymentIntentStatus.RequiresPaymentMethod,
            ClientSecret = Guid.NewGuid().ToString("N") // mock secret
        };
        _db.PaymentIntents.Add(intent);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetIntent), new { id = intent.Id }, intent);
    }

    [HttpGet("intents/{id}")]
    public async Task<ActionResult<PaymentIntent>> GetIntent(Guid id)
    {
        var i = await _db.PaymentIntents.FindAsync(id);
        return i == null ? NotFound() : i;
    }

    [HttpPost("intents/{id}/transactions")]
    public async Task<ActionResult> RecordTransaction(Guid id, [FromBody] RecordTransactionRequest req)
    {
        var intent = await _db.PaymentIntents.FindAsync(id);
        if (intent == null) return NotFound();
        var tx = new PaymentTransaction
        {
            PaymentIntentId = id,
            Type = req.Type,
            AmountCents = req.AmountCents,
            Currency = req.Currency ?? intent.Currency,
            ProviderChargeId = req.ProviderChargeId,
            ProviderRefundId = req.ProviderRefundId,
            FailureCode = req.FailureCode,
            FailureMessage = req.FailureMessage,
            RawPayloadJson = req.RawPayloadJson
        };
        _db.PaymentTransactions.Add(tx);

        if (req.Type == TransactionType.Authorization || req.Type == TransactionType.Capture)
            intent.Status = PaymentIntentStatus.Succeeded;
        if (req.Type == TransactionType.Failure)
            intent.Status = PaymentIntentStatus.Canceled;

        await PublishOutboxEventAsync(req, intent);
        await _db.SaveChangesAsync();
        
        return NoContent();
    }

    [HttpPost("subscriptions/{contractId}/renewals")]
    public async Task<ActionResult<PaymentIntent>> CreateRenewal(Guid contractId)
    {
        var c = await _db.SubscriptionContracts.FindAsync(contractId);
        if (c == null) return NotFound();
        var amount = 1999; // mock price cents
        var intent = new PaymentIntent
        {
            Kind = PaymentIntentKind.Subscription,
            SubjectId = contractId,
            PatientId = c.PatientId,
            Provider = "mock",
            AmountCents = amount,
            Currency = "USD",
            Status = PaymentIntentStatus.RequiresPaymentMethod,
            ClientSecret = Guid.NewGuid().ToString("N")
        };
        _db.PaymentIntents.Add(intent);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetIntent), new { id = intent.Id }, intent);
    }

    [HttpPost("webhooks/{provider}")]
    [AllowAnonymous]
    [RequireWebhookSignature]
    public async Task<ActionResult> Webhook(string provider)
    {
        var payload = await new StreamReader(Request.Body).ReadToEndAsync();
        var id = Request.Headers["X-Event-Id"].FirstOrDefault() ?? Guid.NewGuid().ToString();
        var exists = await _db.PspWebhookEvents.AnyAsync(e => e.Id == id && e.Provider == provider);
        if (!exists)
        {
            _db.PspWebhookEvents.Add(new PspWebhookEvent { Id = id, Provider = provider, PayloadJson = payload, Processed = false });
            await _db.SaveChangesAsync();
        }
        return Ok();
    }

    private async Task PublishOutboxEventAsync(RecordTransactionRequest req, PaymentIntent intent)
    {
        if (req.Type == TransactionType.Authorization || req.Type == TransactionType.Capture)
        {
            if (intent.Kind == PaymentIntentKind.Appointment)
            {
                await _publishEndpoint.Publish<IBillingPaymentProcessed>(new
                {
                    AppointmentId = intent.SubjectId,
                    IsPaid = true,
                    intent.AmountCents,
                    PlanCode = (string?)null,
                    Error = (string?)null
                });
            }
            else if (intent.Kind == PaymentIntentKind.Subscription)
            {
                var contract = await _db.SubscriptionContracts.FindAsync(intent.SubjectId);
                var planCode = contract?.PlanCode ?? "UNKNOWN";

                await _publishEndpoint.Publish<ISubscriptionPaymentProcessed>(new
                {
                    SubscriptionId = intent.SubjectId,
                    PatientId = intent.PatientId,
                    IsPaid = true,
                    intent.AmountCents,
                    PlanCode = planCode
                });
            }
        }
    }
}

public record CreateIntentRequest(PaymentIntentKind Kind, Guid SubjectId, Guid PatientId, string? Provider, long AmountCents, string? Currency);
public record RecordTransactionRequest(TransactionType Type, long AmountCents, string? Currency, string? ProviderChargeId, string? ProviderRefundId, string? FailureCode, string? FailureMessage, string? RawPayloadJson);
