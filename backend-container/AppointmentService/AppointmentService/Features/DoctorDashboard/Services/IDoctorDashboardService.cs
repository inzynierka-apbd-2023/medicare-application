using AppointmentService.Features.DoctorDashboard.DTOs;

namespace AppointmentService.Features.DoctorDashboard.Services;

public interface IDoctorDashboardService
{
    Task<DoctorQuickStatsResponse> GetQuickStatsAsync(Guid doctorId, CancellationToken cancellationToken = default);
}
