namespace DocumentsService.Contracts;

using Medicare.Messaging.Contracts;

public record DocumentCreated(Guid DocumentId, Guid PatientId, Guid DoctorId, int Type, DateTime CreatedAt) : IDocumentCreated;
public record VisitNoteAdded(Guid DocumentId) : IVisitNoteAdded;
public record PrescriptionIssued(Guid DocumentId, string? AtcCode, string? Medication) : IPrescriptionIssued;
public record LabResultsPosted(Guid DocumentId, int ResultCount) : ILabResultsPosted;
public record DocumentAssignedToAppointment(Guid DocumentId, Guid AppointmentId) : IDocumentAssignedToAppointment;
public record ReferralAdded(Guid DocumentId) : IReferralAdded;
public record SickLeaveAdded(Guid DocumentId) : ISickLeaveAdded;
