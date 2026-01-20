using MediatR;
using PatientService.Models;

namespace PatientService.Features.Patients.Queries.GetPatientsBatch;

public record GetPatientsBatchQuery(List<Guid> PatientIds) : IRequest<List<PatientOverview>>;
