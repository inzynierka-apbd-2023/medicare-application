using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using BillingService.Data; // Assuming this exists or similar
using Microsoft.EntityFrameworkCore;

namespace BillingService.Controllers;

[ApiController]
[Route("api/billing/payment")]
public class PaymentController : ControllerBase
{
    private readonly IConnection _mqConnection;
    private readonly ILogger<PaymentController> _logger;
    private readonly BillingDbContext _db;

    public PaymentController(IConnection mqConnection, ILogger<PaymentController> logger, BillingDbContext db)
    {
        _mqConnection = mqConnection;
        _logger = logger;
        _db = db;
    }

    [HttpPost("mock")]
    public async Task<IActionResult> ProcessMockPayment([FromBody] MockPaymentRequest req)
    {
        _logger.LogInformation("Processing mock payment for Appointment {Id} via {Method}", req.AppointmentId, req.PaymentMethod);

        // 1. Find the Billing Record
        var paymentRecord = await _db.AppointmentPayments.FirstOrDefaultAsync(ap => ap.AppointmentId == req.AppointmentId);
        
        if (paymentRecord == null)
        {
             // If not found, maybe create it? For now, log warning and return not found or try to proceed if we can recover.
             _logger.LogWarning("AppointmentPayment record not found for {Id}", req.AppointmentId);
             // Let's create one on the fly for robustness if this happens
             paymentRecord = new BillingService.Models.AppointmentPayment
             {
                 AppointmentId = req.AppointmentId,
                 PatientId = req.PatientId, 
                 AmountCents = 30000, 
                 Currency = "PLN",
                 CreatedAt = DateTime.UtcNow
             };
             _db.AppointmentPayments.Add(paymentRecord);
        }

        // 2. Create a successful "Mock" Payment Intent
        var intent = new BillingService.Models.PaymentIntent
        {
            Id = Guid.NewGuid(),
            Kind = BillingService.Models.PaymentIntentKind.Appointment,
            SubjectId = req.AppointmentId,
            PatientId = req.PatientId,
            Provider = "mock",
            AmountCents = paymentRecord.AmountCents,
            Currency = "PLN",
            Status = BillingService.Models.PaymentIntentStatus.Succeeded,
            CreatedAt = DateTime.UtcNow,
            ClientSecret = "mock_secret_" + Guid.NewGuid()
        };
        
        _db.PaymentIntents.Add(intent);
        
        // 3. Link Intent to Payment Record
        paymentRecord.PaymentIntentId = intent.Id;
        
        await _db.SaveChangesAsync();
        _logger.LogInformation("Persisted Mock PaymentIntent {IntentId} for Appointment {ApptId} (Patient: {PatientId})", intent.Id, req.AppointmentId, req.PatientId);

        // 4. Publish Event
        await PublishPaymentProcessedAsync(req.AppointmentId, true, paymentRecord.AmountCents); 

        return Ok(new { Success = true, Message = "Payment processed successfully" });
    }

    private async Task PublishPaymentProcessedAsync(Guid appointmentId, bool isPaid, long amountCents)
    {
        try
        {
            await using var channel = await _mqConnection.CreateChannelAsync();
            // Declare exchange to ensure it exists
            await channel.ExchangeDeclareAsync("billing.events", ExchangeType.Topic, durable: true);

            var evt = new
            {
                AppointmentId = appointmentId,
                IsPaid = isPaid,
                AmountCents = amountCents,
                PlanCode = "MOCK",
                OccurredAt = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(evt);
            var body = Encoding.UTF8.GetBytes(json);

            var props = new BasicProperties();
            await channel.BasicPublishAsync(exchange: "billing.events",
                                 routingKey: "billing.appointment_payment_processed",
                                 mandatory: false,
                                 basicProperties: props,
                                 body: body);
            
            _logger.LogInformation("Published billing.appointment_payment_processed for {Id}", appointmentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish payment processed event");
            // Not throwing, as we want to return success to UI for now? 
            // Ideally should throw or return 500.
            throw;
        }
    }
}

public record MockPaymentRequest(Guid AppointmentId, Guid PatientId, string PaymentMethod);
