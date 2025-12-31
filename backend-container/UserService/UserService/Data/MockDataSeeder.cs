using Microsoft.EntityFrameworkCore;
using UserService.Models;

namespace UserService.Data;

/// <summary>
/// Shared deterministic IDs for cross-service mock data references
/// </summary>
public static class MockIds
{
    // Patient Users (7)
    public static readonly Guid PatientUser1 = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000001");
    public static readonly Guid PatientUser2 = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000002");
    public static readonly Guid PatientUser3 = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000003");
    public static readonly Guid PatientUser4 = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000004");
    public static readonly Guid PatientUser5 = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000005");
    public static readonly Guid PatientUser6 = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000006");
    public static readonly Guid PatientUser7 = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000007");

    // Doctor Users (7)
    public static readonly Guid DoctorUser1 = Guid.Parse("bbbbbbbb-0002-0002-0002-000000000001");
    public static readonly Guid DoctorUser2 = Guid.Parse("bbbbbbbb-0002-0002-0002-000000000002");
    public static readonly Guid DoctorUser3 = Guid.Parse("bbbbbbbb-0002-0002-0002-000000000003");
    public static readonly Guid DoctorUser4 = Guid.Parse("bbbbbbbb-0002-0002-0002-000000000004");
    public static readonly Guid DoctorUser5 = Guid.Parse("bbbbbbbb-0002-0002-0002-000000000005");
    public static readonly Guid DoctorUser6 = Guid.Parse("bbbbbbbb-0002-0002-0002-000000000006");
    public static readonly Guid DoctorUser7 = Guid.Parse("bbbbbbbb-0002-0002-0002-000000000007");

    // Receptionist Users (7)
    public static readonly Guid ReceptionistUser1 = Guid.Parse("cccccccc-0003-0003-0003-000000000001");
    public static readonly Guid ReceptionistUser2 = Guid.Parse("cccccccc-0003-0003-0003-000000000002");
    public static readonly Guid ReceptionistUser3 = Guid.Parse("cccccccc-0003-0003-0003-000000000003");
    public static readonly Guid ReceptionistUser4 = Guid.Parse("cccccccc-0003-0003-0003-000000000004");
    public static readonly Guid ReceptionistUser5 = Guid.Parse("cccccccc-0003-0003-0003-000000000005");
    public static readonly Guid ReceptionistUser6 = Guid.Parse("cccccccc-0003-0003-0003-000000000006");
    public static readonly Guid ReceptionistUser7 = Guid.Parse("cccccccc-0003-0003-0003-000000000007");

    // Admin User
    public static readonly Guid AdminUser1 = Guid.Parse("dddddddd-0004-0004-0004-000000000001");

    // Owner User
    public static readonly Guid OwnerUser1 = Guid.Parse("eeeeeeee-0005-0005-0005-000000000001");

    // All User IDs for easy iteration
    public static readonly Guid[] AllPatientUserIds = { PatientUser1, PatientUser2, PatientUser3, PatientUser4, PatientUser5, PatientUser6, PatientUser7 };
    public static readonly Guid[] AllDoctorUserIds = { DoctorUser1, DoctorUser2, DoctorUser3, DoctorUser4, DoctorUser5, DoctorUser6, DoctorUser7 };
    public static readonly Guid[] AllReceptionistUserIds = { ReceptionistUser1, ReceptionistUser2, ReceptionistUser3, ReceptionistUser4, ReceptionistUser5, ReceptionistUser6, ReceptionistUser7 };
}

public static class MockDataSeeder
{
    public static async Task SeedAsync(UserDbContext db)
    {
        var roles = await db.Roles.ToDictionaryAsync(r => r.Name, r => r.Id);
        if (!roles.Any())
        {
            Console.WriteLine("[MockDataSeeder] No roles found! Skipping user seeding.");
            return;
        }

        var existingUserIds = await db.Users.Select(u => u.Id).ToHashSetAsync();
        int created = 0;

        // Seed 7 Patient Users
        var patientNames = new[] 
        { 
            ("Alice", "Johnson", "alice.johnson@medicare.local"),
            ("Bob", "Smith", "bob.smith@medicare.local"),
            ("Carol", "Williams", "carol.williams@medicare.local"),
            ("David", "Brown", "david.brown@medicare.local"),
            ("Emma", "Davis", "emma.davis@medicare.local"),
            ("Frank", "Miller", "frank.miller@medicare.local"),
            ("Grace", "Wilson", "grace.wilson@medicare.local")
        };

        for (int i = 0; i < 7; i++)
        {
            var userId = MockIds.AllPatientUserIds[i];
            if (existingUserIds.Contains(userId)) continue;
            if (!roles.TryGetValue("Patient", out var roleId)) continue;

            var (firstName, lastName, email) = patientNames[i];
            db.Users.Add(new User
            {
                Id = userId,
                Username = $"patient_{i + 1}@medicare.local",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("P@ssw0rd!"),
                RoleId = roleId,
                CreatedAt = DateTime.UtcNow.AddDays(-30 + i),
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            });
            db.UserProfiles.Add(new UserProfile
            {
                UserId = userId,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Phone = $"+1-555-010{i + 1}",
                DateOfBirth = new DateTime(1980 + i * 5, 3, 15),
                Gender = i % 2 == 0 ? "Female" : "Male",
                AddressLine1 = $"{100 + i * 10} Main Street",
                City = "Springfield",
                State = "IL",
                ZipCode = $"6270{i}",
                Country = "USA",
                CreatedAt = DateTime.UtcNow.AddDays(-30 + i),
                UpdatedAt = DateTime.UtcNow
            });
            created++;
        }

        // Seed 7 Doctor Users
        var doctorNames = new[] 
        { 
            ("Dr. John", "Carter", "john.carter@medicare.local", "Cardiology specialist with 15 years of experience"),
            ("Dr. Sarah", "Chen", "sarah.chen@medicare.local", "General practitioner focused on preventive care"),
            ("Dr. Michael", "Roberts", "michael.roberts@medicare.local", "Dermatologist specializing in skin conditions"),
            ("Dr. Emily", "Thompson", "emily.thompson@medicare.local", "Pediatrician providing comprehensive child care"),
            ("Dr. James", "Wilson", "james.wilson@medicare.local", "Orthopedic surgeon with expertise in joint replacement"),
            ("Dr. Lisa", "Anderson", "lisa.anderson@medicare.local", "Neurologist treating brain and nervous system disorders"),
            ("Dr. Robert", "Martinez", "robert.martinez@medicare.local", "Endocrinologist managing hormonal conditions")
        };

        for (int i = 0; i < 7; i++)
        {
            var userId = MockIds.AllDoctorUserIds[i];
            if (existingUserIds.Contains(userId)) continue;
            if (!roles.TryGetValue("Doctor", out var roleId)) continue;

            var (firstName, lastName, email, _) = doctorNames[i];
            db.Users.Add(new User
            {
                Id = userId,
                Username = $"doctor_{i + 1}@medicare.local",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("P@ssw0rd!"),
                RoleId = roleId,
                CreatedAt = DateTime.UtcNow.AddDays(-60 + i),
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            });
            db.UserProfiles.Add(new UserProfile
            {
                UserId = userId,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Phone = $"+1-555-020{i + 1}",
                DateOfBirth = new DateTime(1970 + i * 3, 6, 20),
                Gender = i % 2 == 0 ? "Male" : "Female",
                AddressLine1 = $"{200 + i * 10} Medical Plaza",
                City = "Springfield",
                State = "IL",
                ZipCode = $"6271{i}",
                Country = "USA",
                CreatedAt = DateTime.UtcNow.AddDays(-60 + i),
                UpdatedAt = DateTime.UtcNow
            });
            created++;
        }

        // Seed 7 Receptionist Users
        var receptionistNames = new[] 
        { 
            ("Nancy", "Taylor", "nancy.taylor@medicare.local"),
            ("Patricia", "Thomas", "patricia.thomas@medicare.local"),
            ("Richard", "Jackson", "richard.jackson@medicare.local"),
            ("Susan", "White", "susan.white@medicare.local"),
            ("Thomas", "Harris", "thomas.harris@medicare.local"),
            ("Jennifer", "Martin", "jennifer.martin@medicare.local"),
            ("William", "Garcia", "william.garcia@medicare.local")
        };

        for (int i = 0; i < 7; i++)
        {
            var userId = MockIds.AllReceptionistUserIds[i];
            if (existingUserIds.Contains(userId)) continue;
            if (!roles.TryGetValue("Receptionist", out var roleId)) continue;

            var (firstName, lastName, email) = receptionistNames[i];
            db.Users.Add(new User
            {
                Id = userId,
                Username = $"receptionist_{i + 1}@medicare.local",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("P@ssw0rd!"),
                RoleId = roleId,
                CreatedAt = DateTime.UtcNow.AddDays(-45 + i),
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            });
            db.UserProfiles.Add(new UserProfile
            {
                UserId = userId,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Phone = $"+1-555-030{i + 1}",
                DateOfBirth = new DateTime(1985 + i * 2, 9, 10),
                Gender = i % 2 == 0 ? "Female" : "Male",
                AddressLine1 = $"{300 + i * 10} Reception Way",
                City = "Springfield",
                State = "IL",
                ZipCode = $"6272{i}",
                Country = "USA",
                CreatedAt = DateTime.UtcNow.AddDays(-45 + i),
                UpdatedAt = DateTime.UtcNow
            });
            created++;
        }

        // Seed Admin User
        if (!existingUserIds.Contains(MockIds.AdminUser1) && roles.TryGetValue("Admin", out var adminRoleId))
        {
            db.Users.Add(new User
            {
                Id = MockIds.AdminUser1,
                Username = "admin@medicare.local",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("P@ssw0rd!"),
                RoleId = adminRoleId,
                CreatedAt = DateTime.UtcNow.AddDays(-90),
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            });
            db.UserProfiles.Add(new UserProfile
            {
                UserId = MockIds.AdminUser1,
                FirstName = "System",
                LastName = "Administrator",
                Email = "admin@medicare.local",
                Phone = "+1-555-0001",
                CreatedAt = DateTime.UtcNow.AddDays(-90),
                UpdatedAt = DateTime.UtcNow
            });
            created++;
        }

        // Seed Owner User
        if (!existingUserIds.Contains(MockIds.OwnerUser1) && roles.TryGetValue("Owner", out var ownerRoleId))
        {
            db.Users.Add(new User
            {
                Id = MockIds.OwnerUser1,
                Username = "owner@medicare.local",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("P@ssw0rd!"),
                RoleId = ownerRoleId,
                CreatedAt = DateTime.UtcNow.AddDays(-100),
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            });
            db.UserProfiles.Add(new UserProfile
            {
                UserId = MockIds.OwnerUser1,
                FirstName = "Clinic",
                LastName = "Owner",
                Email = "owner@medicare.local",
                Phone = "+1-555-0000",
                CreatedAt = DateTime.UtcNow.AddDays(-100),
                UpdatedAt = DateTime.UtcNow
            });
            created++;
        }

        if (created > 0)
        {
            await db.SaveChangesAsync();
            Console.WriteLine($"[MockDataSeeder] Created {created} mock users with profiles.");
        }
        else
        {
            Console.WriteLine("[MockDataSeeder] All mock users already exist.");
        }
    }
}
