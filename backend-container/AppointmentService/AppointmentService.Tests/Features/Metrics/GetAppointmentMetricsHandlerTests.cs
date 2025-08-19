using AppointmentService.Features.Metrics.Handlers;
using AppointmentService.Features.Metrics.Queries;
using AppointmentService.Features.Metrics.Services;
using Xunit;

namespace AppointmentService.Tests.Features.Metrics;

public class GetAppointmentMetricsHandlerTests : IClassFixture<MetricsInMemoryDbFixture>
{
    private readonly MetricsInMemoryDbFixture _fx;
    public GetAppointmentMetricsHandlerTests(MetricsInMemoryDbFixture fx) => _fx = fx;

    [Fact]
    public async Task HandlerReturnsServiceData()
    {
        var svc = new AppointmentMetricsService(_fx.Context);
        var handler = new GetAppointmentMetricsHandler(svc);
        var end = DateTime.UtcNow.Date;
        var start = end.AddDays(-7);
        var resp = await handler.Handle(new GetAppointmentMetricsQuery { StartDate = start, EndDate = end }, CancellationToken.None);
        Assert.Equal(start, resp.StartDate);
        Assert.Equal(end, resp.EndDate);
    }
}
