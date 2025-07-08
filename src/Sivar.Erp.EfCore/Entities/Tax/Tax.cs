using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sivar.Erp.Services.Taxes;

namespace Sivar.Erp.EfCore.Entities.Tax
{
    [Table("Taxes")]
    public class Tax : ITax
    {
        [Key]
        public Guid Oid { get; set; } = Guid.NewGuid();

        [Column(TypeName = "decimal(18,4)")]
        public decimal Amount { get; set; }

        public TaxApplicationLevel ApplicationLevel { get; set; }

        [Required]
        [MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        public bool IsEnabled { get; set; } = true;

        public bool IsIncludedInPrice { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Column(TypeName = "decimal(5,4)")]
        public decimal Percentage { get; set; }

        public TaxType TaxType { get; set; }
    }
}
