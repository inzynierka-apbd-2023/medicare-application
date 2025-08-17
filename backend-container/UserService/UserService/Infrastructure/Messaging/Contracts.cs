namespace UserService.Infrastructure.Messaging;

public record UserRegistered(string UserId, string Username, string Email, DateTime OccurredAtUtc);
