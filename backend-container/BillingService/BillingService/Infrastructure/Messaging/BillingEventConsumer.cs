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

            _logger.LogInformation("Connected to RabbitMQ and ready to consume messages (UserCreated)");
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
                
                await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message");
                await _channel.BasicNackAsync(ea.DeliveryTag, false, false, stoppingToken); 
            }
        };

        await _channel.BasicConsumeAsync("billing.user_created", false, consumer, cancellationToken: stoppingToken);

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

    public override void Dispose()
    {
        base.Dispose();
    }
}
