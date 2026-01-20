using MassTransit;
using Medicare.Messaging.Contracts;
using Microsoft.EntityFrameworkCore;
using UserService.Data;

namespace UserService.Consumers;

public class GetPatientsConsumer : IConsumer<IGetPatients>
{
    private readonly UserDbContext _context;

    public GetPatientsConsumer(UserDbContext context)
    {
        _context = context;
    }

    public async Task Consume(ConsumeContext<IGetPatients> context)
    {
        var ids = context.Message.PatientIds;
        var profiles = await _context.UserProfiles
            .AsNoTracking()
            .Where(p => ids.Contains(p.UserId))
            .ToListAsync();

        var response = profiles.Select(p => new PatientProfile 
        {
            PatientId = p.UserId,
            FirstName = p.FirstName,
            LastName = p.LastName,
            Email = p.Email,
            Phone = p.Phone,
            Gender = p.Gender,
            DateOfBirth = p.DateOfBirth,
            AddressLine1 = p.AddressLine1,
            AddressLine2 = p.AddressLine2,
            City = p.City,
            State = p.State,
            ZipCode = p.ZipCode,
            Country = p.Country
        }).ToList<IPatientProfile>();

        await context.RespondAsync<IPatientProfiles>(new { Profiles = response });
    }

    public record PatientProfile : IPatientProfile
    {
        public Guid PatientId { get; init; }
        public string FirstName { get; init; } = "";
        public string LastName { get; init; } = "";
        public string Email { get; init; } = "";
        public string? Phone { get; init; }
        public string? Gender { get; init; }
        public DateTime? DateOfBirth { get; init; }
        public string? AddressLine1 { get; init; }
        public string? AddressLine2 { get; init; }
        public string? City { get; init; }
        public string? State { get; init; }
        public string? ZipCode { get; init; }
        public string? Country { get; init; }
        public string? BloodType { get; init; }
        public string? Allergies { get; init; }
        public string? Medications { get; init; }
        public string? MedicalHistory { get; init; }
        public string? InsuranceProvider { get; init; }
        public string? InsurancePolicyNumber { get; init; }
        public string? EmergencyContactName { get; init; }
        public string? EmergencyContactPhone { get; init; }
    }
}
