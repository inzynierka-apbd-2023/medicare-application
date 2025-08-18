using Microsoft.EntityFrameworkCore;
using Xunit;
using AppointmentService.Data;
using AppointmentService.Features.Analytics.Handlers;
using AppointmentService.Features.Analytics.Queries;
using AppointmentService.Models;
using AppointmentService.Services;
using Moq;

namespace AppointmentService.Tests.Features.Analytics;

public class GetAppointmentAnalyticsHandlerTests
{
    private AppointmentDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppointmentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppointmentDbContext(options);
    }

    private void SeedTestData(AppointmentDbContext context)
    {
        // Seed test data
        var doctorId = Guid.NewGuid().ToString();
        var patientId = Guid.NewGuid().ToString();
        var statusId = Guid.NewGuid().ToString();
        var scheduleId = Guid.NewGuid().ToString();
        var specializationId = Guid.NewGuid().ToString();

        // Add users
        context.Users.AddRange(
            new User { Id = doctorId, Role_Id = "doctor-role", Schedule_Id = scheduleId, Is_Active = true, Created_At = DateTime.UtcNow, Updated_At = DateTime.UtcNow },
            new User { Id = patientId, Role_Id = "patient-role", Schedule_Id = scheduleId, Is_Active = true, Created_At = DateTime.UtcNow, Updated_At = DateTime.UtcNow }
        );

        // Add user profiles
        context.UserProfiles.AddRange(
            new UserProfile { User_Id = doctorId, FirstName = "John", LastName = "Smith", Email = "john.smith@test.com", Created_At = DateTime.UtcNow, Updated_At = DateTime.UtcNow },
            new UserProfile { User_Id = patientId, FirstName = "Jane", LastName = "Doe", Email = "jane.doe@test.com", Created_At = DateTime.UtcNow, Updated_At = DateTime.UtcNow }
        );

        // Add doctor
        context.Doctors.Add(new Doctor { Id = doctorId, License_Number = "DOC123", Years_Experience = 10 });

        // Add patient
        context.Patients.Add(new Patient { Id = patientId, General_Doctor_Id = doctorId, Medical_Record_Number = "PAT123" });

        // Add specialization
        context.Specializations.Add(new Specialization { Id = specializationId, Name = "Cardiology", Service_Id = Guid.NewGuid().ToString(), Is_Active = true });

        // Add doctor specialization
        context.DoctorSpecializations.Add(new DoctorSpecialization { Id = Guid.NewGuid().ToString(), Doctor_Id = doctorId, Specialization_Id = specializationId, Is_Primary = true });

        // Add appointment status
        context.ScheduleAppointmentStatuses.Add(new ScheduleAppointmentStatus { Id = statusId, Name = "completed", Description = "Completed", Color_Code = "#28a745" });

        // Add appointments
        var appointmentId = Guid.NewGuid().ToString();
        var appointment = new ScheduleAppointment
        {
            Id = appointmentId,
            Schedule_Id = scheduleId,
            Day = DateTime.UtcNow.AddDays(-1),
            Duration_Minutes = 30,
            Doctor_User_Id = doctorId,
            Patient_User_Id = patientId,
            Schedule_Appointment_Status_Id = statusId,
            Total_Cost = 100,
            Created_At = DateTime.UtcNow,
            Updated_At = DateTime.UtcNow
        };
        context.ScheduleAppointments.Add(appointment);

        // Add payment
        context.AppointmentPayments.Add(new AppointmentPayment
        {
            Id = Guid.NewGuid().ToString(),
            Amount = 100,
            Currency = "USD",
            Status = "Paid",
            Schedule_Appointment_Id = appointmentId,
            Patient_Id = patientId,
            Paid_At = DateTime.UtcNow
        });

        // Add rating
        context.Rates.Add(new Rate
        {
            Id = Guid.NewGuid().ToString(),
            Rate_Value = 5,
            Doctor_User_Id = doctorId,
            Patient_User_Id = patientId,
            Appointment_Id = appointmentId,
            Rated_At = DateTime.UtcNow
        });

        context.SaveChanges();
    }

    [Fact]
    public async Task Handle_ValidQuery_ReturnsAnalyticsResponse()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        SeedTestData(context);

        var mockNotificationService = new Mock<INotificationService>();
        mockNotificationService.Setup(x => x.CreateNotificationAsync(It.IsAny<CreateNotificationRequest>()))
                              .Returns(Task.CompletedTask);

        var handler = new GetAppointmentAnalyticsHandler(context, mockNotificationService.Object);
        var query = new GetAppointmentAnalyticsQuery
        {
            StartDate = DateTime.UtcNow.AddDays(-7),
            EndDate = DateTime.UtcNow
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Metrics);
        Assert.NotEmpty(result.DoctorPerformance);
        Assert.NotEmpty(result.SpecializationStats);
        Assert.NotNull(result.TimeAnalysis);

        // Verify notification was created
        mockNotificationService.Verify(x => x.CreateNotificationAsync(It.IsAny<CreateNotificationRequest>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithDoctorFilter_FiltersDataCorrectly()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        SeedTestData(context);

        var doctorId = context.Doctors.First().Id;
        var mockNotificationService = new Mock<INotificationService>();
        mockNotificationService.Setup(x => x.CreateNotificationAsync(It.IsAny<CreateNotificationRequest>()))
                              .Returns(Task.CompletedTask);

        var handler = new GetAppointmentAnalyticsHandler(context, mockNotificationService.Object);
        var query = new GetAppointmentAnalyticsQuery
        {
            StartDate = DateTime.UtcNow.AddDays(-7),
            EndDate = DateTime.UtcNow,
            DoctorId = doctorId
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.DoctorPerformance);
        Assert.Equal(doctorId, result.DoctorPerformance.First().Id);
    }

    [Fact]
    public async Task Handle_MetricsCalculation_ReturnsCorrectValues()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        SeedTestData(context);

        var mockNotificationService = new Mock<INotificationService>();
        mockNotificationService.Setup(x => x.CreateNotificationAsync(It.IsAny<CreateNotificationRequest>()))
                              .Returns(Task.CompletedTask);

        var handler = new GetAppointmentAnalyticsHandler(context, mockNotificationService.Object);
        var query = new GetAppointmentAnalyticsQuery
        {
            StartDate = DateTime.UtcNow.AddDays(-7),
            EndDate = DateTime.UtcNow
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        var totalAppointmentsMetric = result.Metrics.FirstOrDefault(m => m.Title == "Total Appointments");
        Assert.NotNull(totalAppointmentsMetric);
        Assert.Equal(1, totalAppointmentsMetric.Value); // We seeded one appointment

        var completedMetric = result.Metrics.FirstOrDefault(m => m.Title == "Completed");
        Assert.NotNull(completedMetric);
        Assert.Equal(1, completedMetric.Value); // Our seeded appointment is completed

        var revenueMetric = result.Metrics.FirstOrDefault(m => m.Title == "Total Revenue");
        Assert.NotNull(revenueMetric);
        Assert.Equal(100, revenueMetric.Value); // Our seeded payment is $100
    }

    [Fact]
    public async Task Handle_DoctorPerformance_CalculatesCorrectUtilizationRate()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        SeedTestData(context);

        var mockNotificationService = new Mock<INotificationService>();
        mockNotificationService.Setup(x => x.CreateNotificationAsync(It.IsAny<CreateNotificationRequest>()))
                              .Returns(Task.CompletedTask);

        var handler = new GetAppointmentAnalyticsHandler(context, mockNotificationService.Object);
        var query = new GetAppointmentAnalyticsQuery
        {
            StartDate = DateTime.UtcNow.AddDays(-7),
            EndDate = DateTime.UtcNow
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        var doctorPerformance = result.DoctorPerformance.First();
        Assert.Equal(100.0, doctorPerformance.UtilizationRate); // 1 completed out of 1 total = 100%
        Assert.Equal(5.0, doctorPerformance.AverageRating); // Our seeded rating is 5
        Assert.Equal(100, doctorPerformance.Revenue); // Our seeded payment is $100
    }

    [Fact]
    public async Task Handle_SpecializationStats_GroupsDataCorrectly()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        SeedTestData(context);

        var mockNotificationService = new Mock<INotificationService>();
        mockNotificationService.Setup(x => x.CreateNotificationAsync(It.IsAny<CreateNotificationRequest>()))
                              .Returns(Task.CompletedTask);

        var handler = new GetAppointmentAnalyticsHandler(context, mockNotificationService.Object);
        var query = new GetAppointmentAnalyticsQuery
        {
            StartDate = DateTime.UtcNow.AddDays(-7),
            EndDate = DateTime.UtcNow
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        var cardiologyStats = result.SpecializationStats.FirstOrDefault(s => s.Specialization == "Cardiology");
        Assert.NotNull(cardiologyStats);
        Assert.Equal(1, cardiologyStats.TotalAppointments);
        Assert.Equal(1, cardiologyStats.TotalDoctors);
        Assert.Equal(100.0, cardiologyStats.CompletionRate); // 1 completed out of 1 total
    }

    [Fact]
    public async Task Handle_TimeSlotAnalysis_GeneratesCorrectTimeSlots()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        SeedTestData(context);

        var mockNotificationService = new Mock<INotificationService>();
        mockNotificationService.Setup(x => x.CreateNotificationAsync(It.IsAny<CreateNotificationRequest>()))
                              .Returns(Task.CompletedTask);

        var handler = new GetAppointmentAnalyticsHandler(context, mockNotificationService.Object);
        var query = new GetAppointmentAnalyticsQuery
        {
            StartDate = DateTime.UtcNow.AddDays(-7),
            EndDate = DateTime.UtcNow
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result.TimeAnalysis);
        Assert.NotEmpty(result.TimeAnalysis.TimeSlots);
        Assert.NotEmpty(result.TimeAnalysis.WeeklyData);
        Assert.Equal(7, result.TimeAnalysis.WeeklyData.Count()); // 7 days of week
    }

    [Fact]
    public async Task Handle_EmptyDatabase_ReturnsEmptyResults()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        // Don't seed any data

        var mockNotificationService = new Mock<INotificationService>();
        mockNotificationService.Setup(x => x.CreateNotificationAsync(It.IsAny<CreateNotificationRequest>()))
                              .Returns(Task.CompletedTask);

        var handler = new GetAppointmentAnalyticsHandler(context, mockNotificationService.Object);
        var query = new GetAppointmentAnalyticsQuery
        {
            StartDate = DateTime.UtcNow.AddDays(-7),
            EndDate = DateTime.UtcNow
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Metrics); // Metrics should still be returned with zero values
        Assert.Empty(result.DoctorPerformance);
        Assert.Empty(result.SpecializationStats);

        // Check that total appointments metric shows 0
        var totalAppointmentsMetric = result.Metrics.FirstOrDefault(m => m.Title == "Total Appointments");
        Assert.NotNull(totalAppointmentsMetric);
        Assert.Equal(0, totalAppointmentsMetric.Value);
    }
}
