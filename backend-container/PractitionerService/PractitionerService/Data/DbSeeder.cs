using Microsoft.EntityFrameworkCore;
using PractitionerService.Models;

namespace PractitionerService.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PractitionerDbContext>();

        await db.Database.MigrateAsync();

        await SeedCatalogAsync(db);
        await MockDataSeeder.SeedAsync(db);
        await CreateViewsAsync(db);
    }

    private static async Task CreateViewsAsync(PractitionerDbContext db)
    {
        try
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
        }
        catch
        {
            // Ignore if fails (e.g. user table missing) as per requirements to not resolve problems now
        }
    }

    private static async Task SeedCatalogAsync(PractitionerDbContext db)
    {
        if (!await db.Services.AnyAsync())
        {
            db.Services.AddRange(
                new MedicalService { Name = "General Consultation", Description = "Routine check and consultation" },
                new MedicalService { Name = "Cardiology", Description = "Heart-related services" },
                new MedicalService { Name = "Dermatology", Description = "Skin-related services" }
            );
        }
        if (!await db.Specializations.AnyAsync())
        {
            db.Specializations.AddRange(
                new Specialization { Name = "Cardiologist" },
                new Specialization { Name = "Dermatologist" },
                new Specialization { Name = "General Practitioner" }
            );
        }
        await db.SaveChangesAsync();
    }
}
