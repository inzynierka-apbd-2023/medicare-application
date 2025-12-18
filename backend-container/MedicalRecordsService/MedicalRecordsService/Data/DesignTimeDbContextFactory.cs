using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MedicalRecordsService.Data;

public class MedicalRecordsDbContextFactory : IDesignTimeDbContextFactory<MedicalRecordsDbContext>
{
    public MedicalRecordsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MedicalRecordsDbContext>();
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=MedicalRecordsServiceDb_Design;Trusted_Connection=True;");
        return new MedicalRecordsDbContext(optionsBuilder.Options);
    }
}
