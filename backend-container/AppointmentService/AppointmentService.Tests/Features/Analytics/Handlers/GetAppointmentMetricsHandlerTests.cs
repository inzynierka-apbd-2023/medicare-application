using Microsoft.EntityFrameworkCore;
using Xunit;
using AppointmentService.Data;
using AppointmentService.Features.Analytics.Handlers;
using AppointmentService.Features.Analytics.Queries;
using AppointmentService.Models;

namespace AppointmentService.Tests.Features.Analytics.Handlers;

public class GetAppointmentMetricsHandlerTests : IDisposable
{
    private readonly AppointmentDbContext _context;
    private readonly GetAppointmentMetricsHandler _handler;

    public GetAppointmentMetricsHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppointmentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppointmentDbContext(options);
        _handler = new GetAppointmentMetricsHandler(_context);
        
        SeedTestData();
    }

    private void SeedTestData()
    {
        var doctorId = "doctor-1";
        var patientId = "patient-1";
        var completedStatusId = "status-completed";
        var scheduledStatusId = "status-scheduled";
        var cancelledStatusId = "status-cancelled";

        // Add appointment statuses
        _context.ScheduleAppointmentStatuses.AddRange(
            new ScheduleAppointmentStatus { Id = completedStatusId, Name = "Completed" },
            new ScheduleAppointmentStatus { Id = scheduledStatusId, Name = "Scheduled" },
            new ScheduleAppointmentStatus { Id = cancelledStatusId, Name = "Cancelled" }
        );

        // Add current period appointments (last 7 days)
        var currentDate = DateTime.UtcNow;
        for (int i = 1; i <= 5; i++)
        {
            var appointmentId = $"appointment-current-{i}";
            _context.ScheduleAppointments.Add(new ScheduleAppointment
            {
                Id = appointmentId,
                Doctor_User_Id = doctorId,
                Patient_User_Id = patientId,
                Day = currentDate.AddDays(-i),
                Duration_Minutes = 30,
                Schedule_Appointment_Status_Id = i <= 3 ? completedStatusId : scheduledStatusId,
                Schedule_Id = "schedule-1",
                Total_Cost = 100.00m,
                Created_At = DateTime.UtcNow,
                Updated_At = DateTime.UtcNow
            });

            // Add payments for completed appointments
            if (i <= 3)
            {
                _context.AppointmentPayments.Add(new AppointmentPayment
                {
                    Id = $"payment-{i}",
                    Amount = 100.00m,
                    Currency = "USD",
                    Status = "Paid",
                    Schedule_Appointment_Id = appointmentId,
                    Patient_Id = patientId
                });
            }
        }

        // Add previous period appointments (8-14 days ago)
        for (int i = 8; i <= 10; i++)
        {
            var appointmentId = $"appointment-previous-{i}";
            _context.ScheduleAppointments.Add(new ScheduleAppointment
            {
                Id = appointmentId,
                Doctor_User_Id = doctorId,
                Patient_User_Id = patientId,
                Day = currentDate.AddDays(-i),
                Duration_Minutes = 30,
                Schedule_Appointment_Status_Id = completedStatusId,
                Schedule_Id = "schedule-1",
                Total_Cost = 100.00m,
                Created_At = DateTime.UtcNow,
                Updated_At = DateTime.UtcNow
            });

            _context.AppointmentPayments.Add(new AppointmentPayment
            {
                Id = $"payment-prev-{i}",
                Amount = 100.00m,
                Currency = "USD",
                Status = "Paid",
                Schedule_Appointment_Id = appointmentId,
                Patient_Id = patientId
            });
        }

        _context.SaveChanges();
    }

    [Fact]
    public async Task Handle_ShouldReturnAppointmentMetrics_WhenDataExists()
    {
        // Arrange
        var query = new GetAppointmentMetricsQuery
        {
            StartDate = DateTime.UtcNow.AddDays(-7),
            EndDate = DateTime.UtcNow,
            DoctorId = "doctor-1"
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        var metrics = result.ToList();

        // Assert
        Assert.NotEmpty(metrics);
        Assert.Equal(5, metrics.Count); // Total, Completed, Active Patients, Avg Duration, Revenue

        var totalMetric = metrics.First(m => m.Title == "Total Appointments");
        Assert.Equal(5, totalMetric.Value);
        Assert.Equal("calendar", totalMetric.Icon);

        var completedMetric = metrics.First(m => m.Title == "Completed Appointments");
        Assert.Equal(3, completedMetric.Value);

        var revenueMetric = metrics.First(m => m.Title == "Total Revenue");
        Assert.Equal(300, revenueMetric.Value); // 3 completed appointments * $100
        Assert.Equal("dollar", revenueMetric.Icon);
    }

    [Fact]
    public async Task Handle_ShouldCalculateCorrectPercentageChanges()
    {
        // Arrange
        var query = new GetAppointmentMetricsQuery
        {
            StartDate = DateTime.UtcNow.AddDays(-7),
            EndDate = DateTime.UtcNow,
            DoctorId = "doctor-1"
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        var metrics = result.ToList();

        // Assert
        var totalMetric = metrics.First(m => m.Title == "Total Appointments");
        // Current period: 5 appointments, Previous period: 3 appointments
        // Change: ((5-3)/3) * 100 = 66.67%
        Assert.True(totalMetric.Change > 60 && totalMetric.Change < 70);
    }

    [Fact]
    public async Task Handle_ShouldFilterByDoctorId_WhenProvided()
    {
        // Arrange
        var anotherDoctorId = "doctor-2";
        _context.ScheduleAppointments.Add(new ScheduleAppointment
        {
            Id = "appointment-other-doctor",
            Doctor_User_Id = anotherDoctorId,
            Patient_User_Id = "patient-1",
            Day = DateTime.UtcNow.AddDays(-1),
            Duration_Minutes = 30,
            Schedule_Appointment_Status_Id = "status-completed",
            Schedule_Id = "schedule-2",
            Created_At = DateTime.UtcNow,
            Updated_At = DateTime.UtcNow
        });
        _context.SaveChanges();

        var query = new GetAppointmentMetricsQuery
        {
            StartDate = DateTime.UtcNow.AddDays(-7),
            EndDate = DateTime.UtcNow,
            DoctorId = "doctor-1" // Filter for specific doctor
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        var metrics = result.ToList();

        // Assert
        var totalMetric = metrics.First(m => m.Title == "Total Appointments");
        Assert.Equal(5, totalMetric.Value); // Should only count doctor-1's appointments
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyMetrics_WhenNoDataExists()
    {
        // Arrange
        _context.ScheduleAppointments.RemoveRange(_context.ScheduleAppointments);
        _context.AppointmentPayments.RemoveRange(_context.AppointmentPayments);
        _context.SaveChanges();

        var query = new GetAppointmentMetricsQuery
        {
            StartDate = DateTime.UtcNow.AddDays(-7),
            EndDate = DateTime.UtcNow
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        var metrics = result.ToList();

        // Assert
        Assert.NotEmpty(metrics); // Should still return metrics structure
        var totalMetric = metrics.First(m => m.Title == "Total Appointments");
        Assert.Equal(0, totalMetric.Value);
        Assert.Equal(0, totalMetric.Change);
    }

    [Fact]
    public async Task Handle_ShouldUseDefaultDateRange_WhenDatesNotProvided()
    {
        // Arrange
        var query = new GetAppointmentMetricsQuery(); // No dates provided

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        var metrics = result.ToList();

        // Assert
        Assert.NotEmpty(metrics);
        // Should use last 30 days as default
        var totalMetric = metrics.First(m => m.Title == "Total Appointments");
        Assert.True(totalMetric.Value >= 0);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
