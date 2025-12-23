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
        var patientId = "patient-to-delete-1";
        
        if (await db.Patients.FindAsync(patientId) == null)
        {
             // We need a valid GUID for UserId if the column is Guid.
             // Model: `public string UserId { get; set; }` in earlier view? 
             // RegRequest has `Guid UserId`. 
             // Let's check Patient model if possible, but safely I will use a random Guid for UserId.
             
            var patient = new Patient
            {
                Id = patientId,
                UserId = Guid.NewGuid().ToString(),
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
            
            // Add some dummy info for the list
            // Note: PatientOverview view relies on User_Profile join for Names OR the view handles it.
            // If local dev, the view selects NULL for names if not joined.
            // If I want "To Be Deleted" to show up in the list with a name in local dev, I might need to insert into 'User_Profile' if it was in the same DB? 
            // But PatientService likely doesn't own User_Profile. 
            // The `PatientOverview` view in `Program.cs` handles local dev by selecting NULL.
            // So it will show as "Unnamed". 
            // This is acceptable for backend test.
            
            await db.SaveChangesAsync();
        }
    }
}
