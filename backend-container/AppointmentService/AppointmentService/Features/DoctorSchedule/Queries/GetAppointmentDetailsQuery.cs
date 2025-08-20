using MediatR;

namespace AppointmentService.Features.DoctorSchedule.Queries;

public class GetAppointmentDetailsQuery : IRequest<DTOs.DoctorScheduleEventDto?>
{
    public Guid AppointmentId { get; set; }
}
