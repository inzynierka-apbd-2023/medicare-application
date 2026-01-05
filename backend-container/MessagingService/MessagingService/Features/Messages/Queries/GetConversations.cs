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

// DTO for SQL query
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
        // 1. Get messages where user is sender or recipient
        var messages = await _db.Messages
            .Where(m => m.SenderId == request.UserId || m.RecipientId == request.UserId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

        if (!messages.Any())
            return new List<ConversationDto>();

        // 2. Group by the *other* user
        var grouped = messages
            .GroupBy(m => m.SenderId == request.UserId ? m.RecipientId : m.SenderId)
            .ToList();

        // 3. Try to get names from PatientDoctorContact table first (has proper doctor names)
        var patientDoctorContacts = await _db.PatientDoctorContacts.ToListAsync(cancellationToken);

        // 4. Build conversation DTOs
        var conversations = grouped
            .Select(g =>
            {
                var otherUserId = g.Key;
                var lastMsg = g.First();
                var unreadCount = g.Count(m => m.RecipientId == request.UserId && !m.IsRead);

                // Determine participant name using multiple fallback strategies
                string participantName = "Unknown User";
                
                // Strategy 1: Check PatientDoctorContact table (has enriched names)
                if (request.UserType.Equals("patient", StringComparison.OrdinalIgnoreCase))
                {
                    // Current user is patient, other is doctor
                    var contact = patientDoctorContacts.FirstOrDefault(c => 
                        c.PatientUserId == request.UserId && c.DoctorUserId == otherUserId);
                    if (contact != null && !string.IsNullOrEmpty(contact.DoctorName))
                    {
                        participantName = contact.DoctorName;
                    }
                }
                else
                {
                    // Current user is doctor, other is patient
                    var contact = patientDoctorContacts.FirstOrDefault(c => 
                        c.DoctorUserId == request.UserId && c.PatientUserId == otherUserId);
                    // PatientDoctorContact doesn't store patient names, so we'll fall back
                }
                
                // Strategy 2: Use stored names from messages
                if (participantName == "Unknown User")
                {
                    if (lastMsg.SenderId == request.UserId)
                    {
                        // I sent the last message, other is recipient
                        if (!string.IsNullOrEmpty(lastMsg.RecipientName))
                            participantName = lastMsg.RecipientName;
                    }
                    else
                    {
                        // Other sent the last message
                        if (!string.IsNullOrEmpty(lastMsg.SenderName))
                            participantName = lastMsg.SenderName;
                    }
                }
                
                // Strategy 3: Search all messages in this conversation for a name
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

                // Determine participant type
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
