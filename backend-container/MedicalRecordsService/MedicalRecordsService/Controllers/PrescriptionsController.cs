using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MedicalRecordsService.Data;
using MedicalRecordsService.Models;

namespace MedicalRecordsService.Controllers;

[ApiController]
[Route("api/medical-records/prescriptions")]
public class PrescriptionsController : ControllerBase
{
    private readonly MedicalRecordsDbContext _db;
    public PrescriptionsController(MedicalRecordsDbContext db) => _db = db;

    /// <summary>
    /// Get all prescriptions with optional filters
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? patientId,
        [FromQuery] Guid? doctorId,
        [FromQuery] string? status,
        [FromQuery] string? search)
    {
        var query = _db.Prescriptions.AsNoTracking().AsQueryable();

        if (patientId.HasValue)
            query = query.Where(p => p.PatientId == patientId.Value);

        if (doctorId.HasValue)
            query = query.Where(p => p.DoctorId == doctorId.Value);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(p => p.Status == status);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(p =>
                p.MedicationName.ToLower().Contains(s) ||
                (p.Instructions != null && p.Instructions.ToLower().Contains(s)));
        }

        var prescriptions = await query
            .OrderByDescending(p => p.PrescribedDate)
            .ToListAsync();

        return Ok(prescriptions);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePrescription([FromBody] CreatePrescriptionRequest req)
    {
        // Use provided MedicalRecordId or generate a placeholder if empty/null
        var medicalRecordId = req.MedicalRecordId != Guid.Empty 
            ? req.MedicalRecordId 
            : Guid.NewGuid();
            
        var prescription = new Prescription
        {
            MedicalRecordId = medicalRecordId,
            PatientId = req.PatientId,
            DoctorId = req.DoctorId != Guid.Empty ? req.DoctorId : Guid.NewGuid(),
            MedicationName = req.MedicationName,
            AtcCode = req.AtcCode,
            Dosage = req.Dosage,
            Frequency = req.Frequency,
            DurationDays = req.DurationDays,
            Instructions = req.Instructions,
            PrescribedDate = req.PrescribedDate != default ? req.PrescribedDate : DateTime.UtcNow,
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Prescriptions.Add(prescription);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = prescription.Id }, prescription);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var prescription = await _db.Prescriptions.FindAsync(id);
        if (prescription == null) return NotFound();
        return Ok(prescription);
    }

    [HttpGet("patient/{patientId}")]
    public async Task<IActionResult> GetByPatientId(Guid patientId)
    {
        var prescriptions = await _db.Prescriptions
            .Where(p => p.PatientId == patientId)
            .OrderByDescending(p => p.PrescribedDate)
            .ToListAsync();
        return Ok(prescriptions);
    }

    [HttpGet("patient/{patientId}/active")]
    public async Task<IActionResult> GetActiveByPatientId(Guid patientId)
    {
        var prescriptions = await _db.Prescriptions
            .Where(p => p.PatientId == patientId && p.Status == "Active")
            .OrderByDescending(p => p.PrescribedDate)
            .ToListAsync();
        return Ok(prescriptions);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePrescription(Guid id, [FromBody] UpdatePrescriptionRequest req)
    {
        var prescription = await _db.Prescriptions.FindAsync(id);
        if (prescription == null) return NotFound();

        prescription.MedicationName = req.MedicationName;
        prescription.Dosage = req.Dosage;
        prescription.Frequency = req.Frequency;
        prescription.DurationDays = req.DurationDays;
        prescription.Instructions = req.Instructions;
        prescription.Status = req.Status ?? prescription.Status;
        prescription.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(prescription);
    }

    [HttpPut("{id}/status")]
    [Authorize]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdatePrescriptionStatusRequest req)
    {
        var prescription = await _db.Prescriptions.FindAsync(id);
        if (prescription == null) return NotFound();

        prescription.Status = req.Status;
        prescription.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(prescription);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeletePrescription(Guid id)
    {
        var prescription = await _db.Prescriptions.FindAsync(id);
        if (prescription == null) return NotFound();

        _db.Prescriptions.Remove(prescription);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}

public record CreatePrescriptionRequest(
    Guid MedicalRecordId,
    Guid PatientId,
    Guid DoctorId,
    string MedicationName,
    string? AtcCode,
    string Dosage,
    string Frequency,
    int DurationDays,
    string? Instructions,
    DateTime PrescribedDate
);

public record UpdatePrescriptionRequest(
    string MedicationName,
    string Dosage,
    string Frequency,
    int DurationDays,
    string? Instructions,
    string? Status
);

public record UpdatePrescriptionStatusRequest(string Status);

