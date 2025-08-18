using MediatR;
using AppointmentService.Features.Analytics.DTOs;

namespace AppointmentService.Features.Analytics.Queries;

public class GetAppointmentAnalyticsQuery : IRequest<AppointmentAnalyticsResponse>
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? DoctorId { get; set; }
    public string? Specialization { get; set; }
    public string? Status { get; set; }
}
