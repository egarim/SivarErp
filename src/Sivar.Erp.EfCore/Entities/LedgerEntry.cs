using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sivar.Erp.Core.Enums;
using Sivar.Erp.Modules.Accounting.Domain.Transactions;

namespace Sivar.Erp.EfCore.Entities;

/// <summary>
/// Entity Framework entity for Ledger Entries
/// </summary>
[Table("LedgerEntries")]
public class LedgerEntry : BaseEntity, ILedgerEntry
{
    /// <summary>
    /// Reference to the parent transaction
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string TransactionNumber { get; set; } = string.Empty;

    /// <summary>
    /// Unique ledger entry number
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string LedgerEntryNumber { get; set; } = string.Empty;

    /// <summary>
    /// Type of entry (debit or credit)
    /// </summary>
    public EntryType EntryType { get; set; }

    /// <summary>
    /// Amount of the entry
    /// </summary>
    [Column(TypeName = "decimal(18,4)")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Name of the account
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string AccountName { get; set; } = string.Empty;

    /// <summary>
    /// Official code/identifier for the account
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string OfficialCode { get; set; } = string.Empty;

    /// <summary>
    /// Optional description for the entry
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Navigation property to the parent transaction
    /// </summary>
    [ForeignKey(nameof(TransactionNumber))]
    public virtual Transaction? Transaction { get; set; }
}
