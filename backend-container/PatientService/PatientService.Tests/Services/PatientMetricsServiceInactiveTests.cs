using PatientService.Features.Metrics.Services;
using PatientService.Tests.Fixtures;
using PatientService.Models;
using Xunit;

namespace PatientService.Tests.Services;

public class PatientMetricsServiceInactiveTests : IClassFixture<InMemoryDbFixture>
{
    private readonly InMemoryDbFixture _fx;
    private readonly PatientMetricsService _service;

    public PatientMetricsServiceInactiveTests(InMemoryDbFixture fx)
    {
        _fx = fx;
        _service = new PatientMetricsService(fx.Context);
    }

    [Fact]
    public async Task InactiveLatestStatus_NotCountedAsActive()
    {
        var now = DateTime.UtcNow.Date;
        var id = Guid.NewGuid().ToString();
        _fx.Context.Patients.Add(new Patient { Id = id, UserId = Guid.NewGuid().ToString(), CreatedAt = now.AddDays(-20), UpdatedAt = now });
        _fx.Context.PatientStatuses.Add(new PatientStatus { Id = Guid.NewGuid().ToString(), PatientId = id, Status = "Active", EffectiveAt = now.AddDays(-10) });
        _fx.Context.PatientStatuses.Add(new PatientStatus { Id = Guid.NewGuid().ToString(), PatientId = id, Status = "Inactive", EffectiveAt = now.AddDays(-1) });
        await _fx.Context.SaveChangesAsync();

        var result = await _service.GetMetricsAsync(now.AddDays(-30), now, CancellationToken.None);
        Assert.Equal(0, result.TotalActivePatients);
    }
}
