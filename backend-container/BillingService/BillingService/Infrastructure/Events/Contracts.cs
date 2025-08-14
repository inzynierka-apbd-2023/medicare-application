namespace BillingService.Infrastructure.Events;

public static class BillingEvents
{
    public const string AppointmentPaid = "billing.appointment.paid";
    public const string SubscriptionPaid = "billing.subscription.paid";
    public const string PaymentFailed = "billing.payment.failed";
    public const string SubscriptionRenewalDue = "billing.subscription.renewal_due";
}
