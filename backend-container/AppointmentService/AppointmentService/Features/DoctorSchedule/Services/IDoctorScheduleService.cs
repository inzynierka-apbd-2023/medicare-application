using AppointmentService.Features.DoctorSchedule.DTOs;

namespace AppointmentService.Features.DoctorSchedule.Services;

public interface IDoctorScheduleService
{
    Task<DoctorScheduleResponse> GetDoctorScheduleAsync(Guid doctorId, DateTime? startDate, DateTime? endDate, string? status, CancellationToken cancellationToken = default);
    Task<DoctorScheduleResponse> GetTodaysAppointmentsAsync(Guid doctorId, CancellationToken cancellationToken = default);
    Task<DoctorScheduleEventDto?> GetAppointmentDetailsAsync(Guid appointmentId, CancellationToken cancellationToken = default);
    Task<bool> UpdateAppointmentStatusAsync(Guid appointmentId, string status, string? notes, CancellationToken cancellationToken = default);
    Task<bool> AddAppointmentNotesAsync(Guid appointmentId, string notes, CancellationToken cancellationToken = default);
}
