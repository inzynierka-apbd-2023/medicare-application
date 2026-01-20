using MassTransit;
using Medicare.Messaging.Contracts;
using UserService.DTOs;
using UserService.Services;

namespace UserService.Consumers;

public class CreateUserConsumer : IConsumer<ICreateUser>
{
    private readonly IUserService _userService;
    private readonly ILogger<CreateUserConsumer> _logger;

    public CreateUserConsumer(IUserService userService, ILogger<CreateUserConsumer> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ICreateUser> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Creating user {Email}", msg.Email);

        try
        {
            var createUserDto = new CreateUserDto
            {
                Email = msg.Email,
                Username = msg.Email,
                Password = msg.Password ?? "DefaultPassword123!",
                FirstName = msg.FirstName,
                LastName = msg.LastName,
                PhoneNumber = msg.Phone,
                Role = msg.Role,
                DateOfBirth = msg.DateOfBirth,
                AddressLine1 = msg.AddressLine1,
                AddressLine2 = msg.AddressLine2,
                City = msg.City,
                State = msg.State,
                ZipCode = msg.ZipCode,
                Country = msg.Country,
                AvatarUrl = null
            };

            var createdUser = await _userService.CreateUserAsync(createUserDto);

            await context.RespondAsync<ICreatedUserResponse>(new CreatedUserResponseDto
            {
                Id = createdUser.Id,
                Success = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create user {Email}", msg.Email);
            await context.RespondAsync<ICreatedUserResponse>(new CreatedUserResponseDto
            {
                Id = Guid.Empty,
                Success = false,
                ErrorMessage = ex.Message
            });
        }
    }

    private record CreatedUserResponseDto : ICreatedUserResponse
    {
        public Guid Id { get; init; }
        public bool Success { get; init; }
        public string? ErrorMessage { get; init; }
    }
}
