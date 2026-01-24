using MassTransit;
using Medicare.Messaging.Contracts;
using Microsoft.EntityFrameworkCore;
using PractitionerService.Data;
using PractitionerService.Models;

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
        
        // Use DoctorDirectory view which joins Doctor with User data
        var doctorDirectories = await _context.Set<DoctorDirectory>()
            .AsNoTracking()
            .Where(d => ids.Contains(d.DoctorId) || ids.Contains(d.UserId))
            .ToListAsync();

        var response = doctorDirectories.Select(d => new DoctorProfile
        {
            DoctorId = d.DoctorId,
            UserId = d.UserId,
            FirstName = d.FirstName ?? "",
            LastName = d.LastName ?? "",
            Email = d.Email ?? "",
            SpecializationNames = d.Specializations ?? ""
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
