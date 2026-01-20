using Microsoft.EntityFrameworkCore;
using MedicalCatalogService.Data;

namespace MedicalCatalogService.Data.Seeders;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MedicalCatalogDbContext>();
        var logger = scope.ServiceProvider.GetService<ILogger<MedicalCatalogDbContext>>();
        
        await context.Database.MigrateAsync();
        
        // First seed from CSV files (comprehensive data)
        await CsvDataSeeder.SeedAsync(context, logger);
        
        // Then run mock seeder for any additional mock data (panels, consumer names, etc.)
        await MockDataSeeder.SeedAsync(context);
    }
}
