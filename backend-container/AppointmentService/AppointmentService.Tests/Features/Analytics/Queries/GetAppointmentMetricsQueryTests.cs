using Xunit;
using AppointmentService.Features.Analytics.Queries;

namespace AppointmentService.Tests.Features.Analytics.Queries;

public class GetAppointmentMetricsQueryTests
{
    [Fact]
    public void GetAppointmentMetricsQuery_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var query = new GetAppointmentMetricsQuery();

        // Assert
        Assert.Null(query.StartDate);
        Assert.Null(query.EndDate);
        Assert.Null(query.DoctorId);
    }

    [Fact]
    public void GetAppointmentMetricsQuery_ShouldSetProperties()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;
        var doctorId = "doctor-123";

        // Act
        var query = new GetAppointmentMetricsQuery
        {
            StartDate = startDate,
            EndDate = endDate,
            DoctorId = doctorId
        };

        // Assert
        Assert.Equal(startDate, query.StartDate);
        Assert.Equal(endDate, query.EndDate);
        Assert.Equal(doctorId, query.DoctorId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void GetAppointmentMetricsQuery_ShouldHandleEmptyDoctorId(string? doctorId)
    {
        // Arrange & Act
        var query = new GetAppointmentMetricsQuery
        {
            DoctorId = doctorId
        };

        // Assert
        Assert.Equal(doctorId, query.DoctorId);
    }

    [Fact]
    public void GetAppointmentMetricsQuery_ShouldHandleValidGuidString()
    {
        // Arrange
        var guidString = Guid.NewGuid().ToString();

        // Act
        var query = new GetAppointmentMetricsQuery
        {
            DoctorId = guidString
        };

        // Assert
        Assert.Equal(guidString, query.DoctorId);
        Assert.True(Guid.TryParse(query.DoctorId, out _));
    }
}
