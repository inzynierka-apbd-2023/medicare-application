using MassTransit;
using Medicare.Messaging.Contracts;
using Microsoft.EntityFrameworkCore;
using PractitionerService.Data;

namespace PractitionerService.Consumers;

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
        var profiles = await _context.Doctors
            .AsNoTracking()
            .Where(d => ids.Contains(d.Id))
            .Include(d => d.Specializations)
            .Select(d => new
            {
                DoctorId = d.Id,
                UserId = d.UserId,
                FirstName = d.FirstName,
                LastName = d.LastName,
                Specializations = d.Specializations.Select(s => s.Name)
            })
            .ToListAsync();

        var response = profiles.Select(p => new DoctorProfile 
        {
            DoctorId = p.DoctorId,
            UserId = p.UserId,
            FirstName = p.FirstName,
            LastName = p.LastName,
            SpecializationNames = string.Join(", ", p.Specializations)
        }).ToList<IDoctorProfile>();

        await context.RespondAsync<IDoctorProfiles>(new { Profiles = response });
    }

    public record DoctorProfile : IDoctorProfile
    {
        public Guid DoctorId { get; init; }
        public Guid UserId { get; init; }
        public string FirstName { get; init; }
        public string LastName { get; init; }
        public string Email { get; init; } = "";
        public string SpecializationNames { get; init; }
    }
}
