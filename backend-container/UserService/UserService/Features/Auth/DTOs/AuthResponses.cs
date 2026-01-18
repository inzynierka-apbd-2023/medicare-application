using UserService.DTOs;

namespace UserService.Features.Auth.Commands;

public record RegisterUserResponse
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public RefreshTokenResponseDto? TokenResponse { get; init; }
}

public record LoginUserResponse
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public bool UserInactive { get; init; }
    public RefreshTokenResponseDto? TokenResponse { get; init; }
}

public record RefreshTokenResponse
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public RefreshTokenResponseDto? TokenResponse { get; init; }
}

public record LogoutResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
}

public record ForgotPasswordResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
}

public record ResetPasswordResponse
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
}

public record ChangePasswordResponse
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
}

public record TestTokenResponse
{
    public string Token { get; init; } = string.Empty;
}
