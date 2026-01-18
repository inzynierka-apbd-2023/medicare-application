using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PatientService.Data;
using PatientService.Models;
using Microsoft.AspNetCore.Authorization;
namespace PatientService.Controllers;

[ApiController]
[Route("api/patient/[controller]")]
[Authorize]
public class OverviewController : ControllerBase
{
    private readonly PatientDbContext _db;
    public OverviewController(PatientDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? q)
    {
        var query = _db.Set<PatientOverview>().AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            var ql = q.ToLowerInvariant();
            query = query.Where(p => (p.FirstName ?? "").ToLower().Contains(ql) || (p.LastName ?? "").ToLower().Contains(ql));
        }
        var results = await query.Take(100).ToListAsync();
        return Ok(results);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var item = await _db.Set<PatientOverview>().FirstOrDefaultAsync(p => p.PatientId == id);
        if (item == null) return NotFound();
        return Ok(item);
    }
}
