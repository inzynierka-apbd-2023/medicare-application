using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LabService.Data;
using LabService.Models;

namespace LabService.Controllers;

[ApiController]
[Route("api/lab/[controller]")]
public class LabResultsController : ControllerBase
{
    private readonly LabDbContext _db;
    public LabResultsController(LabDbContext db) => _db = db;

    [HttpGet("patient/{patientId}")]
    public async Task<IActionResult> GetByPatientId(Guid patientId)
    {
        var results = await _db.LabResults
            .Where(r => r.PatientId == patientId)
            .OrderByDescending(r => r.ResultDate)
            .ToListAsync();
        return Ok(results);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _db.LabResults.FindAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("{id}/detail")]
    public async Task<IActionResult> GetDetailById(Guid id)
    {
        var result = await _db.LabResults.FindAsync(id);
        if (result == null) return NotFound();

        var test = await _db.LabTests.FindAsync(result.LabTestId);
        var order = test != null ? await _db.LabOrders.FindAsync(test.LabOrderId) : null;
        var reviews = await _db.LabResultReviews
            .Where(r => r.LabResultId == id)
            .OrderByDescending(r => r.ReviewedAt)
            .ToListAsync();

        return Ok(new
        {
            Result = result,
            Test = test,
            Order = order,
            Reviews = reviews
        });
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateResult([FromBody] CreateLabResultRequest req)
    {
        var result = new LabResult
        {
            LabTestId = req.LabTestId,
            PatientId = req.PatientId,
            Value = req.Value,
            Unit = req.Unit,
            ReferenceRange = req.ReferenceRange,
            Flag = req.Flag,
            Comments = req.Comments,
            ResultDate = req.ResultDate,
            CreatedAt = DateTime.UtcNow
        };

        _db.LabResults.Add(result);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("pending-review")]
    [Authorize]
    public async Task<IActionResult> GetPendingReview()
    {
        var pendingResults = await _db.LabResults
            .Where(r => r.ReviewStatus == "Pending")
            .OrderBy(r => r.ResultDate)
            .ToListAsync();
        return Ok(pendingResults);
    }

    [HttpPost("{id}/review")]
    [Authorize]
    public async Task<IActionResult> ReviewResult(Guid id, [FromBody] ReviewLabResultRequest req)
    {
        var result = await _db.LabResults.FindAsync(id);
        if (result == null) return NotFound();

        var review = new LabResultReview
        {
            LabResultId = id,
            ReviewedByDoctorId = req.ReviewedByDoctorId,
            ReviewedAt = DateTime.UtcNow,
            ReviewStatus = req.ReviewStatus,
            ReviewNotes = req.ReviewNotes,
            Recommendations = req.Recommendations,
            CreatedAt = DateTime.UtcNow
        };

        result.ReviewedByDoctorId = req.ReviewedByDoctorId;
        result.ReviewedAt = DateTime.UtcNow;
        result.ReviewStatus = "Reviewed";

        _db.LabResultReviews.Add(review);
        await _db.SaveChangesAsync();

        return Ok(review);
    }
}

public record CreateLabResultRequest(
    Guid LabTestId,
    Guid PatientId,
    string? Value,
    string? Unit,
    string? ReferenceRange,
    string? Flag,
    string? Comments,
    DateTime ResultDate
);

public record ReviewLabResultRequest(
    Guid ReviewedByDoctorId,
    string ReviewStatus,
    string? ReviewNotes,
    string? Recommendations
);
