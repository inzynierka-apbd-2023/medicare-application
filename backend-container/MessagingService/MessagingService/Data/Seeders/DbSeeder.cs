using Microsoft.EntityFrameworkCore;
using MessagingService.Data;

namespace MessagingService.Data.Seeders;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MessagingDbContext>();
        
        await db.Database.MigrateAsync();
        await MockDataSeeder.SeedAsync(db);
    }
}
