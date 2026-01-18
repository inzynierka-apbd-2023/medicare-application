using DocumentsService.Data;
using Microsoft.EntityFrameworkCore;

namespace DocumentsService.Data.Seeders;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DocumentsDbContext>();

        await db.Database.MigrateAsync();
        
        await SeedDocumentTypesAsync(db);
        await MockDataSeeder.SeedAsync(db);
    }

    private static async Task SeedDocumentTypesAsync(DocumentsDbContext db)
    {
        var typesToSeed = new[]
        {
            new { Code = "VISIT_NOTE", Name = "Visit Note", Description = "Clinical visit document" },
            new { Code = "PRESCRIPTION", Name = "Prescription", Description = "Medication order" },
            new { Code = "REFERRAL", Name = "Referral", Description = "Referral to specialist/provider" },
            new { Code = "SICK_LEAVE", Name = "Sick Leave", Description = "Work absence certificate" },
            new { Code = "LAB_RESULTS", Name = "Lab Results", Description = "Laboratory results report" }
        };

        foreach (var t in typesToSeed)
        {
            if (!await db.DocumentTypes.AnyAsync(dt => dt.Code == t.Code))
            {
                db.DocumentTypes.Add(new DocumentsService.Models.DocumentType
                {
                    Code = t.Code,
                    Name = t.Name,
                    Description = t.Description
                });
            }
        }
        await db.SaveChangesAsync();
    }
}
