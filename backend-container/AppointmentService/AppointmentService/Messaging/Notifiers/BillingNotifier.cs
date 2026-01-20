using AppointmentService.Models;
using MassTransit;
using Medicare.Messaging.Contracts;

namespace AppointmentService.Messaging.Notifiers;

public interface IBillingNotifier
{
    Task NotifyPaymentInitiated(Guid appointmentId, Guid patientId, string paymentMethod);
}

public class BillingNotifier : IBillingNotifier
{
    private readonly IPublishEndpoint _publishEndpoint;

    public BillingNotifier(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task NotifyPaymentInitiated(Guid appointmentId, Guid patientId, string paymentMethod)
    {
        await _publishEndpoint.Publish<IBillingPaymentInitiated>(new
        {
            AppointmentId = appointmentId,
            PatientId = patientId,
            PaymentMethod = paymentMethod,
            Timestamp = DateTime.UtcNow
        });
    }
}
