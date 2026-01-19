using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AppointmentService.Features.Metrics.DTOs;
using AppointmentService.Features.Metrics.Validators;
using MediatR;
using AppointmentService.Features.Metrics.Queries;

namespace AppointmentService.Controllers;

[ApiController]
[Route("api/appointment/[controller]")]
[Authorize(Roles = "Owner,Admin")]
public class MetricsController : ControllerBase
{
    private readonly IMediator _mediator;
    public MetricsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(AppointmentMetricsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AppointmentMetricsResponse>> GetMetrics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var errors = AppointmentMetricsRequestValidator.Validate(startDate, endDate).ToList();
        if (errors.Count > 0)
        {
            return BadRequest(new { Errors = errors });
        }
        if (!startDate.HasValue && !endDate.HasValue)
        {
            endDate = DateTime.UtcNow.Date;
            startDate = endDate.Value.AddDays(-30);
        }
        else if (startDate.HasValue && !endDate.HasValue)
        {
            endDate = DateTime.UtcNow.Date;
        }
        else if (!startDate.HasValue && endDate.HasValue)
        {
            startDate = endDate.Value.AddDays(-30);
        }
        var result = await _mediator.Send(new GetAppointmentMetricsQuery { StartDate = startDate, EndDate = endDate });
        return Ok(result);
    }
}
