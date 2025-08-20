using MediatR;
using AppointmentService.Features.DoctorDashboard.DTOs;

namespace AppointmentService.Features.DoctorDashboard.Queries;

public class GetDoctorQuickStatsQuery : IRequest<DoctorQuickStatsResponse>
{
    public Guid DoctorId { get; set; }
}
