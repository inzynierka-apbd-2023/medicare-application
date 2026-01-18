using MediatR;
using MessagingService.Data;
using MessagingService.Models;
using Microsoft.EntityFrameworkCore;

namespace MessagingService.Features.Messages.Queries;

public record ConversationDto(
    Guid Id,
    string ParticipantName,
    string ParticipantType,
    string ParticipantId,
    string? LastMessageContent,
    DateTime UpdatedAt,
    int UnreadCount
);

public record GetConversationsQuery(Guid UserId, string UserType) : IRequest<List<ConversationDto>>;

public record UserProfileInfo(Guid Id, string? FirstName, string? LastName);

public class GetConversationsHandler : IRequestHandler<GetConversationsQuery, List<ConversationDto>>
{
    private readonly MessagingDbContext _db;

    public GetConversationsHandler(MessagingDbContext db)
    {
        _db = db;
    }

    public async Task<List<ConversationDto>> Handle(GetConversationsQuery request, CancellationToken cancellationToken)
    {
        var messages = await _db.Messages
            .Where(m => m.SenderId == request.UserId || m.RecipientId == request.UserId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

        if (!messages.Any())
            return new List<ConversationDto>();

        var grouped = messages
            .GroupBy(m => m.SenderId == request.UserId ? m.RecipientId : m.SenderId)
            .ToList();

        var patientDoctorContacts = await _db.PatientDoctorContacts.ToListAsync(cancellationToken);

        var conversations = grouped
            .Select(g =>
            {
                var otherUserId = g.Key;
                var lastMsg = g.First();
                var unreadCount = g.Count(m => m.RecipientId == request.UserId && !m.IsRead);

                string participantName = "Unknown User";
                
                if (request.UserType.Equals("patient", StringComparison.OrdinalIgnoreCase))
                {
                    var contact = patientDoctorContacts.FirstOrDefault(c => 
                        c.PatientUserId == request.UserId && c.DoctorUserId == otherUserId);
                    if (contact != null && !string.IsNullOrEmpty(contact.DoctorName))
                    {
                        participantName = contact.DoctorName;
                    }
                }
                else
                {
                    var contact = patientDoctorContacts.FirstOrDefault(c => 
                        c.DoctorUserId == request.UserId && c.PatientUserId == otherUserId);
                }
                
                if (participantName == "Unknown User")
                {
                    if (lastMsg.SenderId == request.UserId)
                    {
                        if (!string.IsNullOrEmpty(lastMsg.RecipientName))
                            participantName = lastMsg.RecipientName;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(lastMsg.SenderName))
                            participantName = lastMsg.SenderName;
                    }
                }
                
                if (participantName == "Unknown User")
                {
                    foreach (var msg in g)
                    {
                        if (msg.SenderId == otherUserId && !string.IsNullOrEmpty(msg.SenderName))
                        {
                            participantName = msg.SenderName;
                            break;
                        }
                        if (msg.RecipientId == otherUserId && !string.IsNullOrEmpty(msg.RecipientName))
                        {
                            participantName = msg.RecipientName;
                            break;
                        }
                    }
                }

                var participantType = request.UserType.Equals("patient", StringComparison.OrdinalIgnoreCase)
                    ? "doctor"
                    : "patient";

                return new ConversationDto(
                    Id: otherUserId,
                    ParticipantName: participantName,
                    ParticipantType: participantType,
                    ParticipantId: otherUserId.ToString(),
                    LastMessageContent: lastMsg.Content,
                    UpdatedAt: lastMsg.CreatedAt,
                    UnreadCount: unreadCount
                );
            })
            .ToList();

        return conversations;
    }
}
