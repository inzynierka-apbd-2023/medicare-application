namespace AppointmentService.Features.DoctorSchedule.DTOs;

public class UserProfileDto
{
    public Guid User_Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Email { get; set; } = default!;
    public string? Phone { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
}
