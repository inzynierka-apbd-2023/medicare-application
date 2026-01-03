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

public class BillingEventConsumer : BackgroundService
{
    private readonly IServiceProvider _sp;
    private readonly ILogger<BillingEventConsumer> _logger;
    private readonly IConnection _conn;
    private IModel? _channel;

    public BillingEventConsumer(IServiceProvider sp, IConnection conn, ILogger<BillingEventConsumer> logger)
    {
        _sp = sp;
        _conn = conn;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Use the Aspire-provided connection
        Console.WriteLine("[BillingConsumer] 🔌 Attempting to connect to RabbitMQ...");
        try
        {
            _channel = _conn.CreateModel();
            Console.WriteLine("[BillingConsumer] ✅ Created RabbitMQ channel");
            
            _channel.ExchangeDeclare("user.events", ExchangeType.Topic, durable: true);
            Console.WriteLine("[BillingConsumer] ✅ Declared exchange 'user.events'");
            
            _channel.QueueDeclare("billing.user_created", durable: true, exclusive: false, autoDelete: false);
            Console.WriteLine("[BillingConsumer] ✅ Declared queue 'billing.user_created'");
            
            _channel.QueueBind("billing.user_created", "user.events", "user.created");
            Console.WriteLine("[BillingConsumer] ✅ Bound queue to exchange with routing key 'user.created'");
            
            _logger.LogInformation("Connected to RabbitMQ user.created");
            Console.WriteLine("[BillingConsumer] 🎉 Successfully connected and ready to consume messages!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BillingConsumer] ❌ Failed to setup RabbitMQ channel: {ex.Message}");
            _logger.LogError(ex, "Failed to setup RabbitMQ channel");
            return;
        }

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var msg = Encoding.UTF8.GetString(body);
                Console.WriteLine($"[BillingConsumer] 📨 Received message: {msg}");
                
                var evt = JsonSerializer.Deserialize<UserRegistered>(msg, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (evt != null)
                {
                    Console.WriteLine($"[BillingConsumer] 📋 Parsed event - UserId: {evt.UserId}, PlanId: '{evt.PlanId ?? "(null)"}'");
                    await HandleUserCreatedAsync(evt);
                }
                else
                {
                    Console.WriteLine($"[BillingConsumer] ⚠️ Failed to parse event from message");
                }
                
                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message");
                _channel.BasicNack(ea.DeliveryTag, false, false); // or true to requeue
            }
        };

        _channel.BasicConsume("billing.user_created", false, consumer);

        // Keep running
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task HandleUserCreatedAsync(UserRegistered evt)
    {
        Console.WriteLine($"[BillingConsumer] 🔄 HandleUserCreatedAsync called for user {evt.UserId}");
        
        using var scope = _sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BillingDbContext>();

        // Check if subscription already exists (idempotency)
        var exists = await db.SubscriptionContracts.AnyAsync(s => s.PatientId == evt.UserId);
        if (exists)
        {
            Console.WriteLine($"[BillingConsumer] ⏭️ Subscription for user {evt.UserId} already exists. Skipping.");
            _logger.LogInformation($"Subscription for user {evt.UserId} already exists. Skipping.");
            return;
        }

        var planCode = evt.PlanId ?? "FREE"; // Default to FREE if not provided
        Console.WriteLine($"[BillingConsumer] 📝 PlanCode from event: '{evt.PlanId}' -> Using: '{planCode}'");
        
        var plan = await db.Plans.FindAsync(planCode);
        
        if (plan == null)
        {
            Console.WriteLine($"[BillingConsumer] ⚠️ Plan '{planCode}' not found in database! Defaulting to FREE.");
            _logger.LogWarning($"Plan {planCode} not found. Defaulting to FREE.");
            planCode = "FREE";
            plan = await db.Plans.FindAsync("FREE");
        }
        else
        {
            Console.WriteLine($"[BillingConsumer] ✅ Found plan: {plan.Name} (Code: {plan.Code})");
        }

        // Determine dates
        var now = DateTime.UtcNow;
        var end = plan?.BillingPeriod == "yearly" ? now.AddYears(1) : now.AddMonths(1);

        // Determine status - if free, Active. If paid, maybe Active (demo) or Pending.
        // For this demo/MVP, let's auto-activate "FREE" and "PAY PER VISIT", 
        // but maybe also "BASIC" and "PREMIUM" for simplicity unless payment is strictly required.
        // Given user instructions "Create new subscription logic", let's make it Active.
        
        var sub = new SubscriptionContract
        {
            Id = Guid.NewGuid(),
            PatientId = evt.UserId,
            PlanCode = planCode,
            PeriodStart = now,
            PeriodEnd = end,
            Status = SubscriptionStatus.Active, // Auto-activate for demo
            DefaultPaymentMethodId = null // No payment method yet
        };

        db.SubscriptionContracts.Add(sub);
        await db.SaveChangesAsync();
        
        Console.WriteLine($"[BillingConsumer] ✅✅ CREATED subscription {sub.Id} for user {evt.UserId} with plan '{planCode}'");
        _logger.LogInformation($"Created subscription {sub.Id} for user {evt.UserId} with plan {planCode}");
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        // Note: _conn is managed by Aspire DI, don't dispose it here
        base.Dispose();
    }
}
