using Microsoft.EntityFrameworkCore;

namespace LabService.Data.Seeders;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LabDbContext>();
        
        await db.Database.MigrateAsync();
        await MockDataSeeder.SeedAsync(db);
    }
}
