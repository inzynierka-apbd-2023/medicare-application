namespace AppointmentService.Features.DoctorPatients.DTOs;

/// <summary>
/// DTO representing a patient in the doctor's patient list.
/// Built from appointment data enriched with user profile information.
/// </summary>
public record DoctorPatientDto
{
    public Guid Id { get; init; } // Patient's User ID (for navigation/actions)
    public string Name { get; init; } = "";
    public int Age { get; init; }
    public string Gender { get; init; } = "";
    public DateTime LastVisit { get; init; }
    public int Visits { get; init; }
    public string Notes { get; init; } = "";
    public string? Email { get; init; }
    public string? Phone { get; init; }
}

public record DoctorPatientsResponse
{
    public List<DoctorPatientDto> Patients { get; init; } = new();
    public int TotalCount { get; init; }
}
