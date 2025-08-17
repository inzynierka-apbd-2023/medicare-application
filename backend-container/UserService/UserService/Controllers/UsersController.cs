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

    // GET: api/users/availability?email=...&username=... (no auth, used during sign-up)
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

    // GET: api/users/{id}
    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<UserResponseDto>> GetById(string id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null) return NotFound();
        return Ok(user);
    }

    // PUT: api/users/{id}
    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<UserResponseDto>> Update(string id, [FromBody] UpdateUserDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var updated = await _userService.UpdateUserAsync(id, dto);
            if (updated == null) return NotFound();
            return Ok(updated);
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
}
