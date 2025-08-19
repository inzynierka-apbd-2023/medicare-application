using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotificationService.Data;

namespace NotificationService.Controllers;

[ApiController]
[Route("api/notifications/admin")] 
public class AdminController : ControllerBase
{
    private readonly NotificationsDbContext _db;
    public AdminController(NotificationsDbContext db) { _db = db; }

    // Purge notifications schema and recreate via migrations
    [HttpPost("purge")]
    public async Task<IActionResult> Purge()
    {
        // Drop migration history table first, then user table, then schema
        var dropSql = @"
IF EXISTS (SELECT 1 FROM sys.objects o JOIN sys.schemas s ON o.schema_id = s.schema_id WHERE s.name = 'notifications' AND o.name = '__EFMigrationsHistory' AND o.type = 'U')
BEGIN
    DROP TABLE [notifications].[__EFMigrationsHistory];
END
IF EXISTS (SELECT 1 FROM sys.objects o JOIN sys.schemas s ON o.schema_id = s.schema_id WHERE s.name = 'notifications' AND o.name = 'Notification' AND o.type = 'U')
BEGIN
    DROP TABLE [notifications].[Notification];
END
IF EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'notifications')
BEGIN
    DROP SCHEMA [notifications];
END
";
        try
        {
            await _db.Database.ExecuteSqlRawAsync(dropSql);
        }
        catch
        {
            // Best-effort purge; continue to migrate
        }

        // Recreate schema via EF Core migrations
        await _db.Database.MigrateAsync();
        return Ok(new { status = "purged" });
    }

    // Debug: list latest notifications (no auth here since service is internal behind nginx in dev)
    [HttpGet("last")] 
    public async Task<ActionResult<IEnumerable<object>>> Last([FromQuery] int take = 20)
    {
        if (take < 1) take = 1; if (take > 100) take = 100;
        var list = await _db.Notifications
            .OrderByDescending(n => n.Creation_Date)
            .Take(take)
            .Select(n => new { n.Id, n.Recipient_User_Id, n.Description, n.Type, n.Creation_Date, n.Is_Read, n.Action_Url })
            .ToListAsync();
        return Ok(list);
    }
}
