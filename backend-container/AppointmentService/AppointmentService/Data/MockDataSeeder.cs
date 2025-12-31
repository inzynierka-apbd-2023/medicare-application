using Microsoft.EntityFrameworkCore;
using AppointmentService.Models;

namespace AppointmentService.Data;

/// <summary>
/// Shared deterministic IDs for cross-service mock data references
/// </summary>
public static class MockIds
{
    // Patient IDs (from PatientService)
    public static readonly Guid Patient1 = Guid.Parse("11111111-1111-1111-1111-000000000001");
    public static readonly Guid Patient2 = Guid.Parse("11111111-1111-1111-1111-000000000002");
    public static readonly Guid Patient3 = Guid.Parse("11111111-1111-1111-1111-000000000003");
    public static readonly Guid Patient4 = Guid.Parse("11111111-1111-1111-1111-000000000004");
    public static readonly Guid Patient5 = Guid.Parse("11111111-1111-1111-1111-000000000005");
    public static readonly Guid Patient6 = Guid.Parse("11111111-1111-1111-1111-000000000006");
    public static readonly Guid Patient7 = Guid.Parse("11111111-1111-1111-1111-000000000007");

    // Doctor IDs (from PractitionerService)
    public static readonly Guid Doctor1 = Guid.Parse("22222222-2222-2222-2222-000000000001");
    public static readonly Guid Doctor2 = Guid.Parse("22222222-2222-2222-2222-000000000002");
    public static readonly Guid Doctor3 = Guid.Parse("22222222-2222-2222-2222-000000000003");
    public static readonly Guid Doctor4 = Guid.Parse("22222222-2222-2222-2222-000000000004");
    public static readonly Guid Doctor5 = Guid.Parse("22222222-2222-2222-2222-000000000005");
    public static readonly Guid Doctor6 = Guid.Parse("22222222-2222-2222-2222-000000000006");
    public static readonly Guid Doctor7 = Guid.Parse("22222222-2222-2222-2222-000000000007");

    // Appointment IDs
    public static readonly Guid Appointment1 = Guid.Parse("55555555-5555-5555-5555-000000000001");
    public static readonly Guid Appointment2 = Guid.Parse("55555555-5555-5555-5555-000000000002");
    public static readonly Guid Appointment3 = Guid.Parse("55555555-5555-5555-5555-000000000003");
    public static readonly Guid Appointment4 = Guid.Parse("55555555-5555-5555-5555-000000000004");
    public static readonly Guid Appointment5 = Guid.Parse("55555555-5555-5555-5555-000000000005");
    public static readonly Guid Appointment6 = Guid.Parse("55555555-5555-5555-5555-000000000006");
    public static readonly Guid Appointment7 = Guid.Parse("55555555-5555-5555-5555-000000000007");

    // Category IDs
    public static readonly Guid CategoryCheckup = Guid.Parse("66666666-6666-6666-6666-000000000001");
    public static readonly Guid CategoryFollowUp = Guid.Parse("66666666-6666-6666-6666-000000000002");
    public static readonly Guid CategoryConsultation = Guid.Parse("66666666-6666-6666-6666-000000000003");
    public static readonly Guid CategoryEmergency = Guid.Parse("66666666-6666-6666-6666-000000000004");
    public static readonly Guid CategoryProcedure = Guid.Parse("66666666-6666-6666-6666-000000000005");
    public static readonly Guid CategoryLabReview = Guid.Parse("66666666-6666-6666-6666-000000000006");
    public static readonly Guid CategoryVaccination = Guid.Parse("66666666-6666-6666-6666-000000000007");

    public static readonly Guid[] AllPatientIds = { Patient1, Patient2, Patient3, Patient4, Patient5, Patient6, Patient7 };
    public static readonly Guid[] AllDoctorIds = { Doctor1, Doctor2, Doctor3, Doctor4, Doctor5, Doctor6, Doctor7 };
    public static readonly Guid[] AllAppointmentIds = { Appointment1, Appointment2, Appointment3, Appointment4, Appointment5, Appointment6, Appointment7 };
}

public static class MockDataSeeder
{
    public static async Task SeedAsync(AppointmentDbContext db)
    {
        int created = 0;

        // Seed Appointment Categories
        var categories = new[]
        {
            (MockIds.CategoryCheckup, "Annual Checkup", "Routine yearly health examination", 30, true),
            (MockIds.CategoryFollowUp, "Follow-Up Visit", "Post-treatment follow-up appointment", 20, true),
            (MockIds.CategoryConsultation, "General Consultation", "Initial consultation for new concerns", 45, true),
            (MockIds.CategoryEmergency, "Emergency Visit", "Urgent same-day appointment", 60, true),
            (MockIds.CategoryProcedure, "Medical Procedure", "Minor in-office procedures", 90, true),
            (MockIds.CategoryLabReview, "Lab Results Review", "Review of laboratory test results", 15, true),
            (MockIds.CategoryVaccination, "Vaccination", "Immunization appointment", 15, true)
        };

        var existingCategoryIds = await db.AppointmentCategories.Select(c => c.Id).ToHashSetAsync();
        foreach (var (id, name, description, duration, isActive) in categories)
        {
            if (!existingCategoryIds.Contains(id))
            {
                db.AppointmentCategories.Add(new AppointmentCategory
                {
                    Id = id,
                    Name = name,
                    Description = description,
                    DurationMinutes = duration,
                    IsActive = isActive,
                    CreatedAt = DateTime.UtcNow.AddDays(-90)
                });
                created++;
            }
        }

        // Seed Schedules (doctor weekly schedules)
        var scheduleEntries = new (Guid doctorId, DayOfWeek day, TimeOnly start, TimeOnly end)[]
        {
            (MockIds.Doctor1, DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(17, 0)),
            (MockIds.Doctor1, DayOfWeek.Wednesday, new TimeOnly(9, 0), new TimeOnly(17, 0)),
            (MockIds.Doctor1, DayOfWeek.Friday, new TimeOnly(9, 0), new TimeOnly(13, 0)),
            (MockIds.Doctor2, DayOfWeek.Monday, new TimeOnly(8, 0), new TimeOnly(16, 0)),
            (MockIds.Doctor2, DayOfWeek.Tuesday, new TimeOnly(8, 0), new TimeOnly(16, 0)),
            (MockIds.Doctor2, DayOfWeek.Thursday, new TimeOnly(8, 0), new TimeOnly(16, 0)),
            (MockIds.Doctor3, DayOfWeek.Tuesday, new TimeOnly(10, 0), new TimeOnly(18, 0)),
            (MockIds.Doctor3, DayOfWeek.Thursday, new TimeOnly(10, 0), new TimeOnly(18, 0)),
            (MockIds.Doctor4, DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(17, 0)),
            (MockIds.Doctor4, DayOfWeek.Wednesday, new TimeOnly(9, 0), new TimeOnly(17, 0)),
            (MockIds.Doctor5, DayOfWeek.Tuesday, new TimeOnly(7, 0), new TimeOnly(15, 0)),
            (MockIds.Doctor5, DayOfWeek.Friday, new TimeOnly(7, 0), new TimeOnly(15, 0)),
            (MockIds.Doctor6, DayOfWeek.Monday, new TimeOnly(11, 0), new TimeOnly(19, 0)),
            (MockIds.Doctor6, DayOfWeek.Thursday, new TimeOnly(11, 0), new TimeOnly(19, 0)),
            (MockIds.Doctor7, DayOfWeek.Wednesday, new TimeOnly(9, 0), new TimeOnly(17, 0)),
            (MockIds.Doctor7, DayOfWeek.Friday, new TimeOnly(9, 0), new TimeOnly(14, 0))
        };

        var existingScheduleKeys = await db.Schedules
            .Select(s => new { s.DoctorId, s.DayOfWeek })
            .ToListAsync();
        var existingScheduleSet = existingScheduleKeys.Select(x => (x.DoctorId, x.DayOfWeek)).ToHashSet();

        foreach (var (doctorId, day, start, end) in scheduleEntries)
        {
            if (!existingScheduleSet.Contains((doctorId, day)))
            {
                db.Schedules.Add(new Schedule
                {
                    Id = Guid.NewGuid(),
                    DoctorId = doctorId,
                    DayOfWeek = day,
                    StartTime = start,
                    EndTime = end,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-60),
                    UpdatedAt = DateTime.UtcNow
                });
                created++;
            }
        }

        // Seed Appointments (7 appointments with various statuses)
        var appointmentData = new[]
        {
            (MockIds.Appointment1, MockIds.Patient1, MockIds.Doctor1, DateTime.UtcNow.AddDays(1).Date.AddHours(10), 30, "Scheduled", "Annual Checkup", "Routine annual health check"),
            (MockIds.Appointment2, MockIds.Patient2, MockIds.Doctor2, DateTime.UtcNow.AddDays(2).Date.AddHours(14), 45, "Confirmed", "General Consultation", "New patient consultation for back pain"),
            (MockIds.Appointment3, MockIds.Patient3, MockIds.Doctor3, DateTime.UtcNow.AddDays(-1).Date.AddHours(11), 30, "Completed", "Follow-Up Visit", "Post-procedure follow-up"),
            (MockIds.Appointment4, MockIds.Patient4, MockIds.Doctor4, DateTime.UtcNow.AddDays(3).Date.AddHours(9), 20, "Scheduled", "Vaccination", "Flu vaccination"),
            (MockIds.Appointment5, MockIds.Patient5, MockIds.Doctor5, DateTime.UtcNow.AddDays(-3).Date.AddHours(8), 60, "Completed", "Medical Procedure", "Minor surgical procedure completed"),
            (MockIds.Appointment6, MockIds.Patient6, MockIds.Doctor6, DateTime.UtcNow.AddDays(5).Date.AddHours(15), 15, "Scheduled", "Lab Results Review", "Review of blood work results"),
            (MockIds.Appointment7, MockIds.Patient7, MockIds.Doctor7, DateTime.UtcNow.AddDays(-2).Date.AddHours(10), 45, "Cancelled", "General Consultation", "Patient cancelled due to illness")
        };

        var existingAppointmentIds = await db.Appointments.Select(a => a.Id).ToHashSetAsync();
        foreach (var (id, patientId, doctorId, scheduledAt, duration, status, type, complaint) in appointmentData)
        {
            if (!existingAppointmentIds.Contains(id))
            {
                db.Appointments.Add(new Appointment
                {
                    Id = id,
                    PatientId = patientId,
                    DoctorId = doctorId,
                    ScheduledAt = scheduledAt,
                    ScheduledEndAt = scheduledAt.AddMinutes(duration),
                    Status = status,
                    AppointmentType = type,
                    ChiefComplaint = complaint,
                    Notes = $"Mock appointment for testing - {type}",
                    CreatedAt = DateTime.UtcNow.AddDays(-7),
                    UpdatedAt = DateTime.UtcNow
                });
                created++;
            }
        }

        // Seed Appointment Slots (available slots for next 7 days)
        var baseDate = DateTime.UtcNow.Date.AddDays(1);
        var existingSlotKeys = await db.AppointmentSlots
            .Select(s => new { s.DoctorId, s.StartTime })
            .ToListAsync();
        var existingSlotSet = existingSlotKeys.Select(x => (x.DoctorId, x.StartTime)).ToHashSet();

        for (int day = 0; day < 7; day++)
        {
            var slotDate = baseDate.AddDays(day);
            
            // Create slots for Doctor1 and Doctor2 (9 AM to 5 PM, 30-min slots)
            foreach (var doctorId in new[] { MockIds.Doctor1, MockIds.Doctor2 })
            {
                for (int hour = 9; hour < 17; hour++)
                {
                    var startTime = slotDate.AddHours(hour);
                    if (!existingSlotSet.Contains((doctorId, startTime)))
                    {
                        db.AppointmentSlots.Add(new AppointmentSlot
                        {
                            Id = Guid.NewGuid(),
                            DoctorId = doctorId,
                            StartTime = startTime,
                            EndTime = startTime.AddMinutes(30),
                            IsAvailable = true,
                            CreatedAt = DateTime.UtcNow
                        });
                        created++;
                    }
                }
            }
        }

        if (created > 0)
        {
            await db.SaveChangesAsync();
            Console.WriteLine($"[MockDataSeeder] Created {created} appointment records (categories, schedules, appointments, slots).");
        }
        else
        {
            Console.WriteLine("[MockDataSeeder] All appointment mock data already exists.");
        }
    }
}
