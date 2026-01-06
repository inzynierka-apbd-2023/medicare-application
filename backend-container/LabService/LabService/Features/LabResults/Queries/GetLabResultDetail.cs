using MediatR;
using Microsoft.EntityFrameworkCore;
using LabService.Data;
using LabService.Models;

namespace LabService.Features.LabResults.Queries;

// Query to get full lab result detail
public record GetLabResultDetailQuery(Guid Id) : IRequest<LabResultDetailDto?>;

public record LabResultDetailDto(
    LabResult Result,
    LabTest? Test,
    LabOrder? Order,
    List<LabResultReview> Reviews
);

public class GetLabResultDetailHandler : IRequestHandler<GetLabResultDetailQuery, LabResultDetailDto?>
{
    private readonly LabDbContext _db;

    public GetLabResultDetailHandler(LabDbContext db) => _db = db;

    public async Task<LabResultDetailDto?> Handle(GetLabResultDetailQuery request, CancellationToken cancellationToken)
    {
        var result = await _db.LabResults.FindAsync([request.Id], cancellationToken);
        if (result == null) return null;

        var test = await _db.LabTests.FindAsync([result.LabTestId], cancellationToken);
        var order = test != null 
            ? await _db.LabOrders.FindAsync([test.LabOrderId], cancellationToken) 
            : null;
        var reviews = await _db.LabResultReviews
            .Where(r => r.LabResultId == request.Id)
            .OrderByDescending(r => r.ReviewedAt)
            .ToListAsync(cancellationToken);

        return new LabResultDetailDto(result, test, order, reviews);
    }
}
