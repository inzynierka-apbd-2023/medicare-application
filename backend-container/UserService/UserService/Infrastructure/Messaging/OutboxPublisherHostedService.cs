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
    private IConnection? _conn;
    private IModel? _ch;

    public OutboxPublisherHostedService(IServiceProvider sp, IOptions<RabbitOptions> options)
    {
        _sp = sp;
        _opt = options.Value;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        var f = new ConnectionFactory { HostName = _opt.Host, UserName = _opt.Username, Password = _opt.Password };
        _conn = f.CreateConnection();
        _ch = _conn.CreateModel();
        _ch.ExchangeDeclare(_opt.Exchange, ExchangeType.Topic, durable: true);
    Console.WriteLine($"[OutboxPublisher] Connected to RabbitMQ host='{_opt.Host}', user='{_opt.Username}', exchange='{_opt.Exchange}'");
        return base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
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
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                    continue;
                }
                Console.WriteLine($"[OutboxPublisher] Found {events.Count} pending outbox event(s) to publish...");
                foreach (var evt in events)
                {
                    var body = Encoding.UTF8.GetBytes(evt.PayloadJson);
                    var props = _ch!.CreateBasicProperties();
                    props.ContentType = "application/json";
                    props.DeliveryMode = 2;
                    props.MessageId = evt.Id;
                    _ch.BasicPublish(_opt.Exchange, evt.Type, props, body);
                    Console.WriteLine($"[OutboxPublisher] Published event id={evt.Id} type='{evt.Type}'");
                    // mark as published
                    evt.PublishedAt = DateTime.UtcNow;
                }
                await db.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OutboxPublisher] Error while publishing: {ex.Message}");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
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
