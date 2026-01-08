using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using UserService.Models;

namespace UserService.Data;

public static class MockDataSeeder
{
    // Shared IDs - must match across all services
    private static readonly Guid Doctor1Id = Guid.Parse("bbbbbbbb-0002-0002-0002-000000000001");
    private static readonly Guid Doctor2Id = Guid.Parse("bbbbbbbb-0002-0002-0002-000000000002");
    private static readonly Guid Patient1Id = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000001");
    private static readonly Guid Patient2Id = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000002");
    private static readonly Guid Patient3Id = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000003");
    private static readonly Guid Patient4Id = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000004");
    private static readonly Guid Patient5Id = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000005");
    private static readonly Guid Patient6Id = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000006");
    private static readonly Guid Patient7Id = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000007");
    private static readonly Guid ReceptionistId = Guid.Parse("cccccccc-0003-0003-0003-000000000001");
    private static readonly Guid OwnerId = Guid.Parse("ffffffff-0005-0005-0005-000000000001");
    private static readonly Guid AdminId = Guid.Parse("dddddddd-0004-0004-0004-000000000001");

    // Role IDs
    private static readonly Guid AdminRoleId = Guid.Parse("11111111-1111-1111-1111-000000000001");
    private static readonly Guid DoctorRoleId = Guid.Parse("11111111-1111-1111-1111-000000000002");
    private static readonly Guid PatientRoleId = Guid.Parse("11111111-1111-1111-1111-000000000003");
    private static readonly Guid ReceptionistRoleId = Guid.Parse("11111111-1111-1111-1111-000000000004");

    public static async Task SeedAsync(UserDbContext context)
    {
        Console.WriteLine("[UserService Seeder] Starting...");

        await SeedRolesAsync(context);
        await SeedUsersAsync(context);
        await context.SaveChangesAsync();

        Console.WriteLine("[UserService Seeder] Complete!");
    }

    private static async Task SeedRolesAsync(UserDbContext context)
    {
        var roles = new[]
        {
            new Role { Id = AdminRoleId, Name = "Admin", Description = "System administrator" },
            new Role { Id = DoctorRoleId, Name = "Doctor", Description = "Medical practitioner" },
            new Role { Id = PatientRoleId, Name = "Patient", Description = "Patient user" },
            new Role { Id = ReceptionistRoleId, Name = "Receptionist", Description = "Front desk staff" }
        };

        foreach (var role in roles)
        {
            if (!context.Roles.Any(r => r.Id == role.Id))
            {
                context.Roles.Add(role);
                Console.WriteLine($"[UserService Seeder] Added role: {role.Name}");
            }
        }
        await context.SaveChangesAsync();
    }

    private static async Task SeedUsersAsync(UserDbContext context)
    {
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("P@ssw0rd!");

        // Admin
        await CreateUserWithProfile(context, new User
        {
            Id = AdminId,
            Username = "admin",
            PasswordHash = passwordHash,
            RoleId = AdminRoleId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }, new UserProfile
        {
            UserId = AdminId,
            FirstName = "System",
            LastName = "Administrator",
            Email = "admin@medicare.local",
            DateOfBirth = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Gender = "Other",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        // Doctor 1 - Cardiologist
        await CreateUserWithProfile(context, new User
        {
            Id = Doctor1Id,
            Username = "doctor1",
            PasswordHash = passwordHash,
            RoleId = DoctorRoleId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }, new UserProfile
        {
            UserId = Doctor1Id,
            FirstName = "John",
            LastName = "Carter",
            Email = "doctor1@medicare.local",
            DateOfBirth = new DateTime(1975, 3, 15, 0, 0, 0, DateTimeKind.Utc),
            Gender = "Male",
            Phone = "555-0101",
            AddressLine1 = "100 Medical Plaza, Suite 200",
            City = "Chicago",
            State = "IL",
            ZipCode = "60601",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        // Doctor 2 - General Practitioner
        await CreateUserWithProfile(context, new User
        {
            Id = Doctor2Id,
            Username = "doctor2",
            PasswordHash = passwordHash,
            RoleId = DoctorRoleId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }, new UserProfile
        {
            UserId = Doctor2Id,
            FirstName = "Sarah",
            LastName = "Chen",
            Email = "doctor2@medicare.local",
            DateOfBirth = new DateTime(1982, 7, 22, 0, 0, 0, DateTimeKind.Utc),
            Gender = "Female",
            Phone = "555-0102",
            AddressLine1 = "100 Medical Plaza, Suite 300",
            City = "Chicago",
            State = "IL",
            ZipCode = "60601",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        // Patient 1
        await CreateUserWithProfile(context, new User
        {
            Id = Patient1Id,
            Username = "patient1",
            PasswordHash = passwordHash,
            RoleId = PatientRoleId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }, new UserProfile
        {
            UserId = Patient1Id,
            FirstName = "Alice",
            LastName = "Johnson",
            Email = "patient1@medicare.local",
            DateOfBirth = new DateTime(1990, 5, 10, 0, 0, 0, DateTimeKind.Utc),
            Gender = "Female",
            Phone = "555-0201",
            AddressLine1 = "456 Patient Ave",
            City = "Chicago",
            State = "IL",
            ZipCode = "60602",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        // Patient 2
        await CreateUserWithProfile(context, new User
        {
            Id = Patient2Id,
            Username = "patient2",
            PasswordHash = passwordHash,
            RoleId = PatientRoleId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }, new UserProfile
        {
            UserId = Patient2Id,
            FirstName = "Bob",
            LastName = "Smith",
            Email = "patient2@medicare.local",
            DateOfBirth = new DateTime(1985, 11, 25, 0, 0, 0, DateTimeKind.Utc),
            Gender = "Male",
            Phone = "555-0202",
            AddressLine1 = "789 Health St",
            City = "Chicago",
            State = "IL",
            ZipCode = "60603",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        // Patient 3
        await CreateUserWithProfile(context, new User
        {
            Id = Patient3Id,
            Username = "patient3",
            PasswordHash = passwordHash,
            RoleId = PatientRoleId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }, new UserProfile
        {
            UserId = Patient3Id,
            FirstName = "Carol",
            LastName = "Davis",
            Email = "patient3@medicare.local",
            DateOfBirth = new DateTime(1978, 3, 18, 0, 0, 0, DateTimeKind.Utc),
            Gender = "Female",
            Phone = "555-0203",
            AddressLine1 = "321 Wellness Blvd",
            City = "Chicago",
            State = "IL",
            ZipCode = "60604",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        // Patient 4
        await CreateUserWithProfile(context, new User
        {
            Id = Patient4Id,
            Username = "patient4",
            PasswordHash = passwordHash,
            RoleId = PatientRoleId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }, new UserProfile
        {
            UserId = Patient4Id,
            FirstName = "David",
            LastName = "Wilson",
            Email = "patient4@medicare.local",
            DateOfBirth = new DateTime(1992, 7, 4, 0, 0, 0, DateTimeKind.Utc),
            Gender = "Male",
            Phone = "555-0204",
            AddressLine1 = "555 Care Lane",
            City = "Chicago",
            State = "IL",
            ZipCode = "60605",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        // Patient 5
        await CreateUserWithProfile(context, new User
        {
            Id = Patient5Id,
            Username = "patient5",
            PasswordHash = passwordHash,
            RoleId = PatientRoleId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }, new UserProfile
        {
            UserId = Patient5Id,
            FirstName = "Emma",
            LastName = "Brown",
            Email = "patient5@medicare.local",
            DateOfBirth = new DateTime(1988, 12, 15, 0, 0, 0, DateTimeKind.Utc),
            Gender = "Female",
            Phone = "555-0205",
            AddressLine1 = "777 Medical Dr",
            City = "Chicago",
            State = "IL",
            ZipCode = "60606",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        // Patient 6
        await CreateUserWithProfile(context, new User
        {
            Id = Patient6Id,
            Username = "patient6",
            PasswordHash = passwordHash,
            RoleId = PatientRoleId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }, new UserProfile
        {
            UserId = Patient6Id,
            FirstName = "Frank",
            LastName = "Miller",
            Email = "patient6@medicare.local",
            DateOfBirth = new DateTime(1975, 9, 22, 0, 0, 0, DateTimeKind.Utc),
            Gender = "Male",
            Phone = "555-0206",
            AddressLine1 = "888 Health Pkwy",
            City = "Chicago",
            State = "IL",
            ZipCode = "60607",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        // Patient 7
        await CreateUserWithProfile(context, new User
        {
            Id = Patient7Id,
            Username = "patient7",
            PasswordHash = passwordHash,
            RoleId = PatientRoleId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }, new UserProfile
        {
            UserId = Patient7Id,
            FirstName = "Grace",
            LastName = "Taylor",
            Email = "patient7@medicare.local",
            DateOfBirth = new DateTime(1995, 2, 28, 0, 0, 0, DateTimeKind.Utc),
            Gender = "Female",
            Phone = "555-0207",
            AddressLine1 = "999 Clinic Rd",
            City = "Chicago",
            State = "IL",
            ZipCode = "60608",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        // Receptionist
        await CreateUserWithProfile(context, new User
        {
            Id = ReceptionistId,
            Username = "receptionist",
            PasswordHash = passwordHash,
            RoleId = ReceptionistRoleId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }, new UserProfile
        {
            UserId = ReceptionistId,
            FirstName = "Mary",
            LastName = "Williams",
            Email = "receptionist@medicare.local",
            DateOfBirth = new DateTime(1988, 9, 3, 0, 0, 0, DateTimeKind.Utc),
            Gender = "Female",
            Phone = "555-0301",
            AddressLine1 = "100 Medical Plaza",
            City = "Chicago",
            State = "IL",
            ZipCode = "60601",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        // Owner (using dynamic role lookup since seeded in Program.cs with random ID if not present)
        // Note: For consistency, we rely on the role name "Owner" being present.
        var ownerRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Owner");
        if (ownerRole != null)
        {
             await CreateUserWithProfile(context, new User
            {
                Id = OwnerId,
                Username = "owner",
                PasswordHash = passwordHash,
                RoleId = ownerRole.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }, new UserProfile
            {
                UserId = OwnerId,
                FirstName = "Big",
                LastName = "Boss",
                Email = "owner@medicare.local",
                DateOfBirth = new DateTime(1965, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Gender = "Male",
                Phone = "555-0000",
                AddressLine1 = "100 Medical Plaza, Penthouse",
                City = "Chicago",
                State = "IL",
                ZipCode = "60601",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }
    }

    private static Task CreateUserWithProfile(UserDbContext context, User user, UserProfile profile)
    {
        if (!context.Users.Any(u => u.Id == user.Id))
        {
            context.Users.Add(user);
            context.UserProfiles.Add(profile);
            Console.WriteLine($"[UserService Seeder] Added user: {user.Username} with ID {user.Id}");
        }
        return Task.CompletedTask;
    }
}
