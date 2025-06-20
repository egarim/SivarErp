using System;

namespace Sivar.Erp.ErpSystem.Diagnostics
{
    public class PerformanceLog
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Method { get; set; }
        public long ExecutionTimeMs { get; set; }
        public long MemoryDeltaBytes { get; set; }
        public bool IsSlow { get; set; }
        public bool IsMemoryIntensive { get; set; }
    }
}