using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PatientService.Data;

namespace PatientService.Data.Seeders;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PatientDbContext>();
        var logger = scope.ServiceProvider.GetService<ILogger<PatientDbContext>>();

        await db.Database.MigrateAsync();
        await MockDataSeeder.SeedAsync(db);
        await CreateViewsAsync(db, logger);
    }

    private static async Task CreateViewsAsync(PatientDbContext db, ILogger? logger)
    {
        // Retry logic: wait for UserService to create User_Profile table
        const int maxRetries = 5;
        const int delayMs = 2000;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                // First check if User_Profile table exists
                var tableExistsSql = @"
                    SELECT COUNT(1) FROM INFORMATION_SCHEMA.TABLES 
                    WHERE TABLE_SCHEMA = 'user' AND TABLE_NAME = 'User_Profile'";
                
                var connection = db.Database.GetDbConnection();
                if (connection.State != System.Data.ConnectionState.Open)
                    await connection.OpenAsync();

                await using var cmd = connection.CreateCommand();
                cmd.CommandText = tableExistsSql;
                var tableExists = ((int)(await cmd.ExecuteScalarAsync() ?? 0)) > 0;

                if (!tableExists)
                {
                    logger?.LogWarning("Attempt {Attempt}/{MaxRetries}: User_Profile table not found, waiting...", attempt, maxRetries);
                    if (attempt < maxRetries)
                    {
                        await Task.Delay(delayMs);
                        continue;
                    }
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
                    up.Address_Line1 AS AddressLine1,
                    up.Address_Line2 AS AddressLine2,
                    up.City,
                    up.State,
                    up.ZipCode,
                    up.Country,                       
                    (SELECT TOP 1 s.Status FROM patient.Patient_Status s WHERE s.PatientId = p.Id ORDER BY s.EffectiveAt DESC) AS CurrentStatus,
                    (SELECT TOP 1 ec.Name FROM patient.Emergency_Contact ec WHERE ec.PatientId = p.Id) AS EmergencyContactName,
                    (SELECT TOP 1 ec.Phone FROM patient.Emergency_Contact ec WHERE ec.PatientId = p.Id) AS EmergencyContactPhone
                FROM patient.Patient p
                LEFT JOIN [user].[User_Profile] up ON up.User_Id = p.UserId;";

                await db.Database.ExecuteSqlRawAsync(viewSql);
                logger?.LogInformation("PatientOverview view created successfully with User_Profile join on attempt {Attempt}", attempt);
                return; // Success!
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "Attempt {Attempt}/{MaxRetries}: Failed to create PatientOverview view with User_Profile join", attempt, maxRetries);
                if (attempt < maxRetries)
                {
                    await Task.Delay(delayMs);
                }
            }
        }

        // Fallback: create view without User_Profile join
        logger?.LogWarning("All attempts failed. Creating fallback PatientOverview view without User_Profile data.");
        var fallbackSql = @"
            CREATE OR ALTER VIEW patient.PatientOverview AS
            SELECT p.Id AS PatientId,
                   p.UserId,
                   CAST(NULL AS NVARCHAR(100)) AS FirstName,
                   CAST(NULL AS NVARCHAR(100)) AS LastName,
                   CAST(NULL AS NVARCHAR(255)) AS Email,
                   CAST(NULL AS NVARCHAR(20)) AS Phone,
                   CAST(NULL AS DATETIME2) AS DateOfBirth,
                   CAST(NULL AS NVARCHAR(20)) AS Gender,
                   NULL AS AddressLine1,
                   NULL AS AddressLine2,
                   NULL AS City,
                   NULL AS State,
                   NULL AS ZipCode,
                   NULL AS Country,
                   (SELECT TOP 1 s.Status FROM patient.Patient_Status s WHERE s.PatientId = p.Id ORDER BY s.EffectiveAt DESC) AS CurrentStatus,
                   (SELECT TOP 1 ec.Name FROM patient.Emergency_Contact ec WHERE ec.PatientId = p.Id) AS EmergencyContactName,
                   (SELECT TOP 1 ec.Phone FROM patient.Emergency_Contact ec WHERE ec.PatientId = p.Id) AS EmergencyContactPhone
            FROM patient.Patient p;";
        await db.Database.ExecuteSqlRawAsync(fallbackSql);
    }
}
