using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.DTOs;
using UserService.Services;

namespace UserService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("availability")]
    [AllowAnonymous]
    public async Task<ActionResult<object>> CheckAvailability([FromQuery] string? email, [FromQuery] string? username)
    {
        var result = new Dictionary<string, object?>();
        if (!string.IsNullOrWhiteSpace(email))
        {
            var exists = await _userService.EmailExistsAsync(email);
            result["emailExists"] = exists;
        }
        if (!string.IsNullOrWhiteSpace(username))
        {
            var exists = await _userService.UsernameExistsAsync(username);
            result["usernameExists"] = exists;
        }
        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<UserResponseDto>> GetById(Guid id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null) return NotFound();
        return Ok(user);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<UserResponseDto>> Update(Guid id, [FromBody] UpdateUserDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var currentUserIdClaim = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value 
                                 ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (currentUserIdClaim == null || !Guid.TryParse(currentUserIdClaim, out var currentUserId))
        {
             return Unauthorized();
        }

        if (currentUserId != id && !User.IsInRole("Admin")) 
        {
            return Forbid();
        }

        var updated = await _userService.UpdateUserAsync(id, dto);
        if (updated == null) return NotFound();
        return Ok(updated);
    }
}
