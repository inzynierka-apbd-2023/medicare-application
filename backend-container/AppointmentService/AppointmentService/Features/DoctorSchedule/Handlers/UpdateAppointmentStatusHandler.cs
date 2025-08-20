using MediatR;
using AppointmentService.Features.DoctorSchedule.Commands;
using AppointmentService.Features.DoctorSchedule.Services;

namespace AppointmentService.Features.DoctorSchedule.Handlers;

public class UpdateAppointmentStatusHandler : IRequestHandler<UpdateAppointmentStatusCommand, bool>
{
    private readonly IDoctorScheduleService _service;

    public UpdateAppointmentStatusHandler(IDoctorScheduleService service)
    {
        _service = service;
    }

    public async Task<bool> Handle(UpdateAppointmentStatusCommand request, CancellationToken cancellationToken)
    {
        return await _service.UpdateAppointmentStatusAsync(
            request.AppointmentId, 
            request.Status, 
            request.Notes, 
            cancellationToken);
    }
}
