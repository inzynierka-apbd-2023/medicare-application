using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MedicalRecordsService.Data;
using MedicalRecordsService.Models;

namespace MedicalRecordsService.Controllers;

[ApiController]
[Route("api/medical/[controller]")]
public class MedicalRecordsController : ControllerBase
{
    private readonly MedicalRecordsDbContext _db;
    public MedicalRecordsController(MedicalRecordsDbContext db) => _db = db;

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateMedicalRecord([FromBody] CreateMedicalRecordRequest req)
    {
        if (req.PatientId == Guid.Empty || req.DoctorId == Guid.Empty)
            return BadRequest("PatientId and DoctorId are required");

        var record = new MedicalRecord
        {
            PatientId = req.PatientId.ToString(),
            DoctorId = req.DoctorId.ToString(),
            AppointmentId = req.AppointmentId,
            VisitDate = req.VisitDate,
            ChiefComplaint = req.ChiefComplaint,
            HistoryOfPresentIllness = req.HistoryOfPresentIllness,
            PhysicalExamination = req.PhysicalExamination,
            Assessment = req.Assessment,
            Plan = req.Plan,
            Notes = req.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.MedicalRecords.Add(record);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = record.Id }, record);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var record = await _db.MedicalRecords.FindAsync(id);
        if (record == null) return NotFound();
        return Ok(record);
    }

    [HttpGet("patient/{patientId}")]
    public async Task<IActionResult> GetByPatientId(Guid patientId)
    {
        var patientIdStr = patientId.ToString();
        var records = await _db.MedicalRecords
            .Where(r => r.PatientId == patientIdStr)
            .OrderByDescending(r => r.VisitDate)
            .ToListAsync();
        return Ok(records);
    }

    [HttpGet("{id}/complete")]
    public async Task<IActionResult> GetCompleteRecord(string id)
    {
        var record = await _db.MedicalRecords.FindAsync(id);
        if (record == null) return NotFound();

        var diagnoses = await _db.Diagnoses
            .Where(d => d.MedicalRecordId == id)
            .ToListAsync();

        var prescriptions = await _db.Prescriptions
            .Where(p => p.MedicalRecordId == id)
            .ToListAsync();

        var vitalSigns = await _db.VitalSigns
            .Where(v => v.MedicalRecordId == id)
            .ToListAsync();

        return Ok(new
        {
            Record = record,
            Diagnoses = diagnoses,
            Prescriptions = prescriptions,
            VitalSigns = vitalSigns
        });
    }
}

public record CreateMedicalRecordRequest(
    Guid PatientId,
    Guid DoctorId,
    string? AppointmentId,
    DateTime VisitDate,
    string? ChiefComplaint,
    string? HistoryOfPresentIllness,
    string? PhysicalExamination,
    string? Assessment,
    string? Plan,
    string? Notes
);
