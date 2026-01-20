using MassTransit;
using Medicare.Messaging.Contracts;
using UserService.DTOs;
using UserService.Services;

namespace UserService.Consumers;

public class UpdateUserConsumer : IConsumer<IUpdateUser>
{
    private readonly IUserService _userService;
    private readonly ILogger<UpdateUserConsumer> _logger;

    public UpdateUserConsumer(IUserService userService, ILogger<UpdateUserConsumer> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<IUpdateUser> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Updating user {UserId}", msg.UserId);

        try
        {
            var updateUserDto = new UpdateUserDto
            {
                Email = msg.Email,
                Username = msg.Email, // Keeping consistency with CreateUser
                FirstName = msg.FirstName,
                LastName = msg.LastName,
                PhoneNumber = msg.Phone,
                Role = null, // Role update usually separate or needs specific handling if passed
                DateOfBirth = msg.DateOfBirth,
                AddressLine1 = msg.AddressLine1,
                AddressLine2 = msg.AddressLine2,
                City = msg.City,
                State = msg.State,
                ZipCode = msg.ZipCode,
                Country = msg.Country,
                AvatarUrl = null,
                IsActive = true
            };

            var updatedUser = await _userService.UpdateUserAsync(msg.UserId, updateUserDto);

            if (updatedUser != null)
            {
                await context.RespondAsync<IUpdatedUserResponse>(new UpdatedUserResponseDto { Success = true });
            }
            else
            {
                await context.RespondAsync<IUpdatedUserResponse>(new UpdatedUserResponseDto 
                { 
                    Success = false, 
                    ErrorMessage = $"User with ID {msg.UserId} not found" 
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update user {UserId}", msg.UserId);
            var errorMessage = ex.Message;
            await context.RespondAsync<IUpdatedUserResponse>(new UpdatedUserResponseDto 
            { 
                Success = false, 
                ErrorMessage = errorMessage 
            });
        }
    }

    private record UpdatedUserResponseDto : IUpdatedUserResponse
    {
        public bool Success { get; init; }
        public string? ErrorMessage { get; init; }
    }
}
