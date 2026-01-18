using MediatR;
using UserService.DTOs;

namespace UserService.Features.Auth.Commands;

public record RegisterUserCommand : IRequest<RegisterUserResponse>
{
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public string Role { get; init; } = "Patient";
    public string? PlanId { get; init; }
    public DateTime? DateOfBirth { get; init; }
    public string? ClientIp { get; init; }
    public string? UserAgent { get; init; }
}

public record LoginUserCommand : IRequest<LoginUserResponse>
{
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string? ClientIp { get; init; }
    public string? UserAgent { get; init; }
}

public record RefreshTokenCommand : IRequest<RefreshTokenResponse>
{
    public string RefreshToken { get; init; } = string.Empty;
    public string? ClientIp { get; init; }
    public string? UserAgent { get; init; }
}

public record LogoutCommand : IRequest<LogoutResponse>
{
    public string? RefreshToken { get; init; }
    public string? ClientIp { get; init; }
}

public record ForgotPasswordCommand : IRequest<ForgotPasswordResponse>
{
    public string Email { get; init; } = string.Empty;
}

public record ResetPasswordCommand : IRequest<ResetPasswordResponse>
{
    public string Token { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
}

public record ChangePasswordCommand : IRequest<ChangePasswordResponse>
{
    public Guid UserId { get; init; }
    public string CurrentPassword { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
}

public record GenerateTestTokenCommand : IRequest<TestTokenResponse>
{
    public Guid UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
}
