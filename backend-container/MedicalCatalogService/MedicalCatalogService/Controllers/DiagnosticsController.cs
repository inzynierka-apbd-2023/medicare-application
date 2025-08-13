using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MedicalCatalogService.Data;

namespace MedicalCatalogService.Controllers;

[ApiController]
[Route("api/catalog/diag")] 
public class DiagnosticsController : ControllerBase
{
    private readonly MedicalCatalogDbContext _db;
    public DiagnosticsController(MedicalCatalogDbContext db) => _db = db;

    [HttpGet("migrations")] 
    public IActionResult GetMigrations()
    {
        var all = _db.Database.GetMigrations().ToArray();
        var applied = _db.Database.GetAppliedMigrations().ToArray();
        var pending = _db.Database.GetPendingMigrations().ToArray();
        return Ok(new { all, applied, pending, historyTable = "catalog.__EFMigrationsHistory" });
    }

    [HttpGet("schema")] 
    public async Task<IActionResult> CheckSchema()
    {
        var checks = new[]
        {
            "catalog.icd10",
            "catalog.loinc",
            "catalog.release",
            "catalog.loinc_map_to",
            "catalog.loinc_answer_list",
            "catalog.loinc_answer_link",
            "catalog.loinc_consumer_name",
            "catalog.loinc_panel",
            "catalog.loinc_panel_item",
        };
        var results = new Dictionary<string, bool>();
        await using var cmd = _db.Database.GetDbConnection().CreateCommand();
        await _db.Database.OpenConnectionAsync();
        foreach (var fqn in checks)
        {
            cmd.CommandText = $"SELECT COUNT(1) FROM sys.objects WHERE object_id = OBJECT_ID('{fqn}')";
            var count = (int)(await cmd.ExecuteScalarAsync() ?? 0);
            results[fqn] = count > 0;
        }
        return Ok(results);
    }

    [HttpGet("loinc-stats")] // counts to verify imports
    public async Task<IActionResult> GetLoincStats()
    {
    var loincCount = await _db.Loinc.CountAsync();
    var mapToCount = await _db.LoincMapTo.CountAsync();
    var answerListTargets = await _db.LoincAnswerList.CountAsync();
    var loincToList = await _db.LoincAnswerLink.CountAsync();
    var releases = await _db.Releases.Where(r => r.System == "loinc").OrderByDescending(r => r.ReleasedOn).Take(3).ToListAsync();
    return Ok(new { loincCount, mapToCount, answerListTargets, loincToList, releases });
    }

    [HttpGet("loinc-mapto/{code}")] // check specific loinc->loinc mappings
    public async Task<IActionResult> GetLoincMapTo([FromRoute] string code)
    {
        var list = await _db.LoincMapTo.Where(m => m.FromLoinc == code)
            .OrderBy(m => m.ToLoinc)
            .Select(m => new { targetCode = m.ToLoinc, mapType = m.MapType, comment = m.Comment })
            .ToListAsync();
        return Ok(list);
    }

    [HttpGet("loinc-answers/{code}")] // check LOINC -> AnswerList and list -> answers
    public async Task<IActionResult> GetLoincAnswers([FromRoute] string code)
    {
        var lists = await _db.LoincAnswerLink.Where(l => l.LoincNum == code)
            .Select(l => l.AnswerListId)
            .Distinct()
            .ToListAsync();
    var answers = await _db.LoincAnswerList
        .Where(a => lists.Contains(a.AnswerListId))
        .Select(a => new { a.AnswerListId, a.AnswerStringId, a.DisplayName, a.Description })
            .ToListAsync();
        return Ok(new { lists, answers });
    }

    // Aliases removed in new schema

    [HttpGet("loinc-panel/{code}")] // panel definition and items
    public async Task<IActionResult> GetLoincPanel([FromRoute] string code)
    {
        var panel = await _db.LoincPanel.FirstOrDefaultAsync(p => p.PanelLoincNum == code);
        var items = await _db.LoincPanelItem.Where(i => i.PanelLoincNum == code)
            .OrderBy(i => i.Ordinal ?? int.MaxValue).ThenBy(i => i.ItemLoincNum)
            .Select(i => new { i.ItemLoincNum, i.Ordinal, i.Optionality })
            .ToListAsync();
        return Ok(new { panel, items });
    }
}
