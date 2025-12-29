using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MedicalRecordsService.Data;
using MedicalRecordsService.Models;

namespace MedicalRecordsService.Controllers;

[ApiController]
[Route("api/medical/[controller]")]
public class PrescriptionsController : ControllerBase
{
    private readonly MedicalRecordsDbContext _db;
    public PrescriptionsController(MedicalRecordsDbContext db) => _db = db;

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreatePrescription([FromBody] CreatePrescriptionRequest req)
    {
        var prescription = new Prescription
        {
            MedicalRecordId = req.MedicalRecordId,
            PatientId = req.PatientId,
            DoctorId = req.DoctorId,
            MedicationName = req.MedicationName,
            AtcCode = req.AtcCode,
            Dosage = req.Dosage,
            Frequency = req.Frequency,
            DurationDays = req.DurationDays,
            Instructions = req.Instructions,
            PrescribedDate = req.PrescribedDate,
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

public record UpdatePrescriptionStatusRequest(string Status);
