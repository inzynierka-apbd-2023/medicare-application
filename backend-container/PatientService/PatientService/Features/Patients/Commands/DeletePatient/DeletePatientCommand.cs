using MediatR;

namespace PatientService.Features.Patients.Commands.DeletePatient;

public record DeletePatientCommand(string PatientId) : IRequest<bool>;
