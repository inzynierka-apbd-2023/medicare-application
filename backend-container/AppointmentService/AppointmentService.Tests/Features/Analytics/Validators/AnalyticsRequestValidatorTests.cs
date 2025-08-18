using Xunit;
using System.ComponentModel.DataAnnotations;
using AppointmentService.Features.Analytics.Validators;

namespace AppointmentService.Tests.Features.Analytics.Validators;

public class AnalyticsRequestValidatorTests
{
    [Fact]
    public void ValidateDateRange_ValidRange_ReturnsSuccess()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;

        // Act
        var result = AnalyticsRequestValidator.ValidateDateRange(startDate, endDate);

        // Assert
        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void ValidateDateRange_StartDateAfterEndDate_ReturnsError()
    {
        // Arrange
        var startDate = DateTime.UtcNow;
        var endDate = DateTime.UtcNow.AddDays(-30);

        // Act
        var result = AnalyticsRequestValidator.ValidateDateRange(startDate, endDate);

        // Assert
        Assert.NotEqual(ValidationResult.Success, result);
        Assert.Equal("Start date cannot be later than end date", result.ErrorMessage);
    }

    [Fact]
    public void ValidateDateRange_StartDateInFuture_ReturnsError()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(1);
        var endDate = DateTime.UtcNow.AddDays(2);

        // Act
        var result = AnalyticsRequestValidator.ValidateDateRange(startDate, endDate);

        // Assert
        Assert.NotEqual(ValidationResult.Success, result);
        Assert.Equal("Start date cannot be in the future", result.ErrorMessage);
    }

    [Fact]
    public void ValidateDateRange_RangeTooLarge_ReturnsError()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-400);
        var endDate = DateTime.UtcNow;

        // Act
        var result = AnalyticsRequestValidator.ValidateDateRange(startDate, endDate);

        // Assert
        Assert.NotEqual(ValidationResult.Success, result);
        Assert.Equal("Date range cannot exceed 365 days", result.ErrorMessage);
    }

    [Fact]
    public void ValidateDateRange_NullDates_ReturnsSuccess()
    {
        // Act
        var result = AnalyticsRequestValidator.ValidateDateRange(null, null);

        // Assert
        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void ValidateDoctorId_ValidGuid_ReturnsSuccess()
    {
        // Arrange
        var doctorId = Guid.NewGuid().ToString();

        // Act
        var result = AnalyticsRequestValidator.ValidateDoctorId(doctorId);

        // Assert
        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void ValidateDoctorId_InvalidGuid_ReturnsError()
    {
        // Arrange
        var doctorId = "invalid-guid";

        // Act
        var result = AnalyticsRequestValidator.ValidateDoctorId(doctorId);

        // Assert
        Assert.NotEqual(ValidationResult.Success, result);
        Assert.Equal("Doctor ID must be a valid GUID", result.ErrorMessage);
    }

    [Fact]
    public void ValidateDoctorId_NullOrEmpty_ReturnsSuccess()
    {
        // Act & Assert
        Assert.Equal(ValidationResult.Success, AnalyticsRequestValidator.ValidateDoctorId(null));
        Assert.Equal(ValidationResult.Success, AnalyticsRequestValidator.ValidateDoctorId(""));
    }

    [Fact]
    public void ValidateSpecialization_ValidName_ReturnsSuccess()
    {
        // Arrange
        var specialization = "Cardiology";

        // Act
        var result = AnalyticsRequestValidator.ValidateSpecialization(specialization);

        // Assert
        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void ValidateSpecialization_TooLong_ReturnsError()
    {
        // Arrange
        var specialization = new string('a', 201); // 201 characters

        // Act
        var result = AnalyticsRequestValidator.ValidateSpecialization(specialization);

        // Assert
        Assert.NotEqual(ValidationResult.Success, result);
        Assert.Equal("Specialization name cannot exceed 200 characters", result.ErrorMessage);
    }

    [Fact]
    public void ValidateSpecialization_InvalidCharacters_ReturnsError()
    {
        // Arrange
        var specialization = "Cardiology123!";

        // Act
        var result = AnalyticsRequestValidator.ValidateSpecialization(specialization);

        // Assert
        Assert.NotEqual(ValidationResult.Success, result);
        Assert.Equal("Specialization name can only contain letters and spaces", result.ErrorMessage);
    }

    [Fact]
    public void ValidateSpecialization_WithSpaces_ReturnsSuccess()
    {
        // Arrange
        var specialization = "Internal Medicine";

        // Act
        var result = AnalyticsRequestValidator.ValidateSpecialization(specialization);

        // Assert
        Assert.Equal(ValidationResult.Success, result);
    }

    [Theory]
    [InlineData("scheduled")]
    [InlineData("confirmed")]
    [InlineData("cancelled")]
    [InlineData("completed")]
    [InlineData("no-show")]
    [InlineData("SCHEDULED")] // Case insensitive
    [InlineData("Completed")] // Case insensitive
    public void ValidateStatus_ValidStatus_ReturnsSuccess(string status)
    {
        // Act
        var result = AnalyticsRequestValidator.ValidateStatus(status);

        // Assert
        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void ValidateStatus_InvalidStatus_ReturnsError()
    {
        // Arrange
        var status = "invalid-status";

        // Act
        var result = AnalyticsRequestValidator.ValidateStatus(status);

        // Assert
        Assert.NotEqual(ValidationResult.Success, result);
        Assert.Contains("Status must be one of:", result.ErrorMessage);
    }

    [Fact]
    public void ValidateAnalyticsRequest_AllValidParameters_ReturnsEmptyList()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;
        var doctorId = Guid.NewGuid().ToString();
        var specialization = "Cardiology";
        var status = "completed";

        // Act
        var results = AnalyticsRequestValidator.ValidateAnalyticsRequest(
            startDate, endDate, doctorId, specialization, status);

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void ValidateAnalyticsRequest_MultipleInvalidParameters_ReturnsMultipleErrors()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(1); // Future date
        var endDate = DateTime.UtcNow.AddDays(-1); // Before start date
        var doctorId = "invalid-guid";
        var specialization = "Cardiology123!"; // Invalid characters
        var status = "invalid-status";

        // Act
        var results = AnalyticsRequestValidator.ValidateAnalyticsRequest(
            startDate, endDate, doctorId, specialization, status);

        // Assert
        Assert.NotEmpty(results);
        Assert.True(results.Count >= 4); // Should have multiple validation errors
    }

    [Fact]
    public void ValidateAnalyticsRequest_NullParameters_ReturnsEmptyList()
    {
        // Act
        var results = AnalyticsRequestValidator.ValidateAnalyticsRequest(
            null, null, null, null, null);

        // Assert
        Assert.Empty(results);
    }
}
