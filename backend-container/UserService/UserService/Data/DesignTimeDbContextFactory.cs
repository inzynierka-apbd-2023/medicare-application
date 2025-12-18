using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace UserService.Data;

/// <summary>
/// Factory to create UserDbContext at design-time for EF Core migrations.
/// This bypasses the need for a running application during migration generation.
/// </summary>
public class UserDbContextFactory : IDesignTimeDbContextFactory<UserDbContext>
{
    public UserDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<UserDbContext>();
        
        // Use a dummy connection string for design-time operations
        // The actual connection string is provided at runtime via configuration
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=UserServiceDb_Design;Trusted_Connection=True;");
        
        return new UserDbContext(optionsBuilder.Options);
    }
}
