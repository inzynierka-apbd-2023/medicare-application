using Microsoft.EntityFrameworkCore;
using PractitionerService.Models;

namespace PractitionerService.Data;

/// <summary>
/// Shared deterministic IDs for cross-service mock data references.
/// Only 2 doctors are seeded for simplified testing.
/// Doctor.Id = Doctor.UserId for simplicity.
/// </summary>
public static class MockIds
{
    // ========== DOCTORS (only 2 fully seeded) ==========
    // These IDs match UserService.MockIds.DoctorUser1/2
    // Doctor.Id = Doctor.UserId = User.Id (same GUID everywhere)
    public static readonly Guid DoctorUser1 = Guid.Parse("bbbbbbbb-0002-0002-0002-000000000001");
    public static readonly Guid DoctorUser2 = Guid.Parse("bbbbbbbb-0002-0002-0002-000000000002");
    
    // Doctor entity uses same ID as User ID
    public static readonly Guid Doctor1 = DoctorUser1;
    public static readonly Guid Doctor2 = DoctorUser2;

    // ========== RECEPTIONIST ==========
    public static readonly Guid ReceptionistUser1 = Guid.Parse("cccccccc-0003-0003-0003-000000000001");

    // ========== SPECIALIZATIONS ==========
    public static readonly Guid SpecCardiologist = Guid.Parse("33333333-3333-3333-3333-000000000001");
    public static readonly Guid SpecGeneralPractitioner = Guid.Parse("33333333-3333-3333-3333-000000000002");

    // ========== SERVICES ==========
    public static readonly Guid ServiceConsultation = Guid.Parse("44444444-4444-4444-4444-000000000001");
    public static readonly Guid ServiceCardiology = Guid.Parse("44444444-4444-4444-4444-000000000002");

    public static readonly Guid[] AllDoctorUserIds = { DoctorUser1, DoctorUser2 };
    public static readonly Guid[] AllDoctorIds = { Doctor1, Doctor2 };
    public static readonly Guid[] AllSpecializationIds = { SpecCardiologist, SpecGeneralPractitioner };
    public static readonly Guid[] AllServiceIds = { ServiceConsultation, ServiceCardiology };
}

public static class MockDataSeeder
{
    public static async Task SeedAsync(PractitionerDbContext db)
    {
        int created = 0;
        Console.WriteLine("[MockDataSeeder] Starting PractitionerService seeding...");

        // ========================================
        // SEED SPECIALIZATIONS
        // ========================================
        var specializations = new[]
        {
            (MockIds.SpecCardiologist, "Cardiologist"),
            (MockIds.SpecGeneralPractitioner, "General Practitioner"),
        };

        var existingSpecIds = await db.Specializations.Select(s => s.Id).ToHashSetAsync();
        foreach (var (id, name) in specializations)
        {
            if (!existingSpecIds.Contains(id))
            {
                db.Specializations.Add(new Specialization { Id = id, Name = name });
                created++;
                Console.WriteLine($"[MockDataSeeder] Created specialization: {name} (ID: {id})");
            }
        }

        // ========================================
        // SEED MEDICAL SERVICES
        // ========================================
        var services = new[]
        {
            (MockIds.ServiceConsultation, "General Consultation", "Routine check-up and consultation", 30),
            (MockIds.ServiceCardiology, "Cardiology Services", "Heart and cardiovascular examinations", 45),
        };

        var existingServiceIds = await db.Services.Select(s => s.Id).ToHashSetAsync();
        foreach (var (id, name, description, _) in services)
        {
            if (!existingServiceIds.Contains(id))
            {
                db.Services.Add(new MedicalService { Id = id, Name = name, Description = description });
                created++;
                Console.WriteLine($"[MockDataSeeder] Created service: {name} (ID: {id})");
            }
        }

        // ========================================
        // SEED 2 DOCTORS - FULLY COMPLETE
        // ========================================
        var doctorData = new[]
        {
            (
                MockIds.Doctor1, 
                MockIds.DoctorUser1, 
                "Cardiology specialist with 15 years of experience in interventional procedures."
            ),
            (
                MockIds.Doctor2, 
                MockIds.DoctorUser2, 
                "General practitioner focused on preventive care and family medicine."
            ),
        };

        var existingDoctorIds = await db.Doctors.Select(d => d.Id).ToHashSetAsync();
        foreach (var (doctorId, userId, bio) in doctorData)
        {
            if (!existingDoctorIds.Contains(doctorId))
            {
                db.Doctors.Add(new Doctor
                {
                    Id = doctorId,
                    UserId = userId,
                    Bio = bio,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-60),
                    UpdatedAt = DateTime.UtcNow
                });
                created++;
                Console.WriteLine($"[MockDataSeeder] Created doctor entity: DoctorId={doctorId}, UserId={userId}");
            }
        }

        // ========================================
        // SEED 1 RECEPTIONIST
        // ========================================
        var existingReceptionistUserIds = await db.Receptionists.Select(r => r.UserId).ToHashSetAsync();
        if (!existingReceptionistUserIds.Contains(MockIds.ReceptionistUser1))
        {
            db.Receptionists.Add(new Receptionist
            {
                Id = Guid.NewGuid(),
                UserId = MockIds.ReceptionistUser1,
                CreatedAt = DateTime.UtcNow.AddDays(-45),
                UpdatedAt = DateTime.UtcNow
            });
            created++;
            Console.WriteLine($"[MockDataSeeder] Created receptionist: UserId={MockIds.ReceptionistUser1}");
        }

        // Save doctors first so FK constraints work
        if (created > 0)
        {
            await db.SaveChangesAsync();
            Console.WriteLine($"[MockDataSeeder] Saved base entities ({created} records)");
        }

        // ========================================
        // SEED DOCTOR-SPECIALIZATION MAPPINGS
        // Doctor1 (John Carter): Cardiologist + General Practitioner
        // Doctor2 (Sarah Chen): General Practitioner only
        // ========================================
        var doctorSpecMappings = new[]
        {
            (MockIds.Doctor1, MockIds.SpecCardiologist),
            (MockIds.Doctor1, MockIds.SpecGeneralPractitioner),
            (MockIds.Doctor2, MockIds.SpecGeneralPractitioner),
        };

        var existingDoctorSpecs = await db.DoctorSpecializations
            .Select(ds => new { ds.DoctorId, ds.SpecializationId })
            .ToListAsync();
        var existingDoctorSpecSet = existingDoctorSpecs.Select(x => (x.DoctorId, x.SpecializationId)).ToHashSet();

        foreach (var (doctorId, specId) in doctorSpecMappings)
        {
            if (!existingDoctorSpecSet.Contains((doctorId, specId)))
            {
                db.DoctorSpecializations.Add(new DoctorSpecialization
                {
                    DoctorId = doctorId,
                    SpecializationId = specId
                });
                created++;
                Console.WriteLine($"[MockDataSeeder] Mapped doctor {doctorId} to specialization {specId}");
            }
        }

        // ========================================
        // SEED SPECIALIZATION-SERVICE MAPPINGS
        // ========================================
        var specServiceMappings = new[]
        {
            (MockIds.SpecCardiologist, MockIds.ServiceCardiology),
            (MockIds.SpecCardiologist, MockIds.ServiceConsultation),
            (MockIds.SpecGeneralPractitioner, MockIds.ServiceConsultation),
        };

        var existingSpecServices = await db.SpecializationServices
            .Select(ss => new { ss.SpecializationId, ss.ServiceId })
            .ToListAsync();
        var existingSpecServiceSet = existingSpecServices.Select(x => (x.SpecializationId, x.ServiceId)).ToHashSet();

        foreach (var (specId, serviceId) in specServiceMappings)
        {
            if (!existingSpecServiceSet.Contains((specId, serviceId)))
            {
                db.SpecializationServices.Add(new SpecializationService
                {
                    SpecializationId = specId,
                    ServiceId = serviceId
                });
                created++;
                Console.WriteLine($"[MockDataSeeder] Mapped specialization {specId} to service {serviceId}");
            }
        }

        // ========================================
        // SEED DOCTOR SCHEDULES (AVAILABILITY)
        // Doctor1 (John Carter): Mon 9-13, Wed 14-18, Fri 9-12
        // Doctor2 (Sarah Chen): Mon 8-16, Tue 8-16, Thu 8-16
        // ========================================
        var scheduleEntries = new (Guid doctorId, int dayOfWeek, TimeSpan start, TimeSpan end)[]
        {
            // Doctor1 - John Carter (Cardiologist)
            (MockIds.Doctor1, 1, new TimeSpan(9, 0, 0), new TimeSpan(13, 0, 0)),   // Monday
            (MockIds.Doctor1, 3, new TimeSpan(14, 0, 0), new TimeSpan(18, 0, 0)),  // Wednesday
            (MockIds.Doctor1, 5, new TimeSpan(9, 0, 0), new TimeSpan(12, 0, 0)),   // Friday
            
            // Doctor2 - Sarah Chen (General Practitioner)
            (MockIds.Doctor2, 1, new TimeSpan(8, 0, 0), new TimeSpan(16, 0, 0)),   // Monday
            (MockIds.Doctor2, 2, new TimeSpan(8, 0, 0), new TimeSpan(16, 0, 0)),   // Tuesday
            (MockIds.Doctor2, 4, new TimeSpan(8, 0, 0), new TimeSpan(16, 0, 0)),   // Thursday
        };

        var existingScheduleKeys = await db.DoctorSchedules
            .Select(s => new { s.DoctorId, s.DayOfWeek })
            .ToListAsync();
        var existingScheduleSet = existingScheduleKeys.Select(x => (x.DoctorId, x.DayOfWeek)).ToHashSet();

        foreach (var (doctorId, dayOfWeek, start, end) in scheduleEntries)
        {
            if (!existingScheduleSet.Contains((doctorId, dayOfWeek)))
            {
                db.DoctorSchedules.Add(new DoctorSchedule
                {
                    Id = Guid.NewGuid(),
                    DoctorId = doctorId,
                    DayOfWeek = dayOfWeek,
                    StartTime = start,
                    EndTime = end,
                    CreatedAt = DateTime.UtcNow.AddDays(-30),
                    UpdatedAt = DateTime.UtcNow
                });
                created++;
                var dayName = dayOfWeek switch { 1 => "Monday", 2 => "Tuesday", 3 => "Wednesday", 4 => "Thursday", 5 => "Friday", _ => $"Day{dayOfWeek}" };
                Console.WriteLine($"[MockDataSeeder] Created schedule for doctor {doctorId}: {dayName} {start:hh\\:mm}-{end:hh\\:mm}");
            }
        }

        if (created > 0)
        {
            await db.SaveChangesAsync();
            Console.WriteLine($"[MockDataSeeder] ===== TOTAL: Created {created} practitioner records =====");
            

            Console.WriteLine("[MockDataSeeder] Doctor Summary:");
            Console.WriteLine($"  - Dr. John Carter (Cardiologist): DoctorId={MockIds.Doctor1}, UserId={MockIds.DoctorUser1}");
            Console.WriteLine($"    Specializations: Cardiologist, General Practitioner");
            Console.WriteLine($"    Schedule: Mon 9-13, Wed 14-18, Fri 9-12");
            Console.WriteLine($"  - Dr. Sarah Chen (GP): DoctorId={MockIds.Doctor2}, UserId={MockIds.DoctorUser2}");
            Console.WriteLine($"    Specializations: General Practitioner");
            Console.WriteLine($"    Schedule: Mon 8-16, Tue 8-16, Thu 8-16");
        }
        else
        {
            Console.WriteLine("[MockDataSeeder] All practitioner mock data already exists.");
        }
    }
}
