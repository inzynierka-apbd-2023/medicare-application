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
            "catalog.Medical_Condition",
            "catalog.Lab_Test_Type",
            "catalog.icd10",
            "catalog.snomed",
            "catalog.loinc",
            "catalog.cpt",
            "catalog.hcpcs",
            "catalog.release",
            "catalog.mappings",
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
}
