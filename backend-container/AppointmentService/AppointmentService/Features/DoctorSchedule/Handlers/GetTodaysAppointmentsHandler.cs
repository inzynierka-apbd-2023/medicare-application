using MediatR;
using AppointmentService.Features.DoctorSchedule.DTOs;
using AppointmentService.Features.DoctorSchedule.Queries;
using AppointmentService.Features.DoctorSchedule.Services;

namespace AppointmentService.Features.DoctorSchedule.Handlers;

public class GetTodaysAppointmentsHandler : IRequestHandler<GetTodaysAppointmentsQuery, DoctorScheduleResponse>
{
    private readonly IDoctorScheduleService _service;

    public GetTodaysAppointmentsHandler(IDoctorScheduleService service)
    {
        _service = service;
    }

    public async Task<DoctorScheduleResponse> Handle(GetTodaysAppointmentsQuery request, CancellationToken cancellationToken)
    {
        return await _service.GetTodaysAppointmentsAsync(request.DoctorId, cancellationToken);
    }
}
