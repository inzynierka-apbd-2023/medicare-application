using DocumentsService.Data;
using DocumentsService.Contracts;
using DocumentsService.Infrastructure.Events;
using DocumentsService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace DocumentsService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly DocumentsDbContext _db;
    private readonly IEventPublisher _events;
    private readonly ILogger<DocumentsController> _logger;
    private readonly IConnection _rabbitConn;

    public DocumentsController(DocumentsDbContext db, IEventPublisher events, ILogger<DocumentsController> logger, IConnection rabbitConn) 
    { 
        _db = db; 
        _events = events; 
        _logger = logger; 
        _rabbitConn = rabbitConn;
    }

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
        
        // Attempt enrichment (best-effort)
        doc.PatientName = await ResolvePatientNameQuickAsync(doc.PatientId) ?? doc.PatientName;
        doc.DoctorName = await ResolveDoctorNameQuickAsync(doc.DoctorId) ?? doc.DoctorName;

        _db.Documents.Add(doc);
        await _db.SaveChangesAsync();
        await _events.PublishAsync(new DocumentCreated(doc.Id, doc.PatientId, doc.DoctorId, doc.Type, doc.CreatedAt));
        return CreatedAtAction(nameof(GetById), new { id = doc.Id }, doc);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<Document>> GetById(Guid id)
    {
        var d = await _db.Documents
            .Include(x => x.VisitDocument)
            .Include(x => x.Prescription)
            .Include(x => x.Referral)
            .Include(x => x.SickLeave)
            .Include(x => x.LabResults).ThenInclude(r => r!.Results)
            .Include(x => x.Assignments)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (d == null) return NotFound();
        return d;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<Document>>> List([FromQuery] Guid? patientId, [FromQuery] Guid? appointmentId, [FromQuery] int? type)
    {
        IQueryable<Document> q = _db.Documents
            .Include(x => x.VisitDocument)
            .Include(x => x.Prescription)
            .Include(x => x.Referral)
            .Include(x => x.SickLeave)
            .Include(x => x.LabResults).ThenInclude(r => r!.Results)
            .Include(x => x.Assignments);
        if (patientId != null && patientId != Guid.Empty) 
        {
            q = q.Where(d => d.PatientId == patientId);
        }
        if (type.HasValue) q = q.Where(d => d.Type == type.Value);
        if (appointmentId != null && appointmentId != Guid.Empty)
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
    public async Task<ActionResult> AttachVisitNote(Guid id, [FromBody] VisitDocument payload)
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
    public async Task<ActionResult> AttachPrescription(Guid id, [FromBody] PrescriptionRequest payload)
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
    public async Task<ActionResult> AttachReferral(Guid id, [FromBody] Referral payload)
    {
        var doc = await _db.Documents.FindAsync(id);
        if (doc == null) return NotFound();
        payload.DocumentId = id;
        _db.Referrals.Add(payload);
        await _db.SaveChangesAsync();
        await _events.PublishAsync(new ReferralAdded(id));
        return NoContent();
    }

    [HttpPost("{id}/sick-leave")]
    [Authorize]
    public async Task<ActionResult> AttachSickLeave(Guid id, [FromBody] SickLeave payload)
    {
        var doc = await _db.Documents.FindAsync(id);
        if (doc == null) return NotFound();
        payload.DocumentId = id;
        _db.SickLeaves.Add(payload);
        await _db.SaveChangesAsync();
        await _events.PublishAsync(new SickLeaveAdded(id));
        return NoContent();
    }

    [HttpPost("{id}/lab-results")]
    [Authorize]
    public async Task<ActionResult> AttachLabResults(Guid id, [FromBody] LabResultsRequest payload)
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
    public async Task<ActionResult> AssignToAppointment(Guid id, [FromBody] AssignRequest req)
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

    [HttpGet("{id}/pdf")]
    [Authorize]
    public async Task<IActionResult> DownloadPdf(Guid id)
    {
        var d = await _db.Documents
            .Include(x => x.VisitDocument)
            .Include(x => x.Prescription)
            .Include(x => x.Referral)
            .Include(x => x.SickLeave)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (d == null) return NotFound();
        if (d.Type == (int)DocumentKind.LabResults)
            return BadRequest("Lab results PDFs are not supported in this endpoint.");

        await EnrichNamesIfMissingAsync(d);

        var payload = BuildPdfPayload(d);
        payload["PatientName"] = d.PatientName;
        payload["DoctorName"] = d.DoctorName;
        
        var corrId = Guid.NewGuid().ToString();
        var pdfBytes = await RequestPdfOverRabbitAsync(payload, corrId, HttpContext.RequestAborted);
        if (pdfBytes == null) return StatusCode(504, "PDF generation timed out");
        
        var fileName = $"document-{d.Id}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }

    private async Task EnrichNamesIfMissingAsync(DocumentsService.Models.Document d)
    {
        if (!string.IsNullOrWhiteSpace(d.PatientName) && !string.IsNullOrWhiteSpace(d.DoctorName)) return;
        
        bool changed = false;
        if (string.IsNullOrWhiteSpace(d.PatientName))
        {
            var name = await ResolvePatientNameQuickAsync(d.PatientId);
            if (!string.IsNullOrWhiteSpace(name)) { d.PatientName = name; changed = true; }
        }
        if (string.IsNullOrWhiteSpace(d.DoctorName))
        {
            var name = await ResolveDoctorNameQuickAsync(d.DoctorId);
            if (!string.IsNullOrWhiteSpace(name)) { d.DoctorName = name; changed = true; }
        }
        if (changed)
        {
            await _db.SaveChangesAsync();
        }
    }

    [HttpPost("admin/backfill-names")]
    [Authorize]
    public async Task<ActionResult<BackfillNamesResult>> BackfillNames([FromQuery] int batchSize = 200)
    {
        if (batchSize <= 0) batchSize = 100;
        if (batchSize > 1000) batchSize = 1000;

        var docs = await _db.Documents
            .Where(d => d.PatientName == null || d.DoctorName == null)
            .OrderBy(d => d.CreatedAt)
            .Take(batchSize)
            .ToListAsync();

        int updated = 0, skipped = 0;
        foreach (var d in docs)
        {
            var (changed, wasSkipped) = await BackfillNamesForDocAsync(d);
            if (changed) updated++;
            if (wasSkipped) skipped++;
        }

        if (updated > 0)
        {
            await _db.SaveChangesAsync();
        }

        var remaining = await _db.Documents.CountAsync(d => d.PatientName == null || d.DoctorName == null);
        return Ok(new BackfillNamesResult(docs.Count, updated, skipped, remaining));
    }

    [HttpPost("admin/set-names")]
    [Authorize]
    public async Task<ActionResult<BackfillNamesResult>> SetNames([FromBody] SetNamesRequest req)
    {
        var validationError = ValidateSetNames(req);
        if (validationError != null) return BadRequest(validationError);

        var docs = await FilterDocuments(req).ToListAsync();
        var updated = docs.Count(d => ApplyNameUpdates(d, req));
        if (updated > 0) await _db.SaveChangesAsync();
        var remaining = await _db.Documents.CountAsync(d => d.PatientName == null || d.DoctorName == null);
        return Ok(new BackfillNamesResult(docs.Count, updated, docs.Count - updated, remaining));
    }

    private static string? ValidateSetNames(SetNamesRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.PatientId) && string.IsNullOrWhiteSpace(req.DoctorId))
            return "Provide at least PatientId or DoctorId";
        if (string.IsNullOrWhiteSpace(req.PatientName) && string.IsNullOrWhiteSpace(req.DoctorName))
            return "Provide at least PatientName or DoctorName to update";
        return null;
    }

    private IQueryable<DocumentsService.Models.Document> FilterDocuments(SetNamesRequest req)
    {
        IQueryable<DocumentsService.Models.Document> q = _db.Documents;
        if (!string.IsNullOrWhiteSpace(req.PatientId) && Guid.TryParse(req.PatientId, out var pid)) q = q.Where(d => d.PatientId == pid);
        if (!string.IsNullOrWhiteSpace(req.DoctorId) && Guid.TryParse(req.DoctorId, out var did)) q = q.Where(d => d.DoctorId == did);
        return q;
    }

    private static bool ApplyNameUpdates(DocumentsService.Models.Document d, SetNamesRequest req)
    {
        bool changed = false;
        if (!string.IsNullOrWhiteSpace(req.PatientName) && !string.Equals(d.PatientName, req.PatientName, StringComparison.Ordinal))
        { d.PatientName = req.PatientName; changed = true; }
        if (!string.IsNullOrWhiteSpace(req.DoctorName) && !string.Equals(d.DoctorName, req.DoctorName, StringComparison.Ordinal))
        { d.DoctorName = req.DoctorName; changed = true; }
        return changed;
    }

    private async Task<(bool changed, bool skipped)> BackfillNamesForDocAsync(DocumentsService.Models.Document d)
    {
        var beforePatient = d.PatientName;
        var beforeDoctor = d.DoctorName;

        if (beforePatient == null)
            d.PatientName = await ResolvePatientNameAsync(d.PatientId); // Direct call
        if (beforeDoctor == null)
            d.DoctorName = await ResolveDoctorNameAsync(d.DoctorId); // Direct call

        var changed = !string.Equals(beforePatient, d.PatientName, StringComparison.Ordinal) ||
                      !string.Equals(beforeDoctor, d.DoctorName, StringComparison.Ordinal);
        return (changed, !changed);
    }

    private static Dictionary<string, object?> BuildPdfPayload(DocumentsService.Models.Document d)
    {
        var kind = (DocumentKind)d.Type;
        var baseObj = new Dictionary<string, object?>
        {
            ["DocumentId"] = d.Id,
            ["CreatedAt"] = d.CreatedAt,
            ["PatientId"] = d.PatientId,
            ["DoctorId"] = d.DoctorId,
            ["PatientName"] = d.PatientName,
            ["DoctorName"] = d.DoctorName,
            ["Notes"] = d.Notes,
            ["Type"] = kind.ToString()
        };
        switch (kind)
        {
            case DocumentKind.Prescription:
                baseObj["Prescription"] = new
                {
                    d.Prescription!.Medication,
                    d.Prescription!.Dosage,
                    d.Prescription!.Frequency,
                    d.Prescription!.DurationDays,
                    d.Prescription!.Instructions,
                    d.Prescription!.AtcCode,
                    d.Prescription!.AtcName
                };
                break;
            case DocumentKind.Referral:
                baseObj["Referral"] = new
                {
                    d.Referral!.Speciality,
                    d.Referral!.ReferredTo,
                    d.Referral!.ValidFrom,
                    d.Referral!.ValidTo,
                    d.Referral!.Reason,
                    d.Referral!.UrgencyLevel
                };
                break;
            case DocumentKind.SickLeave:
                baseObj["SickLeave"] = new
                {
                    d.SickLeave!.StartDate,
                    d.SickLeave!.EndDate,
                    d.SickLeave!.DaysOff,
                    d.SickLeave!.WorkRestrictions
                };
                break;
            case DocumentKind.VisitNote:
                baseObj["Visit"] = new
                {
                    d.VisitDocument!.Symptoms,
                    d.VisitDocument!.Findings,
                    d.VisitDocument!.Diagnosis,
                    d.VisitDocument!.Recommendations,
                    d.VisitDocument!.FollowUpDate
                };
                break;
            case DocumentKind.LabResults:
                if (d.LabResults != null)
                {
                    baseObj["LabResults"] = new
                    {
                        d.LabResults.TestType,
                        d.LabResults.TestDate,
                        d.LabResults.Laboratory,
                        d.LabResults.Interpretation,
                        Results = d.LabResults.Results.Select(r =>
                        {
                            var status = r.Status ?? "Normal";
                            if (r.IsAbnormal == true)
                                status = string.IsNullOrWhiteSpace(r.Status) ? "Abnormal" : r.Status!;
                            return new
                            {
                                Parameter = r.ParameterName,
                                Value = (r.NumericValue?.ToString() ?? r.Value),
                                r.Unit,
                                ReferenceRange = r.ReferenceRange,
                                Status = status
                            };
                        }).ToList()
                    };
                }
                break;
        }
        return baseObj;
    }

    private async Task<byte[]?> RequestPdfOverRabbitAsync(object payload, string corrId, CancellationToken ct)
    {
        await using var channel = await _rabbitConn.CreateChannelAsync(cancellationToken: ct);
        
        const string requestQueue = "pdf.generate.document";
        
        await channel.QueueDeclareAsync(requestQueue, durable: true, exclusive: false, autoDelete: false, cancellationToken: ct);

        var replyQueueResult = await channel.QueueDeclareAsync(queue: "", durable: false, exclusive: true, autoDelete: true, cancellationToken: ct);
        var replyQueue = replyQueueResult.QueueName;
        
        var tcs = new TaskCompletionSource<byte[]>(TaskCreationOptions.RunContinuationsAsynchronously);
        
        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            try
            {
                var incomingCorrId = ea.BasicProperties.CorrelationId;
                if (incomingCorrId == corrId)
                {
                    tcs.TrySetResult(ea.Body.ToArray());
                }
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
            await Task.CompletedTask;
        };
        
        await channel.BasicConsumeAsync(consumer: consumer, queue: replyQueue, autoAck: true, cancellationToken: ct);

        var props = new BasicProperties
        {
            ReplyTo = replyQueue,
            CorrelationId = corrId,
            ContentType = "application/json"
        };

        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { PropertyNamingPolicy = null });
        var body = Encoding.UTF8.GetBytes(json);
        
        await channel.BasicPublishAsync(
            exchange: string.Empty, 
            routingKey: requestQueue, 
            mandatory: false,
            basicProperties: props, 
            body: body,
            cancellationToken: ct);

        // Wait for response with timeout
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(30));
        
        try 
        {
            var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(Timeout.Infinite, cts.Token));
            if (completedTask == tcs.Task) 
            {
                return await tcs.Task;
            }
        }
        catch (OperationCanceledException) 
        {
            return null;
        }

        return null;
    }

    private static string CatalogBase(HttpContext ctx)
        => ctx.RequestServices.GetService<IConfiguration>()?["CATALOG_SERVICE_BASE_URL"]
           ?? "http://medical-catalog-service:8083";

    private static string PractitionerBase(HttpContext ctx)
        => ctx.RequestServices.GetService<IConfiguration>()?["PRACTITIONER_SERVICE_BASE_URL"]
           ?? "http://practitioner-service:8081";
    private static string ArchiveBase(HttpContext ctx)
        => ctx.RequestServices.GetService<IConfiguration>()?["ARCHIVE_SERVICE_BASE_URL"]
           ?? "http://archive-service:8091";

    private static string PatientBase(HttpContext ctx)
        => ctx.RequestServices.GetService<IConfiguration>()?["PATIENT_SERVICE_BASE_URL"]
           ?? "http://patient-service:8082";

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

    private sealed record DoctorDirectoryDto(string DoctorId, string UserId, string FirstName, string LastName);
    private sealed record ArchivedDoctorDto(Guid DoctorId, string? FullName);
    private sealed record PatientOverviewDto(string PatientId, string UserId, string? FirstName, string? LastName);

    private async Task<string?> ResolveDoctorNameAsync(Guid doctorId)
    {
        try
        {
            using var http = new HttpClient { BaseAddress = new Uri(PractitionerBase(HttpContext)) };
            var dto = await http.GetFromJsonAsync<DoctorDirectoryDto>($"/api/practitioner/doctors/{Uri.EscapeDataString(doctorId.ToString())}/directory");
            if (dto == null) return null;
            var full = ($"{dto.FirstName} {dto.LastName}").Trim();
            return string.IsNullOrWhiteSpace(full) ? null : full;
        }
        catch
        {
            try
            {
                using var http2 = new HttpClient { BaseAddress = new Uri(ArchiveBase(HttpContext)) };
                var dto2 = await http2.GetFromJsonAsync<ArchivedDoctorDto>($"/archive/doctors/{Uri.EscapeDataString(doctorId.ToString())}");
                if (dto2 == null) return null;
                var full = ($"{dto2.FullName}").Trim();
                return string.IsNullOrWhiteSpace(full) ? null : full;
            }
            catch { return null; }
        }
    }

    private async Task<string?> ResolvePatientNameAsync(Guid patientId)
    {
        try
        {
            using var http = new HttpClient { BaseAddress = new Uri(PatientBase(HttpContext)) };
            var dto = await http.GetFromJsonAsync<PatientOverviewDto>($"/api/patient/overview/{Uri.EscapeDataString(patientId.ToString())}");
            if (dto == null) return null;
            var full = ($"{dto.FirstName} {dto.LastName}").Trim();
            return string.IsNullOrWhiteSpace(full) ? null : full;
        }
        catch { return null; }
    }

    private async Task<string?> ResolveDoctorNameQuickAsync(Guid doctorId)
    {
        try
        {
            using var http = new HttpClient { BaseAddress = new Uri(PractitionerBase(HttpContext)), Timeout = TimeSpan.FromSeconds(1) };
            var dto = await http.GetFromJsonAsync<DoctorDirectoryDto>($"/api/practitioner/doctors/{Uri.EscapeDataString(doctorId.ToString())}/directory");
            if (dto == null) return null;
            var full = ($"{dto.FirstName} {dto.LastName}").Trim();
            return string.IsNullOrWhiteSpace(full) ? null : full;
        }
        catch
        {
            try
            {
                using var http2 = new HttpClient { BaseAddress = new Uri(ArchiveBase(HttpContext)), Timeout = TimeSpan.FromSeconds(1) };
                var dto2 = await http2.GetFromJsonAsync<ArchivedDoctorDto>($"/archive/doctors/{Uri.EscapeDataString(doctorId.ToString())}");
                if (dto2 == null) return null;
                var full = ($"{dto2.FullName}").Trim();
                return string.IsNullOrWhiteSpace(full) ? null : full;
            }
            catch { return null; }
        }
    }

    private async Task<string?> ResolvePatientNameQuickAsync(Guid patientId)
    {
        try
        {
            using var http = new HttpClient { BaseAddress = new Uri(PatientBase(HttpContext)), Timeout = TimeSpan.FromSeconds(1) };
            var dto = await http.GetFromJsonAsync<PatientOverviewDto>($"/api/patient/overview/{Uri.EscapeDataString(patientId.ToString())}");
            if (dto == null) return null;
            var full = ($"{dto.FirstName} {dto.LastName}").Trim();
            return string.IsNullOrWhiteSpace(full) ? null : full;
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
    Guid PatientId,
    Guid DoctorId,
    string? Notes,
    Guid? DocumentTypeId,
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

public record AssignRequest(Guid AppointmentId);

public record BackfillNamesResult(int Processed, int Updated, int Skipped, int Remaining);
public record SetNamesRequest(string? PatientId, string? DoctorId, string? PatientName, string? DoctorName);

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
