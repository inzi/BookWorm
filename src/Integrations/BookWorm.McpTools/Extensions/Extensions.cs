﻿namespace BookWorm.McpTools.Extensions;

public static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        builder.AddDefaultCors();

        // Add exception handlers
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        // Configure HTTP client
        services.AddHttpServiceReference<ICatalogApi>(
            $"{Protocol.HttpOrHttps}://{Application.Catalog}",
            HealthStatus.Degraded
        );

        services
            .AddMcpServer()
            .WithHttpTransport()
            .WithToolsFromAssembly()
            .WithPromptsFromAssembly();

        services
            .AddOpenTelemetry()
            .WithTracing(b => b.AddSource("*"))
            .WithMetrics(b => b.AddMeter("*"))
            .WithLogging();
    }
}
