using Microsoft.EntityFrameworkCore;
using NotificationService.Data;

namespace NotificationService.Data.Seeders;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NotificationsDbContext>();

        await db.Database.MigrateAsync();
        
        await MockDataSeeder.SeedAsync(db);
    }
}
