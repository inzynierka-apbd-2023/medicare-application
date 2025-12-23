using MediatR;
using PatientService.Models;

namespace PatientService.Features.Patients.Queries.ListPatients;

// Use PatientOverview model as it's optimized for lists
public record ListPatientsQuery(string? SearchTerm, int Page = 1, int PageSize = 10) : IRequest<ListPatientsResponse>;

public record ListPatientsResponse(List<PatientOverview> Items, int TotalCount, int CurrentPage, int TotalPages);
