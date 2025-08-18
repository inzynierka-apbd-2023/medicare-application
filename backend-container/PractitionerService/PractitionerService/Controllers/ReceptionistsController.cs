using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PractitionerService.Data;
using PractitionerService.Models;

namespace PractitionerService.Controllers;

[ApiController]
[Route("api/practitioner/[controller]")]
public class ReceptionistsController : ControllerBase
{
    private readonly PractitionerDbContext _db;

    public ReceptionistsController(PractitionerDbContext db) => _db = db;

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Register([FromBody] RegisterReceptionistRequest req)
    {
        if (req.UserId == Guid.Empty) return BadRequest("UserId is required");
        var userIdStr = req.UserId.ToString();
        if (await _db.Receptionists.AnyAsync(r => r.UserId == userIdStr)) return Conflict("Receptionist already registered for this user");
        var rec = new Receptionist { UserId = userIdStr, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        _db.Receptionists.Add(rec);
        await _db.SaveChangesAsync();
        // TODO: publish ReceptionistRegistered
        return Created($"/api/receptionists/{rec.Id}", new { rec.Id, rec.UserId });
    }
}

public record RegisterReceptionistRequest(Guid UserId);
