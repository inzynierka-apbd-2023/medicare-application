using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MessagingService.Data;
using MessagingService.Models;

namespace MessagingService.Controllers;

[ApiController]
[Route("api/messaging/[controller]")]
public class ThreadsController : ControllerBase
{
    private readonly MessagingDbContext _db;
    public ThreadsController(MessagingDbContext db) => _db = db;

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateThread([FromBody] CreateThreadRequest req)
    {
        var thread = new MessageThread
        {
            Subject = req.Subject,
            InitiatorId = req.InitiatorId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.MessageThreads.Add(thread);

        // Add initiator as participant
        var initiatorParticipant = new ThreadParticipant
        {
            ThreadId = thread.Id,
            UserId = req.InitiatorId,
            JoinedAt = DateTime.UtcNow
        };
        _db.ThreadParticipants.Add(initiatorParticipant);

        // Add other participants
        foreach (var participantId in req.ParticipantIds)
        {
            if (participantId != req.InitiatorId)
            {
                var participant = new ThreadParticipant
                {
                    ThreadId = thread.Id,
                    UserId = participantId,
                    JoinedAt = DateTime.UtcNow
                };
                _db.ThreadParticipants.Add(participant);
            }
        }

        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = thread.Id }, thread);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var thread = await _db.MessageThreads.FindAsync(id);
        if (thread == null) return NotFound();
        return Ok(thread);
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserThreads(string userId)
    {
        var threads = await _db.ThreadParticipants
            .Where(p => p.UserId == userId && p.IsActive)
            .Include(p => p.ThreadId)
            .Select(p => _db.MessageThreads.First(t => t.Id == p.ThreadId))
            .OrderByDescending(t => t.UpdatedAt)
            .ToListAsync();
        return Ok(threads);
    }

    [HttpPost("{id}/messages")]
    [Authorize]
    public async Task<IActionResult> SendThreadMessage(string id, [FromBody] SendThreadMessageRequest req)
    {
        var thread = await _db.MessageThreads.FindAsync(id);
        if (thread == null) return NotFound();

        // Verify sender is participant
        var isParticipant = await _db.ThreadParticipants
            .AnyAsync(p => p.ThreadId == id && p.UserId == req.SenderId && p.IsActive);
        if (!isParticipant) return Forbid("You are not a participant in this thread");

        var message = new ThreadMessage
        {
            ThreadId = id,
            SenderId = req.SenderId,
            Content = req.Content,
            SentAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _db.ThreadMessages.Add(message);

        // Update thread timestamp
        thread.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(message);
    }

    [HttpGet("{id}/messages")]
    public async Task<IActionResult> GetThreadMessages(string id)
    {
        var messages = await _db.ThreadMessages
            .Where(m => m.ThreadId == id)
            .OrderBy(m => m.SentAt)
            .ToListAsync();
        return Ok(messages);
    }

    [HttpGet("{id}/participants")]
    public async Task<IActionResult> GetThreadParticipants(string id)
    {
        var participants = await _db.ThreadParticipants
            .Where(p => p.ThreadId == id && p.IsActive)
            .ToListAsync();
        return Ok(participants);
    }
}

public record CreateThreadRequest(
    string Subject,
    string InitiatorId,
    List<string> ParticipantIds
);

public record SendThreadMessageRequest(
    string SenderId,
    string Content
);
