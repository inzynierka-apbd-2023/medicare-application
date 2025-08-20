using Microsoft.EntityFrameworkCore;
using Xunit;
using AppointmentService.Data;
using AppointmentService.Features.Analytics.Handlers;
using AppointmentService.Features.Analytics.Queries;
using AppointmentService.Models;

namespace AppointmentService.Tests.Features.Analytics.Handlers;

public class GetDoctorPerformanceHandlerTests : IDisposable
{
    private readonly AppointmentDbContext _context;
    private readonly GetDoctorPerformanceHandler _handler;

    public GetDoctorPerformanceHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppointmentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppointmentDbContext(options);
        _handler = new GetDoctorPerformanceHandler(_context);
        
        SeedTestData();
    }

    private void SeedTestData()
    {
        var doctor1Id = "doctor-1";
        var doctor2Id = "doctor-2";
        var patient1Id = "patient-1";
        var patient2Id = "patient-2";
        var completedStatusId = "status-completed";
        var cancelledStatusId = "status-cancelled";
        var noShowStatusId = "status-noshow";
        var cardiology1Id = "spec-cardiology-1";
        var cardiology2Id = "spec-cardiology-2";

        // Add appointment statuses
        _context.ScheduleAppointmentStatuses.AddRange(
            new ScheduleAppointmentStatus { Id = completedStatusId, Name = "Completed" },
            new ScheduleAppointmentStatus { Id = cancelledStatusId, Name = "Cancelled" },
            new ScheduleAppointmentStatus { Id = noShowStatusId, Name = "No-Show" }
        );

        // Add user profiles
        _context.UserProfiles.AddRange(
            new UserProfile { User_Id = doctor1Id, FirstName = "John", LastName = "Smith", Email = "john@test.com", Created_At = DateTime.UtcNow, Updated_At = DateTime.UtcNow },
            new UserProfile { User_Id = doctor2Id, FirstName = "Jane", LastName = "Doe", Email = "jane@test.com", Created_At = DateTime.UtcNow, Updated_At = DateTime.UtcNow }
        );

        // Add doctors
        _context.Doctors.AddRange(
            new Doctor { Id = doctor1Id, License_Number = "LIC001", Years_Experience = 10 },
            new Doctor { Id = doctor2Id, License_Number = "LIC002", Years_Experience = 5 }
        );

        // Add specializations
        _context.Specializations.AddRange(
            new Specialization { Id = cardiology1Id, Name = "Cardiology", Service_Id = "service-1" },
            new Specialization { Id = cardiology2Id, Name = "Neurology", Service_Id = "service-2" }
        );

        // Add doctor specializations
        _context.DoctorSpecializations.AddRange(
            new DoctorSpecialization { Id = "ds-1", Doctor_Id = doctor1Id, Specialization_Id = cardiology1Id, Is_Primary = true },
            new DoctorSpecialization { Id = "ds-2", Doctor_Id = doctor2Id, Specialization_Id = cardiology2Id, Is_Primary = true }
        );

        // Add appointments for doctor 1
        var currentDate = DateTime.UtcNow;
        for (int i = 1; i <= 10; i++)
        {
            var appointmentId = $"appointment-{i}";
            var statusId = i <= 7 ? completedStatusId : (i <= 8 ? cancelledStatusId : noShowStatusId);
            
            _context.ScheduleAppointments.Add(new ScheduleAppointment
            {
                Id = appointmentId,
                Doctor_User_Id = doctor1Id,
                Patient_User_Id = i % 2 == 0 ? patient1Id : patient2Id,
                Day = currentDate.AddDays(-i),
                Duration_Minutes = 30,
                Schedule_Appointment_Status_Id = statusId,
                Schedule_Id = "schedule-1",
                Total_Cost = 150.00m,
                Created_At = DateTime.UtcNow,
                Updated_At = DateTime.UtcNow
            });

            // Add payments for completed appointments
            if (i <= 7)
            {
                _context.AppointmentPayments.Add(new AppointmentPayment
                {
                    Id = $"payment-{i}",
                    Amount = 150.00m,
                    Currency = "USD",
                    Status = "Paid",
                    Schedule_Appointment_Id = appointmentId,
                    Patient_Id = i % 2 == 0 ? patient1Id : patient2Id
                });
            }
        }

        // Add ratings for doctor 1
        for (int i = 1; i <= 5; i++)
        {
            _context.Rates.Add(new Rate
            {
                Id = $"rate-{i}",
                Rate_Value = (byte)(4 + (i % 2)), // Ratings of 4 and 5
                Doctor_User_Id = doctor1Id,
                Patient_User_Id = patient1Id,
                Appointment_Id = $"appointment-{i}",
                Rated_At = DateTime.UtcNow,
                Description = $"Great service {i}"
            });
        }

        // Add fewer appointments for doctor 2
        for (int i = 11; i <= 13; i++)
        {
            _context.ScheduleAppointments.Add(new ScheduleAppointment
            {
                Id = $"appointment-{i}",
                Doctor_User_Id = doctor2Id,
                Patient_User_Id = patient1Id,
                Day = currentDate.AddDays(-(i-10)),
                Duration_Minutes = 45,
                Schedule_Appointment_Status_Id = completedStatusId,
                Schedule_Id = "schedule-2",
                Total_Cost = 200.00m,
                Created_At = DateTime.UtcNow,
                Updated_At = DateTime.UtcNow
            });

            _context.AppointmentPayments.Add(new AppointmentPayment
            {
                Id = $"payment-{i}",
                Amount = 200.00m,
                Currency = "USD",
                Status = "Paid",
                Schedule_Appointment_Id = $"appointment-{i}",
                Patient_Id = patient1Id
            });
        }

        _context.SaveChanges();
    }

    [Fact]
    public async Task Handle_ShouldReturnDoctorPerformanceData_WhenDataExists()
    {
        // Arrange
        var query = new GetDoctorPerformanceQuery
        {
            StartDate = DateTime.UtcNow.AddDays(-15),
            EndDate = DateTime.UtcNow
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        var performanceList = result.ToList();

        // Assert
        Assert.Equal(2, performanceList.Count);

        var doctor1Performance = performanceList.First(p => p.Name.Contains("John"));
        Assert.Equal("John Smith", doctor1Performance.Name);
        Assert.Equal("Cardiology", doctor1Performance.Specialization);
        Assert.Equal(10, doctor1Performance.TotalAppointments);
        Assert.Equal(7, doctor1Performance.CompletedAppointments);
        Assert.Equal(1, doctor1Performance.CancelledAppointments);
        Assert.Equal(2, doctor1Performance.NoShowAppointments);
        Assert.Equal(4.6, doctor1Performance.AverageRating); // (4+5+4+5+4)/5 = 4.4, but depends on actual data
        Assert.Equal(5, doctor1Performance.TotalRatings);
        Assert.Equal(1050.00m, doctor1Performance.Revenue); // 7 completed * $150
        Assert.Equal(70.0, doctor1Performance.UtilizationRate); // 7/10 * 100
    }

    [Fact]
    public async Task Handle_ShouldFilterByDoctorId_WhenProvided()
    {
        // Arrange
        var query = new GetDoctorPerformanceQuery
        {
            StartDate = DateTime.UtcNow.AddDays(-15),
            EndDate = DateTime.UtcNow,
            DoctorId = "doctor-1"
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        var performanceList = result.ToList();

        // Assert
        Assert.Single(performanceList);
        Assert.Equal("John Smith", performanceList.First().Name);
    }

    [Fact]
    public async Task Handle_ShouldFilterBySpecialization_WhenProvided()
    {
        // Arrange
        var query = new GetDoctorPerformanceQuery
        {
            StartDate = DateTime.UtcNow.AddDays(-15),
            EndDate = DateTime.UtcNow,
            Specialization = "Cardiology"
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        var performanceList = result.ToList();

        // Assert
        Assert.Single(performanceList);
        Assert.Equal("Cardiology", performanceList.First().Specialization);
    }

    [Fact]
    public async Task Handle_ShouldCalculateCorrectUtilizationRate()
    {
        // Arrange
        var query = new GetDoctorPerformanceQuery
        {
            StartDate = DateTime.UtcNow.AddDays(-15),
            EndDate = DateTime.UtcNow,
            DoctorId = "doctor-1"
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        var performance = result.First();

        // Assert
        // Doctor 1: 7 completed out of 10 total = 70%
        Assert.Equal(70.0, performance.UtilizationRate);
    }

    [Fact]
    public async Task Handle_ShouldReturnZeroMetrics_WhenNoDoctorsExist()
    {
        // Arrange
        _context.ScheduleAppointments.RemoveRange(_context.ScheduleAppointments);
        _context.UserProfiles.RemoveRange(_context.UserProfiles);
        _context.Doctors.RemoveRange(_context.Doctors);
        _context.SaveChanges();

        var query = new GetDoctorPerformanceQuery
        {
            StartDate = DateTime.UtcNow.AddDays(-15),
            EndDate = DateTime.UtcNow
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        var performanceList = result.ToList();

        // Assert
        Assert.Empty(performanceList);
    }

    [Fact]
    public async Task Handle_ShouldHandleDefaultDateRange_WhenDatesNotProvided()
    {
        // Arrange
        var query = new GetDoctorPerformanceQuery(); // No dates provided

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        var performanceList = result.ToList();

        // Assert
        Assert.NotEmpty(performanceList);
        // Should still return data as most appointments are within last 30 days
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
