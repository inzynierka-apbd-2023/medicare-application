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
    private const string UnknownUser = "Unknown User";
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
        var isPatient = request.UserType.Equals("patient", StringComparison.OrdinalIgnoreCase);

        return grouped
            .Select(g => BuildConversationDto(g, request.UserId, isPatient, patientDoctorContacts))
            .ToList();
    }

    private static ConversationDto BuildConversationDto(
        IGrouping<Guid, Message> messageGroup,
        Guid requestUserId,
        bool isPatient,
        List<PatientDoctorContact> contacts)
    {
        var otherUserId = messageGroup.Key;
        var lastMessage = messageGroup.First();
        var unreadCount = messageGroup.Count(m => m.RecipientId == requestUserId && !m.IsRead);

        var participantName = ResolveParticipantName(messageGroup, otherUserId, requestUserId, isPatient, contacts);
        var participantType = isPatient ? "doctor" : "patient";

        return new ConversationDto(
            Id: otherUserId,
            ParticipantName: participantName,
            ParticipantType: participantType,
            ParticipantId: otherUserId.ToString(),
            LastMessageContent: lastMessage.Content,
            UpdatedAt: lastMessage.CreatedAt,
            UnreadCount: unreadCount
        );
    }

    private static string ResolveParticipantName(
        IGrouping<Guid, Message> messageGroup,
        Guid otherUserId,
        Guid requestUserId,
        bool isPatient,
        List<PatientDoctorContact> contacts)
    {
        return TryGetNameFromContacts(otherUserId, requestUserId, isPatient, contacts)
            ?? TryGetNameFromLastMessage(messageGroup.First(), requestUserId)
            ?? TryGetNameFromAllMessages(messageGroup, otherUserId)
            ?? UnknownUser;
    }

    private static string? TryGetNameFromContacts(
        Guid otherUserId,
        Guid requestUserId,
        bool isPatient,
        List<PatientDoctorContact> contacts)
    {
        if (!isPatient)
            return null;

        var contact = contacts.FirstOrDefault(c => c.PatientUserId == requestUserId && c.DoctorUserId == otherUserId);

        return !string.IsNullOrEmpty(contact?.DoctorName) ? contact.DoctorName : null;
    }

    private static string? TryGetNameFromLastMessage(Message lastMessage, Guid requestUserId)
    {
        var name = lastMessage.SenderId == requestUserId
            ? lastMessage.RecipientName
            : lastMessage.SenderName;

        return string.IsNullOrEmpty(name) ? null : name;
    }

    private static string? TryGetNameFromAllMessages(IEnumerable<Message> messages, Guid otherUserId)
    {
        foreach (var message in messages)
        {
            if (message.SenderId == otherUserId && !string.IsNullOrEmpty(message.SenderName))
                return message.SenderName;

            if (message.RecipientId == otherUserId && !string.IsNullOrEmpty(message.RecipientName))
                return message.RecipientName;
        }

        return null;
    }
}
