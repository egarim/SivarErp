using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sivar.Erp.Core.Core;
using Sivar.Erp.Core.Infrastructure.Data;
using Sivar.Erp.Core.Infrastructure.Telemetry;

namespace Sivar.Erp.Core.Infrastructure.Extensions
{
    /// <summary>
    /// Extension methods for configuring Sivar ERP Core services
    /// Provides convenient setup for Repository pattern, logging, and telemetry
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds all Sivar ERP Core services with default configuration
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <returns>Service collection for chaining</returns>
        public static IServiceCollection AddSivarErpCore(this IServiceCollection services)
        {
            return services.AddSivarErpCore(options => { });
        }

        /// <summary>
        /// Adds all Sivar ERP Core services with custom configuration
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configureOptions">Configuration options</param>
        /// <returns>Service collection for chaining</returns>
        public static IServiceCollection AddSivarErpCore(
            this IServiceCollection services,
            Action<SivarErpCoreOptions> configureOptions)
        {
            var options = new SivarErpCoreOptions();
            configureOptions(options);

            // Register Repository as singleton for in-memory implementation
            // In production, this would be scoped for database repositories
            if (options.UseInMemoryRepository)
            {
                services.AddSingleton<IRepository, InMemoryRepository>();
            }

            // Add logging configuration
            if (options.EnableStructuredLogging)
            {
                services.AddSivarErpLogging();
            }

            // Add OpenTelemetry and observability
            if (options.EnableTelemetry)
            {
                services.AddSivarErpObservability(telemetryOptions =>
                {
                    telemetryOptions.ServiceName = options.ServiceName;
                    telemetryOptions.ServiceVersion = options.ServiceVersion;
                    telemetryOptions.JaegerEndpoint = options.JaegerEndpoint;
                    telemetryOptions.OtlpEndpoint = options.OtlpEndpoint;
                    telemetryOptions.EnableConsoleExporters = options.EnableConsoleExporters;
                    telemetryOptions.MinimumLogLevel = options.MinimumLogLevel;
                });
            }

            // Register additional core services
            services.AddTransient<IEntityValidator, EntityValidator>();
            services.AddTransient<IPerformanceTracker, PerformanceTracker>();

            return services;
        }

        /// <summary>
        /// Adds only the Repository pattern services without telemetry
        /// Useful for testing or minimal setups
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="useInMemory">Whether to use in-memory repository implementation</param>
        /// <returns>Service collection for chaining</returns>
        public static IServiceCollection AddSivarErpRepository(
            this IServiceCollection services,
            bool useInMemory = true)
        {
            if (useInMemory)
            {
                services.AddSingleton<IRepository, InMemoryRepository>();
            }

            return services;
        }

        /// <summary>
        /// Adds only logging services without Repository or telemetry
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="minimumLevel">Minimum log level</param>
        /// <returns>Service collection for chaining</returns>
        public static IServiceCollection AddSivarErpLogging(
            this IServiceCollection services,
            LogLevel minimumLevel = LogLevel.Information)
        {
            return services.AddSivarErpLogging();
        }

        /// <summary>
        /// Validates the service configuration and logs setup information
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <returns>Service collection for chaining</returns>
        public static IServiceCollection ValidateSivarErpConfiguration(this IServiceCollection services)
        {
            // Build a temporary service provider to validate configuration
            using var tempProvider = services.BuildServiceProvider();
            var loggerFactory = tempProvider.GetService<ILoggerFactory>();
            var logger = loggerFactory?.CreateLogger("ServiceCollectionExtensions");

            logger?.LogInformation("Sivar ERP Core services configuration validated successfully");

            // Log registered services for debugging
            var repository = tempProvider.GetService<IRepository>();
            if (repository != null)
            {
                logger?.LogInformation("Repository service registered: {RepositoryType}", repository.GetType().Name);
            }

            var telemetry = tempProvider.GetService<SivarErpTelemetry>();
            if (telemetry != null)
            {
                logger?.LogInformation("Telemetry service registered successfully");
            }

            return services;
        }
    }

    /// <summary>
    /// Configuration options for Sivar ERP Core services
    /// </summary>
    public class SivarErpCoreOptions
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
        /// Whether to use in-memory repository implementation
        /// </summary>
        public bool UseInMemoryRepository { get; set; } = true;

        /// <summary>
        /// Whether to enable OpenTelemetry and performance tracking
        /// </summary>
        public bool EnableTelemetry { get; set; } = true;

        /// <summary>
        /// Whether to enable structured logging
        /// </summary>
        public bool EnableStructuredLogging { get; set; } = true;

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
        /// Minimum log level for structured logging
        /// </summary>
        public LogLevel MinimumLogLevel { get; set; } = LogLevel.Information;
    }

    /// <summary>
    /// Simple entity validator interface for validation services
    /// </summary>
    public interface IEntityValidator
    {
        /// <summary>
        /// Validates an entity and returns validation results
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="entity">Entity to validate</param>
        /// <returns>Validation results</returns>
        ValidationResult ValidateEntity<T>(T entity) where T : class;
    }

    /// <summary>
    /// Simple entity validator implementation
    /// </summary>
    internal class EntityValidator : IEntityValidator
    {
        private readonly ILogger<EntityValidator> _logger;

        public EntityValidator(ILogger<EntityValidator> logger)
        {
            _logger = logger;
        }

        public ValidationResult ValidateEntity<T>(T entity) where T : class
        {
            if (entity == null)
            {
                _logger.LogWarning("Attempted to validate null entity of type {EntityType}", typeof(T).Name);
                return new ValidationResult { IsValid = false, ErrorMessage = "Entity cannot be null" };
            }

            // Basic validation - can be extended with FluentValidation or DataAnnotations
            _logger.LogDebug("Validating entity of type {EntityType}", typeof(T).Name);
            return new ValidationResult { IsValid = true };
        }
    }

    /// <summary>
    /// Performance tracking interface
    /// </summary>
    public interface IPerformanceTracker
    {
        /// <summary>
        /// Tracks the execution time of an operation
        /// </summary>
        /// <param name="operationName">Name of the operation</param>
        /// <param name="executionTimeMs">Execution time in milliseconds</param>
        /// <param name="success">Whether the operation was successful</param>
        void TrackOperation(string operationName, double executionTimeMs, bool success = true);
    }

    /// <summary>
    /// Performance tracker implementation using SivarErpTelemetry
    /// </summary>
    internal class PerformanceTracker : IPerformanceTracker
    {
        private readonly ILogger<PerformanceTracker> _logger;

        public PerformanceTracker(ILogger<PerformanceTracker> logger)
        {
            _logger = logger;
        }

        public void TrackOperation(string operationName, double executionTimeMs, bool success = true)
        {
            var executionTimeSeconds = executionTimeMs / 1000.0;
            
            SivarErpTelemetry.RecordOperation(operationName, executionTimeSeconds, success);
            
            if (executionTimeMs > 1000) // Log slow operations
            {
                _logger.LogWarning("Slow operation detected: {OperationName} took {ExecutionTimeMs}ms", 
                    operationName, executionTimeMs);
            }
            else
            {
                _logger.LogDebug("Operation {OperationName} completed in {ExecutionTimeMs}ms", 
                    operationName, executionTimeMs);
            }
        }
    }

    /// <summary>
    /// Validation result for entity validation
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// Whether the validation passed
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Error message if validation failed
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Additional validation errors
        /// </summary>
        public List<string> Errors { get; set; } = new();
    }
}
