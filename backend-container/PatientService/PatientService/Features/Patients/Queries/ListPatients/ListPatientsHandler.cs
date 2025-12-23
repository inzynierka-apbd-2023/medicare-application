using MediatR;
using Microsoft.EntityFrameworkCore;
using PatientService.Data;
using PatientService.Models;

namespace PatientService.Features.Patients.Queries.ListPatients;

public class ListPatientsHandler : IRequestHandler<ListPatientsQuery, ListPatientsResponse>
{
    private readonly PatientDbContext _db;

    public ListPatientsHandler(PatientDbContext db)
    {
        _db = db;
    }

    public async Task<ListPatientsResponse> Handle(ListPatientsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Set<PatientOverview>().AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(p => 
                (p.FirstName != null && p.FirstName.ToLower().Contains(term)) ||
                (p.LastName != null && p.LastName.ToLower().Contains(term)) ||
                (p.Email != null && p.Email.ToLower().Contains(term))
            );
        }

        var totalCount = await query.CountAsync(cancellationToken);
        
        var items = await query
            .OrderByDescending(p => p.PatientId) // Or explicit CreatedAt if available in view
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new ListPatientsResponse(
            items, 
            totalCount, 
            request.Page, 
            (int)Math.Ceiling(totalCount / (double)request.PageSize)
        );
    }
}
