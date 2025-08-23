using System.ComponentModel;

namespace Sivar.Erp.Core.Infrastructure.Performance
{
    /// <summary>
    /// Service for monitoring system performance and tracking operation metrics
    /// </summary>
    [Description("Service for monitoring system performance")]
    public interface IPerformanceMonitor
    {
        /// <summary>
        /// Starts tracking performance for an operation
        /// </summary>
        /// <param name="operationName">Name of the operation to track</param>
        /// <returns>Disposable tracker that automatically records metrics when disposed</returns>
        [Description("Starts tracking performance for an operation")]
        IDisposable StartTracking([Description("Operation name")] string operationName);

        /// <summary>
        /// Records a completed operation with its duration
        /// </summary>
        /// <param name="operationName">Name of the operation</param>
        /// <param name="duration">Duration of the operation</param>
        /// <param name="success">Whether the operation was successful</param>
        [Description("Records a completed operation with its duration")]
        void RecordOperation(
            [Description("Operation name")] string operationName,
            [Description("Operation duration")] TimeSpan duration,
            [Description("Success status")] bool success = true);

        /// <summary>
        /// Records an error for monitoring
        /// </summary>
        /// <param name="operationName">Name of the operation where error occurred</param>
        /// <param name="error">Error details</param>
        [Description("Records an error for monitoring")]
        void RecordError(
            [Description("Operation name")] string operationName,
            [Description("Error details")] Exception error);

        /// <summary>
        /// Gets performance metrics for a specified time range
        /// </summary>
        /// <param name="timeRange">Time range for metrics</param>
        /// <returns>Aggregated performance metrics</returns>
        [Description("Gets performance metrics")]
        Task<PerformanceMetrics> GetMetricsAsync(
            [Description("Time range for metrics")] TimeRange timeRange);

        /// <summary>
        /// Gets real-time performance metrics
        /// </summary>
        /// <returns>Current performance metrics</returns>
        [Description("Gets real-time performance metrics")]
        Task<PerformanceMetrics> GetRealTimeMetricsAsync();

        /// <summary>
        /// Gets metrics for a specific operation
        /// </summary>
        /// <param name="operationName">Name of the operation</param>
        /// <param name="timeRange">Time range for metrics</param>
        /// <returns>Operation-specific metrics</returns>
        [Description("Gets metrics for a specific operation")]
        Task<OperationMetrics> GetOperationMetricsAsync(
            [Description("Operation name")] string operationName,
            [Description("Time range for metrics")] TimeRange timeRange);

        /// <summary>
        /// Clears old performance data beyond retention period
        /// </summary>
        /// <param name="retentionPeriod">How long to keep performance data</param>
        /// <returns>Number of records cleaned up</returns>
        [Description("Clears old performance data beyond retention period")]
        Task<int> CleanupOldDataAsync([Description("Retention period")] TimeSpan retentionPeriod);

        /// <summary>
        /// Gets current system health status
        /// </summary>
        /// <returns>System health information</returns>
        [Description("Gets current system health status")]
        Task<SystemHealth> GetSystemHealthAsync();
    }

    /// <summary>
    /// Represents system health status
    /// </summary>
    [Description("Represents system health status")]
    public class SystemHealth
    {
        /// <summary>
        /// Overall health status
        /// </summary>
        [Description("Overall health status")]
        public HealthStatus Status { get; set; }

        /// <summary>
        /// Current CPU usage percentage
        /// </summary>
        [Description("Current CPU usage percentage")]
        public double CpuUsage { get; set; }

        /// <summary>
        /// Current memory usage in bytes
        /// </summary>
        [Description("Current memory usage in bytes")]
        public long MemoryUsage { get; set; }

        /// <summary>
        /// Available memory in bytes
        /// </summary>
        [Description("Available memory in bytes")]
        public long AvailableMemory { get; set; }

        /// <summary>
        /// Memory usage percentage
        /// </summary>
        [Description("Memory usage percentage")]
        public double MemoryUsagePercentage => AvailableMemory > 0 ? (double)MemoryUsage / (MemoryUsage + AvailableMemory) * 100 : 0;

        /// <summary>
        /// Number of active database connections
        /// </summary>
        [Description("Number of active database connections")]
        public int ActiveConnections { get; set; }

        /// <summary>
        /// Current error rate
        /// </summary>
        [Description("Current error rate")]
        public double ErrorRate { get; set; }

        /// <summary>
        /// Average response time in milliseconds
        /// </summary>
        [Description("Average response time in milliseconds")]
        public double AverageResponseTime { get; set; }

        /// <summary>
        /// System uptime
        /// </summary>
        [Description("System uptime")]
        public TimeSpan Uptime { get; set; }

        /// <summary>
        /// Last health check timestamp
        /// </summary>
        [Description("Last health check timestamp")]
        public DateTime LastCheckTime { get; set; }

        /// <summary>
        /// Health check details
        /// </summary>
        [Description("Health check details")]
        public List<HealthCheckResult> HealthChecks { get; set; } = new();
    }

    /// <summary>
    /// Health status enumeration
    /// </summary>
    [Description("Health status enumeration")]
    public enum HealthStatus
    {
        /// <summary>
        /// System is healthy
        /// </summary>
        [Description("System is healthy")]
        Healthy,

        /// <summary>
        /// System has warnings but is functional
        /// </summary>
        [Description("System has warnings but is functional")]
        Warning,

        /// <summary>
        /// System is degraded
        /// </summary>
        [Description("System is degraded")]
        Degraded,

        /// <summary>
        /// System is unhealthy
        /// </summary>
        [Description("System is unhealthy")]
        Unhealthy
    }

    /// <summary>
    /// Represents a health check result
    /// </summary>
    [Description("Represents a health check result")]
    public class HealthCheckResult
    {
        /// <summary>
        /// Name of the health check
        /// </summary>
        [Description("Name of the health check")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Health check status
        /// </summary>
        [Description("Health check status")]
        public HealthStatus Status { get; set; }

        /// <summary>
        /// Description of the check result
        /// </summary>
        [Description("Description of the check result")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Duration of the health check
        /// </summary>
        [Description("Duration of the health check")]
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Additional data from the health check
        /// </summary>
        [Description("Additional data from the health check")]
        public Dictionary<string, object> Data { get; set; } = new();
    }
}
