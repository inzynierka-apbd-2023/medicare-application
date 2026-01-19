using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AppointmentService.Features.DoctorPatients.DTOs;
using AppointmentService.Features.DoctorPatients.Queries;

namespace AppointmentService.Controllers;

[ApiController]
[Route("api/appointment/doctor-patients")]
[Authorize]
public class DoctorPatientsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DoctorPatientsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{doctorId:guid}")]
    [ProducesResponseType(typeof(DoctorPatientsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDoctorPatients(Guid doctorId, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetDoctorPatientsQuery(doctorId), cancellationToken);
        return Ok(response);
    }
}
