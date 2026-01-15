using Microsoft.EntityFrameworkCore;
using AppointmentService.Models;

namespace AppointmentService.Data;

/// <summary>
/// Shared deterministic IDs for cross-service mock data references.
/// Only 2 doctors and 2 patients seeded for simplified testing.
/// Patient/Doctor IDs match User IDs from UserService.
/// </summary>
public static class MockIds
{
    // ========== PATIENTS (matching UserService) ==========
    public static readonly Guid Patient1 = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000001");
    public static readonly Guid Patient2 = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000002");

    // ========== DOCTORS (matching UserService - UserId = DoctorId) ==========
    public static readonly Guid Doctor1 = Guid.Parse("bbbbbbbb-0002-0002-0002-000000000001");
    public static readonly Guid Doctor2 = Guid.Parse("bbbbbbbb-0002-0002-0002-000000000002");

    // ========== APPOINTMENT IDs ==========
    public static readonly Guid Appointment1 = Guid.Parse("55555555-5555-5555-5555-000000000001");
    public static readonly Guid Appointment2 = Guid.Parse("55555555-5555-5555-5555-000000000002");

    // ========== CATEGORY IDs ==========
    public static readonly Guid CategoryCheckup = Guid.Parse("66666666-6666-6666-6666-000000000001");
    public static readonly Guid CategoryConsultation = Guid.Parse("66666666-6666-6666-6666-000000000002");

    public static readonly Guid[] AllPatientIds = { Patient1, Patient2 };
    public static readonly Guid[] AllDoctorIds = { Doctor1, Doctor2 };
}

public static class MockDataSeeder
{
    public static async Task SeedAsync(AppointmentDbContext db)
    {
        int created = 0;
        Console.WriteLine("[MockDataSeeder] Starting AppointmentService seeding...");

        // ========================================
        // SEED APPOINTMENT CATEGORIES
        // ========================================
        var categories = new[]
        {
            (MockIds.CategoryCheckup, "Annual Checkup", "Routine yearly health examination", 30, true),
            (MockIds.CategoryConsultation, "General Consultation", "Initial consultation for new concerns", 45, true),
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
                Console.WriteLine($"[MockDataSeeder] Created category: {name}");
            }
        }

        // ========================================
        // SEED SCHEDULES (doctor weekly schedules)
        // These match PractitionerService schedules
        // ========================================
        var scheduleEntries = new (Guid doctorId, DayOfWeek day, TimeOnly start, TimeOnly end)[]
        {
            // Doctor1 - John Carter (Cardiologist)
            (MockIds.Doctor1, DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(13, 0)),
            (MockIds.Doctor1, DayOfWeek.Wednesday, new TimeOnly(14, 0), new TimeOnly(18, 0)),
            (MockIds.Doctor1, DayOfWeek.Friday, new TimeOnly(9, 0), new TimeOnly(12, 0)),
            
            // Doctor2 - Sarah Chen (General Practitioner)
            (MockIds.Doctor2, DayOfWeek.Monday, new TimeOnly(8, 0), new TimeOnly(16, 0)),
            (MockIds.Doctor2, DayOfWeek.Tuesday, new TimeOnly(8, 0), new TimeOnly(16, 0)),
            (MockIds.Doctor2, DayOfWeek.Thursday, new TimeOnly(8, 0), new TimeOnly(16, 0)),
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
                Console.WriteLine($"[MockDataSeeder] Created schedule for doctor {doctorId}: {day} {start:HH:mm}-{end:HH:mm}");
            }
        }

        // ========================================
        // SEED SAMPLE APPOINTMENTS
        // Patient1 with Doctor1, Patient2 with Doctor2
        // ========================================
        var appointmentData = new[]
        {
            (
                MockIds.Appointment1, 
                MockIds.Patient1,  // Alice Johnson
                MockIds.Doctor1,   // Dr. John Carter
                DateTime.UtcNow.AddDays(1).Date.AddHours(10),  // Tomorrow at 10:00
                30, 
                "Scheduled", 
                "in-person", 
                "Routine annual health check"
            ),
            (
                MockIds.Appointment2, 
                MockIds.Patient2,  // Bob Smith
                MockIds.Doctor2,   // Dr. Sarah Chen
                DateTime.UtcNow.AddDays(2).Date.AddHours(14),  // Day after tomorrow at 14:00
                45, 
                "Scheduled", 
                "in-person", 
                "New patient consultation for back pain"
            ),
            // TODAY's appointments for Doctor1
            (
                Guid.Parse("55555555-5555-5555-5555-000000000003"), 
                MockIds.Patient1,  // Alice Johnson
                MockIds.Doctor1,   // Dr. John Carter
                DateTime.UtcNow.Date.AddHours(9),  // Today at 9:00 AM
                30, 
                "Scheduled", 
                "in-person", 
                "Follow-up appointment for blood pressure check"
            ),
            (
                Guid.Parse("55555555-5555-5555-5555-000000000004"), 
                MockIds.Patient2,  // Bob Smith
                MockIds.Doctor1,   // Dr. John Carter
                DateTime.UtcNow.Date.AddHours(10).AddMinutes(30),  // Today at 10:30 AM
                30, 
                "Scheduled", 
                "video", 
                "Video consultation for test results"
            ),
            (
                Guid.Parse("55555555-5555-5555-5555-000000000005"), 
                MockIds.Patient1,  // Alice Johnson
                MockIds.Doctor1,   // Dr. John Carter
                DateTime.UtcNow.Date.AddHours(14),  // Today at 2:00 PM
                45, 
                "Scheduled", 
                "in-person", 
                "Annual wellness exam"
            ),
            // TODAY's appointments for Doctor2
            (
                Guid.Parse("55555555-5555-5555-5555-000000000006"), 
                MockIds.Patient2,  // Bob Smith
                MockIds.Doctor2,   // Dr. Sarah Chen
                DateTime.UtcNow.Date.AddHours(11),  // Today at 11:00 AM
                30, 
                "Scheduled", 
                "in-person", 
                "New patient intake"
            ),
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
                Console.WriteLine($"[MockDataSeeder] Created appointment: Patient={patientId}, Doctor={doctorId}, At={scheduledAt}");
            }
        }

        // ========================================
        // SEED APPOINTMENT SLOTS (for next 7 days)
        // ========================================
        var baseDate = DateTime.UtcNow.Date.AddDays(1);
        var existingSlotKeys = await db.AppointmentSlots
            .Select(s => new { s.DoctorId, s.StartTime })
            .ToListAsync();
        var existingSlotSet = existingSlotKeys.Select(x => (x.DoctorId, x.StartTime)).ToHashSet();

        for (int day = 0; day < 7; day++)
        {
            var slotDate = baseDate.AddDays(day);
            
            // Create slots for Doctor1: 9 AM to 5 PM, 30-min slots
            for (int hour = 9; hour < 17; hour++)
            {
                var startTime = slotDate.AddHours(hour);
                if (!existingSlotSet.Contains((MockIds.Doctor1, startTime)))
                {
                    db.AppointmentSlots.Add(new AppointmentSlot
                    {
                        Id = Guid.NewGuid(),
                        DoctorId = MockIds.Doctor1,
                        StartTime = startTime,
                        EndTime = startTime.AddMinutes(30),
                        IsAvailable = true,
                        CreatedAt = DateTime.UtcNow
                    });
                    created++;
                }
            }
            
            // Create slots for Doctor2: 8 AM to 4 PM, 30-min slots
            for (int hour = 8; hour < 16; hour++)
            {
                var startTime = slotDate.AddHours(hour);
                if (!existingSlotSet.Contains((MockIds.Doctor2, startTime)))
                {
                    db.AppointmentSlots.Add(new AppointmentSlot
                    {
                        Id = Guid.NewGuid(),
                        DoctorId = MockIds.Doctor2,
                        StartTime = startTime,
                        EndTime = startTime.AddMinutes(30),
                        IsAvailable = true,
                        CreatedAt = DateTime.UtcNow
                    });
                    created++;
                }
            }
        }


        // ========================================
        // SEED APPOINTMENT PAYMENTS (matching BillingService)
        // Appointments 1, 3, 5 are Paid (300 PLN)
        // ========================================
        var paymentData = new[]
        {
            (MockIds.Appointment1, 30000L, MockIds.Patient1),
            (Guid.Parse("55555555-5555-5555-5555-000000000003"), 30000L, MockIds.Patient1),
            (Guid.Parse("55555555-5555-5555-5555-000000000005"), 30000L, MockIds.Patient1),
        };

        var existingPaymentIds = await db.AppointmentPayments.Select(p => p.AppointmentId).ToHashSetAsync();
        foreach (var (appointmentId, amount, patientId) in paymentData)
        {
            if (!existingPaymentIds.Contains(appointmentId))
            {
                 // Reconstruct IntentId based on billing logic (9999...01 + index)
                 // Appt1 (i=0) -> Intent1 (...01)
                 // Appt3 (i=2) -> Intent3 (...03)
                 // Appt5 (i=4) -> Intent5 (...05)
                 
                 Guid intentId = Guid.Empty;
                 if (appointmentId == MockIds.Appointment1) intentId = Guid.Parse("99999999-9999-9999-9999-000000000001");
                 else if (appointmentId == Guid.Parse("55555555-5555-5555-5555-000000000003")) intentId = Guid.Parse("99999999-9999-9999-9999-000000000003");
                 else if (appointmentId == Guid.Parse("55555555-5555-5555-5555-000000000005")) intentId = Guid.Parse("99999999-9999-9999-9999-000000000005");

                 db.AppointmentPayments.Add(new AppointmentPayment
                 {
                     Id = Guid.NewGuid(),
                     AppointmentId = appointmentId,
                     PatientId = patientId, 
                     AmountCents = amount,
                     Currency = "PLN",
                     PaymentIntentId = intentId,
                     CreatedAt = DateTime.UtcNow.AddDays(-1),
                     ForDate = DateTime.UtcNow
                 });
                 created++;
                 Console.WriteLine($"[MockDataSeeder] Created payment for appointment {appointmentId} ({amount/100m} PLN)");
            }
        }

        if (created > 0)
        {
            await db.SaveChangesAsync();
            Console.WriteLine($"[MockDataSeeder] ===== TOTAL: Created {created} records (including payments) =====");
            
            // Summary
            Console.WriteLine("[MockDataSeeder] Appointment Summary:");
            Console.WriteLine($"  - Appointment 1: Patient1 (Alice) with Doctor1 (John Carter) - Tomorrow 10:00");
            Console.WriteLine($"  - Appointment 2: Patient2 (Bob) with Doctor2 (Sarah Chen) - Day after tomorrow 14:00");
            Console.WriteLine("[MockDataSeeder] Doctor IDs (for reference):");
            Console.WriteLine($"  - Doctor1 (John Carter): {MockIds.Doctor1}");
            Console.WriteLine($"  - Doctor2 (Sarah Chen): {MockIds.Doctor2}");
        }
        else
        {
            Console.WriteLine("[MockDataSeeder] All appointment mock data already exists.");
        }
    }
}
