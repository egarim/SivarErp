using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sivar.Erp.ErpSystem.ActivityStream;

namespace Sivar.Erp.EfCore.Entities.System
{
    [Table("ActivityRecords")]
    public class ActivityRecord
    {
        [Key]
        public Guid Oid { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(100)]
        public string Actor { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Verb { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Target { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? TimeZoneId { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [MaxLength(1000)]
        public string? SerializedActivity { get; set; }
    }
}
