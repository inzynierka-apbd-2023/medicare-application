using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using AppointmentService.Features.DoctorDashboard.DTOs;
using AppointmentService.Features.DoctorDashboard.Queries;

namespace AppointmentService.Controllers;

[ApiController]
[Route("api/appointment/doctor-dashboard")]
[Authorize]
public class DoctorDashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public DoctorDashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{doctorId}/quick-stats")]
    public async Task<ActionResult<DoctorQuickStatsResponse>> GetQuickStats(Guid doctorId)
    {
        var query = new GetDoctorQuickStatsQuery { DoctorId = doctorId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
