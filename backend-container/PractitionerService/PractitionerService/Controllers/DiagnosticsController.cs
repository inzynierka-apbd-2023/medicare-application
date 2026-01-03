using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PractitionerService.Data;
using PractitionerService.Models;
using System.Data;

namespace PractitionerService.Controllers;

[ApiController]
[Route("api/practitioner/diag")] 
public class DiagnosticsController : ControllerBase
{
    private readonly PractitionerDbContext _db;
    public DiagnosticsController(PractitionerDbContext db) => _db = db;

    [HttpGet("doctor-directory")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> DoctorDirectory()
    {
        var doctors = await _db.Set<DoctorDirectory>().Take(10).ToListAsync();
        return Ok(new { 
            count = doctors.Count,
            doctors = doctors.Select(d => new {
                d.DoctorId,
                d.UserId,
                d.FirstName,
                d.LastName,
                d.Email,
                d.IsActive
            })
        });
    }

    [HttpGet("migrations")] 
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult Migrations()
    {
        var applied = _db.Database.GetAppliedMigrations().ToList();
        var all = _db.Database.GetMigrations().ToList();
        var pending = _db.Database.GetPendingMigrations().ToList();
        return Ok(new { all, applied, pending, historyTable = "practitioner.__EFMigrationsHistory" });
    }

    [HttpGet("schema")] 
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> Schema()
    {
        await using var conn = _db.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open) await conn.OpenAsync();
        var names = new[] { "Doctor", "Receptionist", "Service", "Specialization", "Doctor_Specialization", "Doctor_Schedule" };
        var results = new Dictionary<string, bool>();
        foreach (var t in names)
        {
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT COUNT(1) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'practitioner' AND TABLE_NAME = @t";
            var p = cmd.CreateParameter();
            p.ParameterName = "@t"; p.Value = t;
            cmd.Parameters.Add(p);
            var count = (int)(await cmd.ExecuteScalarAsync() ?? 0);
            results[$"practitioner.{t}"] = count > 0;
        }
        // view check
        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"SELECT COUNT(1) FROM sys.views WHERE object_id = OBJECT_ID('practitioner.DoctorDirectory')";
            var count = (int)(await cmd.ExecuteScalarAsync() ?? 0);
            results["practitioner.DoctorDirectory (view)"] = count > 0;
        }
        return Ok(new { tables = results });
    }
}
