using UserService.DTOs;
using UserService.Models;

namespace UserService.Services;

public interface IUserService
{
    Task<IEnumerable<UserResponseDto>> GetAllUsersAsync();
    Task<UserResponseDto?> GetUserByIdAsync(Guid id);
    Task<UserResponseDto?> GetUserByUsernameAsync(string username);
    Task<UserResponseDto> CreateUserAsync(CreateUserDto createUserDto);
    Task<UserResponseDto?> UpdateUserAsync(Guid id, UpdateUserDto updateUserDto);
    Task<bool> DeleteUserAsync(Guid id);
    Task<bool> UserExistsAsync(Guid id);
    Task<UserResponseDto?> AuthenticateAsync(string username, string password);
    Task<bool> EmailExistsAsync(string email, Guid? excludeUserId = null);
    Task<bool> UsernameExistsAsync(string username, Guid? excludeUserId = null);
}
