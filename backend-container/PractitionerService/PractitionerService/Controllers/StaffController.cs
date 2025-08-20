using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PractitionerService.Features.StaffManagement.Commands;
using PractitionerService.Features.StaffManagement.DTOs;
using PractitionerService.Features.StaffManagement.Queries;

namespace PractitionerService.Controllers;

[ApiController]
[Route("api/practitioner/staff")]
public class StaffController : ControllerBase
{
    private readonly IMediator _mediator;

    public StaffController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all staff members with optional filtering
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetStaff([FromQuery] StaffSearchRequest searchRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var query = new GetAllStaffQuery { SearchRequest = searchRequest };
        var result = await _mediator.Send(query);

        if (!result.Success)
        {
            return StatusCode(501, result); // Not Implemented
        }

        return Ok(result);
    }

    /// <summary>
    /// Get staff member by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetStaffById(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest("Staff ID is required");
        }

        var query = new GetStaffByIdQuery { Id = id };
        var result = await _mediator.Send(query);

        if (!result.Success)
        {
            return StatusCode(501, result); // Not Implemented
        }

        return Ok(result);
    }

    /// <summary>
    /// Get staff members by role (Doctor or Receptionist)
    /// </summary>
    [HttpGet("role/{role}")]
    public async Task<IActionResult> GetStaffByRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            return BadRequest("Role is required");
        }

        if (role != "Doctor" && role != "Receptionist")
        {
            return BadRequest("Role must be 'Doctor' or 'Receptionist'");
        }

        var query = new GetStaffByRoleQuery { Role = role };
        var result = await _mediator.Send(query);

        if (!result.Success)
        {
            return StatusCode(501, result); // Not Implemented
        }

        return Ok(result);
    }

    /// <summary>
    /// Create a new staff member
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateStaff([FromBody] CreateStaffRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Additional validation
        if (request.Role != "Doctor" && request.Role != "Receptionist")
        {
            return BadRequest("Role must be 'Doctor' or 'Receptionist'");
        }

        if (request.Role == "Doctor")
        {
            if (string.IsNullOrWhiteSpace(request.LicenseNumber))
            {
                return BadRequest("License number is required for doctors");
            }
            if (request.YearsExperience == null || request.YearsExperience < 0)
            {
                return BadRequest("Years of experience is required for doctors and must be non-negative");
            }
        }

        if (request.Role == "Receptionist")
        {
            if (string.IsNullOrWhiteSpace(request.Department))
            {
                return BadRequest("Department is required for receptionists");
            }
        }

        var command = new CreateStaffCommand { Request = request };
        var result = await _mediator.Send(command);

        if (!result.Success)
        {
            return StatusCode(501, result); // Not Implemented
        }

        return CreatedAtAction(nameof(GetStaffById), new { id = result.Data?.Id }, result);
    }

    /// <summary>
    /// Update an existing staff member
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateStaff(string id, [FromBody] UpdateStaffRequest request)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest("Staff ID is required");
        }

        if (request.Id != id)
        {
            return BadRequest("ID in URL and request body must match");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Additional validation
        if (request.Role != "Doctor" && request.Role != "Receptionist")
        {
            return BadRequest("Role must be 'Doctor' or 'Receptionist'");
        }

        var command = new UpdateStaffCommand { Request = request };
        var result = await _mediator.Send(command);

        if (!result.Success)
        {
            return StatusCode(501, result); // Not Implemented
        }

        return Ok(result);
    }

    /// <summary>
    /// Delete (deactivate) a staff member
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteStaff(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest("Staff ID is required");
        }

        var command = new DeleteStaffCommand { Id = id };
        var result = await _mediator.Send(command);

        if (!result.Success)
        {
            return StatusCode(501, result); // Not Implemented
        }

        return Ok(result);
    }

    /// <summary>
    /// Get available specializations
    /// </summary>
    [HttpGet("specializations")]
    public async Task<IActionResult> GetSpecializations()
    {
        var query = new GetSpecializationsQuery();
        var result = await _mediator.Send(query);

        if (!result.Success)
        {
            return StatusCode(501, result); // Not Implemented
        }

        return Ok(result);
    }

    /// <summary>
    /// Get available services
    /// </summary>
    [HttpGet("services")]
    public async Task<IActionResult> GetServices()
    {
        var query = new GetServicesQuery();
        var result = await _mediator.Send(query);

        if (!result.Success)
        {
            return StatusCode(501, result); // Not Implemented
        }

        return Ok(result);
    }
}
