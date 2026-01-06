using MediatR;
using Microsoft.EntityFrameworkCore;
using LabService.Data;

namespace LabService.Features.LabResults.Queries;

// Query to get lab results pending review
public record GetPendingLabResultsQuery() : IRequest<List<PendingLabResultDto>>;

// Rich DTO for pending lab results
public record PendingLabResultDto(
    Guid Id,
    Guid PatientId,
    Guid LabTestId,
    string TestName,
    string LoincCode,
    string? Value,
    string? Unit,
    string? ReferenceRange,
    string? Flag,
    string? Comments,
    DateTime ResultDate,
    string ReviewStatus,
    string Priority,
    string? OrderNotes,
    DateTime CreatedAt
);

public class GetPendingLabResultsHandler : IRequestHandler<GetPendingLabResultsQuery, List<PendingLabResultDto>>
{
    private readonly LabDbContext _db;

    public GetPendingLabResultsHandler(LabDbContext db) => _db = db;

    public async Task<List<PendingLabResultDto>> Handle(GetPendingLabResultsQuery request, CancellationToken cancellationToken)
    {
        var results = await (
            from r in _db.LabResults
            join t in _db.LabTests on r.LabTestId equals t.Id
            join o in _db.LabOrders on t.LabOrderId equals o.Id
            where r.ReviewStatus == "Pending"
            orderby o.Priority descending, r.ResultDate
            select new PendingLabResultDto(
                r.Id,
                r.PatientId,
                r.LabTestId,
                t.TestName,
                t.LoincCode,
                r.Value,
                r.Unit,
                r.ReferenceRange,
                r.Flag,
                r.Comments,
                r.ResultDate,
                r.ReviewStatus,
                o.Priority,
                o.ClinicalNotes,
                r.CreatedAt
            )
        ).ToListAsync(cancellationToken);

        return results;
    }
}
