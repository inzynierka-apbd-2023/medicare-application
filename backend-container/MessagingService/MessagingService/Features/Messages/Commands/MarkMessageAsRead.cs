using MediatR;
using MessagingService.Data;
using Microsoft.EntityFrameworkCore;

namespace MessagingService.Features.Messages.Commands;

public record MarkMessageAsReadCommand(Guid MessageId, Guid UserId) : IRequest<bool>;

public class MarkMessageAsReadHandler : IRequestHandler<MarkMessageAsReadCommand, bool>
{
    private readonly MessagingDbContext _db;
    private readonly ILogger<MarkMessageAsReadHandler> _logger;

    public MarkMessageAsReadHandler(MessagingDbContext db, ILogger<MarkMessageAsReadHandler> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<bool> Handle(MarkMessageAsReadCommand request, CancellationToken cancellationToken)
    {
        var message = await _db.Messages.FirstOrDefaultAsync(m => m.Id == request.MessageId, cancellationToken);
        
        if (message == null)
        {
            _logger.LogWarning("Message {MessageId} not found when marking as read", request.MessageId);
            return false;
        }

        // Only the recipient can mark as read? Or sender too? Usually recipient.
        // If strict security needed: if (message.RecipientId != request.UserId) ...
        
        if (!message.IsRead)
        {
            message.IsRead = true;
            message.ReadAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Message {MessageId} marked as read by user {UserId}", request.MessageId, request.UserId);
        }

        return true;
    }
}
