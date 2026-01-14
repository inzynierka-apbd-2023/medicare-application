using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MedicalRecordsService.Data;
using MedicalRecordsService.Models;

using MediatR;
using MedicalRecordsService.Features.MedicalRecords.Queries.GetPatientHistory;

namespace MedicalRecordsService.Controllers;

[ApiController]
[Route("api/medical-records/records")]
public class MedicalRecordsController : ControllerBase
{
    private readonly MedicalRecordsDbContext _db;
    private readonly IMediator _mediator;
    public MedicalRecordsController(MedicalRecordsDbContext db, IMediator mediator)
    {
        _db = db;
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateMedicalRecord([FromBody] CreateMedicalRecordRequest req)
    {
        if (req.PatientId == Guid.Empty || req.DoctorId == Guid.Empty)
            return BadRequest("PatientId and DoctorId are required");

        var record = new MedicalRecord
        {
            PatientId = req.PatientId,
            DoctorId = req.DoctorId,
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
        var records = await _db.MedicalRecords
            .Where(r => r.PatientId == patientId)
            .OrderByDescending(r => r.VisitDate)
            .ToListAsync();
        return Ok(records);
    }

    [HttpGet("patient-history/{patientId}")]
    public async Task<IActionResult> GetPatientHistory(Guid patientId)
    {
        var result = await _mediator.Send(new GetPatientHistoryQuery(patientId));
        return Ok(result);
    }

    [HttpGet("{id}/complete")]
    public async Task<IActionResult> GetCompleteRecord(Guid id)
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
    Guid? AppointmentId,
    DateTime VisitDate,
    string? ChiefComplaint,
    string? HistoryOfPresentIllness,
    string? PhysicalExamination,
    string? Assessment,
    string? Plan,
    string? Notes
);
