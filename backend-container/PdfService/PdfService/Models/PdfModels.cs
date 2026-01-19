namespace PdfService.Models;

public sealed record PrescriptionDto(string? Medication, string? Dosage, string? Frequency, int? DurationDays, string? Instructions, string? AtcCode, string? AtcName);
public sealed record ReferralDto(string? Speciality, string? ReferredTo, DateTime? ValidFrom, DateTime? ValidTo, string? Reason, string? UrgencyLevel);
public sealed record SickLeaveDto(string? StartDate, string? EndDate, int? DaysOff, string? WorkRestrictions);
public sealed record VisitDto(string? Symptoms, string? Findings, string? Diagnosis, string? Recommendations, string? FollowUpDate);
public sealed record LabResultsDto(string? TestType, DateTime? TestDate, string? Laboratory, string? Interpretation, List<LabResultItemDto>? Results);
public sealed record LabResultItemDto(string? Parameter, string? Value, string? Unit, string? ReferenceRange, string? Status);
