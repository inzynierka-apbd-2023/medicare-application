using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PractitionerService.Data;

public class PractitionerDbContextFactory : IDesignTimeDbContextFactory<PractitionerDbContext>
{
    public PractitionerDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PractitionerDbContext>();
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=PractitionerServiceDb_Design;Trusted_Connection=True;");
        return new PractitionerDbContext(optionsBuilder.Options);
    }
}
