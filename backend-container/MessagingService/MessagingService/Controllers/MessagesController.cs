using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MessagingService.Data;
using MessagingService.Models;

namespace MessagingService.Controllers;

[ApiController]
[Route("api/messaging/[controller]")]
public partial class MessagesController : ControllerBase
{
    private readonly MessagingDbContext _db;
    public MessagesController(MessagingDbContext db) => _db = db;

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest req)
    {
        var message = new Message
        {
            SenderId = req.SenderId,
            RecipientId = req.RecipientId,
            Subject = req.Subject,
            Content = req.Content,
            MessageType = req.MessageType ?? "General",
            Priority = req.Priority ?? "Normal",
            SentAt = DateTime.UtcNow,
            RelatedEntityId = req.RelatedEntityId,
            RelatedEntityType = req.RelatedEntityType,
            CreatedAt = DateTime.UtcNow
        };

        _db.Messages.Add(message);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = message.Id }, message);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var message = await _db.Messages.FindAsync(id);
        if (message == null) return NotFound();
        return Ok(message);
    }

    [HttpGet("inbox/{userId}")]
    public async Task<IActionResult> GetInbox(Guid userId)
    {
        var messages = await _db.Messages
            .Where(m => m.RecipientId == userId)
            .OrderByDescending(m => m.SentAt)
            .ToListAsync();
        return Ok(messages);
    }

    [HttpGet("sent/{userId}")]
    public async Task<IActionResult> GetSentMessages(Guid userId)
    {
        var messages = await _db.Messages
            .Where(m => m.SenderId == userId)
            .OrderByDescending(m => m.SentAt)
            .ToListAsync();
        return Ok(messages);
    }

    [HttpGet("unread/{userId}")]
    public async Task<IActionResult> GetUnreadMessages(Guid userId)
    {
        var messages = await _db.Messages
            .Where(m => m.RecipientId == userId && !m.IsRead)
            .OrderByDescending(m => m.SentAt)
            .ToListAsync();
        return Ok(messages);
    }

    [HttpPut("{id}/read")]
    [Authorize]
    public async Task<IActionResult> MarkAsRead(Guid id, [FromBody] MarkAsReadRequest req)
    {
        var message = await _db.Messages.FindAsync(id);
        if (message == null) return NotFound();
        
        if (message.RecipientId != req.UserId)
            return Forbid("You can only mark your own messages as read");

        message.IsRead = true;
        message.ReadAt = DateTime.UtcNow;

        // Create receipt
        var receipt = new MessageReceipt
        {
            MessageId = id,
            UserId = req.UserId,
            ReadAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _db.MessageReceipts.Add(receipt);
        await _db.SaveChangesAsync();

        return Ok(message);
    }

    [HttpGet("conversation/{userId1}/{userId2}")]
    public async Task<IActionResult> GetConversation(Guid userId1, Guid userId2)
    {
        var messages = await _db.Messages
            .Where(m => (m.SenderId == userId1 && m.RecipientId == userId2) ||
                       (m.SenderId == userId2 && m.RecipientId == userId1))
            .OrderBy(m => m.SentAt)
            .ToListAsync();
        return Ok(messages);
    }
}

public record SendMessageRequest(
    Guid SenderId,
    Guid RecipientId,
    string Subject,
    string Content,
    string? MessageType,
    string? Priority,
    Guid? RelatedEntityId,
    string? RelatedEntityType
);

public record MarkAsReadRequest(Guid UserId);

public record ConversationDto(
    Guid ParticipantId,
    string LastMessageContent,
    DateTime LastMessageDate,
    int UnreadCount
);

public partial class MessagesController
{
    [HttpGet("conversations/{userId}")]
    public async Task<IActionResult> GetConversations(Guid userId)
    {
        // Fetch all messages involving the user
        var messages = await _db.Messages
            .AsNoTracking()
            .Where(m => m.SenderId == userId || m.RecipientId == userId)
            .OrderByDescending(m => m.SentAt)
            .ToListAsync();

        // Group by the "other" person
        var grouped = messages
            .GroupBy(m => m.SenderId == userId ? m.RecipientId : m.SenderId)
            .Select(g => 
            {
                var lastMsg = g.First(); // Already ordered by SentAt desc
                var unread = g.Count(m => m.RecipientId == userId && !m.IsRead);
                return new ConversationDto(
                    g.Key,
                    lastMsg.Content,
                    lastMsg.SentAt,
                    unread
                );
            })
            .ToList();

        return Ok(grouped);
    }
}
