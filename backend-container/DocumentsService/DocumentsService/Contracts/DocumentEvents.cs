namespace DocumentsService.Contracts;

public record DocumentCreated(string DocumentId, string PatientId, string DoctorId, int Type, DateTime CreatedAt);
public record VisitNoteAdded(string DocumentId);
public record PrescriptionIssued(string DocumentId, string? AtcCode, string? Medication);
public record LabResultsPosted(string DocumentId, int ResultCount);
public record DocumentAssignedToAppointment(string DocumentId, string AppointmentId);
public record ReferralAdded(string DocumentId);
public record SickLeaveAdded(string DocumentId);
