using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PatientService.Data;
using System.Data;
using Microsoft.AspNetCore.Authorization;
namespace PatientService.Controllers;

[ApiController]
[Route("api/patient/diag")] 
[Authorize(Roles = "Owner,Admin")]
public class DiagnosticsController : ControllerBase
{
    private readonly PatientDbContext _db;
    private readonly ILogger<DiagnosticsController> _logger;
    public DiagnosticsController(PatientDbContext db, ILogger<DiagnosticsController> logger)
    {
        _db = db;
        _logger = logger;
    }

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

    [HttpPost("recreate-overview-view")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RecreateOverviewView()
    {
        try
        {
            // Check if User_Profile table exists
            await using var conn = _db.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open) await conn.OpenAsync();

            await using var checkCmd = conn.CreateCommand();
            checkCmd.CommandText = @"SELECT COUNT(1) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'user' AND TABLE_NAME = 'User_Profile'";
            var tableExists = ((int)(await checkCmd.ExecuteScalarAsync() ?? 0)) > 0;

            if (!tableExists)
            {
                return StatusCode(500, new { error = "User_Profile table does not exist. UserService may not have completed migration." });
            }

            var viewSql = @"
                CREATE OR ALTER VIEW patient.PatientOverview AS
                SELECT p.Id AS PatientId,
                       p.UserId,
                       up.FirstName,
                       up.LastName,
                       up.Email,
                       up.Phone,
                       up.DateOfBirth,
                       up.Gender,
                       up.Address_Line1 AS Address,
                       (SELECT TOP 1 s.Status FROM patient.Patient_Status s WHERE s.PatientId = p.Id ORDER BY s.EffectiveAt DESC) AS CurrentStatus,
                       (SELECT TOP 1 ec.Name FROM patient.Emergency_Contact ec WHERE ec.PatientId = p.Id) AS EmergencyContactName,
                       (SELECT TOP 1 ec.Phone FROM patient.Emergency_Contact ec WHERE ec.PatientId = p.Id) AS EmergencyContactPhone
                FROM patient.Patient p
                LEFT JOIN [user].[User_Profile] up ON up.User_Id = p.UserId;";

            await _db.Database.ExecuteSqlRawAsync(viewSql);
            _logger.LogInformation("PatientOverview view recreated successfully via admin endpoint");

            return Ok(new { success = true, message = "PatientOverview view recreated with User_Profile join" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to recreate PatientOverview view");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
