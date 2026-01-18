using Microsoft.EntityFrameworkCore;
using PractitionerService.Models;

namespace PractitionerService.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PractitionerDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<PractitionerDbContext>>();

        await db.Database.MigrateAsync();

        await SeedCatalogAsync(db, logger);
        await MockDataSeeder.SeedAsync(db, logger);
        await CreateViewsAsync(db, logger);
    }

    private static async Task CreateViewsAsync(PractitionerDbContext db, ILogger logger)
    {
        const int maxRetries = 10;
        const int retryDelayMs = 1000;
        
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                var checkResult = await db.Database.SqlQueryRaw<int>(
                    "SELECT CASE WHEN OBJECT_ID('[user].[User_Profile]', 'U') IS NOT NULL THEN 1 ELSE 0 END AS Value"
                ).FirstOrDefaultAsync();

                if (checkResult == 1)
                {
                    var viewSql = @"
CREATE OR ALTER VIEW practitioner.DoctorDirectory AS
SELECT d.Id AS DoctorId,
       d.UserId,
       up.FirstName,
       up.LastName,
       up.Email,
       up.Phone,
       STUFF((
           SELECT ',' + CAST(ds.SpecializationId AS NVARCHAR(36))
           FROM practitioner.Doctor_Specialization ds
           WHERE ds.DoctorId = d.Id
           FOR XML PATH(''), TYPE
       ).value('.','NVARCHAR(MAX)'), 1, 1, '') AS Specializations,
       NULL AS Services,
       d.IsActive
FROM practitioner.Doctor d
LEFT JOIN [user].[User_Profile] up ON up.User_Id = d.UserId;
";
                    await db.Database.ExecuteSqlRawAsync(viewSql);
                    logger.LogInformation("DoctorDirectory view created.");
                    return;
                }
                
                await Task.Delay(retryDelayMs);
            }
            catch (Exception ex)
            {
                if (attempt == maxRetries)
                {
                    logger.LogWarning($"View creation failed after {maxRetries} attempts: {ex.Message}");
                }
                else
                {
                    await Task.Delay(retryDelayMs);
                }
            }
        }

        try
        {
            var fallbackSql = @"
CREATE OR ALTER VIEW practitioner.DoctorDirectory AS
SELECT d.Id AS DoctorId,
       d.UserId,
       NULL AS FirstName,
       NULL AS LastName,
       NULL AS Email,
       NULL AS Phone,
       STUFF((
           SELECT ',' + CAST(ds.SpecializationId AS NVARCHAR(36))
           FROM practitioner.Doctor_Specialization ds
           WHERE ds.DoctorId = d.Id
           FOR XML PATH(''), TYPE
       ).value('.','NVARCHAR(MAX)'), 1, 1, '') AS Specializations,
       NULL AS Services,
       d.IsActive
FROM practitioner.Doctor d;
";
            await db.Database.ExecuteSqlRawAsync(fallbackSql);
            logger.LogInformation("DoctorDirectory fallback view created.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fallback view creation failed");
        }
    }

    private static async Task SeedCatalogAsync(PractitionerDbContext db, ILogger logger)
    {
        var anyServices = await db.Services.AnyAsync();
        if (!anyServices)
        {
            db.Services.AddRange(
                new MedicalService { Name = "General Consultation", Description = "Routine check and consultation" },
                new MedicalService { Name = "Cardiology", Description = "Heart-related services" },
                new MedicalService { Name = "Dermatology", Description = "Skin-related services" }
            );
        }
        var anySpecs = await db.Specializations.AnyAsync();
        if (!anySpecs)
        {
            db.Specializations.AddRange(
                new Specialization { Name = "Cardiologist" },
                new Specialization { Name = "Dermatologist" },
                new Specialization { Name = "General Practitioner" }
            );
        }
        if (!anyServices || !anySpecs)
        {
            await db.SaveChangesAsync();
        }
    }
}
