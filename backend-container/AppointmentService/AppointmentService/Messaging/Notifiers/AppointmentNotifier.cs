using AppointmentService.Models;
using MassTransit;
using Medicare.Messaging.Contracts;

namespace AppointmentService.Messaging.Notifiers;

public interface IAppointmentNotifier
{
    Task NotifyAppointmentCreated(Appointment appointment);
    Task NotifyAppointmentUpdated(Appointment appointment);
    Task NotifyAppointmentRated(Guid appointmentId, Guid doctorId, Guid patientId, byte rating, string? description);
}

public class AppointmentNotifier : IAppointmentNotifier
{
    private readonly IPublishEndpoint _publishEndpoint;

    public AppointmentNotifier(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task NotifyAppointmentCreated(Appointment appointment)
    {
        await _publishEndpoint.Publish<IAppointmentCreated>(new
        {
            AppointmentId = appointment.Id,
            appointment.PatientId,
            appointment.DoctorId,
            appointment.ScheduledAt,
            OccurredAt = DateTime.UtcNow
        });
    }

    public async Task NotifyAppointmentUpdated(Appointment appointment)
    {
        await _publishEndpoint.Publish<IAppointmentUpdated>(new
        {
            AppointmentId = appointment.Id,
            appointment.DoctorId,
            appointment.Status,
            appointment.UpdatedAt,
            OccurredAt = DateTime.UtcNow
        });
    }

    public async Task NotifyAppointmentRated(Guid appointmentId, Guid doctorId, Guid patientId, byte rating, string? description)
    {
        await _publishEndpoint.Publish<IAppointmentRated>(new
        {
            AppointmentId = appointmentId,
            DoctorId = doctorId,
            PatientId = patientId,
            Rating = rating,
            Description = description,
            OccurredAt = DateTime.UtcNow
        });
    }
}
