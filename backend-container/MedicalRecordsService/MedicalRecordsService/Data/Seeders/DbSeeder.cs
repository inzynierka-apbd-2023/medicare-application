using MedicalRecordsService.Data;
using Microsoft.EntityFrameworkCore;

namespace MedicalRecordsService.Data.Seeders;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MedicalRecordsDbContext>();

        await db.Database.MigrateAsync();
        await MockDataSeeder.SeedAsync(db);
    }
}
