using ArchiveService.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArchiveService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ArchiveController : ControllerBase
{
    private readonly ArchiveDbContext _db;

    public ArchiveController(ArchiveDbContext db)
    {
        _db = db;
    }

    [HttpGet("doctors/{doctorId}")]
    public async Task<IActionResult> GetArchivedDoctor(Guid doctorId)
    {
        var archived = await _db.ArchivedDoctors.FindAsync(doctorId);
        return archived is null ? NotFound() : Ok(archived);
    }
}
