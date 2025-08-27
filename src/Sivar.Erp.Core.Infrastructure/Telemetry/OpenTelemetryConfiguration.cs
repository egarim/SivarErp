using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Sivar.Erp.Core.Infrastructure.Telemetry
{
    /// <summary>
    /// OpenTelemetry configuration for Sivar ERP Core
    /// Sets up tracing, metrics, and logging with Jaeger and Prometheus exporters
    /// </summary>
    public static class OpenTelemetryConfiguration
    {
        /// <summary>
        /// Configures OpenTelemetry services for the application
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="serviceName">Name of the service for telemetry</param>
        /// <param name="serviceVersion">Version of the service</param>
        /// <param name="jaegerEndpoint">Jaeger endpoint URL (optional)</param>
        /// <param name="otlpEndpoint">OTLP endpoint URL (optional)</param>
        /// <returns>Service collection for chaining</returns>
        public static IServiceCollection AddSivarErpTelemetry(
            this IServiceCollection services,
            string serviceName = "Sivar.Erp.Core",
            string serviceVersion = "1.0.0",
            string? jaegerEndpoint = null,
            string? otlpEndpoint = null)
        {
            // Configure resource information
            var resourceBuilder = ResourceBuilder.CreateDefault()
                .AddService(serviceName, serviceVersion)
                .AddAttributes(new Dictionary<string, object>
                {
                    ["service.name"] = serviceName,
                    ["service.version"] = serviceVersion,
                    ["service.instance.id"] = Environment.MachineName,
                    ["deployment.environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"
                });

            // Configure OpenTelemetry Tracing
            services.AddOpenTelemetry()
                .WithTracing(builder =>
                {
                    builder
                        .SetResourceBuilder(resourceBuilder)
                        .AddSource(SivarErpTelemetry.ActivitySourceName)
                        .AddAspNetCoreInstrumentation(options =>
                        {
                            options.RecordException = true;
                        })
                        .AddHttpClientInstrumentation(options =>
                        {
                            options.RecordException = true;
                        })
                        .AddSqlClientInstrumentation(options =>
                        {
                            options.SetDbStatementForText = true;
                            options.RecordException = true;
                        });

                    // Add exporters based on configuration
                    if (!string.IsNullOrEmpty(jaegerEndpoint))
                    {
                        builder.AddJaegerExporter(options =>
                        {
                            options.Endpoint = new Uri(jaegerEndpoint);
                        });
                    }
                    else
                    {
                        // Default Jaeger configuration for local development
                        builder.AddJaegerExporter(options =>
                        {
                            options.AgentHost = "localhost";
                            options.AgentPort = 6831;
                        });
                    }

                    if (!string.IsNullOrEmpty(otlpEndpoint))
                    {
                        builder.AddOtlpExporter(options =>
                        {
                            options.Endpoint = new Uri(otlpEndpoint);
                            options.Protocol = OtlpExportProtocol.Grpc;
                        });
                    }

                    // Add console exporter for development (disabled - not available)
                    // if (IsDevelopmentEnvironment())
                    // {
                    //     builder.AddConsoleExporter();
                    // }
                })
                .WithMetrics(builder =>
                {
                    builder
                        .SetResourceBuilder(resourceBuilder)
                        .AddMeter(SivarErpTelemetry.MeterName)
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation();

                    // Add Prometheus exporter for metrics
                    builder.AddPrometheusExporter();

                    // Add OTLP exporter if configured
                    if (!string.IsNullOrEmpty(otlpEndpoint))
                    {
                        builder.AddOtlpExporter(options =>
                        {
                            options.Endpoint = new Uri(otlpEndpoint);
                            options.Protocol = OtlpExportProtocol.Grpc;
                        });
                    }

                    // Add console exporter for development (disabled - not available)
                    // if (IsDevelopmentEnvironment())
                    // {
                    //     builder.AddConsoleExporter();
                    // }
                });

            return services;
        }

        /// <summary>
        /// Adds structured logging configuration compatible with OpenTelemetry
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <returns>Service collection for chaining</returns>
        public static IServiceCollection AddSivarErpLogging(this IServiceCollection services)
        {
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                
                // Add console logging for development
                if (IsDevelopmentEnvironment())
                {
                    builder.AddConsole();
                }

                // Add debug logging for development
                if (IsDevelopmentEnvironment())
                {
                    builder.AddDebug();
                }

                // Configure log levels
                builder.SetMinimumLevel(IsDevelopmentEnvironment() ? LogLevel.Debug : LogLevel.Information);
                
                // Add filters for noise reduction
                builder.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);
                builder.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Information);
                builder.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);
            });

            return services;
        }

        /// <summary>
        /// Configures OpenTelemetry for minimal APIs and web applications
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configuration">Configuration for endpoints and settings</param>
        /// <returns>Service collection for chaining</returns>
        public static IServiceCollection AddSivarErpObservability(
            this IServiceCollection services,
            Action<SivarErpTelemetryOptions>? configuration = null)
        {
            var options = new SivarErpTelemetryOptions();
            configuration?.Invoke(options);

            services
                .AddSivarErpTelemetry(
                    options.ServiceName,
                    options.ServiceVersion,
                    options.JaegerEndpoint,
                    options.OtlpEndpoint)
                .AddSivarErpLogging();

            // Register telemetry as singleton for application use
            services.AddSingleton<SivarErpTelemetry>();

            return services;
        }

        /// <summary>
        /// Determines if the application is running in development environment
        /// </summary>
        /// <returns>True if development environment</returns>
        private static bool IsDevelopmentEnvironment()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? 
                             Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? 
                             "Production";
            
            return string.Equals(environment, "Development", StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// Configuration options for Sivar ERP telemetry
    /// </summary>
    public class SivarErpTelemetryOptions
    {
        /// <summary>
        /// Service name for telemetry identification
        /// </summary>
        public string ServiceName { get; set; } = "Sivar.Erp.Core";

        /// <summary>
        /// Service version for telemetry identification
        /// </summary>
        public string ServiceVersion { get; set; } = "1.0.0";

        /// <summary>
        /// Jaeger endpoint URL for trace export
        /// </summary>
        public string? JaegerEndpoint { get; set; }

        /// <summary>
        /// OTLP endpoint URL for trace and metrics export
        /// </summary>
        public string? OtlpEndpoint { get; set; }

        /// <summary>
        /// Enable console exporters for development
        /// </summary>
        public bool EnableConsoleExporters { get; set; } = true;

        /// <summary>
        /// Enable detailed SQL instrumentation
        /// </summary>
        public bool EnableSqlInstrumentation { get; set; } = true;

        /// <summary>
        /// Enable HTTP client instrumentation
        /// </summary>
        public bool EnableHttpInstrumentation { get; set; } = true;

        /// <summary>
        /// Minimum log level for structured logging
        /// </summary>
        public LogLevel MinimumLogLevel { get; set; } = LogLevel.Information;
    }
}
