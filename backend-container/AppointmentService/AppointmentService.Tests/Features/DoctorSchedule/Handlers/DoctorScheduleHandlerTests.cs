using Moq;
using Xunit;
using AppointmentService.Features.DoctorSchedule.Handlers;
using AppointmentService.Features.DoctorSchedule.Queries;
using AppointmentService.Features.DoctorSchedule.Commands;
using AppointmentService.Features.DoctorSchedule.Services;
using AppointmentService.Features.DoctorSchedule.DTOs;

namespace AppointmentService.Tests.Features.DoctorSchedule.Handlers;

public class GetDoctorScheduleHandlerTests
{
    private readonly Mock<IDoctorScheduleService> _mockService;
    private readonly GetDoctorScheduleHandler _handler;

    public GetDoctorScheduleHandlerTests()
    {
        _mockService = new Mock<IDoctorScheduleService>();
        _handler = new GetDoctorScheduleHandler(_mockService.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnScheduleResponse()
    {
        // Arrange
        var query = new GetDoctorScheduleQuery
        {
            DoctorId = Guid.NewGuid(),
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(7),
            Status = "Scheduled"
        };

        var expectedResponse = new DoctorScheduleResponse
        {
            Schedule = new List<DoctorScheduleEventDto>
            {
                new DoctorScheduleEventDto
                {
                    Id = Guid.NewGuid().ToString(),
                    PatientId = Guid.NewGuid().ToString(),
                    PatientName = "John Doe",
                    PatientAge = 30,
                    PatientPhone = "+1234567890",
                    PatientEmail = "john.doe@test.com",
                    AppointmentType = "Consultation",
                    Date = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                    Time = DateTime.UtcNow.ToString("HH:mm"),
                    Duration = 60,
                    Status = "scheduled",
                    ChiefComplaint = "Regular checkup",
                    Notes = "Test appointment"
                }
            },
            TotalCount = 1
        };

        _mockService.Setup(x => x.GetDoctorScheduleAsync(
            query.DoctorId,
            query.StartDate,
            query.EndDate,
            query.Status,
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResponse.TotalCount, result.TotalCount);
        Assert.Single(result.Schedule);
        Assert.Equal("John Doe", result.Schedule.First().PatientName);

        _mockService.Verify(x => x.GetDoctorScheduleAsync(
            query.DoctorId,
            query.StartDate,
            query.EndDate,
            query.Status,
            It.IsAny<CancellationToken>()), Times.Once);
    }
}

public class GetTodaysAppointmentsHandlerTests
{
    private readonly Mock<IDoctorScheduleService> _mockService;
    private readonly GetTodaysAppointmentsHandler _handler;

    public GetTodaysAppointmentsHandlerTests()
    {
        _mockService = new Mock<IDoctorScheduleService>();
        _handler = new GetTodaysAppointmentsHandler(_mockService.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnTodaysAppointments()
    {
        // Arrange
        var query = new GetTodaysAppointmentsQuery { DoctorId = Guid.NewGuid() };
        var today = DateTime.UtcNow;

        var expectedResponse = new DoctorScheduleResponse
        {
            Schedule = new List<DoctorScheduleEventDto>
            {
                new DoctorScheduleEventDto
                {
                    Id = Guid.NewGuid().ToString(),
                    PatientId = Guid.NewGuid().ToString(),
                    PatientName = "Jane Doe",
                    Date = today.ToString("yyyy-MM-dd"),
                    Time = today.ToString("HH:mm")
                }
            },
            TotalCount = 1
        };

        _mockService.Setup(x => x.GetTodaysAppointmentsAsync(
            query.DoctorId,
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Schedule);
        Assert.Equal("Jane Doe", result.Schedule.First().PatientName);
    }
}

public class GetAppointmentDetailsHandlerTests
{
    private readonly Mock<IDoctorScheduleService> _mockService;
    private readonly GetAppointmentDetailsHandler _handler;

    public GetAppointmentDetailsHandlerTests()
    {
        _mockService = new Mock<IDoctorScheduleService>();
        _handler = new GetAppointmentDetailsHandler(_mockService.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnAppointmentDetails()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();
        var query = new GetAppointmentDetailsQuery { AppointmentId = appointmentId };

        var expectedAppointment = new DoctorScheduleEventDto
        {
            Id = appointmentId.ToString(),
            PatientId = Guid.NewGuid().ToString(),
            PatientName = "John Smith",
            PatientAge = 45,
            ChiefComplaint = "Chest pain",
            MedicalHistory = new List<string> { "Hypertension" },
            Allergies = new List<string> { "Penicillin" },
            CurrentMedications = new List<string> { "Lisinopril" }
        };

        _mockService.Setup(x => x.GetAppointmentDetailsAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedAppointment);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(appointmentId.ToString(), result.Id);
        Assert.Equal("John Smith", result.PatientName);
        Assert.Equal("Chest pain", result.ChiefComplaint);
        Assert.Contains("Hypertension", result.MedicalHistory);
    }
}

public class UpdateAppointmentStatusHandlerTests
{
    private readonly Mock<IDoctorScheduleService> _mockService;
    private readonly UpdateAppointmentStatusHandler _handler;

    public UpdateAppointmentStatusHandlerTests()
    {
        _mockService = new Mock<IDoctorScheduleService>();
        _handler = new UpdateAppointmentStatusHandler(_mockService.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnTrue_WhenUpdateSucceeds()
    {
        // Arrange
        var command = new UpdateAppointmentStatusCommand
        {
            AppointmentId = Guid.NewGuid(),
            Status = "Completed",
            Notes = "Appointment completed successfully"
        };

        _mockService.Setup(x => x.UpdateAppointmentStatusAsync(
            command.AppointmentId,
            command.Status,
            command.Notes,
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        _mockService.Verify(x => x.UpdateAppointmentStatusAsync(
            command.AppointmentId,
            command.Status,
            command.Notes,
            It.IsAny<CancellationToken>()), Times.Once);
    }
}

public class AddAppointmentNotesHandlerTests
{
    private readonly Mock<IDoctorScheduleService> _mockService;
    private readonly AddAppointmentNotesHandler _handler;

    public AddAppointmentNotesHandlerTests()
    {
        _mockService = new Mock<IDoctorScheduleService>();
        _handler = new AddAppointmentNotesHandler(_mockService.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnTrue_WhenNotesAddedSuccessfully()
    {
        // Arrange
        var command = new AddAppointmentNotesCommand
        {
            AppointmentId = Guid.NewGuid(),
            Notes = "Additional notes for the appointment"
        };

        _mockService.Setup(x => x.AddAppointmentNotesAsync(
            command.AppointmentId,
            command.Notes,
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        _mockService.Verify(x => x.AddAppointmentNotesAsync(
            command.AppointmentId,
            command.Notes,
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
