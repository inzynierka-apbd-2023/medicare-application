using MassTransit;
using Medicare.Messaging.Contracts;
using UserService.Services;

namespace UserService.Consumers;

public class DeleteUserConsumer : IConsumer<IDeleteUser>
{
    private readonly IUserService _userService;
    private readonly ILogger<DeleteUserConsumer> _logger;

    public DeleteUserConsumer(IUserService userService, ILogger<DeleteUserConsumer> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<IDeleteUser> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Deleting user {UserId}", msg.UserId);

        try
        {
            var success = await _userService.DeleteUserAsync(msg.UserId);

            if (success)
            {
                await context.RespondAsync<IDeletedUserResponse>(new DeletedUserResponseDto { Success = true });
            }
            else
            {
                await context.RespondAsync<IDeletedUserResponse>(new DeletedUserResponseDto 
                { 
                    Success = false, 
                    ErrorMessage = $"User with ID {msg.UserId} not found" 
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete user {UserId}", msg.UserId);
            await context.RespondAsync<IDeletedUserResponse>(new DeletedUserResponseDto 
            { 
                Success = false, 
                ErrorMessage = ex.Message 
            });
        }
    }

    private record DeletedUserResponseDto : IDeletedUserResponse
    {
        public bool Success { get; init; }
        public string? ErrorMessage { get; init; }
    }
}
