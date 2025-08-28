using DevExpress.Persistent.Base;
using Sivar.Erp.Modules.Accounting.Domain.Transactions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sivar.Erp.EfCore.Entities;

[DefaultClassOptions()]
[NavigationItem("Accounting")]
/// <summary>
/// Entity Framework entity for Financial Transactions
/// </summary>
[Table("Transactions")]
public class Transaction : BaseEntity, ITransaction
{
    /// <summary>
    /// Reference to the parent document
    /// </summary>
    [MaxLength(100)]
    public virtual string? DocumentNumber { get; set; }

    /// <summary>
    /// Unique transaction number
    /// </summary>
    [Required]
    [MaxLength(100)]
    public virtual string TransactionNumber { get; set; } = string.Empty;

    /// <summary>
    /// Date of the transaction (may differ from document date)
    /// </summary>
    public virtual DateOnly TransactionDate { get; set; }

    /// <summary>
    /// Description of the transaction
    /// </summary>
    [MaxLength(500)]
    public virtual string? Description { get; set; }

    /// <summary>
    /// Whether the transaction has been posted
    /// </summary>
    public virtual bool IsPosted { get; set; } = false;

    /// <summary>
    /// Navigation property to ledger entries
    /// </summary>
    public virtual ICollection<LedgerEntry> LedgerEntries { get; set; } = new List<LedgerEntry>();

    // Interface implementation for IEnumerable<ILedgerEntry>
    IEnumerable<ILedgerEntry> ITransaction.LedgerEntries 
    { 
        get => LedgerEntries; 
        set => LedgerEntries = value.Cast<LedgerEntry>().ToList(); 
    }

    /// <summary>
    /// Posts the transaction
    /// </summary>
    public void Post()
    {
        IsPosted = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Unposts the transaction
    /// </summary>
    public void UnPost()
    {
        IsPosted = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
