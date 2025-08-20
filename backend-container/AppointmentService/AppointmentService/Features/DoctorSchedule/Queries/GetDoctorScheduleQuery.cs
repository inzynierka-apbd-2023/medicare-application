using MediatR;
using AppointmentService.Features.DoctorSchedule.DTOs;

namespace AppointmentService.Features.DoctorSchedule.Queries;

public class GetDoctorScheduleQuery : IRequest<DoctorScheduleResponse>
{
    public Guid DoctorId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Status { get; set; }
}
