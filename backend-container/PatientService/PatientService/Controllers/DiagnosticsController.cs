using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PatientService.Data;
using System.Data;

namespace PatientService.Controllers;

[ApiController]
[Route("api/patient/diag")] 
[Authorize(Roles = "Owner,Admin")]
public class DiagnosticsController : ControllerBase
{
    private readonly PatientDbContext _db;
    public DiagnosticsController(PatientDbContext db) => _db = db;

    [HttpGet("migrations")] 
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult Migrations()
    {
        var applied = _db.Database.GetAppliedMigrations().ToList();
        var all = _db.Database.GetMigrations().ToList();
        var pending = _db.Database.GetPendingMigrations().ToList();
        return Ok(new { all, applied, pending, historyTable = "patient.__EFMigrationsHistory" });
    }

    [HttpGet("schema")] 
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> Schema()
    {
        await using var conn = _db.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open) await conn.OpenAsync();
        var names = new[] { "Patient", "Emergency_Contact", "Insurance", "Patient_Status" };
        var results = new Dictionary<string, bool>();
        foreach (var t in names)
        {
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT COUNT(1) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'patient' AND TABLE_NAME = @t";
            var p = cmd.CreateParameter();
            p.ParameterName = "@t"; p.Value = t;
            cmd.Parameters.Add(p);
            var count = (int)(await cmd.ExecuteScalarAsync() ?? 0);
            results[$"patient.{t}"] = count > 0;
        }
        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"SELECT COUNT(1) FROM sys.views WHERE object_id = OBJECT_ID('patient.PatientOverview')";
            var count = (int)(await cmd.ExecuteScalarAsync() ?? 0);
            results["patient.PatientOverview (view)"] = count > 0;
        }
        return Ok(new { tables = results });
    }
}
