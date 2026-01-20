using MassTransit;
using Medicare.Messaging.Contracts;
using Microsoft.EntityFrameworkCore;
using BillingService.Data;

namespace BillingService.Consumers;

public class GetAppointmentPaymentsConsumer : IConsumer<IGetAppointmentPayments>
{
    private readonly BillingDbContext _context;

    public GetAppointmentPaymentsConsumer(BillingDbContext context)
    {
        _context = context;
    }

    public async Task Consume(ConsumeContext<IGetAppointmentPayments> context)
    {
        var ids = context.Message.AppointmentIds;
        var payments = await _context.AppointmentPayments
            .AsNoTracking()
            .Where(p => ids.Contains(p.AppointmentId))
            .ToListAsync();

        var response = payments.Select(p => new AppointmentPayment 
        {
            AppointmentId = p.AppointmentId,
            AmountCents = p.AmountCents, 
            Status = p.AmountCents == 0 ? "Free" : (p.PaymentIntentId.HasValue ? "Paid" : "Pending")
        }).ToList<IAppointmentPayment>();

        await context.RespondAsync<IAppointmentPayments>(new { Payments = response });
    }

    public record AppointmentPayment : IAppointmentPayment
    {
        public Guid AppointmentId { get; init; }
        public long AmountCents { get; init; }
        public required string Status { get; init; }
    }
}
