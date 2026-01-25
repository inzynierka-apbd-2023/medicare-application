using System.Threading.RateLimiting;
using Medicare.ServiceDefaults.ErrorHandling;
using Medicare.ServiceDefaults.Webhooks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Microsoft.Extensions.Hosting;

using MassTransit;
using Medicare.Messaging.Contracts;
using Microsoft.EntityFrameworkCore;

public static class Extensions
{
    public static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder)
    {
        builder.ConfigureOpenTelemetry();

        builder.AddDefaultHealthChecks();

        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler();
            http.AddServiceDiscovery();
        });

        builder.AddGlobalExceptionHandling();
        builder.AddDefaultRateLimiting();

        return builder;
    }

    public static IHostApplicationBuilder AddGlobalExceptionHandling(this IHostApplicationBuilder builder)
    {
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();

        return builder;
    }

    public static WebApplication UseGlobalExceptionHandling(this WebApplication app)
    {
        app.UseExceptionHandler();

        return app;
    }

    public static IHostApplicationBuilder AddDefaultRateLimiting(this IHostApplicationBuilder builder)
    {
        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                var factory = RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: partition =>
                    {
                        var section = builder.Configuration.GetSection("RateLimiting");
                        var permitLimit = section.GetValue<int?>("PermitLimit") ?? 100;
                        var windowSeconds = section.GetValue<int?>("WindowSeconds") ?? 60;
                        var queueLimit = section.GetValue<int?>("QueueLimit") ?? 5;

                        return new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = permitLimit,
                            Window = TimeSpan.FromSeconds(windowSeconds),
                            QueueLimit = queueLimit,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                        };
                    });

                return factory;
            });
        });

        return builder;
    }

    public static WebApplication UseDefaultRateLimiting(this WebApplication app)
    {
        app.UseRateLimiter();
        return app;
    }

    public static IHostApplicationBuilder ConfigureOpenTelemetry(this IHostApplicationBuilder builder)
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();
            })
            .WithTracing(tracing =>
            {
                tracing.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();
            });

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    private static IHostApplicationBuilder AddOpenTelemetryExporters(this IHostApplicationBuilder builder)
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            builder.Services.AddOpenTelemetry()
                .WithMetrics(metrics => metrics.AddOtlpExporter())
                .WithTracing(tracing => tracing.AddOtlpExporter());
        }

        return builder;
    }

    public static IHostApplicationBuilder AddDefaultHealthChecks(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
        }

        app.MapHealthChecks("/health");

        app.MapHealthChecks("/alive", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("live")
        });

        return app;
    }

    public static IServiceCollection AddWebhookSignatureValidation(this IServiceCollection services)
    {
        services.AddScoped<WebhookSignatureFilter>();
        services.Configure<Microsoft.AspNetCore.Mvc.MvcOptions>(options =>
        {
            options.Filters.Add<WebhookSignatureFilter>();
        });

        return services;
    }

    public static IHostApplicationBuilder AddMedicareMassTransit<TDbContext>(this IHostApplicationBuilder builder, Action<IBusRegistrationConfigurator>? configure = null)
        where TDbContext : DbContext
    {
        builder.Services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();

            // Configure Outbox with EF Core
            x.AddEntityFrameworkOutbox<TDbContext>(o =>
            {
                o.QueryDelay = TimeSpan.FromSeconds(5);
                o.UseSqlServer();
                o.UseBusOutbox();
            });

            configure?.Invoke(x);

            x.UsingRabbitMq((context, cfg) =>
            {
                var connectionString = builder.Configuration.GetConnectionString("rabbitmq");
                cfg.Host(connectionString);
                
                cfg.ConfigureEndpoints(context);
            });
        });

        return builder;
    }

    public static IHostApplicationBuilder AddMedicareMassTransit(this IHostApplicationBuilder builder, Action<IBusRegistrationConfigurator>? configure = null)
    {
        builder.Services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();

            configure?.Invoke(x);

            x.UsingRabbitMq((context, cfg) =>
            {
                var connectionString = builder.Configuration.GetConnectionString("rabbitmq");
                cfg.Host(connectionString);
                
                cfg.ConfigureEndpoints(context);
            });
        });

        return builder;
    }
}
