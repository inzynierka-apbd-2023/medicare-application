using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using BillingService.Features.RevenueMetrics.DTOs;
using BillingService.Features.RevenueMetrics.Queries;
using BillingService.Features.RevenueMetrics.Validators;

namespace BillingService.Controllers;

[ApiController]
[Route("api/billing/revenue-metrics")]
[Authorize(Roles = "Owner,Admin")]
public class RevenueMetricsController : ControllerBase
{
    private readonly IMediator _mediator;

    public RevenueMetricsController(IMediator mediator)
    {
        _mediator = mediator;
    }

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
            return BadRequest(new { Errors = errors });
        }

        var query = new GetDailyRevenueQuery { Date = dateOnly };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

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
            return BadRequest(new { Errors = errors });
        }

        var query = new GetMonthlyRevenueQuery { Year = requestYear, Month = requestMonth };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("yearly")]
    [ProducesResponseType(typeof(YearlyRevenueResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<YearlyRevenueResponse>> GetYearlyRevenue([FromQuery] int? year = null)
    {
        var requestYear = year ?? DateTime.UtcNow.Year;
        
        var errors = YearlyRevenueRequestValidator.Validate(requestYear).ToList();
        if (errors.Count > 0)
        {
            return BadRequest(new { Errors = errors });
        }

        var query = new GetYearlyRevenueQuery { Year = requestYear };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("payment-types")]
    [ProducesResponseType(typeof(PaymentTypesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaymentTypesResponse>> GetPaymentTypes([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
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
            return BadRequest(new { Errors = errors });
        }

        var query = new GetPaymentTypesQuery { StartDate = startDateOnly, EndDate = endDateOnly };
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
