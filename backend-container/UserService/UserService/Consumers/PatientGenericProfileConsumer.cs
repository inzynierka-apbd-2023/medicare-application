using MassTransit;
using Medicare.Messaging.Contracts;
using Microsoft.EntityFrameworkCore;
using UserService.Data;

namespace UserService.Consumers;

public class PatientGenericProfileConsumer : IConsumer<IGetPatient>
{
    private readonly UserDbContext _db;
    private readonly ILogger<PatientGenericProfileConsumer> _logger;

    public PatientGenericProfileConsumer(UserDbContext db, ILogger<PatientGenericProfileConsumer> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<IGetPatient> context)
    {
        var patientId = context.Message.PatientId;
        _logger.LogInformation("Processing GetPatient request for {PatientId}", patientId);

        var profile = await _db.UserProfiles.FindAsync(new object[] { patientId }, context.CancellationToken);

        if (profile == null)
        {
            throw new InvalidOperationException($"Patient {patientId} not found");
        }

        await context.RespondAsync<IPatientProfile>(new
        {
            PatientId = profile.UserId,
            FirstName = profile.FirstName,
            LastName = profile.LastName,
            Email = profile.Email,
            Phone = profile.Phone,
            DateOfBirth = profile.DateOfBirth,
            Gender = profile.Gender,
            AddressLine1 = profile.AddressLine1,
            AddressLine2 = profile.AddressLine2,
            City = profile.City,
            State = profile.State,
            ZipCode = profile.ZipCode,
            Country = profile.Country,
            BloodType = (string?)null,
            Allergies = (string?)null,
            Medications = (string?)null,
            MedicalHistory = (string?)null,
            InsuranceProvider = (string?)null,
            InsurancePolicyNumber = (string?)null,
            EmergencyContactName = (string?)null,
            EmergencyContactPhone = (string?)null
        });
    }
}
