using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using UserService.DTOs;
using UserService.Features.Auth.Commands;

namespace UserService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// User login
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(RefreshTokenResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginDto loginDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _mediator.Send(new LoginUserCommand
        {
            Username = loginDto.Username,
            Password = loginDto.Password,
            ClientIp = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Request.Headers.UserAgent.ToString()
        });

        if (!result.Success)
        {
            return result.UserInactive 
                ? Unauthorized(new { message = result.ErrorMessage }) 
                : Unauthorized(new { message = result.ErrorMessage });
        }

        return Ok(result.TokenResponse);
    }

    /// <summary>
    /// User registration
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(RefreshTokenResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(CreateUserDto createUserDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _mediator.Send(new RegisterUserCommand
        {
            Username = createUserDto.Username,
            Email = createUserDto.Email,
            Password = createUserDto.Password,
            FirstName = createUserDto.FirstName,
            LastName = createUserDto.LastName,
            PhoneNumber = createUserDto.PhoneNumber,
            Role = createUserDto.Role,
            PlanId = createUserDto.PlanId,
            DateOfBirth = createUserDto.DateOfBirth,
            ClientIp = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Request.Headers.UserAgent.ToString()
        });

        if (!result.Success)
        {
            return Conflict(new { message = result.ErrorMessage });
        }

        return Created(string.Empty, result.TokenResponse);
    }

    /// <summary>
    /// Test JWT token (for development/testing purposes)
    /// </summary>
    [HttpPost("test-token")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> TestToken(TestTokenDto testTokenDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _mediator.Send(new GenerateTestTokenCommand
        {
            UserId = testTokenDto.UserId,
            Username = testTokenDto.Username,
            Role = testTokenDto.Role
        });

        return Ok(new { token = result.Token });
    }

    /// <summary>
    /// Refresh token
    /// </summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(RefreshTokenResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken(RefreshRequestDto req)
    {
        var result = await _mediator.Send(new RefreshTokenCommand
        {
            RefreshToken = req.RefreshToken,
            ClientIp = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Request.Headers.UserAgent.ToString()
        });

        if (!result.Success)
        {
            return Unauthorized(new { message = result.ErrorMessage });
        }

        return Ok(result.TokenResponse);
    }

    /// <summary>
    /// Logout (revoke refresh token)
    /// </summary>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout([FromBody] RefreshRequestDto? req)
    {
        string? refresh = req?.RefreshToken;
        if (string.IsNullOrWhiteSpace(refresh) && Request.Headers.TryGetValue("X-Refresh-Token", out var headerVal))
        {
            refresh = headerVal.FirstOrDefault();
        }

        var result = await _mediator.Send(new LogoutCommand
        {
            RefreshToken = refresh,
            ClientIp = HttpContext.Connection.RemoteIpAddress?.ToString()
        });

        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// Request password reset - sends reset email
    /// </summary>
    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        var result = await _mediator.Send(new ForgotPasswordCommand { Email = dto.Email });
        return Ok(new { message = result.Message });
    }

    /// <summary>
    /// Reset password using token
    /// </summary>
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        var result = await _mediator.Send(new ResetPasswordCommand
        {
            Token = dto.Token,
            NewPassword = dto.NewPassword
        });

        if (!result.Success)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(new { message = "Password has been reset successfully" });
    }

    /// <summary>
    /// Change password for authenticated user
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var result = await _mediator.Send(new ChangePasswordCommand
        {
            UserId = userId,
            CurrentPassword = dto.CurrentPassword,
            NewPassword = dto.NewPassword
        });

        if (!result.Success)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(new { message = "Password updated successfully" });
    }
}
