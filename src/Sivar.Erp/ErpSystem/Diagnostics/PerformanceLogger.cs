using Microsoft.Extensions.Logging;
using Sivar.Erp.ErpSystem.Diagnostics;
using Sivar.Erp.Services;
using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Threading.Tasks;

namespace Sivar.Erp.ErpSystem.Diagnostics
{
    public class PerformanceLogger<T>
    {
        private static readonly Meter Meter = new("ERP.Performance", "1.0.0");

        private static readonly Histogram<long> ExecutionTimeHistogram =
            Meter.CreateHistogram<long>("method.execution.time.ms", "ms", "Execution time per method");

        private static readonly Histogram<long> MemoryUsageHistogram =
            Meter.CreateHistogram<long>("method.memory.allocated.bytes", "bytes", "Memory allocated per method");

        private static readonly ObservableGauge<long> GcMemoryGauge =
           Meter.CreateObservableGauge("gc.memory.bytes", () =>
               new Measurement<long>(
                   GC.GetTotalMemory(false),
                   new[] { new KeyValuePair<string, object?>("unit", "bytes") }
               ),
               "bytes", // Unit (optional metadata, not the tag!)
               "Total managed heap memory"
           );


        private readonly ILogger<T> _logger;
        private readonly PerformanceLogMode _logMode;
        private readonly int _slowThresholdMs;
        private readonly long _memoryThresholdBytes;
        private readonly IObjectDb _objectDb;

        public PerformanceLogger(ILogger<T> logger,
                                 PerformanceLogMode logMode = PerformanceLogMode.All,
                                 int slowThresholdMs = 100,
                                 long memoryThresholdBytes = 10_000_000,
                                 IObjectDb objectDb = null)
        {
            _logger = logger;
            _logMode = logMode;
            _slowThresholdMs = slowThresholdMs;
            _memoryThresholdBytes = memoryThresholdBytes;
            _objectDb = objectDb;
        }

        public void Track(string methodName, Action action)
        {
            long memoryBefore = GC.GetTotalMemory(false);
            var stopwatch = Stopwatch.StartNew();

            try
            {
                action();
            }
            catch (Exception ex)
            {
                if (_logMode.HasFlag(PerformanceLogMode.Narrative))
                    _logger.LogError(ex, "Error executing {Method}", methodName);
                throw;
            }

            stopwatch.Stop();
            long memoryAfter = GC.GetTotalMemory(false);
            long memoryDelta = memoryAfter - memoryBefore;
            
            RecordPerformanceMetrics(methodName, stopwatch.ElapsedMilliseconds, memoryDelta);
        }

        public TResult Track<TResult>(string methodName, Func<TResult> func)
        {
            long memoryBefore = GC.GetTotalMemory(false);
            var stopwatch = Stopwatch.StartNew();
            TResult result;

            try
            {
                result = func();
            }
            catch (Exception ex)
            {
                if (_logMode.HasFlag(PerformanceLogMode.Narrative))
                    _logger.LogError(ex, "Error executing {Method}", methodName);
                throw;
            }

            stopwatch.Stop();
            long memoryAfter = GC.GetTotalMemory(false);
            long memoryDelta = memoryAfter - memoryBefore;
            
            RecordPerformanceMetrics(methodName, stopwatch.ElapsedMilliseconds, memoryDelta);

            return result;
        }
        
        public async Task<TResult> Track<TResult>(string methodName, Func<Task<TResult>> func)
        {
            long memoryBefore = GC.GetTotalMemory(false);
            var stopwatch = Stopwatch.StartNew();
            TResult result;

            try
            {
                result = await func();
            }
            catch (Exception ex)
            {
                if (_logMode.HasFlag(PerformanceLogMode.Narrative))
                    _logger.LogError(ex, "Error executing {Method}", methodName);
                throw;
            }

            stopwatch.Stop();
            long memoryAfter = GC.GetTotalMemory(false);
            long memoryDelta = memoryAfter - memoryBefore;
            
            RecordPerformanceMetrics(methodName, stopwatch.ElapsedMilliseconds, memoryDelta);

            return result;
        }
        
        private void RecordPerformanceMetrics(string methodName, long elapsedMilliseconds, long memoryDelta)
        {
            bool isSlow = elapsedMilliseconds > _slowThresholdMs;
            bool isMemoryIntensive = memoryDelta > _memoryThresholdBytes;
            
            if (_logMode.HasFlag(PerformanceLogMode.Narrative))
            {
                _logger.LogInformation("Method {Method} took {Elapsed} ms and used {MemoryDelta} bytes",
                    methodName, elapsedMilliseconds, memoryDelta);

                if (isSlow)
                    _logger.LogWarning("⚠️ SLOW: {Method} took {Elapsed} ms", methodName, elapsedMilliseconds);

                if (isMemoryIntensive)
                    _logger.LogWarning("⚠️ HIGH MEMORY: {Method} allocated {MemoryDelta} bytes", methodName, memoryDelta);
            }

            if (_logMode.HasFlag(PerformanceLogMode.ExecutionTime))
            {
                ExecutionTimeHistogram.Record(
                    elapsedMilliseconds,
                    KeyValuePair.Create("method", (object?)methodName));
            }

            if (_logMode.HasFlag(PerformanceLogMode.Memory))
            {
                MemoryUsageHistogram.Record(
                    memoryDelta,
                    KeyValuePair.Create("method", (object?)methodName));
            }
            
            // Store performance log in ObjectDb if provided
            if (_objectDb != null)
            {
                var performanceLog = new PerformanceLog
                {
                    Timestamp = DateTime.UtcNow,
                    Method = methodName,
                    ExecutionTimeMs = elapsedMilliseconds,
                    MemoryDeltaBytes = memoryDelta,
                    IsSlow = isSlow,
                    IsMemoryIntensive = isMemoryIntensive
                };
                
                _objectDb.PerformanceLogs.Add(performanceLog);
            }
        }
    }
}
