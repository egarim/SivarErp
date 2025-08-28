using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sivar.Erp.Modules.Inventory.Core.Interfaces;

namespace Sivar.Erp.EfCore.Entities;

/// <summary>
/// Entity Framework entity for Items
/// </summary>
[Table("Items")]
public class Item : BaseEntity, IItem
{
    /// <summary>
    /// Unique code for the item
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Type of item (Product, Service, etc.)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Description of the item
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Base price of the item
    /// </summary>
    [Column(TypeName = "decimal(18,4)")]
    public decimal BasePrice { get; set; }
}
