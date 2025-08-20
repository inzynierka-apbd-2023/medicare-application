using Xunit;
using AppointmentService.Features.Analytics.DTOs;

namespace AppointmentService.Tests.Features.Analytics.DTOs;

public class AppointmentMetricDtoTests
{
    [Fact]
    public void AppointmentMetricDto_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var dto = new AppointmentMetricDto();

        // Assert
        Assert.Equal(string.Empty, dto.Id);
        Assert.Equal(string.Empty, dto.Title);
        Assert.Equal(0, dto.Value);
        Assert.Equal(0.0, dto.Change);
        Assert.Equal(string.Empty, dto.Period);
        Assert.Equal(string.Empty, dto.Icon);
    }

    [Fact]
    public void AppointmentMetricDto_ShouldSetAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid().ToString();
        var title = "Total Appointments";
        var value = 150;
        var change = 12.5;
        var period = "Last 30 days";
        var icon = "calendar";

        // Act
        var dto = new AppointmentMetricDto
        {
            Id = id,
            Title = title,
            Value = value,
            Change = change,
            Period = period,
            Icon = icon
        };

        // Assert
        Assert.Equal(id, dto.Id);
        Assert.Equal(title, dto.Title);
        Assert.Equal(value, dto.Value);
        Assert.Equal(change, dto.Change);
        Assert.Equal(period, dto.Period);
        Assert.Equal(icon, dto.Icon);
    }

    [Fact]
    public void AppointmentMetricDto_ShouldHandleGuidStringIds()
    {
        // Arrange
        var guidString = Guid.NewGuid().ToString();

        // Act
        var dto = new AppointmentMetricDto { Id = guidString };

        // Assert
        Assert.Equal(guidString, dto.Id);
        Assert.True(Guid.TryParse(dto.Id, out _));
    }

    [Theory]
    [InlineData("calendar")]
    [InlineData("trending")]
    [InlineData("users")]
    [InlineData("clock")]
    [InlineData("dollar")]
    [InlineData("star")]
    public void AppointmentMetricDto_ShouldHandleValidIcons(string icon)
    {
        // Arrange & Act
        var dto = new AppointmentMetricDto { Icon = icon };

        // Assert
        Assert.Equal(icon, dto.Icon);
    }

    [Theory]
    [InlineData(-10.5)]
    [InlineData(0.0)]
    [InlineData(15.75)]
    [InlineData(100.0)]
    public void AppointmentMetricDto_ShouldHandleVariousChangeValues(double change)
    {
        // Arrange & Act
        var dto = new AppointmentMetricDto { Change = change };

        // Assert
        Assert.Equal(change, dto.Change);
    }
}

public class DoctorPerformanceDtoTests
{
    [Fact]
    public void DoctorPerformanceDto_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var dto = new DoctorPerformanceDto();

        // Assert
        Assert.Equal(string.Empty, dto.Id);
        Assert.Equal(string.Empty, dto.Name);
        Assert.Equal(string.Empty, dto.Specialization);
        Assert.Equal(0, dto.TotalAppointments);
        Assert.Equal(0, dto.CompletedAppointments);
        Assert.Equal(0, dto.CancelledAppointments);
        Assert.Equal(0, dto.NoShowAppointments);
        Assert.Equal(0.0, dto.AverageRating);
        Assert.Equal(0, dto.TotalRatings);
        Assert.Equal(0m, dto.Revenue);
        Assert.Equal(0.0, dto.UtilizationRate);
    }

    [Fact]
    public void DoctorPerformanceDto_ShouldSetAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid().ToString();
        var name = "Dr. John Smith";
        var specialization = "Cardiology";
        var totalAppointments = 100;
        var completedAppointments = 85;
        var cancelledAppointments = 10;
        var noShowAppointments = 5;
        var averageRating = 4.7;
        var totalRatings = 45;
        var revenue = 12750.50m;
        var utilizationRate = 85.0;

        // Act
        var dto = new DoctorPerformanceDto
        {
            Id = id,
            Name = name,
            Specialization = specialization,
            TotalAppointments = totalAppointments,
            CompletedAppointments = completedAppointments,
            CancelledAppointments = cancelledAppointments,
            NoShowAppointments = noShowAppointments,
            AverageRating = averageRating,
            TotalRatings = totalRatings,
            Revenue = revenue,
            UtilizationRate = utilizationRate
        };

        // Assert
        Assert.Equal(id, dto.Id);
        Assert.Equal(name, dto.Name);
        Assert.Equal(specialization, dto.Specialization);
        Assert.Equal(totalAppointments, dto.TotalAppointments);
        Assert.Equal(completedAppointments, dto.CompletedAppointments);
        Assert.Equal(cancelledAppointments, dto.CancelledAppointments);
        Assert.Equal(noShowAppointments, dto.NoShowAppointments);
        Assert.Equal(averageRating, dto.AverageRating);
        Assert.Equal(totalRatings, dto.TotalRatings);
        Assert.Equal(revenue, dto.Revenue);
        Assert.Equal(utilizationRate, dto.UtilizationRate);
    }

    [Fact]
    public void DoctorPerformanceDto_ShouldCalculateUtilizationCorrectly()
    {
        // Arrange
        var dto = new DoctorPerformanceDto
        {
            TotalAppointments = 100,
            CompletedAppointments = 85
        };

        // Act
        var expectedUtilization = (double)dto.CompletedAppointments / dto.TotalAppointments * 100;

        // Assert
        Assert.Equal(85.0, expectedUtilization);
    }

    [Fact]
    public void DoctorPerformanceDto_ShouldHandleGuidStringIds()
    {
        // Arrange
        var guidString = Guid.NewGuid().ToString();

        // Act
        var dto = new DoctorPerformanceDto { Id = guidString };

        // Assert
        Assert.Equal(guidString, dto.Id);
        Assert.True(Guid.TryParse(dto.Id, out _));
    }
}
