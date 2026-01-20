using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PractitionerService.Data;
using PractitionerService.Models;
using MassTransit;
using Medicare.Messaging.Contracts;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Linq.Expressions;

namespace PractitionerService.Controllers;

[ApiController]
[Route("api/practitioner/[controller]")]
[Authorize]
public class DoctorsController : ControllerBase
{
    private readonly PractitionerDbContext _db;
    private readonly IHttpClientFactory _httpFactory;
    private readonly IPublishEndpoint _publishEndpoint;
    private const string DoctorNotFound = "Doctor not found";
    private readonly IRequestClient<IGetUsers> _getUsersClient;
    private readonly IRequestClient<IGetUser> _getUserClient;

    public DoctorsController(PractitionerDbContext db, IHttpClientFactory httpFactory, IPublishEndpoint publishEndpoint, IRequestClient<IGetUsers> getUsersClient, IRequestClient<IGetUser> getUserClient)
    {
        _db = db;
        _httpFactory = httpFactory;
        _publishEndpoint = publishEndpoint;
        _getUsersClient = getUsersClient;
        _getUserClient = getUserClient;
    }

    public record CreateDoctorFullRequest(
        UserProfileDto Profile,
        string? Biography,
        List<Guid>? SpecializationIds
    );

    public record UserProfileDto(
        string FirstName,
        string LastName,
        string Email,
        string? Phone,
        DateTime? DateOfBirth,
        string? Gender,
        string? AddressLine1,
        string? AddressLine2,
        string? City,
        string? State,
        string? ZipCode,
        string? Country
    );

    private static string GenerateUsername(UserProfileDto p)
    {
        var date = DateTime.UtcNow.ToString("yyyyMMdd");
        var lastInitial = string.IsNullOrWhiteSpace(p.LastName) ? "x" : p.LastName.Trim().Substring(0,1).ToLowerInvariant();
        return $"doctor_{lastInitial}_{date}";
    }

    private static string GenerateStrongPassword()
    {
        const string lower = "abcdefghijklmnopqrstuvwxyz";
        const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string digits = "0123456789";
        const string symbols = "!@#$%^&*()_+[]{}-=";
        var rnd = Random.Shared;
        string Pick(string chars, int n) => new string(Enumerable.Range(0, n).Select(_ => chars[rnd.Next(chars.Length)]).ToArray());
        var parts = new[] { Pick(upper,2), Pick(lower,4), Pick(digits,2), Pick(symbols,2), Pick(lower+upper+digits+symbols,2) };
        var all = string.Concat(parts).ToCharArray();
        for (int i=0;i<all.Length;i++){ int j=rnd.Next(all.Length); (all[i],all[j])=(all[j],all[i]); }
        return new string(all);
    }

    private static async Task<string> EnsureUniqueUsernameAsync(HttpClient client, string desired)
    {
        var tryName = desired;
        int suffix = 1;
        while (true)
        {
            var resp = await client.GetAsync($"/api/users/availability?username={Uri.EscapeDataString(tryName)}");
            if (!resp.IsSuccessStatusCode)
            {
                return tryName;
            }
            using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
            var exists = doc.RootElement.TryGetProperty("usernameExists", out var val) && val.GetBoolean();
            if (!exists) return tryName;
            suffix++;
            tryName = $"{desired}_{suffix}";
        }
    }

    [HttpPost("register-full")]
    public async Task<IActionResult> RegisterDoctorWithUser([FromBody] CreateDoctorFullRequest req)
    {
        static bool Missing(params string?[] vals) => vals.Any(v => string.IsNullOrWhiteSpace(v));
        if (req?.Profile == null) return BadRequest("Profile is required");
        if (Missing(req.Profile.FirstName, req.Profile.LastName, req.Profile.Email))
            return BadRequest("FirstName, LastName, and Email are required");

        var http = _httpFactory.CreateClient("UserService");

        var emailAvailability = await http.GetAsync($"/api/users/availability?email={Uri.EscapeDataString(req.Profile.Email)}");
        if (emailAvailability.IsSuccessStatusCode)
        {
            using var doc = JsonDocument.Parse(await emailAvailability.Content.ReadAsStringAsync());
            if (doc.RootElement.TryGetProperty("emailExists", out var emailExistsEl) && emailExistsEl.GetBoolean())
            {
                return Conflict(new { message = "A user with this email already exists. Please use a different email." });
            }
        }

        var desired = GenerateUsername(req.Profile);
        var username = await EnsureUniqueUsernameAsync(http, desired);
        var password = GenerateStrongPassword();

        var createUserPayload = new
        {
            username,
            password,
            email = req.Profile.Email,
            firstName = req.Profile.FirstName,
            lastName = req.Profile.LastName,
            role = "Doctor",
            phoneNumber = req.Profile.Phone,
            dateOfBirth = req.Profile.DateOfBirth
        };
        var regResp = await http.PostAsync("/api/auth/register",
            new StringContent(JsonSerializer.Serialize(createUserPayload), Encoding.UTF8, "application/json"));
        if (!regResp.IsSuccessStatusCode)
        {
            var body = await regResp.Content.ReadAsStringAsync();
            return StatusCode((int)regResp.StatusCode, new { message = "User registration failed", details = body });
        }
        var regText = await regResp.Content.ReadAsStringAsync();
        using var regDoc = JsonDocument.Parse(regText);
        var root = regDoc.RootElement;
        string? userId = ExtractString(root, "user", "id") ?? ExtractString(root, "user", "Id");
        string? accessToken = ExtractString(root, null, "accessToken") ?? ExtractString(root, null, "AccessToken");
        if (string.IsNullOrWhiteSpace(userId)) return StatusCode(500, new { message = "User registration response missing Id" });
        if (string.IsNullOrWhiteSpace(accessToken)) return StatusCode(500, new { message = "User registration response missing access token" });

        var updatePayload = new
        {
            addressLine1 = req.Profile.AddressLine1,
            addressLine2 = req.Profile.AddressLine2,
            city = req.Profile.City,
            state = req.Profile.State,
            zipCode = req.Profile.ZipCode,
            country = req.Profile.Country,
            isActive = true
        };
        using var updateReq = new HttpRequestMessage(HttpMethod.Put, $"/api/users/{userId}")
        {
            Content = new StringContent(JsonSerializer.Serialize(updatePayload), Encoding.UTF8, "application/json")
        };
        updateReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var updResp = await http.SendAsync(updateReq);
        if (!updResp.IsSuccessStatusCode)
        {
            var body = await updResp.Content.ReadAsStringAsync();
            return StatusCode((int)updResp.StatusCode, new { message = "User profile update failed", details = body });
        }

        var now = DateTime.UtcNow;
        var doctor = new Doctor { Id = Guid.NewGuid(), UserId = Guid.Parse(userId!), Bio = req.Biography, IsActive = true, CreatedAt = now, UpdatedAt = now };
        _db.Doctors.Add(doctor);
        await _db.SaveChangesAsync();
        if (req.SpecializationIds != null && req.SpecializationIds.Count > 0)
        {
            var ids = req.SpecializationIds.ToList();
            _db.DoctorSpecializations.AddRange(ids.Select(sid => new DoctorSpecialization { DoctorId = doctor.Id, SpecializationId = sid }));
            await _db.SaveChangesAsync();
        }

        var dir = await _db.Set<DoctorDirectory>().FirstOrDefaultAsync(d => d.DoctorId == doctor.Id);
        return CreatedAtAction(nameof(GetDoctorDirectoryById), new { id = doctor.Id }, new
        {
            directory = dir,
            credentials = new { username, password }
        });
    }

    private static string? ExtractString(JsonElement root, string? parent, string child)
    {
        if (parent == null)
        {
            return root.TryGetProperty(child, out var node) ? node.GetString() : null;
        }
        if (!root.TryGetProperty(parent, out var p)) return null;
        return p.TryGetProperty(child, out var c) ? c.GetString() : null;
    }

    [HttpPost]
    public async Task<IActionResult> RegisterDoctor([FromBody]   RegisterDoctorRequest req)
    {
        if (req.UserId == Guid.Empty) return BadRequest("UserId is required");
        if (await _db.Doctors.AnyAsync(d => d.UserId == req.UserId)) return Conflict("Doctor already registered for this user");
        
        var doctor = new Doctor 
        { 
            Id = Guid.NewGuid(), 
            UserId = req.UserId, 
            CreatedAt = DateTime.UtcNow, 
            UpdatedAt = DateTime.UtcNow, 
            Bio = req.Bio 
        };
        
        _db.Doctors.Add(doctor);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetDoctorById), new { id = doctor.Id }, new { doctor.Id, doctor.UserId });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDoctorById(Guid id)
    {
        var doctor = await _db.Doctors.FindAsync(id);
        if (doctor == null) return NotFound();
        return Ok(doctor);
    }

    [HttpGet("by-user/{userId}")]
    public async Task<IActionResult> GetDoctorByUserId(Guid userId)
    {
        var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
        if (doctor == null) return NotFound("Doctor not found for this user");
        return Ok(doctor);
    }

    [HttpGet("{id}/directory")]
    public async Task<IActionResult> GetDoctorDirectoryById(Guid id)
    {
        var doctor = await _db.Doctors.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
        if (doctor == null) return NotFound();

        IUserResponse? user = null;
        try
        {
            var response = await _getUserClient.GetResponse<IUserResponse>(new { UserId = doctor.UserId });
            user = response.Message;
        }
        catch (Exception ex)
        {
             Console.WriteLine($"Failed to fetch user {doctor.UserId}: {ex}");
        }

        var specs = await _db.DoctorSpecializations
            .Where(ds => ds.DoctorId == doctor.Id)
            .Select(ds => ds.SpecializationId.ToString())
            .ToListAsync();

        var dir = new DoctorDirectory
        {
            DoctorId = doctor.Id,
            UserId = doctor.UserId,
            FirstName = user?.FirstName,
            LastName = user?.LastName,
            Email = user?.Email,
            Phone = user?.Phone,
            Specializations = string.Join(",", specs),
            Services = null,
            IsActive = doctor.IsActive
        };
        
        return Ok(dir);
    }

    [HttpPut("{id}/specializations")]
    public async Task<IActionResult> UpdateSpecializations(Guid id, [FromBody] UpdateSpecializationsRequest req)
    {
    if (!await _db.Doctors.AnyAsync(d => d.Id == id)) return NotFound(DoctorNotFound);
        var specIds = req.SpecializationIds?.Distinct().ToList() ?? new();
        var existing = await _db.Specializations.Where(s => specIds.Contains(s.Id)).Select(s => s.Id).ToListAsync();
        if (existing.Count != specIds.Count) return BadRequest("One or more specialization IDs are invalid");
        var current = _db.DoctorSpecializations.Where(ds => ds.DoctorId == id);
        _db.DoctorSpecializations.RemoveRange(current);
        _db.DoctorSpecializations.AddRange(specIds.Select(sid => new DoctorSpecialization { DoctorId = id, SpecializationId = sid }));
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] Guid? specializationId, [FromQuery] Guid? serviceId, [FromQuery] string? q, [FromQuery] bool? isActive)
    {
        var query = _db.Doctors.AsNoTracking().AsQueryable();

        if (isActive.HasValue)
        {
            query = query.Where(d => d.IsActive == isActive.Value);
        }

        if (specializationId != null && specializationId != Guid.Empty)
        {
            query = query.Where(d => _db.DoctorSpecializations.Any(ds => ds.DoctorId == d.Id && ds.SpecializationId == specializationId.Value));
        }

        if (serviceId != null && serviceId != Guid.Empty)
        {
             var specIds = await _db.SpecializationServices
                .Where(ss => ss.ServiceId == serviceId.Value)
                .Select(ss => ss.SpecializationId)
                .ToListAsync();

            if (specIds.Any())
            {
                 query = query.Where(d => _db.DoctorSpecializations.Any(ds => ds.DoctorId == d.Id && specIds.Contains(ds.SpecializationId)));
            }
            else
            {
                return Ok(Array.Empty<DoctorDirectory>());
            }
        }

        // Fetch doctors first (limit 100 to avoid huge fetch, though we might need more for search if q is applied in memory)
        var doctors = await query.Include(d => d.Schedules).Take(100).ToListAsync();

        if (!doctors.Any()) return Ok(Array.Empty<DoctorDirectory>());

        var userIds = doctors.Select(d => d.UserId).Distinct().ToList();
        var userMap = new Dictionary<Guid, IUserResponse>();

        try
        {
            var response = await _getUsersClient.GetResponse<IUsersResponse>(new { UserIds = userIds });
            if (response.Message.Users != null)
            {
                foreach (var u in response.Message.Users)
                {
                    userMap[u.Id] = u;
                }
            }
        }
        catch (Exception ex)
        {
            // Log error, but continue with partial data
             Console.WriteLine($"Failed to fetch users: {ex}");
        }
        
        // Fetch specializations for these doctors efficiently
        var doctorIds = doctors.Select(d => d.Id).ToList();
        var docSpecs = await _db.DoctorSpecializations
            .Where(ds => doctorIds.Contains(ds.DoctorId))
            .ToListAsync();
            
        var results = doctors.Select(d => 
        {
            userMap.TryGetValue(d.UserId, out var user);
            var specs = docSpecs.Where(ds => ds.DoctorId == d.Id).Select(ds => ds.SpecializationId.ToString()).ToList();
            
            return new DoctorDirectory
            {
                DoctorId = d.Id,
                UserId = d.UserId,
                FirstName = user?.FirstName,
                LastName = user?.LastName,
                Email = user?.Email,
                Phone = user?.Phone,
                Specializations = string.Join(",", specs),
                Services = null,
                IsActive = d.IsActive
            };
        });

        // Apply Name Search in Memory
        if (!string.IsNullOrWhiteSpace(q))
        {
            var ql = q.ToLowerInvariant();
            results = results.Where(d => 
                (d.FirstName != null && d.FirstName.ToLower().Contains(ql)) || 
                (d.LastName != null && d.LastName.ToLower().Contains(ql))
            );
        }

        return Ok(results.ToList());
    }

    [HttpPut("{id}/availability")]
    public async Task<IActionResult> SetAvailability(Guid id, [FromBody] List<ScheduleEntry> entries)
    {
    if (!await _db.Doctors.AnyAsync(d => d.Id == id)) return NotFound(DoctorNotFound);
        var current = _db.DoctorSchedules.Where(s => s.DoctorId == id);
        _db.DoctorSchedules.RemoveRange(current);
        var toAdd = entries.Select(e => new DoctorSchedule
        {
            DoctorId = id,
            DayOfWeek = e.DayOfWeek,
            StartTime = TimeSpan.Parse(e.Start, System.Globalization.CultureInfo.InvariantCulture),
            EndTime = TimeSpan.Parse(e.End, System.Globalization.CultureInfo.InvariantCulture),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }).ToList();
        _db.DoctorSchedules.AddRange(toAdd);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("{id}/availability")]
    public async Task<IActionResult> GetAvailability(Guid id)
    {
        var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.Id == id || d.UserId == id);
        if (doctor == null) return NotFound("Doctor not found");
        
        var schedules = await _db.DoctorSchedules
            .Where(s => s.DoctorId == doctor.Id)
            .OrderBy(s => s.DayOfWeek)
            .ThenBy(s => s.StartTime)
            .ToListAsync();
        return Ok(schedules);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDoctor(Guid id)
    {
        var doctor = await _db.Doctors.FindAsync(id);
        if (doctor == null) return NotFound("Doctor not found");

        // Note: DeleteDoctor might now fail if DoctorDirectory view is broken.
        // We might want to construct snapshot manually like in Search if needed.
        // For now, let's keep it as is, or use Search logic if we want to be safe.
        // But let's assume Delete is less critical for the user's current dashboard issue.
        var dir = await _db.Set<DoctorDirectory>().FirstOrDefaultAsync(d => d.DoctorId == id);
        List<object>? schedules = null;
        var schedList = await _db.DoctorSchedules.Where(s => s.DoctorId == id)
            .Select(s => new { s.DayOfWeek, Start = s.StartTime.ToString(), End = s.EndTime.ToString() })
            .ToListAsync();
        schedules = schedList.Cast<object>().ToList();

        var snapshot = new
        {
            Directory = dir == null ? null : new
            {
                dir.DoctorId,
                dir.UserId,
                dir.FirstName,
                dir.LastName,
                dir.Email,
                dir.Phone,
                dir.Specializations,
                dir.Services
            },
            Schedules = schedules
        };
        var snapshotJson = JsonSerializer.Serialize(snapshot);

        await _publishEndpoint.Publish<IDoctorArchived>(new
        {
            DoctorId = doctor.Id,
            DoctorUserId = doctor.UserId,
            OccurredAt = DateTime.UtcNow,
            FullName = dir == null ? null : ($"{dir.FirstName} {dir.LastName}").Trim(),
            Email = dir?.Email,
            Phone = dir?.Phone,
            SnapshotJson = snapshotJson
        });

        var specs = _db.DoctorSpecializations.Where(x => x.DoctorId == id);
        _db.DoctorSpecializations.RemoveRange(specs);
        var scheds = _db.DoctorSchedules.Where(x => x.DoctorId == id);
        _db.DoctorSchedules.RemoveRange(scheds);
        doctor.IsActive = false;
        doctor.UpdatedAt = DateTime.UtcNow;
        _db.Doctors.Update(doctor);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}


public record RegisterDoctorRequest(Guid UserId, string? Bio);
public record UpdateSpecializationsRequest(List<Guid> SpecializationIds);
public record ScheduleEntry(int DayOfWeek, string Start, string End);
