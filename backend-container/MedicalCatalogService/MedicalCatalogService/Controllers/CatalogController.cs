using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MedicalCatalogService.Data;
using MedicalCatalogService.Models;

namespace MedicalCatalogService.Controllers;

[ApiController]
[Route("api/catalog")] 
public class CatalogController : ControllerBase
{
    private readonly MedicalCatalogDbContext _db;
    public CatalogController(MedicalCatalogDbContext db) => _db = db;

    // Legacy endpoints removed (conditions, lab-tests, snomed, cpt, hcpcs)

    [HttpGet("icd10")]
    public async Task<IActionResult> GetIcd10([FromQuery] string? q)
    {
        var query = _db.Icd10.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            var like = $"%{q}%";
            query = query.Where(x => EF.Functions.Like(x.Code, like) || EF.Functions.Like(x.Title!, like));
        }
        var list = await query.OrderBy(x => x.Code).Take(200).ToListAsync();
        return Ok(list);
    }

    // SNOMED endpoint removed

    [HttpGet("loinc")]
    public async Task<IActionResult> GetLoinc([FromQuery] string? q)
    {
        var query = _db.Loinc.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            var like = $"%{q}%";
            query = query.Where(x => EF.Functions.Like(x.LoincNum, like) || EF.Functions.Like(x.LongCommonName!, like) || EF.Functions.Like(x.Component!, like));
        }
        var list = await query.OrderBy(x => x.LoincNum).Take(200).ToListAsync();
        return Ok(list);
    }

    // CPT endpoint removed

    // HCPCS endpoint removed

    [HttpGet("releases")]
    public async Task<IActionResult> GetReleases([FromQuery] string? system)
    {
        var query = _db.Releases.AsQueryable();
        if (!string.IsNullOrWhiteSpace(system))
        {
            query = query.Where(x => x.System == system);
        }
        var list = await query.OrderByDescending(x => x.ReleasedOn).ThenBy(x => x.System).Take(100).ToListAsync();
        return Ok(list);
    }

    // Lab test types endpoint removed

    // UpsertCondition removed

    // UpsertLabTest removed
}
