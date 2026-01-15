using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientService.Features.Metrics.DTOs;
using PatientService.Features.Metrics.Validators;
using MediatR;
using PatientService.Features.Metrics.Queries;

namespace PatientService.Controllers;

/// <summary>
/// Provides aggregated patient-related metrics for Owner/Admin dashboards.
/// Provides aggregated patient-related metrics for Owner/Admin dashboards.
/// </summary>
[ApiController]
[Route("api/patient/[controller]")]
[Authorize(Roles = "Owner,Admin")]
public class MetricsController : ControllerBase
{
    private readonly ILogger<MetricsController> _logger;
    private readonly IMediator _mediator;

    public MetricsController(ILogger<MetricsController> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Returns patient metrics for the specified (optional) date range.
    /// </summary>
    /// <param name="startDate">Inclusive start date (UTC date part). Optional.</param>
    /// <param name="endDate">Inclusive end date (UTC date part). Optional.</param>
    [HttpGet]
    [ProducesResponseType(typeof(PatientMetricsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPatientMetrics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var errors = PatientMetricsRequestValidator.Validate(startDate, endDate).ToList();
        if (errors.Count > 0)
        {
            _logger.LogWarning("Invalid patient metrics request: {Errors}", string.Join("; ", errors));
            return BadRequest(new { Errors = errors });
        }

        // Normalize default period (last 30 days) if none provided
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
