using DevExpress.Persistent.Base;
using Sivar.Erp.Modules.Documents.Core.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sivar.Erp.EfCore.Entities;

[DefaultClassOptions()]
[NavigationItem("Documents")]
/// <summary>
/// Entity Framework entity for Document Types
/// </summary>
[Table("DocumentTypes")]
public class DocumentType : BaseEntity, IDocumentType
{
    /// <summary>
    /// Unique code for the document type (e.g., "INV", "PO")
    /// </summary>
    [Required]
    [MaxLength(50)]
    public virtual string Code { get; set; } = string.Empty;

    /// <summary>
    /// Display name of the document type
    /// </summary>
    [Required]
    [MaxLength(200)]
    public virtual string Name { get; set; } = string.Empty;

    /// <summary>
    /// Whether this document type is currently active
    /// </summary>
    public virtual bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Category or operation of the document type (e.g., "SalesInvoice", "PurchaseOrder")
    /// </summary>
    [Required]
    [MaxLength(100)]
    public virtual string DocumentOperation { get; set; } = string.Empty;

    // For INotifyPropertyChanged interface implementation (not used in EF context)
    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
}
