using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using NotificationService.Data;
using Microsoft.Extensions.DependencyInjection;

namespace NotificationService.Services;

public class NotificationConsumerService : BackgroundService
{
    private readonly ILogger<NotificationConsumerService> _logger;
    private readonly IServiceProvider _sp;
    private readonly IConnection _conn;
    private IChannel? _channel;
    private const string EmailQueue = "email.events";

    public NotificationConsumerService(ILogger<NotificationConsumerService> logger, IServiceProvider sp, IConnection conn)
    {
        _logger = logger;
        _sp = sp;
        _conn = conn;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _channel = await _conn.CreateChannelAsync(cancellationToken: stoppingToken);
                
                await _channel.QueueDeclareAsync(queue: EmailQueue, durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: stoppingToken);

                // Consumer for email events
                var emailConsumer = new AsyncEventingBasicConsumer(_channel);
                emailConsumer.ReceivedAsync += HandleEmailEvent;
                await _channel.BasicConsumeAsync(queue: EmailQueue, autoAck: true, consumer: emailConsumer, cancellationToken: stoppingToken);

                _logger.LogInformation("NotificationConsumerService started on queue: {Queue}", EmailQueue);

                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "NotificationConsumerService failed, retrying in 5s");
                await Task.Delay(5000, stoppingToken);
            }
        }
    }

    private async Task HandleEmailEvent(object model, BasicDeliverEventArgs ea)
    {
        try
        {
            var json = Encoding.UTF8.GetString(ea.Body.Span);
            _logger.LogInformation("[Email MQ] Received email event: {Json}", json);
            
            var evt = JsonSerializer.Deserialize<EmailEvent>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (evt == null) return;

            using var scope = _sp.CreateScope();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            switch (evt.Type)
            {
                case "welcome":
                    await emailService.SendWelcomeEmailAsync(evt.Email, evt.FirstName ?? "User");
                    break;
                    
                case "password_reset":
                    if (!string.IsNullOrEmpty(evt.ResetToken))
                    {
                        await emailService.SendPasswordResetEmailAsync(evt.Email, evt.FirstName ?? "User", evt.ResetToken);
                    }
                    break;
                    
                default:
                    _logger.LogWarning("Unknown email event type: {Type}", evt.Type);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing email message");
        }
    }

    public override void Dispose()
    {
        // _channel?.Dispose(); // Not sync disposable
        base.Dispose();
    }
}

public record EmailEvent(
    string Type,        // "welcome" or "password_reset"
    string Email,
    string? FirstName,
    string? ResetToken  // Only for password_reset
);
