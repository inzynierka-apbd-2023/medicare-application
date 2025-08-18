using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MediatR;
using Moq;
using Xunit;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using AppointmentService.Controllers;
using AppointmentService.Features.Analytics.Queries;
using AppointmentService.Features.Analytics.DTOs;

namespace AppointmentService.Tests.Controllers;

public class AnalyticsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<ILogger<AnalyticsController>> _loggerMock;
    private readonly AnalyticsController _controller;

    public AnalyticsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _loggerMock = new Mock<ILogger<AnalyticsController>>();
        _controller = new AnalyticsController(_mediatorMock.Object, _loggerMock.Object);
    }

    private void SetupUserClaims(string userId, string role)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Role, role)
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    [Fact]
    public async Task GetDashboardAnalytics_AdminUser_ReturnsOkResult()
    {
        // Arrange
        SetupUserClaims("admin-user-id", "Admin");

        var expectedResponse = new AppointmentAnalyticsResponse
        {
            Metrics = new List<AppointmentMetricDto>
            {
                new() { Id = "1", Title = "Total Appointments", Value = 100, Change = 5.0, Period = "vs last month", Icon = "calendar" }
            }
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAppointmentAnalyticsQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetDashboardAnalytics();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(expectedResponse, okResult.Value);
        _mediatorMock.Verify(m => m.Send(It.IsAny<GetAppointmentAnalyticsQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetDashboardAnalytics_DoctorUser_RestrictsToOwnData()
    {
        // Arrange
        var doctorId = "doctor-user-id";
        SetupUserClaims(doctorId, "Doctor");

        var expectedResponse = new AppointmentAnalyticsResponse();
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAppointmentAnalyticsQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetDashboardAnalytics();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        _mediatorMock.Verify(m => m.Send(It.Is<GetAppointmentAnalyticsQuery>(q => q.DoctorId == doctorId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetDashboardAnalytics_UnauthorizedUser_ReturnsForbid()
    {
        // Arrange
        SetupUserClaims("unauthorized-user", "Patient");

        // Act
        var result = await _controller.GetDashboardAnalytics();

        // Assert
        Assert.IsType<ForbidResult>(result.Result);
        _mediatorMock.Verify(m => m.Send(It.IsAny<GetAppointmentAnalyticsQuery>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetAppointmentMetrics_ValidRequest_ReturnsMetrics()
    {
        // Arrange
        SetupUserClaims("admin-user-id", "Admin");

        var expectedMetrics = new List<AppointmentMetricDto>
        {
            new() { Id = "1", Title = "Total Appointments", Value = 50, Change = 2.5, Period = "vs last month", Icon = "calendar" },
            new() { Id = "2", Title = "Completed", Value = 45, Change = 3.1, Period = "vs last month", Icon = "trending" }
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAppointmentMetricsQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(expectedMetrics);

        // Act
        var result = await _controller.GetAppointmentMetrics();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(expectedMetrics, okResult.Value);
    }

    [Fact]
    public async Task GetAppointmentTrends_ValidRequest_ReturnsTrends()
    {
        // Arrange
        SetupUserClaims("admin-user-id", "Admin");

        var expectedTrends = new List<TrendDataDto>
        {
            new() { Date = "2024-01-01", Appointments = 10, Completed = 8, Cancelled = 1, NoShow = 1, Revenue = 800 }
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAppointmentTrendsQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(expectedTrends);

        // Act
        var result = await _controller.GetAppointmentTrends();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(expectedTrends, okResult.Value);
    }

    [Fact]
    public async Task GetDoctorPerformance_ReceptionistUser_ReturnsAllDoctorData()
    {
        // Arrange
        SetupUserClaims("receptionist-user-id", "Receptionist");

        var expectedPerformance = new List<DoctorPerformanceDto>
        {
            new() 
            { 
                Id = "doctor1", 
                Name = "Dr. John Smith", 
                Specialization = "Cardiology",
                TotalAppointments = 20,
                CompletedAppointments = 18,
                UtilizationRate = 90.0
            }
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetDoctorPerformanceQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(expectedPerformance);

        // Act
        var result = await _controller.GetDoctorPerformance();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(expectedPerformance, okResult.Value);
    }

    [Fact]
    public async Task GetSpecializationStats_DoctorUser_ReturnsForbid()
    {
        // Arrange
        SetupUserClaims("doctor-user-id", "Doctor");

        // Act
        var result = await _controller.GetSpecializationStats();

        // Assert
        Assert.IsType<ForbidResult>(result.Result);
        _mediatorMock.Verify(m => m.Send(It.IsAny<GetSpecializationStatsQuery>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetSpecializationStats_AdminUser_ReturnsStats()
    {
        // Arrange
        SetupUserClaims("admin-user-id", "Admin");

        var expectedStats = new List<SpecializationStatsDto>
        {
            new() 
            { 
                Specialization = "Cardiology", 
                TotalAppointments = 100, 
                TotalDoctors = 3,
                CompletionRate = 95.0
            }
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetSpecializationStatsQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(expectedStats);

        // Act
        var result = await _controller.GetSpecializationStats();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(expectedStats, okResult.Value);
    }

    [Fact]
    public async Task GetTimeSlotAnalysis_ValidRequest_ReturnsAnalysis()
    {
        // Arrange
        SetupUserClaims("admin-user-id", "Admin");

        var expectedAnalysis = new TimeSlotAnalysisDto
        {
            TimeSlots = new List<TimeSlotDataDto>
            {
                new() { Hour = 9, TimeSlot = "09:00-10:00", TotalAppointments = 5, Monday = 1, Tuesday = 1 }
            },
            WeeklyData = new List<DayDataDto>
            {
                new() { Day = "Monday", TotalAppointments = 10, PeakHour = "10:00-11:00", UtilizationRate = 85.0 }
            }
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetTimeSlotAnalysisQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(expectedAnalysis);

        // Act
        var result = await _controller.GetTimeSlotAnalysis();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(expectedAnalysis, okResult.Value);
    }

    [Theory]
    [InlineData("Admin", true)]
    [InlineData("Manager", true)]
    [InlineData("Doctor", true)]
    [InlineData("Receptionist", true)]
    [InlineData("Patient", false)]
    [InlineData("Unknown", false)]
    public async Task GetDashboardAnalytics_RoleBasedAccess_ReturnsCorrectResponse(string role, bool shouldHaveAccess)
    {
        // Arrange
        SetupUserClaims("test-user-id", role);

        if (shouldHaveAccess)
        {
            _mediatorMock.Setup(m => m.Send(It.IsAny<GetAppointmentAnalyticsQuery>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(new AppointmentAnalyticsResponse());
        }

        // Act
        var result = await _controller.GetDashboardAnalytics();

        // Assert
        if (shouldHaveAccess)
        {
            Assert.IsType<OkObjectResult>(result.Result);
            _mediatorMock.Verify(m => m.Send(It.IsAny<GetAppointmentAnalyticsQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        else
        {
            Assert.IsType<ForbidResult>(result.Result);
            _mediatorMock.Verify(m => m.Send(It.IsAny<GetAppointmentAnalyticsQuery>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }

    [Fact]
    public async Task GetDashboardAnalytics_MediatorThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        SetupUserClaims("admin-user-id", "Admin");

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAppointmentAnalyticsQuery>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _controller.GetDashboardAnalytics();

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        Assert.Equal("An error occurred while retrieving analytics data", statusCodeResult.Value);
    }

    [Fact]
    public async Task GetAppointmentMetrics_WithDateFilters_PassesCorrectParameters()
    {
        // Arrange
        SetupUserClaims("admin-user-id", "Admin");
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;
        var doctorId = "specific-doctor-id";

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAppointmentMetricsQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<AppointmentMetricDto>());

        // Act
        await _controller.GetAppointmentMetrics(startDate, endDate, doctorId);

        // Assert
        _mediatorMock.Verify(m => m.Send(It.Is<GetAppointmentMetricsQuery>(q => 
            q.StartDate == startDate && 
            q.EndDate == endDate && 
            q.DoctorId == doctorId), It.IsAny<CancellationToken>()), Times.Once);
    }
}
