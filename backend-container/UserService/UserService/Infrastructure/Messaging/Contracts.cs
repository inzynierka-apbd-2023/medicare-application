namespace UserService.Infrastructure.Messaging;

public record UserRegistered(Guid UserId, string Username, string Email, DateTime OccurredAtUtc, string? PlanId);
