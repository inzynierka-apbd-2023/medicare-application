using MassTransit;
using Medicare.Messaging.Contracts;
using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Models;

namespace UserService.Consumers;

public class GetUserConsumer : IConsumer<IGetUser>, IConsumer<IGetUsers>
{
    private readonly UserDbContext _context;
    private readonly ILogger<GetUserConsumer> _logger;

    public GetUserConsumer(UserDbContext context, ILogger<GetUserConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<IGetUser> context)
    {
        var msg = context.Message;
        var user = await _context.Users
            .AsNoTracking()
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Id == msg.UserId);

        if (user != null)
        {
            await context.RespondAsync<IUserResponse>(MapToUserResponse(user));
        }
        else
        {
            await context.RespondAsync<IUserResponse>(new UserResponseDto
            {
                Id = Guid.Empty,
                IsActive = false
            });
        }
    }

    public async Task Consume(ConsumeContext<IGetUsers> context)
    {
        var msg = context.Message;
        var users = await _context.Users
            .AsNoTracking()
            .Include(u => u.Profile)
            .Where(u => msg.UserIds.Contains(u.Id))
            .ToListAsync();

        var responseList = users.Select(MapToUserResponse).ToList<IUserResponse>();
        
        await context.RespondAsync<IUsersResponse>(new UsersResponseDto { Users = responseList });
    }

    private static UserResponseDto MapToUserResponse(User user)
    {
        return new UserResponseDto
        {
            Id = user.Id,
            FirstName = user.Profile?.FirstName ?? "",
            LastName = user.Profile?.LastName ?? "",
            Email = user.Profile?.Email ?? "",
            Phone = user.Profile?.Phone,
            DateOfBirth = user.Profile?.DateOfBirth,
            Gender = user.Profile?.Gender,
            AddressLine1 = user.Profile?.AddressLine1,
            AddressLine2 = user.Profile?.AddressLine2,
            City = user.Profile?.City,
            State = user.Profile?.State,
            ZipCode = user.Profile?.ZipCode,
            Country = user.Profile?.Country,
            IsActive = user.IsActive
        };
    }

    private record UserResponseDto : IUserResponse
    {
        public Guid Id { get; init; }
        public string FirstName { get; init; } = "";
        public string LastName { get; init; } = "";
        public string Email { get; init; } = "";
        public string? Phone { get; init; }
        public DateTime? DateOfBirth { get; init; }
        public string? Gender { get; init; }
        public string? AddressLine1 { get; init; }
        public string? AddressLine2 { get; init; }
        public string? City { get; init; }
        public string? State { get; init; }
        public string? ZipCode { get; init; }
        public string? Country { get; init; }
        public bool IsActive { get; init; }
    }

    private record UsersResponseDto : IUsersResponse
    {
        public List<IUserResponse> Users { get; init; } = new();
    }
}
