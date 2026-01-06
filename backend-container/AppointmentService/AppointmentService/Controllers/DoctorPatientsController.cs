using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AppointmentService.Features.DoctorPatients.DTOs;
using AppointmentService.Features.DoctorPatients.Queries;

namespace AppointmentService.Controllers;

/// <summary>
/// Controller for doctor's patient list.
/// Returns all patients that have had appointments with the doctor.
/// </summary>
[ApiController]
[Route("api/appointment/doctor-patients")]
[Authorize]
public class DoctorPatientsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<DoctorPatientsController> _logger;

    public DoctorPatientsController(IMediator mediator, ILogger<DoctorPatientsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all patients for a specific doctor based on their appointments.
    /// </summary>
    [HttpGet("{doctorId:guid}")]
    [ProducesResponseType(typeof(DoctorPatientsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDoctorPatients(Guid doctorId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching patients for doctor {DoctorId}", doctorId);

        var response = await _mediator.Send(new GetDoctorPatientsQuery(doctorId), cancellationToken);

        _logger.LogInformation("Found {Count} patients for doctor {DoctorId}", response.TotalCount, doctorId);

        return Ok(response);
    }
}
