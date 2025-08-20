using MediatR;

namespace AppointmentService.Features.DoctorSchedule.Commands;

public class UpdateAppointmentStatusCommand : IRequest<bool>
{
    public Guid AppointmentId { get; set; }
    public string Status { get; set; } = default!;
    public string? Notes { get; set; }
}
