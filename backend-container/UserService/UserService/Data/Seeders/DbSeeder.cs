using Microsoft.EntityFrameworkCore;
using UserService.Models;
using UserService.Data;

namespace UserService.Data.Seeders;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        
        await db.Database.MigrateAsync();
        await SeedRolesAsync(db);
        await MockDataSeeder.SeedAsync(db);
    }

    private static async Task SeedRolesAsync(UserDbContext db)
    {
        var existingRoles = await db.Roles.Select(r => r.Name).ToListAsync();
        var rolesToAdd = new List<Role>();
        
        if (!existingRoles.Contains("Admin"))
            rolesToAdd.Add(new Role { Id = Guid.NewGuid(), Name = "Admin", Description = "Administrator" });
        if (!existingRoles.Contains("Doctor"))
            rolesToAdd.Add(new Role { Id = Guid.NewGuid(), Name = "Doctor", Description = "Doctor user" });
        if (!existingRoles.Contains("Patient"))
            rolesToAdd.Add(new Role { Id = Guid.NewGuid(), Name = "Patient", Description = "Patient user" });
        if (!existingRoles.Contains("Receptionist"))
            rolesToAdd.Add(new Role { Id = Guid.NewGuid(), Name = "Receptionist", Description = "Receptionist user" });
        if (!existingRoles.Contains("Owner"))
            rolesToAdd.Add(new Role { Id = Guid.NewGuid(), Name = "Owner", Description = "Clinic owner" });
        
        if (rolesToAdd.Any())
        {
            db.Roles.AddRange(rolesToAdd);
            await db.SaveChangesAsync();
        }
    }
}
