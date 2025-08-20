using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using AppointmentService.Features.DoctorSchedule.DTOs;
using AppointmentService.Features.DoctorSchedule.Queries;
using AppointmentService.Features.DoctorSchedule.Commands;
using AppointmentService.Features.DoctorSchedule.Validators;

namespace AppointmentService.Controllers;

[ApiController]
[Route("api/appointment/doctor-schedule")]
[Authorize]
public class DoctorScheduleController : ControllerBase
{
    private readonly IMediator _mediator;

    public DoctorScheduleController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{doctorId}")]
    public async Task<ActionResult<DoctorScheduleResponse>> GetDoctorSchedule(
        Guid doctorId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? status = null)
    {
        var query = new GetDoctorScheduleQuery
        {
            DoctorId = doctorId,
            StartDate = startDate,
            EndDate = endDate,
            Status = status
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{doctorId}/today")]
    public async Task<ActionResult<DoctorScheduleResponse>> GetTodaysAppointments(Guid doctorId)
    {
        var query = new GetTodaysAppointmentsQuery { DoctorId = doctorId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("appointment/{appointmentId}")]
    public async Task<ActionResult<DoctorScheduleEventDto>> GetAppointmentDetails(Guid appointmentId)
    {
        var query = new GetAppointmentDetailsQuery { AppointmentId = appointmentId };
        var result = await _mediator.Send(query);
        
        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpPut("appointment/{appointmentId}/status")]
    public async Task<ActionResult> UpdateAppointmentStatus(
        Guid appointmentId, 
        [FromBody] UpdateAppointmentStatusRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (!DoctorScheduleValidators.IsValidStatus(request.Status))
            return BadRequest("Invalid status. Must be one of: scheduled, completed, no-show, cancelled");

        var command = new UpdateAppointmentStatusCommand
        {
            AppointmentId = appointmentId,
            Status = request.Status,
            Notes = request.Notes
        };

        var result = await _mediator.Send(command);
        
        if (!result)
            return NotFound();

        return NoContent();
    }

    [HttpPut("appointment/{appointmentId}/notes")]
    public async Task<ActionResult> AddAppointmentNotes(
        Guid appointmentId, 
        [FromBody] AddAppointmentNotesRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var command = new AddAppointmentNotesCommand
        {
            AppointmentId = appointmentId,
            Notes = request.Notes
        };

        var result = await _mediator.Send(command);
        
        if (!result)
            return NotFound();

        return NoContent();
    }
}
