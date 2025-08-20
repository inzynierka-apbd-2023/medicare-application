using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PractitionerService.Data;
using PractitionerService.Models;

namespace PractitionerService.Controllers;

[ApiController]
[Route("api/practitioner/[controller]")]
public class DoctorsController : ControllerBase
{
    private readonly PractitionerDbContext _db;

    public DoctorsController(PractitionerDbContext db)
    {
        _db = db;
    }

    // Register doctor (link to existing userId)
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> RegisterDoctor([FromBody] RegisterDoctorRequest req)
    {
        if (req.UserId == Guid.Empty) return BadRequest("UserId is required");
        var userIdStr = req.UserId.ToString();
        if (await _db.Doctors.AnyAsync(d => d.UserId == userIdStr)) return Conflict("Doctor already registered for this user");
        
        var doctor = new Doctor 
        { 
            Id = Guid.NewGuid().ToString(), // Generate ID manually for compatibility
            UserId = userIdStr, 
            CreatedAt = DateTime.UtcNow, 
            UpdatedAt = DateTime.UtcNow, 
            Bio = req.Bio 
        };
        
        _db.Doctors.Add(doctor);
        await _db.SaveChangesAsync();
        // TODO: publish DoctorRegistered event
        return CreatedAtAction(nameof(GetDoctorById), new { id = doctor.Id }, new { doctor.Id, doctor.UserId });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDoctorById(string id)
    {
        var doctor = await _db.Doctors.FindAsync(id);
        if (doctor == null) return NotFound();
        return Ok(doctor);
    }

    // Lightweight directory projection for a single doctor
    [HttpGet("{id}/directory")]
    public async Task<IActionResult> GetDoctorDirectoryById(string id)
    {
        var item = await _db.Set<DoctorDirectory>().FirstOrDefaultAsync(d => d.DoctorId == id);
        if (item == null) return NotFound();
        return Ok(item);
    }

    // Update specializations for a doctor
    [HttpPut("{id}/specializations")]
    [Authorize]
    public async Task<IActionResult> UpdateSpecializations(string id, [FromBody] UpdateSpecializationsRequest req)
    {
        if (!await _db.Doctors.AnyAsync(d => d.Id == id)) return NotFound("Doctor not found");
        // Ensure all provided specialization IDs exist
        var specIds = req.SpecializationIds?.Distinct().Select(g => g.ToString()).ToList() ?? new();
        var existing = await _db.Specializations.Where(s => specIds.Contains(s.Id)).Select(s => s.Id).ToListAsync();
        if (existing.Count != specIds.Count) return BadRequest("One or more specialization IDs are invalid");
        // Replace current set
        var current = _db.DoctorSpecializations.Where(ds => ds.DoctorId == id);
        _db.DoctorSpecializations.RemoveRange(current);
        _db.DoctorSpecializations.AddRange(specIds.Select(sid => new DoctorSpecialization { DoctorId = id, SpecializationId = sid }));
        await _db.SaveChangesAsync();
        // TODO: publish DoctorSpecializationUpdated event
        return NoContent();
    }

    // Catalog/search endpoint
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] Guid? specializationId, [FromQuery] Guid? serviceId, [FromQuery] string? q)
    {
        // Query from projection view DoctorDirectory and filter
        var query = _db.Set<DoctorDirectory>().AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            var ql = q.ToLowerInvariant();
            query = query.Where(d => d.FirstName.ToLower().Contains(ql) || d.LastName.ToLower().Contains(ql));
        }
        if (specializationId != null && specializationId != Guid.Empty)
        {
            var specializationIdStr = specializationId.ToString();
            query = query.Where(d => d.Specializations != null && d.Specializations.Contains(specializationIdStr));
        }
        if (serviceId != null && serviceId != Guid.Empty)
        {
            var sid = serviceId.ToString();
            // map service -> specialization ids
            var specIds = await _db.SpecializationServices
                .Where(ss => ss.ServiceId == sid)
                .Select(ss => ss.SpecializationId)
                .ToListAsync();
            if (specIds.Count > 0)
            {
                // intersect with doctor specialization CSV
                query = query.Where(d => d.Specializations != null && specIds.Any(sid => d.Specializations!.Contains(sid)));
            }
            else
            {
                // No mapping -> empty
                return Ok(Array.Empty<DoctorDirectory>());
            }
        }
        var results = await query.Take(100).ToListAsync();
        return Ok(results);
    }

    // Manage recurring availability
    [HttpPut("{id}/availability")]
    [Authorize]
    public async Task<IActionResult> SetAvailability(string id, [FromBody] List<ScheduleEntry> entries)
    {
        if (!await _db.Doctors.AnyAsync(d => d.Id == id)) return NotFound("Doctor not found");
        var current = _db.DoctorSchedules.Where(s => s.DoctorId == id);
        _db.DoctorSchedules.RemoveRange(current);
        var toAdd = entries.Select(e => new DoctorSchedule
        {
            DoctorId = id,
            DayOfWeek = e.DayOfWeek,
            StartTime = TimeSpan.Parse(e.Start),
            EndTime = TimeSpan.Parse(e.End),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }).ToList();
        _db.DoctorSchedules.AddRange(toAdd);
        await _db.SaveChangesAsync();
        // TODO: publish DoctorAvailabilityChanged event
        return NoContent();
    }

    // Read recurring availability
    [HttpGet("{id}/availability")]
    public async Task<IActionResult> GetAvailability(string id)
    {
        if (!await _db.Doctors.AnyAsync(d => d.Id == id)) return NotFound("Doctor not found");
        var schedules = await _db.DoctorSchedules
            .Where(s => s.DoctorId == id)
            .OrderBy(s => s.DayOfWeek)
            .ThenBy(s => s.StartTime)
            .ToListAsync();
        return Ok(schedules);
    }
}

public record RegisterDoctorRequest(Guid UserId, string? Bio);
public record UpdateSpecializationsRequest(List<Guid> SpecializationIds);
public record ScheduleEntry(int DayOfWeek, string Start, string End);
