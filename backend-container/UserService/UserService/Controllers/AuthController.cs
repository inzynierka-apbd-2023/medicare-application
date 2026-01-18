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
    private readonly IConfiguration _configuration;

    public AuthController(IMediator mediator, IConfiguration configuration)
    {
        _mediator = mediator;
        _configuration = configuration;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
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

        SetTokenCookies(result.TokenResponse.AccessToken, result.TokenResponse.RefreshToken);
        return Ok(result.TokenResponse.User);
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status201Created)]
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

        SetTokenCookies(result.TokenResponse.AccessToken, result.TokenResponse.RefreshToken);
        return Created(string.Empty, result.TokenResponse.User);
    }

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

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken(RefreshRequestDto req)
    {
        string? refreshToken = req.RefreshToken;

        if (string.IsNullOrWhiteSpace(refreshToken) && Request.Cookies.TryGetValue("refreshToken", out var cookieToken))       
        {
            refreshToken = cookieToken;
        }

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
             return Unauthorized(new { message = "No refresh token provided" });
        }

        var result = await _mediator.Send(new RefreshTokenCommand
        {
            RefreshToken = refreshToken,
            ClientIp = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Request.Headers.UserAgent.ToString()
        });

        if (!result.Success)
        {
            return Unauthorized(new { message = result.ErrorMessage });
        }

        SetTokenCookies(result.TokenResponse.AccessToken, result.TokenResponse.RefreshToken);
        return Ok(result.TokenResponse.User);
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout([FromBody] RefreshRequestDto? req)
    {
        string? refresh = req?.RefreshToken;
        
        if (string.IsNullOrWhiteSpace(refresh) && Request.Cookies.TryGetValue("refreshToken", out var cookieToken))
        {
            refresh = cookieToken;
        }

        if (string.IsNullOrWhiteSpace(refresh) && Request.Headers.TryGetValue("X-Refresh-Token", out var headerVal))
        {
            refresh = headerVal.FirstOrDefault();
        }

        if (!string.IsNullOrWhiteSpace(refresh))
        {
            await _mediator.Send(new LogoutCommand
            {
                RefreshToken = refresh,
                ClientIp = HttpContext.Connection.RemoteIpAddress?.ToString()
            });
        }

        Response.Cookies.Delete("accessToken");
        Response.Cookies.Delete("refreshToken");

        return Ok(new { message = "Logged out successfully" });
    }

    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        var result = await _mediator.Send(new ForgotPasswordCommand { Email = dto.Email });
        return Ok(new { message = result.Message });
    }

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

    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userIdClaim = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value 
                          ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                          
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

    private void SetTokenCookies(string accessToken, string refreshToken)
    {
        var jwtExpiryMinutes = int.Parse(_configuration.GetSection("Jwt")["ExpiryInMinutes"] ?? "15");
        var refreshExpiryDays = int.Parse(_configuration.GetSection("RefreshToken")["ExpiryInDays"] ?? "7");
        var refreshCookiePath = _configuration.GetSection("RefreshToken")["CookiePath"] ?? "/api/auth/refresh";

        var cookieSection = _configuration.GetSection("Cookie");
        var httpOnly = bool.Parse(cookieSection["HttpOnly"] ?? "true");
        var secure = bool.Parse(cookieSection["Secure"] ?? "true");
        var sameSiteStr = cookieSection["SameSite"] ?? "None";
        var sameSite = sameSiteStr switch
        {
            "Strict" => SameSiteMode.Strict,
            "Lax" => SameSiteMode.Lax,
            "None" => SameSiteMode.None,
            _ => SameSiteMode.None
        };

        var accessOptions = new CookieOptions
        {
            HttpOnly = httpOnly,
            Secure = secure,
            SameSite = sameSite,
            Expires = DateTime.UtcNow.AddMinutes(jwtExpiryMinutes)
        };

        var refreshOptions = new CookieOptions
        {
            HttpOnly = httpOnly,
            Secure = secure,
            SameSite = sameSite,
            Expires = DateTime.UtcNow.AddDays(refreshExpiryDays),
            Path = refreshCookiePath
        };

        Response.Cookies.Append("accessToken", accessToken, accessOptions);
        Response.Cookies.Append("refreshToken", refreshToken, refreshOptions);
    }
}
