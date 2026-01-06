using Microsoft.AspNetCore.Mvc;
using MediatR;
using LabService.Features.LabResults.Queries;
using LabService.Features.LabResults.Commands;

namespace LabService.Controllers;

[ApiController]
[Route("api/lab/[controller]")]
public class LabResultsController : ControllerBase
{
    private readonly IMediator _mediator;
    public LabResultsController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Get all lab results pending review
    /// </summary>
    [HttpGet("pending-review")]
    public async Task<IActionResult> GetPendingReview()
    {
        var results = await _mediator.Send(new GetPendingLabResultsQuery());
        return Ok(results);
    }

    /// <summary>
    /// Get lab result by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var detail = await _mediator.Send(new GetLabResultDetailQuery(id));
        if (detail == null) return NotFound();
        return Ok(detail.Result);
    }

    /// <summary>
    /// Get full lab result detail with test, order, and reviews
    /// </summary>
    [HttpGet("{id}/detail")]
    public async Task<IActionResult> GetDetailById(Guid id)
    {
        var detail = await _mediator.Send(new GetLabResultDetailQuery(id));
        if (detail == null) return NotFound();
        return Ok(new
        {
            Result = detail.Result,
            Test = detail.Test,
            Order = detail.Order,
            Reviews = detail.Reviews
        });
    }

    /// <summary>
    /// Submit a review for a lab result
    /// </summary>
    [HttpPost("{id}/review")]
    public async Task<IActionResult> ReviewResult(Guid id, [FromBody] ReviewLabResultRequest req)
    {
        try
        {
            var review = await _mediator.Send(new SubmitLabResultReviewCommand(
                id,
                req.ReviewedByDoctorId,
                req.ReviewStatus,
                req.ReviewNotes,
                req.Recommendations
            ));
            return Ok(review);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Quick approve a lab result
    /// </summary>
    [HttpPost("{id}/quick-approve")]
    public async Task<IActionResult> QuickApprove(Guid id, [FromBody] QuickApproveRequest req)
    {
        try
        {
            var review = await _mediator.Send(new SubmitLabResultReviewCommand(
                id,
                req.DoctorId,
                "Reviewed",
                "Quick approval - results within normal limits",
                null
            ));
            return Ok(review);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}

public record ReviewLabResultRequest(
    Guid ReviewedByDoctorId,
    string ReviewStatus,
    string? ReviewNotes,
    string? Recommendations
);

public record QuickApproveRequest(Guid DoctorId);
