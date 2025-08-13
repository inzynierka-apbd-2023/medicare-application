using UserService.DTOs;
using UserService.Models;

namespace UserService.Services;

public interface IUserService
{
    Task<IEnumerable<UserResponseDto>> GetAllUsersAsync();
    Task<UserResponseDto?> GetUserByIdAsync(string id);
    Task<UserResponseDto?> GetUserByUsernameAsync(string username);
    Task<UserResponseDto> CreateUserAsync(CreateUserDto createUserDto);
    Task<UserResponseDto?> UpdateUserAsync(string id, UpdateUserDto updateUserDto);
    Task<bool> DeleteUserAsync(string id);
    Task<bool> UserExistsAsync(string id);
    Task<UserResponseDto?> AuthenticateAsync(string username, string password);
}
