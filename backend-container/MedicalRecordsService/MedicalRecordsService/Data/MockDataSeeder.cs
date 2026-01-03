using Microsoft.EntityFrameworkCore;
using MedicalRecordsService.Models;

namespace MedicalRecordsService.Data;

/// <summary>
/// Shared deterministic IDs for cross-service mock data references
/// Patient/Doctor IDs match User IDs from UserService for seamless auth integration
/// </summary>
public static class MockIds
{
    // Patient IDs (matching User IDs from UserService for login integration)
    public static readonly Guid Patient1 = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000001");
    public static readonly Guid Patient2 = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000002");
    public static readonly Guid Patient3 = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000003");
    public static readonly Guid Patient4 = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000004");
    public static readonly Guid Patient5 = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000005");
    public static readonly Guid Patient6 = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000006");
    public static readonly Guid Patient7 = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000007");

    // Doctor IDs (matching User IDs from UserService for login integration)
    public static readonly Guid Doctor1 = Guid.Parse("bbbbbbbb-0002-0002-0002-000000000001");
    public static readonly Guid Doctor2 = Guid.Parse("bbbbbbbb-0002-0002-0002-000000000002");
    public static readonly Guid Doctor3 = Guid.Parse("bbbbbbbb-0002-0002-0002-000000000003");
    public static readonly Guid Doctor4 = Guid.Parse("bbbbbbbb-0002-0002-0002-000000000004");
    public static readonly Guid Doctor5 = Guid.Parse("bbbbbbbb-0002-0002-0002-000000000005");
    public static readonly Guid Doctor6 = Guid.Parse("bbbbbbbb-0002-0002-0002-000000000006");
    public static readonly Guid Doctor7 = Guid.Parse("bbbbbbbb-0002-0002-0002-000000000007");

    // Appointment IDs (from AppointmentService)
    public static readonly Guid Appointment1 = Guid.Parse("55555555-5555-5555-5555-000000000001");
    public static readonly Guid Appointment2 = Guid.Parse("55555555-5555-5555-5555-000000000002");
    public static readonly Guid Appointment3 = Guid.Parse("55555555-5555-5555-5555-000000000003");
    public static readonly Guid Appointment4 = Guid.Parse("55555555-5555-5555-5555-000000000004");
    public static readonly Guid Appointment5 = Guid.Parse("55555555-5555-5555-5555-000000000005");
    public static readonly Guid Appointment6 = Guid.Parse("55555555-5555-5555-5555-000000000006");
    public static readonly Guid Appointment7 = Guid.Parse("55555555-5555-5555-5555-000000000007");

    // Medical Record IDs
    public static readonly Guid Record1 = Guid.Parse("bbbb1111-1111-1111-1111-000000000001");
    public static readonly Guid Record2 = Guid.Parse("bbbb1111-1111-1111-1111-000000000002");
    public static readonly Guid Record3 = Guid.Parse("bbbb1111-1111-1111-1111-000000000003");
    public static readonly Guid Record4 = Guid.Parse("bbbb1111-1111-1111-1111-000000000004");
    public static readonly Guid Record5 = Guid.Parse("bbbb1111-1111-1111-1111-000000000005");
    public static readonly Guid Record6 = Guid.Parse("bbbb1111-1111-1111-1111-000000000006");
    public static readonly Guid Record7 = Guid.Parse("bbbb1111-1111-1111-1111-000000000007");

    public static readonly Guid[] AllPatientIds = { Patient1, Patient2, Patient3, Patient4, Patient5, Patient6, Patient7 };
    public static readonly Guid[] AllDoctorIds = { Doctor1, Doctor2, Doctor3, Doctor4, Doctor5, Doctor6, Doctor7 };
    public static readonly Guid[] AllAppointmentIds = { Appointment1, Appointment2, Appointment3, Appointment4, Appointment5, Appointment6, Appointment7 };
    public static readonly Guid[] AllRecordIds = { Record1, Record2, Record3, Record4, Record5, Record6, Record7 };
}

public static class MockDataSeeder
{
    public static async Task SeedAsync(MedicalRecordsDbContext db)
    {
        int created = 0;

        // Medical Records data
        var recordData = new[]
        {
            (MockIds.Record1, MockIds.Patient1, MockIds.Doctor1, MockIds.Appointment1, "Persistent headaches and fatigue", "Patient reports 2-week history of daily tension headaches", "NAD, BP 128/82, normal neurological exam", "Tension headache, rule out secondary causes", "Lifestyle modifications, OTC analgesics"),
            (MockIds.Record2, MockIds.Patient2, MockIds.Doctor2, MockIds.Appointment2, "Annual wellness exam", "No acute complaints, routine checkup", "All vitals normal, healthy appearance", "General good health", "Continue current lifestyle, schedule follow-up in 1 year"),
            (MockIds.Record3, MockIds.Patient3, MockIds.Doctor3, MockIds.Appointment3, "Skin rash on arms", "Itchy red patches for 1 week", "Erythematous papules bilateral arms", "Contact dermatitis", "Topical steroid, avoid irritants"),
            (MockIds.Record4, MockIds.Patient4, MockIds.Doctor4, MockIds.Appointment4, "Child well-visit", "Routine pediatric checkup", "Growth and development appropriate for age", "Healthy child", "Vaccinations up to date, next visit in 6 months"),
            (MockIds.Record5, MockIds.Patient5, MockIds.Doctor5, MockIds.Appointment5, "Knee pain after sports", "Right knee pain for 3 days after soccer", "Mild effusion right knee, stable ligaments", "Knee contusion, possible meniscal strain", "RICE protocol, NSAID, follow-up if no improvement"),
            (MockIds.Record6, MockIds.Patient6, MockIds.Doctor6, MockIds.Appointment6, "Memory concerns", "Family reports mild forgetfulness over 6 months", "MMSE 27/30, mild short-term memory issues", "Mild cognitive impairment, needs further workup", "MRI brain, neuropsych testing"),
            (MockIds.Record7, MockIds.Patient7, MockIds.Doctor7, MockIds.Appointment7, "Fatigue and weight gain", "Unexplained 10lb weight gain, fatigue x 3 months", "Mild bradycardia, dry skin, sluggish reflexes", "Hypothyroidism suspected", "TSH and Free T4 ordered, follow-up for results")
        };

        var existingRecordIds = await db.MedicalRecords.Select(r => r.Id).ToHashSetAsync();
        foreach (var (id, patientId, doctorId, appointmentId, complaint, hpi, exam, assessment, plan) in recordData)
        {
            if (!existingRecordIds.Contains(id))
            {
                db.MedicalRecords.Add(new MedicalRecord
                {
                    Id = id,
                    PatientId = patientId,
                    DoctorId = doctorId,
                    AppointmentId = appointmentId,
                    VisitDate = DateTime.UtcNow.AddDays(-7 + Array.IndexOf(MockIds.AllRecordIds, id)),
                    ChiefComplaint = complaint,
                    HistoryOfPresentIllness = hpi,
                    PhysicalExamination = exam,
                    Assessment = assessment,
                    Plan = plan,
                    Notes = "Medical record created during mock data seeding",
                    CreatedAt = DateTime.UtcNow.AddDays(-7),
                    UpdatedAt = DateTime.UtcNow
                });
                created++;
            }
        }

        if (created > 0)
        {
            await db.SaveChangesAsync();
        }

        // Seed Prescriptions (linked to medical records)
        var prescriptionData = new[]
        {
            (MockIds.Record1, MockIds.Patient1, MockIds.Doctor1, "Ibuprofen", "M01AE01", "400mg", "Every 6 hours as needed", 14, "Take with food"),
            (MockIds.Record2, MockIds.Patient2, MockIds.Doctor2, "Vitamin D3", null, "1000 IU", "Once daily", 90, "Take with fatty meal for absorption"),
            (MockIds.Record3, MockIds.Patient3, MockIds.Doctor3, "Hydrocortisone cream", "D07AA02", "1%", "Apply twice daily", 14, "Apply thin layer to affected areas"),
            (MockIds.Record5, MockIds.Patient5, MockIds.Doctor5, "Naproxen", "M01AE02", "500mg", "Twice daily", 7, "Take with food to reduce GI upset"),
            (MockIds.Record6, MockIds.Patient6, MockIds.Doctor6, "Vitamin B12", "B03BA01", "1000mcg", "Once daily", 30, "Sublingual form preferred"),
            (MockIds.Record7, MockIds.Patient7, MockIds.Doctor7, "Levothyroxine", "H03AA01", "50mcg", "Once daily in morning", 30, "Take on empty stomach, wait 30 min before eating"),
            (MockIds.Record1, MockIds.Patient1, MockIds.Doctor1, "Acetaminophen", "N02BE01", "500mg", "Every 4-6 hours as needed", 14, "Do not exceed 3g daily")
        };

        var existingPrescriptionRecordIds = await db.Prescriptions.Select(p => p.MedicalRecordId).ToHashSetAsync();
        foreach (var (recordId, patientId, doctorId, medName, atcCode, dosage, freq, duration, instructions) in prescriptionData)
        {
            // Allow multiple prescriptions per record, check unique by medication name
            var exists = await db.Prescriptions.AnyAsync(p => p.MedicalRecordId == recordId && p.MedicationName == medName);
            if (!exists)
            {
                db.Prescriptions.Add(new Prescription
                {
                    Id = Guid.NewGuid(),
                    MedicalRecordId = recordId,
                    PatientId = patientId,
                    DoctorId = doctorId,
                    MedicationName = medName,
                    AtcCode = atcCode,
                    Dosage = dosage,
                    Frequency = freq,
                    DurationDays = duration,
                    Instructions = instructions,
                    PrescribedDate = DateTime.UtcNow.AddDays(-7),
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow.AddDays(-7),
                    UpdatedAt = DateTime.UtcNow
                });
                created++;
            }
        }

        // Seed Diagnoses (linked to medical records)
        var diagnosisData = new[]
        {
            (MockIds.Record1, "G43.909", "Migraine, unspecified", "Primary"),
            (MockIds.Record1, "R51", "Headache", "Secondary"),
            (MockIds.Record2, "Z00.00", "Encounter for general adult medical examination", "Primary"),
            (MockIds.Record3, "L23.9", "Allergic contact dermatitis, unspecified cause", "Primary"),
            (MockIds.Record4, "Z00.129", "Encounter for routine child health examination", "Primary"),
            (MockIds.Record5, "S80.01", "Contusion of right knee", "Primary"),
            (MockIds.Record5, "S83.249", "Other tear of medial meniscus", "Secondary"),
            (MockIds.Record6, "F06.70", "Mild neurocognitive disorder due to unknown cause", "Primary"),
            (MockIds.Record7, "E03.9", "Hypothyroidism, unspecified", "Primary")
        };

        var existingDiagnoses = await db.Diagnoses
            .Select(d => new { d.MedicalRecordId, d.Icd10Code })
            .ToListAsync();
        var existingDiagnosisSet = existingDiagnoses.Select(x => (x.MedicalRecordId, x.Icd10Code)).ToHashSet();

        foreach (var (recordId, icd10, description, type) in diagnosisData)
        {
            if (!existingDiagnosisSet.Contains((recordId, icd10)))
            {
                db.Diagnoses.Add(new Diagnosis
                {
                    Id = Guid.NewGuid(),
                    MedicalRecordId = recordId,
                    Icd10Code = icd10,
                    Description = description,
                    Type = type,
                    CreatedAt = DateTime.UtcNow.AddDays(-7)
                });
                created++;
            }
        }

        // Seed Vital Signs (linked to medical records and patients)
        var vitalSignsData = new (Guid recordId, Guid patientId, decimal? temp, int? sys, int? dia, int? hr, int? rr, decimal? o2, decimal? height, decimal? weight)[]
        {
            (MockIds.Record1, MockIds.Patient1, 98.6m, 128, 82, 72, 16, 98.5m, 170m, 75m),
            (MockIds.Record2, MockIds.Patient2, 98.4m, 118, 76, 68, 14, 99m, 165m, 68m),
            (MockIds.Record3, MockIds.Patient3, 98.8m, 122, 78, 74, 16, 98m, 158m, 62m),
            (MockIds.Record4, MockIds.Patient4, 98.2m, 100, 65, 88, 20, 99m, 120m, 28m),
            (MockIds.Record5, MockIds.Patient5, 98.6m, 130, 85, 78, 18, 97m, 180m, 82m),
            (MockIds.Record6, MockIds.Patient6, 97.8m, 140, 88, 62, 14, 96m, 162m, 70m),
            (MockIds.Record7, MockIds.Patient7, 97.2m, 115, 72, 58, 12, 98m, 155m, 72m)
        };

        var existingVitalRecordIds = await db.VitalSigns.Select(v => v.MedicalRecordId).ToHashSetAsync();
        foreach (var (recordId, patientId, temp, sys, dia, hr, rr, o2, height, weight) in vitalSignsData)
        {
            if (!existingVitalRecordIds.Contains(recordId))
            {
                db.VitalSigns.Add(new VitalSigns
                {
                    Id = Guid.NewGuid(),
                    MedicalRecordId = recordId,
                    PatientId = patientId,
                    MeasuredAt = DateTime.UtcNow.AddDays(-7 + Array.IndexOf(MockIds.AllRecordIds, recordId)),
                    Temperature = temp,
                    SystolicBP = sys,
                    DiastolicBP = dia,
                    HeartRate = hr,
                    RespiratoryRate = rr,
                    OxygenSaturation = o2,
                    Height = height,
                    Weight = weight,
                    Notes = "Vital signs recorded during visit",
                    CreatedAt = DateTime.UtcNow.AddDays(-7)
                });
                created++;
            }
        }

        if (created > 0)
        {
            await db.SaveChangesAsync();
            Console.WriteLine($"[MockDataSeeder] Created {created} medical records (records, prescriptions, diagnoses, vital signs).");
        }
        else
        {
            Console.WriteLine("[MockDataSeeder] All medical records mock data already exists.");
        }
    }
}
