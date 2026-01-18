using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MessagingService.Features.Messages.Commands;
using MessagingService.Features.Messages.Queries;

namespace MessagingService.Controllers;

[ApiController]
[Route("api/messaging/[controller]")]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly IMediator _mediator;

    public MessagesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest req)
    {
        var command = new SendMessageCommand(
            req.SenderId,
            req.RecipientId,
            req.Content,
            req.Subject,
            req.SenderName,
            req.RecipientName,
            req.RelatedEntityId?.ToString(),
            req.RelatedEntityType
        );

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpGet("conversations/{userId}")]
    public async Task<IActionResult> GetConversations(Guid userId, [FromQuery] string userType = "patient")
    {
        var query = new GetConversationsQuery(userId, userType);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("conversation/{userId1}/{userId2}")]
    public async Task<IActionResult> GetMessages(Guid userId1, Guid userId2)
    {
        var query = new GetMessagesQuery(userId1, userId2);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("recipients/{userId}")]
    public async Task<IActionResult> GetAvailableRecipients(Guid userId, [FromQuery] string userRole = "patient")
    {
        var query = new GetAvailableRecipientsQuery(userId, userRole);
        var result = await _mediator.Send(query);
        return Ok(result);
    }
    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id, [FromBody] MarkAsReadRequest req)
    {
        var command = new MarkMessageAsReadCommand(id, req.UserId);
        var result = await _mediator.Send(command);
        if (!result) return NotFound();
        return Ok(new { Success = true });
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
    string? RelatedEntityType,
    string? SenderName,
    string? RecipientName
);

public record MarkAsReadRequest(Guid UserId);
