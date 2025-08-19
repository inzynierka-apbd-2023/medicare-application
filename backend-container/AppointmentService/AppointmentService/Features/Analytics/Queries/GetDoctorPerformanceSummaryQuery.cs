using MediatR;
using AppointmentService.Features.Analytics.DTOs;

namespace AppointmentService.Features.Analytics.Queries;

public class GetDoctorPerformanceSummaryQuery : IRequest<DoctorPerformanceSummaryDto>
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
