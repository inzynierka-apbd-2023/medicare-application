using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.DTOs;
using UserService.Models;

namespace UserService.Services;

public class UserServiceImpl : IUserService
{
    private readonly UserDbContext _context;

    public UserServiceImpl(UserDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync()
    {
        var users = await _context.Users
            .Include(u => u.Role)
            .Include(u => u.Profile)
            .Where(u => u.IsActive)
            .OrderBy(u => u.Username)
            .ToListAsync();

        return users.Select(MapToDto);
    }

    public async Task<UserResponseDto?> GetUserByIdAsync(string id)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Id == id && u.IsActive);

        return user != null ? MapToDto(user) : null;
    }

    public async Task<UserResponseDto?> GetUserByUsernameAsync(string username)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

        return user != null ? MapToDto(user) : null;
    }

    public async Task<UserResponseDto> CreateUserAsync(CreateUserDto createUserDto)
    {
        // Check if username or email already exists
        if (await UsernameExistsAsync(createUserDto.Username))
        {
            throw new InvalidOperationException("Username already exists");
        }

        if (await EmailExistsAsync(createUserDto.Email))
        {
            throw new InvalidOperationException("Email already exists");
        }

        // Get the role by name
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == createUserDto.Role);
        if (role == null)
        {
            throw new InvalidOperationException($"Role '{createUserDto.Role}' not found");
        }

        // Create user
        var userId = Guid.NewGuid().ToString();
        var user = new User
        {
            Id = userId,
            Username = createUserDto.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password),
            RoleId = role.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        // Create user profile
        var userProfile = new UserProfile
        {
            UserId = userId,
            FirstName = createUserDto.FirstName,
            LastName = createUserDto.LastName,
            Email = createUserDto.Email,
            Phone = createUserDto.PhoneNumber,
            DateOfBirth = createUserDto.DateOfBirth,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        _context.UserProfiles.Add(userProfile);
        await _context.SaveChangesAsync();

        // Reload with navigation properties
        user = await _context.Users
            .Include(u => u.Role)
            .Include(u => u.Profile)
            .FirstAsync(u => u.Id == userId);

        return MapToDto(user);
    }

    public async Task<UserResponseDto?> UpdateUserAsync(string id, UpdateUserDto updateUserDto)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null) return null;

        // Update user fields
        if (!string.IsNullOrEmpty(updateUserDto.Username))
        {
            if (await UsernameExistsAsync(updateUserDto.Username, id))
            {
                throw new InvalidOperationException("Username already exists");
            }
            user.Username = updateUserDto.Username;
        }

        if (!string.IsNullOrEmpty(updateUserDto.Role))
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == updateUserDto.Role);
            if (role == null)
            {
                throw new InvalidOperationException($"Role '{updateUserDto.Role}' not found");
            }
            user.RoleId = role.Id;
        }

        if (updateUserDto.IsActive.HasValue)
        {
            user.IsActive = updateUserDto.IsActive.Value;
        }

        user.UpdatedAt = DateTime.UtcNow;

        // Update user profile
        if (user.Profile != null)
        {
            if (!string.IsNullOrEmpty(updateUserDto.Email))
            {
                if (await EmailExistsAsync(updateUserDto.Email, id))
                {
                    throw new InvalidOperationException("Email already exists");
                }
                user.Profile.Email = updateUserDto.Email;
            }

            if (!string.IsNullOrEmpty(updateUserDto.FirstName))
                user.Profile.FirstName = updateUserDto.FirstName;

            if (!string.IsNullOrEmpty(updateUserDto.LastName))
                user.Profile.LastName = updateUserDto.LastName;

            if (!string.IsNullOrEmpty(updateUserDto.PhoneNumber))
                user.Profile.Phone = updateUserDto.PhoneNumber;

            if (updateUserDto.DateOfBirth.HasValue)
                user.Profile.DateOfBirth = updateUserDto.DateOfBirth;

            if (updateUserDto.AvatarUrl != null)
                user.Profile.AvatarUrl = string.IsNullOrWhiteSpace(updateUserDto.AvatarUrl) ? null : updateUserDto.AvatarUrl;

            // Address fields (partial updates allowed)
            if (!string.IsNullOrWhiteSpace(updateUserDto.AddressLine1))
                user.Profile.AddressLine1 = updateUserDto.AddressLine1;
            if (!string.IsNullOrWhiteSpace(updateUserDto.AddressLine2))
                user.Profile.AddressLine2 = updateUserDto.AddressLine2;
            if (!string.IsNullOrWhiteSpace(updateUserDto.City))
                user.Profile.City = updateUserDto.City;
            if (!string.IsNullOrWhiteSpace(updateUserDto.State))
                user.Profile.State = updateUserDto.State;
            if (!string.IsNullOrWhiteSpace(updateUserDto.ZipCode))
                user.Profile.ZipCode = updateUserDto.ZipCode;
            if (!string.IsNullOrWhiteSpace(updateUserDto.Country))
                user.Profile.Country = updateUserDto.Country;

            user.Profile.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return MapToDto(user);
    }

    public async Task<bool> DeleteUserAsync(string id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return false;

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UserExistsAsync(string id)
    {
        return await _context.Users.AnyAsync(u => u.Id == id && u.IsActive);
    }

    public async Task<UserResponseDto?> AuthenticateAsync(string username, string password)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

        if (user?.PasswordHash == null) return null;

        bool isValidPassword = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        if (!isValidPassword) return null;

        return MapToDto(user);
    }

    public async Task<bool> UsernameExistsAsync(string username, string? excludeUserId = null)
    {
        return await _context.Users.AnyAsync(u => 
            u.Username == username && 
            u.IsActive && 
            (excludeUserId == null || u.Id != excludeUserId));
    }

    public async Task<bool> EmailExistsAsync(string email, string? excludeUserId = null)
    {
        return await _context.UserProfiles.AnyAsync(up => 
            up.Email == email && 
            (excludeUserId == null || up.UserId != excludeUserId));
    }

    private static UserResponseDto MapToDto(User user)
    {
        return new UserResponseDto
        {
            Id = user.Id,
            Username = user.Username ?? "",
            Email = user.Profile?.Email ?? "",
            FirstName = user.Profile?.FirstName ?? "",
            LastName = user.Profile?.LastName ?? "",
            PhoneNumber = user.Profile?.Phone,
            Role = user.Role?.Name ?? "",
            DateOfBirth = user.Profile?.DateOfBirth,
            Address = user.Profile == null ? null : BuildAddress(user.Profile),
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            IsActive = user.IsActive
        };
    }

    private static string? BuildAddress(UserProfile profile)
    {
        var parts = new List<string?>
        {
            profile.AddressLine1,
            profile.AddressLine2,
            profile.City,
            profile.State,
            profile.ZipCode,
            profile.Country
        }
        .Where(p => !string.IsNullOrWhiteSpace(p))
        .ToList();

        return parts.Count == 0 ? null : string.Join(", ", parts);
    }
}
