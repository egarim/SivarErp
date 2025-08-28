using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sivar.Erp.Modules.Documents.Core.Interfaces;

namespace Sivar.Erp.EfCore.Entities;

/// <summary>
/// Entity Framework entity for Document Accounting Profiles
/// </summary>
[Table("DocumentAccountingProfiles")]
public class DocumentAccountingProfile : BaseEntity, IDocumentAccountingProfile
{
    /// <summary>
    /// Document operation this profile applies to (e.g. "SalesInvoice", "PurchaseInvoice")
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string DocumentOperation { get; set; } = string.Empty;

    /// <summary>
    /// Account code to use for sales or revenue
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string SalesAccountCode { get; set; } = string.Empty;

    /// <summary>
    /// Account code to use for accounts receivable
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string AccountsReceivableCode { get; set; } = string.Empty;

    /// <summary>
    /// Account code to use for cost of goods sold
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string CostOfGoodsSoldAccountCode { get; set; } = string.Empty;

    /// <summary>
    /// Account code to use for inventory
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string InventoryAccountCode { get; set; } = string.Empty;

    /// <summary>
    /// Cost ratio used for calculating cost of goods sold (e.g. 0.6 for 60%)
    /// </summary>
    [Column(TypeName = "decimal(18,4)")]
    public decimal CostRatio { get; set; }

    /// <summary>
    /// Created by user
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// Created date
    /// </summary>
    public DateTimeOffset CreatedDate { get; set; } = DateTimeOffset.UtcNow;
}
