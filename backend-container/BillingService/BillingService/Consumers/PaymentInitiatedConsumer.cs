using MassTransit;
using Medicare.Messaging.Contracts;
using BillingService.Data;
using BillingService.Models;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Consumers;

public class PaymentInitiatedConsumer : IConsumer<IBillingPaymentInitiated>
{
    private readonly ILogger<PaymentInitiatedConsumer> _logger;
    private readonly BillingDbContext _db;
    private readonly IPublishEndpoint _publishEndpoint;

    public PaymentInitiatedConsumer(
        ILogger<PaymentInitiatedConsumer> logger,
        BillingDbContext db,
        IPublishEndpoint publishEndpoint)
    {
        _logger = logger;
        _db = db;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Consume(ConsumeContext<IBillingPaymentInitiated> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Processing PaymentInitiated for {Id}", msg.AppointmentId);

        try
        {
            // 1. Find or Create Billing Record (Idempotency)
            var paymentRecord = await _db.AppointmentPayments.FirstOrDefaultAsync(ap => ap.AppointmentId == msg.AppointmentId, context.CancellationToken);
            
            if (paymentRecord == null)
            {
                 paymentRecord = new AppointmentPayment
                 {
                     AppointmentId = msg.AppointmentId,
                     PatientId = msg.PatientId, 
                     AmountCents = 30000, 
                     Currency = "PLN",
                     CreatedAt = DateTime.UtcNow,
                     ForDate = DateTime.UtcNow
                 };
                 _db.AppointmentPayments.Add(paymentRecord);
            }
            else if (paymentRecord.PaymentIntentId.HasValue) 
            {
                 await PublishProcessed(msg.AppointmentId, true, paymentRecord.AmountCents, context.CancellationToken);
                 await _db.SaveChangesAsync(context.CancellationToken);
                 return;
            }

            var intent = new PaymentIntent
            {
                Id = Guid.NewGuid(),
                Kind = PaymentIntentKind.Appointment,
                SubjectId = msg.AppointmentId,
                PatientId = msg.PatientId,
                Provider = "mock",
                AmountCents = paymentRecord.AmountCents,
                Currency = "PLN",
                Status = PaymentIntentStatus.Succeeded,
                CreatedAt = DateTime.UtcNow,
                ClientSecret = "mock_secret_" + Guid.NewGuid()
            };
            
            _db.PaymentIntents.Add(intent);
            
            paymentRecord.PaymentIntentId = intent.Id;
            
            await PublishProcessed(msg.AppointmentId, true, paymentRecord.AmountCents, context.CancellationToken);
            await _db.SaveChangesAsync(context.CancellationToken);
        }
        catch (Exception ex)
        {
             _logger.LogError(ex, "Error handling payment initiation");
             await _publishEndpoint.Publish<IBillingPaymentProcessed>(new
             {
                 msg.AppointmentId,
                 IsPaid = false,
                 AmountCents = 0L,
                 Error = ex.Message
             }, context.CancellationToken);

             await _db.SaveChangesAsync(context.CancellationToken);
        }
    }

    private async Task PublishProcessed(Guid appointmentId, bool isPaid, long amount, CancellationToken ct)
    {
        await _publishEndpoint.Publish<IBillingPaymentProcessed>(new
        {
            AppointmentId = appointmentId,
            IsPaid = isPaid,
            AmountCents = amount,
            PlanCode = "MOCK"
        }, ct);
    }
}
