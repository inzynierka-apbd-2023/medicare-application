namespace DocumentsService.Contracts;

public record DocumentCreated(Guid DocumentId, Guid PatientId, Guid DoctorId, int Type, DateTime CreatedAt);
public record VisitNoteAdded(Guid DocumentId);
public record PrescriptionIssued(Guid DocumentId, string? AtcCode, string? Medication);
public record LabResultsPosted(Guid DocumentId, int ResultCount);
public record DocumentAssignedToAppointment(Guid DocumentId, Guid AppointmentId);
public record ReferralAdded(Guid DocumentId);
public record SickLeaveAdded(Guid DocumentId);
