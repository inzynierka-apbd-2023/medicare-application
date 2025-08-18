using MediatR;
using AppointmentService.Features.Analytics.DTOs;

namespace AppointmentService.Features.Analytics.Queries;

public class GetAppointmentMetricsQuery : IRequest<IEnumerable<AppointmentMetricDto>>
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? DoctorId { get; set; }
}

public class GetAppointmentTrendsQuery : IRequest<IEnumerable<TrendDataDto>>
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? DoctorId { get; set; }
    public int Days { get; set; } = 30;
}

public class GetDoctorPerformanceQuery : IRequest<IEnumerable<DoctorPerformanceDto>>
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? DoctorId { get; set; }
    public string? Specialization { get; set; }
}

public class GetSpecializationStatsQuery : IRequest<IEnumerable<SpecializationStatsDto>>
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class GetTimeSlotAnalysisQuery : IRequest<TimeSlotAnalysisDto>
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? DoctorId { get; set; }
}
