using MediatR;
using MessagingService.Data;
using MessagingService.Models;
using Microsoft.EntityFrameworkCore;

namespace MessagingService.Features.Messages.Commands;

public record SendMessageCommand(
    Guid SenderId, 
    Guid RecipientId, 
    string Content, 
    string Subject,
    string? SenderName = null,
    string? RecipientName = null,
    string? RelatedEntityId = null,
    string? RelatedEntityType = null
) : IRequest<Message>;

public class SendMessageHandler : IRequestHandler<SendMessageCommand, Message>
{
    private readonly MessagingDbContext _db;
    private readonly MessagingService.Infrastructure.Messaging.IMessagePublisher _publisher;

    public SendMessageHandler(MessagingDbContext db, MessagingService.Infrastructure.Messaging.IMessagePublisher publisher)
    {
        _db = db;
        _publisher = publisher;
    }

    public async Task<Message> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        var msg = new Message
        {
            Id = Guid.NewGuid(),
            SenderId = request.SenderId,
            RecipientId = request.RecipientId,
            Subject = request.Subject,
            Content = request.Content,
            MessageType = "General",
            Priority = "Normal",
            SentAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            IsRead = false,
            RelatedEntityType = request.RelatedEntityType,
            SenderName = request.SenderName,
            RecipientName = request.RecipientName
        };

        if (Guid.TryParse(request.RelatedEntityId, out var relId))
        {
            msg.RelatedEntityId = relId;
        }

        _db.Messages.Add(msg);
        await _db.SaveChangesAsync(cancellationToken);

        await _publisher.PublishMessageSentAsync(msg);

        return msg;
    }
}
