using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sivar.Erp.Services.Accounting.Transactions;
using Sivar.Erp;

namespace Sivar.Erp.EfCore.Entities.Accounting
{
    [Table("LedgerEntries")]
    public class LedgerEntry : ILedgerEntry
    {
        [Key]
        public Guid Oid { get; set; } = Guid.NewGuid();

        public DateTime InsertedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(100)]
        public string InsertedBy { get; set; } = string.Empty;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(100)]
        public string UpdatedBy { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string TransactionNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string LedgerEntryNumber { get; set; } = string.Empty;

        public EntryType EntryType { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(200)]
        public string AccountName { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string OfficialCode { get; set; } = string.Empty;

        // Foreign Key
        public Guid TransactionId { get; set; }

        // Navigation property
        [ForeignKey(nameof(TransactionId))]
        public virtual Transaction Transaction { get; set; } = null!;
    }
}
