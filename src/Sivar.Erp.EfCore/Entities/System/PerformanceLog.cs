using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sivar.Erp.ErpSystem.Diagnostics;

namespace Sivar.Erp.EfCore.Entities.System
{
    [Table("PerformanceLogs")]
    public class PerformanceLog
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(200)]
        public string Method { get; set; } = string.Empty;

        public long ExecutionTimeMs { get; set; }

        public long MemoryDeltaBytes { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        public string? UserName { get; set; }

        [MaxLength(100)]
        public string? InstanceId { get; set; }

        [MaxLength(200)]
        public string? Context { get; set; }

        public bool IsSlow { get; set; }

        public bool IsMemoryIntensive { get; set; }
    }
}
