using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using DocumentsService.Data;

namespace DocumentsService.Infrastructure.Events;

/// <summary>
/// Publishes document domain events to RabbitMQ and also emits user-facing notifications
/// into the shared notifications.events queue (consumed by NotificationService).
/// </summary>
public sealed class RabbitMqEventPublisher : IEventPublisher
{
    private readonly ILogger<RabbitMqEventPublisher> _logger;
    private readonly IServiceProvider _services;
    private readonly IConnection _connection;

    public RabbitMqEventPublisher(ILogger<RabbitMqEventPublisher> logger, IServiceProvider services, IConnection connection)
    {
        _logger = logger;
        _services = services;
        _connection = connection;
    }

    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default)
    {
        try
        {
            // Use injected connection; CreateModel is cheap enough to do per publish or we could pool channels.
            // For now, using per-publish channel is standard enough unless high throughput is needed.
            using var channel = _connection.CreateModel();

            // Publish the raw domain event to an events exchange (fanout for now)
            const string domainExchange = "documents.events";
            channel.ExchangeDeclare(domainExchange, ExchangeType.Fanout, durable: false, autoDelete: false);
            var evtJson = JsonSerializer.Serialize(@event);
            channel.BasicPublish(exchange: domainExchange, routingKey: string.Empty, basicProperties: null, body: Encoding.UTF8.GetBytes(evtJson));

            // Additionally, map certain events to user notifications (single, idempotent-worthy actions)
            TryPublishNotificationIfApplicable(channel, @event);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish event {EventType}", typeof(TEvent).Name);
        }
        return Task.CompletedTask;
    }

    private void TryPublishNotificationIfApplicable<TEvent>(IModel channel, TEvent @event)
    {
        try
        {
            var (recipientUserId, description, type, actionUrl) = MapToNotification(@event);
            var docId = ExtractDocumentId(@event);
            if (recipientUserId is null && !string.IsNullOrWhiteSpace(docId))
            {
                // Try to resolve from DocumentId -> Document.PatientId
                try
                {
                    using var scope = _services.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<DocumentsDbContext>();
                    var doc = db.Documents.Find(docId!);
                    if (doc != null) recipientUserId = doc.PatientId;
                }
                catch { /* best-effort only */ }
            }
            if (recipientUserId is null || description is null)
            {
                _logger.LogDebug("[Docs->Notif] Skipping notification publish for event {EventType} (docId={DocumentId}): recipient or description missing.", typeof(TEvent).Name, docId);
                return;
            }

            var queue = "notifications.events";
            channel.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false);
            var payload = new
            {
                RecipientUserId = recipientUserId,
                Description = description,
                Type = (byte)type,
                SourceService = "documents-service",
                ActionUrl = actionUrl,
                PriorityLevel = (string?)null,
                ExpiresAt = (DateTime?)null,
            };
            var json = JsonSerializer.Serialize(payload);
            _logger.LogInformation("[Docs->Notif] Publishing notification: recipient={RecipientUserId}, type={Type}, docId={DocumentId}, action={ActionUrl}, desc='{Description}'",
                recipientUserId, type, docId, actionUrl, description);
            channel.BasicPublish(exchange: string.Empty, routingKey: queue, basicProperties: null, body: Encoding.UTF8.GetBytes(json));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to publish mapped notification for event {EventType}", typeof(TEvent).Name);
        }
    }

    private static (string? recipientUserId, string? description, int type, string? actionUrl) MapToNotification<TEvent>(TEvent @event)
    {
        // Notification Type mapping:
        // 1 = info (appointments), 2 = success (documents), 3 = warning, 4 = error
        switch (@event)
        {
            // DocumentCreated: generic fallback
            case DocumentsService.Contracts.DocumentCreated created:
                return (
                    created.PatientId,
                    BuildDescriptionForKind(created.Type, null),
                    2,
                    $"/my-documents?documentId={created.DocumentId}"
                );

            // Specific document updates that create a new deliverable
            case DocumentsService.Contracts.LabResultsPosted lab:
                return (
                    // Patient id not present on this event; rely on generic document link only
                    recipientUserId: null,
                    description: lab.ResultCount > 0 ? "You have new lab results" : "New lab results are available",
                    type: 2,
                    actionUrl: $"/my-documents?documentId={lab.DocumentId}&filter=lab-results"
                );

            case DocumentsService.Contracts.PrescriptionIssued rx:
                var med = string.IsNullOrWhiteSpace(rx.Medication) ? null : rx.Medication;
                var rxMsg = med is null ? "You have a new prescription" : $"You have a new prescription: {med}";
                return (
                    recipientUserId: null,
                    description: rxMsg,
                    type: 2,
                    actionUrl: $"/my-documents?documentId={rx.DocumentId}&filter=prescriptions"
                );

            case DocumentsService.Contracts.ReferralAdded referral:
                return (
                    recipientUserId: null,
                    description: "You have a new referral",
                    type: 2,
                    actionUrl: $"/my-documents?documentId={referral.DocumentId}&filter=medical-records"
                );

            case DocumentsService.Contracts.SickLeaveAdded sick:
                return (
                    recipientUserId: null,
                    description: "You have a new sick leave",
                    type: 2,
                    actionUrl: $"/my-documents?documentId={sick.DocumentId}&filter=medical-records"
                );

            default:
                return (null, null, 0, null);
        }
    }

    private static string? ExtractDocumentId<TEvent>(TEvent @event)
        => @event switch
        {
            DocumentsService.Contracts.DocumentCreated e => e.DocumentId,
            DocumentsService.Contracts.VisitNoteAdded e => e.DocumentId,
            DocumentsService.Contracts.PrescriptionIssued e => e.DocumentId,
            DocumentsService.Contracts.LabResultsPosted e => e.DocumentId,
            DocumentsService.Contracts.DocumentAssignedToAppointment e => e.DocumentId,
            DocumentsService.Contracts.ReferralAdded e => e.DocumentId,
            DocumentsService.Contracts.SickLeaveAdded e => e.DocumentId,
            _ => null
        };

    private static string BuildDescriptionForKind(int type, string? extra)
    {
        // Mirrors DocumentsService.Models.DocumentKind values
        return type switch
        {
            5 => "You have new lab results",
            2 => extra is null ? "You have a new prescription" : $"You have a new prescription: {extra}",
            3 => "You have a new referral",
            4 => "You have a new sick leave",
            _ => "You have a new document",
        };
    }

    // No lookup on publisher side; NotificationService uses the recipient if present.
}
