using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using MediatR;
using BillingService.Controllers;
using BillingService.Features.RevenueMetrics.DTOs;
using BillingService.Features.RevenueMetrics.Queries;
using BillingService.Features.RevenueMetrics.Handlers;
using BillingService.Services;
using BillingService.Data;
using BillingService.Models;

namespace BillingService.Tests.Controllers;

public class RevenueMetricsControllerTests : IDisposable
{
    private readonly BillingDbContext _context;
    private readonly RevenueMetricsService _service;
    private readonly IMediator _mediator;
    private readonly RevenueMetricsController _controller;

    public RevenueMetricsControllerTests()
    {
        var options = new DbContextOptionsBuilder<BillingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new BillingDbContext(options);
        _service = new RevenueMetricsService(_context);
        
        // Create a simple mediator implementation for testing
        _mediator = new TestMediator(_service);
        _controller = new RevenueMetricsController(_mediator, new TestLogger<RevenueMetricsController>());
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task GetDailyRevenue_WithRealData_ReturnsOkResult()
    {
        // Arrange
        var testDate = new DateTime(2025, 8, 19);
        
        // Create test data
        var paymentIntent = new PaymentIntent
        {
            Id = "intent-1",
            Kind = PaymentIntentKind.Appointment,
            Status = PaymentIntentStatus.Succeeded,
            AmountCents = 15000,
            CreatedAt = testDate,
            PatientId = "patient-1",
            Provider = "stripe",
            SubjectId = "appointment-1"
        };

        var transaction = new PaymentTransaction
        {
            Id = "txn-1",
            PaymentIntentId = "intent-1",
            Type = TransactionType.Capture,
            AmountCents = 15000,
            OccurredAt = testDate.AddHours(12)
        };

        _context.PaymentIntents.Add(paymentIntent);
        _context.PaymentTransactions.Add(transaction);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetDailyRevenue(testDate);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<DailyRevenueResponse>(okResult.Value);
        Assert.Equal(150.00m, response.TotalRevenue); // 15000 cents = $150.00
        Assert.Equal(DateOnly.FromDateTime(testDate), response.Date);
        Assert.Equal(1, response.TransactionCount);
    }

    [Fact]
    public async Task GetDailyRevenue_WithoutDate_UsesCurrentDate()
    {
        // Act
        var result = await _controller.GetDailyRevenue(null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<DailyRevenueResponse>(okResult.Value);
        Assert.Equal(DateOnly.FromDateTime(DateTime.UtcNow.Date), response.Date);
    }

    [Fact]
    public async Task GetMonthlyRevenue_WithRealData_ReturnsOkResult()
    {
        // Arrange
        var year = 2025;
        var month = 8;
        var testDate = new DateTime(year, month, 15);
        
        var paymentIntent = new PaymentIntent
        {
            Id = "intent-monthly",
            Kind = PaymentIntentKind.Subscription,
            Status = PaymentIntentStatus.Succeeded,
            AmountCents = 25000,
            PatientId = "patient-2",
            Provider = "stripe",
            SubjectId = "subscription-1"
        };

        var transaction = new PaymentTransaction
        {
            Id = "txn-monthly",
            PaymentIntentId = "intent-monthly",
            Type = TransactionType.Capture,
            AmountCents = 25000,
            OccurredAt = testDate
        };

        _context.PaymentIntents.Add(paymentIntent);
        _context.PaymentTransactions.Add(transaction);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetMonthlyRevenue(year, month);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<MonthlyRevenueResponse>(okResult.Value);
        Assert.Equal(250.00m, response.TotalRevenue);
        Assert.Equal(year, response.Year);
        Assert.Equal(month, response.Month);
        Assert.Equal(31, response.DailyBreakdown.Count); // August has 31 days
    }
}

// Simple test implementations
public class TestLogger<T> : ILogger<T>
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => false;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
}

public class TestMediator : IMediator
{
    private readonly RevenueMetricsService _service;

    public TestMediator(RevenueMetricsService service)
    {
        _service = service;
    }

    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        return request switch
        {
            GetDailyRevenueQuery query => (TResponse)(object)await _service.GetDailyRevenueAsync(query.Date, cancellationToken),
            GetMonthlyRevenueQuery query => (TResponse)(object)await _service.GetMonthlyRevenueAsync(query.Year, query.Month, cancellationToken),
            GetYearlyRevenueQuery query => (TResponse)(object)await _service.GetYearlyRevenueAsync(query.Year, cancellationToken),
            GetPaymentTypesQuery query => (TResponse)(object)await HandlePaymentTypesQuery(query, cancellationToken),
            _ => throw new NotSupportedException($"Request type {typeof(TResponse)} is not supported")
        };
    }

    public async Task<TResponse> Send<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest<TResponse>
    {
        return await Send((IRequest<TResponse>)request, cancellationToken);
    }

    public async Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
    {
        await Task.CompletedTask;
    }

    private async Task<PaymentTypesResponse> HandlePaymentTypesQuery(GetPaymentTypesQuery query, CancellationToken cancellationToken)
    {
        var breakdown = await _service.GetPaymentTypesBreakdownAsync(query.StartDate, query.EndDate, cancellationToken);
        
        return new PaymentTypesResponse
        {
            PaymentTypes = new List<PaymentTypeBreakdown>
            {
                new PaymentTypeBreakdown
                {
                    PaymentType = "Appointment Payments",
                    Revenue = breakdown.AppointmentPayments.Revenue,
                    PaymentCount = breakdown.AppointmentPayments.Count,
                    Percentage = breakdown.AppointmentPayments.Percentage
                },
                new PaymentTypeBreakdown
                {
                    PaymentType = "Subscription Payments",
                    Revenue = breakdown.SubscriptionPayments.Revenue,
                    PaymentCount = breakdown.SubscriptionPayments.Count,
                    Percentage = breakdown.SubscriptionPayments.Percentage
                }
            },
            TotalRevenue = breakdown.TotalRevenue,
            TotalPaymentCount = breakdown.AppointmentPayments.Count + breakdown.SubscriptionPayments.Count,
            StartDate = breakdown.StartDate,
            EndDate = breakdown.EndDate
        };
    }

    public Task<object?> Send(object request, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task Publish(object notification, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        throw new NotSupportedException();
    }
}
