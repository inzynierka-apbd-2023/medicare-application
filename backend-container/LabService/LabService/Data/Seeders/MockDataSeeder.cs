using Microsoft.EntityFrameworkCore;
using LabService.Models;

namespace LabService.Data.Seeders;

public static class MockIds
{
    public static readonly Guid Patient1 = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000001");
    public static readonly Guid Patient2 = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000002");
    public static readonly Guid Patient3 = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000003");
    public static readonly Guid Patient4 = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000004");
    public static readonly Guid Patient5 = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000005");
    public static readonly Guid Patient6 = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000006");
    public static readonly Guid Patient7 = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000007");

    public static readonly Guid Doctor1 = Guid.Parse("bbbbbbbb-0002-0002-0002-000000000001");
    public static readonly Guid Doctor2 = Guid.Parse("bbbbbbbb-0002-0002-0002-000000000002");
    public static readonly Guid Doctor3 = Guid.Parse("bbbbbbbb-0002-0002-0002-000000000003");
    public static readonly Guid Doctor4 = Guid.Parse("bbbbbbbb-0002-0002-0002-000000000004");
    public static readonly Guid Doctor5 = Guid.Parse("bbbbbbbb-0002-0002-0002-000000000005");
    public static readonly Guid Doctor6 = Guid.Parse("bbbbbbbb-0002-0002-0002-000000000006");
    public static readonly Guid Doctor7 = Guid.Parse("bbbbbbbb-0002-0002-0002-000000000007");

    public static readonly Guid Record1 = Guid.Parse("bbbb1111-1111-1111-1111-000000000001");
    public static readonly Guid Record2 = Guid.Parse("bbbb1111-1111-1111-1111-000000000002");
    public static readonly Guid Record3 = Guid.Parse("bbbb1111-1111-1111-1111-000000000003");
    public static readonly Guid Record4 = Guid.Parse("bbbb1111-1111-1111-1111-000000000004");
    public static readonly Guid Record5 = Guid.Parse("bbbb1111-1111-1111-1111-000000000005");
    public static readonly Guid Record6 = Guid.Parse("bbbb1111-1111-1111-1111-000000000006");
    public static readonly Guid Record7 = Guid.Parse("bbbb1111-1111-1111-1111-000000000007");

    public static readonly Guid LabOrder1 = Guid.Parse("ffff1111-1111-1111-1111-000000000001");
    public static readonly Guid LabOrder2 = Guid.Parse("ffff1111-1111-1111-1111-000000000002");
    public static readonly Guid LabOrder3 = Guid.Parse("ffff1111-1111-1111-1111-000000000003");
    public static readonly Guid LabOrder4 = Guid.Parse("ffff1111-1111-1111-1111-000000000004");
    public static readonly Guid LabOrder5 = Guid.Parse("ffff1111-1111-1111-1111-000000000005");
    public static readonly Guid LabOrder6 = Guid.Parse("ffff1111-1111-1111-1111-000000000006");
    public static readonly Guid LabOrder7 = Guid.Parse("ffff1111-1111-1111-1111-000000000007");

    public static readonly Guid LabTest1 = Guid.Parse("1111ffff-1111-1111-1111-000000000001");
    public static readonly Guid LabTest2 = Guid.Parse("1111ffff-1111-1111-1111-000000000002");
    public static readonly Guid LabTest3 = Guid.Parse("1111ffff-1111-1111-1111-000000000003");
    public static readonly Guid LabTest4 = Guid.Parse("1111ffff-1111-1111-1111-000000000004");
    public static readonly Guid LabTest5 = Guid.Parse("1111ffff-1111-1111-1111-000000000005");
    public static readonly Guid LabTest6 = Guid.Parse("1111ffff-1111-1111-1111-000000000006");
    public static readonly Guid LabTest7 = Guid.Parse("1111ffff-1111-1111-1111-000000000007");

    public static readonly Guid[] AllPatientIds = { Patient1, Patient2, Patient3, Patient4, Patient5, Patient6, Patient7 };
    public static readonly Guid[] AllDoctorIds = { Doctor1, Doctor2, Doctor3, Doctor4, Doctor5, Doctor6, Doctor7 };
    public static readonly Guid[] AllRecordIds = { Record1, Record2, Record3, Record4, Record5, Record6, Record7 };
    public static readonly Guid[] AllLabOrderIds = { LabOrder1, LabOrder2, LabOrder3, LabOrder4, LabOrder5, LabOrder6, LabOrder7 };
    public static readonly Guid[] AllLabTestIds = { LabTest1, LabTest2, LabTest3, LabTest4, LabTest5, LabTest6, LabTest7 };
}

public static class MockDataSeeder
{
    public static async Task SeedAsync(LabDbContext db)
    {
        int created = 0;

        var orderData = new[]
        {
            (MockIds.LabOrder1, MockIds.Patient1, MockIds.Doctor1, MockIds.Record1, "Ordered", "Annual checkup - routine blood work", "Normal"),
            (MockIds.LabOrder2, MockIds.Patient2, MockIds.Doctor2, MockIds.Record2, "Completed", "Pre-operative evaluation", "High"),
            (MockIds.LabOrder3, MockIds.Patient3, MockIds.Doctor3, MockIds.Record3, "InProgress", "Skin allergy panel", "Normal"),
            (MockIds.LabOrder4, MockIds.Patient4, MockIds.Doctor4, MockIds.Record4, "Completed", "Pediatric wellness panel", "Normal"),
            (MockIds.LabOrder5, MockIds.Patient5, MockIds.Doctor5, MockIds.Record5, "Completed", "Sports physical - drug screen", "Normal"),
            (MockIds.LabOrder6, MockIds.Patient6, MockIds.Doctor6, MockIds.Record6, "Ordered", "Comprehensive metabolic panel", "Urgent"),
            (MockIds.LabOrder7, MockIds.Patient7, MockIds.Doctor7, MockIds.Record7, "Collected", "Thyroid function tests", "High")
        };

        var existingOrderIds = await db.LabOrders.Select(o => o.Id).ToHashSetAsync();
        foreach (var (id, patientId, doctorId, recordId, status, notes, priority) in orderData)
        {
            if (!existingOrderIds.Contains(id))
            {
                db.LabOrders.Add(new LabOrder
                {
                    Id = id,
                    PatientId = patientId,
                    OrderingDoctorId = doctorId,
                    MedicalRecordId = recordId,
                    OrderedDate = DateTime.UtcNow.AddDays(-7 + Array.IndexOf(MockIds.AllLabOrderIds, id)),
                    Status = status,
                    ClinicalNotes = notes,
                    Priority = priority,
                    CollectedAt = status != "Ordered" ? DateTime.UtcNow.AddDays(-5 + Array.IndexOf(MockIds.AllLabOrderIds, id)) : null,
                    CreatedAt = DateTime.UtcNow.AddDays(-7),
                    UpdatedAt = DateTime.UtcNow
                });
                created++;
            }
        }

        if (created > 0)
        {
            await db.SaveChangesAsync();
        }

        var testData = new (Guid testId, Guid orderId, string loincCode, string testName, string status)[]
        {
            (MockIds.LabTest1, MockIds.LabOrder1, "718-7", "Hemoglobin", "Completed"),
            (MockIds.LabTest2, MockIds.LabOrder1, "4544-3", "Hematocrit", "Completed"),
            (MockIds.LabTest3, MockIds.LabOrder2, "2160-0", "Creatinine", "Completed"),
            (MockIds.LabTest4, MockIds.LabOrder3, "6718-7", "Allergen Panel IgE", "InProgress"),
            (MockIds.LabTest5, MockIds.LabOrder4, "2345-7", "Glucose", "Completed"),
            (MockIds.LabTest6, MockIds.LabOrder5, "3016-3", "TSH", "Completed"),
            (MockIds.LabTest7, MockIds.LabOrder7, "3024-7", "Free T4", "Pending")
        };

        var existingTestIds = await db.LabTests.Select(t => t.Id).ToHashSetAsync();
        foreach (var (testId, orderId, loincCode, testName, status) in testData)
        {
            if (!existingTestIds.Contains(testId))
            {
                var startedAt = status != "Pending" ? DateTime.UtcNow.AddDays(-4) : (DateTime?)null;
                var completedAt = status == "Completed" ? DateTime.UtcNow.AddDays(-2) : (DateTime?)null;

                db.LabTests.Add(new LabTest
                {
                    Id = testId,
                    LabOrderId = orderId,
                    LoincCode = loincCode,
                    TestName = testName,
                    Status = status,
                    Instructions = $"Standard lab protocol for {testName}",
                    StartedAt = startedAt,
                    CompletedAt = completedAt,
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                });
                created++;
            }
        }

        if (created > 0)
        {
            await db.SaveChangesAsync();
        }

        var resultData = new (Guid testId, Guid patientId, string value, string unit, string range, string flag, string reviewStatus)[]
        {
            (MockIds.LabTest1, MockIds.Patient1, "14.5", "g/dL", "12.0-17.5", "Normal", "Reviewed"),
            (MockIds.LabTest2, MockIds.Patient1, "43.2", "%", "36-50", "Normal", "Reviewed"),
            (MockIds.LabTest3, MockIds.Patient2, "1.0", "mg/dL", "0.7-1.3", "Normal", "Reviewed"),
            (MockIds.LabTest5, MockIds.Patient4, "95", "mg/dL", "70-100", "Normal", "Reviewed"),
            (MockIds.LabTest6, MockIds.Patient5, "2.5", "mIU/L", "0.4-4.0", "Normal", "Pending"),
            (MockIds.LabTest7, MockIds.Patient7, "0.8", "ng/dL", "0.8-1.8", "Low", "Pending")
        };

        var existingResultTestIds = await db.LabResults.Select(r => r.LabTestId).ToHashSetAsync();
        foreach (var (testId, patientId, value, unit, range, flag, reviewStatus) in resultData)
        {
            if (!existingResultTestIds.Contains(testId))
            {
                var isReviewed = reviewStatus == "Reviewed";
                var reviewedByDoctorId = isReviewed ? MockIds.AllDoctorIds[Array.IndexOf(MockIds.AllLabTestIds, testId) % 7] : (Guid?)null;
                
                db.LabResults.Add(new LabResult
                {
                    Id = Guid.NewGuid(),
                    LabTestId = testId,
                    PatientId = patientId,
                    Value = value,
                    Unit = unit,
                    ReferenceRange = range,
                    Flag = flag,
                    Comments = flag == "Normal" ? "Within expected range" : "Requires clinical attention",
                    ResultDate = DateTime.UtcNow.AddDays(-2),
                    ReviewedByDoctorId = reviewedByDoctorId,
                    ReviewedAt = isReviewed ? DateTime.UtcNow.AddDays(-1) : null,
                    ReviewStatus = reviewStatus,
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                });
                created++;
            }
        }

        var reviewData = new (Guid testId, Guid reviewingDoctorId, string reviewStatus, string notes, string recommendations)[]
        {
            (MockIds.LabTest1, MockIds.Doctor1, "Reviewed", "Hemoglobin levels are within normal range.", "Continue current health regimen."),
            (MockIds.LabTest2, MockIds.Doctor1, "Reviewed", "Hematocrit is normal.", "No action needed."),
            (MockIds.LabTest3, MockIds.Doctor2, "Reviewed", "Kidney function is excellent.", "Patient cleared for surgery."),
            (MockIds.LabTest5, MockIds.Doctor4, "Reviewed", "Blood glucose is optimal for child's age.", "Continue healthy diet."),
            (MockIds.LabTest6, MockIds.Doctor5, "RequiresFollowUp", "TSH is normal but borderline. Recommend recheck in 6 months.", "Schedule follow-up testing.")
        };

        var existingReviewTestIds = await db.LabResultReviews.Select(r => r.LabResultId).ToHashSetAsync();
        var testToResultMap = await db.LabResults
            .Select(r => new { r.LabTestId, r.Id })
            .ToDictionaryAsync(x => x.LabTestId, x => x.Id);

        foreach (var (testId, reviewingDoctorId, reviewStatus, notes, recommendations) in reviewData)
        {
            if (testToResultMap.TryGetValue(testId, out var resultId) && !existingReviewTestIds.Contains(resultId))
            {
                db.LabResultReviews.Add(new LabResultReview
                {
                    Id = Guid.NewGuid(),
                    LabResultId = resultId,
                    ReviewedByDoctorId = reviewingDoctorId,
                    ReviewedAt = DateTime.UtcNow.AddDays(-1),
                    ReviewStatus = reviewStatus,
                    ReviewNotes = notes,
                    Recommendations = recommendations,
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                });
                created++;
            }
        }

        if (created > 0)
        {
            await db.SaveChangesAsync();
        }
    }
}
