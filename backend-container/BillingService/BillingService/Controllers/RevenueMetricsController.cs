using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using BillingService.Features.RevenueMetrics.DTOs;
using BillingService.Features.RevenueMetrics.Queries;
using BillingService.Features.RevenueMetrics.Validators;

namespace BillingService.Controllers;

/// <summary>
/// Provides revenue metrics endpoints for Owner dashboard.
/// NOTE: Implementation logic intentionally omitted (stub) - only wiring & validation.
/// </summary>
[ApiController]
[Route("api/billing/revenue-metrics")]
[Authorize(Roles = "Owner")]
public class RevenueMetricsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<RevenueMetricsController> _logger;

    public RevenueMetricsController(IMediator mediator, ILogger<RevenueMetricsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Returns daily revenue metrics for the specified date.
    /// </summary>
    /// <param name="date">Date for which to get revenue metrics (optional, defaults to today).</param>
    [HttpGet("daily")]
    [ProducesResponseType(typeof(DailyRevenueResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DailyRevenueResponse>> GetDailyRevenue([FromQuery] DateTime? date = null)
    {
        var requestDate = date ?? DateTime.UtcNow.Date;
        var dateOnly = DateOnly.FromDateTime(requestDate);
        
        var errors = DailyRevenueRequestValidator.Validate(requestDate).ToList();
        if (errors.Count > 0)
        {
            _logger.LogWarning("Invalid daily revenue request for date {Date}: {Errors}", requestDate, string.Join("; ", errors));
            return BadRequest(new { Errors = errors });
        }

        var query = new GetDailyRevenueQuery { Date = dateOnly };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Returns monthly revenue metrics for the specified month and year.
    /// </summary>
    /// <param name="year">Year for which to get revenue metrics (optional, defaults to current year).</param>
    /// <param name="month">Month for which to get revenue metrics (optional, defaults to current month).</param>
    [HttpGet("monthly")]
    [ProducesResponseType(typeof(MonthlyRevenueResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MonthlyRevenueResponse>> GetMonthlyRevenue([FromQuery] int? year = null, [FromQuery] int? month = null)
    {
        var now = DateTime.UtcNow;
        var requestYear = year ?? now.Year;
        var requestMonth = month ?? now.Month;
        
        var errors = MonthlyRevenueRequestValidator.Validate(requestYear, requestMonth).ToList();
        if (errors.Count > 0)
        {
            _logger.LogWarning("Invalid monthly revenue request for {Year}-{Month:D2}: {Errors}", requestYear, requestMonth, string.Join("; ", errors));
            return BadRequest(new { Errors = errors });
        }

        var query = new GetMonthlyRevenueQuery { Year = requestYear, Month = requestMonth };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Returns yearly revenue metrics for the specified year.
    /// </summary>
    /// <param name="year">Year for which to get revenue metrics (optional, defaults to current year).</param>
    [HttpGet("yearly")]
    [ProducesResponseType(typeof(YearlyRevenueResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<YearlyRevenueResponse>> GetYearlyRevenue([FromQuery] int? year = null)
    {
        var requestYear = year ?? DateTime.UtcNow.Year;
        
        var errors = YearlyRevenueRequestValidator.Validate(requestYear).ToList();
        if (errors.Count > 0)
        {
            _logger.LogWarning("Invalid yearly revenue request for year {Year}: {Errors}", requestYear, string.Join("; ", errors));
            return BadRequest(new { Errors = errors });
        }

        var query = new GetYearlyRevenueQuery { Year = requestYear };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Returns payment types breakdown for the specified date range.
    /// </summary>
    /// <param name="startDate">Inclusive start date (optional, defaults to 30 days ago).</param>
    /// <param name="endDate">Inclusive end date (optional, defaults to today).</param>
    [HttpGet("payment-types")]
    [ProducesResponseType(typeof(PaymentTypesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaymentTypesResponse>> GetPaymentTypes([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
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
        
        var startDateOnly = DateOnly.FromDateTime(startDate!.Value);
        var endDateOnly = DateOnly.FromDateTime(endDate!.Value);
        
        var errors = PaymentTypesRequestValidator.Validate(startDate, endDate).ToList();
        if (errors.Count > 0)
        {
            _logger.LogWarning("Invalid payment types request for period {StartDate} to {EndDate}: {Errors}", startDate, endDate, string.Join("; ", errors));
            return BadRequest(new { Errors = errors });
        }

        var query = new GetPaymentTypesQuery { StartDate = startDateOnly, EndDate = endDateOnly };
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
