using Microsoft.EntityFrameworkCore;
using PatientService.Data;

namespace PatientService.Tests.Fixtures;

public class InMemoryDbFixture : IDisposable
{
    public PatientDbContext Context { get; }

    public InMemoryDbFixture()
    {
        var options = new DbContextOptionsBuilder<PatientDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        Context = new PatientDbContext(options);
    }

    public void Dispose()
    {
        Context.Database.EnsureDeleted();
        Context.Dispose();
    }
}
