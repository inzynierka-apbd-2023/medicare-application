using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;
using Xunit;
using AppointmentService.Features.DoctorSchedule.Services;
using AppointmentService.Features.DoctorSchedule.DTOs;

namespace AppointmentService.Tests.Features.DoctorSchedule.Services;

public class PatientServiceTests
{
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly PatientService _patientService;

    public PatientServiceTests()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        _mockConfiguration = new Mock<IConfiguration>();

        _mockConfiguration.Setup(x => x["Services:PatientService:BaseUrl"])
            .Returns("http://localhost:8081");

        _patientService = new PatientService(_httpClient, _mockConfiguration.Object);
    }

    [Fact]
    public async Task GetPatientAsync_ShouldReturnPatientDto_WhenApiReturnsSuccess()
    {
        // Arrange
        var patientId = Guid.NewGuid().ToString();
        var expectedPatient = new PatientDto
        {
            Id = patientId,
            UserId = Guid.NewGuid().ToString(),
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@test.com",
            PhoneNumber = "+1234567890",
            DateOfBirth = DateTime.UtcNow.AddYears(-30)
        };

        var jsonResponse = JsonSerializer.Serialize(expectedPatient);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, System.Text.Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Get && 
                    req.RequestUri!.ToString().Contains($"/api/patient/patients/{patientId}")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _patientService.GetPatientAsync(patientId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedPatient.Id, result.Id);
        Assert.Equal(expectedPatient.FirstName, result.FirstName);
        Assert.Equal(expectedPatient.LastName, result.LastName);
        Assert.Equal(expectedPatient.Email, result.Email);
        Assert.Equal(expectedPatient.PhoneNumber, result.PhoneNumber);
    }

    [Fact]
    public async Task GetPatientAsync_ShouldReturnNull_WhenApiReturnsNotFound()
    {
        // Arrange
        var patientId = Guid.NewGuid().ToString();
        var httpResponse = new HttpResponseMessage(HttpStatusCode.NotFound);

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _patientService.GetPatientAsync(patientId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetPatientAsync_ShouldReturnNull_WhenExceptionThrown()
    {
        // Arrange
        var patientId = Guid.NewGuid().ToString();

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        var result = await _patientService.GetPatientAsync(patientId);

        // Assert
        Assert.Null(result);
    }
}

public class MedicalRecordsServiceTests
{
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly MedicalRecordsService _medicalRecordsService;

    public MedicalRecordsServiceTests()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        _mockConfiguration = new Mock<IConfiguration>();

        _mockConfiguration.Setup(x => x["Services:MedicalRecordsService:BaseUrl"])
            .Returns("http://localhost:8083");

        _medicalRecordsService = new MedicalRecordsService(_httpClient, _mockConfiguration.Object);
    }

    [Fact]
    public async Task GetMedicalRecordAsync_ShouldReturnMedicalRecordDto_WhenApiReturnsSuccess()
    {
        // Arrange
        var patientId = Guid.NewGuid().ToString();
        var expectedRecord = new MedicalRecordDto
        {
            MedicalHistory = new List<string> { "Hypertension", "Diabetes" },
            Allergies = new List<string> { "Penicillin", "Shellfish" },
            CurrentMedications = new List<string> { "Metformin", "Lisinopril" }
        };

        var jsonResponse = JsonSerializer.Serialize(expectedRecord);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, System.Text.Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Get && 
                    req.RequestUri!.ToString().Contains($"/api/medical-records/patients/{patientId}")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _medicalRecordsService.GetMedicalRecordAsync(patientId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedRecord.MedicalHistory.Count, result.MedicalHistory.Count);
        Assert.Contains("Hypertension", result.MedicalHistory);
        Assert.Contains("Penicillin", result.Allergies);
        Assert.Contains("Metformin", result.CurrentMedications);
    }

    [Fact]
    public async Task GetMedicalRecordAsync_ShouldReturnNull_WhenApiReturnsNotFound()
    {
        // Arrange
        var patientId = Guid.NewGuid().ToString();
        var httpResponse = new HttpResponseMessage(HttpStatusCode.NotFound);

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _medicalRecordsService.GetMedicalRecordAsync(patientId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetMedicalRecordAsync_ShouldReturnNull_WhenExceptionThrown()
    {
        // Arrange
        var patientId = Guid.NewGuid().ToString();

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        var result = await _medicalRecordsService.GetMedicalRecordAsync(patientId);

        // Assert
        Assert.Null(result);
    }
}
