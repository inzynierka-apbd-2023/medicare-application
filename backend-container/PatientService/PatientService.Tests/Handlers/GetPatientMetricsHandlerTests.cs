using MediatR;
using PatientService.Features.Metrics.Handlers;
using PatientService.Features.Metrics.Queries;
using PatientService.Features.Metrics.Services;
using PatientService.Tests.Fixtures;
using PatientService.Models;
using Xunit;

namespace PatientService.Tests.Handlers;

public class GetPatientMetricsHandlerTests : IClassFixture<InMemoryDbFixture>
{
    private readonly InMemoryDbFixture _fx;
    private readonly IPatientMetricsService _service;

    public GetPatientMetricsHandlerTests(InMemoryDbFixture fx)
    {
        _fx = fx;
        _service = new PatientMetricsService(fx.Context);
    }

    [Fact]
    public async Task HandlerReturnsServiceData()
    {
        var now = DateTime.UtcNow.Date;
        var id = Guid.NewGuid().ToString();
        _fx.Context.Patients.Add(new Patient { Id = id, UserId = Guid.NewGuid().ToString(), CreatedAt = now.AddDays(-1), UpdatedAt = now });
        _fx.Context.PatientStatuses.Add(new PatientStatus { Id = Guid.NewGuid().ToString(), PatientId = id, Status = "Active", EffectiveAt = now });
        await _fx.Context.SaveChangesAsync();

        var handler = new GetPatientMetricsHandler(_service);
        var result = await handler.Handle(new GetPatientMetricsQuery { StartDate = now.AddDays(-7), EndDate = now }, CancellationToken.None);

        Assert.Equal(1, result.TotalActivePatients);
        Assert.Equal(1, result.NewPatients);
    }
}
