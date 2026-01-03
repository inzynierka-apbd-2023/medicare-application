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
    private IModel? _channel;
    private const string NotificationsQueue = "notifications.events";
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
                _channel = _conn.CreateModel();
                
                // Declare queues
                _channel.QueueDeclare(queue: NotificationsQueue, durable: true, exclusive: false, autoDelete: false, arguments: null);
                _channel.QueueDeclare(queue: EmailQueue, durable: true, exclusive: false, autoDelete: false, arguments: null);

                // Consumer for in-app notifications
                var notificationConsumer = new AsyncEventingBasicConsumer(_channel);
                notificationConsumer.Received += HandleNotificationEvent;
                _channel.BasicConsume(queue: NotificationsQueue, autoAck: true, consumer: notificationConsumer);

                // Consumer for email events
                var emailConsumer = new AsyncEventingBasicConsumer(_channel);
                emailConsumer.Received += HandleEmailEvent;
                _channel.BasicConsume(queue: EmailQueue, autoAck: true, consumer: emailConsumer);

                _logger.LogInformation("NotificationConsumerService started on queues: {Queue1}, {Queue2}", 
                    NotificationsQueue, EmailQueue);

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

    private async Task HandleNotificationEvent(object model, BasicDeliverEventArgs ea)
    {
        try
        {
            var json = Encoding.UTF8.GetString(ea.Body.ToArray());
            var evt = JsonSerializer.Deserialize<NotificationEvent>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (evt != null)
            {
                _logger.LogInformation("[Notifications MQ] Received notification: recipient={RecipientUserId}, type={Type}", 
                    evt.RecipientUserId, evt.Type);
                    
                using var scope = _sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<NotificationsDbContext>();
                
                db.Notifications.Add(new NotificationService.Models.Notification
                {
                    Recipient_User_Id = evt.RecipientUserId,
                    Description = evt.Description,
                    Type = evt.Type,
                    Source_Service = evt.SourceService,
                    Action_Url = evt.ActionUrl,
                    Priority_Level = evt.PriorityLevel,
                    Expires_At = evt.ExpiresAt,
                    Creation_Date = DateTime.UtcNow,
                    Is_Read = false
                });
                await db.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing notification message");
        }
    }

    private async Task HandleEmailEvent(object model, BasicDeliverEventArgs ea)
    {
        try
        {
            var json = Encoding.UTF8.GetString(ea.Body.ToArray());
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
        _channel?.Close();
        _channel?.Dispose();
        base.Dispose();
    }
}

public record NotificationEvent(
    Guid RecipientUserId,
    string? Description,
    byte Type,
    string? SourceService,
    string? ActionUrl,
    string? PriorityLevel,
    DateTime? ExpiresAt
);

public record EmailEvent(
    string Type,        // "welcome" or "password_reset"
    string Email,
    string? FirstName,
    string? ResetToken  // Only for password_reset
);
