using UserService.DTOs;

namespace UserService.Features.Auth.Commands;

/// <summary>
/// Response for RegisterUserCommand
/// </summary>
public record RegisterUserResponse
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public RefreshTokenResponseDto? TokenResponse { get; init; }
}

/// <summary>
/// Response for LoginUserCommand
/// </summary>
public record LoginUserResponse
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public bool UserInactive { get; init; }
    public RefreshTokenResponseDto? TokenResponse { get; init; }
}

/// <summary>
/// Response for RefreshTokenCommand
/// </summary>
public record RefreshTokenResponse
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public RefreshTokenResponseDto? TokenResponse { get; init; }
}

/// <summary>
/// Response for LogoutCommand
/// </summary>
public record LogoutResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
}

/// <summary>
/// Response for ForgotPasswordCommand
/// </summary>
public record ForgotPasswordResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
}

/// <summary>
/// Response for ResetPasswordCommand
/// </summary>
public record ResetPasswordResponse
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Response for ChangePasswordCommand
/// </summary>
public record ChangePasswordResponse
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Response for GenerateTestTokenCommand
/// </summary>
public record TestTokenResponse
{
    public string Token { get; init; } = string.Empty;
}
