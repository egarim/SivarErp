using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sivar.Erp.Services.Accounting.FiscalPeriods;
using Sivar.Erp;

namespace Sivar.Erp.EfCore.Entities.Accounting
{
    [Table("FiscalPeriods")]
    public class FiscalPeriod : IFiscalPeriod
    {
        [Key]
        public Guid Oid { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        public DateTime InsertedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(100)]
        public string InsertedBy { get; set; } = string.Empty;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(100)]
        public string UpdatedBy { get; set; } = string.Empty;

        public DateOnly StartDate { get; set; }

        public DateOnly EndDate { get; set; }

        public FiscalPeriodStatus Status { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
    }
}
