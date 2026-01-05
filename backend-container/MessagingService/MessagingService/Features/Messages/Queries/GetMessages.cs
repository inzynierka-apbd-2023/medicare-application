using MediatR;
using MessagingService.Data;
using MessagingService.Models;
using Microsoft.EntityFrameworkCore;

namespace MessagingService.Features.Messages.Queries;

public record GetMessagesQuery(Guid UserId, Guid OtherUserId) : IRequest<List<Message>>;

public class GetMessagesHandler : IRequestHandler<GetMessagesQuery, List<Message>>
{
    private readonly MessagingDbContext _db;

    public GetMessagesHandler(MessagingDbContext db)
    {
        _db = db;
    }

    public async Task<List<Message>> Handle(GetMessagesQuery request, CancellationToken cancellationToken)
    {
        return await _db.Messages
            .Where(m => (m.SenderId == request.UserId && m.RecipientId == request.OtherUserId) ||
                        (m.SenderId == request.OtherUserId && m.RecipientId == request.UserId))
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
