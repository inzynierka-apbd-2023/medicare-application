using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PractitionerService.Data;
using PractitionerService.Models;

namespace PractitionerService.Controllers;

[ApiController]
[Route("api/practitioner/[controller]")]
[Authorize]
public class ReceptionistsController : ControllerBase
{
    private readonly PractitionerDbContext _db;

    public ReceptionistsController(PractitionerDbContext db) => _db = db;

    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegisterReceptionistRequest req)
    {
        if (req.UserId == Guid.Empty) return BadRequest("UserId is required");
        if (await _db.Receptionists.AnyAsync(r => r.UserId == req.UserId)) return Conflict("Receptionist already registered for this user");
        
        var rec = new Receptionist 
        { 
            Id = Guid.NewGuid(),
            UserId = req.UserId, 
            CreatedAt = DateTime.UtcNow, 
            UpdatedAt = DateTime.UtcNow 
        };
        
        _db.Receptionists.Add(rec);
        await _db.SaveChangesAsync();
        // TODO: publish ReceptionistRegistered
        return CreatedAtAction(nameof(GetReceptionistById), new { id = rec.Id }, new { rec.Id, rec.UserId });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetReceptionistById(Guid id)
    {
        var receptionist = await _db.Receptionists.FindAsync(id);
        if (receptionist == null) return NotFound();
        return Ok(receptionist);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var receptionists = await _db.Receptionists.ToListAsync();
        return Ok(receptionists);
    }
}

public record RegisterReceptionistRequest(Guid UserId);
