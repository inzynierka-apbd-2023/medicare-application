using Microsoft.EntityFrameworkCore;
using DocumentsService.Models;

namespace DocumentsService.Data;

/// <summary>
/// Shared deterministic IDs for cross-service mock data references
/// Patient/Doctor IDs match User IDs from UserService for seamless auth integration
/// </summary>
public static class MockIds
{
    // Patient IDs (matching User IDs from UserService for login integration)
    public static readonly Guid Patient1 = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000001");
    public static readonly Guid Patient2 = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000002");
    public static readonly Guid Patient3 = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000003");
    public static readonly Guid Patient4 = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000004");
    public static readonly Guid Patient5 = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000005");
    public static readonly Guid Patient6 = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000006");
    public static readonly Guid Patient7 = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000007");

    // Doctor IDs (matching User IDs from UserService for login integration)
    public static readonly Guid Doctor1 = Guid.Parse("bbbbbbbb-0002-0002-0002-000000000001");
    public static readonly Guid Doctor2 = Guid.Parse("bbbbbbbb-0002-0002-0002-000000000002");
    public static readonly Guid Doctor3 = Guid.Parse("bbbbbbbb-0002-0002-0002-000000000003");
    public static readonly Guid Doctor4 = Guid.Parse("bbbbbbbb-0002-0002-0002-000000000004");
    public static readonly Guid Doctor5 = Guid.Parse("bbbbbbbb-0002-0002-0002-000000000005");
    public static readonly Guid Doctor6 = Guid.Parse("bbbbbbbb-0002-0002-0002-000000000006");
    public static readonly Guid Doctor7 = Guid.Parse("bbbbbbbb-0002-0002-0002-000000000007");

    // Appointment IDs (from AppointmentService)
    public static readonly Guid Appointment1 = Guid.Parse("55555555-5555-5555-5555-000000000001");
    public static readonly Guid Appointment2 = Guid.Parse("55555555-5555-5555-5555-000000000002");
    public static readonly Guid Appointment3 = Guid.Parse("55555555-5555-5555-5555-000000000003");
    public static readonly Guid Appointment4 = Guid.Parse("55555555-5555-5555-5555-000000000004");
    public static readonly Guid Appointment5 = Guid.Parse("55555555-5555-5555-5555-000000000005");
    public static readonly Guid Appointment6 = Guid.Parse("55555555-5555-5555-5555-000000000006");
    public static readonly Guid Appointment7 = Guid.Parse("55555555-5555-5555-5555-000000000007");

    // Document IDs
    public static readonly Guid Document1 = Guid.Parse("aaaa1111-1111-1111-1111-000000000001");
    public static readonly Guid Document2 = Guid.Parse("aaaa1111-1111-1111-1111-000000000002");
    public static readonly Guid Document3 = Guid.Parse("aaaa1111-1111-1111-1111-000000000003");
    public static readonly Guid Document4 = Guid.Parse("aaaa1111-1111-1111-1111-000000000004");
    public static readonly Guid Document5 = Guid.Parse("aaaa1111-1111-1111-1111-000000000005");
    public static readonly Guid Document6 = Guid.Parse("aaaa1111-1111-1111-1111-000000000006");
    public static readonly Guid Document7 = Guid.Parse("aaaa1111-1111-1111-1111-000000000007");
    public static readonly Guid Document8 = Guid.Parse("aaaa1111-1111-1111-1111-000000000008");

    public static readonly Guid[] AllPatientIds = { Patient1, Patient2, Patient3, Patient4, Patient5, Patient6, Patient7 };
    public static readonly Guid[] AllDoctorIds = { Doctor1, Doctor2, Doctor3, Doctor4, Doctor5, Doctor6, Doctor7 };
    public static readonly Guid[] AllAppointmentIds = { Appointment1, Appointment2, Appointment3, Appointment4, Appointment5, Appointment6, Appointment7 };
    public static readonly Guid[] AllDocumentIds = { Document1, Document2, Document3, Document4, Document5, Document6, Document7 };
}

public static class MockDataSeeder
{
    public static async Task SeedAsync(DocumentsDbContext db)
    {
        int created = 0;

        // Get document types
        var docTypes = await db.DocumentTypes.ToDictionaryAsync(t => t.Code, t => t);
        if (!docTypes.Any())
        {
            Console.WriteLine("[MockDataSeeder] No document types found! Skipping seeding.");
            return;
        }

        var patientNames = new[] { "Alice Johnson", "Bob Smith", "Carol Williams", "David Brown", "Emma Davis", "Frank Miller", "Grace Wilson" };
        var doctorNames = new[] { "Dr. John Carter", "Dr. Sarah Chen", "Dr. Michael Roberts", "Dr. Emily Thompson", "Dr. James Wilson", "Dr. Lisa Anderson", "Dr. Robert Martinez" };

        // Document data: (docId, patientIdx, doctorIdx, typeCode, notes)
        var documentData = new[]
        {
            (MockIds.Document1, 0, 0, "VISIT_NOTE", "Initial consultation - comprehensive health assessment"),
            (MockIds.Document2, 1, 1, "PRESCRIPTION", "Medication prescription for chronic condition management"),
            (MockIds.Document3, 2, 2, "REFERRAL", "Referral to specialist for further evaluation"),
            (MockIds.Document4, 3, 3, "SICK_LEAVE", "Medical certificate for work absence"),
            (MockIds.Document5, 4, 4, "LAB_RESULTS", "Complete blood count and metabolic panel results"),
            (MockIds.Document6, 5, 5, "VISIT_NOTE", "Follow-up visit after procedure"),
            (MockIds.Document6, 5, 5, "VISIT_NOTE", "Follow-up visit after procedure"),
            (MockIds.Document7, 6, 6, "PRESCRIPTION", "Post-operative pain management prescription"),
            // Add Lab Results for Patient1 (index 0) to verify API connection
            (MockIds.Document8, 0, 0, "LAB_RESULTS", "Annual health checkup blood work")
        };

        var existingDocumentIds = await db.Documents.Select(d => d.Id).ToHashSetAsync();

        foreach (var (docId, patientIdx, doctorIdx, typeCode, notes) in documentData)
        {
            if (existingDocumentIds.Contains(docId)) continue;
            if (!docTypes.TryGetValue(typeCode, out var docType)) continue;

            var doc = new Document
            {
                Id = docId,
                PatientId = MockIds.AllPatientIds[patientIdx],
                DoctorId = MockIds.AllDoctorIds[doctorIdx],
                DocumentTypeId = docType.Id,
                Type = typeCode switch
                {
                    "VISIT_NOTE" => (int)DocumentKind.VisitNote,
                    "PRESCRIPTION" => (int)DocumentKind.Prescription,
                    "REFERRAL" => (int)DocumentKind.Referral,
                    "SICK_LEAVE" => (int)DocumentKind.SickLeave,
                    "LAB_RESULTS" => (int)DocumentKind.LabResults,
                    _ => (int)DocumentKind.VisitNote
                },
                Notes = notes,
                PatientName = patientNames[patientIdx],
                DoctorName = doctorNames[doctorIdx],
                CreatedAt = DateTime.UtcNow.AddDays(-14 + patientIdx)
            };
            db.Documents.Add(doc);
            created++;
        }

        if (created > 0)
        {
            await db.SaveChangesAsync();
        }

        // Add type-specific details for each document
        var existingVisitDocs = await db.VisitDocuments.Select(v => v.DocumentId).ToHashSetAsync();
        var existingPrescriptions = await db.Prescriptions.Select(p => p.DocumentId).ToHashSetAsync();
        var existingReferrals = await db.Referrals.Select(r => r.DocumentId).ToHashSetAsync();
        var existingSickLeaves = await db.SickLeaves.Select(s => s.DocumentId).ToHashSetAsync();
        var existingLabResults = await db.LabResults.Select(l => l.DocumentId).ToHashSetAsync();

        // Visit Notes
        if (!existingVisitDocs.Contains(MockIds.Document1))
        {
            db.VisitDocuments.Add(new VisitDocument
            {
                DocumentId = MockIds.Document1,
                Symptoms = "Fatigue, headaches, occasional dizziness",
                Findings = "BP 128/82, HR 72, Temp 98.6F, BMI 24.5",
                Diagnosis = "Mild hypertension, tension headaches",
                Recommendations = "Lifestyle modifications, stress management",
                VitalSignsJson = "{\"bp\":\"128/82\",\"hr\":72,\"temp\":98.6,\"weight\":165}",
                TreatmentPlan = "Low sodium diet, regular exercise, follow-up in 3 months",
                FollowUpDate = DateTime.UtcNow.AddMonths(3)
            });
            created++;
        }

        if (!existingVisitDocs.Contains(MockIds.Document6))
        {
            db.VisitDocuments.Add(new VisitDocument
            {
                DocumentId = MockIds.Document6,
                Symptoms = "Post-operative pain, mild swelling",
                Findings = "Surgical site healing well, no signs of infection",
                Diagnosis = "Normal post-operative recovery",
                Recommendations = "Continue pain management, avoid strenuous activity",
                VitalSignsJson = "{\"bp\":\"118/75\",\"hr\":68,\"temp\":98.2}",
                TreatmentPlan = "Suture removal in 7 days, physical therapy referral",
                FollowUpDate = DateTime.UtcNow.AddDays(7)
            });
            created++;
        }

        // Prescriptions
        if (!existingPrescriptions.Contains(MockIds.Document2))
        {
            db.Prescriptions.Add(new Prescription
            {
                DocumentId = MockIds.Document2,
                Medication = "Metformin",
                Dosage = "500mg",
                Frequency = "Twice daily with meals",
                DurationDays = 90,
                Instructions = "Take with food to minimize GI side effects. Monitor blood glucose levels.",
                PharmacyName = "Medicare Pharmacy",
                PharmacyPhone = "+1-555-7890",
                RefillsRemaining = 3,
                AtcCode = "A10BA02",
                AtcName = "Metformin"
            });
            created++;
        }

        if (!existingPrescriptions.Contains(MockIds.Document7))
        {
            db.Prescriptions.Add(new Prescription
            {
                DocumentId = MockIds.Document7,
                Medication = "Ibuprofen",
                Dosage = "400mg",
                Frequency = "Every 6 hours as needed",
                DurationDays = 14,
                Instructions = "Take with food. Do not exceed 1600mg daily. Avoid if kidney issues.",
                PharmacyName = "City Drug Store",
                PharmacyPhone = "+1-555-4321",
                RefillsRemaining = 1,
                AtcCode = "M01AE01",
                AtcName = "Ibuprofen"
            });
            created++;
        }

        // Referral
        if (!existingReferrals.Contains(MockIds.Document3))
        {
            db.Referrals.Add(new Referral
            {
                DocumentId = MockIds.Document3,
                Speciality = "Cardiology",
                ReferredTo = "Dr. John Carter, Heart Care Center",
                ValidFrom = DateTime.UtcNow,
                ValidTo = DateTime.UtcNow.AddMonths(6),
                Reason = "Suspected arrhythmia, frequent palpitations. Recommend ECG and Holter monitor.",
                UrgencyLevel = "High"
            });
            created++;
        }

        // Sick Leave
        if (!existingSickLeaves.Contains(MockIds.Document4))
        {
            db.SickLeaves.Add(new SickLeave
            {
                DocumentId = MockIds.Document4,
                StartDate = DateTime.UtcNow.AddDays(-3),
                EndDate = DateTime.UtcNow.AddDays(4),
                DaysOff = 7,
                ReturnToWorkDate = DateTime.UtcNow.AddDays(5),
                WorkRestrictions = "No heavy lifting for 2 weeks. Avoid prolonged standing."
            });
            created++;
        }

        // Lab Results
        if (!existingLabResults.Contains(MockIds.Document5))
        {
            var labResult = new LabResults
            {
                DocumentId = MockIds.Document5,
                TestType = "Complete Blood Count (CBC) & Comprehensive Metabolic Panel",
                TestDate = DateTime.UtcNow.AddDays(-2),
                Laboratory = "Medicare Clinical Laboratory",
                OverallStatus = "Final",
                Interpretation = "Results within normal limits. No significant abnormalities detected.",
                ReferenceRanges = "Standard adult reference ranges applied",
                TechnicianName = "Lab Tech Maria Garcia",
                DoctorComments = "All values normal. Continue current health regimen."
            };
            db.LabResults.Add(labResult);
            await db.SaveChangesAsync();

            // Add individual test results
            var testResults = new[]
            {
                ("718-7", "Hemoglobin", "14.5", 14.5m, "g/dL", "12.0-17.5", "Normal", false),
                ("4544-3", "Hematocrit", "43.2", 43.2m, "%", "36-50", "Normal", false),
                ("789-8", "Erythrocytes (RBC)", "4.8", 4.8m, "10^12/L", "4.2-5.9", "Normal", false),
                ("2160-0", "Creatinine", "1.0", 1.0m, "mg/dL", "0.7-1.3", "Normal", false),
                ("2345-7", "Glucose", "95", 95m, "mg/dL", "70-100", "Normal", false),
                ("2951-2", "Sodium", "140", 140m, "mEq/L", "136-145", "Normal", false),
                ("2823-3", "Potassium", "4.2", 4.2m, "mEq/L", "3.5-5.0", "Normal", false)
            };

            foreach (var (loincCode, paramName, value, numValue, unit, refRange, status, isAbnormal) in testResults)
            {
                db.LabTestResults.Add(new LabTestResult
                {
                    Id = Guid.NewGuid(),
                    LabResultsDocumentId = MockIds.Document5,
                    LoincCode = loincCode,
                    ParameterName = paramName,
                    Value = value,
                    NumericValue = numValue,
                    Unit = unit,
                    ReferenceRange = refRange,
                    Status = status,
                    IsAbnormal = isAbnormal
                });
                created++;
            }
        }

        // Lab Results for Document8 (Patient1)
        if (!existingLabResults.Contains(MockIds.Document8))
        {
            var labResult = new LabResults
            {
                DocumentId = MockIds.Document8,
                TestType = "Lipid Panel & Thyroid Function",
                TestDate = DateTime.UtcNow.AddDays(-5),
                Laboratory = "Medicare Central Lab",
                OverallStatus = "Final",
                Interpretation = "Slightly elevated LDL, otherwise normal.",
                ReferenceRanges = "Adult standard",
                TechnicianName = "John Doe",
                DoctorComments = "Please schedule follow-up to discuss diet."
            };
            db.LabResults.Add(labResult);
            await db.SaveChangesAsync();

            var testResults = new[]
            {
                ("2085-9", "HDL Cholesterol", "55", 55m, "mg/dL", ">40", "Normal", false),
                ("2089-1", "LDL Cholesterol", "135", 135m, "mg/dL", "<100", "High", true),
                ("3016-3", "TSH", "2.5", 2.5m, "mIU/L", "0.4-4.0", "Normal", false)
            };

            foreach (var (loincCode, paramName, value, numValue, unit, refRange, status, isAbnormal) in testResults)
            {
                db.LabTestResults.Add(new LabTestResult
                {
                    Id = Guid.NewGuid(),
                    LabResultsDocumentId = MockIds.Document8,
                    LoincCode = loincCode,
                    ParameterName = paramName,
                    Value = value,
                    NumericValue = numValue,
                    Unit = unit,
                    ReferenceRange = refRange,
                    Status = status,
                    IsAbnormal = isAbnormal
                });
                created++;
            }
        }

        // Add Document Assignments (link documents to appointments)
        var existingAssignments = await db.DocumentAssignments
            .Select(a => new { a.DocumentId, a.AppointmentId })
            .ToListAsync();
        var existingAssignmentSet = existingAssignments.Select(x => (x.DocumentId, x.AppointmentId)).ToHashSet();

        var assignments = new[]
        {
            (MockIds.Document1, MockIds.Appointment1),
            (MockIds.Document2, MockIds.Appointment2),
            (MockIds.Document3, MockIds.Appointment3),
            (MockIds.Document4, MockIds.Appointment4),
            (MockIds.Document5, MockIds.Appointment5),
            (MockIds.Document6, MockIds.Appointment6),
            (MockIds.Document6, MockIds.Appointment6),
            (MockIds.Document7, MockIds.Appointment7),
            (MockIds.Document8, MockIds.Appointment1)
        };

        foreach (var (docId, appointmentId) in assignments)
        {
            if (!existingAssignmentSet.Contains((docId, appointmentId)))
            {
                db.DocumentAssignments.Add(new DocumentAssignment
                {
                    Id = Guid.NewGuid(),
                    DocumentId = docId,
                    AppointmentId = appointmentId,
                    AssignedAt = DateTime.UtcNow.AddDays(-7)
                });
                created++;
            }
        }

        if (created > 0)
        {
            await db.SaveChangesAsync();
            Console.WriteLine($"[MockDataSeeder] Created {created} document records (documents, visit notes, prescriptions, referrals, sick leaves, lab results, assignments).");
        }
        else
        {
            Console.WriteLine("[MockDataSeeder] All document mock data already exists.");
        }
    }
}
