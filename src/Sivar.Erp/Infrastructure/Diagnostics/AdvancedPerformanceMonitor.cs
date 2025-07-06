using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sivar.Erp.Core.Contracts;

namespace Sivar.Erp.Infrastructure.Diagnostics
{
    /// <summary>
    /// Advanced performance monitoring service for .NET 9
    /// Integrates with OpenTelemetry and modern observability patterns
    /// </summary>
    public class AdvancedPerformanceMonitor : IDisposable
    {
        private static readonly ActivitySource ActivitySource = new("Sivar.Erp.Performance");
        private static readonly Meter Meter = new("Sivar.Erp.Metrics", "1.0.0");

        // Metrics for .NET 9 observability
        private static readonly Counter<int> OperationCounter = 
            Meter.CreateCounter<int>("erp.operations.total", "count", "Total number of operations");
        
        private static readonly Histogram<double> OperationDuration = 
            Meter.CreateHistogram<double>("erp.operations.duration", "ms", "Operation duration in milliseconds");
        
        private static readonly Histogram<long> MemoryUsage = 
            Meter.CreateHistogram<long>("erp.operations.memory", "bytes", "Memory usage per operation");

        private readonly ILogger<AdvancedPerformanceMonitor> _logger;
        private readonly IObjectDb? _objectDb;
        private readonly IPerformanceContextProvider? _contextProvider;

        public AdvancedPerformanceMonitor(
            ILogger<AdvancedPerformanceMonitor> logger,
            IObjectDb? objectDb = null,
            IPerformanceContextProvider? contextProvider = null)
        {
            _logger = logger;
            _objectDb = objectDb;
            _contextProvider = contextProvider;
        }

        /// <summary>
        /// Monitors operation with full .NET 9 observability features
        /// </summary>
        public async Task<T> MonitorOperationAsync<T>(
            string operationName,
            Func<Task<T>> operation,
            Dictionary<string, object?>? tags = null)
        {
            using var activity = ActivitySource.StartActivity(operationName);
            
            // Add context tags
            activity?.SetTag("user.id", _contextProvider?.UserId);
            activity?.SetTag("user.name", _contextProvider?.UserName);
            activity?.SetTag("session.id", _contextProvider?.SessionId);
            
            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    activity?.SetTag(tag.Key, tag.Value);
                }
            }

            var stopwatch = Stopwatch.StartNew();
            var memoryBefore = GC.GetTotalMemory(false);

            try
            {
                var result = await operation();
                stopwatch.Stop();

                await RecordSuccessMetrics(operationName, stopwatch.ElapsedMilliseconds, memoryBefore, tags);
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                await RecordErrorMetrics(operationName, stopwatch.ElapsedMilliseconds, memoryBefore, ex, tags);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Monitors synchronous operation
        /// </summary>
        public T MonitorOperation<T>(
            string operationName,
            Func<T> operation,
            Dictionary<string, object?>? tags = null)
        {
            using var activity = ActivitySource.StartActivity(operationName);
            
            // Add context tags
            activity?.SetTag("user.id", _contextProvider?.UserId);
            activity?.SetTag("user.name", _contextProvider?.UserName);
            activity?.SetTag("session.id", _contextProvider?.SessionId);
            
            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    activity?.SetTag(tag.Key, tag.Value);
                }
            }

            var stopwatch = Stopwatch.StartNew();
            var memoryBefore = GC.GetTotalMemory(false);

            try
            {
                var result = operation();
                stopwatch.Stop();

                RecordSuccessMetrics(operationName, stopwatch.ElapsedMilliseconds, memoryBefore, tags).Wait();
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                RecordErrorMetrics(operationName, stopwatch.ElapsedMilliseconds, memoryBefore, ex, tags).Wait();
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                throw;
            }
        }

        private async Task RecordSuccessMetrics(
            string operationName, 
            long elapsedMs, 
            long memoryBefore, 
            Dictionary<string, object?>? tags)
        {
            var memoryAfter = GC.GetTotalMemory(false);
            var memoryDelta = memoryAfter - memoryBefore;

            // Record metrics with proper array conversion
            var metricTags = CreateMetricTags(operationName, "success", tags);
            OperationCounter.Add(1, metricTags.ToArray());
            OperationDuration.Record(elapsedMs, metricTags.ToArray());
            MemoryUsage.Record(memoryDelta, metricTags.ToArray());

            // Log performance information
            _logger.LogInformation(
                "Operation {OperationName} completed successfully in {ElapsedMs}ms using {MemoryDelta} bytes",
                operationName, elapsedMs, memoryDelta);

            // Store in ObjectDb if available
            if (_objectDb?.PerformanceLogs != null)
            {
                var performanceLog = new PerformanceLog
                {
                    Timestamp = DateTime.UtcNow,
                    Method = operationName,
                    ExecutionTimeMs = elapsedMs,
                    MemoryDeltaBytes = memoryDelta,
                    IsSlow = elapsedMs > 1000,
                    IsMemoryIntensive = memoryDelta > 10_000_000,
                    UserName = _contextProvider?.UserName,
                    InstanceId = _contextProvider?.InstanceId
                };

                _objectDb.PerformanceLogs.Add(performanceLog);
            }
        }

        private async Task RecordErrorMetrics(
            string operationName, 
            long elapsedMs, 
            long memoryBefore, 
            Exception exception,
            Dictionary<string, object?>? tags)
        {
            var memoryAfter = GC.GetTotalMemory(false);
            var memoryDelta = memoryAfter - memoryBefore;

            // Record error metrics with proper array conversion
            var metricTags = CreateMetricTags(operationName, "error", tags);
            metricTags.Add(new("error.type", exception.GetType().Name));
            
            OperationCounter.Add(1, metricTags.ToArray());
            OperationDuration.Record(elapsedMs, metricTags.ToArray());
            MemoryUsage.Record(memoryDelta, metricTags.ToArray());

            // Log error
            _logger.LogError(exception,
                "Operation {OperationName} failed after {ElapsedMs}ms: {ErrorMessage}",
                operationName, elapsedMs, exception.Message);

            // Store error in ObjectDb if available
            if (_objectDb?.PerformanceLogs != null)
            {
                var performanceLog = new PerformanceLog
                {
                    Timestamp = DateTime.UtcNow,
                    Method = operationName,
                    ExecutionTimeMs = elapsedMs,
                    MemoryDeltaBytes = memoryDelta,
                    IsSlow = true, // Mark errors as slow for attention
                    IsMemoryIntensive = memoryDelta > 10_000_000,
                    UserName = _contextProvider?.UserName,
                    InstanceId = _contextProvider?.InstanceId
                };

                _objectDb.PerformanceLogs.Add(performanceLog);
            }
        }

        private List<KeyValuePair<string, object?>> CreateMetricTags(
            string operationName, 
            string status, 
            Dictionary<string, object?>? additionalTags)
        {
            var tags = new List<KeyValuePair<string, object?>>
            {
                new("operation.name", operationName),
                new("operation.status", status),
                new("user.id", _contextProvider?.UserId),
                new("session.id", _contextProvider?.SessionId)
            };

            if (additionalTags != null)
            {
                foreach (var tag in additionalTags)
                {
                    tags.Add(new(tag.Key, tag.Value));
                }
            }

            return tags;
        }

        /// <summary>
        /// Gets performance statistics for analysis
        /// </summary>
        public PerformanceStatistics GetStatistics(TimeSpan? period = null)
        {
            if (_objectDb?.PerformanceLogs == null)
            {
                return new PerformanceStatistics();
            }

            var cutoff = DateTime.UtcNow - (period ?? TimeSpan.FromMinutes(15));
            var logs = _objectDb.PerformanceLogs
                .Where(l => l.Timestamp >= cutoff)
                .ToList();

            return new PerformanceStatistics
            {
                TotalOperations = logs.Count,
                AverageExecutionTime = logs.Any() ? logs.Average(l => l.ExecutionTimeMs) : 0,
                MaxExecutionTime = logs.Any() ? logs.Max(l => l.ExecutionTimeMs) : 0,
                SlowOperations = logs.Count(l => l.IsSlow),
                MemoryIntensiveOperations = logs.Count(l => l.IsMemoryIntensive),
                TotalMemoryUsed = logs.Sum(l => l.MemoryDeltaBytes),
                Period = period ?? TimeSpan.FromMinutes(15)
            };
        }

        public void Dispose()
        {
            ActivitySource?.Dispose();
            Meter?.Dispose();
        }
    }

    public record PerformanceStatistics
    {
        public int TotalOperations { get; init; }
        public double AverageExecutionTime { get; init; }
        public long MaxExecutionTime { get; init; }
        public int SlowOperations { get; init; }
        public int MemoryIntensiveOperations { get; init; }
        public long TotalMemoryUsed { get; init; }
        public TimeSpan Period { get; init; }
    }
}