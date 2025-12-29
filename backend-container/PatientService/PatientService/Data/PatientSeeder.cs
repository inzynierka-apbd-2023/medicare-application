using PatientService.Data;
using PatientService.Models;

namespace PatientService.Data;

public static class PatientSeeder
{
    public static async Task SeedAsync(PatientDbContext db)
    {
        // Check if our specific test patient exists
        var testUserId = "reception_a_20250818"; // This is actually a username, but Patient model maps UserId. 
        // NOTE: The Patient.UserId is often a GUID from Identity service.
        // However, the functionalities.md says "is for user reception_a...".
        // If the Identity service isn't mocking this user ID as a guid, we might have issues if UserId is typed GUID in DB.
        // Looking at RegisterPatientRequest: Guid UserId.
        // So `reception_a_20250818` is likely the USERNAME in the auth system, and has a corresponding GUID.
        // I will assume for the SEEDER that we need to create a patient that "reception_a..." can Delete.
        // It doesn't mean the patient's ID is "reception_a...".
        // Let's create a patient named "To Be Deleted". 
        // AND ensuring "reception_a..." has permissions is an Auth concern (Roles). 
        // The prompt says "is for user reception_a_20250818 etc". 
        // This implies this user will perform the action.
        
        // Let's seed a patient with a known ID so the frontend/manual test can target it.
        // Let's seed a patient with a known ID so the frontend/manual test can target it.
        // Guid equivalent of "patient-to-delete-1" deterministic hash or fixed guid
        var patientId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        
        if (await db.Patients.FindAsync(patientId) == null)
        {
            var patient = new Patient
            {
                Id = patientId,
                UserId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            db.Patients.Add(patient);
            
            db.PatientStatuses.Add(new PatientStatus
            {
                PatientId = patientId,
                Status = "Active",
                EffectiveAt = DateTime.UtcNow
            });

            await db.SaveChangesAsync();
        }
    }
}
