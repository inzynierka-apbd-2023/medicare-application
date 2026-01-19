using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientService.Features.Metrics.DTOs;
using PatientService.Features.Metrics.Validators;
using MediatR;
using PatientService.Features.Metrics.Queries;

namespace PatientService.Controllers;

[ApiController]
[Route("api/patient/[controller]")]
[Authorize(Roles = "Owner,Admin")]
public class MetricsController : ControllerBase
{
    private readonly IMediator _mediator;

    public MetricsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PatientMetricsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPatientMetrics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var errors = PatientMetricsRequestValidator.Validate(startDate, endDate).ToList();
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

        var result = await _mediator.Send(new GetPatientMetricsQuery { StartDate = startDate, EndDate = endDate });
        return Ok(result);
    }
}
