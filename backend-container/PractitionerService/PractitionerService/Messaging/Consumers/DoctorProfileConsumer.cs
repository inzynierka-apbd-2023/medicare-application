using MassTransit;
using Medicare.Messaging.Contracts;
using Microsoft.EntityFrameworkCore;
using PractitionerService.Data;

namespace PractitionerService.Messaging.Consumers;

public class DoctorProfileConsumer : IConsumer<IGetDoctor>
{
    private readonly PractitionerDbContext _db;
    private readonly ILogger<DoctorProfileConsumer> _logger;

    public DoctorProfileConsumer(PractitionerDbContext db, ILogger<DoctorProfileConsumer> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<IGetDoctor> context)
    {
        var doctorId = context.Message.DoctorId;
        _logger.LogInformation("Processing GetDoctor request for {DoctorId}", doctorId);

        var doc = await _db.Set<Models.DoctorDirectory>()
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.DoctorId == doctorId || d.UserId == doctorId, context.CancellationToken);

        if (doc == null)
        {
            throw new InvalidOperationException($"Doctor not found: {doctorId}");
        }

        await context.RespondAsync<IDoctorProfile>(new
        {
            doc.DoctorId,
            doc.UserId,
            FirstName = doc.FirstName ?? "",
            LastName = doc.LastName ?? "",
            Email = doc.Email ?? "",
            SpecializationNames = doc.Specializations ?? ""
        });
    }
}
