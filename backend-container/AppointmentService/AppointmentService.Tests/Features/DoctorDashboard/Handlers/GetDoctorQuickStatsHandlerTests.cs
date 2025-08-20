using Moq;
using Xunit;
using AppointmentService.Features.DoctorDashboard.Handlers;
using AppointmentService.Features.DoctorDashboard.Queries;
using AppointmentService.Features.DoctorDashboard.Services;
using AppointmentService.Features.DoctorDashboard.DTOs;

namespace AppointmentService.Tests.Features.DoctorDashboard.Handlers;

public class GetDoctorQuickStatsHandlerTests
{
    private readonly Mock<IDoctorDashboardService> _mockService;
    private readonly GetDoctorQuickStatsHandler _handler;

    public GetDoctorQuickStatsHandlerTests()
    {
        _mockService = new Mock<IDoctorDashboardService>();
        _handler = new GetDoctorQuickStatsHandler(_mockService.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnQuickStats()
    {
        var query = new GetDoctorQuickStatsQuery { DoctorId = Guid.NewGuid() };
        var expectedResponse = new DoctorQuickStatsResponse
        {
            Stats = new List<DoctorQuickStatsDto>
            {
                new() { Label = "Patients Today", Value = 5 },
                new() { Label = "Total Patients", Value = 150 },
                new() { Label = "Visits this Month", Value = 45 },
                new() { Label = "Unread Messages", Value = 3 }
            }
        };

        _mockService.Setup(x => x.GetQuickStatsAsync(query.DoctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(4, result.Stats.Count);
        Assert.Equal(5, result.Stats.First(s => s.Label == "Patients Today").Value);
        Assert.Equal(150, result.Stats.First(s => s.Label == "Total Patients").Value);

        _mockService.Verify(x => x.GetQuickStatsAsync(query.DoctorId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
