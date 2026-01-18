using MediatR;
using LabService.Data;
using LabService.Models;

namespace LabService.Features.LabResults.Commands;

public record SubmitLabResultReviewCommand(
    Guid LabResultId,
    Guid ReviewedByDoctorId,
    string ReviewStatus,
    string? ReviewNotes,
    string? Recommendations
) : IRequest<LabResultReview>;

public class SubmitLabResultReviewHandler : IRequestHandler<SubmitLabResultReviewCommand, LabResultReview>
{
    private readonly LabDbContext _db;

    public SubmitLabResultReviewHandler(LabDbContext db) => _db = db;

    public async Task<LabResultReview> Handle(SubmitLabResultReviewCommand request, CancellationToken cancellationToken)
    {
        var result = await _db.LabResults.FindAsync([request.LabResultId], cancellationToken);
        if (result == null)
            throw new KeyNotFoundException($"Lab result {request.LabResultId} not found");

        // Create the review record
        var review = new LabResultReview
        {
            LabResultId = request.LabResultId,
            ReviewedByDoctorId = request.ReviewedByDoctorId,
            ReviewedAt = DateTime.UtcNow,
            ReviewStatus = request.ReviewStatus,
            ReviewNotes = request.ReviewNotes,
            Recommendations = request.Recommendations,
            CreatedAt = DateTime.UtcNow
        };

        // Update the lab result
        result.ReviewedByDoctorId = request.ReviewedByDoctorId;
        result.ReviewedAt = DateTime.UtcNow;
        result.ReviewStatus = request.ReviewStatus == "RequiresFollowUp" ? "RequiresFollowUp" : "Reviewed";

        _db.LabResultReviews.Add(review);
        await _db.SaveChangesAsync(cancellationToken);

        return review;
    }
}
