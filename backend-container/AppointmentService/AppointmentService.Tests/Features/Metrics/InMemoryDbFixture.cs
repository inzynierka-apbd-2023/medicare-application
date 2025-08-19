using AppointmentService.Data;
using Microsoft.EntityFrameworkCore;

namespace AppointmentService.Tests.Features.Metrics;

public class MetricsInMemoryDbFixture : IDisposable
{
    public AppointmentDbContext Context { get; }
    public MetricsInMemoryDbFixture()
    {
        var options = new DbContextOptionsBuilder<AppointmentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        Context = new AppointmentDbContext(options);
        Seed();
    }

    private void Seed()
    {
        var end = DateTime.UtcNow.Date;
        var start = end.AddDays(-15);
        var statuses = new [] {
            (Id: Guid.NewGuid().ToString(), Name: "scheduled"),
            (Id: Guid.NewGuid().ToString(), Name: "completed"),
            (Id: Guid.NewGuid().ToString(), Name: "cancelled"),
            (Id: Guid.NewGuid().ToString(), Name: "no-show")
        };
        foreach (var s in statuses)
        {
            Context.ScheduleAppointmentStatuses.Add(new AppointmentService.Models.ScheduleAppointmentStatus { Id = s.Id, Name = s.Name });
        }

        var doctorIds = Enumerable.Range(0,5).Select(_ => Guid.NewGuid().ToString()).ToArray();
        var patientIds = Enumerable.Range(0,8).Select(_ => Guid.NewGuid().ToString()).ToArray();

        var completedId = statuses.First(x => x.Name == "completed").Id;
        var cancelledId = statuses.First(x => x.Name == "cancelled").Id;
        var noShowId   = statuses.First(x => x.Name == "no-show").Id;

        for (int i=0;i<20;i++)
        {
            var day = start.AddDays(i % 10);
            var statusId = (i % 4) switch { 0 => completedId, 1 => cancelledId, 2 => noShowId, _ => completedId };
            Context.ScheduleAppointments.Add(new AppointmentService.Models.ScheduleAppointment {
                Id = Guid.NewGuid().ToString(),
                Day = day,
                Duration_Minutes = 30 + (i % 3) * 15,
                Doctor_User_Id = doctorIds[i % doctorIds.Length],
                Patient_User_Id = patientIds[i % patientIds.Length],
                Schedule_Appointment_Status_Id = statusId,
                Schedule_Id = Guid.NewGuid().ToString(),
                Appointment_Type = "in-person",
                Created_At = day,
                Updated_At = day
            });
        }
        Context.SaveChanges();
    }

    public void Dispose()
    {
        Context.Database.EnsureDeleted();
        Context.Dispose();
    }
}
