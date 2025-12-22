using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using PractitionerService.Features.PerformanceMetrics.DTOs;
using PractitionerService.Features.PerformanceMetrics.Queries;
using PractitionerService.Features.PerformanceMetrics.Validators;

namespace PractitionerService.Controllers;

/// <summary>
/// Exposes summary doctor performance metrics for Owner dashboard.
/// NOTE: Implementation is stubbed (no business logic yet) – only validation & wiring.
/// </summary>
[ApiController]
[Route("api/practitioner/doctor-performance")]
[Authorize(Roles = "Owner,Admin")] // restricted to Owner role as requested
public class DoctorPerformanceController : ControllerBase
{
	private readonly IMediator _mediator;
	private readonly ILogger<DoctorPerformanceController> _logger;

	public DoctorPerformanceController(IMediator mediator, ILogger<DoctorPerformanceController> logger)
	{
		_mediator = mediator;
		_logger = logger;
	}

	/// <summary>
	/// Returns high-level doctor performance summary (stubbed values for now).
	/// </summary>
	/// <param name="startDate">Inclusive UTC date (optional). Defaults to 30 days before endDate.</param>
	/// <param name="endDate">Inclusive UTC date (optional). Defaults to today.</param>
	[HttpGet("summary")]
	[ProducesResponseType(typeof(DoctorPerformanceSummaryResponse), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
	public async Task<IActionResult> GetDoctorPerformanceSummary([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
	{
		var errors = DoctorPerformanceRequestValidator.Validate(startDate, endDate).ToList();
		if (errors.Count > 0)
		{
			_logger.LogWarning("Invalid doctor performance summary request: {Errors}", string.Join("; ", errors));
			return BadRequest(new { Errors = errors });
		}

		// Normalize period similar to patient metrics (default last 30 days)
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

		var result = await _mediator.Send(new GetDoctorPerformanceSummaryQuery { StartDate = startDate, EndDate = endDate });
		// Overwrite normalized dates on stub response for clarity
		result.StartDate = startDate;
		result.EndDate = endDate;
		return Ok(result);
	}
}
