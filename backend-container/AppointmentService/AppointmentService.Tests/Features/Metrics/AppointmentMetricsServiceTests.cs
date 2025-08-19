using AppointmentService.Features.Metrics.Services;
using Xunit;

namespace AppointmentService.Tests.Features.Metrics;

public class AppointmentMetricsServiceTests : IClassFixture<MetricsInMemoryDbFixture>
{
    private readonly MetricsInMemoryDbFixture _fx;
    public AppointmentMetricsServiceTests(MetricsInMemoryDbFixture fx) => _fx = fx;

    [Fact]
    public async Task CalculatesBasicCounts()
    {
        var svc = new AppointmentMetricsService(_fx.Context);
        var end = DateTime.UtcNow.Date;
        var start = end.AddDays(-10);
        var result = await svc.GetMetricsAsync(start, end, CancellationToken.None);
        Assert.True(result.TotalAppointments > 0);
        Assert.True(result.CompletedAppointments >= 0);
        Assert.True(result.ActiveDoctorsInPeriod > 0);
        Assert.True(result.UniquePatientsInPeriod > 0);
        Assert.False(result.IsStub);
    }
}
