using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotificationService.Data;

namespace NotificationService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly NotificationsDbContext _db;

    public NotificationsController(NotificationsDbContext db)
    {
        _db = db;
    }

    public record NotificationDto(
        Guid Id,
        Guid RecipientUserId,
        string? Description,
        byte Type,
        DateTime CreationDate,
        bool IsRead,
        string? ActionUrl
    );

    [HttpGet]
    public async Task<ActionResult<IEnumerable<NotificationDto>>> Get(
        [FromQuery] Guid recipientUserId,
        [FromQuery] bool unreadOnly = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (recipientUserId == Guid.Empty)
        {
            return BadRequest("recipientUserId is required");
        }
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        var q = _db.Notifications.AsNoTracking()
            .Where(n => n.Recipient_User_Id == recipientUserId);
        if (unreadOnly)
        {
            q = q.Where(n => n.Is_Read != true);
        }

        var items = await q
            .OrderByDescending(n => n.Creation_Date)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new NotificationDto(
                n.Id,
                n.Recipient_User_Id,
                n.Description,
                n.Type,
                n.Creation_Date,
                n.Is_Read == true,
                n.Action_Url
            ))
            .ToListAsync();

        return Ok(items);
    }

    [HttpPost("{id}/read")]
    public async Task<IActionResult> MarkAsRead([FromRoute] Guid id)
    {
        if (id == Guid.Empty) return BadRequest();
        var n = await _db.Notifications.FirstOrDefaultAsync(x => x.Id == id);
        if (n == null) return NotFound();
        if (n.Is_Read == true) return NoContent();
        n.Is_Read = true;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
