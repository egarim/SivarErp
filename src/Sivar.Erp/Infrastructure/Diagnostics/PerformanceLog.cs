using System;

namespace Sivar.Erp.Infrastructure.Diagnostics
{
    public class PerformanceLog
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Method { get; set; } = string.Empty;
        public long ExecutionTimeMs { get; set; }
        public long MemoryDeltaBytes { get; set; }
        public bool IsSlow { get; set; }
        public bool IsMemoryIntensive { get; set; }

        // Platform-agnostic context properties
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string? InstanceId { get; set; }
        public string? SessionId { get; set; }
        public string? Context { get; set; } // Additional context info
    }
}