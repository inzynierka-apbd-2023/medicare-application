using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MedicalCatalogService.Data;

public class MedicalCatalogDbContextFactory : IDesignTimeDbContextFactory<MedicalCatalogDbContext>
{
    public MedicalCatalogDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MedicalCatalogDbContext>();
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=MedicalCatalogServiceDb_Design;Trusted_Connection=True;");
        return new MedicalCatalogDbContext(optionsBuilder.Options);
    }
}
