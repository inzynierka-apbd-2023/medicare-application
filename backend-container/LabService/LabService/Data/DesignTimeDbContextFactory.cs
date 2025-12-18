using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LabService.Data;

public class LabDbContextFactory : IDesignTimeDbContextFactory<LabDbContext>
{
    public LabDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<LabDbContext>();
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=LabServiceDb_Design;Trusted_Connection=True;");
        return new LabDbContext(optionsBuilder.Options);
    }
}
