using Microsoft.EntityFrameworkCore;
using AppointmentService.Data;

namespace AppointmentService.Data.Seeders;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppointmentDbContext>();
        
        await db.Database.MigrateAsync();
        await MockDataSeeder.SeedAsync(db);
    }
}
