using System.ComponentModel;
using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Sivar.Erp.Core.Infrastructure.Telemetry;

namespace Sivar.Erp.Core.Infrastructure.Performance
{
    /// <summary>
    /// In-memory implementation of performance monitoring service
    /// </summary>
    [Description("In-memory implementation of performance monitoring service")]
    public class InMemoryPerformanceMonitor : IPerformanceMonitor
    {
        private readonly ILogger<InMemoryPerformanceMonitor> _logger;
        private readonly ConcurrentQueue<PerformanceRecord> _performanceRecords;
        private readonly ConcurrentDictionary<string, OperationStats> _operationStats;
        private readonly DateTime _startTime;

        /// <summary>
        /// Initializes the performance monitor
        /// </summary>
        /// <param name="logger">Logger for the service</param>
        public InMemoryPerformanceMonitor(ILogger<InMemoryPerformanceMonitor> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _performanceRecords = new ConcurrentQueue<PerformanceRecord>();
            _operationStats = new ConcurrentDictionary<string, OperationStats>();
            _startTime = DateTime.UtcNow;
        }

        /// <summary>
        /// Starts tracking performance for an operation
        /// </summary>
        [Description("Starts tracking performance for an operation")]
        public IDisposable StartTracking(string operationName)
        {
            using var activity = SivarErpTelemetry.ActivitySource.StartActivity($"Performance.{operationName}");
            activity?.SetTag("operation.name", operationName);
            
            return new PerformanceTracker(operationName, RecordOperation);
        }

        /// <summary>
        /// Records a completed operation with its duration
        /// </summary>
        [Description("Records a completed operation with its duration")]
        public void RecordOperation(string operationName, TimeSpan duration, bool success = true)
        {
            var record = new PerformanceRecord
            {
                OperationName = operationName,
                Timestamp = DateTime.UtcNow,
                Duration = duration,
                Success = success,
                MemoryUsage = GC.GetTotalMemory(false)
            };

            _performanceRecords.Enqueue(record);

            // Update operation statistics
            _operationStats.AddOrUpdate(operationName,
                new OperationStats
                {
                    OperationName = operationName,
                    ExecutionCount = 1,
                    TotalDuration = duration.TotalMilliseconds,
                    FailureCount = success ? 0 : 1,
                    MinDuration = duration.TotalMilliseconds,
                    MaxDuration = duration.TotalMilliseconds
                },
                (key, existing) =>
                {
                    existing.ExecutionCount++;
                    existing.TotalDuration += duration.TotalMilliseconds;
                    if (!success) existing.FailureCount++;
                    existing.MinDuration = Math.Min(existing.MinDuration, duration.TotalMilliseconds);
                    existing.MaxDuration = Math.Max(existing.MaxDuration, duration.TotalMilliseconds);
                    return existing;
                });

            SivarErpTelemetry.RecordOperation(operationName, duration.TotalMilliseconds, success);

            _logger.LogDebug("Recorded operation {OperationName} - Duration: {Duration}ms, Success: {Success}",
                operationName, duration.TotalMilliseconds, success);
        }

        /// <summary>
        /// Records an error for monitoring
        /// </summary>
        [Description("Records an error for monitoring")]
        public void RecordError(string operationName, Exception error)
        {
            var record = new PerformanceRecord
            {
                OperationName = operationName,
                Timestamp = DateTime.UtcNow,
                Duration = TimeSpan.Zero,
                Success = false,
                ErrorMessage = error.Message,
                MemoryUsage = GC.GetTotalMemory(false)
            };

            _performanceRecords.Enqueue(record);

            SivarErpTelemetry.RecordError(operationName, error.GetType().Name);

            _logger.LogError(error, "Recorded error for operation {OperationName}", operationName);
        }

        /// <summary>
        /// Gets performance metrics for a specified time range
        /// </summary>
        [Description("Gets performance metrics")]
        public async Task<PerformanceMetrics> GetMetricsAsync(TimeRange timeRange)
        {
            var records = _performanceRecords
                .Where(r => r.Timestamp >= timeRange.StartTime && r.Timestamp <= timeRange.EndTime)
                .ToList();

            if (!records.Any())
            {
                return new PerformanceMetrics
                {
                    TimeRange = timeRange
                };
            }

            var successfulRecords = records.Where(r => r.Success).ToList();
            var durations = successfulRecords.Select(r => r.Duration.TotalMilliseconds).OrderBy(d => d).ToList();

            var metrics = new PerformanceMetrics
            {
                TimeRange = timeRange,
                TotalOperations = records.Count,
                TotalErrors = records.Count(r => !r.Success),
                MemoryUsage = GC.GetTotalMemory(false),
                PeakMemoryUsage = records.Max(r => r.MemoryUsage),
                GarbageCollections = GC.CollectionCount(0) + GC.CollectionCount(1) + GC.CollectionCount(2)
            };

            if (durations.Any())
            {
                metrics.AverageOperationDuration = durations.Average();
                metrics.MinOperationDuration = durations.First();
                metrics.MaxOperationDuration = durations.Last();
                
                // Calculate 95th percentile
                var p95Index = (int)Math.Ceiling(0.95 * durations.Count) - 1;
                metrics.P95OperationDuration = durations[Math.Max(0, p95Index)];
            }

            // Group by operation
            var operationGroups = records.GroupBy(r => r.OperationName);
            foreach (var group in operationGroups)
            {
                var opRecords = group.ToList();
                var opSuccessful = opRecords.Where(r => r.Success).ToList();
                
                metrics.OperationMetrics[group.Key] = new OperationMetrics
                {
                    OperationName = group.Key,
                    ExecutionCount = opRecords.Count,
                    TotalDuration = opSuccessful.Sum(r => r.Duration.TotalMilliseconds),
                    FailureCount = opRecords.Count(r => !r.Success)
                };
            }

            return await Task.FromResult(metrics);
        }

        /// <summary>
        /// Gets real-time performance metrics
        /// </summary>
        [Description("Gets real-time performance metrics")]
        public async Task<PerformanceMetrics> GetRealTimeMetricsAsync()
        {
            var timeRange = TimeRange.LastMinutes(5); // Last 5 minutes
            return await GetMetricsAsync(timeRange);
        }

        /// <summary>
        /// Gets metrics for a specific operation
        /// </summary>
        [Description("Gets metrics for a specific operation")]
        public async Task<OperationMetrics> GetOperationMetricsAsync(string operationName, TimeRange timeRange)
        {
            if (_operationStats.TryGetValue(operationName, out var stats))
            {
                return await Task.FromResult(new OperationMetrics
                {
                    OperationName = stats.OperationName,
                    ExecutionCount = stats.ExecutionCount,
                    TotalDuration = stats.TotalDuration,
                    FailureCount = stats.FailureCount
                });
            }

            return await Task.FromResult(new OperationMetrics { OperationName = operationName });
        }

        /// <summary>
        /// Clears old performance data beyond retention period
        /// </summary>
        [Description("Clears old performance data beyond retention period")]
        public async Task<int> CleanupOldDataAsync(TimeSpan retentionPeriod)
        {
            var cutoffTime = DateTime.UtcNow - retentionPeriod;
            var recordsToKeep = new Queue<PerformanceRecord>();
            var removedCount = 0;

            // This is a simplified cleanup - in a real implementation you'd need a more sophisticated approach
            while (_performanceRecords.TryDequeue(out var record))
            {
                if (record.Timestamp >= cutoffTime)
                {
                    recordsToKeep.Enqueue(record);
                }
                else
                {
                    removedCount++;
                }
            }

            // Re-enqueue the records to keep
            foreach (var record in recordsToKeep)
            {
                _performanceRecords.Enqueue(record);
            }

            _logger.LogInformation("Cleaned up {RemovedCount} performance records older than {RetentionPeriod}",
                removedCount, retentionPeriod);

            return await Task.FromResult(removedCount);
        }

        /// <summary>
        /// Gets current system health status
        /// </summary>
        [Description("Gets current system health status")]
        public async Task<SystemHealth> GetSystemHealthAsync()
        {
            var recentMetrics = await GetRealTimeMetricsAsync();
            var memoryUsage = GC.GetTotalMemory(false);
            var uptime = DateTime.UtcNow - _startTime;

            var health = new SystemHealth
            {
                Status = HealthStatus.Healthy,
                MemoryUsage = memoryUsage,
                AvailableMemory = Math.Max(0, 512 * 1024 * 1024 - memoryUsage), // Assume 512MB limit
                ErrorRate = recentMetrics.ErrorRate,
                AverageResponseTime = recentMetrics.AverageOperationDuration,
                Uptime = uptime,
                LastCheckTime = DateTime.UtcNow,
                ActiveConnections = 1 // Simplified for in-memory implementation
            };

            // Determine health status based on metrics
            if (health.ErrorRate > 10)
            {
                health.Status = HealthStatus.Unhealthy;
            }
            else if (health.ErrorRate > 5 || health.AverageResponseTime > 1000)
            {
                health.Status = HealthStatus.Degraded;
            }
            else if (health.MemoryUsagePercentage > 80)
            {
                health.Status = HealthStatus.Warning;
            }

            // Add health checks
            health.HealthChecks.AddRange(await PerformHealthChecksAsync());

            return health;
        }

        /// <summary>
        /// Performs individual health checks
        /// </summary>
        private async Task<List<HealthCheckResult>> PerformHealthChecksAsync()
        {
            var checks = new List<HealthCheckResult>();

            // Memory check
            var memoryUsage = GC.GetTotalMemory(false);
            var memoryCheckStart = DateTime.UtcNow;
            var memoryStatus = memoryUsage > 256 * 1024 * 1024 ? HealthStatus.Warning : HealthStatus.Healthy;
            
            checks.Add(new HealthCheckResult
            {
                Name = "Memory",
                Status = memoryStatus,
                Description = $"Memory usage: {memoryUsage / 1024 / 1024:F1} MB",
                Duration = DateTime.UtcNow - memoryCheckStart,
                Data = new Dictionary<string, object> { ["MemoryBytes"] = memoryUsage }
            });

            // Repository check
            var repoCheckStart = DateTime.UtcNow;
            checks.Add(new HealthCheckResult
            {
                Name = "Repository",
                Status = HealthStatus.Healthy,
                Description = "Repository is accessible",
                Duration = DateTime.UtcNow - repoCheckStart
            });

            return await Task.FromResult(checks);
        }

        /// <summary>
        /// Internal performance record structure
        /// </summary>
        private class PerformanceRecord
        {
            public string OperationName { get; set; } = string.Empty;
            public DateTime Timestamp { get; set; }
            public TimeSpan Duration { get; set; }
            public bool Success { get; set; }
            public string? ErrorMessage { get; set; }
            public long MemoryUsage { get; set; }
        }

        /// <summary>
        /// Internal operation statistics
        /// </summary>
        private class OperationStats
        {
            public string OperationName { get; set; } = string.Empty;
            public long ExecutionCount { get; set; }
            public double TotalDuration { get; set; }
            public long FailureCount { get; set; }
            public double MinDuration { get; set; }
            public double MaxDuration { get; set; }
        }
    }
}
