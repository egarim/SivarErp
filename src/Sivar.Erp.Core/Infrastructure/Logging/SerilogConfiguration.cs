using System.ComponentModel;
using Serilog;
using Serilog.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


namespace Sivar.Erp.Core.Infrastructure.Logging
{
    /// <summary>
    /// Serilog configuration and setup for ERP system
    /// </summary>
    [Description("Serilog configuration and setup for ERP system")]
    public static class SerilogConfiguration
    {
        /// <summary>
        /// Configures Serilog with multiple sinks including file, console, debug, and Seq
        /// </summary>
        /// <param name="hostBuilder">Host builder</param>
        /// <param name="configuration">Configuration instance</param>
        /// <returns>Host builder for chaining</returns>
        [Description("Configures Serilog with multiple sinks")]
        public static IHostBuilder UseSerilogLogging(this IHostBuilder hostBuilder, IConfiguration? configuration = null)
        {
            return hostBuilder.UseSerilog((context, services, loggerConfiguration) =>
            {
                var config = configuration ?? context.Configuration;
                ConfigureSerilog(loggerConfiguration, config, context.HostingEnvironment);
            });
        }

        /// <summary>
        /// Configures Serilog for services without host builder
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configuration">Configuration instance</param>
        /// <param name="environment">Hosting environment</param>
        /// <returns>Service collection for chaining</returns>
        [Description("Configures Serilog for services")]
        public static IServiceCollection AddSerilogLogging(this IServiceCollection services, 
            IConfiguration configuration, IHostEnvironment? environment = null)
        {
            var loggerConfiguration = new LoggerConfiguration();
            ConfigureSerilog(loggerConfiguration, configuration, environment);
            
            Log.Logger = loggerConfiguration.CreateLogger();
            services.AddSingleton(Log.Logger);
            
            return services;
        }

        /// <summary>
        /// Core Serilog configuration method
        /// </summary>
        /// <param name="loggerConfiguration">Logger configuration</param>
        /// <param name="configuration">App configuration</param>
        /// <param name="environment">Hosting environment</param>
        [Description("Core Serilog configuration method")]
        private static void ConfigureSerilog(LoggerConfiguration loggerConfiguration, 
            IConfiguration configuration, IHostEnvironment? environment)
        {
            var logLevel = GetLogLevel(configuration, environment);
            var logPath = GetLogPath(configuration);
            var seqServerUrl = configuration["Logging:Seq:ServerUrl"];
            var seqApiKey = configuration["Logging:Seq:ApiKey"];

            loggerConfiguration
                .MinimumLevel.Is(logLevel)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .Enrich.WithEnvironmentName()
                .Enrich.WithProperty("Application", "Sivar.Erp")
                .Enrich.WithProperty("Version", GetApplicationVersion());

            // Console sink - always enabled
            loggerConfiguration.WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}");

            // Debug sink - enabled in development
            if (environment?.IsDevelopment() ?? true)
            {
                loggerConfiguration.WriteTo.Debug(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}");
            }

            // File sink - always enabled with rolling files
            loggerConfiguration.WriteTo.File(
                path: logPath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                shared: true,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}");

            // Structured JSON file sink for better parsing
            loggerConfiguration.WriteTo.File(
                new Serilog.Formatting.Compact.CompactJsonFormatter(),
                path: logPath.Replace(".log", "-structured.json"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                shared: true);

            // Seq sink - if configured
            if (!string.IsNullOrWhiteSpace(seqServerUrl))
            {
                if (!string.IsNullOrWhiteSpace(seqApiKey))
                {
                    loggerConfiguration.WriteTo.Seq(seqServerUrl, apiKey: seqApiKey);
                }
                else
                {
                    loggerConfiguration.WriteTo.Seq(seqServerUrl);
                }
            }

            // Error file sink for errors and fatals only
            loggerConfiguration.WriteTo.File(
                path: logPath.Replace(".log", "-errors.log"),
                restrictedToMinimumLevel: LogEventLevel.Error,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 90,
                shared: true,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}");
        }

        /// <summary>
        /// Gets the log level from configuration
        /// </summary>
        /// <param name="configuration">Configuration instance</param>
        /// <param name="environment">Hosting environment</param>
        /// <returns>Log event level</returns>
        [Description("Gets the log level from configuration")]
        private static LogEventLevel GetLogLevel(IConfiguration configuration, IHostEnvironment? environment)
        {
            var logLevelString = configuration["Logging:LogLevel:Default"] ?? 
                                configuration["Logging:Default"] ?? 
                                "Information";

            if (environment?.IsDevelopment() ?? false)
            {
                logLevelString = configuration["Logging:LogLevel:Development"] ?? logLevelString;
            }

            return logLevelString.ToUpperInvariant() switch
            {
                "VERBOSE" => LogEventLevel.Verbose,
                "DEBUG" => LogEventLevel.Debug,
                "INFORMATION" => LogEventLevel.Information,
                "WARNING" => LogEventLevel.Warning,
                "ERROR" => LogEventLevel.Error,
                "FATAL" => LogEventLevel.Fatal,
                _ => LogEventLevel.Information
            };
        }

        /// <summary>
        /// Gets the log file path from configuration
        /// </summary>
        /// <param name="configuration">Configuration instance</param>
        /// <returns>Log file path</returns>
        [Description("Gets the log file path from configuration")]
        private static string GetLogPath(IConfiguration configuration)
        {
            var logPath = configuration["Logging:FilePath"] ?? "logs/sivar-erp-.log";
            
            // Ensure the directory exists
            var directory = Path.GetDirectoryName(logPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            return logPath;
        }

        /// <summary>
        /// Gets the application version for logging enrichment
        /// </summary>
        /// <returns>Application version string</returns>
        [Description("Gets the application version")]
        private static string GetApplicationVersion()
        {
            try
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var version = assembly.GetName().Version;
                return version?.ToString() ?? "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }
    }

    /// <summary>
    /// Extensions for enhanced logging capabilities
    /// </summary>
    [Description("Extensions for enhanced logging capabilities")]
    public static class LoggingExtensions
    {
        /// <summary>
        /// Logs structured ERP operation information
        /// </summary>
        /// <param name="logger">Logger instance</param>
        /// <param name="operation">Operation name</param>
        /// <param name="entityType">Entity type</param>
        /// <param name="entityId">Entity identifier</param>
        /// <param name="userId">User identifier</param>
        /// <param name="additionalData">Additional structured data</param>
        [Description("Logs structured ERP operation")]
        public static void LogErpOperation(this ILogger logger, string operation, string entityType, 
            string entityId, string? userId = null, object? additionalData = null)
        {
            logger.Information("ERP Operation: {Operation} on {EntityType} {EntityId} by {UserId} - {Data}",
                operation, entityType, entityId, userId ?? "System", additionalData);
        }

        /// <summary>
        /// Logs business process events with context
        /// </summary>
        /// <param name="logger">Logger instance</param>
        /// <param name="processName">Business process name</param>
        /// <param name="stepName">Process step name</param>
        /// <param name="status">Step status</param>
        /// <param name="duration">Step duration</param>
        /// <param name="context">Process context</param>
        [Description("Logs business process events")]
        public static void LogBusinessProcess(this ILogger logger, string processName, string stepName, 
            string status, TimeSpan? duration = null, object? context = null)
        {
            logger.Information("Business Process: {ProcessName}.{StepName} - {Status} ({Duration}ms) - {Context}",
                processName, stepName, status, duration?.TotalMilliseconds ?? 0, context);
        }

        /// <summary>
        /// Logs audit events for compliance
        /// </summary>
        /// <param name="logger">Logger instance</param>
        /// <param name="auditType">Type of audit event</param>
        /// <param name="resourceType">Resource being audited</param>
        /// <param name="resourceId">Resource identifier</param>
        /// <param name="action">Action performed</param>
        /// <param name="userId">User performing action</param>
        /// <param name="beforeValue">Value before change</param>
        /// <param name="afterValue">Value after change</param>
        [Description("Logs audit events for compliance")]
        public static void LogAuditEvent(this ILogger logger, string auditType, string resourceType, 
            string resourceId, string action, string userId, object? beforeValue = null, object? afterValue = null)
        {
            logger.Information("Audit: {AuditType} - {ResourceType} {ResourceId} {Action} by {UserId} - Before: {Before} After: {After}",
                auditType, resourceType, resourceId, action, userId, beforeValue, afterValue);
        }

        /// <summary>
        /// Logs performance metrics
        /// </summary>
        /// <param name="logger">Logger instance</param>
        /// <param name="operationName">Name of the operation</param>
        /// <param name="duration">Operation duration</param>
        /// <param name="itemCount">Number of items processed</param>
        /// <param name="success">Whether operation succeeded</param>
        [Description("Logs performance metrics")]
        public static void LogPerformanceMetric(this ILogger logger, string operationName, 
            TimeSpan duration, int itemCount = 0, bool success = true)
        {
            logger.Information("Performance: {OperationName} completed in {Duration}ms - Items: {ItemCount} - Success: {Success}",
                operationName, duration.TotalMilliseconds, itemCount, success);
        }
    }
}
