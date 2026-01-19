using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Medicare.ServiceDefaults.Webhooks;

public class WebhookSignatureFilter : IAsyncActionFilter
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<WebhookSignatureFilter> _logger;

    public WebhookSignatureFilter(IConfiguration configuration, ILogger<WebhookSignatureFilter> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var attribute = context.ActionDescriptor.EndpointMetadata
            .OfType<RequireWebhookSignatureAttribute>()
            .FirstOrDefault();

        if (attribute == null)
        {
            await next();
            return;
        }

        var request = context.HttpContext.Request;
        var provider = context.RouteData.Values[attribute.ProviderRouteParam]?.ToString() ?? "default";
        var signatureHeader = request.Headers[attribute.SignatureHeader].FirstOrDefault();

        if (string.IsNullOrEmpty(signatureHeader))
        {
            if (attribute.RejectMissingSignature)
            {
                _logger.LogWarning("Webhook request rejected: missing signature header {Header} for provider {Provider}",
                    attribute.SignatureHeader, provider);
                context.Result = new UnauthorizedObjectResult(new { error = "Missing webhook signature" });
                return;
            }

            _logger.LogDebug("Webhook signature validation skipped: no signature header present");
            await next();
            return;
        }

        var secret = _configuration[$"Webhooks:Secrets:{provider}"];

        if (string.IsNullOrEmpty(secret))
        {
            _logger.LogError("Webhook secret not configured for provider {Provider}", provider);
            context.Result = new StatusCodeResult(500);
            return;
        }

        request.EnableBuffering();
        request.Body.Position = 0;

        using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0;

        var expectedSignature = ComputeHmacSha256(body, secret);

        if (!CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(signatureHeader),
            Encoding.UTF8.GetBytes(expectedSignature)))
        {
            _logger.LogWarning("Webhook signature validation failed for provider {Provider}", provider);
            context.Result = new UnauthorizedObjectResult(new { error = "Invalid webhook signature" });
            return;
        }

        _logger.LogDebug("Webhook signature validated successfully for provider {Provider}", provider);
        await next();
    }

    private static string ComputeHmacSha256(string payload, string secret)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);

        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(payloadBytes);

        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
