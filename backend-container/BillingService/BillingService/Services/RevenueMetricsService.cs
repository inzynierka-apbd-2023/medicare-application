using Microsoft.EntityFrameworkCore;
using BillingService.Data;
using BillingService.Features.RevenueMetrics.Queries;
using BillingService.Features.RevenueMetrics.DTOs;
using BillingService.Models;

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

        var revenueData = await GetRevenueDataForPeriodAsync(startDate, endDate, cancellationToken);

        return new DailyRevenueResponse
        {
            Date = date,
            TotalRevenue = revenueData.TotalRevenue,
            AppointmentRevenue = revenueData.AppointmentRevenue,
            SubscriptionRevenue = revenueData.SubscriptionRevenue,
            TransactionCount = revenueData.TransactionCount
        };
    }

    public async Task<MonthlyRevenueResponse> GetMonthlyRevenueAsync(int year, int month, CancellationToken cancellationToken = default)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var revenueData = await GetRevenueDataForPeriodAsync(startDate, endDate, cancellationToken);
        var dailyBreakdown = BuildDailyBreakdown(revenueData.Transactions, year, month);
        var growthPercentage = await CalculateGrowthPercentageAsync(startDate, revenueData.TotalRevenue, cancellationToken);

        return new MonthlyRevenueResponse
        {
            Year = year,
            Month = month,
            TotalRevenue = revenueData.TotalRevenue,
            AppointmentRevenue = revenueData.AppointmentRevenue,
            SubscriptionRevenue = revenueData.SubscriptionRevenue,
            TransactionCount = revenueData.TransactionCount,
            GrowthPercentage = Math.Round(growthPercentage, 1),
            DailyBreakdown = dailyBreakdown
        };
    }

    public async Task<YearlyRevenueResponse> GetYearlyRevenueAsync(int year, CancellationToken cancellationToken = default)
    {
        var startDate = new DateTime(year, 1, 1);
        var endDate = new DateTime(year + 1, 1, 1).AddTicks(-1);

        var revenueData = await GetRevenueDataForPeriodAsync(startDate, endDate, cancellationToken);
        var monthlyBreakdown = BuildMonthlyBreakdown(revenueData.Transactions, year);

        return new YearlyRevenueResponse
        {
            Year = year,
            TotalRevenue = revenueData.TotalRevenue,
            AppointmentRevenue = revenueData.AppointmentRevenue,
            SubscriptionRevenue = revenueData.SubscriptionRevenue,
            TransactionCount = revenueData.TransactionCount,
            MonthlyBreakdown = monthlyBreakdown
        };
    }

    public async Task<PaymentTypesBreakdownResponse> GetPaymentTypesBreakdownAsync(DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default)
    {
        var start = startDate.ToDateTime(TimeOnly.MinValue);
        var end = endDate.ToDateTime(TimeOnly.MaxValue);

        var revenueData = await GetRevenueDataForPeriodAsync(start, end, cancellationToken);

        var appointmentPercentage = CalculatePercentage(revenueData.AppointmentRevenue, revenueData.TotalRevenue);
        var subscriptionPercentage = CalculatePercentage(revenueData.SubscriptionRevenue, revenueData.TotalRevenue);

        return new PaymentTypesBreakdownResponse
        {
            StartDate = startDate,
            EndDate = endDate,
            TotalRevenue = revenueData.TotalRevenue,
            AppointmentPayments = new PaymentTypeData
            {
                Revenue = revenueData.AppointmentRevenue,
                Count = revenueData.AppointmentCount,
                Percentage = appointmentPercentage
            },
            SubscriptionPayments = new PaymentTypeData
            {
                Revenue = revenueData.SubscriptionRevenue,
                Count = revenueData.SubscriptionCount,
                Percentage = subscriptionPercentage
            }
        };
    }

    private async Task<RevenueData> GetRevenueDataForPeriodAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var transactions = await _context.PaymentTransactions
            .Where(pt => pt.OccurredAt >= startDate && pt.OccurredAt <= endDate
                        && pt.Type == TransactionType.Capture)
            .ToListAsync(cancellationToken);

        var appointmentIntentIds = await GetPaymentIntentIdsByKindAsync(PaymentIntentKind.Appointment, cancellationToken);
        var subscriptionIntentIds = await GetPaymentIntentIdsByKindAsync(PaymentIntentKind.Subscription, cancellationToken);

        var appointmentTransactions = transactions.Where(t => appointmentIntentIds.Contains(t.PaymentIntentId)).ToList();
        var subscriptionTransactions = transactions.Where(t => subscriptionIntentIds.Contains(t.PaymentIntentId)).ToList();

        return new RevenueData(
            Transactions: transactions,
            TotalRevenue: ConvertCentsToDecimal(transactions.Sum(t => t.AmountCents)),
            TransactionCount: transactions.Count,
            AppointmentRevenue: ConvertCentsToDecimal(appointmentTransactions.Sum(t => t.AmountCents)),
            AppointmentCount: appointmentTransactions.Count,
            SubscriptionRevenue: ConvertCentsToDecimal(subscriptionTransactions.Sum(t => t.AmountCents)),
            SubscriptionCount: subscriptionTransactions.Count
        );
    }

    private async Task<HashSet<Guid>> GetPaymentIntentIdsByKindAsync(PaymentIntentKind kind, CancellationToken cancellationToken)
    {
        var ids = await _context.PaymentIntents
            .Where(pi => pi.Kind == kind && pi.Status == PaymentIntentStatus.Succeeded)
            .Select(pi => pi.Id)
            .ToListAsync(cancellationToken);
        
        return ids.ToHashSet();
    }

    private async Task<decimal> CalculateGrowthPercentageAsync(DateTime currentMonthStart, decimal currentRevenue, CancellationToken cancellationToken)
    {
        var prevMonthStart = currentMonthStart.AddMonths(-1);
        var prevMonthEnd = currentMonthStart.AddDays(-1);

        var prevMonthRevenue = await _context.PaymentTransactions
            .Where(pt => pt.OccurredAt >= prevMonthStart && pt.OccurredAt <= prevMonthEnd
                        && pt.Type == TransactionType.Capture)
            .SumAsync(t => t.AmountCents, cancellationToken) / 100.0m;

        if (prevMonthRevenue > 0)
            return ((currentRevenue - prevMonthRevenue) / prevMonthRevenue) * 100m;

        return currentRevenue > 0 ? 100m : 0;
    }

    private static List<DailyRevenueItem> BuildDailyBreakdown(List<PaymentTransaction> transactions, int year, int month)
    {
        var daysInMonth = DateTime.DaysInMonth(year, month);
        var breakdown = new List<DailyRevenueItem>();

        for (int day = 1; day <= daysInMonth; day++)
        {
            var dayStart = new DateTime(year, month, day);
            var dayEnd = dayStart.AddDays(1).AddTicks(-1);

            var dayTransactions = transactions.Where(t => t.OccurredAt >= dayStart && t.OccurredAt <= dayEnd).ToList();

            breakdown.Add(new DailyRevenueItem
            {
                Day = day,
                Revenue = ConvertCentsToDecimal(dayTransactions.Sum(t => t.AmountCents)),
                TransactionCount = dayTransactions.Count
            });
        }

        return breakdown;
    }

    private static List<MonthlyRevenueItem> BuildMonthlyBreakdown(List<PaymentTransaction> transactions, int year)
    {
        var breakdown = new List<MonthlyRevenueItem>();

        for (int month = 1; month <= 12; month++)
        {
            var monthStart = new DateTime(year, month, 1);
            var monthEnd = monthStart.AddMonths(1).AddTicks(-1);

            var monthTransactions = transactions.Where(t => t.OccurredAt >= monthStart && t.OccurredAt <= monthEnd).ToList();

            breakdown.Add(new MonthlyRevenueItem
            {
                Month = month,
                Revenue = ConvertCentsToDecimal(monthTransactions.Sum(t => t.AmountCents)),
                TransactionCount = monthTransactions.Count
            });
        }

        return breakdown;
    }

    private static decimal ConvertCentsToDecimal(long cents) => cents / 100.0m;

    private static decimal CalculatePercentage(decimal part, decimal total) => total > 0 ? (part / total) * 100 : 0;

    private record RevenueData(
        List<PaymentTransaction> Transactions,
        decimal TotalRevenue,
        int TransactionCount,
        decimal AppointmentRevenue,
        int AppointmentCount,
        decimal SubscriptionRevenue,
        int SubscriptionCount
    );
}
