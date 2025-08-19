using PatientService.Features.Metrics.Services;
using PatientService.Tests.Fixtures;
using PatientService.Models;
using Xunit;

namespace PatientService.Tests.Services;

public class PatientMetricsServiceTests : IClassFixture<InMemoryDbFixture>
{
    private readonly InMemoryDbFixture _fx;
    private readonly PatientMetricsService _service;

    public PatientMetricsServiceTests(InMemoryDbFixture fx)
    {
        _fx = fx;
        _service = new PatientMetricsService(fx.Context);
    }

    [Fact]
    public async Task ReturnsCountsAndRetention()
    {
        var now = DateTime.UtcNow.Date;
        var oldId = Guid.NewGuid().ToString();
        var newId = Guid.NewGuid().ToString();
        _fx.Context.Patients.Add(new Patient { Id = oldId, UserId = Guid.NewGuid().ToString(), CreatedAt = now.AddDays(-40), UpdatedAt = now });
        _fx.Context.Patients.Add(new Patient { Id = newId, UserId = Guid.NewGuid().ToString(), CreatedAt = now.AddDays(-2), UpdatedAt = now });
        _fx.Context.PatientStatuses.Add(new PatientStatus { Id = Guid.NewGuid().ToString(), PatientId = oldId, Status = "Active", EffectiveAt = now.AddDays(-10) });
        _fx.Context.PatientStatuses.Add(new PatientStatus { Id = Guid.NewGuid().ToString(), PatientId = newId, Status = "Active", EffectiveAt = now.AddDays(-1) });
        await _fx.Context.SaveChangesAsync();

        var result = await _service.GetMetricsAsync(now.AddDays(-30), now, CancellationToken.None);

        Assert.Equal(2, result.TotalActivePatients);
        Assert.Equal(1, result.NewPatients); // only newId inside window
        Assert.True(result.RetentionRate >= 0);
        Assert.False(result.IsStub);
    }

    [Fact]
    public async Task RetentionZeroWhenNoPrior()
    {
        var now = DateTime.UtcNow.Date;
        var id = Guid.NewGuid().ToString();
        _fx.Context.Patients.Add(new Patient { Id = id, UserId = Guid.NewGuid().ToString(), CreatedAt = now.AddDays(-5), UpdatedAt = now });
        _fx.Context.PatientStatuses.Add(new PatientStatus { Id = Guid.NewGuid().ToString(), PatientId = id, Status = "Active", EffectiveAt = now.AddDays(-1) });
        await _fx.Context.SaveChangesAsync();

        var result = await _service.GetMetricsAsync(now.AddDays(-3), now, CancellationToken.None);
        Assert.Equal(0, result.RetentionRate);
    }
}
