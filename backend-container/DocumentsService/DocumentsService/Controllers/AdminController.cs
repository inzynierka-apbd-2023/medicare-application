using DocumentsService.Data;
using DocumentsService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DocumentsService.Controllers;

[ApiController]
[Route("api/documents/admin")]
public class AdminController : ControllerBase
{
    private readonly DocumentsDbContext _db;
    private readonly IWebHostEnvironment _env;
    public AdminController(DocumentsDbContext db, IWebHostEnvironment env)
    {
        _db = db; _env = env;
    }

    [HttpPost("purge-and-seed")]
    [Authorize]
    public async Task<ActionResult> PurgeAndSeed()
    {
        if (_env.IsProduction()) return Forbid("Not allowed in production.");

        var strategy = _db.Database.CreateExecutionStrategy();
        try
        {
            await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _db.Database.BeginTransactionAsync();
                try
                {
                    // Purge in dependency order
                    await _db.Database.ExecuteSqlRawAsync("DELETE FROM documents.Documents_Assigned;");
                    await _db.Database.ExecuteSqlRawAsync("DELETE FROM documents.Lab_Test_Result;");
                    await _db.Database.ExecuteSqlRawAsync("DELETE FROM documents.Lab_Results;");
                    await _db.Database.ExecuteSqlRawAsync("DELETE FROM documents.Prescription;");
                    await _db.Database.ExecuteSqlRawAsync("DELETE FROM documents.Referral;");
                    await _db.Database.ExecuteSqlRawAsync("DELETE FROM documents.Sick_Leave;");
                    await _db.Database.ExecuteSqlRawAsync("DELETE FROM documents.Visit_Document;");
                    await _db.Database.ExecuteSqlRawAsync("DELETE FROM documents.[Document];");
                    await _db.Database.ExecuteSqlRawAsync("DELETE FROM documents.Lab_Test_Type;");

                    await SeedAsync(_db);

                    await tx.CommitAsync();
                }
                catch
                {
                    await tx.RollbackAsync();
                    throw;
                }
            });
            return NoContent();
        }
        catch (Exception ex)
        {
            return Problem($"Purge/seed failed: {ex.Message}");
        }
    }

    [HttpPost("purge-documents")]
    [Authorize]
    public async Task<ActionResult> PurgeDocuments()
    {
        if (_env.IsProduction()) return Forbid("Not allowed in production.");

        var strategy = _db.Database.CreateExecutionStrategy();
        try
        {
            await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _db.Database.BeginTransactionAsync();
                try
                {
                    // Purge only document data; keep Document_Type and Lab_Test_Type
                    await _db.Database.ExecuteSqlRawAsync("DELETE FROM documents.Documents_Assigned;");
                    await _db.Database.ExecuteSqlRawAsync("DELETE FROM documents.Lab_Test_Result;");
                    await _db.Database.ExecuteSqlRawAsync("DELETE FROM documents.Lab_Results;");
                    await _db.Database.ExecuteSqlRawAsync("DELETE FROM documents.Prescription;");
                    await _db.Database.ExecuteSqlRawAsync("DELETE FROM documents.Referral;");
                    await _db.Database.ExecuteSqlRawAsync("DELETE FROM documents.Sick_Leave;");
                    await _db.Database.ExecuteSqlRawAsync("DELETE FROM documents.Visit_Document;");
                    await _db.Database.ExecuteSqlRawAsync("DELETE FROM documents.[Document];");
                    await tx.CommitAsync();
                }
                catch
                {
                    await tx.RollbackAsync();
                    throw;
                }
            });
            return NoContent();
        }
        catch (Exception ex)
        {
            return Problem($"Purge failed: {ex.Message}");
        }
    }

    private static async Task SeedAsync(DocumentsDbContext db)
    {
        // Ensure required document types exist (if migration seeding didn’t run)
        async Task<Guid> EnsureTypeAsync(string code, string name)
        {
            var t = await db.DocumentTypes.FirstOrDefaultAsync(x => x.Code == code);
            if (t == null)
            {
                t = new DocumentType { Code = code, Name = name };
                db.DocumentTypes.Add(t);
                await db.SaveChangesAsync();
            }
            return t.Id;
        }

        var visitTypeId = await EnsureTypeAsync("VISIT_NOTE", "Visit Note");
        var rxTypeId = await EnsureTypeAsync("PRESCRIPTION", "Prescription");
        var labTypeId = await EnsureTypeAsync("LAB_RESULTS", "Lab Results");
        var referralTypeId = await EnsureTypeAsync("REFERRAL", "Referral");
        var sickTypeId = await EnsureTypeAsync("SICK_LEAVE", "Sick Leave");

        var patientId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();

        // Visit note
        var visit = new Document
        {
            PatientId = patientId,
            DoctorId = doctorId,
            DocumentTypeId = visitTypeId,
            Type = (int)DocumentKind.VisitNote,
            Notes = "Initial consultation for fatigue and headache"
        };
        db.Documents.Add(visit);
        await db.SaveChangesAsync();
        db.VisitDocuments.Add(new VisitDocument
        {
            DocumentId = visit.Id,
            Symptoms = "Headache, fatigue",
            Findings = "BP 128/82, HR 74",
            Diagnosis = "G44.2 – Tension-type headache", // ICD-10 code + description
            Recommendations = "Hydration, rest",
            VitalSignsJson = "{\"BP\":\"128/82\",\"HR\":74}",
            TreatmentPlan = "OTC analgesic",
            FollowUpDate = DateTime.UtcNow.Date.AddDays(7)
        });

        // Prescription (Paracetamol as example)
        var rxDoc = new Document
        {
            PatientId = patientId,
            DoctorId = doctorId,
            DocumentTypeId = rxTypeId,
            Type = (int)DocumentKind.Prescription,
            Notes = "Analgesic for headache"
        };
        db.Documents.Add(rxDoc);
        await db.SaveChangesAsync();
        db.Prescriptions.Add(new Prescription
        {
            DocumentId = rxDoc.Id,
            Medication = "Paracetamol",
            Dosage = "500 mg",
            Frequency = "Every 8 hours",
            DurationDays = 3,
            Instructions = "After meals",
            RefillsRemaining = 0,
            // Optional catalog projection
            AtcCode = "N02BE01",
            AtcName = "Paracetamol"
        });

        // Lab results (CBC snippet)
        var labDoc = new Document
        {
            PatientId = patientId,
            DoctorId = doctorId,
            DocumentTypeId = labTypeId,
            Type = (int)DocumentKind.LabResults,
            Notes = "CBC panel"
        };
        db.Documents.Add(labDoc);
        await db.SaveChangesAsync();
        var results = new LabResults
        {
            DocumentId = labDoc.Id,
            TestType = "CBC",
            TestDate = DateTime.UtcNow.Date.AddDays(-1),
            Laboratory = "Local Lab",
            OverallStatus = "Final",
            Interpretation = "Within normal ranges",
            TechnicianName = "Tech J. Doe"
        };
        db.LabResults.Add(results);

        // Ensure Lab_Test_Type projections for a couple of LOINC codes (common ones)
        async Task<LabTestType> EnsureLoincAsync(string code, string name, string? exampleUnits = null)
        {
            var t = await db.LabTestTypes.FirstOrDefaultAsync(x => x.LoincCode == code);
            if (t == null)
            {
                t = new LabTestType { LoincCode = code, Name = name, ExampleUnits = exampleUnits };
                db.LabTestTypes.Add(t);
                await db.SaveChangesAsync();
            }
            return t;
        }

        var hb = await EnsureLoincAsync("718-7", "Hemoglobin [Mass/volume] in Blood", "g/dL");
        var wbc = await EnsureLoincAsync("6690-2", "Leukocytes [#/volume] in Blood by Automated count", "10^9/L");

        db.LabTestResults.AddRange(
            new LabTestResult
            {
                LabResultsDocumentId = labDoc.Id,
                LabTestTypeId = hb.Id,
                LoincCode = hb.LoincCode,
                ParameterName = "Hemoglobin",
                NumericValue = 13.4m,
                Unit = "g/dL",
                ReferenceRange = "12.0-16.0",
                Status = "Final",
                IsAbnormal = false
            },
            new LabTestResult
            {
                LabResultsDocumentId = labDoc.Id,
                LabTestTypeId = wbc.Id,
                LoincCode = wbc.LoincCode,
                ParameterName = "WBC",
                NumericValue = 6.2m,
                Unit = "10^9/L",
                ReferenceRange = "4.0-11.0",
                Status = "Final",
                IsAbnormal = false
            }
        );

        // Referral and Sick Leave examples
        var referralDoc = new Document
        {
            PatientId = patientId,
            DoctorId = doctorId,
            DocumentTypeId = referralTypeId,
            Type = (int)DocumentKind.Referral,
            Notes = "Refer to neurology for persistent headaches"
        };
        db.Documents.Add(referralDoc);
        await db.SaveChangesAsync();
        db.Referrals.Add(new Referral
        {
            DocumentId = referralDoc.Id,
            Speciality = "Neurology",
            ReferredTo = "Dr. N. Expert",
            ValidFrom = DateTime.UtcNow.Date,
            ValidTo = DateTime.UtcNow.Date.AddMonths(1),
            Reason = "Persistent headaches",
            UrgencyLevel = "Routine"
        });

        var sickDoc = new Document
        {
            PatientId = patientId,
            DoctorId = doctorId,
            DocumentTypeId = sickTypeId,
            Type = (int)DocumentKind.SickLeave,
            Notes = "2 days sick leave"
        };
        db.Documents.Add(sickDoc);
        await db.SaveChangesAsync();
        db.SickLeaves.Add(new SickLeave
        {
            DocumentId = sickDoc.Id,
            StartDate = DateTime.UtcNow.Date,
            EndDate = DateTime.UtcNow.Date.AddDays(2),
            DaysOff = 2,
            ReturnToWorkDate = DateTime.UtcNow.Date.AddDays(3),
            WorkRestrictions = "None"
        });

        await db.SaveChangesAsync();
    }

    public record SeedForPatientRequest(
        Guid PatientId,
        Guid DoctorId,
        Guid? AssignVisitToAppointmentId,
        Guid? AssignPrescriptionToAppointmentId,
        Guid? AssignReferralToAppointmentId,
        Guid? AssignSickLeaveToAppointmentId
    );

    [HttpPost("seed-for-patient")]
    [Authorize]
    public async Task<ActionResult> SeedForPatient([FromBody] SeedForPatientRequest req)
    {
        if (_env.IsProduction()) return Forbid("Not allowed in production.");

        // Ensure doc types
        Guid EnsureId(string code, string name)
        {
            var t = _db.DocumentTypes.FirstOrDefault(x => x.Code == code);
            if (t == null) { t = new DocumentType { Code = code, Name = name }; _db.DocumentTypes.Add(t); _db.SaveChanges(); }
            return t.Id;
        }
        var visitTypeId = EnsureId("VISIT_NOTE", "Visit Note");
        var rxTypeId = EnsureId("PRESCRIPTION", "Prescription");
        var labTypeId = EnsureId("LAB_RESULTS", "Lab Results");
        var referralTypeId = EnsureId("REFERRAL", "Referral");
        var sickTypeId = EnsureId("SICK_LEAVE", "Sick Leave");

        var patientId = req.PatientId;
        var doctorId = req.DoctorId;

        // Visit Note
        var visit = new Document { PatientId = patientId, DoctorId = doctorId, DocumentTypeId = visitTypeId, Type = (int)DocumentKind.VisitNote, Notes = "Visit summary for headache" };
        _db.Documents.Add(visit); await _db.SaveChangesAsync();
        _db.VisitDocuments.Add(new VisitDocument { DocumentId = visit.Id, Symptoms = "Headache, fatigue", Findings = "BP 128/82, HR 74", Diagnosis = "G44.2 – Tension-type headache", Recommendations = "Hydration, rest" });
        if (req.AssignVisitToAppointmentId.HasValue)
            _db.DocumentAssignments.Add(new DocumentAssignment { DocumentId = visit.Id, AppointmentId = req.AssignVisitToAppointmentId.Value });

        // Prescription (ATC aligned)
        var rxDoc = new Document { PatientId = patientId, DoctorId = doctorId, DocumentTypeId = rxTypeId, Type = (int)DocumentKind.Prescription, Notes = "Analgesic prescription" };
        _db.Documents.Add(rxDoc); await _db.SaveChangesAsync();
        _db.Prescriptions.Add(new Prescription { DocumentId = rxDoc.Id, Medication = "Paracetamol", Dosage = "500 mg", Frequency = "Every 8 hours", DurationDays = 3, Instructions = "After meals", AtcCode = "N02BE01", AtcName = "Paracetamol" });
        if (req.AssignPrescriptionToAppointmentId.HasValue)
            _db.DocumentAssignments.Add(new DocumentAssignment { DocumentId = rxDoc.Id, AppointmentId = req.AssignPrescriptionToAppointmentId.Value });

        // Referral
        var refDoc = new Document { PatientId = patientId, DoctorId = doctorId, DocumentTypeId = referralTypeId, Type = (int)DocumentKind.Referral, Notes = "Refer to neurology" };
        _db.Documents.Add(refDoc); await _db.SaveChangesAsync();
        _db.Referrals.Add(new Referral { DocumentId = refDoc.Id, Speciality = "Neurology", ReferredTo = "Dr. N. Expert", ValidFrom = DateTime.UtcNow.Date, ValidTo = DateTime.UtcNow.Date.AddMonths(1), Reason = "Headache", UrgencyLevel = "Routine" });
        if (req.AssignReferralToAppointmentId.HasValue)
            _db.DocumentAssignments.Add(new DocumentAssignment { DocumentId = refDoc.Id, AppointmentId = req.AssignReferralToAppointmentId.Value });

        // Sick Leave
        var slDoc = new Document { PatientId = patientId, DoctorId = doctorId, DocumentTypeId = sickTypeId, Type = (int)DocumentKind.SickLeave, Notes = "Short sick leave" };
        _db.Documents.Add(slDoc); await _db.SaveChangesAsync();
        _db.SickLeaves.Add(new SickLeave { DocumentId = slDoc.Id, StartDate = DateTime.UtcNow.Date, EndDate = DateTime.UtcNow.Date.AddDays(2), DaysOff = 2 });
        if (req.AssignSickLeaveToAppointmentId.HasValue)
            _db.DocumentAssignments.Add(new DocumentAssignment { DocumentId = slDoc.Id, AppointmentId = req.AssignSickLeaveToAppointmentId.Value });

        // Lab Results (LOINC aligned, no appointment by default)
        var labDoc = new Document { PatientId = patientId, DoctorId = doctorId, DocumentTypeId = labTypeId, Type = (int)DocumentKind.LabResults, Notes = "CBC and lipids" };
        _db.Documents.Add(labDoc); await _db.SaveChangesAsync();
        var lab = new LabResults { DocumentId = labDoc.Id, TestType = "CBC & Lipid Panel", TestDate = DateTime.UtcNow.Date.AddDays(-1), Laboratory = "IMUP Medical Laboratory", OverallStatus = "Final", Interpretation = "Slightly elevated LDL, other values normal" };
        _db.LabResults.Add(lab);
        // Ensure Lab_Test_Type projections
        LabTestType EnsureLoinc(string code, string name, string? units)
        {
            var t = _db.LabTestTypes.FirstOrDefault(x => x.LoincCode == code);
            if (t == null) { t = new LabTestType { LoincCode = code, Name = name, ExampleUnits = units }; _db.LabTestTypes.Add(t); _db.SaveChanges(); }
            return t;
        }
        var loincHb = EnsureLoinc("718-7", "Hemoglobin [Mass/volume] in Blood", "g/dL");
        var loincLDL = EnsureLoinc("2089-1", "Cholesterol in LDL [Mass/volume] in Serum or Plasma", "mg/dL");
        _db.LabTestResults.AddRange(
            new LabTestResult { LabResultsDocumentId = labDoc.Id, LabTestTypeId = loincHb.Id, LoincCode = loincHb.LoincCode, ParameterName = "Hemoglobin", NumericValue = 14.2m, Unit = "g/dL", ReferenceRange = "12.0-16.0", Status = "Final", IsAbnormal = false },
            new LabTestResult { LabResultsDocumentId = labDoc.Id, LabTestTypeId = loincLDL.Id, LoincCode = loincLDL.LoincCode, ParameterName = "LDL Cholesterol", NumericValue = 155m, Unit = "mg/dL", ReferenceRange = "<100", Status = "Final", IsAbnormal = true }
        );

        await _db.SaveChangesAsync();
        return NoContent();
    }
}
