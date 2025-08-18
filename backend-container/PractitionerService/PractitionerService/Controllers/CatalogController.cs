using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PractitionerService.Data;
using PractitionerService.Models;

namespace PractitionerService.Controllers;

[ApiController]
[Route("api/practitioner/catalog")]
public class CatalogController : ControllerBase
{
    private readonly PractitionerDbContext _db;
    public CatalogController(PractitionerDbContext db) => _db = db;

    [HttpGet("services")]
    public async Task<IActionResult> GetServices()
    {
        var items = await _db.Services.Select(s => new { s.Id, s.Name, s.Description }).ToListAsync();
        return Ok(items);
    }

    [HttpGet("specializations")]
    public async Task<IActionResult> GetSpecializations([FromQuery] Guid? serviceId)
    {
        // For now, independent of service; extend later with mapping table
        var items = await _db.Specializations.Select(s => new { s.Id, s.Name }).ToListAsync();
        return Ok(items);
    }
}
