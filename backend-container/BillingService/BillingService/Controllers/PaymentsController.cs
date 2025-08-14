using BillingService.Data;
using BillingService.Models;
using BillingService.Infrastructure.Events;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly BillingDbContext _db;
    public PaymentsController(BillingDbContext db) { _db = db; }

    // Create payment intent (appointment or subscription)
    [HttpPost("intents")]
    [Authorize]
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
    [Authorize]
    public async Task<ActionResult<PaymentIntent>> GetIntent(string id)
    {
        var i = await _db.PaymentIntents.FindAsync(id);
        return i == null ? NotFound() : i;
    }

    // Record transaction in ledger
    [HttpPost("intents/{id}/transactions")]
    [Authorize]
    public async Task<ActionResult> RecordTransaction(string id, [FromBody] RecordTransactionRequest req)
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

        // Update intent status minimally
        if (req.Type == TransactionType.Authorization || req.Type == TransactionType.Capture)
            intent.Status = PaymentIntentStatus.Succeeded;
        if (req.Type == TransactionType.Failure)
            intent.Status = PaymentIntentStatus.Canceled;

        await _db.SaveChangesAsync();
        await EnqueueOutboxAsync(req, intent);
        return NoContent();
    }

    // Manage subscription renewal (create intent for next period)
    [HttpPost("subscriptions/{contractId}/renewals")]
    [Authorize]
    public async Task<ActionResult<PaymentIntent>> CreateRenewal(string contractId)
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
        await _db.OutboxEvents.AddAsync(new OutboxEvent
        {
            Type = BillingEvents.SubscriptionRenewalDue,
            PayloadJson = System.Text.Json.JsonSerializer.Serialize(new { ContractId = contractId, AmountCents = amount })
        });
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetIntent), new { id = intent.Id }, intent);
    }

    // Webhook receiver (idempotent)
    [HttpPost("webhooks/{provider}")]
    [AllowAnonymous]
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

    private async Task EnqueueOutboxAsync(RecordTransactionRequest req, PaymentIntent intent)
    {
        string type = req.Type switch
        {
            TransactionType.Authorization or TransactionType.Capture => intent.Kind == PaymentIntentKind.Appointment ? BillingEvents.AppointmentPaid : BillingEvents.SubscriptionPaid,
            TransactionType.Failure => BillingEvents.PaymentFailed,
            _ => ""
        };
        if (!string.IsNullOrEmpty(type))
        {
            await _db.OutboxEvents.AddAsync(new OutboxEvent
            {
                Type = type,
                PayloadJson = System.Text.Json.JsonSerializer.Serialize(new { IntentId = intent.Id, Kind = intent.Kind.ToString(), AmountCents = intent.AmountCents })
            });
            await _db.SaveChangesAsync();
        }
    }
}

public record CreateIntentRequest(PaymentIntentKind Kind, string SubjectId, string PatientId, string? Provider, long AmountCents, string? Currency);
public record RecordTransactionRequest(TransactionType Type, long AmountCents, string? Currency, string? ProviderChargeId, string? ProviderRefundId, string? FailureCode, string? FailureMessage, string? RawPayloadJson);
