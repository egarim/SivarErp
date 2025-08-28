using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sivar.Erp.Modules.Accounting.Transactions;

namespace Sivar.Erp.EfCore.Entities;

/// <summary>
/// Entity Framework entity for Transaction Batches
/// </summary>
[Table("TransactionBatches")]
public class TransactionBatch : BaseEntity, ITransactionBatch
{
    /// <summary>
    /// Batch reference code
    /// </summary>
    [Required]
    [MaxLength(100)]
    public virtual string ReferenceCode { get; set; } = string.Empty;

    /// <summary>
    /// Transaction number for the batch
    /// </summary>
    [Required]
    [MaxLength(100)]
    public virtual string TransactionNumber { get; set; } = string.Empty;

    /// <summary>
    /// Date when the batch was created
    /// </summary>
    public virtual DateOnly BatchDate { get; set; }

    /// <summary>
    /// Description of the transaction batch
    /// </summary>
    [MaxLength(500)]
    public virtual string? Description { get; set; }

    /// <summary>
    /// Status of the batch
    /// </summary>
    public virtual BatchStatus Status { get; set; } = BatchStatus.Draft;

    /// <summary>
    /// Whether the batch has been posted
    /// </summary>
    public virtual bool IsPosted { get; set; } = false;

    /// <summary>
    /// Navigation property to transactions in this batch
    /// </summary>
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    /// <summary>
    /// Posts the batch
    /// </summary>
    public void Post()
    {
        IsPosted = true;
        Status = BatchStatus.Posted;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Unposts the batch
    /// </summary>
    public void UnPost()
    {
        IsPosted = false;
        Status = BatchStatus.Draft;
        UpdatedAt = DateTime.UtcNow;
    }
}

