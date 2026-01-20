using MassTransit;
using Medicare.Messaging.Contracts;
using Microsoft.EntityFrameworkCore;
using PractitionerService.Data;

namespace PractitionerService.Messaging.Consumers;

public class GetDoctorsConsumer : IConsumer<IGetDoctors>
{
    private readonly PractitionerDbContext _context;

    public GetDoctorsConsumer(PractitionerDbContext context)
    {
        _context = context;
    }

    public async Task Consume(ConsumeContext<IGetDoctors> context)
    {
        var ids = context.Message.DoctorIds;
        
        var doctors = await _context.Doctors
            .AsNoTracking()
            .Where(d => ids.Contains(d.Id))
            .ToListAsync();

        var doctorIds = doctors.Select(d => d.Id).ToList();
        
        var specializationsByDoctor = await _context.DoctorSpecializations
            .Where(ds => doctorIds.Contains(ds.DoctorId))
            .Join(_context.Specializations, ds => ds.SpecializationId, s => s.Id, (ds, s) => new { ds.DoctorId, s.Name })
            .GroupBy(x => x.DoctorId)
            .ToDictionaryAsync(g => g.Key, g => string.Join(", ", g.Select(x => x.Name)));

        var response = doctors.Select(d => new DoctorProfile
        {
            DoctorId = d.Id,
            UserId = d.UserId,
            FirstName = "",
            LastName = "",
            SpecializationNames = specializationsByDoctor.GetValueOrDefault(d.Id, "")
        }).ToList<IDoctorProfile>();

        await context.RespondAsync<IDoctorProfiles>(new { Profiles = response });
    }

    public record DoctorProfile : IDoctorProfile
    {
        public Guid DoctorId { get; init; }
        public Guid UserId { get; init; }
        public required string FirstName { get; init; }
        public required string LastName { get; init; }
        public string Email { get; init; } = "";
        public required string SpecializationNames { get; init; }
    }
}
