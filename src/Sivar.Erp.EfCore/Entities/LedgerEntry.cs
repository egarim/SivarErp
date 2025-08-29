using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sivar.Erp.Modules.Accounting.Transactions;


namespace Sivar.Erp.EfCore.Entities;

/// <summary>
/// Entity Framework entity for Ledger Entries
/// </summary>
[Table("LedgerEntries")]
public class LedgerEntry : BaseEntity, ILedgerEntry
{
    /// <summary>
    /// Reference to the parent transaction (business key for compatibility)
    /// </summary>
    [Required]
    [MaxLength(100)]
    public virtual string TransactionNumber { get; set; } = string.Empty;

    /// <summary>
    /// Foreign key reference to the parent transaction Oid
    /// </summary>
    public virtual Guid? TransactionId { get; set; }

    /// <summary>
    /// Unique ledger entry number
    /// </summary>
    [Required]
    [MaxLength(100)]
    public virtual string LedgerEntryNumber { get; set; } = string.Empty;

    /// <summary>
    /// Type of entry (debit or credit)
    /// </summary>
    public virtual EntryType EntryType { get; set; }

    /// <summary>
    /// Amount of the entry
    /// </summary>
    [Column(TypeName = "decimal(18,4)")]
    public virtual decimal Amount { get; set; }

    /// <summary>
    /// Name of the account
    /// </summary>
    [Required]
    [MaxLength(200)]
    public virtual string AccountName { get; set; } = string.Empty;

    /// <summary>
    /// Official code/identifier for the account
    /// </summary>
    [Required]
    [MaxLength(50)]
    public virtual string OfficialCode { get; set; } = string.Empty;

    /// <summary>
    /// Optional description for the entry
    /// </summary>
    [MaxLength(500)]
    public virtual string? Description { get; set; }

    /// <summary>
    /// Navigation property to the parent transaction
    /// </summary>
    [ForeignKey(nameof(TransactionId))]
    public virtual Transaction? Transaction { get; set; }
}
