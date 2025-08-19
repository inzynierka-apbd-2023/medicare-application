using MediatR;
using AppointmentService.Features.Metrics.DTOs;

namespace AppointmentService.Features.Metrics.Queries;

public class GetAppointmentMetricsQuery : IRequest<AppointmentMetricsResponse>
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
