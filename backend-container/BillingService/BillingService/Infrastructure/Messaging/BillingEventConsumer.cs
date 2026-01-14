using BillingService.Data;
using BillingService.Models;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace BillingService.Infrastructure.Messaging;

// Minimal record matching the one published by UserService
public record UserRegistered(Guid UserId, string Username, string Email, DateTime OccurredAtUtc, string? PlanId);
// Record from AppointmentService
public record AppointmentCreated(Guid AppointmentId, Guid PatientId, Guid DoctorId, DateTime ScheduledAt, DateTime OccurredAt);
public record AppointmentBillingProcessed(Guid AppointmentId, bool IsPaid, long AmountCents, string? PlanCode);

public record PaymentInitiated(Guid AppointmentId, Guid PatientId, string PaymentMethod, DateTime Timestamp);

public class BillingEventConsumer : BackgroundService
{
    private readonly IServiceProvider _sp;
    private readonly ILogger<BillingEventConsumer> _logger;
    private readonly IConnection _conn;
    private IChannel? _channel;

    public BillingEventConsumer(IServiceProvider sp, IConnection conn, ILogger<BillingEventConsumer> logger)
    {
        _sp = sp;
        _conn = conn;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Attempting to connect to RabbitMQ...");
        try
        {
            _channel = await _conn.CreateChannelAsync(cancellationToken: stoppingToken);
            
            await _channel.ExchangeDeclareAsync("user.events", ExchangeType.Topic, durable: true, cancellationToken: stoppingToken);
            await _channel.QueueDeclareAsync("billing.user_created", durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: stoppingToken);
            await _channel.QueueBindAsync("billing.user_created", "user.events", "user.created", arguments: null, cancellationToken: stoppingToken);

            // Bind to appointment.events
            await _channel.ExchangeDeclareAsync("appointment.events", ExchangeType.Topic, durable: true, cancellationToken: stoppingToken);
            await _channel.QueueDeclareAsync("billing.appointment_created", durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: stoppingToken);
            await _channel.QueueBindAsync("billing.appointment_created", "appointment.events", "appointment.created", arguments: null, cancellationToken: stoppingToken);

            // Declare exchange for billing events
            await _channel.ExchangeDeclareAsync("billing.events", ExchangeType.Topic, durable: true, cancellationToken: stoppingToken);
            
            // Bind to billing.payment_initiated
            await _channel.QueueBindAsync("billing.payment_requests", "billing.events", "billing.payment_initiated", arguments: null, cancellationToken: stoppingToken);

            _logger.LogInformation("Connected to RabbitMQ and ready to consume messages");
        }
        catch (Exception ex)
        {
             _logger.LogError(ex, "Failed to setup RabbitMQ channel");
            return;
        }

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var msg = Encoding.UTF8.GetString(body);
                
                if (ea.RoutingKey == "user.created")
                {
                    var evt = JsonSerializer.Deserialize<UserRegistered>(msg, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (evt != null && evt.UserId != Guid.Empty)
                    {
                        await HandleUserCreatedAsync(evt);
                    }
                }
                else if (ea.RoutingKey == "appointment.created") 
                {
                    var evt = JsonSerializer.Deserialize<AppointmentCreated>(msg, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (evt != null)
                    {
                        await HandleAppointmentCreatedAsync(evt);
                    }
                }
                else if (ea.RoutingKey == "billing.payment_initiated")
                {
                    var evt = JsonSerializer.Deserialize<PaymentInitiated>(msg, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (evt != null)
                    {
                        await HandlePaymentInitiatedAsync(evt);
                    }
                }
                
                await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message");
                await _channel.BasicNackAsync(ea.DeliveryTag, false, false, stoppingToken); 
            }
        };

        await _channel.BasicConsumeAsync("billing.user_created", false, consumer, cancellationToken: stoppingToken);
        await _channel.BasicConsumeAsync("billing.payment_requests", false, consumer, cancellationToken: stoppingToken);
        await _channel.BasicConsumeAsync("billing.appointment_created", false, consumer, cancellationToken: stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task HandleUserCreatedAsync(UserRegistered evt)
    {
        using var scope = _sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BillingDbContext>();

        var exists = await db.SubscriptionContracts.AnyAsync(s => s.PatientId == evt.UserId);
        if (exists)
        {
            _logger.LogInformation($"Subscription for user {evt.UserId} already exists. Skipping.");
            return;
        }

        var planCode = evt.PlanId ?? "FREE"; 
        
        var plan = await db.Plans.FindAsync(planCode);
        
        if (plan == null)
        {
            _logger.LogWarning($"Plan {planCode} not found. Defaulting to FREE.");
            planCode = "FREE";
            plan = await db.Plans.FindAsync("FREE");
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

        db.SubscriptionContracts.Add(sub);
        await db.SaveChangesAsync();
        
        _logger.LogInformation($"Created subscription {sub.Id} for user {evt.UserId} with plan {planCode}");
    }

    private async Task HandleAppointmentCreatedAsync(AppointmentCreated evt)
    {
        using var scope = _sp.CreateScope();
        var billingService = scope.ServiceProvider.GetRequiredService<BillingService.Services.AppointmentBillingService>();

        try 
        {
            var result = await billingService.EvaluateAndRecordPaymentAsync(evt.AppointmentId, evt.PatientId, evt.ScheduledAt);

            // Publish result so AppointmentService can update status
            await PublishPaymentProcessedAsync(evt.AppointmentId, !result.IsFree && result.AmountCents == 0 /* free is paid */ || result.IsFree, result.AmountCents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to evaluate billing for appointment {Id}", evt.AppointmentId);
        }
    }

    private async Task HandlePaymentInitiatedAsync(PaymentInitiated evt)
    {
        using var scope = _sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BillingDbContext>();

        try
        {
            // 1. Find or Create Billing Record (Idempotency)
            var paymentRecord = await db.AppointmentPayments.FirstOrDefaultAsync(ap => ap.AppointmentId == evt.AppointmentId);
            
            if (paymentRecord == null)
            {
                 paymentRecord = new AppointmentPayment
                 {
                     AppointmentId = evt.AppointmentId,
                     PatientId = evt.PatientId, 
                     AmountCents = 30000, 
                     Currency = "PLN",
                     CreatedAt = DateTime.UtcNow,
                     ForDate = DateTime.UtcNow
                 };
                 db.AppointmentPayments.Add(paymentRecord);
            }
            else if (paymentRecord.PaymentIntentId.HasValue) 
            {
                 // Resend event just in case
                 await PublishPaymentProcessedAsync(evt.AppointmentId, true, paymentRecord.AmountCents);
                 return;
            }

            // 2. Create Intent
            var intent = new PaymentIntent
            {
                Id = Guid.NewGuid(),
                Kind = PaymentIntentKind.Appointment,
                SubjectId = evt.AppointmentId,
                PatientId = evt.PatientId,
                Provider = "mock",
                AmountCents = paymentRecord.AmountCents,
                Currency = "PLN",
                Status = PaymentIntentStatus.Succeeded,
                CreatedAt = DateTime.UtcNow,
                ClientSecret = "mock_secret_" + Guid.NewGuid()
            };
            
            db.PaymentIntents.Add(intent);
            
            // 3. Link
            paymentRecord.PaymentIntentId = intent.Id;
            
            await db.SaveChangesAsync();
            
            // 4. Publish Event
            await PublishPaymentProcessedAsync(evt.AppointmentId, true, paymentRecord.AmountCents); 
        }
        catch (Exception ex)
        {
             _logger.LogError(ex, "Error handling payment initiation");
        }
    }

    private async Task PublishPaymentProcessedAsync(Guid appointmentId, bool isPaid, long amountCents)
    {
        await using var channel = await _conn.CreateChannelAsync();
        // Exchange declared in setup
        var evt = new AppointmentBillingProcessed(appointmentId, isPaid, amountCents, "MOCK");

        var json = JsonSerializer.Serialize(evt, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }); 
        var body = Encoding.UTF8.GetBytes(json);

        var props = new BasicProperties();
        await channel.BasicPublishAsync(exchange: "billing.events",
                                routingKey: "billing.appointment_payment_processed",
                                mandatory: false,
                                basicProperties: props,
                                body: body);
    }

    public override void Dispose()
    {
        base.Dispose();
    }
}
