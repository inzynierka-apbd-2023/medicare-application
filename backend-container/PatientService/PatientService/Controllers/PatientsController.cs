using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PatientService.Data;
using PatientService.Models;

namespace PatientService.Controllers;

[ApiController]
[Route("api/patient/[controller]")]
public class PatientsController : ControllerBase
{
    private readonly PatientDbContext _db;
    public PatientsController(PatientDbContext db) => _db = db;

    // Register patient; PrimaryDoctorId is optional but recommended
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Register([FromBody] RegisterPatientRequest req)
    {
        if (req.UserId == Guid.Empty) return BadRequest("UserId is required");
        var userIdStr = req.UserId.ToString();
        if (await _db.Patients.AnyAsync(p => p.UserId == userIdStr)) return Conflict("Patient already exists for this user");
        var patient = new Patient
        {
            UserId = userIdStr,
            PrimaryDoctorId = req.PrimaryDoctorId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Patients.Add(patient);
        // initial status Active
        _db.PatientStatuses.Add(new PatientStatus
        {
            PatientId = patient.Id,
            Status = "Active",
            EffectiveAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
        // TODO: publish PatientRegistered event
        return CreatedAtAction(nameof(GetById), new { id = patient.Id }, new { patient.Id, patient.UserId });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var patient = await _db.Patients.FindAsync(id);
        if (patient == null) return NotFound();
        return Ok(patient);
    }

    [HttpPut("{id}/status")]
    [Authorize]
    public async Task<IActionResult> ChangeStatus(string id, [FromBody] ChangeStatusRequest req)
    {
        if (!await _db.Patients.AnyAsync(p => p.Id == id)) return NotFound("Patient not found");
        _db.PatientStatuses.Add(new PatientStatus
        {
            PatientId = id,
            Status = req.Status,
            EffectiveAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
        // TODO: publish PatientStatusChanged event
        return NoContent();
    }

    [HttpPut("{id}/emergency-contacts")]
    [Authorize]
    public async Task<IActionResult> SetEmergencyContacts(string id, [FromBody] List<EmergencyContactRequest> contacts)
    {
        if (!await _db.Patients.AnyAsync(p => p.Id == id)) return NotFound("Patient not found");
        var current = _db.EmergencyContacts.Where(c => c.PatientId == id);
        _db.EmergencyContacts.RemoveRange(current);
        _db.EmergencyContacts.AddRange(contacts.Select(c => new EmergencyContact
        {
            PatientId = id,
            Name = c.Name,
            Relation = c.Relation,
            Phone = c.Phone
        }));
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("{id}/insurance")]
    [Authorize]
    public async Task<IActionResult> UpdateInsurance(string id, [FromBody] InsuranceRequest req)
    {
        if (!await _db.Patients.AnyAsync(p => p.Id == id)) return NotFound("Patient not found");
        // replace existing insurance records (simple model)
        var existing = _db.Insurances.Where(i => i.PatientId == id);
        _db.Insurances.RemoveRange(existing);
        _db.Insurances.Add(new Insurance
        {
            PatientId = id,
            Provider = req.Provider,
            PolicyNumber = req.PolicyNumber,
            ValidFrom = req.ValidFrom,
            ValidTo = req.ValidTo
        });
        await _db.SaveChangesAsync();
        // TODO: publish InsuranceUpdated event
        return NoContent();
    }
}

public record RegisterPatientRequest(Guid UserId, string? PrimaryDoctorId);
public record ChangeStatusRequest(string Status);
public record EmergencyContactRequest(string Name, string? Relation, string? Phone);
public record InsuranceRequest(string? Provider, string? PolicyNumber, DateTime? ValidFrom, DateTime? ValidTo);
