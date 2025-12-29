using MediatR;

namespace PatientService.Features.Patients.Commands.DeletePatient;

public record DeletePatientCommand(Guid PatientId) : IRequest<bool>;
