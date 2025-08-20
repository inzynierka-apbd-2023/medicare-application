using MediatR;
using AppointmentService.Features.DoctorSchedule.DTOs;
using AppointmentService.Features.DoctorSchedule.Queries;
using AppointmentService.Features.DoctorSchedule.Services;

namespace AppointmentService.Features.DoctorSchedule.Handlers;

public class GetAppointmentDetailsHandler : IRequestHandler<GetAppointmentDetailsQuery, DoctorScheduleEventDto?>
{
    private readonly IDoctorScheduleService _service;

    public GetAppointmentDetailsHandler(IDoctorScheduleService service)
    {
        _service = service;
    }

    public async Task<DoctorScheduleEventDto?> Handle(GetAppointmentDetailsQuery request, CancellationToken cancellationToken)
    {
        return await _service.GetAppointmentDetailsAsync(request.AppointmentId, cancellationToken);
    }
}
