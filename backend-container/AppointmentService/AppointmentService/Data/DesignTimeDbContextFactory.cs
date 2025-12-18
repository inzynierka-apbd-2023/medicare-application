using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AppointmentService.Data;

public class AppointmentDbContextFactory : IDesignTimeDbContextFactory<AppointmentDbContext>
{
    public AppointmentDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppointmentDbContext>();
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=AppointmentServiceDb_Design;Trusted_Connection=True;");
        return new AppointmentDbContext(optionsBuilder.Options);
    }
}
