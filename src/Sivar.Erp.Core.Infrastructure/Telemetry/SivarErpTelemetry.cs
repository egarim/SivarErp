using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;

namespace Sivar.Erp.Core.Infrastructure.Telemetry
{
    /// <summary>
    /// Centralized telemetry and performance tracking for Sivar ERP Core
    /// Provides OpenTelemetry integration with structured logging and metrics
    /// </summary>
    public class SivarErpTelemetry
    {
        public const string ActivitySourceName = "Sivar.Erp.Core";
        public const string MeterName = "Sivar.Erp.Core.Metrics";
        
        private static readonly ActivitySource _activitySource = new(ActivitySourceName);
        private static readonly Meter Meter = new(MeterName);
        
        /// <summary>
        /// Gets the ActivitySource for creating activities
        /// </summary>
        public static ActivitySource ActivitySource => _activitySource;
        
        // Performance Counters
        private static readonly Counter<long> _operationCounter = Meter.CreateCounter<long>(
            "sivar_erp_operations_total",
            description: "Total number of operations performed");
            
        private static readonly Counter<long> _errorCounter = Meter.CreateCounter<long>(
            "sivar_erp_errors_total", 
            description: "Total number of errors encountered");
            
        private static readonly Counter<long> _warningCounter = Meter.CreateCounter<long>(
            "sivar_erp_warnings_total",
            description: "Total number of warnings logged");
            
        private static readonly Histogram<double> _operationDuration = Meter.CreateHistogram<double>(
            "sivar_erp_operation_duration_seconds",
            description: "Duration of operations in seconds");
            
        private static readonly UpDownCounter<long> _activeUsers = Meter.CreateUpDownCounter<long>(
            "sivar_erp_active_users",
            description: "Number of currently active users");
            
        private static readonly Gauge<long> _memoryUsage = Meter.CreateGauge<long>(
            "sivar_erp_memory_usage_bytes",
            description: "Current memory usage in bytes");

        // Repository-specific metrics
        private static readonly Counter<long> _repositoryOperations = Meter.CreateCounter<long>(
            "sivar_erp_repository_operations_total",
            description: "Total repository operations performed");
            
        private static readonly Histogram<double> _repositoryQueryDuration = Meter.CreateHistogram<double>(
            "sivar_erp_repository_query_duration_seconds",
            description: "Duration of repository queries in seconds");

        /// <summary>
        /// Starts a new activity for tracking operation performance
        /// </summary>
        /// <param name="operationName">Name of the operation</param>
        /// <param name="tags">Optional tags for the activity</param>
        /// <returns>Activity instance or null if not enabled</returns>
        public static Activity? StartActivity(string operationName, Dictionary<string, object?>? tags = null)
        {
            var activity = _activitySource.StartActivity(operationName);
            
            if (activity != null && tags != null)
            {
                foreach (var tag in tags)
                {
                    activity.SetTag(tag.Key, tag.Value);
                }
            }
            
            return activity;
        }

        /// <summary>
        /// Records an operation execution with timing
        /// </summary>
        /// <param name="operationType">Type of operation</param>
        /// <param name="duration">Duration in seconds</param>
        /// <param name="success">Whether the operation was successful</param>
        /// <param name="entityType">Optional entity type involved</param>
        public static void RecordOperation(string operationType, double duration, bool success = true, string? entityType = null)
        {
            var tags = new TagList();
            tags.Add("operation_type", operationType);
            tags.Add("success", success.ToString().ToLower());
            
            if (!string.IsNullOrEmpty(entityType))
            {
                tags.Add("entity_type", entityType);
            }

            _operationCounter.Add(1, tags);
            _operationDuration.Record(duration, tags);
            
            if (!success)
            {
                _errorCounter.Add(1, tags);
            }
        }

        /// <summary>
        /// Records a repository operation with timing
        /// </summary>
        /// <param name="operationType">Type of repository operation (Get, Create, Update, Delete)</param>
        /// <param name="entityType">Type of entity being operated on</param>
        /// <param name="duration">Duration in seconds</param>
        /// <param name="recordCount">Number of records affected</param>
        public static void RecordRepositoryOperation(string operationType, string entityType, double duration, int recordCount = 1)
        {
            var tags = new TagList();
            tags.Add("operation_type", operationType);
            tags.Add("entity_type", entityType);
            tags.Add("record_count", recordCount.ToString());

            _repositoryOperations.Add(1, tags);
            _repositoryQueryDuration.Record(duration, tags);
        }

        /// <summary>
        /// Records an error occurrence
        /// </summary>
        /// <param name="errorType">Type of error</param>
        /// <param name="component">Component where error occurred</param>
        public static void RecordError(string errorType, string component)
        {
            var tags = new TagList();
            tags.Add("error_type", errorType);
            tags.Add("component", component);
            
            _errorCounter.Add(1, tags);
        }

        /// <summary>
        /// Records a warning occurrence
        /// </summary>
        /// <param name="warningType">Type of warning</param>
        /// <param name="component">Component where warning occurred</param>
        public static void RecordWarning(string warningType, string component)
        {
            var tags = new TagList();
            tags.Add("warning_type", warningType);
            tags.Add("component", component);
            
            _warningCounter.Add(1, tags);
        }

        /// <summary>
        /// Updates active user count
        /// </summary>
        /// <param name="delta">Change in user count (positive for login, negative for logout)</param>
        public static void UpdateActiveUsers(int delta)
        {
            _activeUsers.Add(delta);
        }

        /// <summary>
        /// Records current memory usage
        /// </summary>
        public static void RecordMemoryUsage()
        {
            var memoryUsage = GC.GetTotalMemory(false);
            _memoryUsage.Record(memoryUsage);
        }

        /// <summary>
        /// Helper method to time an operation and record metrics
        /// </summary>
        /// <typeparam name="T">Return type of the operation</typeparam>
        /// <param name="operationName">Name of the operation</param>
        /// <param name="operation">The operation to execute</param>
        /// <param name="entityType">Optional entity type</param>
        /// <returns>Result of the operation</returns>
        public static T TrackOperation<T>(string operationName, Func<T> operation, string? entityType = null)
        {
            using var activity = StartActivity(operationName, new Dictionary<string, object?>
            {
                ["entity_type"] = entityType
            });

            var stopwatch = Stopwatch.StartNew();
            var success = true;
            
            try
            {
                return operation();
            }
            catch (Exception ex)
            {
                success = false;
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                RecordError(ex.GetType().Name, operationName);
                throw;
            }
            finally
            {
                stopwatch.Stop();
                var duration = stopwatch.Elapsed.TotalSeconds;
                RecordOperation(operationName, duration, success, entityType);
            }
        }

        /// <summary>
        /// Helper method to time an async operation and record metrics
        /// </summary>
        /// <typeparam name="T">Return type of the operation</typeparam>
        /// <param name="operationName">Name of the operation</param>
        /// <param name="operation">The async operation to execute</param>
        /// <param name="entityType">Optional entity type</param>
        /// <returns>Result of the operation</returns>
        public static async Task<T> TrackOperationAsync<T>(string operationName, Func<Task<T>> operation, string? entityType = null)
        {
            using var activity = StartActivity(operationName, new Dictionary<string, object?>
            {
                ["entity_type"] = entityType
            });

            var stopwatch = Stopwatch.StartNew();
            var success = true;
            
            try
            {
                return await operation();
            }
            catch (Exception ex)
            {
                success = false;
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                RecordError(ex.GetType().Name, operationName);
                throw;
            }
            finally
            {
                stopwatch.Stop();
                var duration = stopwatch.Elapsed.TotalSeconds;
                RecordOperation(operationName, duration, success, entityType);
            }
        }

        /// <summary>
        /// Disposes telemetry resources
        /// </summary>
        public static void Dispose()
        {
            _activitySource.Dispose();
            Meter.Dispose();
        }
    }
}
