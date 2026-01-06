using PatientService.Data;
using PatientService.Models;

namespace PatientService.Data;

public static class PatientSeeder
{
    // Shared IDs - matching UserService and MedicalRecordsService
    private static readonly Guid Patient1Id = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000001"); // Alice
    private static readonly Guid Patient2Id = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000002"); // Bob

    public static async Task SeedAsync(PatientDbContext db)
    {
        await SeedPatientAsync(db, Patient1Id, Patient1Id, "Active");
        await SeedPatientAsync(db, Patient2Id, Patient2Id, "Active");

        await db.SaveChangesAsync();
    }

    private static async Task SeedPatientAsync(PatientDbContext db, Guid id, Guid userId, string status)
    {
        if (await db.Patients.FindAsync(id) == null)
        {
            var patient = new Patient
            {
                Id = id,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            db.Patients.Add(patient);
            
            db.PatientStatuses.Add(new PatientStatus
            {
                PatientId = id,
                Status = status,
                EffectiveAt = DateTime.UtcNow
            });
        }
    }
}
