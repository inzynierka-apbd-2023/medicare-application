using Microsoft.EntityFrameworkCore;

namespace PatientService.Data.Seeders;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PatientDbContext>();

        await db.Database.MigrateAsync();
        await MockDataSeeder.SeedAsync(db);
        await CreateViewsAsync(db);
    }

    private static async Task CreateViewsAsync(PatientDbContext db)
    {
        int retries = 0;
        bool created = false;

        while (!created && retries < 15)
        {
            try
            {
                var checkTableSql = "SELECT OBJECT_ID('[user].[User_Profile]', 'U')";
                await db.Database.ExecuteSqlRawAsync($"DECLARE @id INT = ({checkTableSql}); IF @id IS NULL THROW 50000, 'User table not found', 1;");

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

                await db.Database.ExecuteSqlRawAsync(viewSql);
                created = true;
            }
            catch
            {
                retries++;
                await Task.Delay(2000);
            }
        }

        if (!created)
        {
            var fallbackSql = @"
                CREATE OR ALTER VIEW patient.PatientOverview AS
                SELECT p.Id AS PatientId,
                       p.UserId,
                       NULL AS FirstName,
                       NULL AS LastName,
                       NULL AS Email,
                       NULL AS Phone,
                       NULL AS DateOfBirth,
                       NULL AS Gender,
                       NULL AS Address,
                       (SELECT TOP 1 s.Status FROM patient.Patient_Status s WHERE s.PatientId = p.Id ORDER BY s.EffectiveAt DESC) AS CurrentStatus,
                       (SELECT TOP 1 ec.Name FROM patient.Emergency_Contact ec WHERE ec.PatientId = p.Id) AS EmergencyContactName,
                       (SELECT TOP 1 ec.Phone FROM patient.Emergency_Contact ec WHERE ec.PatientId = p.Id) AS EmergencyContactPhone
                FROM patient.Patient p;";
            await db.Database.ExecuteSqlRawAsync(fallbackSql);
        }
    }
}
