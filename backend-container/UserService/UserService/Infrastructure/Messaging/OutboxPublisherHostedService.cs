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
    private IChannel? _ch;

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
            _ch = await _conn.CreateChannelAsync(cancellationToken: stoppingToken);
            await _ch.ExchangeDeclareAsync(_opt.Exchange, ExchangeType.Topic, durable: true, cancellationToken: stoppingToken);
        }
        catch (Exception ex)
        {
             while (!stoppingToken.IsCancellationRequested && _ch == null)
            {
                try
                {
                     await Task.Delay(5000, stoppingToken);
                     _ch = await _conn.CreateChannelAsync(cancellationToken: stoppingToken);
                     await _ch.ExchangeDeclareAsync(_opt.Exchange, ExchangeType.Topic, durable: true, cancellationToken: stoppingToken);
                }
                catch { }
            }
        }


        while (!stoppingToken.IsCancellationRequested && _ch != null)
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
                foreach (var evt in events)
                {
                    var body = Encoding.UTF8.GetBytes(evt.PayloadJson);
                    var props = new BasicProperties();
                    props.ContentType = "application/json";
                    props.DeliveryMode = DeliveryModes.Persistent; // 2 = Persistent
                    props.MessageId = evt.Id.ToString();
                    
                    await _ch.BasicPublishAsync(_opt.Exchange, evt.Type, true, props, body, stoppingToken);
                    
                    evt.PublishedAt = DateTime.UtcNow;
                }
                await db.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                
                if (_ch == null || _ch.IsClosed)
                {
                    _ch = await _conn.CreateChannelAsync(cancellationToken: stoppingToken);
                }
            }
        }
    }

    public override void Dispose()
    {
        base.Dispose();
    }
}
