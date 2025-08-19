using Microsoft.Extensions.Logging.Abstractions;
using AppointmentService.Controllers;
using Xunit;
using MediatR;
using Moq;
using AppointmentService.Features.Metrics.DTOs;
using AppointmentService.Features.Metrics.Queries;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentService.Tests.Features.Metrics;

public class MetricsControllerTests
{
    [Fact]
    public async Task RejectsInvalidRange()
    {
        var mediator = new Mock<IMediator>();
        var ctrl = new MetricsController(NullLogger<MetricsController>.Instance, mediator.Object);
        var result = await ctrl.GetMetrics(DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(-1));
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task SendsQueryOnValid()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<GetAppointmentMetricsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AppointmentMetricsResponse());
        var ctrl = new MetricsController(NullLogger<MetricsController>.Instance, mediator.Object);
        var end = DateTime.UtcNow.Date; var start = end.AddDays(-5);
        var result = await ctrl.GetMetrics(start, end);
        Assert.IsType<OkObjectResult>(result.Result);
        mediator.Verify(m => m.Send(It.Is<GetAppointmentMetricsQuery>(q => q.StartDate==start && q.EndDate==end), It.IsAny<CancellationToken>()), Times.Once);
    }
}
