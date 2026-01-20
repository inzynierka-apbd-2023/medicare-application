using MassTransit;
using MessagingService.Data;
using MessagingService.Models;
using Medicare.Messaging.Contracts;
using Microsoft.EntityFrameworkCore;

namespace MessagingService.Messaging.Consumers;

public class AppointmentCreatedConsumer : IConsumer<IAppointmentCreated>
{
    private readonly ILogger<AppointmentCreatedConsumer> _logger;
    private readonly MessagingDbContext _db;
    private readonly IRequestClient<IGetDoctor> _doctorClient;

    public AppointmentCreatedConsumer(
        ILogger<AppointmentCreatedConsumer> logger,
        MessagingDbContext db,
        IRequestClient<IGetDoctor> doctorClient)
    {
        _logger = logger;
        _db = db;
        _doctorClient = doctorClient;
    }

    public async Task Consume(ConsumeContext<IAppointmentCreated> context)
    {
        var evt = context.Message;
        _logger.LogInformation("Processing AppointmentCreated for Patient {PatientId} -> Doctor {DoctorId}", evt.PatientId, evt.DoctorId);

        var existing = await _db.PatientDoctorContacts
            .FirstOrDefaultAsync(c => c.PatientUserId == evt.PatientId && c.DoctorUserId == evt.DoctorId);

        var now = DateTime.UtcNow;
        string? doctorName = null;
        string? doctorSpecialization = null;

        if (existing == null || string.IsNullOrEmpty(existing.DoctorName) || existing.DoctorName == "Doctor")
        {
            try
            {
                var response = await _doctorClient.GetResponse<IDoctorProfile>(new { DoctorId = evt.DoctorId });
                if (response.Message != null)
                {
                    doctorName = $"{response.Message.FirstName} {response.Message.LastName}".Trim();
                    doctorSpecialization = response.Message.SpecializationNames;
                    _logger.LogInformation("Fetched doctor details: {Name}, {Spec}", doctorName, doctorSpecialization);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch doctor details for {DoctorId}", evt.DoctorId);
            }
        }

        if (existing != null)
        {
            existing.LastContactAt = now;
            existing.UpdatedAt = now;

            if (!string.IsNullOrEmpty(doctorName))
            {
                existing.DoctorName = doctorName;
            }
            if (!string.IsNullOrEmpty(doctorSpecialization))
            {
                existing.DoctorSpecialization = doctorSpecialization;
            }

            _logger.LogInformation("Updated contact for Patient {PatientId} -> Doctor {DoctorId}", evt.PatientId, evt.DoctorId);
        }
        else
        {
            var contact = new PatientDoctorContact
            {
                Id = Guid.NewGuid(),
                PatientUserId = evt.PatientId,
                DoctorUserId = evt.DoctorId,
                DoctorName = doctorName ?? "Doctor",
                DoctorSpecialization = doctorSpecialization ?? "General",
                FirstContactAt = now,
                LastContactAt = now,
                CreatedAt = now,
                UpdatedAt = now
            };

            _db.PatientDoctorContacts.Add(contact);
            _logger.LogInformation("Created contact for Patient {PatientId} -> Doctor {DoctorId}", evt.PatientId, evt.DoctorId);
        }

        await _db.SaveChangesAsync();
    }
}
