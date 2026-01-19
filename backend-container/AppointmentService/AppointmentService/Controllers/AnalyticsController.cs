using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Security.Claims;
using AppointmentService.Features.Analytics.Queries;
using AppointmentService.Features.Analytics.DTOs;
using AppointmentService.Features.Analytics.Validators;

namespace AppointmentService.Controllers;

[ApiController]
[Route("api/appointment/[controller]")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AnalyticsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("doctor-performance/summary")]
    public async Task<ActionResult<DoctorPerformanceSummaryDto>> GetDoctorPerformanceSummary(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var userRole = User.FindFirst("role")?.Value;
        if (userRole != "Owner" && userRole != "Admin")
        {
            return Forbid("Insufficient permissions to access doctor performance summary");
        }

        var query = new GetDoctorPerformanceSummaryQuery
        {
            StartDate = startDate,
            EndDate = endDate
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<AppointmentAnalyticsResponse>> GetDashboardAnalytics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] Guid? doctorId = null,
        [FromQuery] string? specialization = null,
        [FromQuery] string? status = null)
    {
        var validationResults = AnalyticsRequestValidator.ValidateAnalyticsRequest(
            startDate, endDate, doctorId, specialization, status);

        if (validationResults.Any())
        {
            var errors = validationResults.Select(r => r.ErrorMessage).ToArray();
            return BadRequest(new { Errors = errors });
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userRole = User.FindFirst("role")?.Value;

        if (!IsAuthorizedForAnalytics(userRole))
        {
            return Forbid("Insufficient permissions to access analytics data");
        }

        if (userRole == "Doctor" && doctorId == null)
        {
            if (Guid.TryParse(userId, out var userGuid))
            {
                doctorId = userGuid;
            }
        }

        var query = new GetAppointmentAnalyticsQuery
        {
            StartDate = startDate,
            EndDate = endDate,
            DoctorId = doctorId,
            Specialization = specialization,
            Status = status
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("metrics")]
    public async Task<ActionResult<IEnumerable<AppointmentMetricDto>>> GetAppointmentMetrics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] Guid? doctorId = null)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userRole = User.FindFirst("role")?.Value;

        if (!IsAuthorizedForAnalytics(userRole))
        {
            return Forbid("Insufficient permissions to access analytics data");
        }

        if (userRole == "Doctor" && doctorId == null)
        {
            if (Guid.TryParse(userId, out var userGuid))
            {
                doctorId = userGuid;
            }
        }

        var query = new GetAppointmentMetricsQuery
        {
            StartDate = startDate,
            EndDate = endDate,
            DoctorId = doctorId
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("trends")]
    public async Task<ActionResult<IEnumerable<TrendDataDto>>> GetAppointmentTrends(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] Guid? doctorId = null,
        [FromQuery] int days = 30)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userRole = User.FindFirst("role")?.Value;

        if (!IsAuthorizedForAnalytics(userRole))
        {
            return Forbid("Insufficient permissions to access analytics data");
        }

        if (userRole == "Doctor" && doctorId == null)
        {
            if (Guid.TryParse(userId, out var userGuid))
            {
                doctorId = userGuid;
            }
        }

        var query = new GetAppointmentTrendsQuery
        {
            StartDate = startDate,
            EndDate = endDate,
            DoctorId = doctorId,
            Days = days
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("doctor-performance")]
    public async Task<ActionResult<IEnumerable<DoctorPerformanceDto>>> GetDoctorPerformance(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] Guid? doctorId = null,
        [FromQuery] string? specialization = null)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userRole = User.FindFirst("role")?.Value;

        if (!IsAuthorizedForAnalytics(userRole))
        {
            return Forbid("Insufficient permissions to access analytics data");
        }

        if (userRole == "Doctor")
        {
            if (Guid.TryParse(userId, out var userGuid))
            {
                doctorId = userGuid;
            }
        }

        var query = new GetDoctorPerformanceQuery
        {
            StartDate = startDate,
            EndDate = endDate,
            DoctorId = doctorId,
            Specialization = specialization
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("specialization-stats")]
    public async Task<ActionResult<IEnumerable<SpecializationStatsDto>>> GetSpecializationStats(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var userRole = User.FindFirst("role")?.Value;

        if (!IsAuthorizedForSpecializationStats(userRole))
        {
            return Forbid("Insufficient permissions to access specialization statistics");
        }

        var query = new GetSpecializationStatsQuery
        {
            StartDate = startDate,
            EndDate = endDate
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("time-slot-analysis")]
    public async Task<ActionResult<TimeSlotAnalysisDto>> GetTimeSlotAnalysis(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] Guid? doctorId = null)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userRole = User.FindFirst("role")?.Value;

        if (!IsAuthorizedForAnalytics(userRole))
        {
            return Forbid("Insufficient permissions to access analytics data");
        }

        if (userRole == "Doctor" && doctorId == null)
        {
            if (Guid.TryParse(userId, out var userGuid))
            {
                doctorId = userGuid;
            }
        }

        var query = new GetTimeSlotAnalysisQuery
        {
            StartDate = startDate,
            EndDate = endDate,
            DoctorId = doctorId
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    private static bool IsAuthorizedForAnalytics(string? userRole)
    {
        return userRole switch
        {
            "Doctor" => true,
            "Receptionist" => true,
            "Admin" => true,
            "Owner" => true,
            _ => false
        };
    }

    private static bool IsAuthorizedForSpecializationStats(string? userRole)
    {
        return userRole switch
        {
            "Admin" => true,
            "Owner" => true,
            _ => false
        };
    }
}
