using Microsoft.EntityFrameworkCore;

namespace BillingService.Data.Seeders;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BillingDbContext>();

        await db.Database.MigrateAsync();
        await MockDataSeeder.SeedAsync(db);
    }
}
