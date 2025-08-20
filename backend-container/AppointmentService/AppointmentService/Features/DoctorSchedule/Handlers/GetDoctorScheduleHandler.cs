using MediatR;
using AppointmentService.Features.DoctorSchedule.DTOs;
using AppointmentService.Features.DoctorSchedule.Queries;
using AppointmentService.Features.DoctorSchedule.Services;

namespace AppointmentService.Features.DoctorSchedule.Handlers;

public class GetDoctorScheduleHandler : IRequestHandler<GetDoctorScheduleQuery, DoctorScheduleResponse>
{
    private readonly IDoctorScheduleService _service;

    public GetDoctorScheduleHandler(IDoctorScheduleService service)
    {
        _service = service;
    }

    public async Task<DoctorScheduleResponse> Handle(GetDoctorScheduleQuery request, CancellationToken cancellationToken)
    {
        return await _service.GetDoctorScheduleAsync(
            request.DoctorId, 
            request.StartDate, 
            request.EndDate, 
            request.Status, 
            cancellationToken);
    }
}
