using Microsoft.AspNetCore.Mvc;
using UserService.DTOs;
using UserService.Services;
using UserService.Infrastructure.Messaging;
using UserService.Data;
using UserService.Models;

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
            var tokenResponse = _jwtService.GenerateTokenResponse(user);

            var response = new AuthResponseDto
            {
                Token = tokenResponse.Token,
                User = user
            };

            return Ok(response);
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
    public async Task<ActionResult<AuthResponseDto>> Register(CreateUserDto createUserDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userService.CreateUserAsync(createUserDto);
            // transactional outbox: store event in same DB
            var evt = new UserRegistered(user.Id, user.Username, user.Email, DateTime.UtcNow);
            _db.OutboxEvents.Add(new OutboxEvent
            {
                Type = "user.created",
                PayloadJson = System.Text.Json.JsonSerializer.Serialize(evt)
            });
            await _db.SaveChangesAsync();
            
            // Generate JWT token response for the new user
            var tokenResponse = _jwtService.GenerateTokenResponse(user);

            var response = new AuthResponseDto
            {
                Token = tokenResponse.Token,
                User = user
            };

            return CreatedAtAction("Login", response);
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
    public IActionResult RefreshToken()
    {
        return Ok(new { message = "Refresh token endpoint - not yet implemented" });
    }

    /// <summary>
    /// Logout (placeholder for future implementation with token blacklisting)
    /// </summary>
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        return Ok(new { message = "Logout successful" });
    }
}
