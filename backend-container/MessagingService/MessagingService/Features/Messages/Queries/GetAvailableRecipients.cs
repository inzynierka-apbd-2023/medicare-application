using MediatR;
using MessagingService.Data;
using Microsoft.EntityFrameworkCore;

namespace MessagingService.Features.Messages.Queries;

public record GetAvailableRecipientsQuery(Guid UserId, string UserRole) : IRequest<List<RecipientDto>>;

public record RecipientDto(Guid Id, string Name, string Type, string? Specialization);

public class GetAvailableRecipientsHandler : IRequestHandler<GetAvailableRecipientsQuery, List<RecipientDto>>
{
    private readonly MessagingDbContext _db;
    private readonly ILogger<GetAvailableRecipientsHandler> _logger;

    public GetAvailableRecipientsHandler(MessagingDbContext db, ILogger<GetAvailableRecipientsHandler> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<RecipientDto>> Handle(GetAvailableRecipientsQuery request, CancellationToken cancellationToken)
    {
        var existingConversationUserIds = await _db.Messages
            .Where(m => m.SenderId == request.UserId || m.RecipientId == request.UserId)
            .Select(m => m.SenderId == request.UserId ? m.RecipientId : m.SenderId)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (request.UserRole.Equals("patient", StringComparison.OrdinalIgnoreCase))
        {
            var contacts = await _db.PatientDoctorContacts
                .Where(c => c.PatientUserId == request.UserId)
                .Where(c => !existingConversationUserIds.Contains(c.DoctorUserId))
                .GroupBy(c => c.DoctorUserId)
                .Select(g => g.OrderByDescending(c => c.LastContactAt).First())
                .ToListAsync(cancellationToken);

            return contacts.Select(c => new RecipientDto(
                c.DoctorUserId,
                c.DoctorName ?? "Doctor",
                "doctor",
                c.DoctorSpecialization
            )).ToList();
        }
        else if (request.UserRole.Equals("doctor", StringComparison.OrdinalIgnoreCase))
        {
            var contacts = await _db.PatientDoctorContacts
                .Where(c => c.DoctorUserId == request.UserId)
                .Where(c => !existingConversationUserIds.Contains(c.PatientUserId))
                .GroupBy(c => c.PatientUserId)
                .Select(g => g.OrderByDescending(c => c.LastContactAt).First())
                .ToListAsync(cancellationToken);

            var recipients = new List<RecipientDto>();
            foreach (var contact in contacts)
            {
                string patientName = $"Patient {contact.PatientUserId.ToString()[..8]}";
                
                try
                {
                    var profile = await _db.Database.SqlQueryRaw<UserProfileNameDto>(
                        "SELECT FirstName, LastName FROM [user].[User_Profile] WHERE Id = {0}",
                        contact.PatientUserId)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (profile != null && (!string.IsNullOrEmpty(profile.FirstName) || !string.IsNullOrEmpty(profile.LastName)))
                    {
                        patientName = $"{profile.FirstName} {profile.LastName}".Trim();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not fetch name for patient {PatientId}", contact.PatientUserId);
                }

                recipients.Add(new RecipientDto(
                    contact.PatientUserId,
                    patientName,
                    "patient",
                    null
                ));
            }

            return recipients;
        }

        return new List<RecipientDto>();
    }
}

public record UserProfileNameDto(string? FirstName, string? LastName);
