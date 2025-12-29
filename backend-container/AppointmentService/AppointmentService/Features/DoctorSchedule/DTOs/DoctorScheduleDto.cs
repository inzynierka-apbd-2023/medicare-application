using System.ComponentModel.DataAnnotations;

namespace AppointmentService.Features.DoctorSchedule.DTOs;

public class DoctorScheduleEventDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = default!;
    public int PatientAge { get; set; }
    public string PatientPhone { get; set; } = default!;
    public string? PatientEmail { get; set; }
    public string AppointmentType { get; set; } = default!;
    public string Date { get; set; } = default!;
    public string Time { get; set; } = default!;
    public int Duration { get; set; }
    public string Status { get; set; } = default!;
    public string? ChiefComplaint { get; set; }
    public string? Notes { get; set; }
    public List<string> MedicalHistory { get; set; } = new();
    public List<string> Allergies { get; set; } = new();
    public List<string> CurrentMedications { get; set; } = new();
}

public class DoctorScheduleResponse
{
    public List<DoctorScheduleEventDto> Schedule { get; set; } = new();
    public int TotalCount { get; set; }
}

public class UpdateAppointmentStatusRequest
{
    [Required]
    public string Status { get; set; } = default!;
    
    [StringLength(500)]
    public string? Notes { get; set; }
}

public class AddAppointmentNotesRequest
{
    [Required]
    [StringLength(500)]
    public string Notes { get; set; } = default!;
}
