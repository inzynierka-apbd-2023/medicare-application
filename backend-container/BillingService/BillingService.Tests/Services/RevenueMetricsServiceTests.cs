using Microsoft.EntityFrameworkCore;
using BillingService.Data;
using BillingService.Services;
using BillingService.Models;
using BillingService.Features.RevenueMetrics.DTOs;

namespace BillingService.Tests.Services;

public class RevenueMetricsServiceTests : IDisposable
{
    private readonly BillingDbContext _context;
    private readonly RevenueMetricsService _service;

    public RevenueMetricsServiceTests()
    {
        var options = new DbContextOptionsBuilder<BillingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new BillingDbContext(options);
        _service = new RevenueMetricsService(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    private static PaymentIntent CreateTestPaymentIntent(
        string id, 
        PaymentIntentKind kind, 
        long amountCents, 
        DateTime? createdAt = null,
        PaymentIntentStatus status = PaymentIntentStatus.Succeeded)
    {
        return new PaymentIntent
        {
            Id = id,
            Kind = kind,
            Status = status,
            AmountCents = amountCents,
            CreatedAt = createdAt ?? DateTime.UtcNow,
            PatientId = $"patient-{id}",
            Provider = "stripe",
            SubjectId = kind == PaymentIntentKind.Appointment ? $"appointment-{id}" : $"subscription-{id}"
        };
    }

    [Fact]
    public async Task GetDailyRevenueAsync_WithTransactionsOnDate_ReturnsCorrectRevenue()
    {
        // Arrange
        var testDate = new DateTime(2025, 8, 19);
        
        var paymentIntent1 = CreateTestPaymentIntent("1", PaymentIntentKind.Appointment, 10000, testDate);
        var paymentIntent2 = CreateTestPaymentIntent("2", PaymentIntentKind.Subscription, 5000, testDate);

        var transaction1 = new PaymentTransaction
        {
            Id = "txn-1",
            PaymentIntentId = "1",
            Type = TransactionType.Capture,
            AmountCents = 10000,
            OccurredAt = testDate.AddHours(10)
        };

        var transaction2 = new PaymentTransaction
        {
            Id = "txn-2",
            PaymentIntentId = "2",
            Type = TransactionType.Capture,
            AmountCents = 5000,
            OccurredAt = testDate.AddHours(14)
        };

        _context.PaymentIntents.AddRange(paymentIntent1, paymentIntent2);
        _context.PaymentTransactions.AddRange(transaction1, transaction2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetDailyRevenueAsync(DateOnly.FromDateTime(testDate), CancellationToken.None);

        // Assert
        Assert.Equal(150.00m, result.TotalRevenue); // 15000 cents = $150
        Assert.Equal(100.00m, result.AppointmentRevenue); // 10000 cents = $100
        Assert.Equal(50.00m, result.SubscriptionRevenue); // 5000 cents = $50
        Assert.Equal(DateOnly.FromDateTime(testDate), result.Date);
        Assert.Equal(2, result.TransactionCount);
    }    [Fact]
    public async Task GetDailyRevenueAsync_WithNoTransactions_ReturnsZeroRevenue()
    {
        // Arrange
        var testDate = new DateOnly(2025, 8, 19);

        // Act
        var result = await _service.GetDailyRevenueAsync(testDate);

        // Assert
        Assert.Equal(testDate, result.Date);
        Assert.Equal(0m, result.TotalRevenue);
        Assert.Equal(0m, result.AppointmentRevenue);
        Assert.Equal(0m, result.SubscriptionRevenue);
        Assert.Equal(0, result.TransactionCount);
    }

    [Fact]
    public async Task GetDailyRevenueAsync_WithFailedTransactions_ExcludesFailedTransactions()
    {
        // Arrange
        var testDate = new DateOnly(2025, 8, 19);
        var testDateTime = testDate.ToDateTime(TimeOnly.MinValue);

        var paymentIntent = CreateTestPaymentIntent("intent-1", PaymentIntentKind.Appointment, 5000, testDateTime);

        var transactions = new List<PaymentTransaction>
        {
            new PaymentTransaction
            {
                Id = "txn-success",
                PaymentIntentId = "intent-1",
                Type = TransactionType.Capture,
                AmountCents = 5000,
                OccurredAt = testDateTime.AddHours(10)
            },
            new PaymentTransaction
            {
                Id = "txn-failed",
                PaymentIntentId = "intent-1", 
                Type = TransactionType.Failure,
                AmountCents = 3000,
                OccurredAt = testDateTime.AddHours(12)
            }
        };

        _context.PaymentIntents.Add(paymentIntent);
        _context.PaymentTransactions.AddRange(transactions);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetDailyRevenueAsync(testDate);

        // Assert
        Assert.Equal(50.00m, result.TotalRevenue); // Only successful transaction
        Assert.Equal(1, result.TransactionCount); // Only successful transaction counted
    }

    [Fact]
    public async Task GetMonthlyRevenueAsync_WithTransactionsInMonth_ReturnsCorrectRevenue()
    {
        // Arrange
        var year = 2025;
        var month = 8;
        var startDate = new DateTime(year, month, 1);

        var paymentIntent = CreateTestPaymentIntent("intent-1", PaymentIntentKind.Appointment, 10000);

        var transactions = new List<PaymentTransaction>
        {
            new PaymentTransaction
            {
                Id = "txn-1",
                PaymentIntentId = "intent-1",
                Type = TransactionType.Capture,
                AmountCents = 5000,
                OccurredAt = startDate.AddDays(5)
            },
            new PaymentTransaction
            {
                Id = "txn-2",
                PaymentIntentId = "intent-1",
                Type = TransactionType.Capture,
                AmountCents = 3000,
                OccurredAt = startDate.AddDays(15)
            }
        };

        _context.PaymentIntents.Add(paymentIntent);
        _context.PaymentTransactions.AddRange(transactions);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetMonthlyRevenueAsync(year, month);

        // Assert
        Assert.Equal(year, result.Year);
        Assert.Equal(month, result.Month);
        Assert.Equal(80.00m, result.TotalRevenue);
        Assert.Equal(2, result.TransactionCount);
        Assert.NotNull(result.DailyBreakdown);
        Assert.Equal(31, result.DailyBreakdown.Count); // August has 31 days
    }

    [Fact]
    public async Task GetYearlyRevenueAsync_WithTransactionsInYear_ReturnsCorrectRevenue()
    {
        // Arrange
        var year = 2025;
        var startDate = new DateTime(year, 1, 1);

        var paymentIntent = CreateTestPaymentIntent("intent-1", PaymentIntentKind.Subscription, 12000);

        var transactions = new List<PaymentTransaction>
        {
            new PaymentTransaction
            {
                Id = "txn-jan",
                PaymentIntentId = "intent-1",
                Type = TransactionType.Capture,
                AmountCents = 6000,
                OccurredAt = startDate.AddMonths(0).AddDays(15) // January
            },
            new PaymentTransaction
            {
                Id = "txn-jun",
                PaymentIntentId = "intent-1",
                Type = TransactionType.Capture,
                AmountCents = 6000,
                OccurredAt = startDate.AddMonths(5).AddDays(15) // June
            }
        };

        _context.PaymentIntents.Add(paymentIntent);
        _context.PaymentTransactions.AddRange(transactions);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetYearlyRevenueAsync(year);

        // Assert
        Assert.Equal(year, result.Year);
        Assert.Equal(120.00m, result.TotalRevenue);
        Assert.Equal(0m, result.AppointmentRevenue); // Subscription payment
        Assert.Equal(120.00m, result.SubscriptionRevenue);
        Assert.Equal(2, result.TransactionCount);
        Assert.NotNull(result.MonthlyBreakdown);
        Assert.Equal(12, result.MonthlyBreakdown.Count); // 12 months
    }

    [Fact]
    public async Task GetPaymentTypesBreakdownAsync_WithMixedPayments_ReturnsCorrectBreakdown()
    {
        // Arrange
        var startDate = new DateOnly(2025, 8, 1);
        var endDate = new DateOnly(2025, 8, 31);
        var start = startDate.ToDateTime(TimeOnly.MinValue);
        var end = endDate.ToDateTime(TimeOnly.MaxValue);

        var appointmentIntent = CreateTestPaymentIntent("apt-intent", PaymentIntentKind.Appointment, 7000);

        var subscriptionIntent = CreateTestPaymentIntent("sub-intent", PaymentIntentKind.Subscription, 3000);

        var transactions = new List<PaymentTransaction>
        {
            new PaymentTransaction
            {
                Id = "apt-txn",
                PaymentIntentId = "apt-intent",
                Type = TransactionType.Capture,
                AmountCents = 7000,
                OccurredAt = start.AddDays(10)
            },
            new PaymentTransaction
            {
                Id = "sub-txn",
                PaymentIntentId = "sub-intent",
                Type = TransactionType.Capture,
                AmountCents = 3000,
                OccurredAt = start.AddDays(20)
            }
        };

        _context.PaymentIntents.AddRange(appointmentIntent, subscriptionIntent);
        _context.PaymentTransactions.AddRange(transactions);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetPaymentTypesBreakdownAsync(startDate, endDate);

        // Assert
        Assert.Equal(startDate, result.StartDate);
        Assert.Equal(endDate, result.EndDate);
        Assert.Equal(100.00m, result.TotalRevenue); // $70 + $30
        Assert.Equal(70.00m, result.AppointmentPayments.Revenue);
        Assert.Equal(30.00m, result.SubscriptionPayments.Revenue);
        Assert.Equal(70.0m, result.AppointmentPayments.Percentage);
        Assert.Equal(30.0m, result.SubscriptionPayments.Percentage);
        Assert.Equal(1, result.AppointmentPayments.Count);
        Assert.Equal(1, result.SubscriptionPayments.Count);
    }

    [Fact]
    public async Task GetPaymentTypesBreakdownAsync_WithZeroRevenue_ReturnsZeroPercentages()
    {
        // Arrange
        var startDate = new DateOnly(2025, 8, 1);
        var endDate = new DateOnly(2025, 8, 31);

        // Act
        var result = await _service.GetPaymentTypesBreakdownAsync(startDate, endDate);

        // Assert
        Assert.Equal(0m, result.TotalRevenue);
        Assert.Equal(0m, result.AppointmentPayments.Percentage);
        Assert.Equal(0m, result.SubscriptionPayments.Percentage);
    }
}
