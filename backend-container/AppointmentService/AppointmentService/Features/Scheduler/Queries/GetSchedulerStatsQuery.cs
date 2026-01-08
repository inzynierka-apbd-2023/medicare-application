using MediatR;
using AppointmentService.Features.Scheduler.DTOs;

namespace AppointmentService.Features.Scheduler.Queries;

public class GetSchedulerStatsQuery : IRequest<SchedulerStatsResponse>
{
    public Guid? DoctorId { get; set; }
    public Guid? PatientId { get; set; }
}
