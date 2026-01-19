using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.Data;

namespace UserService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly UserDbContext _db;
    public AdminController(UserDbContext db) => _db = db;

    [HttpGet("outbox")] 
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<object>>> Outbox()
    {
        var items = await _db.OutboxEvents
            .OrderByDescending(o => o.OccurredAt)
            .Take(20)
            .Select(o => new { o.Id, o.Type, o.OccurredAt, o.PublishedAt, Payload = o.PayloadJson.Substring(0, Math.Min(200, o.PayloadJson.Length)) })
            .ToListAsync();
        return Ok(items);
    }
}
