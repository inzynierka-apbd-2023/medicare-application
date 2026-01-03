using BillingService.Data;
using BillingService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace BillingService.Infrastructure.Messaging;

public class RabbitOptions
{
    public string Host { get; set; } = "rabbitmq";
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string Exchange { get; set; } = "user.events";
    public string Queue { get; set; } = "billing.user_created";
}

// Minimal record matching the one published by UserService
public record UserRegistered(Guid UserId, string Username, string Email, DateTime OccurredAtUtc, string? PlanId);

public class BillingEventConsumer : BackgroundService
{
    private readonly IServiceProvider _sp;
    private readonly RabbitOptions _opt;
    private readonly ILogger<BillingEventConsumer> _logger;
    private IConnection? _conn;
    private IModel? _channel;

    public BillingEventConsumer(IServiceProvider sp, IOptions<RabbitOptions> options, ILogger<BillingEventConsumer> logger)
    {
        _sp = sp;
        _opt = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _opt.Host,
            UserName = _opt.Username,
            Password = _opt.Password,
            DispatchConsumersAsync = true
        };

        // Retry loop for connection
        while (!stoppingToken.IsCancellationRequested && _conn == null)
        {
            try
            {
                _conn = factory.CreateConnection();
                _channel = _conn.CreateModel();
                
                _channel.ExchangeDeclare(_opt.Exchange, ExchangeType.Topic, durable: true);
                _channel.QueueDeclare(_opt.Queue, durable: true, exclusive: false, autoDelete: false);
                _channel.QueueBind(_opt.Queue, _opt.Exchange, "user.created");
                
                _logger.LogInformation("Connected to RabbitMQ user.created");
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Failed to connect to RabbitMQ: {ex.Message}. Retrying...");
                await Task.Delay(5000, stoppingToken);
            }
        }

        if (_conn == null) return;

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var msg = Encoding.UTF8.GetString(body);
                var evt = JsonSerializer.Deserialize<UserRegistered>(msg);

                if (evt != null)
                {
                    await HandleUserCreatedAsync(evt);
                }
                
                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message");
                _channel.BasicNack(ea.DeliveryTag, false, false); // or true to requeue
            }
        };

        _channel.BasicConsume(_opt.Queue, false, consumer);

        // Keep running
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task HandleUserCreatedAsync(UserRegistered evt)
    {
        using var scope = _sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BillingDbContext>();

        // Check if subscription already exists (idempotency)
        var exists = await db.SubscriptionContracts.AnyAsync(s => s.PatientId == evt.UserId);
        if (exists)
        {
            _logger.LogInformation($"Subscription for user {evt.UserId} already exists. Skipping.");
            return;
        }

        var planCode = evt.PlanId ?? "FREE"; // Default to FREE if not provided
        var plan = await db.Plans.FindAsync(planCode);
        
        if (plan == null)
        {
            _logger.LogWarning($"Plan {planCode} not found. Defaulting to FREE.");
            planCode = "FREE";
            plan = await db.Plans.FindAsync("FREE");
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
        _logger.LogInformation($"Created subscription {sub.Id} for user {evt.UserId} with plan {planCode}");
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _conn?.Dispose();
        base.Dispose();
    }
}
