using Microsoft.EntityFrameworkCore;
using BillingService.Data;
using BillingService.Features.RevenueMetrics.Queries;
using BillingService.Features.RevenueMetrics.DTOs;

namespace BillingService.Services;

public class RevenueMetricsService : IRevenueMetricsService
{
    private readonly BillingDbContext _context;

    public RevenueMetricsService(BillingDbContext context)
    {
        _context = context;
    }

    public async Task<DailyRevenueResponse> GetDailyRevenueAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        var startDate = date.ToDateTime(TimeOnly.MinValue);
        var endDate = date.ToDateTime(TimeOnly.MaxValue);

        // Get successful payment transactions for the day
        var successfulTransactions = await _context.PaymentTransactions
            .Where(pt => pt.OccurredAt >= startDate && pt.OccurredAt <= endDate 
                        && pt.Type == Models.TransactionType.Capture)
            .ToListAsync(cancellationToken);

        var totalRevenue = successfulTransactions.Sum(t => t.AmountCents) / 100.0m;
        var transactionCount = successfulTransactions.Count;

        // Get appointment-specific revenue
        var appointmentPaymentIntents = await _context.PaymentIntents
            .Where(pi => pi.Kind == Models.PaymentIntentKind.Appointment
                        && pi.Status == Models.PaymentIntentStatus.Succeeded)
            .Select(pi => pi.Id)
            .ToListAsync(cancellationToken);

        var appointmentTransactions = successfulTransactions
            .Where(t => appointmentPaymentIntents.Contains(t.PaymentIntentId))
            .ToList();

        var appointmentRevenue = appointmentTransactions.Sum(t => t.AmountCents) / 100.0m;

        // Get subscription-specific revenue
        var subscriptionPaymentIntents = await _context.PaymentIntents
            .Where(pi => pi.Kind == Models.PaymentIntentKind.Subscription
                        && pi.Status == Models.PaymentIntentStatus.Succeeded)
            .Select(pi => pi.Id)
            .ToListAsync(cancellationToken);

        var subscriptionTransactions = successfulTransactions
            .Where(t => subscriptionPaymentIntents.Contains(t.PaymentIntentId))
            .ToList();

        var subscriptionRevenue = subscriptionTransactions.Sum(t => t.AmountCents) / 100.0m;

        return new DailyRevenueResponse
        {
            Date = date,
            TotalRevenue = totalRevenue,
            AppointmentRevenue = appointmentRevenue,
            SubscriptionRevenue = subscriptionRevenue,
            TransactionCount = transactionCount
        };
    }

    public async Task<MonthlyRevenueResponse> GetMonthlyRevenueAsync(int year, int month, CancellationToken cancellationToken = default)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        // Get successful payment transactions for the month
        var successfulTransactions = await _context.PaymentTransactions
            .Where(pt => pt.OccurredAt >= startDate && pt.OccurredAt <= endDate
                        && pt.Type == Models.TransactionType.Capture)
            .ToListAsync(cancellationToken);

        var totalRevenue = successfulTransactions.Sum(t => t.AmountCents) / 100.0m;
        var transactionCount = successfulTransactions.Count;

        // Get appointment-specific revenue
        var appointmentPaymentIntents = await _context.PaymentIntents
            .Where(pi => pi.Kind == Models.PaymentIntentKind.Appointment
                        && pi.Status == Models.PaymentIntentStatus.Succeeded)
            .Select(pi => pi.Id)
            .ToListAsync(cancellationToken);

        var appointmentTransactions = successfulTransactions
            .Where(t => appointmentPaymentIntents.Contains(t.PaymentIntentId))
            .ToList();

        var appointmentRevenue = appointmentTransactions.Sum(t => t.AmountCents) / 100.0m;

        // Get subscription-specific revenue
        var subscriptionPaymentIntents = await _context.PaymentIntents
            .Where(pi => pi.Kind == Models.PaymentIntentKind.Subscription
                        && pi.Status == Models.PaymentIntentStatus.Succeeded)
            .Select(pi => pi.Id)
            .ToListAsync(cancellationToken);

        var subscriptionTransactions = successfulTransactions
            .Where(t => subscriptionPaymentIntents.Contains(t.PaymentIntentId))
            .ToList();

        var subscriptionRevenue = subscriptionTransactions.Sum(t => t.AmountCents) / 100.0m;

        // Calculate daily breakdown
        var dailyBreakdown = new List<DailyRevenueItem>();
        var daysInMonth = DateTime.DaysInMonth(year, month);

        for (int day = 1; day <= daysInMonth; day++)
        {
            var dayStart = new DateTime(year, month, day);
            var dayEnd = dayStart.AddDays(1).AddTicks(-1);

            var dayTransactions = successfulTransactions
                .Where(t => t.OccurredAt >= dayStart && t.OccurredAt <= dayEnd)
                .ToList();

            dailyBreakdown.Add(new DailyRevenueItem
            {
                Day = day,
                Revenue = dayTransactions.Sum(t => t.AmountCents) / 100.0m,
                TransactionCount = dayTransactions.Count
            });
        }

        // Calculate growth percentage (Revenue vs Previous Month)
        var prevMonthDate = startDate.AddMonths(-1);
        var prevMonthEnd = startDate.AddDays(-1);
        
        var prevMonthRevenue = await _context.PaymentTransactions
            .Where(pt => pt.OccurredAt >= prevMonthDate && pt.OccurredAt <= prevMonthEnd
                        && pt.Type == Models.TransactionType.Capture)
            .SumAsync(t => t.AmountCents, cancellationToken) / 100.0m;
            
        decimal growthPercentage = 0;
        if (prevMonthRevenue > 0)
        {
            growthPercentage = ((totalRevenue - prevMonthRevenue) / prevMonthRevenue) * 100m;
        }
        else if (totalRevenue > 0)
        {
            growthPercentage = 100m; // 100% growth if starting from 0
        }

        return new MonthlyRevenueResponse
        {
            Year = year,
            Month = month,
            TotalRevenue = totalRevenue,
            AppointmentRevenue = appointmentRevenue,
            SubscriptionRevenue = subscriptionRevenue,
            TransactionCount = transactionCount,
            GrowthPercentage = Math.Round(growthPercentage, 1),
            DailyBreakdown = dailyBreakdown
        };
    }

    public async Task<YearlyRevenueResponse> GetYearlyRevenueAsync(int year, CancellationToken cancellationToken = default)
    {
        var startDate = new DateTime(year, 1, 1);
        var endDate = new DateTime(year + 1, 1, 1).AddTicks(-1);

        // Get successful payment transactions for the year
        var successfulTransactions = await _context.PaymentTransactions
            .Where(pt => pt.OccurredAt >= startDate && pt.OccurredAt <= endDate
                        && pt.Type == Models.TransactionType.Capture)
            .ToListAsync(cancellationToken);

        var totalRevenue = successfulTransactions.Sum(t => t.AmountCents) / 100.0m;
        var transactionCount = successfulTransactions.Count;

        // Get appointment-specific revenue
        var appointmentPaymentIntents = await _context.PaymentIntents
            .Where(pi => pi.Kind == Models.PaymentIntentKind.Appointment
                        && pi.Status == Models.PaymentIntentStatus.Succeeded)
            .Select(pi => pi.Id)
            .ToListAsync(cancellationToken);

        var appointmentTransactions = successfulTransactions
            .Where(t => appointmentPaymentIntents.Contains(t.PaymentIntentId))
            .ToList();

        var appointmentRevenue = appointmentTransactions.Sum(t => t.AmountCents) / 100.0m;

        // Get subscription-specific revenue
        var subscriptionPaymentIntents = await _context.PaymentIntents
            .Where(pi => pi.Kind == Models.PaymentIntentKind.Subscription
                        && pi.Status == Models.PaymentIntentStatus.Succeeded)
            .Select(pi => pi.Id)
            .ToListAsync(cancellationToken);

        var subscriptionTransactions = successfulTransactions
            .Where(t => subscriptionPaymentIntents.Contains(t.PaymentIntentId))
            .ToList();

        var subscriptionRevenue = subscriptionTransactions.Sum(t => t.AmountCents) / 100.0m;

        // Calculate monthly breakdown
        var monthlyBreakdown = new List<MonthlyRevenueItem>();

        for (int month = 1; month <= 12; month++)
        {
            var monthStart = new DateTime(year, month, 1);
            var monthEnd = monthStart.AddMonths(1).AddTicks(-1);

            var monthTransactions = successfulTransactions
                .Where(t => t.OccurredAt >= monthStart && t.OccurredAt <= monthEnd)
                .ToList();

            monthlyBreakdown.Add(new MonthlyRevenueItem
            {
                Month = month,
                Revenue = monthTransactions.Sum(t => t.AmountCents) / 100.0m,
                TransactionCount = monthTransactions.Count
            });
        }

        return new YearlyRevenueResponse
        {
            Year = year,
            TotalRevenue = totalRevenue,
            AppointmentRevenue = appointmentRevenue,
            SubscriptionRevenue = subscriptionRevenue,
            TransactionCount = transactionCount,
            MonthlyBreakdown = monthlyBreakdown
        };
    }

    public async Task<PaymentTypesBreakdownResponse> GetPaymentTypesBreakdownAsync(DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default)
    {
        var start = startDate.ToDateTime(TimeOnly.MinValue);
        var end = endDate.ToDateTime(TimeOnly.MaxValue);

        // Get successful payment transactions for the period
        var successfulTransactions = await _context.PaymentTransactions
            .Where(pt => pt.OccurredAt >= start && pt.OccurredAt <= end
                        && pt.Type == Models.TransactionType.Capture)
            .ToListAsync(cancellationToken);

        var totalRevenue = successfulTransactions.Sum(t => t.AmountCents) / 100.0m;

        // Get appointment-specific data
        var appointmentPaymentIntents = await _context.PaymentIntents
            .Where(pi => pi.Kind == Models.PaymentIntentKind.Appointment
                        && pi.Status == Models.PaymentIntentStatus.Succeeded)
            .ToListAsync(cancellationToken);

        var appointmentTransactions = successfulTransactions
            .Where(t => appointmentPaymentIntents.Any(api => api.Id == t.PaymentIntentId))
            .ToList();

        var appointmentRevenue = appointmentTransactions.Sum(t => t.AmountCents) / 100.0m;
        var appointmentCount = appointmentTransactions.Count;

        // Get subscription-specific data
        var subscriptionPaymentIntents = await _context.PaymentIntents
            .Where(pi => pi.Kind == Models.PaymentIntentKind.Subscription
                        && pi.Status == Models.PaymentIntentStatus.Succeeded)
            .ToListAsync(cancellationToken);

        var subscriptionTransactions = successfulTransactions
            .Where(t => subscriptionPaymentIntents.Any(spi => spi.Id == t.PaymentIntentId))
            .ToList();

        var subscriptionRevenue = subscriptionTransactions.Sum(t => t.AmountCents) / 100.0m;
        var subscriptionCount = subscriptionTransactions.Count;

        // Calculate percentages
        var appointmentPercentage = totalRevenue > 0 ? (appointmentRevenue / totalRevenue) * 100 : 0;
        var subscriptionPercentage = totalRevenue > 0 ? (subscriptionRevenue / totalRevenue) * 100 : 0;

        return new PaymentTypesBreakdownResponse
        {
            StartDate = startDate,
            EndDate = endDate,
            TotalRevenue = totalRevenue,
            AppointmentPayments = new PaymentTypeData
            {
                Revenue = appointmentRevenue,
                Count = appointmentCount,
                Percentage = appointmentPercentage
            },
            SubscriptionPayments = new PaymentTypeData
            {
                Revenue = subscriptionRevenue,
                Count = subscriptionCount,
                Percentage = subscriptionPercentage
            }
        };
    }
}
