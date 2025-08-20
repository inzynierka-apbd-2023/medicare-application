using Microsoft.EntityFrameworkCore;
using Xunit;
using AppointmentService.Data;
using AppointmentService.Features.DoctorDashboard.Services;
using AppointmentService.Models;

namespace AppointmentService.Tests.Features.DoctorDashboard.Services;

public class DoctorDashboardServiceTests : IDisposable
{
    private readonly AppointmentDbContext _context;
    private readonly DoctorDashboardService _service;

    public DoctorDashboardServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppointmentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppointmentDbContext(options);
        _service = new DoctorDashboardService(_context);

        SeedTestData();
    }

    private void SeedTestData()
    {
        var doctorId = Guid.NewGuid();
        var today = DateTime.UtcNow.Date;

        var appointments = new List<Appointment>
        {
            new()
            {
                Id = Guid.NewGuid().ToString(),
                DoctorId = doctorId.ToString(),
                PatientId = Guid.NewGuid().ToString(),
                ScheduledAt = today.AddHours(10),
                ScheduledEndAt = today.AddHours(11),
                Status = "Scheduled",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid().ToString(),
                DoctorId = doctorId.ToString(),
                PatientId = Guid.NewGuid().ToString(),
                ScheduledAt = today.AddHours(14),
                ScheduledEndAt = today.AddHours(15),
                Status = "Completed",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid().ToString(),
                DoctorId = doctorId.ToString(),
                PatientId = Guid.NewGuid().ToString(),
                ScheduledAt = today.AddDays(-5),
                ScheduledEndAt = today.AddDays(-5).AddHours(1),
                Status = "Completed",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _context.Appointments.AddRange(appointments);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetQuickStatsAsync_ShouldReturnCorrectStats()
    {
        var doctorId = Guid.Parse(_context.Appointments.First().DoctorId);

        var result = await _service.GetQuickStatsAsync(doctorId);

        Assert.NotNull(result);
        Assert.Equal(4, result.Stats.Count);
        
        var patientsToday = result.Stats.First(s => s.Label == "Patients Today");
        Assert.Equal(2, patientsToday.Value);

        var totalPatients = result.Stats.First(s => s.Label == "Total Patients");
        Assert.Equal(3, totalPatients.Value);

        var visitsThisMonth = result.Stats.First(s => s.Label == "Visits this Month");
        Assert.Equal(2, visitsThisMonth.Value);

        var unreadMessages = result.Stats.First(s => s.Label == "Unread Messages");
        Assert.Equal(0, unreadMessages.Value);
    }

    [Fact]
    public async Task GetQuickStatsAsync_WithNonExistentDoctor_ShouldReturnZeroStats()
    {
        var nonExistentDoctorId = Guid.NewGuid();

        var result = await _service.GetQuickStatsAsync(nonExistentDoctorId);

        Assert.NotNull(result);
        Assert.Equal(4, result.Stats.Count);
        Assert.All(result.Stats, stat => Assert.Equal(0, stat.Value));
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
