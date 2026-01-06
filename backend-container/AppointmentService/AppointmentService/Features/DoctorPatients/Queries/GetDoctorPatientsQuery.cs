using MediatR;
using AppointmentService.Features.DoctorPatients.DTOs;

namespace AppointmentService.Features.DoctorPatients.Queries;

/// <summary>
/// Query to get all patients that have had appointments with a specific doctor
/// </summary>
public record GetDoctorPatientsQuery(Guid DoctorId) : IRequest<DoctorPatientsResponse>;
