using MassTransit;
using Medicare.Messaging.Contracts;
using Microsoft.EntityFrameworkCore;
using PractitionerService.Data;

namespace PractitionerService.Messaging.Consumers;

public class GetAppointmentRatingsConsumer : IConsumer<IGetAppointmentRatings>
{
    private readonly PractitionerDbContext _context;

    public GetAppointmentRatingsConsumer(PractitionerDbContext context)
    {
        _context = context;
    }

    public async Task Consume(ConsumeContext<IGetAppointmentRatings> context)
    {
        var appointmentIds = context.Message.AppointmentIds;
        
        var ratings = await _context.Rates
            .AsNoTracking()
            .Where(r => r.Appointment_Id.HasValue && appointmentIds.Contains(r.Appointment_Id.Value))
            .ToListAsync();

        var response = ratings.Select(r => new AppointmentRating
        {
            AppointmentId = r.Appointment_Id!.Value,
            RateValue = r.Rate_Value ?? 0,
            Description = r.Description
        }).ToList<IAppointmentRating>();

        await context.RespondAsync<IAppointmentRatings>(new { Ratings = response });
    }

    public record AppointmentRating : IAppointmentRating
    {
        public Guid AppointmentId { get; init; }
        public byte RateValue { get; init; }
        public string? Description { get; init; }
    }
}
