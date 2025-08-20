using MediatR;
using AppointmentService.Features.AppointmentMetrics.DTOs;

namespace AppointmentService.Features.AppointmentMetrics.Queries;

public class GetAppointmentMetricsQuery : IRequest<IEnumerable<AppointmentMetricDto>>
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid? DoctorId { get; set; }
}
