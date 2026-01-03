using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using UserService.Data;
using UserService.Infrastructure.Messaging;

namespace UserService.Infrastructure.Messaging;

public class RabbitOptions
{
    public string Host { get; set; } = "rabbitmq";
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string Exchange { get; set; } = "user.events";
}

public class OutboxPublisherHostedService : BackgroundService
{
    private readonly IServiceProvider _sp;
    private readonly RabbitOptions _opt;
    private readonly IConnection _conn;
    private IModel? _ch;

    private readonly IConfiguration _config;

    public OutboxPublisherHostedService(IServiceProvider sp, IOptions<RabbitOptions> options, IConfiguration config, IConnection conn)
    {
        _sp = sp;
        _opt = options.Value;
        _config = config;
        _conn = conn;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _ch = _conn.CreateModel();
            _ch.ExchangeDeclare(_opt.Exchange, ExchangeType.Topic, durable: true);
            Console.WriteLine($"[OutboxPublisher] Connected to RabbitMQ exchange='{_opt.Exchange}'");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OutboxPublisher] Failed to setup RabbitMQ channel: {ex.Message}");
            // Simple retry loop for channel creation
             while (!stoppingToken.IsCancellationRequested && _ch == null)
            {
                try
                {
                     await Task.Delay(5000, stoppingToken);
                     _ch = _conn.CreateModel();
                     _ch.ExchangeDeclare(_opt.Exchange, ExchangeType.Topic, durable: true);
                }
                catch { }
            }
        }


        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();
                var events = await db.OutboxEvents
                    .Where(e => e.PublishedAt == null)
                    .OrderBy(e => e.OccurredAt)
                    .Take(50)
                    .ToListAsync(stoppingToken);
                if (events.Count == 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                    continue;
                }
                Console.WriteLine($"[OutboxPublisher] 📬 Found {events.Count} pending outbox event(s) to publish...");
                foreach (var evt in events)
                {
                    var body = Encoding.UTF8.GetBytes(evt.PayloadJson);
                    var props = _ch!.CreateBasicProperties();
                    props.ContentType = "application/json";
                    props.DeliveryMode = 2;
                    props.MessageId = evt.Id.ToString();
                    _ch.BasicPublish(_opt.Exchange, evt.Type, props, body);
                    Console.WriteLine($"[OutboxPublisher] ✅ Published event id={evt.Id} type='{evt.Type}' to exchange='{_opt.Exchange}' routingKey='{evt.Type}'");
                    Console.WriteLine($"[OutboxPublisher] 📄 Payload: {evt.PayloadJson}");
                    // mark as published
                    evt.PublishedAt = DateTime.UtcNow;
                }
                await db.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OutboxPublisher] Error while publishing: {ex.Message}");
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }

    public override void Dispose()
    {
        _ch?.Dispose();
        _conn?.Dispose();
        base.Dispose();
    }
}
