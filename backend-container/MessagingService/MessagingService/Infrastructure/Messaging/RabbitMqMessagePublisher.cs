using System.Text;
using System.Text.Json;
using MessagingService.Models;
using RabbitMQ.Client;

namespace MessagingService.Infrastructure.Messaging;

public interface IMessagePublisher
{
    Task PublishMessageSentAsync(Message message);
}

public class RabbitMqMessagePublisher : IMessagePublisher
{
    private readonly IConnection _connection;
    private const string ExchangeName = "notification.events";

    public RabbitMqMessagePublisher(IConnection connection)
    {
        _connection = connection;
    }

    public async Task PublishMessageSentAsync(Message message)
    {
        using var channel = await _connection.CreateChannelAsync();
        
        await channel.ExchangeDeclareAsync(exchange: ExchangeName, type: ExchangeType.Topic, durable: true);

        var payload = new
        {
            Event = "MessageSent",
            MessageId = message.Id,
            SenderId = message.SenderId,
            RecipientId = message.RecipientId,
            Subject = message.Subject,
            Content = message.Content, // Truncate if sensitive?
            Timestamp = DateTime.UtcNow
        };

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload));
        var props = new BasicProperties
        {
            Persistent = true,
            MessageId = Guid.NewGuid().ToString(),
            CorrelationId = message.Id.ToString()
        };

        await channel.BasicPublishAsync(
            exchange: ExchangeName,
            routingKey: "message.sent",
            mandatory: false,
            basicProperties: props,
            body: body);
    }
}
