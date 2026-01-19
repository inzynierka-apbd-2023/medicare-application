using MassTransit;
using Medicare.Messaging.Contracts;
using BillingService.Services;

namespace BillingService.Consumers;

public class AppointmentCreatedConsumer : IConsumer<IAppointmentCreated>
{
    private readonly ILogger<AppointmentCreatedConsumer> _logger;
    private readonly AppointmentBillingService _billingService;
    private readonly IPublishEndpoint _publishEndpoint;

    public AppointmentCreatedConsumer(
        ILogger<AppointmentCreatedConsumer> logger,
        AppointmentBillingService billingService,
        IPublishEndpoint publishEndpoint)
    {
        _logger = logger;
        _billingService = billingService;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Consume(ConsumeContext<IAppointmentCreated> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Processing AppointmentCreated for {Id}", msg.AppointmentId);

        var result = await _billingService.EvaluateAndRecordPaymentAsync(msg.AppointmentId, msg.PatientId, msg.ScheduledAt);

        await _publishEndpoint.Publish<IBillingPaymentProcessed>(new
        {
            result.AppointmentId,
            IsPaid = (!result.IsFree && result.AmountCents == 0) || result.IsFree,
            result.AmountCents,
            PlanCode = result.PlanCode,
            Error = (string?)null
        }, context.CancellationToken);
    }
}
