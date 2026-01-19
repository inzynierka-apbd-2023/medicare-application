using System.Diagnostics;
using System.Text.Json;
using Medicare.ServiceDefaults.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Medicare.ServiceDefaults.ErrorHandling;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, 
        Exception exception, 
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        var (statusCode, title, detail) = MapException(exception, traceId);

        LogException(exception, statusCode, traceId);

        var problemDetails = CreateProblemDetails(httpContext, statusCode, title, detail, exception, traceId);

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private (int StatusCode, string Title, string Detail) MapException(Exception exception, string traceId)
    {
        return exception switch
        {
            DomainException domainEx => (domainEx.StatusCode, GetTitleForStatusCode(domainEx.StatusCode), domainEx.Message),
            OperationCanceledException => (499, "Client Closed Request", "The request was cancelled by the client."),
            JsonException => (400, "Bad Request", "Invalid JSON in request body."),
            ArgumentNullException argEx => (400, "Bad Request", $"Required parameter '{argEx.ParamName}' is missing."),
            ArgumentException argEx => (400, "Bad Request", argEx.Message),
            UnauthorizedAccessException => (401, "Unauthorized", "You are not authorized to access this resource."),
            InvalidOperationException => (400, "Bad Request", exception.Message),
            _ => (500, "Internal Server Error", _environment.IsDevelopment() 
                ? exception.Message 
                : $"An unexpected error occurred. Trace ID: {traceId}")
        };
    }

    private void LogException(Exception exception, int statusCode, string traceId)
    {
        var logLevel = statusCode switch
        {
            >= 500 => LogLevel.Error,
            >= 400 => LogLevel.Warning,
            _ => LogLevel.Information
        };

        _logger.Log(
            logLevel,
            exception,
            "Exception occurred. TraceId: {TraceId}, StatusCode: {StatusCode}, Message: {Message}",
            traceId,
            statusCode,
            exception.Message);
    }

    private ProblemDetails CreateProblemDetails(
        HttpContext httpContext,
        int statusCode,
        string title,
        string detail,
        Exception exception,
        string traceId)
    {
        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path,
            Type = $"https://httpstatuses.com/{statusCode}"
        };

        problemDetails.Extensions["traceId"] = traceId;
        problemDetails.Extensions["timestamp"] = DateTime.UtcNow.ToString("o");

        if (exception is ValidationException validationEx && validationEx.Errors.Any())
        {
            problemDetails.Extensions["errors"] = validationEx.Errors;
        }

        if (_environment.IsDevelopment())
        {
            problemDetails.Extensions["exceptionType"] = exception.GetType().Name;
            problemDetails.Extensions["stackTrace"] = exception.StackTrace?.Split(Environment.NewLine);
        }

        return problemDetails;
    }

    private static string GetTitleForStatusCode(int statusCode) => statusCode switch
    {
        400 => "Bad Request",
        401 => "Unauthorized",
        403 => "Forbidden",
        404 => "Not Found",
        409 => "Conflict",
        422 => "Unprocessable Entity",
        500 => "Internal Server Error",
        503 => "Service Unavailable",
        _ => "Error"
    };
}
