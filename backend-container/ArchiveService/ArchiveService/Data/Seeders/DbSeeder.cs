using Microsoft.EntityFrameworkCore;
using ArchiveService.Data;

namespace ArchiveService.Data.Seeders;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ArchiveDbContext>();
        
        await context.Database.MigrateAsync();
    }
}
