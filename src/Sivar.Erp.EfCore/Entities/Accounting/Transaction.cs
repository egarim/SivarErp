using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sivar.Erp.Services.Accounting.Transactions;
using Sivar.Erp;

namespace Sivar.Erp.EfCore.Entities.Accounting
{
    [Table("Transactions")]
    public class Transaction : ITransaction
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
        public string DocumentNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string TransactionNumber { get; set; } = string.Empty;

        public DateOnly TransactionDate { get; set; }

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        public bool IsPosted { get; set; }

        // Navigation properties
        public virtual ICollection<LedgerEntry> LedgerEntries { get; set; } = new List<LedgerEntry>();

        // Interface implementation
        IEnumerable<ILedgerEntry> ITransaction.LedgerEntries
        {
            get => LedgerEntries.Cast<ILedgerEntry>();
            set => LedgerEntries = value.Cast<LedgerEntry>().ToList();
        }

        public void Post()
        {
            IsPosted = true;
        }

        public void UnPost()
        {
            IsPosted = false;
        }
    }
}
