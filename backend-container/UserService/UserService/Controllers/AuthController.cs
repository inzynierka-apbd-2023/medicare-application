using Microsoft.AspNetCore.Mvc;
using UserService.DTOs;
using UserService.Services;
using UserService.Infrastructure.Messaging;
using UserService.Data;
using UserService.Models;
using Microsoft.EntityFrameworkCore;

namespace UserService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IJwtService _jwtService;
    private readonly UserDbContext _db;

    public AuthController(IUserService userService, IJwtService jwtService, UserDbContext db)
    {
        _userService = userService;
        _jwtService = jwtService;
        _db = db;
    }

    /// <summary>
    /// User login
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto loginDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Authenticate user
            var user = await _userService.AuthenticateAsync(loginDto.Username, loginDto.Password);
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }

            // Check if user is active
            if (!user.IsActive)
            {
                return Unauthorized(new { message = "Account is deactivated" });
            }

            // Generate JWT token response
            // access + refresh
            var (accessToken, accessExpires) = _jwtService.GenerateAccessToken(user);
            var (refreshToken, refreshExpires, refreshHash) = _jwtService.GenerateRefreshToken();
            _db.RefreshTokens.Add(new RefreshToken
            {
                UserId = user.Id,
                TokenHash = refreshHash,
                ExpiresAt = refreshExpires,
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers.UserAgent.ToString()
            });
            await _db.SaveChangesAsync();

            return Ok(new RefreshTokenResponseDto
            {
                AccessToken = accessToken,
                AccessTokenExpiresAt = accessExpires,
                RefreshToken = refreshToken,
                RefreshTokenExpiresAt = refreshExpires,
                User = user
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>
    /// User registration
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(CreateUserDto createUserDto, [FromServices] RabbitMQ.Client.IConnection rabbitConnection)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userService.CreateUserAsync(createUserDto);
            // transactional outbox: store event in same DB
            var evt = new UserRegistered(user.Id, user.Username, user.Email, DateTime.UtcNow, createUserDto.PlanId);
            _db.OutboxEvents.Add(new OutboxEvent
            {
                Id = Guid.NewGuid(),
                Type = "user.created",
                PayloadJson = System.Text.Json.JsonSerializer.Serialize(evt)
            });
            await _db.SaveChangesAsync();

            // Send welcome email via RabbitMQ
            try
            {
                using var channel = rabbitConnection.CreateModel();
                channel.QueueDeclare(queue: "email.events", durable: true, exclusive: false, autoDelete: false, arguments: null);
                var emailEvent = new
                {
                    Type = "welcome",
                    Email = user.Email,
                    FirstName = user.FirstName
                };
                var body = System.Text.Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(emailEvent));
                channel.BasicPublish(exchange: "", routingKey: "email.events", mandatory: false, basicProperties: null, body: body);
            }
            catch (Exception emailEx)
            {
                // Log but don't fail registration if email fails
                Console.WriteLine($"[Register] Failed to queue welcome email: {emailEx.Message}");
            }
            
            // Generate JWT token response for the new user
            var (accessToken, accessExpires) = _jwtService.GenerateAccessToken(user);
            var (refreshToken, refreshExpires, refreshHash) = _jwtService.GenerateRefreshToken();
            _db.RefreshTokens.Add(new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = refreshHash,
                ExpiresAt = refreshExpires,
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers.UserAgent.ToString()
            });
            await _db.SaveChangesAsync();
            return Created(string.Empty, new RefreshTokenResponseDto
            {
                AccessToken = accessToken,
                AccessTokenExpiresAt = accessExpires,
                RefreshToken = refreshToken,
                RefreshTokenExpiresAt = refreshExpires,
                User = user
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>
    /// Test JWT token (for development/testing purposes)
    /// </summary>
    [HttpPost("test-token")]
    public IActionResult TestToken(TestTokenDto testTokenDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var token = _jwtService.GenerateToken(
                testTokenDto.UserId, 
                testTokenDto.Username, 
                testTokenDto.Role
            );

            return Ok(new { token = token });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>
    /// Refresh token (placeholder for future implementation)
    /// </summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<RefreshTokenResponseDto>> RefreshToken(RefreshRequestDto req)
    {
        if (string.IsNullOrWhiteSpace(req.RefreshToken)) return BadRequest(new { message = "Missing refresh token" });
        var hash = ComputeSha256(req.RefreshToken);
        var match = await _db.RefreshTokens
            .Include(r => r.User)!.ThenInclude(u => u.Profile)
            .Include(r => r.User)!.ThenInclude(u => u.Role)
            .FirstOrDefaultAsync(r => r.TokenHash == hash && r.RevokedAt == null && r.ExpiresAt >= DateTime.UtcNow);
        if (match == null) return Unauthorized(new { message = "Invalid refresh token" });
        if (match.User == null || !match.User.IsActive) return Unauthorized(new { message = "User inactive" });
        var user = await new UserServiceImpl(_db).GetUserByIdAsync(match.UserId);
        if (user == null) return Unauthorized(new { message = "User not found" });

        // rotate
        match.RevokedAt = DateTime.UtcNow;
        match.RevokedByIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        var (newAccess, accessExp) = _jwtService.GenerateAccessToken(user);
        var (newRefresh, refreshExp, refreshHash) = _jwtService.GenerateRefreshToken();
        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refreshHash,
            ExpiresAt = refreshExp,
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Request.Headers.UserAgent.ToString(),
            ReplacedByTokenHash = match.TokenHash
        });
        await _db.SaveChangesAsync();
        return Ok(new RefreshTokenResponseDto
        {
            AccessToken = newAccess,
            AccessTokenExpiresAt = accessExp,
            RefreshToken = newRefresh,
            RefreshTokenExpiresAt = refreshExp,
            User = user
        });
    }

    /// <summary>
    /// Logout (placeholder for future implementation with token blacklisting)
    /// </summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshRequestDto? req)
    {
        string? refresh = req?.RefreshToken;
        if (string.IsNullOrWhiteSpace(refresh) && Request.Headers.TryGetValue("X-Refresh-Token", out var headerVal))
        {
            refresh = headerVal.FirstOrDefault();
        }
        if (string.IsNullOrWhiteSpace(refresh)) return Ok(new { message = "No refresh token provided" });
        var hash = ComputeSha256(refresh);
        var token = await _db.RefreshTokens.FirstOrDefaultAsync(r => r.TokenHash == hash && r.RevokedAt == null);
        if (token != null)
        {
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedByIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            await _db.SaveChangesAsync();
        }
        return Ok(new { message = "Logout successful" });
    }

    /// <summary>
    /// Request password reset - sends reset email
    /// </summary>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto, [FromServices] RabbitMQ.Client.IConnection rabbitConnection)
    {
        try
        {
            // Find user by email
            var user = await _db.Users
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(u => u.Profile != null && u.Profile.Email == dto.Email);

            // Always return success to prevent email enumeration
            if (user == null)
            {
                return Ok(new { message = "If the email exists, a reset link has been sent." });
            }

            // Generate token
            var token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
            var tokenHash = ComputeSha256(token);

            // Store reset token (expires in 1 hour)
            _db.PasswordResetTokens.Add(new PasswordResetToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = tokenHash,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                CreatedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();

            // Perform QueueDeclare with correct signature
            using var channel = rabbitConnection.CreateModel();
            channel.QueueDeclare(queue: "email.events", durable: true, exclusive: false, autoDelete: false, arguments: null);
            var emailEvent = new
            {
                Type = "password_reset",
                Email = user.Profile!.Email,
                FirstName = user.Profile.FirstName,
                ResetToken = token
            };
            var body = System.Text.Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(emailEvent));
            channel.BasicPublish(exchange: "", routingKey: "email.events", mandatory: false, basicProperties: null, body: body);

            return Ok(new { message = "If the email exists, a reset link has been sent." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error processing request", error = ex.Message });
        }
    }

    /// <summary>
    /// Reset password using token
    /// </summary>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        try
        {
            var tokenHash = ComputeSha256(dto.Token);
            var resetToken = await _db.PasswordResetTokens
                .Include(t => t.User)
                .ThenInclude(u => u!.Profile)
                .FirstOrDefaultAsync(t => t.TokenHash == tokenHash && t.UsedAt == null && t.ExpiresAt > DateTime.UtcNow);

            if (resetToken == null)
            {
                return BadRequest(new { message = "Invalid or expired reset token" });
            }

            // Update password
            var user = resetToken.User!;
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            // Mark token as used
            resetToken.UsedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return Ok(new { message = "Password has been reset successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error resetting password", error = ex.Message });
        }
    }

    /// <summary>
    /// Change password for authenticated user
    /// </summary>
    [HttpPost("change-password")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var user = await _db.Users.FindAsync(userId);
            if (user == null) return Unauthorized();

            // Verify current password
            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
            {
                return BadRequest(new { message = "Incorrect current password" });
            }

            // Update with new password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            // Revoke all refresh tokens for this user?
            // Optional: for better security, revoke to force re-login on other devices
            // but might annoy user. For now, let's keep them logged in or just revoke specific token?
            // Let's keep simpler: just update password.

            await _db.SaveChangesAsync();

            return Ok(new { message = "Password updated successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error changing password", error = ex.Message });
        }
    }

    private static string ComputeSha256(string input)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }
}
