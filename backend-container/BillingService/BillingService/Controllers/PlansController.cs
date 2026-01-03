using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using BillingService.Features.Plans.DTOs;
using BillingService.Features.Plans.Queries;

namespace BillingService.Controllers;

/// <summary>
/// Provides plan-related endpoints for subscription management.
/// </summary>
[ApiController]
[Route("api/billing/plans")]
public class PlansController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PlansController> _logger;

    public PlansController(IMediator mediator, ILogger<PlansController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all active plans
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<PlanDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PlanDto>>> GetPlans()
    {
        _logger.LogInformation("GetPlans request received");
        
        var query = new GetAllPlansQuery();
        var result = await _mediator.Send(query);
        
        return Ok(result.Plans);
    }

    /// <summary>
    /// Get a specific plan by code
    /// </summary>
    [HttpGet("{code}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PlanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlanDto>> GetPlan(string code)
    {
        _logger.LogInformation("GetPlan request received for code: {Code}", code);
        
        var query = new GetPlanByCodeQuery { Code = code };
        var result = await _mediator.Send(query);
        
        if (!result.Found)
        {
            return NotFound(new { message = result.ErrorMessage });
        }
        
        return Ok(result.Plan);
    }

    /// <summary>
    /// Get a patient's current active plan
    /// </summary>
    [HttpGet("patient/{patientId}")]
    [Authorize]
    [ProducesResponseType(typeof(GetPatientPlanResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<GetPatientPlanResponse>> GetPatientPlan(Guid patientId)
    {
        _logger.LogInformation("GetPatientPlan request received for patient: {PatientId}", patientId);
        
        var query = new GetPatientPlanQuery { PatientId = patientId };
        var result = await _mediator.Send(query);
        
        return Ok(result);
    }
}
