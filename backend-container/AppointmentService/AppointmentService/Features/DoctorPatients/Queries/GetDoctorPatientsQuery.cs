using MediatR;
using AppointmentService.Features.DoctorPatients.DTOs;

namespace AppointmentService.Features.DoctorPatients.Queries;

public record GetDoctorPatientsQuery(Guid DoctorId) : IRequest<DoctorPatientsResponse>;
