using Microsoft.EntityFrameworkCore;
using BillingService.Models;

namespace BillingService.Data;

/// <summary>
/// Shared deterministic IDs for cross-service mock data references
/// Patient IDs match User IDs from UserService for seamless auth integration
/// </summary>
public static class MockIds
{
    // Patient IDs (matching User IDs from UserService for login integration)
    public static readonly Guid Patient1 = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000001");
    public static readonly Guid Patient2 = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000002");
    public static readonly Guid Patient3 = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000003");
    public static readonly Guid Patient4 = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000004");
    public static readonly Guid Patient5 = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000005");
    public static readonly Guid Patient6 = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000006");
    public static readonly Guid Patient7 = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000007");

    // Appointment IDs (from AppointmentService)
    public static readonly Guid Appointment1 = Guid.Parse("55555555-5555-5555-5555-000000000001");
    public static readonly Guid Appointment2 = Guid.Parse("55555555-5555-5555-5555-000000000002");
    public static readonly Guid Appointment3 = Guid.Parse("55555555-5555-5555-5555-000000000003");
    public static readonly Guid Appointment4 = Guid.Parse("55555555-5555-5555-5555-000000000004");
    public static readonly Guid Appointment5 = Guid.Parse("55555555-5555-5555-5555-000000000005");
    public static readonly Guid Appointment6 = Guid.Parse("55555555-5555-5555-5555-000000000006");
    public static readonly Guid Appointment7 = Guid.Parse("55555555-5555-5555-5555-000000000007");

    // Payment Method IDs
    public static readonly Guid PaymentMethod1 = Guid.Parse("77777777-7777-7777-7777-000000000001");
    public static readonly Guid PaymentMethod2 = Guid.Parse("77777777-7777-7777-7777-000000000002");
    public static readonly Guid PaymentMethod3 = Guid.Parse("77777777-7777-7777-7777-000000000003");
    public static readonly Guid PaymentMethod4 = Guid.Parse("77777777-7777-7777-7777-000000000004");
    public static readonly Guid PaymentMethod5 = Guid.Parse("77777777-7777-7777-7777-000000000005");
    public static readonly Guid PaymentMethod6 = Guid.Parse("77777777-7777-7777-7777-000000000006");
    public static readonly Guid PaymentMethod7 = Guid.Parse("77777777-7777-7777-7777-000000000007");

    // Subscription IDs
    public static readonly Guid Subscription1 = Guid.Parse("88888888-8888-8888-8888-000000000001");
    public static readonly Guid Subscription2 = Guid.Parse("88888888-8888-8888-8888-000000000002");
    public static readonly Guid Subscription3 = Guid.Parse("88888888-8888-8888-8888-000000000003");
    public static readonly Guid Subscription4 = Guid.Parse("88888888-8888-8888-8888-000000000004");
    public static readonly Guid Subscription5 = Guid.Parse("88888888-8888-8888-8888-000000000005");
    public static readonly Guid Subscription6 = Guid.Parse("88888888-8888-8888-8888-000000000006");
    public static readonly Guid Subscription7 = Guid.Parse("88888888-8888-8888-8888-000000000007");

    // Payment Intent IDs
    public static readonly Guid PaymentIntent1 = Guid.Parse("99999999-9999-9999-9999-000000000001");
    public static readonly Guid PaymentIntent2 = Guid.Parse("99999999-9999-9999-9999-000000000002");
    public static readonly Guid PaymentIntent3 = Guid.Parse("99999999-9999-9999-9999-000000000003");
    public static readonly Guid PaymentIntent4 = Guid.Parse("99999999-9999-9999-9999-000000000004");
    public static readonly Guid PaymentIntent5 = Guid.Parse("99999999-9999-9999-9999-000000000005");
    public static readonly Guid PaymentIntent6 = Guid.Parse("99999999-9999-9999-9999-000000000006");
    public static readonly Guid PaymentIntent7 = Guid.Parse("99999999-9999-9999-9999-000000000007");

    public static readonly Guid[] AllPatientIds = { Patient1, Patient2, Patient3, Patient4, Patient5, Patient6, Patient7 };
    public static readonly Guid[] AllAppointmentIds = { Appointment1, Appointment2, Appointment3, Appointment4, Appointment5, Appointment6, Appointment7 };
    public static readonly Guid[] AllPaymentMethodIds = { PaymentMethod1, PaymentMethod2, PaymentMethod3, PaymentMethod4, PaymentMethod5, PaymentMethod6, PaymentMethod7 };
    public static readonly Guid[] AllSubscriptionIds = { Subscription1, Subscription2, Subscription3, Subscription4, Subscription5, Subscription6, Subscription7 };
    public static readonly Guid[] AllPaymentIntentIds = { PaymentIntent1, PaymentIntent2, PaymentIntent3, PaymentIntent4, PaymentIntent5, PaymentIntent6, PaymentIntent7 };
}

public static class MockDataSeeder
{
    public static async Task SeedAsync(BillingDbContext db)
    {
        int created = 0;

        // Seed Plan definitions (5 tiers including FREE)
        var existingPlanCodes = await db.Plans.Select(p => p.Code).ToHashSetAsync();
        var planDefinitions = new[]
        {
            new Plan
            {
                Code = "FREE",
                Name = "Pay Per Visit",
                Description = "No subscription required. Pay only when you book an appointment.",
                PriceCents = 0, // Free - no subscription cost
                Currency = "PLN",
                BillingPeriod = "none",
                FreeVisitsPerMonth = 0,
                HasMessaging = false,
                HasPrescriptions = false,
                HasDocuments = false,
                IsActive = true,
                SortOrder = 0
            },
            new Plan
            {
                Code = "BASIC_MONTHLY",
                Name = "Basic Monthly",
                Description = "5 free visits per month. Additional visits paid separately.",
                PriceCents = 4900, // 49 PLN
                Currency = "PLN",
                BillingPeriod = "monthly",
                FreeVisitsPerMonth = 5,
                HasMessaging = false,
                HasPrescriptions = false,
                HasDocuments = false,
                IsActive = true,
                SortOrder = 1
            },
            new Plan
            {
                Code = "BASIC_YEARLY",
                Name = "Basic Yearly",
                Description = "5 free visits per month. Additional visits paid separately. Save 2 months!",
                PriceCents = 49000, // 490 PLN
                Currency = "PLN",
                BillingPeriod = "yearly",
                FreeVisitsPerMonth = 5,
                HasMessaging = false,
                HasPrescriptions = false,
                HasDocuments = false,
                IsActive = true,
                SortOrder = 2
            },
            new Plan
            {
                Code = "PREMIUM_MONTHLY",
                Name = "Premium Monthly",
                Description = "Unlimited free visits per month. Full access to all features.",
                PriceCents = 14900, // 149 PLN
                Currency = "PLN",
                BillingPeriod = "monthly",
                FreeVisitsPerMonth = int.MaxValue,
                HasMessaging = true,
                HasPrescriptions = true,
                HasDocuments = true,
                IsActive = true,
                SortOrder = 3
            },
            new Plan
            {
                Code = "PREMIUM_YEARLY",
                Name = "Premium Yearly",
                Description = "Unlimited free visits per month. Full access to all features. Save 2 months!",
                PriceCents = 149000, // 1490 PLN
                Currency = "PLN",
                BillingPeriod = "yearly",
                FreeVisitsPerMonth = int.MaxValue,
                HasMessaging = true,
                HasPrescriptions = true,
                HasDocuments = true,
                IsActive = true,
                SortOrder = 4
            }
        };

        foreach (var plan in planDefinitions)
        {
            var existingPlan = await db.Plans.FirstOrDefaultAsync(p => p.Code == plan.Code);
            if (existingPlan == null)
            {
                db.Plans.Add(plan);
                created++;
            }
            else
            {
                // Update definitions for dev sync
                if (existingPlan.FreeVisitsPerMonth != plan.FreeVisitsPerMonth || 
                    existingPlan.Description != plan.Description)
                {
                    existingPlan.FreeVisitsPerMonth = plan.FreeVisitsPerMonth;
                    existingPlan.Description = plan.Description;
                    created++;
                }
            }
        }

        // Seed Payment Methods (7 patients with cards)
        var cardBrands = new[] { "Visa", "Mastercard", "Amex", "Visa", "Mastercard", "Discover", "Visa" };
        var last4s = new[] { "4242", "5555", "3782", "1234", "9876", "6011", "4444" };

        var existingPaymentMethodIds = await db.PaymentMethods.Select(p => p.Id).ToHashSetAsync();
        for (int i = 0; i < 7; i++)
        {
            var pmId = MockIds.AllPaymentMethodIds[i];
            if (!existingPaymentMethodIds.Contains(pmId))
            {
                db.PaymentMethods.Add(new PaymentMethod
                {
                    Id = pmId,
                    PatientId = MockIds.AllPatientIds[i],
                    Provider = "stripe",
                    ProviderToken = $"pm_mock_{pmId:N}",
                    Last4 = last4s[i],
                    Brand = cardBrands[i],
                    IsDefault = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-30 + i)
                });
                created++;
            }
        }

        // Seed Subscription Contracts (7 patients with various plans)
        var subscriptionPlanCodes = new[] { "PREMIUM_MONTHLY", "BASIC_MONTHLY", "PREMIUM_YEARLY", "BASIC_YEARLY", "BASIC_MONTHLY", "PREMIUM_MONTHLY", "BASIC_YEARLY" };
        var statuses = new[] 
        { 
            SubscriptionStatus.Active, 
            SubscriptionStatus.Active, 
            SubscriptionStatus.Active,
            SubscriptionStatus.Paused, 
            SubscriptionStatus.Active, 
            SubscriptionStatus.PastDue, 
            SubscriptionStatus.Canceled 
        };

        var existingSubscriptionIds = await db.SubscriptionContracts.Select(s => s.Id).ToHashSetAsync();
        for (int i = 0; i < 7; i++)
        {
            var subId = MockIds.AllSubscriptionIds[i];
            if (!existingSubscriptionIds.Contains(subId))
            {
                db.SubscriptionContracts.Add(new SubscriptionContract
                {
                    Id = subId,
                    PatientId = MockIds.AllPatientIds[i],
                    PlanCode = subscriptionPlanCodes[i],
                    PeriodStart = DateTime.UtcNow.AddMonths(-1).Date,
                    PeriodEnd = DateTime.UtcNow.AddMonths(1).Date,
                    Status = statuses[i],
                    DefaultPaymentMethodId = MockIds.AllPaymentMethodIds[i]
                });
                created++;
            }
        }

        // Seed Payment Intents (7 for various appointments and subscriptions)
        // Appointments: 300 PLN (30000 cents), Subscriptions: 49 PLN/149 PLN (4900/14900 cents)
        var intentAmounts = new long[] { 30000, 4900, 30000, 4900, 30000, 14900, 30000 }; // in cents
        var intentStatuses = new[]
        {
            PaymentIntentStatus.Succeeded,
            PaymentIntentStatus.Succeeded,
            PaymentIntentStatus.Processing,
            PaymentIntentStatus.Succeeded,
            PaymentIntentStatus.Succeeded,
            PaymentIntentStatus.RequiresConfirmation,
            PaymentIntentStatus.Canceled
        };

        var existingIntentIds = await db.PaymentIntents.Select(p => p.Id).ToHashSetAsync();
        for (int i = 0; i < 7; i++)
        {
            var intentId = MockIds.AllPaymentIntentIds[i];
            if (!existingIntentIds.Contains(intentId))
            {
                db.PaymentIntents.Add(new PaymentIntent
                {
                    Id = intentId,
                    Kind = i % 2 == 0 ? PaymentIntentKind.Appointment : PaymentIntentKind.Subscription,
                    SubjectId = i % 2 == 0 ? MockIds.AllAppointmentIds[i] : MockIds.AllSubscriptionIds[i],
                    PatientId = MockIds.AllPatientIds[i],
                    Provider = "stripe",
                    ProviderIntentId = $"pi_mock_{intentId:N}",
                    ClientSecret = $"pi_mock_{intentId:N}_secret_test",
                    AmountCents = intentAmounts[i],
                    Currency = "PLN",
                    Status = intentStatuses[i],
                    CreatedAt = DateTime.UtcNow.AddDays(-7 + i)
                });
                created++;
            }
        }

        if (created > 0)
        {
            await db.SaveChangesAsync();
        }

        // Seed Payment Transactions (for succeeded intents)
        var existingTransactionIntentIds = await db.PaymentTransactions.Select(t => t.PaymentIntentId).ToHashSetAsync();
        for (int i = 0; i < 7; i++)
        {
            var intentId = MockIds.AllPaymentIntentIds[i];
            if (intentStatuses[i] == PaymentIntentStatus.Succeeded && !existingTransactionIntentIds.Contains(intentId))
            {
                db.PaymentTransactions.Add(new PaymentTransaction
                {
                    Id = Guid.NewGuid(),
                    PaymentIntentId = intentId,
                    Type = TransactionType.Capture,
                    AmountCents = intentAmounts[i],
                    Currency = "USD",
                    OccurredAt = DateTime.UtcNow.AddDays(-6 + i),
                    ProviderChargeId = $"ch_mock_{intentId:N}"
                });
                created++;
            }
        }

        // Seed Appointment Payments (for appointment intents)
        var existingAppPaymentIds = await db.AppointmentPayments.Select(a => a.AppointmentId).ToHashSetAsync();
        for (int i = 0; i < 7; i += 2) // Only even indices are appointment payments
        {
            var appointmentId = MockIds.AllAppointmentIds[i];
            if (!existingAppPaymentIds.Contains(appointmentId))
            {
                db.AppointmentPayments.Add(new AppointmentPayment
                {
                    Id = Guid.NewGuid(),
                    AppointmentId = appointmentId,
                    PatientId = MockIds.AllPatientIds[i],
                    AmountCents = intentAmounts[i],
                    Currency = "USD",
                    PaymentIntentId = MockIds.AllPaymentIntentIds[i]
                });
                created++;
            }
        }

        // Seed Subscription Payments (for subscription intents)
        var existingSubPaymentIds = await db.SubscriptionPayments.Select(s => s.SubscriptionContractId).ToHashSetAsync();
        for (int i = 1; i < 7; i += 2) // Only odd indices are subscription payments
        {
            var subId = MockIds.AllSubscriptionIds[i];
            if (!existingSubPaymentIds.Contains(subId))
            {
                db.SubscriptionPayments.Add(new SubscriptionPayment
                {
                    Id = Guid.NewGuid(),
                    SubscriptionContractId = subId,
                    PatientId = MockIds.AllPatientIds[i],
                    PlanCode = subscriptionPlanCodes[i],
                    PeriodStart = DateTime.UtcNow.AddMonths(-1).Date,
                    PeriodEnd = DateTime.UtcNow.Date,
                    AmountCents = intentAmounts[i],
                    Currency = "USD",
                    PaymentIntentId = MockIds.AllPaymentIntentIds[i]
                });
                created++;
            }
        }

        if (created > 0)
        {
            await db.SaveChangesAsync();
            Console.WriteLine($"[MockDataSeeder] Created {created} billing records (payment methods, subscriptions, intents, transactions, payments).");
        }
        else
        {
            Console.WriteLine("[MockDataSeeder] All billing mock data already exists.");
        }
    }
}
