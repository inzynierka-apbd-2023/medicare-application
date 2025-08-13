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

    [HttpGet("conditions")] 
    public async Task<IActionResult> GetConditions([FromQuery] string? q)
    {
        var query = _db.MedicalConditions.AsQueryable().Where(x => x.IsActive);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var like = $"%{q}%";
            query = query.Where(x => EF.Functions.Like(x.Code, like) || EF.Functions.Like(x.Name, like));
        }
        var list = await query.OrderBy(x => x.Name).Take(200).ToListAsync();
        return Ok(list);
    }

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

    [HttpGet("snomed")]
    public async Task<IActionResult> GetSnomed([FromQuery] string? q)
    {
        var query = _db.Snomed.AsQueryable().Where(x => x.Active);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var like = $"%{q}%";
            query = query.Where(x => EF.Functions.Like(x.PreferredTerm!, like) || EF.Functions.Like(x.Fsn!, like));
            if (long.TryParse(q, out var id))
            {
                query = query.Union(_db.Snomed.Where(x => x.ConceptId == id));
            }
        }
        var list = await query.OrderBy(x => x.PreferredTerm).Take(200).ToListAsync();
        return Ok(list);
    }

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

    [HttpGet("cpt")]
    public async Task<IActionResult> GetCpt([FromQuery] string? q)
    {
        var query = _db.Cpt.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            var like = $"%{q}%";
            query = query.Where(x => EF.Functions.Like(x.Code, like) || EF.Functions.Like(x.ShortDesc!, like) || EF.Functions.Like(x.LongDesc!, like));
        }
        var list = await query.OrderBy(x => x.Code).Take(200).ToListAsync();
        return Ok(list);
    }

    [HttpGet("hcpcs")]
    public async Task<IActionResult> GetHcpcs([FromQuery] string? q)
    {
        var query = _db.Hcpcs.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            var like = $"%{q}%";
            query = query.Where(x => EF.Functions.Like(x.Code, like) || EF.Functions.Like(x.ShortDesc!, like) || EF.Functions.Like(x.LongDesc!, like));
        }
        var list = await query.OrderBy(x => x.Code).Take(200).ToListAsync();
        return Ok(list);
    }

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

    [HttpGet("lab-tests")] 
    public async Task<IActionResult> GetLabTests([FromQuery] string? q)
    {
        var query = _db.LabTestTypes.AsQueryable().Where(x => x.IsActive);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var like = $"%{q}%";
            query = query.Where(x => EF.Functions.Like(x.Code, like) || EF.Functions.Like(x.Name, like));
        }
        var list = await query.OrderBy(x => x.Name).Take(200).ToListAsync();
        return Ok(list);
    }

    [HttpPost("conditions")] 
    [Authorize]
    public async Task<IActionResult> UpsertCondition([FromBody] MedicalCondition c)
    {
        c.UpdatedAt = DateTime.UtcNow;
        var existing = await _db.MedicalConditions.FirstOrDefaultAsync(x => x.Code == c.Code);
        if (existing is null)
        {
            _db.MedicalConditions.Add(c);
        }
        else
        {
            existing.Name = c.Name;
            existing.Description = c.Description;
            existing.IsActive = c.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync();
        // TODO: publish ConditionCatalogUpdated
        return Ok(c);
    }

    [HttpPost("lab-tests")] 
    [Authorize]
    public async Task<IActionResult> UpsertLabTest([FromBody] LabTestType lt)
    {
        lt.UpdatedAt = DateTime.UtcNow;
        var existing = await _db.LabTestTypes.FirstOrDefaultAsync(x => x.Code == lt.Code);
        if (existing is null)
        {
            _db.LabTestTypes.Add(lt);
        }
        else
        {
            existing.Name = lt.Name;
            existing.Unit = lt.Unit;
            existing.ReferenceRange = lt.ReferenceRange;
            existing.IsActive = lt.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync();
        // TODO: publish LabTestTypeUpdated
        return Ok(lt);
    }
}
