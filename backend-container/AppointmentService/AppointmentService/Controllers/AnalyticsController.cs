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
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(IMediator mediator, ILogger<AnalyticsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get comprehensive appointment analytics dashboard data
    /// </summary>
    /// <param name="startDate">Start date for analytics period (optional, defaults to 30 days ago)</param>
    /// <param name="endDate">End date for analytics period (optional, defaults to now)</param>
    /// <param name="doctorId">Filter by specific doctor (optional)</param>
    /// <param name="specialization">Filter by specialization (optional)</param>
    /// <param name="status">Filter by appointment status (optional)</param>
    /// <returns>Complete analytics response with metrics, trends, performance data</returns>
    [HttpGet("dashboard")]
    public async Task<ActionResult<AppointmentAnalyticsResponse>> GetDashboardAnalytics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? doctorId = null,
        [FromQuery] string? specialization = null,
        [FromQuery] string? status = null)
    {
        try
        {
            // Validate request parameters
            var validationResults = AnalyticsRequestValidator.ValidateAnalyticsRequest(
                startDate, endDate, doctorId, specialization, status);

            if (validationResults.Any())
            {
                var errors = validationResults.Select(r => r.ErrorMessage).ToArray();
                _logger.LogWarning("Invalid analytics request: {Errors}", string.Join(", ", errors));
                return BadRequest(new { Errors = errors });
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            _logger.LogInformation("Analytics dashboard requested by user {UserId} with role {UserRole}", userId, userRole);

            // Validate authorization for analytics access
            if (!IsAuthorizedForAnalytics(userRole))
            {
                _logger.LogWarning("Unauthorized analytics access attempt by user {UserId} with role {UserRole}", userId, userRole);
                return Forbid("Insufficient permissions to access analytics data");
            }

            // If user is a doctor, restrict to their own data unless they have admin role
            if (userRole == "Doctor" && string.IsNullOrEmpty(doctorId))
            {
                doctorId = userId; // Restrict to their own data
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

            _logger.LogInformation("Analytics dashboard data successfully retrieved for user {UserId}", userId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving analytics dashboard data");
            return StatusCode(500, "An error occurred while retrieving analytics data");
        }
    }

    /// <summary>
    /// Get appointment metrics only
    /// </summary>
    [HttpGet("metrics")]
    public async Task<ActionResult<IEnumerable<AppointmentMetricDto>>> GetAppointmentMetrics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? doctorId = null)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (!IsAuthorizedForAnalytics(userRole))
            {
                return Forbid("Insufficient permissions to access analytics data");
            }

            if (userRole == "Doctor" && string.IsNullOrEmpty(doctorId))
            {
                doctorId = userId;
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving appointment metrics");
            return StatusCode(500, "An error occurred while retrieving metrics data");
        }
    }

    /// <summary>
    /// Get appointment trends data
    /// </summary>
    [HttpGet("trends")]
    public async Task<ActionResult<IEnumerable<TrendDataDto>>> GetAppointmentTrends(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? doctorId = null,
        [FromQuery] int days = 30)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (!IsAuthorizedForAnalytics(userRole))
            {
                return Forbid("Insufficient permissions to access analytics data");
            }

            if (userRole == "Doctor" && string.IsNullOrEmpty(doctorId))
            {
                doctorId = userId;
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving appointment trends");
            return StatusCode(500, "An error occurred while retrieving trends data");
        }
    }

    /// <summary>
    /// Get doctor performance data
    /// </summary>
    [HttpGet("doctor-performance")]
    public async Task<ActionResult<IEnumerable<DoctorPerformanceDto>>> GetDoctorPerformance(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? doctorId = null,
        [FromQuery] string? specialization = null)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (!IsAuthorizedForAnalytics(userRole))
            {
                return Forbid("Insufficient permissions to access analytics data");
            }

            // Restrict doctor access to their own data
            if (userRole == "Doctor")
            {
                doctorId = userId;
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving doctor performance data");
            return StatusCode(500, "An error occurred while retrieving performance data");
        }
    }

    /// <summary>
    /// Get specialization statistics
    /// </summary>
    [HttpGet("specialization-stats")]
    public async Task<ActionResult<IEnumerable<SpecializationStatsDto>>> GetSpecializationStats(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            // Only admins and managers can see specialization-wide stats
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving specialization statistics");
            return StatusCode(500, "An error occurred while retrieving specialization data");
        }
    }

    /// <summary>
    /// Get time slot analysis data
    /// </summary>
    [HttpGet("time-slot-analysis")]
    public async Task<ActionResult<TimeSlotAnalysisDto>> GetTimeSlotAnalysis(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? doctorId = null)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (!IsAuthorizedForAnalytics(userRole))
            {
                return Forbid("Insufficient permissions to access analytics data");
            }

            if (userRole == "Doctor" && string.IsNullOrEmpty(doctorId))
            {
                doctorId = userId;
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving time slot analysis");
            return StatusCode(500, "An error occurred while retrieving time slot data");
        }
    }

    private static bool IsAuthorizedForAnalytics(string? userRole)
    {
        return userRole switch
        {
            "Doctor" => true,
            "Receptionist" => true,
            "Admin" => true,
            "Manager" => true,
            _ => false
        };
    }

    private static bool IsAuthorizedForSpecializationStats(string? userRole)
    {
        return userRole switch
        {
            "Admin" => true,
            "Manager" => true,
            _ => false
        };
    }
}
