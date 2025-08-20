using MediatR;

namespace AppointmentService.Features.DoctorSchedule.Commands;

public class AddAppointmentNotesCommand : IRequest<bool>
{
    public Guid AppointmentId { get; set; }
    public string Notes { get; set; } = default!;
}
