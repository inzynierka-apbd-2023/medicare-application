using Moq;
using Xunit;
using PatientService.Controllers;
using Microsoft.Extensions.Logging.Abstractions;
using MediatR;
using PatientService.Features.Metrics.DTOs;
using PatientService.Features.Metrics.Queries;
using Microsoft.AspNetCore.Mvc;

namespace PatientService.Tests.Controllers;

public class MetricsControllerTests
{
    [Fact]
    public async Task ReturnsBadRequest_OnInvalidDateRange()
    {
        var mediator = new Mock<IMediator>();
        var controller = new MetricsController(NullLogger<MetricsController>.Instance, mediator.Object);
        var start = DateTime.UtcNow.Date;
        var end = start.AddDays(-1);
        var result = await controller.GetPatientMetrics(start, end);
        var bad = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("StartDate cannot be after EndDate", bad.Value!.ToString());
        mediator.Verify(m => m.Send(It.IsAny<GetPatientMetricsQuery>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SendsQuery_OnValidRequest()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<GetPatientMetricsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PatientMetricsResponse { TotalActivePatients = 1 });
        var controller = new MetricsController(NullLogger<MetricsController>.Instance, mediator.Object);
        var end = DateTime.UtcNow.Date;
        var start = end.AddDays(-7);
        var result = await controller.GetPatientMetrics(start, end);
        var ok = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<PatientMetricsResponse>(ok.Value);
        Assert.Equal(1, dto.TotalActivePatients);
        mediator.Verify(m => m.Send(It.Is<GetPatientMetricsQuery>(q => q.StartDate == start && q.EndDate == end), It.IsAny<CancellationToken>()), Times.Once);
    }
}
