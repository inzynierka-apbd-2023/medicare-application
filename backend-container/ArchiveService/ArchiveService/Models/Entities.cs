namespace ArchiveService.Models;

public class ArchivedDoctor
{
    public Guid DoctorId { get; set; }
    public Guid? UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    // store as JSON to avoid schema coupling
    public string? SpecializationIdsJson { get; set; }
    public DateTime ArchivedAtUtc { get; set; } = DateTime.UtcNow;
    public string? SnapshotJson { get; set; }
}

public class ArchivedDocument
{
    public Guid DocumentId { get; set; }
    public Guid DoctorId { get; set; }
    public Guid? PatientId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? SnapshotJson { get; set; }
    public DateTime ArchivedAtUtc { get; set; } = DateTime.UtcNow;
}
