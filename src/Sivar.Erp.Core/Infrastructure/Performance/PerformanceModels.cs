using System.ComponentModel;

namespace Sivar.Erp.Core.Infrastructure.Performance
{
    /// <summary>
    /// Represents a time range for performance metrics
    /// </summary>
    [Description("Represents a time range for performance metrics")]
    public class TimeRange
    {
        /// <summary>
        /// Start time of the range
        /// </summary>
        [Description("Start time of the range")]
        public DateTime StartTime { get; set; }

        /// <summary>
        /// End time of the range
        /// </summary>
        [Description("End time of the range")]
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Duration of the time range
        /// </summary>
        [Description("Duration of the time range")]
        public TimeSpan Duration => EndTime - StartTime;

        /// <summary>
        /// Creates a time range for the last specified minutes
        /// </summary>
        /// <param name="minutes">Number of minutes</param>
        /// <returns>Time range for the last specified minutes</returns>
        [Description("Creates a time range for the last specified minutes")]
        public static TimeRange LastMinutes(int minutes)
        {
            var now = DateTime.UtcNow;
            return new TimeRange
            {
                StartTime = now.AddMinutes(-minutes),
                EndTime = now
            };
        }

        /// <summary>
        /// Creates a time range for the last specified hours
        /// </summary>
        /// <param name="hours">Number of hours</param>
        /// <returns>Time range for the last specified hours</returns>
        [Description("Creates a time range for the last specified hours")]
        public static TimeRange LastHours(int hours)
        {
            var now = DateTime.UtcNow;
            return new TimeRange
            {
                StartTime = now.AddHours(-hours),
                EndTime = now
            };
        }

        /// <summary>
        /// Creates a time range for today
        /// </summary>
        /// <returns>Time range for today</returns>
        [Description("Creates a time range for today")]
        public static TimeRange Today()
        {
            var today = DateTime.Today;
            return new TimeRange
            {
                StartTime = today,
                EndTime = today.AddDays(1)
            };
        }
    }

    /// <summary>
    /// Represents performance metrics for the ERP system
    /// </summary>
    [Description("Represents performance metrics for the ERP system")]
    public class PerformanceMetrics
    {
        /// <summary>
        /// Time range for these metrics
        /// </summary>
        [Description("Time range for these metrics")]
        public TimeRange TimeRange { get; set; } = new();

        /// <summary>
        /// Total number of operations performed
        /// </summary>
        [Description("Total number of operations performed")]
        public long TotalOperations { get; set; }

        /// <summary>
        /// Average operation duration in milliseconds
        /// </summary>
        [Description("Average operation duration in milliseconds")]
        public double AverageOperationDuration { get; set; }

        /// <summary>
        /// Minimum operation duration in milliseconds
        /// </summary>
        [Description("Minimum operation duration in milliseconds")]
        public double MinOperationDuration { get; set; }

        /// <summary>
        /// Maximum operation duration in milliseconds
        /// </summary>
        [Description("Maximum operation duration in milliseconds")]
        public double MaxOperationDuration { get; set; }

        /// <summary>
        /// 95th percentile operation duration in milliseconds
        /// </summary>
        [Description("95th percentile operation duration in milliseconds")]
        public double P95OperationDuration { get; set; }

        /// <summary>
        /// Total number of errors encountered
        /// </summary>
        [Description("Total number of errors encountered")]
        public long TotalErrors { get; set; }

        /// <summary>
        /// Error rate as a percentage
        /// </summary>
        [Description("Error rate as a percentage")]
        public double ErrorRate => TotalOperations > 0 ? (double)TotalErrors / TotalOperations * 100 : 0;

        /// <summary>
        /// Current memory usage in bytes
        /// </summary>
        [Description("Current memory usage in bytes")]
        public long MemoryUsage { get; set; }

        /// <summary>
        /// Peak memory usage in bytes during the time range
        /// </summary>
        [Description("Peak memory usage in bytes during the time range")]
        public long PeakMemoryUsage { get; set; }

        /// <summary>
        /// Number of garbage collections
        /// </summary>
        [Description("Number of garbage collections")]
        public int GarbageCollections { get; set; }

        /// <summary>
        /// Metrics by operation type
        /// </summary>
        [Description("Metrics by operation type")]
        public Dictionary<string, OperationMetrics> OperationMetrics { get; set; } = new();
    }

    /// <summary>
    /// Represents metrics for a specific operation type
    /// </summary>
    [Description("Represents metrics for a specific operation type")]
    public class OperationMetrics
    {
        /// <summary>
        /// Operation name
        /// </summary>
        [Description("Operation name")]
        public string OperationName { get; set; } = string.Empty;

        /// <summary>
        /// Number of times operation was executed
        /// </summary>
        [Description("Number of times operation was executed")]
        public long ExecutionCount { get; set; }

        /// <summary>
        /// Total duration of all executions in milliseconds
        /// </summary>
        [Description("Total duration of all executions in milliseconds")]
        public double TotalDuration { get; set; }

        /// <summary>
        /// Average duration per execution in milliseconds
        /// </summary>
        [Description("Average duration per execution in milliseconds")]
        public double AverageDuration => ExecutionCount > 0 ? TotalDuration / ExecutionCount : 0;

        /// <summary>
        /// Number of failed executions
        /// </summary>
        [Description("Number of failed executions")]
        public long FailureCount { get; set; }

        /// <summary>
        /// Success rate as a percentage
        /// </summary>
        [Description("Success rate as a percentage")]
        public double SuccessRate => ExecutionCount > 0 ? (double)(ExecutionCount - FailureCount) / ExecutionCount * 100 : 0;
    }

    /// <summary>
    /// Represents a performance tracking session
    /// </summary>
    [Description("Represents a performance tracking session")]
    public class PerformanceTracker : IDisposable
    {
        private readonly string _operationName;
        private readonly Action<string, TimeSpan, bool> _onComplete;
        private readonly DateTime _startTime;
        private bool _disposed;

        /// <summary>
        /// Initializes a new performance tracker
        /// </summary>
        /// <param name="operationName">Name of the operation being tracked</param>
        /// <param name="onComplete">Callback when tracking completes</param>
        public PerformanceTracker(string operationName, Action<string, TimeSpan, bool> onComplete)
        {
            _operationName = operationName;
            _onComplete = onComplete;
            _startTime = DateTime.UtcNow;
        }

        /// <summary>
        /// Completes tracking with success status
        /// </summary>
        /// <param name="success">Whether the operation was successful</param>
        [Description("Completes tracking with success status")]
        public void Complete(bool success = true)
        {
            if (!_disposed)
            {
                var duration = DateTime.UtcNow - _startTime;
                _onComplete(_operationName, duration, success);
                _disposed = true;
            }
        }

        /// <summary>
        /// Disposes the tracker and completes tracking
        /// </summary>
        public void Dispose()
        {
            Complete(true);
        }
    }
}
