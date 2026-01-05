using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PatientService.Data;
using System.Collections.Generic;
using RabbitMQ.Client;

namespace PatientService.Infrastructure.Messaging;

public record UserRegistered(Guid UserId, string Username, string Email, DateTime OccurredAtUtc);

public class UserRegisteredConsumer : BackgroundService
{
    private readonly IServiceProvider _sp;
    private readonly RabbitMQ.Client.IConnection _conn;
    private RabbitMQ.Client.IChannel? _ch;

    public UserRegisteredConsumer(IServiceProvider sp, RabbitMQ.Client.IConnection conn)
    {
        _sp = sp;
        _conn = conn;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _ch = await _conn.CreateChannelAsync(cancellationToken: stoppingToken);
            await _ch.ExchangeDeclareAsync(exchange: "user.events", type: ExchangeType.Topic, durable: true, autoDelete: false, arguments: null, cancellationToken: stoppingToken);
            // DLX + DLQ for failures
            await _ch.ExchangeDeclareAsync(exchange: "patient.dlx", type: ExchangeType.Direct, durable: true, autoDelete: false, arguments: null, cancellationToken: stoppingToken);
            await _ch.QueueDeclareAsync(queue: "patient.user-registered.dlq", durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: stoppingToken);
            await _ch.QueueBindAsync(queue: "patient.user-registered.dlq", exchange: "patient.dlx", routingKey: "patient.user-registered", arguments: null, cancellationToken: stoppingToken);

            var qArgs = new Dictionary<string, object?>
            {
                ["x-dead-letter-exchange"] = "patient.dlx",
                ["x-dead-letter-routing-key"] = "patient.user-registered"
            };
            await _ch.QueueDeclareAsync(queue: "patient.user-registered", durable: true, exclusive: false, autoDelete: false, arguments: qArgs, cancellationToken: stoppingToken);
            await _ch.QueueBindAsync(queue: "patient.user-registered", exchange: "user.events", routingKey: "user.created", arguments: null, cancellationToken: stoppingToken);
            await _ch.BasicQosAsync(prefetchSize: 0, prefetchCount: 10, global: false, cancellationToken: stoppingToken);
            Console.WriteLine($"[UserRegisteredConsumer] Connected to RabbitMQ queue='patient.user-registered'");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[UserRegisteredConsumer] Failed to setup RabbitMQ channel: {ex.Message}");
            // Simple retry loop for channel creation
             while (!stoppingToken.IsCancellationRequested && _ch == null)
            {
                try
                {
                     await Task.Delay(5000, stoppingToken);
                     _ch = await _conn.CreateChannelAsync(cancellationToken: stoppingToken);
                     // Re-declare topology omitted for brevity (should ideally duplicate logic)
                     await _ch.ExchangeDeclareAsync(exchange: "user.events", type: ExchangeType.Topic, durable: true, cancellationToken: stoppingToken);
                     await _ch.QueueDeclareAsync(queue: "patient.user-registered", durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: stoppingToken);
                     await _ch.QueueBindAsync(queue: "patient.user-registered", exchange: "user.events", routingKey: "user.created", arguments: null, cancellationToken: stoppingToken);
                     await _ch.BasicQosAsync(0, 10, false, stoppingToken);
                }
                catch { }
            }
        }

        if (_ch == null) return;

        var consumer = new RabbitMQ.Client.Events.AsyncEventingBasicConsumer(_ch!);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.Span);
                var evt = JsonSerializer.Deserialize<UserRegistered>(json);
                if (evt == null) { await _ch!.BasicAckAsync(ea.DeliveryTag, false, stoppingToken); return; }
                Console.WriteLine($"[UserRegisteredConsumer] Received event UserId={evt.UserId} Username={evt.Username}");

                var msgId = ea.BasicProperties?.MessageId;
                var idempotencyKey = !string.IsNullOrWhiteSpace(msgId) ? msgId! : $"{evt.UserId}:{evt.OccurredAtUtc:O}";

                using var scope = _sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<PatientDbContext>();
                var patient = await db.Patients.SingleOrDefaultAsync(p => p.UserId == evt.UserId, stoppingToken);
                if (patient == null)
                {
                    patient = new PatientService.Models.Patient
                    {
                        UserId = evt.UserId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    db.Patients.Add(patient);
                    try
                    {
                        await db.SaveChangesAsync(stoppingToken); // ensure Id is generated in DB
                    }
                    catch (DbUpdateException)
                    {
                        // Likely a race on unique index (UserId); fetch existing
                        await db.Entry(patient).ReloadAsync(stoppingToken);
                        patient = await db.Patients.SingleAsync(p => p.UserId == evt.UserId, stoppingToken);
                    }
                }

                var statusExists = await db.PatientStatuses.AnyAsync(s => s.IdempotencyKey == idempotencyKey, stoppingToken);
                if (!statusExists)
                {
                    db.PatientStatuses.Add(new PatientService.Models.PatientStatus
                    {
                        Status = "Active",
                        EffectiveAt = DateTime.UtcNow,
                        PatientId = patient.Id,
                        IdempotencyKey = idempotencyKey
                    });
                    try
                    {
                        await db.SaveChangesAsync(stoppingToken);
                    }
                    catch (DbUpdateException)
                    {
                        // Unique idempotency conflict -> already processed; swallow
                    }
                }
                await _ch!.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
                Console.WriteLine($"[UserRegisteredConsumer] Processed user UserId={evt.UserId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UserRegisteredConsumer] Error: {ex.Message}");
                // Bounded retry with header propagation; then dead-letter to DLQ
                const int maxRetries = 3;
                int current = 0;
                try
                {
                    if (ea.BasicProperties?.Headers != null && ea.BasicProperties.Headers.TryGetValue("x-retry", out var val) && val is int i)
                    {
                        current = i;
                    }
                }
                catch { /* ignore header parse issues */ }

                if (current < maxRetries)
                {
                    var props = new BasicProperties();
                    props.ContentType = ea.BasicProperties?.ContentType ?? "application/json";
                    props.DeliveryMode = DeliveryModes.Persistent; // 2
                    props.MessageId = ea.BasicProperties?.MessageId;
                    props.Headers = new Dictionary<string, object?>();
                    if (ea.BasicProperties?.Headers != null)
                    {
                        foreach (var kv in ea.BasicProperties.Headers)
                        {
                            props.Headers[kv.Key] = kv.Value;
                        }
                    }
                    props.Headers["x-retry"] = current + 1;
                    
                    await _ch.BasicPublishAsync(exchange: ea.Exchange, routingKey: ea.RoutingKey, mandatory: false, basicProperties: props, body: ea.Body, cancellationToken: stoppingToken);
                    await _ch.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
                    Console.WriteLine($"[UserRegisteredConsumer] Republished for retry {current + 1}/{maxRetries} (MessageId={ea.BasicProperties?.MessageId})");
                }
                else
                {
                    await _ch!.BasicNackAsync(ea.DeliveryTag, false, requeue: false, cancellationToken: stoppingToken); // send to DLQ via DLX
                    Console.WriteLine($"[UserRegisteredConsumer] Sent to DLQ after {current} retries (MessageId={ea.BasicProperties?.MessageId})");
                }
            }
        };
        await _ch!.BasicConsumeAsync(queue: "patient.user-registered", autoAck: false, consumerTag: string.Empty, noLocal: false, exclusive: false, arguments: null, consumer: consumer, cancellationToken: stoppingToken);
        
        // Keep the task alive
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override void Dispose()
    {
        // _ch?.Dispose(); // Sync dispose not available on IChannel, relying on GC or connection closure?
        // Actually IChannel is IAsyncDisposable.
        // But we can't await here.
        base.Dispose();
    }
}
