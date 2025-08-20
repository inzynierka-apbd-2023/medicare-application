using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;
using Xunit;
using AppointmentService.Data;
using AppointmentService.Features.DoctorSchedule.Services;
using AppointmentService.Features.DoctorSchedule.DTOs;
using AppointmentService.Models;

namespace AppointmentService.Tests.Features.DoctorSchedule.Services;

public class DoctorScheduleServiceTests : IDisposable
{
    private readonly AppointmentDbContext _context;
    private readonly DoctorScheduleService _service;
    private readonly Mock<IPatientService> _mockPatientService;
    private readonly Mock<IMedicalRecordsService> _mockMedicalRecordsService;

    public DoctorScheduleServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppointmentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppointmentDbContext(options);
        _mockPatientService = new Mock<IPatientService>();
        _mockMedicalRecordsService = new Mock<IMedicalRecordsService>();

        _service = new DoctorScheduleService(
            _context,
            _mockPatientService.Object,
            _mockMedicalRecordsService.Object);

        SeedTestData();
    }

    private void SeedTestData()
    {
        var doctorId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();

        var appointment = new Appointment
        {
            Id = appointmentId.ToString(),
            PatientId = patientId.ToString(),
            DoctorId = doctorId.ToString(),
            ScheduledAt = DateTime.UtcNow.AddHours(1),
            ScheduledEndAt = DateTime.UtcNow.AddHours(2),
            Status = "Scheduled",
            AppointmentType = "Consultation",
            ChiefComplaint = "Regular checkup",
            Notes = "Test appointment",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Appointments.Add(appointment);
        _context.SaveChanges();

        // Setup mock responses
        var mockPatient = new PatientDto
        {
            Id = patientId.ToString(),
            UserId = Guid.NewGuid().ToString(),
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@test.com",
            PhoneNumber = "+1234567890",
            DateOfBirth = DateTime.UtcNow.AddYears(-30)
        };

        var mockMedicalRecord = new MedicalRecordDto
        {
            MedicalHistory = new List<string> { "Hypertension", "Diabetes" },
            Allergies = new List<string> { "Penicillin" },
            CurrentMedications = new List<string> { "Metformin", "Lisinopril" }
        };

        _mockPatientService.Setup(x => x.GetPatientAsync(patientId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockPatient);

        _mockMedicalRecordsService.Setup(x => x.GetMedicalRecordAsync(patientId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockMedicalRecord);
    }

    [Fact]
    public async Task GetDoctorScheduleAsync_ShouldReturnScheduleWithEnrichedData()
    {
        // Arrange
        var doctorId = Guid.Parse(_context.Appointments.First().DoctorId);

        // Act
        var result = await _service.GetDoctorScheduleAsync(doctorId, null, null, null);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Schedule);
        
        var appointment = result.Schedule.First();
        Assert.Equal("John Doe", appointment.PatientName);
        Assert.True(appointment.PatientAge > 0);
        Assert.Equal("+1234567890", appointment.PatientPhone);
        Assert.Equal("john.doe@test.com", appointment.PatientEmail);
        Assert.Equal("Regular checkup", appointment.ChiefComplaint);
        Assert.Contains("Hypertension", appointment.MedicalHistory);
        Assert.Contains("Penicillin", appointment.Allergies);
        Assert.Contains("Metformin", appointment.CurrentMedications);
    }

    [Fact]
    public async Task GetDoctorScheduleAsync_WithDateRange_ShouldFilterAppointments()
    {
        // Arrange
        var doctorId = Guid.Parse(_context.Appointments.First().DoctorId);
        var startDate = DateTime.UtcNow.AddDays(-1);
        var endDate = DateTime.UtcNow.AddDays(1);

        // Act
        var result = await _service.GetDoctorScheduleAsync(doctorId, startDate, endDate, null);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Schedule);
    }

    [Fact]
    public async Task GetDoctorScheduleAsync_WithStatus_ShouldFilterByStatus()
    {
        // Arrange
        var doctorId = Guid.Parse(_context.Appointments.First().DoctorId);

        // Act
        var result = await _service.GetDoctorScheduleAsync(doctorId, null, null, "Scheduled");

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Schedule);
        Assert.Equal("scheduled", result.Schedule.First().Status);
    }

    [Fact]
    public async Task GetAppointmentDetailsAsync_ShouldReturnEnrichedAppointmentDetails()
    {
        // Arrange
        var appointmentId = Guid.Parse(_context.Appointments.First().Id);

        // Act
        var result = await _service.GetAppointmentDetailsAsync(appointmentId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("John Doe", result.PatientName);
        Assert.Equal("Regular checkup", result.ChiefComplaint);
        Assert.Contains("Hypertension", result.MedicalHistory);
    }

    [Fact]
    public async Task GetAppointmentDetailsAsync_WithNonExistentId_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _service.GetAppointmentDetailsAsync(nonExistentId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAppointmentStatusAsync_ShouldUpdateStatusAndNotes()
    {
        // Arrange
        var appointmentId = Guid.Parse(_context.Appointments.First().Id);
        var newStatus = "Completed";
        var newNotes = "Appointment completed successfully";

        // Act
        var result = await _service.UpdateAppointmentStatusAsync(appointmentId, newStatus, newNotes);

        // Assert
        Assert.True(result);

        var updatedAppointment = await _context.Appointments.FindAsync(appointmentId.ToString());
        Assert.NotNull(updatedAppointment);
        Assert.Equal(newStatus, updatedAppointment.Status);
        Assert.Equal(newNotes, updatedAppointment.Notes);
    }

    [Fact]
    public async Task UpdateAppointmentStatusAsync_WithNonExistentId_ShouldReturnFalse()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _service.UpdateAppointmentStatusAsync(nonExistentId, "Completed", "Test");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task AddAppointmentNotesAsync_ShouldAddNotes()
    {
        // Arrange
        var appointmentId = Guid.Parse(_context.Appointments.First().Id);
        var additionalNotes = "Additional notes added";

        // Act
        var result = await _service.AddAppointmentNotesAsync(appointmentId, additionalNotes);

        // Assert
        Assert.True(result);

        var updatedAppointment = await _context.Appointments.FindAsync(appointmentId.ToString());
        Assert.NotNull(updatedAppointment);
        Assert.Contains(additionalNotes, updatedAppointment.Notes);
    }

    [Fact]
    public async Task EnrichAppointmentWithPatientDataAsync_WithNullPatientData_ShouldHandleGracefully()
    {
        // Arrange
        _mockPatientService.Setup(x => x.GetPatientAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PatientDto?)null);

        _mockMedicalRecordsService.Setup(x => x.GetMedicalRecordAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MedicalRecordDto?)null);

        var doctorId = Guid.Parse(_context.Appointments.First().DoctorId);

        // Act
        var result = await _service.GetDoctorScheduleAsync(doctorId, null, null, null);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Schedule);
        
        var appointment = result.Schedule.First();
        Assert.Equal("Unknown Patient", appointment.PatientName);
        Assert.Equal(0, appointment.PatientAge);
        Assert.Equal("Not Available", appointment.PatientPhone);
        Assert.Null(appointment.PatientEmail);
        Assert.Empty(appointment.MedicalHistory);
        Assert.Empty(appointment.Allergies);
        Assert.Empty(appointment.CurrentMedications);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
