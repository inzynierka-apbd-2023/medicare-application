using MediatR;
using AppointmentService.Features.DoctorSchedule.DTOs;

namespace AppointmentService.Features.DoctorSchedule.Queries;

public class GetTodaysAppointmentsQuery : IRequest<DoctorScheduleResponse>
{
    public Guid DoctorId { get; set; }
}
