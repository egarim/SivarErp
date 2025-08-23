using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Sivar.Erp.Core.Infrastructure.Logging
{
    /// <summary>
    /// Enhanced logging service specifically designed for ERP operations
    /// </summary>
    [Description("Enhanced logging service for ERP operations")]
    public interface IErpLoggingService
    {
        /// <summary>
        /// Logs document operations with structured data
        /// </summary>
        void LogDocumentOperation(string operation, string documentNumber, string documentType, 
            string userId, object? additionalData = null);

        /// <summary>
        /// Logs accounting transactions with audit trail
        /// </summary>
        void LogAccountingTransaction(string transactionId, decimal amount, string accountCode, 
            string userId, string description);

        /// <summary>
        /// Logs business entity operations
        /// </summary>
        void LogBusinessEntityOperation(string operation, string entityCode, string entityType,
            string userId, object? changes = null);

        /// <summary>
        /// Logs system performance metrics
        /// </summary>
        void LogPerformanceMetrics(string operationName, TimeSpan duration, int recordCount = 0);

        /// <summary>
        /// Logs security events
        /// </summary>
        void LogSecurityEvent(string eventType, string userId, string resource, bool success);

        /// <summary>
        /// Logs import/export operations
        /// </summary>
        void LogImportExportOperation(string operation, string fileName, int recordCount, 
            bool success, string? errorMessage = null);

        /// <summary>
        /// Logs validation errors with context
        /// </summary>
        void LogValidationError(string entityType, string entityId, List<string> errors);

        /// <summary>
        /// Starts a performance timer for operations
        /// </summary>
        IDisposable StartOperationTimer(string operationName);
    }

    /// <summary>
    /// Implementation of ERP logging service using Serilog
    /// </summary>
    [Description("ERP logging service implementation")]
    public class ErpLoggingService : IErpLoggingService
    {
        private readonly ILogger<ErpLoggingService> _logger;
        private readonly Serilog.ILogger _serilogLogger;

        public ErpLoggingService(ILogger<ErpLoggingService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serilogLogger = Log.ForContext<ErpLoggingService>();
        }

        /// <summary>
        /// Logs document operations with structured data
        /// </summary>
        /// <param name="operation">Operation type (Create, Update, Delete, Post, Cancel)</param>
        /// <param name="documentNumber">Document number</param>
        /// <param name="documentType">Type of document</param>
        /// <param name="userId">User performing the operation</param>
        /// <param name="additionalData">Additional structured data</param>
        [Description("Logs document operations")]
        public void LogDocumentOperation(string operation, string documentNumber, string documentType, 
            string userId, object? additionalData = null)
        {
            _serilogLogger.Information("Document Operation: {Operation} on {DocumentType} {DocumentNumber} by {UserId} - {Data}",
                operation, documentType, documentNumber, userId, additionalData);

            _logger.LogInformation("Document {Operation}: {DocumentType} {DocumentNumber} by {UserId}",
                operation, documentType, documentNumber, userId);
        }

        /// <summary>
        /// Logs accounting transactions with audit trail
        /// </summary>
        /// <param name="transactionId">Transaction identifier</param>
        /// <param name="amount">Transaction amount</param>
        /// <param name="accountCode">Account code</param>
        /// <param name="userId">User performing the transaction</param>
        /// <param name="description">Transaction description</param>
        [Description("Logs accounting transactions")]
        public void LogAccountingTransaction(string transactionId, decimal amount, string accountCode, 
            string userId, string description)
        {
            _serilogLogger.Information("Accounting Transaction: {TransactionId} - {Amount:C} to {AccountCode} by {UserId} - {Description}",
                transactionId, amount, accountCode, userId, description);

            _logger.LogInformation("Accounting transaction {TransactionId}: {Amount:C} to {AccountCode}",
                transactionId, amount, accountCode);
        }

        /// <summary>
        /// Logs business entity operations
        /// </summary>
        /// <param name="operation">Operation type</param>
        /// <param name="entityCode">Entity code</param>
        /// <param name="entityType">Type of entity</param>
        /// <param name="userId">User performing the operation</param>
        /// <param name="changes">Changes made to the entity</param>
        [Description("Logs business entity operations")]
        public void LogBusinessEntityOperation(string operation, string entityCode, string entityType,
            string userId, object? changes = null)
        {
            _serilogLogger.Information("Business Entity Operation: {Operation} on {EntityType} {EntityCode} by {UserId} - Changes: {Changes}",
                operation, entityType, entityCode, userId, changes);

            _logger.LogInformation("Business entity {Operation}: {EntityType} {EntityCode}",
                operation, entityType, entityCode);
        }

        /// <summary>
        /// Logs system performance metrics
        /// </summary>
        /// <param name="operationName">Name of the operation</param>
        /// <param name="duration">Operation duration</param>
        /// <param name="recordCount">Number of records processed</param>
        [Description("Logs performance metrics")]
        public void LogPerformanceMetrics(string operationName, TimeSpan duration, int recordCount = 0)
        {
            _serilogLogger.Information("Performance Metric: {OperationName} completed in {Duration}ms processing {RecordCount} records",
                operationName, duration.TotalMilliseconds, recordCount);

            if (duration.TotalSeconds > 5) // Log slow operations as warnings
            {
                _logger.LogWarning("Slow operation detected: {OperationName} took {Duration}ms", 
                    operationName, duration.TotalMilliseconds);
            }
            else
            {
                _logger.LogInformation("Operation {OperationName} completed in {Duration}ms", 
                    operationName, duration.TotalMilliseconds);
            }
        }

        /// <summary>
        /// Logs security events
        /// </summary>
        /// <param name="eventType">Type of security event</param>
        /// <param name="userId">User identifier</param>
        /// <param name="resource">Resource accessed</param>
        /// <param name="success">Whether the access was successful</param>
        [Description("Logs security events")]
        public void LogSecurityEvent(string eventType, string userId, string resource, bool success)
        {
            var logLevel = success ? LogEventLevel.Information : LogEventLevel.Warning;
            
            _serilogLogger.Write(logLevel, "Security Event: {EventType} by {UserId} on {Resource} - Success: {Success}",
                eventType, userId, resource, success);

            if (success)
            {
                _logger.LogInformation("Security: {EventType} by {UserId} on {Resource}", eventType, userId, resource);
            }
            else
            {
                _logger.LogWarning("Security: Failed {EventType} by {UserId} on {Resource}", eventType, userId, resource);
            }
        }

        /// <summary>
        /// Logs import/export operations
        /// </summary>
        /// <param name="operation">Operation type (Import/Export)</param>
        /// <param name="fileName">File name</param>
        /// <param name="recordCount">Number of records processed</param>
        /// <param name="success">Whether the operation succeeded</param>
        /// <param name="errorMessage">Error message if failed</param>
        [Description("Logs import/export operations")]
        public void LogImportExportOperation(string operation, string fileName, int recordCount, 
            bool success, string? errorMessage = null)
        {
            var logLevel = success ? LogEventLevel.Information : LogEventLevel.Error;
            
            _serilogLogger.Write(logLevel, 
                "Import/Export Operation: {Operation} of file {FileName} - {RecordCount} records - Success: {Success} - Error: {ErrorMessage}",
                operation, fileName, recordCount, success, errorMessage);

            if (success)
            {
                _logger.LogInformation("{Operation} completed: {FileName} - {RecordCount} records", 
                    operation, fileName, recordCount);
            }
            else
            {
                _logger.LogError("{Operation} failed: {FileName} - {ErrorMessage}", 
                    operation, fileName, errorMessage);
            }
        }

        /// <summary>
        /// Logs validation errors with context
        /// </summary>
        /// <param name="entityType">Type of entity being validated</param>
        /// <param name="entityId">Entity identifier</param>
        /// <param name="errors">List of validation errors</param>
        [Description("Logs validation errors")]
        public void LogValidationError(string entityType, string entityId, List<string> errors)
        {
            _serilogLogger.Warning("Validation Errors for {EntityType} {EntityId}: {Errors}",
                entityType, entityId, errors);

            _logger.LogWarning("Validation failed for {EntityType} {EntityId}: {ErrorCount} errors",
                entityType, entityId, errors.Count);
        }

        /// <summary>
        /// Starts a performance timer for operations
        /// </summary>
        /// <param name="operationName">Name of the operation to time</param>
        /// <returns>Disposable timer that logs when disposed</returns>
        [Description("Starts performance timer")]
        public IDisposable StartOperationTimer(string operationName)
        {
            return new OperationTimer(operationName, this);
        }

        /// <summary>
        /// Internal class for tracking operation timing
        /// </summary>
        private class OperationTimer : IDisposable
        {
            private readonly string _operationName;
            private readonly ErpLoggingService _loggingService;
            private readonly Stopwatch _stopwatch;

            public OperationTimer(string operationName, ErpLoggingService loggingService)
            {
                _operationName = operationName;
                _loggingService = loggingService;
                _stopwatch = Stopwatch.StartNew();
            }

            public void Dispose()
            {
                _stopwatch.Stop();
                _loggingService.LogPerformanceMetrics(_operationName, _stopwatch.Elapsed);
            }
        }
    }

    /// <summary>
    /// Extensions for easy logging service registration
    /// </summary>
    [Description("Extensions for logging service registration")]
    public static class ErpLoggingServiceExtensions
    {
        /// <summary>
        /// Adds the ERP logging service to the service collection
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <returns>Service collection for chaining</returns>
        [Description("Adds ERP logging service")]
        public static IServiceCollection AddErpLogging(this IServiceCollection services)
        {
            services.AddScoped<IErpLoggingService, ErpLoggingService>();
            return services;
        }
    }
}
