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
    public async Task<IActionResult> GetServices([FromQuery] Guid? specializationId)
    {
        var query = _db.Services.AsQueryable();
        if (specializationId.HasValue && specializationId.Value != Guid.Empty)
        {
            query = from s in _db.Services
                    join ss in _db.SpecializationServices on s.Id equals ss.ServiceId
                    where ss.SpecializationId == specializationId.Value
                    select s;
        }
        var items = await query.Select(s => new { s.Id, s.Name, s.Description }).ToListAsync();
        return Ok(items);
    }

    [HttpGet("specializations")]
    public async Task<IActionResult> GetSpecializations([FromQuery] Guid? serviceId)
    {
        var query = _db.Specializations.AsQueryable();
        if (serviceId.HasValue && serviceId.Value != Guid.Empty)
        {
            query = from sp in _db.Specializations
                    join ss in _db.SpecializationServices on sp.Id equals ss.SpecializationId
                    where ss.ServiceId == serviceId.Value
                    select sp;
        }
        // Use Distinct to avoid duplicates when specializations have multiple service mappings
        var items = await query.Select(s => new { s.Id, s.Name }).Distinct().ToListAsync();
        return Ok(items);
    }
}
