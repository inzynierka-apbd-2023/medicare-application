using MediatR;
using AppointmentService.Features.DoctorDashboard.DTOs;
using AppointmentService.Features.DoctorDashboard.Queries;
using AppointmentService.Features.DoctorDashboard.Services;

namespace AppointmentService.Features.DoctorDashboard.Handlers;

public class GetDoctorQuickStatsHandler : IRequestHandler<GetDoctorQuickStatsQuery, DoctorQuickStatsResponse>
{
    private readonly IDoctorDashboardService _dashboardService;

    public GetDoctorQuickStatsHandler(IDoctorDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    public async Task<DoctorQuickStatsResponse> Handle(GetDoctorQuickStatsQuery request, CancellationToken cancellationToken)
    {
        return await _dashboardService.GetQuickStatsAsync(request.DoctorId, cancellationToken);
    }
}
