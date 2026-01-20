using AppointmentService.Data;
using MassTransit;
using Medicare.Messaging.Contracts;

namespace AppointmentService.Messaging.Consumers;

public class AppointmentPaymentConsumer : IConsumer<IBillingPaymentProcessed>
{
    private readonly AppointmentDbContext _db;
    private readonly ILogger<AppointmentPaymentConsumer> _logger;

    public AppointmentPaymentConsumer(AppointmentDbContext db, ILogger<AppointmentPaymentConsumer> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<IBillingPaymentProcessed> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Processing payment update for appt {Id}: IsPaid={IsPaid}", msg.AppointmentId, msg.IsPaid);

        if (!msg.IsPaid)
        {
            _logger.LogWarning("Payment failed for {Id}: {Error}", msg.AppointmentId, msg.Error);
            return;
        }

        var appt = await _db.Appointments.FindAsync(new object[] { msg.AppointmentId }, context.CancellationToken);
        
        if (appt == null)
        {
            _logger.LogWarning("Appointment {Id} not found during payment update", msg.AppointmentId);
            return;
        }

        appt.IsPaid = msg.IsPaid;
        appt.PaymentProcessed = true; 
        appt.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(context.CancellationToken);
        _logger.LogInformation("Updated Appointment {Id} -> IsPaid: {IsPaid}", appt.Id, appt.IsPaid);
    }
}
