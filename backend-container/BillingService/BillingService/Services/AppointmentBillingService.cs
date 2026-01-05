using BillingService.Data;
using BillingService.Models;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Services;

public class AppointmentBillingService
{
    private readonly BillingDbContext _db;
    private readonly ILogger<AppointmentBillingService> _logger;

    public AppointmentBillingService(BillingDbContext db, ILogger<AppointmentBillingService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<(bool IsFree, long AmountCents, string PlanCode)> EvaluateAndRecordPaymentAsync(Guid appointmentId, Guid patientId, DateTime occurredAt)
    {
        _logger.LogInformation("Evaluating billing for appt {ApptId}, patient {PatientId}", appointmentId, patientId);

        // 1. Get active subscription
        var sub = await _db.SubscriptionContracts
            .OrderByDescending(s => s.PeriodEnd)
            .FirstOrDefaultAsync(s => s.PatientId == patientId && s.Status == SubscriptionStatus.Active);
        
        if (sub == null) 
        {
             _logger.LogWarning("No active subscription found for patient {PatientId}", patientId);
        } else {
             _logger.LogInformation("Found active subscription {SubId} for patient {PatientId}, Plan: {PlanCode}", sub.Id, patientId, sub.PlanCode);
        }

        var planCode = sub?.PlanCode ?? "FREE";
        var plan = await _db.Plans.FindAsync(planCode) ?? await _db.Plans.FindAsync("FREE");

        if (plan == null)
        {
            _logger.LogError("Plan {PlanCode} not found, defaulting to safe paid fallback.", planCode);
            // Fallback: Charge
            return (false, 30000, planCode); 
        }

        // 2. Count appointments in current month
        // 2. Count appointments in the SCHEDULED month
        // We use occurredAt (which should be the ScheduledAt date) to calculate the month bucket
        var startOfMonth = new DateTime(occurredAt.Year, occurredAt.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1);
        
        // Count existing payments in THAT month
        // We use ForDate (newly added) or CreatedAt (for legacy records) to determine the bucket
        var visitsUsed = await _db.AppointmentPayments
            .CountAsync(ap => ap.PatientId == patientId && ap.ForDate >= startOfMonth && ap.ForDate < endOfMonth);

        _logger.LogInformation("Billing Eval: Patient={PatientId}, Plan={PlanCode}, Limit={Limit}, Month={Month}, Used={Used}", 
            patientId, plan.Code, plan.FreeVisitsPerMonth, startOfMonth.ToString("yyyy-MM"), visitsUsed);

        bool isFree = visitsUsed < plan.FreeVisitsPerMonth;
        long amount = isFree ? 0 : 30000; // 300.00 PLN

        // 3. Create Record
        // Check idempotency first just in case
        var existing = await _db.AppointmentPayments.FirstOrDefaultAsync(ap => ap.AppointmentId == appointmentId);
        if (existing != null)
        {
             return (existing.AmountCents == 0, existing.AmountCents, planCode);
        }

        var payment = new AppointmentPayment
        {
            Id = Guid.NewGuid(),
            AppointmentId = appointmentId,
            PatientId = patientId,
            AmountCents = amount,
            Currency = "PLN",
            CreatedAt = DateTime.UtcNow, // Record created now
            ForDate = occurredAt         // Applies to the usage in that month
        };

        _db.AppointmentPayments.Add(payment);
        await _db.SaveChangesAsync();

        return (isFree, amount, planCode);
    }
}
