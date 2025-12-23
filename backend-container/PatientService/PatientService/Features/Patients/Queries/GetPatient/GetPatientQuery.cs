using MediatR;
using PatientService.Models;
using PatientService.Data;

namespace PatientService.Features.Patients.Queries.GetPatient;

public record GetPatientQuery(Guid Id) : IRequest<Patient?>;

public class GetPatientHandler : IRequestHandler<GetPatientQuery, Patient?>
{
    private readonly PatientDbContext _db;
    public GetPatientHandler(PatientDbContext db) => _db = db;

    public async Task<Patient?> Handle(GetPatientQuery request, CancellationToken cancellationToken)
    {
        return await _db.Patients.FindAsync(new object[] { request.Id }, cancellationToken);
    }
}
