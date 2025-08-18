using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LabService.Data;
using LabService.Models;

namespace LabService.Controllers;

[ApiController]
[Route("api/lab/[controller]")]
public class LabOrdersController : ControllerBase
{
    private readonly LabDbContext _db;
    public LabOrdersController(LabDbContext db) => _db = db;

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateOrder([FromBody] CreateLabOrderRequest req)
    {
        var order = new LabOrder
        {
            PatientId = req.PatientId.ToString(),
            OrderingDoctorId = req.OrderingDoctorId,
            MedicalRecordId = req.MedicalRecordId,
            OrderedDate = req.OrderedDate,
            ClinicalNotes = req.ClinicalNotes,
            Priority = req.Priority,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.LabOrders.Add(order);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var order = await _db.LabOrders.FindAsync(id);
        if (order == null) return NotFound();
        return Ok(order);
    }

    [HttpGet("patient/{patientId}")]
    public async Task<IActionResult> GetByPatientId(Guid patientId)
    {
        var patientIdStr = patientId.ToString();
        var orders = await _db.LabOrders
            .Where(o => o.PatientId == patientIdStr)
            .OrderByDescending(o => o.OrderedDate)
            .ToListAsync();
        return Ok(orders);
    }

    [HttpGet("{id}/tests")]
    public async Task<IActionResult> GetOrderTests(string id)
    {
        var tests = await _db.LabTests
            .Where(t => t.LabOrderId == id)
            .ToListAsync();
        return Ok(tests);
    }

    [HttpPost("{id}/tests")]
    [Authorize]
    public async Task<IActionResult> AddTestToOrder(string id, [FromBody] AddLabTestRequest req)
    {
        var order = await _db.LabOrders.FindAsync(id);
        if (order == null) return NotFound();

        var test = new LabTest
        {
            LabOrderId = id,
            LoincCode = req.LoincCode,
            TestName = req.TestName,
            Instructions = req.Instructions,
            CreatedAt = DateTime.UtcNow
        };

        _db.LabTests.Add(test);
        await _db.SaveChangesAsync();

        return Ok(test);
    }

    [HttpPut("{id}/status")]
    [Authorize]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusRequest req)
    {
        var order = await _db.LabOrders.FindAsync(id);
        if (order == null) return NotFound();

        order.Status = req.Status;
        order.UpdatedAt = DateTime.UtcNow;
        
        if (req.Status == "Collected")
        {
            order.CollectedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return Ok(order);
    }
}

public record CreateLabOrderRequest(
    Guid PatientId,
    string OrderingDoctorId,
    string? MedicalRecordId,
    DateTime OrderedDate,
    string? ClinicalNotes,
    string Priority
);

public record AddLabTestRequest(
    string LoincCode,
    string TestName,
    string? Instructions
);

public record UpdateOrderStatusRequest(string Status);
