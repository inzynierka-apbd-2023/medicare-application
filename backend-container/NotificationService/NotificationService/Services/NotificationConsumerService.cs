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
    private const string QueueName = "notifications.events";

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
                // Create channel from injected connection
                _channel = _conn.CreateModel();
                _channel.QueueDeclare(QueueName, durable: true, exclusive: false, autoDelete: false);

                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.Received += async (model, ea) =>
                {
                    try
                    {
                        var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                        var evt = JsonSerializer.Deserialize<NotificationEvent>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (evt != null)
                        {
                            _logger.LogInformation("[Notifications MQ] Received notification: recipient={RecipientUserId}, type={Type}", evt.RecipientUserId, evt.Type);
                            using var scope = _sp.CreateScope();
                            var db = scope.ServiceProvider.GetRequiredService<NotificationsDbContext>();
                            
                            // Map to Entity
                            // Note: Assuming Notification entity has these properties matching Program.cs logic
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
                        _logger.LogError(ex, "Error processing message");
                    }
                };

                _channel.BasicConsume(queue: QueueName, autoAck: true, consumer: consumer);
                _logger.LogInformation("NotificationConsumerService started on queue {Queue}", QueueName);

                // Wait indefinitely (or until cancelled)
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

    public override void Dispose()
    {
        _channel?.Close(); _channel?.Dispose();
        base.Dispose();
    }
}

public record NotificationEvent(
    string RecipientUserId,
    string? Description,
    byte Type,
    string? SourceService,
    string? ActionUrl,
    string? PriorityLevel,
    DateTime? ExpiresAt
);
