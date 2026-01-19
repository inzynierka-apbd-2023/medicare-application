using Microsoft.EntityFrameworkCore;
using MedicalCatalogService.Data;

namespace MedicalCatalogService.Data.Seeders;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MedicalCatalogDbContext>();
        
        await context.Database.MigrateAsync();
        await MockDataSeeder.SeedAsync(context);
    }
}
