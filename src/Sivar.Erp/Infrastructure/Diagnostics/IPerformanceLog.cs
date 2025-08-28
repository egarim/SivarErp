using System;

namespace Sivar.Erp.Infrastructure.Diagnostics
{
    public interface IPerformanceLog
    {
        string? Context { get; set; }
        long ExecutionTimeMs { get; set; }
        int Id { get; set; }
        string? InstanceId { get; set; }
        bool IsMemoryIntensive { get; set; }
        bool IsSlow { get; set; }
        long MemoryDeltaBytes { get; set; }
        string Method { get; set; }
        string? SessionId { get; set; }
        DateTime Timestamp { get; set; }
        string? UserId { get; set; }
        string? UserName { get; set; }
    }
}
