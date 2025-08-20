using MediatR;
using AppointmentService.Features.DoctorSchedule.Commands;
using AppointmentService.Features.DoctorSchedule.Services;

namespace AppointmentService.Features.DoctorSchedule.Handlers;

public class AddAppointmentNotesHandler : IRequestHandler<AddAppointmentNotesCommand, bool>
{
    private readonly IDoctorScheduleService _service;

    public AddAppointmentNotesHandler(IDoctorScheduleService service)
    {
        _service = service;
    }

    public async Task<bool> Handle(AddAppointmentNotesCommand request, CancellationToken cancellationToken)
    {
        return await _service.AddAppointmentNotesAsync(
            request.AppointmentId, 
            request.Notes, 
            cancellationToken);
    }
}
