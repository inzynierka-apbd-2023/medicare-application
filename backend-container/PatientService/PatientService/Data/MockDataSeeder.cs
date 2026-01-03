using Microsoft.EntityFrameworkCore;
using PatientService.Models;

namespace PatientService.Data;

/// <summary>
/// Shared deterministic IDs for cross-service mock data references
/// Patient IDs match User IDs from UserService for seamless auth integration
/// </summary>
public static class MockIds
{
    // Patient User IDs (from UserService) - used as both User ID and Patient entity ID
    public static readonly Guid PatientUser1 = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000001");
    public static readonly Guid PatientUser2 = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000002");
    public static readonly Guid PatientUser3 = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000003");
    public static readonly Guid PatientUser4 = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000004");
    public static readonly Guid PatientUser5 = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000005");
    public static readonly Guid PatientUser6 = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000006");
    public static readonly Guid PatientUser7 = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000007");

    // Doctor User IDs (for PrimaryDoctorId reference)
    public static readonly Guid DoctorUser1 = Guid.Parse("bbbbbbbb-0002-0002-0002-000000000001");
    public static readonly Guid DoctorUser2 = Guid.Parse("bbbbbbbb-0002-0002-0002-000000000002");
    public static readonly Guid DoctorUser3 = Guid.Parse("bbbbbbbb-0002-0002-0002-000000000003");
    public static readonly Guid DoctorUser4 = Guid.Parse("bbbbbbbb-0002-0002-0002-000000000004");
    public static readonly Guid DoctorUser5 = Guid.Parse("bbbbbbbb-0002-0002-0002-000000000005");
    public static readonly Guid DoctorUser6 = Guid.Parse("bbbbbbbb-0002-0002-0002-000000000006");
    public static readonly Guid DoctorUser7 = Guid.Parse("bbbbbbbb-0002-0002-0002-000000000007");

    // Patient entity IDs = User IDs (simplified: Patient.Id == Patient.UserId)
    public static readonly Guid Patient1 = PatientUser1;
    public static readonly Guid Patient2 = PatientUser2;
    public static readonly Guid Patient3 = PatientUser3;
    public static readonly Guid Patient4 = PatientUser4;
    public static readonly Guid Patient5 = PatientUser5;
    public static readonly Guid Patient6 = PatientUser6;
    public static readonly Guid Patient7 = PatientUser7;

    public static readonly Guid[] AllPatientUserIds = { PatientUser1, PatientUser2, PatientUser3, PatientUser4, PatientUser5, PatientUser6, PatientUser7 };
    public static readonly Guid[] AllDoctorUserIds = { DoctorUser1, DoctorUser2, DoctorUser3, DoctorUser4, DoctorUser5, DoctorUser6, DoctorUser7 };
    public static readonly Guid[] AllPatientIds = { Patient1, Patient2, Patient3, Patient4, Patient5, Patient6, Patient7 };
}

public static class MockDataSeeder
{
    public static async Task SeedAsync(PatientDbContext db)
    {
        var existingPatientIds = await db.Patients.Select(p => p.Id).ToHashSetAsync();
        int patientsCreated = 0;
        int contactsCreated = 0;
        int insuranceCreated = 0;
        int statusesCreated = 0;

        // Emergency contact data
        var emergencyContacts = new[]
        {
            ("John Johnson Sr.", "Father", "+1-555-1101"),
            ("Mary Smith", "Spouse", "+1-555-1102"),
            ("Robert Williams", "Brother", "+1-555-1103"),
            ("Susan Brown", "Mother", "+1-555-1104"),
            ("James Davis", "Spouse", "+1-555-1105"),
            ("Patricia Miller", "Sister", "+1-555-1106"),
            ("Michael Wilson", "Son", "+1-555-1107")
        };

        // Insurance providers data
        var insuranceProviders = new[]
        {
            ("Blue Cross Blue Shield", "BCBS-2024-001", DateTime.UtcNow.AddYears(-1), DateTime.UtcNow.AddYears(1)),
            ("Aetna Health", "AET-2024-002", DateTime.UtcNow.AddMonths(-6), DateTime.UtcNow.AddMonths(18)),
            ("United Healthcare", "UHC-2024-003", DateTime.UtcNow.AddYears(-2), DateTime.UtcNow.AddMonths(6)),
            ("Cigna Insurance", "CIG-2024-004", DateTime.UtcNow.AddMonths(-3), DateTime.UtcNow.AddYears(2)),
            ("Humana Health", "HUM-2024-005", DateTime.UtcNow.AddYears(-1), DateTime.UtcNow.AddYears(1)),
            ("Kaiser Permanente", "KP-2024-006", DateTime.UtcNow.AddMonths(-8), DateTime.UtcNow.AddMonths(16)),
            ("Anthem Blue Cross", "ABC-2024-007", DateTime.UtcNow.AddMonths(-4), DateTime.UtcNow.AddMonths(20))
        };

        for (int i = 0; i < 7; i++)
        {
            var patientId = MockIds.AllPatientIds[i];
            var userId = MockIds.AllPatientUserIds[i];
            var doctorId = MockIds.AllDoctorUserIds[i % 7]; // Assign primary doctor

            if (!existingPatientIds.Contains(patientId))
            {
                // Create Patient
                db.Patients.Add(new Patient
                {
                    Id = patientId,
                    UserId = userId,
                    PrimaryDoctorId = doctorId,
                    CreatedAt = DateTime.UtcNow.AddDays(-30 + i),
                    UpdatedAt = DateTime.UtcNow
                });
                patientsCreated++;

                // Create Emergency Contact
                var (contactName, relation, phone) = emergencyContacts[i];
                db.EmergencyContacts.Add(new EmergencyContact
                {
                    Id = Guid.NewGuid(),
                    PatientId = patientId,
                    Name = contactName,
                    Relation = relation,
                    Phone = phone
                });
                contactsCreated++;

                // Create Insurance
                var (provider, policyNum, validFrom, validTo) = insuranceProviders[i];
                db.Insurances.Add(new Insurance
                {
                    Id = Guid.NewGuid(),
                    PatientId = patientId,
                    Provider = provider,
                    PolicyNumber = policyNum,
                    ValidFrom = validFrom,
                    ValidTo = validTo
                });
                insuranceCreated++;

                // Create Patient Status
                db.PatientStatuses.Add(new PatientStatus
                {
                    Id = Guid.NewGuid(),
                    PatientId = patientId,
                    Status = "Active",
                    EffectiveAt = DateTime.UtcNow.AddDays(-30 + i),
                    IdempotencyKey = $"initial-status-{patientId}"
                });
                statusesCreated++;
            }
        }

        // Add additional emergency contacts (total 14 = 7 patients x 2 contacts each)
        var additionalContacts = new[]
        {
            (MockIds.Patient1, "Alice Johnson Jr.", "Daughter", "+1-555-1111"),
            (MockIds.Patient2, "Bob Smith Sr.", "Father", "+1-555-1112"),
            (MockIds.Patient3, "David Williams", "Son", "+1-555-1113"),
            (MockIds.Patient4, "Emma Brown", "Daughter", "+1-555-1114"),
            (MockIds.Patient5, "Frank Davis", "Brother", "+1-555-1115"),
            (MockIds.Patient6, "Grace Miller", "Mother", "+1-555-1116"),
            (MockIds.Patient7, "Henry Wilson", "Father", "+1-555-1117")
        };

        var existingContactPatientIds = await db.EmergencyContacts
            .GroupBy(c => c.PatientId)
            .Where(g => g.Count() >= 2)
            .Select(g => g.Key)
            .ToHashSetAsync();

        foreach (var (patientId, name, relation, phone) in additionalContacts)
        {
            if (!existingContactPatientIds.Contains(patientId) && existingPatientIds.Contains(patientId) || patientsCreated > 0)
            {
                // Only add if patient was just created or doesn't have 2 contacts yet
                var contactCount = await db.EmergencyContacts.CountAsync(c => c.PatientId == patientId);
                if (contactCount < 2)
                {
                    db.EmergencyContacts.Add(new EmergencyContact
                    {
                        Id = Guid.NewGuid(),
                        PatientId = patientId,
                        Name = name,
                        Relation = relation,
                        Phone = phone
                    });
                    contactsCreated++;
                }
            }
        }

        if (patientsCreated > 0 || contactsCreated > 0 || insuranceCreated > 0 || statusesCreated > 0)
        {
            await db.SaveChangesAsync();
            Console.WriteLine($"[MockDataSeeder] Created {patientsCreated} patients, {contactsCreated} emergency contacts, {insuranceCreated} insurance records, {statusesCreated} statuses.");
        }
        else
        {
            Console.WriteLine("[MockDataSeeder] All patient mock data already exists.");
        }
    }
}
