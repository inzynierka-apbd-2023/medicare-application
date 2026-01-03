using Microsoft.EntityFrameworkCore;
using PractitionerService.Models;

namespace PractitionerService.Data;

/// <summary>
/// Shared deterministic IDs for cross-service mock data references
/// Doctor IDs match User IDs from UserService for seamless auth integration
/// </summary>
public static class MockIds
{
    // Doctor User IDs (from UserService) - used as both User ID and Doctor entity ID
    public static readonly Guid DoctorUser1 = Guid.Parse("bbbbbbbb-0002-0002-0002-000000000001");
    public static readonly Guid DoctorUser2 = Guid.Parse("bbbbbbbb-0002-0002-0002-000000000002");
    public static readonly Guid DoctorUser3 = Guid.Parse("bbbbbbbb-0002-0002-0002-000000000003");
    public static readonly Guid DoctorUser4 = Guid.Parse("bbbbbbbb-0002-0002-0002-000000000004");
    public static readonly Guid DoctorUser5 = Guid.Parse("bbbbbbbb-0002-0002-0002-000000000005");
    public static readonly Guid DoctorUser6 = Guid.Parse("bbbbbbbb-0002-0002-0002-000000000006");
    public static readonly Guid DoctorUser7 = Guid.Parse("bbbbbbbb-0002-0002-0002-000000000007");

    // Receptionist Users (from UserService)
    public static readonly Guid ReceptionistUser1 = Guid.Parse("cccccccc-0003-0003-0003-000000000001");
    public static readonly Guid ReceptionistUser2 = Guid.Parse("cccccccc-0003-0003-0003-000000000002");
    public static readonly Guid ReceptionistUser3 = Guid.Parse("cccccccc-0003-0003-0003-000000000003");
    public static readonly Guid ReceptionistUser4 = Guid.Parse("cccccccc-0003-0003-0003-000000000004");
    public static readonly Guid ReceptionistUser5 = Guid.Parse("cccccccc-0003-0003-0003-000000000005");
    public static readonly Guid ReceptionistUser6 = Guid.Parse("cccccccc-0003-0003-0003-000000000006");
    public static readonly Guid ReceptionistUser7 = Guid.Parse("cccccccc-0003-0003-0003-000000000007");

    // Doctor entity IDs = User IDs (simplified: Doctor.Id == Doctor.UserId)
    public static readonly Guid Doctor1 = DoctorUser1;
    public static readonly Guid Doctor2 = DoctorUser2;
    public static readonly Guid Doctor3 = DoctorUser3;
    public static readonly Guid Doctor4 = DoctorUser4;
    public static readonly Guid Doctor5 = DoctorUser5;
    public static readonly Guid Doctor6 = DoctorUser6;
    public static readonly Guid Doctor7 = DoctorUser7;

    // Specialization IDs
    public static readonly Guid SpecCardiologist = Guid.Parse("33333333-3333-3333-3333-000000000001");
    public static readonly Guid SpecGeneralPractitioner = Guid.Parse("33333333-3333-3333-3333-000000000002");
    public static readonly Guid SpecDermatologist = Guid.Parse("33333333-3333-3333-3333-000000000003");
    public static readonly Guid SpecPediatrician = Guid.Parse("33333333-3333-3333-3333-000000000004");
    public static readonly Guid SpecOrthopedist = Guid.Parse("33333333-3333-3333-3333-000000000005");
    public static readonly Guid SpecNeurologist = Guid.Parse("33333333-3333-3333-3333-000000000006");
    public static readonly Guid SpecEndocrinologist = Guid.Parse("33333333-3333-3333-3333-000000000007");

    // Service IDs
    public static readonly Guid ServiceConsultation = Guid.Parse("44444444-4444-4444-4444-000000000001");
    public static readonly Guid ServiceCardiology = Guid.Parse("44444444-4444-4444-4444-000000000002");
    public static readonly Guid ServiceDermatology = Guid.Parse("44444444-4444-4444-4444-000000000003");
    public static readonly Guid ServicePediatric = Guid.Parse("44444444-4444-4444-4444-000000000004");
    public static readonly Guid ServiceOrthopedic = Guid.Parse("44444444-4444-4444-4444-000000000005");
    public static readonly Guid ServiceNeurology = Guid.Parse("44444444-4444-4444-4444-000000000006");
    public static readonly Guid ServiceEndocrinology = Guid.Parse("44444444-4444-4444-4444-000000000007");

    public static readonly Guid[] AllDoctorUserIds = { DoctorUser1, DoctorUser2, DoctorUser3, DoctorUser4, DoctorUser5, DoctorUser6, DoctorUser7 };
    public static readonly Guid[] AllReceptionistUserIds = { ReceptionistUser1, ReceptionistUser2, ReceptionistUser3, ReceptionistUser4, ReceptionistUser5, ReceptionistUser6, ReceptionistUser7 };
    public static readonly Guid[] AllDoctorIds = { Doctor1, Doctor2, Doctor3, Doctor4, Doctor5, Doctor6, Doctor7 };
    public static readonly Guid[] AllSpecializationIds = { SpecCardiologist, SpecGeneralPractitioner, SpecDermatologist, SpecPediatrician, SpecOrthopedist, SpecNeurologist, SpecEndocrinologist };
    public static readonly Guid[] AllServiceIds = { ServiceConsultation, ServiceCardiology, ServiceDermatology, ServicePediatric, ServiceOrthopedic, ServiceNeurology, ServiceEndocrinology };
}

public static class MockDataSeeder
{
    public static async Task SeedAsync(PractitionerDbContext db)
    {
        int created = 0;

        // Seed Specializations
        var specializations = new[]
        {
            (MockIds.SpecCardiologist, "Cardiologist"),
            (MockIds.SpecGeneralPractitioner, "General Practitioner"),
            (MockIds.SpecDermatologist, "Dermatologist"),
            (MockIds.SpecPediatrician, "Pediatrician"),
            (MockIds.SpecOrthopedist, "Orthopedist"),
            (MockIds.SpecNeurologist, "Neurologist"),
            (MockIds.SpecEndocrinologist, "Endocrinologist")
        };

        var existingSpecIds = await db.Specializations.Select(s => s.Id).ToHashSetAsync();
        foreach (var (id, name) in specializations)
        {
            if (!existingSpecIds.Contains(id))
            {
                db.Specializations.Add(new Specialization { Id = id, Name = name });
                created++;
            }
        }

        // Seed Medical Services
        var services = new[]
        {
            (MockIds.ServiceConsultation, "General Consultation", "Routine check-up and consultation"),
            (MockIds.ServiceCardiology, "Cardiology Services", "Heart and cardiovascular examinations"),
            (MockIds.ServiceDermatology, "Dermatology Services", "Skin conditions and treatments"),
            (MockIds.ServicePediatric, "Pediatric Care", "Child and adolescent healthcare"),
            (MockIds.ServiceOrthopedic, "Orthopedic Services", "Bone and joint treatments"),
            (MockIds.ServiceNeurology, "Neurology Services", "Brain and nervous system care"),
            (MockIds.ServiceEndocrinology, "Endocrinology Services", "Hormonal and metabolic treatments")
        };

        var existingServiceIds = await db.Services.Select(s => s.Id).ToHashSetAsync();
        foreach (var (id, name, description) in services)
        {
            if (!existingServiceIds.Contains(id))
            {
                db.Services.Add(new MedicalService { Id = id, Name = name, Description = description });
                created++;
            }
        }

        // Seed Doctors
        var doctorBios = new[]
        {
            "Cardiology specialist with 15 years of experience in interventional procedures.",
            "General practitioner focused on preventive care and family medicine.",
            "Dermatologist specializing in cosmetic and medical dermatology.",
            "Pediatrician providing comprehensive care for children of all ages.",
            "Orthopedic surgeon with expertise in joint replacement and sports medicine.",
            "Neurologist treating epilepsy, stroke, and neurodegenerative diseases.",
            "Endocrinologist managing diabetes, thyroid, and hormonal disorders."
        };

        var existingDoctorIds = await db.Doctors.Select(d => d.Id).ToHashSetAsync();
        for (int i = 0; i < 7; i++)
        {
            var doctorId = MockIds.AllDoctorIds[i];
            var userId = MockIds.AllDoctorUserIds[i];

            if (!existingDoctorIds.Contains(doctorId))
            {
                db.Doctors.Add(new Doctor
                {
                    Id = doctorId,
                    UserId = userId,
                    Bio = doctorBios[i],
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-60 + i),
                    UpdatedAt = DateTime.UtcNow
                });
                created++;
            }
        }

        // Seed Receptionists
        var existingReceptionistUserIds = await db.Receptionists.Select(r => r.UserId).ToHashSetAsync();
        for (int i = 0; i < 7; i++)
        {
            var userId = MockIds.AllReceptionistUserIds[i];
            if (!existingReceptionistUserIds.Contains(userId))
            {
                db.Receptionists.Add(new Receptionist
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow.AddDays(-45 + i),
                    UpdatedAt = DateTime.UtcNow
                });
                created++;
            }
        }

        if (created > 0)
        {
            await db.SaveChangesAsync();
        }

        // Seed Doctor-Specialization mappings (each doctor has 1-2 specializations)
        var doctorSpecMappings = new[]
        {
            (MockIds.Doctor1, MockIds.SpecCardiologist),
            (MockIds.Doctor1, MockIds.SpecGeneralPractitioner),
            (MockIds.Doctor2, MockIds.SpecGeneralPractitioner),
            (MockIds.Doctor3, MockIds.SpecDermatologist),
            (MockIds.Doctor3, MockIds.SpecGeneralPractitioner),
            (MockIds.Doctor4, MockIds.SpecPediatrician),
            (MockIds.Doctor4, MockIds.SpecGeneralPractitioner),
            (MockIds.Doctor5, MockIds.SpecOrthopedist),
            (MockIds.Doctor6, MockIds.SpecNeurologist),
            (MockIds.Doctor6, MockIds.SpecGeneralPractitioner),
            (MockIds.Doctor7, MockIds.SpecEndocrinologist)
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
            }
        }

        // Seed Specialization-Service mappings
        var specServiceMappings = new[]
        {
            (MockIds.SpecCardiologist, MockIds.ServiceCardiology),
            (MockIds.SpecCardiologist, MockIds.ServiceConsultation),
            (MockIds.SpecGeneralPractitioner, MockIds.ServiceConsultation),
            (MockIds.SpecDermatologist, MockIds.ServiceDermatology),
            (MockIds.SpecDermatologist, MockIds.ServiceConsultation),
            (MockIds.SpecPediatrician, MockIds.ServicePediatric),
            (MockIds.SpecPediatrician, MockIds.ServiceConsultation),
            (MockIds.SpecOrthopedist, MockIds.ServiceOrthopedic),
            (MockIds.SpecNeurologist, MockIds.ServiceNeurology),
            (MockIds.SpecEndocrinologist, MockIds.ServiceEndocrinology),
            (MockIds.SpecEndocrinologist, MockIds.ServiceConsultation)
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
            }
        }

        // Seed Doctor Schedules (each doctor has 2-3 schedule entries)
        var scheduleEntries = new (Guid doctorId, int dayOfWeek, TimeSpan start, TimeSpan end)[]
        {
            (MockIds.Doctor1, 1, new TimeSpan(9, 0, 0), new TimeSpan(13, 0, 0)),
            (MockIds.Doctor1, 3, new TimeSpan(14, 0, 0), new TimeSpan(18, 0, 0)),
            (MockIds.Doctor1, 5, new TimeSpan(9, 0, 0), new TimeSpan(12, 0, 0)),
            (MockIds.Doctor2, 1, new TimeSpan(8, 0, 0), new TimeSpan(16, 0, 0)),
            (MockIds.Doctor2, 2, new TimeSpan(8, 0, 0), new TimeSpan(16, 0, 0)),
            (MockIds.Doctor2, 4, new TimeSpan(8, 0, 0), new TimeSpan(16, 0, 0)),
            (MockIds.Doctor3, 2, new TimeSpan(10, 0, 0), new TimeSpan(18, 0, 0)),
            (MockIds.Doctor3, 4, new TimeSpan(10, 0, 0), new TimeSpan(18, 0, 0)),
            (MockIds.Doctor4, 1, new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0)),
            (MockIds.Doctor4, 3, new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0)),
            (MockIds.Doctor4, 5, new TimeSpan(9, 0, 0), new TimeSpan(13, 0, 0)),
            (MockIds.Doctor5, 2, new TimeSpan(7, 0, 0), new TimeSpan(15, 0, 0)),
            (MockIds.Doctor5, 4, new TimeSpan(7, 0, 0), new TimeSpan(15, 0, 0)),
            (MockIds.Doctor6, 1, new TimeSpan(11, 0, 0), new TimeSpan(19, 0, 0)),
            (MockIds.Doctor6, 3, new TimeSpan(11, 0, 0), new TimeSpan(19, 0, 0)),
            (MockIds.Doctor7, 2, new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0)),
            (MockIds.Doctor7, 5, new TimeSpan(9, 0, 0), new TimeSpan(14, 0, 0))
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
            }
        }

        if (created > 0)
        {
            await db.SaveChangesAsync();
            Console.WriteLine($"[MockDataSeeder] Created {created} practitioner records (doctors, receptionists, specializations, services, schedules, mappings).");
        }
        else
        {
            Console.WriteLine("[MockDataSeeder] All practitioner mock data already exists.");
        }
    }
}
