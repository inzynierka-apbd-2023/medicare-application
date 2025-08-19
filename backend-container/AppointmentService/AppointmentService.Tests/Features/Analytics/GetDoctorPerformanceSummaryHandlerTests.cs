using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AppointmentService.Data;
using AppointmentService.Features.Analytics.Handlers;
using AppointmentService.Features.Analytics.Queries;
using AppointmentService.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AppointmentService.Tests.Features.Analytics;

public class GetDoctorPerformanceSummaryHandlerTests
{
    private AppointmentDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppointmentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppointmentDbContext(options);
    }

    private (string DoctorId, string PatientId, string StatusId, string ScheduleId, string AppointmentId) Seed(AppointmentDbContext ctx, int ratings = 1, int appointments = 1)
    {
        var doctorId = Guid.NewGuid().ToString();
        var patientId = Guid.NewGuid().ToString();
        var statusId = Guid.NewGuid().ToString();
        var scheduleId = Guid.NewGuid().ToString();

        ctx.Users.Add(new User { Id = doctorId, Role_Id = "doctor-role", Schedule_Id = scheduleId, Created_At = DateTime.UtcNow, Updated_At = DateTime.UtcNow, Is_Active = true });
        ctx.Users.Add(new User { Id = patientId, Role_Id = "patient-role", Schedule_Id = scheduleId, Created_At = DateTime.UtcNow, Updated_At = DateTime.UtcNow, Is_Active = true });
        ctx.UserProfiles.Add(new UserProfile { User_Id = doctorId, FirstName = "Alice", LastName = "Heart", Email = "alice@test.com", Created_At = DateTime.UtcNow, Updated_At = DateTime.UtcNow });
        ctx.Doctors.Add(new Doctor { Id = doctorId, License_Number = "LIC1", Years_Experience = 5 });
        ctx.Patients.Add(new Patient { Id = patientId, General_Doctor_Id = doctorId, Medical_Record_Number = "MR1" });
        ctx.ScheduleAppointmentStatuses.Add(new ScheduleAppointmentStatus { Id = statusId, Name = "completed", Description = "Completed" });

        string lastApptId = string.Empty;
        for (int i = 0; i < appointments; i++)
        {
            var apptId = Guid.NewGuid().ToString();
            lastApptId = apptId;
            ctx.ScheduleAppointments.Add(new ScheduleAppointment
            {
                Id = apptId,
                Schedule_Id = scheduleId,
                Day = DateTime.UtcNow.AddDays(-1),
                Duration_Minutes = 30,
                Doctor_User_Id = doctorId,
                Patient_User_Id = patientId,
                Schedule_Appointment_Status_Id = statusId,
                Total_Cost = 100,
                Created_At = DateTime.UtcNow,
                Updated_At = DateTime.UtcNow
            });
        }

        for (int i = 0; i < ratings; i++)
        {
            ctx.Rates.Add(new Rate
            {
                Id = Guid.NewGuid().ToString(),
                Rate_Value = 5,
                Doctor_User_Id = doctorId,
                Patient_User_Id = patientId,
                Appointment_Id = lastApptId,
                Rated_At = DateTime.UtcNow
            });
        }

        ctx.SaveChanges();
        return (doctorId, patientId, statusId, scheduleId, lastApptId);
    }

    [Fact]
    public async Task Handle_NoData_ReturnsZeros()
    {
        using var ctx = CreateContext();
        var handler = new GetDoctorPerformanceSummaryHandler(ctx);
        var result = await handler.Handle(new GetDoctorPerformanceSummaryQuery(), CancellationToken.None);
        Assert.NotNull(result);
        Assert.Equal(0, result.TotalDoctors);
        Assert.Equal(0, result.AverageAppointmentsPerDoctor);
        Assert.Equal("N/A", result.TopRatedDoctor);
        Assert.Equal(0, result.DoctorAverageRating);
    }

    [Fact]
    public async Task Handle_WithData_ComputesMetrics()
    {
        using var ctx = CreateContext();
        Seed(ctx, ratings: 3, appointments: 4);
        var handler = new GetDoctorPerformanceSummaryHandler(ctx);
        var result = await handler.Handle(new GetDoctorPerformanceSummaryQuery { StartDate = DateTime.UtcNow.AddDays(-7), EndDate = DateTime.UtcNow }, CancellationToken.None);
        Assert.Equal(1, result.TotalDoctors);
        Assert.Equal(4, result.AverageAppointmentsPerDoctor);
        Assert.Equal("Alice Heart", result.TopRatedDoctor);
        Assert.Equal(5.0m, result.DoctorAverageRating);
    }

    [Fact]
    public async Task Handle_MultipleDoctors_AveragesAcrossAll()
    {
        using var ctx = CreateContext();
        Seed(ctx, ratings: 2, appointments: 2);
        var d2 = Guid.NewGuid().ToString();
        var p2 = Guid.NewGuid().ToString();
        var sched2 = Guid.NewGuid().ToString();
        var status2 = Guid.NewGuid().ToString();
        ctx.Users.AddRange(
            new User { Id = d2, Role_Id = "doctor-role", Schedule_Id = sched2, Created_At = DateTime.UtcNow, Updated_At = DateTime.UtcNow, Is_Active = true },
            new User { Id = p2, Role_Id = "patient-role", Schedule_Id = sched2, Created_At = DateTime.UtcNow, Updated_At = DateTime.UtcNow, Is_Active = true }
        );
        ctx.UserProfiles.Add(new UserProfile { User_Id = d2, FirstName = "Bob", LastName = "Vessel", Email = "bob@test.com", Created_At = DateTime.UtcNow, Updated_At = DateTime.UtcNow });
        ctx.Doctors.Add(new Doctor { Id = d2, License_Number = "LIC2", Years_Experience = 8 });
        ctx.Patients.Add(new Patient { Id = p2, General_Doctor_Id = d2, Medical_Record_Number = "MR2" });
        ctx.ScheduleAppointmentStatuses.Add(new ScheduleAppointmentStatus { Id = status2, Name = "completed", Description = "Completed" });
        for (int i = 0; i < 6; i++)
        {
            ctx.ScheduleAppointments.Add(new ScheduleAppointment
            {
                Id = Guid.NewGuid().ToString(),
                Schedule_Id = sched2,
                Day = DateTime.UtcNow.AddDays(-1),
                Duration_Minutes = 30,
                Doctor_User_Id = d2,
                Patient_User_Id = p2,
                Schedule_Appointment_Status_Id = status2,
                Total_Cost = 50,
                Created_At = DateTime.UtcNow,
                Updated_At = DateTime.UtcNow
            });
            ctx.Rates.Add(new Rate
            {
                Id = Guid.NewGuid().ToString(),
                Rate_Value = (byte)(i % 2 == 0 ? 4 : 5),
                Doctor_User_Id = d2,
                Patient_User_Id = p2,
                Appointment_Id = null,
                Rated_At = DateTime.UtcNow
            });
        }
        ctx.SaveChanges();

        var handler = new GetDoctorPerformanceSummaryHandler(ctx);
        var result = await handler.Handle(new GetDoctorPerformanceSummaryQuery(), CancellationToken.None);
        Assert.Equal(2, result.TotalDoctors);
        Assert.Equal(4m, result.AverageAppointmentsPerDoctor);
        Assert.True(result.TopRatedDoctor == "Alice Heart" || result.TopRatedDoctor == "Bob Vessel");
        Assert.InRange(result.DoctorAverageRating, 4.0m, 5.0m);
    }
}
