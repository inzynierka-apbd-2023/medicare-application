using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientService.Features.Patients.Commands.DeletePatient;
using PatientService.Features.Patients.Commands.RegisterPatient;
using PatientService.Features.Patients.Commands.UpdatePatient;
using PatientService.Features.Patients.Queries.GetPatient;
using PatientService.Features.Patients.Queries.ListPatients;

namespace PatientService.Controllers;

[ApiController]
[Route("api/patient/[controller]")]
public class PatientsController : ControllerBase
{
    private readonly IMediator _mediator;
    public PatientsController(IMediator mediator) => _mediator = mediator;

    // Register patient; PrimaryDoctorId is optional but recommended
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Register([FromBody] RegisterPatientRequest req)
    {
        if (req.UserId == Guid.Empty) return BadRequest("UserId is required");
        
        var result = await _mediator.Send(new RegisterPatientCommand(req.UserId, req.PrimaryDoctorId));
        
        if (result == null) return Conflict("Patient already exists for this user"); 

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, new { result.Id, result.UserId });
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? q, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new ListPatientsQuery(q, page, pageSize));
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var patient = await _mediator.Send(new GetPatientQuery(id));
        if (patient == null) return NotFound();
        return Ok(patient);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin,receptionist")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _mediator.Send(new DeletePatientCommand(id));
        if (!success) return NotFound();
        return NoContent();
    }

    [HttpPut("{id}/status")]
    [Authorize]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeStatusRequest req)
    {
        var success = await _mediator.Send(new ChangePatientStatusCommand(id, req.Status));
        if (!success) return NotFound("Patient not found");
        return NoContent();
    }

    [HttpPut("{id}/emergency-contacts")]
    [Authorize]
    public async Task<IActionResult> SetEmergencyContacts(Guid id, [FromBody] List<EmergencyContactRequest> contacts)
    {
        // Map request DTO to Command DTO
        var commandContacts = contacts.Select(c => new EmergencyContactDto(c.Name, c.Relation, c.Phone)).ToList();
        var success = await _mediator.Send(new SetEmergencyContactsCommand(id, commandContacts));
        if (!success) return NotFound("Patient not found");
        return NoContent();
    }

    [HttpPut("{id}/insurance")]
    [Authorize]
    public async Task<IActionResult> UpdateInsurance(Guid id, [FromBody] InsuranceRequest req)
    {
        var success = await _mediator.Send(new UpdateInsuranceCommand(id, req.Provider, req.PolicyNumber, req.ValidFrom, req.ValidTo));
        if (!success) return NotFound("Patient not found");
        return NoContent();
    }
}

public record RegisterPatientRequest(Guid UserId, Guid? PrimaryDoctorId);
public record ChangeStatusRequest(string Status);
public record EmergencyContactRequest(string Name, string? Relation, string? Phone);
public record InsuranceRequest(string? Provider, string? PolicyNumber, DateTime? ValidFrom, DateTime? ValidTo);
