using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BillingService.Models;

[Table("Payment_Method", Schema = "billing")]
public class PaymentMethod
{
    [Key, MaxLength(36)] public string Id { get; set; } = Guid.NewGuid().ToString();
    [MaxLength(36)] public string PatientId { get; set; } = default!;
    [MaxLength(50)] public string Provider { get; set; } = default!; // e.g., stripe
    [MaxLength(200)] public string ProviderToken { get; set; } = default!; // tokenized payment method id
    [MaxLength(4)] public string? Last4 { get; set; }
    [MaxLength(10)] public string? Brand { get; set; }
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum SubscriptionStatus { None = 0, Active = 1, Paused = 2, Canceled = 3, PastDue = 4 }

[Table("Subscription_Contract", Schema = "billing")]
public class SubscriptionContract
{
    [Key, MaxLength(36)] public string Id { get; set; } = Guid.NewGuid().ToString();
    [MaxLength(36)] public string PatientId { get; set; } = default!;
    [MaxLength(50)] public string PlanCode { get; set; } = default!;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public SubscriptionStatus Status { get; set; }
    [MaxLength(36)] public string? DefaultPaymentMethodId { get; set; }
    public PaymentMethod? DefaultPaymentMethod { get; set; }
}

public enum PaymentIntentKind { Appointment = 1, Subscription = 2 }
public enum PaymentIntentStatus { RequiresPaymentMethod = 1, RequiresConfirmation = 2, Processing = 3, Succeeded = 4, Canceled = 5 }

[Table("Payment_Intent", Schema = "billing")]
public class PaymentIntent
{
    [Key, MaxLength(36)] public string Id { get; set; } = Guid.NewGuid().ToString();
    public PaymentIntentKind Kind { get; set; }
    [MaxLength(36)] public string SubjectId { get; set; } = default!; // appointmentId or subscriptionContractId
    [MaxLength(36)] public string PatientId { get; set; } = default!;
    [MaxLength(50)] public string Provider { get; set; } = default!;
    [MaxLength(100)] public string? ProviderIntentId { get; set; }
    [MaxLength(100)] public string? ClientSecret { get; set; }
    public long AmountCents { get; set; }
    [MaxLength(3)] public string Currency { get; set; } = "USD";
    public PaymentIntentStatus Status { get; set; } = PaymentIntentStatus.RequiresPaymentMethod;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum TransactionType { Authorization = 1, Capture = 2, Refund = 3, Void = 4, Failure = 5 }

[Table("Payment_Transaction", Schema = "billing")]
public class PaymentTransaction
{
    [Key, MaxLength(36)] public string Id { get; set; } = Guid.NewGuid().ToString();
    [MaxLength(36)] public string PaymentIntentId { get; set; } = default!;
    public TransactionType Type { get; set; }
    public long AmountCents { get; set; }
    [MaxLength(3)] public string Currency { get; set; } = "USD";
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    [MaxLength(100)] public string? ProviderChargeId { get; set; }
    [MaxLength(100)] public string? ProviderRefundId { get; set; }
    [MaxLength(1000)] public string? FailureCode { get; set; }
    [MaxLength(2000)] public string? FailureMessage { get; set; }
    public string? RawPayloadJson { get; set; }
}

[Table("Appointment_Payment", Schema = "billing")]
public class AppointmentPayment
{
    [Key, MaxLength(36)] public string Id { get; set; } = Guid.NewGuid().ToString();
    [MaxLength(36)] public string AppointmentId { get; set; } = default!;
    [MaxLength(36)] public string PatientId { get; set; } = default!;
    public long AmountCents { get; set; }
    [MaxLength(3)] public string Currency { get; set; } = "USD";
    [MaxLength(36)] public string? PaymentIntentId { get; set; }
}

[Table("Subscription_Payment", Schema = "billing")]
public class SubscriptionPayment
{
    [Key, MaxLength(36)] public string Id { get; set; } = Guid.NewGuid().ToString();
    [MaxLength(36)] public string SubscriptionContractId { get; set; } = default!;
    [MaxLength(36)] public string PatientId { get; set; } = default!;
    [MaxLength(50)] public string PlanCode { get; set; } = default!;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public long AmountCents { get; set; }
    [MaxLength(3)] public string Currency { get; set; } = "USD";
    [MaxLength(36)] public string? PaymentIntentId { get; set; }
}

[Table("Psp_Webhook_Event", Schema = "billing")]
public class PspWebhookEvent
{
    [Key, MaxLength(100)] public string Id { get; set; } = default!; // provider event id
    [MaxLength(50)] public string Provider { get; set; } = default!;
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
    public string PayloadJson { get; set; } = default!;
    public bool Processed { get; set; }
}

[Table("Outbox_Event", Schema = "billing")]
public class OutboxEvent
{
    [Key, MaxLength(36)] public string Id { get; set; } = Guid.NewGuid().ToString();
    [MaxLength(200)] public string Type { get; set; } = default!;
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public string PayloadJson { get; set; } = default!;
    public DateTime? PublishedAt { get; set; }
}

// Projections (views or tables materialized by background job)
[Table("vw_Patient_Billing_Summary", Schema = "billing")]
public class PatientBillingSummary
{
    [Key, MaxLength(36)] public string PatientId { get; set; } = default!;
    public long TotalPaidCents { get; set; }
    public long OutstandingCents { get; set; }
    public int SuccessfulPayments { get; set; }
}

[Table("vw_Doctor_Revenue_Dashboard", Schema = "billing")]
public class DoctorRevenueDashboard
{
    [Key, MaxLength(36)] public string DoctorId { get; set; } = default!;
    public long TotalRevenueCents { get; set; }
    public int PaidAppointments { get; set; }
}
