using MediatR;
using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.DTOs;
using UserService.Features.Auth.Commands;
using UserService.Infrastructure.Messaging;
using UserService.Models;
using UserService.Services;

namespace UserService.Features.Auth.Handlers;

/// <summary>
/// Handler for RegisterUserCommand
/// </summary>
public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, RegisterUserResponse>
{
    private readonly IUserService _userService;
    private readonly IJwtService _jwtService;
    private readonly UserDbContext _db;
    private readonly ILogger<RegisterUserHandler> _logger;
    private readonly RabbitMQ.Client.IConnection _rabbitConnection;

    public RegisterUserHandler(
        IUserService userService,
        IJwtService jwtService,
        UserDbContext db,
        ILogger<RegisterUserHandler> logger,
        RabbitMQ.Client.IConnection rabbitConnection)
    {
        _userService = userService;
        _jwtService = jwtService;
        _db = db;
        _logger = logger;
        _rabbitConnection = rabbitConnection;
    }

    public async Task<RegisterUserResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Registering user with PlanId: {PlanId}", request.PlanId ?? "(null)");

            var createUserDto = new CreateUserDto
            {
                Username = request.Username,
                Email = request.Email,
                Password = request.Password,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                Role = request.Role,
                PlanId = request.PlanId,
                DateOfBirth = request.DateOfBirth
            };

            var user = await _userService.CreateUserAsync(createUserDto);
            _logger.LogInformation("User created: {UserId}", user.Id);

            // Store outbox event
            var evt = new UserRegistered(user.Id, user.Username, user.Email, DateTime.UtcNow, request.PlanId);
            _db.OutboxEvents.Add(new OutboxEvent
            {
                Id = Guid.NewGuid(),
                Type = "user.created",
                PayloadJson = System.Text.Json.JsonSerializer.Serialize(evt)
            });
            await _db.SaveChangesAsync(cancellationToken);

            // Send welcome email
            try
            {
                await using var channel = await _rabbitConnection.CreateChannelAsync(cancellationToken: cancellationToken);
                await channel.QueueDeclareAsync(queue: "email.events", durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: cancellationToken);
                var emailEvent = new { Type = "welcome", Email = user.Email, FirstName = user.FirstName };
                var body = System.Text.Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(emailEvent));
                var props = new RabbitMQ.Client.BasicProperties();
                await channel.BasicPublishAsync(exchange: "", routingKey: "email.events", mandatory: false, basicProperties: props, body: body, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to queue welcome email");
            }

            // Generate tokens
            var (accessToken, accessExpires) = _jwtService.GenerateAccessToken(user);
            var (refreshToken, refreshExpires, refreshHash) = _jwtService.GenerateRefreshToken();
            _db.RefreshTokens.Add(new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = refreshHash,
                ExpiresAt = refreshExpires,
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = request.ClientIp,
                UserAgent = request.UserAgent
            });
            await _db.SaveChangesAsync(cancellationToken);

            return new RegisterUserResponse
            {
                Success = true,
                TokenResponse = new RefreshTokenResponseDto
                {
                    AccessToken = accessToken,
                    AccessTokenExpiresAt = accessExpires,
                    RefreshToken = refreshToken,
                    RefreshTokenExpiresAt = refreshExpires,
                    User = user
                }
            };
        }
        catch (InvalidOperationException ex)
        {
            return new RegisterUserResponse { Success = false, ErrorMessage = ex.Message };
        }
    }
}

/// <summary>
/// Handler for LoginUserCommand
/// </summary>
public class LoginUserHandler : IRequestHandler<LoginUserCommand, LoginUserResponse>
{
    private readonly IUserService _userService;
    private readonly IJwtService _jwtService;
    private readonly UserDbContext _db;
    private readonly ILogger<LoginUserHandler> _logger;

    public LoginUserHandler(IUserService userService, IJwtService jwtService, UserDbContext db, ILogger<LoginUserHandler> logger)
    {
        _userService = userService;
        _jwtService = jwtService;
        _db = db;
        _logger = logger;
    }

    public async Task<LoginUserResponse> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userService.AuthenticateAsync(request.Username, request.Password);
        if (user == null)
        {
            return new LoginUserResponse { Success = false, ErrorMessage = "Invalid username or password" };
        }

        if (!user.IsActive)
        {
            return new LoginUserResponse { Success = false, UserInactive = true, ErrorMessage = "Account is deactivated" };
        }

        var (accessToken, accessExpires) = _jwtService.GenerateAccessToken(user);
        var (refreshToken, refreshExpires, refreshHash) = _jwtService.GenerateRefreshToken();
        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refreshHash,
            ExpiresAt = refreshExpires,
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = request.ClientIp,
            UserAgent = request.UserAgent
        });
        await _db.SaveChangesAsync(cancellationToken);

        return new LoginUserResponse
        {
            Success = true,
            TokenResponse = new RefreshTokenResponseDto
            {
                AccessToken = accessToken,
                AccessTokenExpiresAt = accessExpires,
                RefreshToken = refreshToken,
                RefreshTokenExpiresAt = refreshExpires,
                User = user
            }
        };
    }
}

/// <summary>
/// Handler for RefreshTokenCommand
/// </summary>
public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenResponse>
{
    private readonly UserDbContext _db;
    private readonly IJwtService _jwtService;
    private readonly IUserService _userService;

    public RefreshTokenHandler(UserDbContext db, IJwtService jwtService, IUserService userService)
    {
        _db = db;
        _jwtService = jwtService;
        _userService = userService;
    }

    public async Task<RefreshTokenResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return new RefreshTokenResponse { Success = false, ErrorMessage = "Missing refresh token" };

        var hash = ComputeSha256(request.RefreshToken);
        var match = await _db.RefreshTokens
            .Include(r => r.User)!.ThenInclude(u => u.Profile)
            .Include(r => r.User)!.ThenInclude(u => u.Role)
            .FirstOrDefaultAsync(r => r.TokenHash == hash && r.RevokedAt == null && r.ExpiresAt >= DateTime.UtcNow, cancellationToken);

        if (match == null)
            return new RefreshTokenResponse { Success = false, ErrorMessage = "Invalid refresh token" };
        if (match.User == null || !match.User.IsActive)
            return new RefreshTokenResponse { Success = false, ErrorMessage = "User inactive" };

        var user = await _userService.GetUserByIdAsync(match.UserId);
        if (user == null)
            return new RefreshTokenResponse { Success = false, ErrorMessage = "User not found" };

        // Rotate tokens
        match.RevokedAt = DateTime.UtcNow;
        match.RevokedByIp = request.ClientIp;
        var (newAccess, accessExp) = _jwtService.GenerateAccessToken(user);
        var (newRefresh, refreshExp, refreshHash) = _jwtService.GenerateRefreshToken();
        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refreshHash,
            ExpiresAt = refreshExp,
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = request.ClientIp,
            UserAgent = request.UserAgent,
            ReplacedByTokenHash = match.TokenHash
        });
        await _db.SaveChangesAsync(cancellationToken);

        return new RefreshTokenResponse
        {
            Success = true,
            TokenResponse = new RefreshTokenResponseDto
            {
                AccessToken = newAccess,
                AccessTokenExpiresAt = accessExp,
                RefreshToken = newRefresh,
                RefreshTokenExpiresAt = refreshExp,
                User = user
            }
        };
    }

    private static string ComputeSha256(string input)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }
}

/// <summary>
/// Handler for LogoutCommand
/// </summary>
public class LogoutHandler : IRequestHandler<LogoutCommand, LogoutResponse>
{
    private readonly UserDbContext _db;

    public LogoutHandler(UserDbContext db)
    {
        _db = db;
    }

    public async Task<LogoutResponse> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return new LogoutResponse { Success = true, Message = "No refresh token provided" };

        var hash = ComputeSha256(request.RefreshToken);
        var token = await _db.RefreshTokens.FirstOrDefaultAsync(r => r.TokenHash == hash && r.RevokedAt == null, cancellationToken);
        if (token != null)
        {
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedByIp = request.ClientIp;
            await _db.SaveChangesAsync(cancellationToken);
        }
        return new LogoutResponse { Success = true, Message = "Logout successful" };
    }

    private static string ComputeSha256(string input)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }
}

/// <summary>
/// Handler for ForgotPasswordCommand
/// </summary>
public class ForgotPasswordHandler : IRequestHandler<ForgotPasswordCommand, ForgotPasswordResponse>
{
    private readonly UserDbContext _db;
    private readonly RabbitMQ.Client.IConnection _rabbitConnection;
    private readonly ILogger<ForgotPasswordHandler> _logger;

    public ForgotPasswordHandler(UserDbContext db, RabbitMQ.Client.IConnection rabbitConnection, ILogger<ForgotPasswordHandler> logger)
    {
        _db = db;
        _rabbitConnection = rabbitConnection;
        _logger = logger;
    }

    public async Task<ForgotPasswordResponse> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _db.Users
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Profile != null && u.Profile.Email == request.Email, cancellationToken);

        // Always return success to prevent email enumeration
        if (user == null)
            return new ForgotPasswordResponse { Success = true, Message = "If the email exists, a reset link has been sent." };

        var token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        var tokenHash = ComputeSha256(token);

        _db.PasswordResetTokens.Add(new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync(cancellationToken);

        // Send email
        try
        {
            await using var channel = await _rabbitConnection.CreateChannelAsync(cancellationToken: cancellationToken);
            await channel.QueueDeclareAsync(queue: "email.events", durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: cancellationToken);
            var emailEvent = new
            {
                Type = "password_reset",
                Email = user.Profile!.Email,
                FirstName = user.Profile.FirstName,
                ResetToken = token
            };
            var body = System.Text.Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(emailEvent));
            var props = new RabbitMQ.Client.BasicProperties();
            await channel.BasicPublishAsync(exchange: "", routingKey: "email.events", mandatory: false, basicProperties: props, body: body, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to queue password reset email");
        }

        return new ForgotPasswordResponse { Success = true, Message = "If the email exists, a reset link has been sent." };
    }

    private static string ComputeSha256(string input)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }
}

/// <summary>
/// Handler for ResetPasswordCommand
/// </summary>
public class ResetPasswordHandler : IRequestHandler<ResetPasswordCommand, ResetPasswordResponse>
{
    private readonly UserDbContext _db;
    private readonly ILogger<ResetPasswordHandler> _logger;

    public ResetPasswordHandler(UserDbContext db, ILogger<ResetPasswordHandler> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<ResetPasswordResponse> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = ComputeSha256(request.Token);
        var resetToken = await _db.PasswordResetTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash && t.UsedAt == null && t.ExpiresAt > DateTime.UtcNow, cancellationToken);

        if (resetToken == null)
            return new ResetPasswordResponse { Success = false, ErrorMessage = "Invalid or expired reset token" };

        var user = resetToken.User!;
        
        // Check if new password is same as current password
        if (BCrypt.Net.BCrypt.Verify(request.NewPassword, user.PasswordHash))
            return new ResetPasswordResponse { Success = false, ErrorMessage = "New password cannot be the same as your current password" };
        
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        resetToken.UsedAt = DateTime.UtcNow;

        // Invalidate all existing refresh tokens (force re-login on all devices)
        var activeTokens = await _db.RefreshTokens
            .Where(t => t.UserId == user.Id && t.RevokedAt == null)
            .ToListAsync(cancellationToken);
        
        foreach (var token in activeTokens)
        {
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedByIp = "password-reset";
        }
        
        _logger.LogInformation("Password reset for user {UserId}. Invalidated {TokenCount} refresh tokens.", user.Id, activeTokens.Count);

        await _db.SaveChangesAsync(cancellationToken);
        return new ResetPasswordResponse { Success = true };
    }

    private static string ComputeSha256(string input)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }
}

/// <summary>
/// Handler for ChangePasswordCommand
/// </summary>
public class ChangePasswordHandler : IRequestHandler<ChangePasswordCommand, ChangePasswordResponse>
{
    private readonly UserDbContext _db;

    public ChangePasswordHandler(UserDbContext db)
    {
        _db = db;
    }

    public async Task<ChangePasswordResponse> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _db.Users.FindAsync(new object[] { request.UserId }, cancellationToken);
        if (user == null)
            return new ChangePasswordResponse { Success = false, ErrorMessage = "User not found" };

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            return new ChangePasswordResponse { Success = false, ErrorMessage = "Incorrect current password" };

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        return new ChangePasswordResponse { Success = true };
    }
}

/// <summary>
/// Handler for GenerateTestTokenCommand
/// </summary>
public class GenerateTestTokenHandler : IRequestHandler<GenerateTestTokenCommand, TestTokenResponse>
{
    private readonly IJwtService _jwtService;

    public GenerateTestTokenHandler(IJwtService jwtService)
    {
        _jwtService = jwtService;
    }

    public Task<TestTokenResponse> Handle(GenerateTestTokenCommand request, CancellationToken cancellationToken)
    {
        var token = _jwtService.GenerateToken(request.UserId, request.Username, request.Role);
        return Task.FromResult(new TestTokenResponse { Token = token });
    }
}
