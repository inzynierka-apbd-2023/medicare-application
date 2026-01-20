namespace Medicare.Messaging.Contracts;

public interface IAppointmentCreated
{
    Guid AppointmentId { get; }
    Guid PatientId { get; }
    Guid DoctorId { get; }
    DateTime ScheduledAt { get; }
    DateTime OccurredAt { get; }
}

public interface IAppointmentUpdated
{
    Guid AppointmentId { get; }
    Guid DoctorId { get; }
    string Status { get; }
    DateTime UpdatedAt { get; }
    DateTime OccurredAt { get; }
}

public interface IAppointmentRated
{
    Guid AppointmentId { get; }
    Guid DoctorId { get; }
    Guid PatientId { get; }
    int Rating { get; }
    string? Description { get; }
    DateTime OccurredAt { get; }
}


public interface IBillingPaymentInitiated
{
    Guid AppointmentId { get; }
    Guid PatientId { get; }
    string PaymentMethod { get; }
    DateTime Timestamp { get; }
}

public interface IBillingPaymentProcessed
{
    Guid AppointmentId { get; }
    bool IsPaid { get; }
    long AmountCents { get; }
    string? PlanCode { get; }
    string? Error { get; }
}

public interface IGetAppointmentPayments
{
    List<Guid> AppointmentIds { get; }
}

public interface IAppointmentPayments
{
    List<IAppointmentPayment> Payments { get; }
}

public interface IAppointmentPayment
{
    Guid AppointmentId { get; }
    long AmountCents { get; }
    string Status { get; }
}

public interface IDoctorArchived
{
    Guid DoctorId { get; }
    Guid? DoctorUserId { get; }
    DateTime OccurredAt { get; }
    string? FullName { get; }
    string? Email { get; }
    string? Phone { get; }
    string? SnapshotJson { get; }
}

public interface INotificationCreated
{
    Guid RecipientUserId { get; }
    string Description { get; }
    byte Type { get; }
    string SourceService { get; }
    string ActionUrl { get; }
    string PriorityLevel { get; }
    DateTime? ExpiresAt { get; }
}

public interface IGetPatient
{
    Guid PatientId { get; }
}

public interface IGetPatients
{
    List<Guid> PatientIds { get; }
}

public interface IPatientProfiles
{
    List<IPatientProfile> Profiles { get; }
}

public interface IPatientProfile
{
    Guid PatientId { get; }
    string FirstName { get; }
    string LastName { get; }
    string Email { get; }
    string? Phone { get; }
    DateTime? DateOfBirth { get; }
    string? Gender { get; }
    string? AddressLine1 { get; }
    string? AddressLine2 { get; }
    string? City { get; }
    string? State { get; }
    string? ZipCode { get; }
    string? Country { get; }
}

public interface IDocumentCreated
{
    Guid DocumentId { get; }
    Guid PatientId { get; }
    Guid DoctorId { get; }
    int Type { get; }
    DateTime CreatedAt { get; }
}

public interface IVisitNoteAdded { Guid DocumentId { get; } }

public interface IPrescriptionIssued 
{ 
    Guid DocumentId { get; } 
    string? AtcCode { get; } 
    string? Medication { get; } 
}

public interface ILabResultsPosted 
{ 
    Guid DocumentId { get; } 
    int ResultCount { get; } 
}

public interface IDocumentAssignedToAppointment 
{ 
    Guid DocumentId { get; } 
    Guid AppointmentId { get; } 
}

public interface IReferralAdded { Guid DocumentId { get; } }

public interface ISickLeaveAdded { Guid DocumentId { get; } }

public interface IGeneratePdfRequest
{
    Guid DocumentId { get; }
    string DocumentType { get; }
    string PayloadJson { get; }
}

public interface IPdfGeneratedResponse
{
    Guid DocumentId { get; }
    byte[] PdfBytes { get; }
}


public interface IGetDoctor
{
    Guid DoctorId { get; }
}

public interface IGetDoctors
{
    List<Guid> DoctorIds { get; }
}

public interface IDoctorProfiles
{
    List<IDoctorProfile> Profiles { get; }
}

public interface IDoctorProfile
{
    Guid DoctorId { get; }
    Guid UserId { get; }
    string FirstName { get; }
    string LastName { get; }
    string Email { get; }
    string SpecializationNames { get; }
}
