using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using System.Runtime;

namespace Sivar.Erp.Core.Infrastructure.Performance
{
    /// <summary>
    /// Utility for optimizing memory usage and performance in the ERP system
    /// </summary>
    [Description("Utility for optimizing memory usage and performance")]
    public static class MemoryOptimizer
    {
        /// <summary>
        /// Configures services for low memory usage scenarios
        /// </summary>
        /// <param name="services">Service collection to configure</param>
        /// <returns>Configured service collection</returns>
        [Description("Configures services for low memory usage")]
        public static IServiceCollection ConfigureForLowMemory(this IServiceCollection services)
        {
            // Configure object pooling for frequently used objects
            services.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
            
            // Add memory-efficient string interning
            services.AddSingleton<IStringInterningService, StringInterningService>();
            
            // Configure garbage collection for low memory
            ConfigureGarbageCollection();
            
            return services;
        }

        /// <summary>
        /// Configures services for high performance scenarios
        /// </summary>
        /// <param name="services">Service collection to configure</param>
        /// <returns>Configured service collection</returns>
        [Description("Configures services for high performance")]
        public static IServiceCollection ConfigureForHighPerformance(this IServiceCollection services)
        {
            // Configure aggressive object pooling
            services.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
            
            // Add caching strategies
            services.AddMemoryCache(options =>
            {
                options.SizeLimit = 1000;
                options.CompactionPercentage = 0.25;
            });
            
            return services;
        }

        /// <summary>
        /// Forces garbage collection and compaction
        /// </summary>
        [Description("Forces garbage collection and compaction")]
        public static void OptimizeMemory()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            // Note: GC.Compact() is not available in .NET Core/5+
        }

        /// <summary>
        /// Gets current memory usage statistics
        /// </summary>
        /// <returns>Memory usage information</returns>
        [Description("Gets current memory usage statistics")]
        public static MemoryUsageInfo GetMemoryUsage()
        {
            return new MemoryUsageInfo
            {
                TotalMemory = GC.GetTotalMemory(false),
                TotalMemoryAfterGC = GC.GetTotalMemory(true),
                Generation0Collections = GC.CollectionCount(0),
                Generation1Collections = GC.CollectionCount(1),
                Generation2Collections = GC.CollectionCount(2),
                TotalAllocatedBytes = GC.GetTotalAllocatedBytes(),
                MaxGeneration = GC.MaxGeneration
            };
        }

        /// <summary>
        /// Configures garbage collection settings for optimal performance
        /// </summary>
        private static void ConfigureGarbageCollection()
        {
            // Set GC latency mode for low latency (if available)
            try
            {
                GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
            }
            catch
            {
                // Fallback if not supported in current runtime
            }
        }
    }

    /// <summary>
    /// Represents memory usage information
    /// </summary>
    [Description("Represents memory usage information")]
    public class MemoryUsageInfo
    {
        /// <summary>
        /// Total memory currently allocated
        /// </summary>
        [Description("Total memory currently allocated")]
        public long TotalMemory { get; set; }

        /// <summary>
        /// Total memory after forcing garbage collection
        /// </summary>
        [Description("Total memory after forcing garbage collection")]
        public long TotalMemoryAfterGC { get; set; }

        /// <summary>
        /// Memory that could be freed by garbage collection
        /// </summary>
        [Description("Memory that could be freed by garbage collection")]
        public long FreableMemory => TotalMemory - TotalMemoryAfterGC;

        /// <summary>
        /// Number of generation 0 garbage collections
        /// </summary>
        [Description("Number of generation 0 garbage collections")]
        public int Generation0Collections { get; set; }

        /// <summary>
        /// Number of generation 1 garbage collections
        /// </summary>
        [Description("Number of generation 1 garbage collections")]
        public int Generation1Collections { get; set; }

        /// <summary>
        /// Number of generation 2 garbage collections
        /// </summary>
        [Description("Number of generation 2 garbage collections")]
        public int Generation2Collections { get; set; }

        /// <summary>
        /// Total bytes allocated since application start
        /// </summary>
        [Description("Total bytes allocated since application start")]
        public long TotalAllocatedBytes { get; set; }

        /// <summary>
        /// Maximum garbage collection generation
        /// </summary>
        [Description("Maximum garbage collection generation")]
        public int MaxGeneration { get; set; }

        /// <summary>
        /// Memory usage in megabytes
        /// </summary>
        [Description("Memory usage in megabytes")]
        public double MemoryUsageMB => TotalMemory / 1024.0 / 1024.0;

        /// <summary>
        /// Freeable memory in megabytes
        /// </summary>
        [Description("Freeable memory in megabytes")]
        public double FreableMemoryMB => FreableMemory / 1024.0 / 1024.0;
    }

    /// <summary>
    /// Service for interning strings to reduce memory usage
    /// </summary>
    [Description("Service for interning strings to reduce memory usage")]
    public interface IStringInterningService
    {
        /// <summary>
        /// Interns a string to reduce memory usage
        /// </summary>
        /// <param name="value">String to intern</param>
        /// <returns>Interned string</returns>
        [Description("Interns a string to reduce memory usage")]
        string Intern(string value);

        /// <summary>
        /// Gets statistics about interned strings
        /// </summary>
        /// <returns>String interning statistics</returns>
        [Description("Gets statistics about interned strings")]
        StringInterningStats GetStats();
    }

    /// <summary>
    /// Implementation of string interning service
    /// </summary>
    [Description("Implementation of string interning service")]
    public class StringInterningService : IStringInterningService
    {
        private readonly Dictionary<string, string> _internedStrings = new();
        private int _internCount;
        private int _hitCount;

        /// <summary>
        /// Interns a string to reduce memory usage
        /// </summary>
        [Description("Interns a string to reduce memory usage")]
        public string Intern(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            if (_internedStrings.TryGetValue(value, out var existing))
            {
                _hitCount++;
                return existing;
            }

            var interned = string.Intern(value);
            _internedStrings[value] = interned;
            _internCount++;
            return interned;
        }

        /// <summary>
        /// Gets statistics about interned strings
        /// </summary>
        [Description("Gets statistics about interned strings")]
        public StringInterningStats GetStats()
        {
            return new StringInterningStats
            {
                InternedCount = _internCount,
                HitCount = _hitCount,
                UniqueStrings = _internedStrings.Count,
                HitRate = _internCount > 0 ? (double)_hitCount / (_internCount + _hitCount) * 100 : 0
            };
        }
    }

    /// <summary>
    /// Statistics about string interning
    /// </summary>
    [Description("Statistics about string interning")]
    public class StringInterningStats
    {
        /// <summary>
        /// Number of strings interned
        /// </summary>
        [Description("Number of strings interned")]
        public int InternedCount { get; set; }

        /// <summary>
        /// Number of cache hits
        /// </summary>
        [Description("Number of cache hits")]
        public int HitCount { get; set; }

        /// <summary>
        /// Number of unique strings
        /// </summary>
        [Description("Number of unique strings")]
        public int UniqueStrings { get; set; }

        /// <summary>
        /// Cache hit rate percentage
        /// </summary>
        [Description("Cache hit rate percentage")]
        public double HitRate { get; set; }
    }
}
