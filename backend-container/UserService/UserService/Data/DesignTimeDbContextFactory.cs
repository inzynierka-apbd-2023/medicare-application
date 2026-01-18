using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace UserService.Data;

public class UserDbContextFactory : IDesignTimeDbContextFactory<UserDbContext>
{
    public UserDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<UserDbContext>();
        
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=UserServiceDb_Design;Trusted_Connection=True;");
        
        return new UserDbContext(optionsBuilder.Options);
    }
}
