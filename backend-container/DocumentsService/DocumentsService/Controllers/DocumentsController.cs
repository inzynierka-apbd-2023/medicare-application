using DocumentsService.Data;
using DocumentsService.Contracts;
using DocumentsService.Infrastructure.Events;
using DocumentsService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;

namespace DocumentsService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly DocumentsDbContext _db;
    private readonly IEventPublisher _events;
    public DocumentsController(DocumentsDbContext db, IEventPublisher events) { _db = db; _events = events; }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<Document>> Create([FromBody] CreateDocumentRequest req)
    {
        var type = await _db.DocumentTypes.FirstOrDefaultAsync(t => t.Id == req.DocumentTypeId || t.Code == (req.DocumentTypeCode ?? string.Empty));
        if (type == null) return BadRequest("Unknown DocumentType");
        int resolvedType = req.Type ?? type.Code?.ToUpperInvariant() switch
        {
            "VISIT_NOTE" => (int)DocumentKind.VisitNote,
            "PRESCRIPTION" => (int)DocumentKind.Prescription,
            "REFERRAL" => (int)DocumentKind.Referral,
            "SICK_LEAVE" => (int)DocumentKind.SickLeave,
            "LAB_RESULTS" => (int)DocumentKind.LabResults,
            _ => (int)DocumentKind.VisitNote
        };
        var doc = new Document
        {
            PatientId = req.PatientId,
            DoctorId = req.DoctorId,
            Notes = req.Notes,
            DocumentTypeId = type.Id,
            Type = resolvedType,
            FilePath = req.FilePath,
            FileSizeBytes = req.FileSizeBytes
        };
    _db.Documents.Add(doc);
    await _db.SaveChangesAsync();
    await _events.PublishAsync(new DocumentCreated(doc.Id, doc.PatientId, doc.DoctorId, doc.Type, doc.CreatedAt));
        return CreatedAtAction(nameof(GetById), new { id = doc.Id }, doc);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<Document>> GetById(string id)
    {
        var d = await _db.Documents
            .Include(x => x.VisitDocument)
            .Include(x => x.Prescription)
            .Include(x => x.Referral)
            .Include(x => x.SickLeave)
            .Include(x => x.LabResults).ThenInclude(r => r!.Results)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (d == null) return NotFound();
        return d;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<Document>>> List([FromQuery] string? patientId, [FromQuery] string? appointmentId, [FromQuery] int? type)
    {
        IQueryable<Document> q = _db.Documents
            .Include(x => x.VisitDocument)
            .Include(x => x.Prescription)
            .Include(x => x.Referral)
            .Include(x => x.SickLeave)
            .Include(x => x.LabResults).ThenInclude(r => r!.Results);
        if (!string.IsNullOrWhiteSpace(patientId)) q = q.Where(d => d.PatientId == patientId);
        if (type.HasValue) q = q.Where(d => d.Type == type.Value);
        if (!string.IsNullOrWhiteSpace(appointmentId))
        {
            q = from d in q
                join a in _db.DocumentAssignments on d.Id equals a.DocumentId
                where a.AppointmentId == appointmentId
                select d;
        }
        var list = await q.OrderByDescending(d => d.CreatedAt).ToListAsync();
        return list;
    }

    [HttpPost("{id}/visit-note")]
    [Authorize]
    public async Task<ActionResult> AttachVisitNote(string id, [FromBody] VisitDocument payload)
    {
        var doc = await _db.Documents.FindAsync(id);
        if (doc == null) return NotFound();
        payload.DocumentId = id;
    _db.VisitDocuments.Add(payload);
    await _db.SaveChangesAsync();
    await _events.PublishAsync(new VisitNoteAdded(id));
        return NoContent();
    }

    [HttpPost("{id}/prescription")]
    [Authorize]
    public async Task<ActionResult> AttachPrescription(string id, [FromBody] PrescriptionRequest payload)
    {
        var doc = await _db.Documents.FindAsync(id);
        if (doc == null) return NotFound();

        string? atcCode = payload.AtcCode;
        string? atcName = null;
        string? medication = payload.Medication;
        if (!string.IsNullOrWhiteSpace(atcCode))
        {
            var atc = await LookupAtcAsync(atcCode!);
            if (atc is null) return BadRequest($"Unknown ATC code: {atcCode}");
            atcCode = atc.Code; atcName = atc.Name;
            if (string.IsNullOrWhiteSpace(medication)) medication = atc.Name;
        }
        else if (!string.IsNullOrWhiteSpace(medication))
        {
            var atc = await SearchAtcAsync(medication!);
            if (atc is not null)
            {
                atcCode = atc.Code; atcName = atc.Name; medication = atc.Name;
            }
        }

        var entity = new Prescription
        {
            DocumentId = id,
            Medication = medication ?? string.Empty,
            Dosage = payload.Dosage,
            Frequency = payload.Frequency,
            DurationDays = payload.DurationDays,
            Instructions = payload.Instructions,
            PharmacyName = payload.PharmacyName,
            PharmacyPhone = payload.PharmacyPhone,
            RefillsRemaining = payload.RefillsRemaining,
            AtcCode = atcCode,
            AtcName = atcName
        };
    _db.Prescriptions.Add(entity);
    await _db.SaveChangesAsync();
    await _events.PublishAsync(new PrescriptionIssued(id, entity.AtcCode, entity.Medication));
        return NoContent();
    }

    [HttpPost("{id}/referral")]
    [Authorize]
    public async Task<ActionResult> AttachReferral(string id, [FromBody] Referral payload)
    {
        var doc = await _db.Documents.FindAsync(id);
        if (doc == null) return NotFound();
        payload.DocumentId = id;
        _db.Referrals.Add(payload);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id}/sick-leave")]
    [Authorize]
    public async Task<ActionResult> AttachSickLeave(string id, [FromBody] SickLeave payload)
    {
        var doc = await _db.Documents.FindAsync(id);
        if (doc == null) return NotFound();
        payload.DocumentId = id;
        _db.SickLeaves.Add(payload);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id}/lab-results")]
    [Authorize]
    public async Task<ActionResult> AttachLabResults(string id, [FromBody] LabResultsRequest payload)
    {
        var doc = await _db.Documents.FindAsync(id);
        if (doc == null) return NotFound();

        var entity = new LabResults
        {
            DocumentId = id,
            TestType = payload.TestType,
            TestDate = payload.TestDate,
            Laboratory = payload.Laboratory,
            OverallStatus = payload.OverallStatus,
            Interpretation = payload.Interpretation,
            ReferenceRanges = payload.ReferenceRanges,
            TechnicianName = payload.TechnicianName,
            DoctorComments = payload.DoctorComments
        };

        var errors = new List<string>();
        if (payload.Results != null)
        {
            foreach (var r in payload.Results)
            {
                var result = new LabTestResult
                {
                    LabResultsDocumentId = id,
                    ParameterName = r.ParameterName,
                    Value = r.Value,
                    NumericValue = r.NumericValue,
                    Unit = r.Unit,
                    ReferenceRange = r.ReferenceRange,
                    Status = r.Status,
                    Notes = r.Notes,
                    IsAbnormal = r.IsAbnormal
                };

                var resolved = await ResolveLoincAsync(r.LoincCode, r.ParameterName);
                if (resolved.Error != null)
                {
                    errors.Add(resolved.Error);
                }
                else if (resolved.Type != null)
                {
                    result.LabTestTypeId = resolved.Type.Id;
                    result.LoincCode = resolved.Code;
                    var unitErr = ValidateUnits(resolved.Loinc, result.Unit, result.ParameterName);
                    if (unitErr != null) errors.Add(unitErr);
                }

                entity.Results.Add(result);
            }
        }

        if (errors.Count > 0)
        {
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                { "Loinc", errors.ToArray() }
            }) { Title = "LOINC validation failed" });
        }

        _db.LabResults.Add(entity);
        await _db.SaveChangesAsync();
        await _events.PublishAsync(new LabResultsPosted(id, entity.Results?.Count ?? 0));
        return NoContent();
    }

    [HttpPost("{id}/assign")]
    [Authorize]
    public async Task<ActionResult> AssignToAppointment(string id, [FromBody] AssignRequest req)
    {
        var doc = await _db.Documents.FindAsync(id);
        if (doc == null) return NotFound();
        var exists = await _db.DocumentAssignments.AnyAsync(x => x.DocumentId == id && x.AppointmentId == req.AppointmentId);
        if (!exists)
        {
            _db.DocumentAssignments.Add(new DocumentAssignment { DocumentId = id, AppointmentId = req.AppointmentId });
            await _db.SaveChangesAsync();
            await _events.PublishAsync(new DocumentAssignedToAppointment(id, req.AppointmentId));
        }
        return NoContent();
    }

    // Helpers
    private static string CatalogBase(HttpContext ctx)
        => ctx.RequestServices.GetService<IConfiguration>()?["CATALOG_SERVICE_BASE_URL"]
           ?? "http://medical-catalog-service:8083";

    private sealed record AtcDto(string AtcCode, string? AtcName);
    private sealed record AtcResult(string Code, string Name);

    private async Task<AtcResult?> LookupAtcAsync(string code)
    {
        try
        {
            using var http = new HttpClient { BaseAddress = new Uri(CatalogBase(HttpContext)) };
            var list = await http.GetFromJsonAsync<List<AtcDto>>("/api/catalog/atc?q=" + Uri.EscapeDataString(code));
            var hit = list?.FirstOrDefault(x => string.Equals(x.AtcCode, code, StringComparison.OrdinalIgnoreCase));
            return hit == null ? null : new AtcResult(hit.AtcCode, hit.AtcName ?? hit.AtcCode);
        }
        catch { return null; }
    }

    private async Task<AtcResult?> SearchAtcAsync(string query)
    {
        try
        {
            using var http = new HttpClient { BaseAddress = new Uri(CatalogBase(HttpContext)) };
            var list = await http.GetFromJsonAsync<List<AtcDto>>("/api/catalog/atc?q=" + Uri.EscapeDataString(query));
            var hit = list?.FirstOrDefault();
            return hit == null ? null : new AtcResult(hit.AtcCode, hit.AtcName ?? hit.AtcCode);
        }
        catch { return null; }
    }

    private sealed record LoincDto(
        string LoincNum,
        string? LongCommonName,
        string? Component,
        string? Property,
        string? TimeAspect,
        string? System,
        string? ScaleType,
        string? MethodType,
        string? ExampleUnits
    );

    private async Task<LoincDto?> LookupLoincAsync(string code)
    {
        try
        {
            using var http = new HttpClient { BaseAddress = new Uri(CatalogBase(HttpContext)) };
            var list = await http.GetFromJsonAsync<List<LoincDto>>("/api/catalog/loinc?q=" + Uri.EscapeDataString(code));
            return list?.FirstOrDefault(x => string.Equals(x.LoincNum, code, StringComparison.OrdinalIgnoreCase));
        }
        catch { return null; }
    }

    private async Task<LoincDto?> SearchLoincAsync(string query)
    {
        try
        {
            using var http = new HttpClient { BaseAddress = new Uri(CatalogBase(HttpContext)) };
            var list = await http.GetFromJsonAsync<List<LoincDto>>("/api/catalog/loinc?q=" + Uri.EscapeDataString(query));
            return list?.FirstOrDefault();
        }
        catch { return null; }
    }

    private sealed record LoincResolution(LabTestType? Type, LoincDto? Loinc, string? Code, string? Error);

    private async Task<LoincResolution> ResolveLoincAsync(string? loincCode, string? parameterName)
    {
        string? code = string.IsNullOrWhiteSpace(loincCode) ? null : loincCode!.Trim();
        LoincDto? loinc = null;
        if (!string.IsNullOrWhiteSpace(code))
        {
            loinc = await LookupLoincAsync(code!);
            if (loinc is null) return new LoincResolution(null, null, null, $"Unknown LOINC code: {code}");
        }
        else if (!string.IsNullOrWhiteSpace(parameterName))
        {
            loinc = await SearchLoincAsync(parameterName!);
            if (loinc != null) code = loinc.LoincNum;
        }

        if (loinc is null || code is null) return new LoincResolution(null, null, null, null);

        var type = await _db.LabTestTypes.FirstOrDefaultAsync(t => t.LoincCode == loinc.LoincNum);
        if (type == null)
        {
            type = new LabTestType
            {
                LoincCode = loinc.LoincNum,
                Name = loinc.LongCommonName,
                LoincComponent = loinc.Component,
                LoincProperty = loinc.Property,
                LoincTime = loinc.TimeAspect,
                LoincSystem = loinc.System,
                LoincScale = loinc.ScaleType,
                LoincMethod = loinc.MethodType,
                ExampleUnits = loinc.ExampleUnits
            };
            _db.LabTestTypes.Add(type);
            await _db.SaveChangesAsync();
        }

        return new LoincResolution(type, loinc, code, null);
    }

    private static string? ValidateUnits(LoincDto? loinc, string? unit, string? param)
    {
        if (loinc == null || string.IsNullOrWhiteSpace(loinc.ExampleUnits) || string.IsNullOrWhiteSpace(unit)) return null;
        return UnitsMatch(unit!, loinc.ExampleUnits!) ? null : $"Unit '{unit}' not compatible with LOINC {loinc.LoincNum} units '{loinc.ExampleUnits}' for parameter '{param}'.";
    }

    private static bool UnitsMatch(string provided, string example)
    {
        var p = NormalizeUnit(provided);
        var candidates = example.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                .Select(NormalizeUnit);
        return candidates.Contains(p);
    }

    private static string NormalizeUnit(string u) => u.Trim().ToLowerInvariant();
}

public record CreateDocumentRequest(
    string PatientId,
    string DoctorId,
    string? Notes,
    string? DocumentTypeId,
    string? DocumentTypeCode,
    int? Type,
    string? FilePath,
    long? FileSizeBytes
);

public record PrescriptionRequest(
    string? Medication,
    string? Dosage,
    string? Frequency,
    int? DurationDays,
    string? Instructions,
    string? PharmacyName,
    string? PharmacyPhone,
    int? RefillsRemaining,
    string? AtcCode
);

public record AssignRequest(string AppointmentId);

public record LabResultsRequest(
    string? TestType,
    DateTime? TestDate,
    string? Laboratory,
    string? OverallStatus,
    string? Interpretation,
    string? ReferenceRanges,
    string? TechnicianName,
    string? DoctorComments,
    List<LabTestResultRequest>? Results
);

public record LabTestResultRequest(
    string? LoincCode,
    string? ParameterName,
    string? Value,
    decimal? NumericValue,
    string? Unit,
    string? ReferenceRange,
    string? Status,
    string? Notes,
    bool? IsAbnormal
);
