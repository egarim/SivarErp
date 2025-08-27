using Sivar.Erp.Core.Domain.Enums;

namespace Sivar.Erp.Core.Domain.Entities.Inventory;

/// <summary>
/// Represents a product/item in the inventory system
/// </summary>
public class Product : BaseEntity, ITenantEntity
{
    public Guid CompanyId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Barcode { get; set; }
    public ProductType Type { get; set; } = ProductType.Finished;
    public string? Category { get; set; }
    public string UnitOfMeasure { get; set; } = "EA"; // Each, Kg, L, etc.
    public decimal StandardCost { get; set; }
    public decimal? MinimumStock { get; set; }
    public decimal? MaximumStock { get; set; }
    public decimal? ReorderPoint { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsStockable { get; set; } = true;
    public bool IsPurchasable { get; set; } = true;
    public bool IsSaleable { get; set; } = true;

    // Navigation properties
    public virtual ICollection<InventoryTransaction> InventoryTransactions { get; set; } = new List<InventoryTransaction>();
    public virtual ICollection<StockLevel> StockLevels { get; set; } = new List<StockLevel>();
}

/// <summary>
/// Represents a warehouse or storage location
/// </summary>
public class Warehouse : BaseEntity, ITenantEntity
{
    public Guid CompanyId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; } = false;

    // Navigation properties
    public virtual ICollection<InventoryTransaction> InventoryTransactions { get; set; } = new List<InventoryTransaction>();
    public virtual ICollection<StockLevel> StockLevels { get; set; } = new List<StockLevel>();
}

/// <summary>
/// Represents inventory transactions (movements)
/// </summary>
public class InventoryTransaction : BaseEntity, ITenantEntity
{
    public Guid CompanyId { get; set; }
    public string TransactionNumber { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; } = DateTime.Today;
    public InventoryTransactionType Type { get; set; }
    public Guid ProductId { get; set; }
    public Guid WarehouseId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }
    public string? Reference { get; set; } // Reference to source document
    public string? Notes { get; set; }
    public Guid? RelatedTransactionId { get; set; } // For reversals or adjustments

    // Navigation properties
    public virtual Product Product { get; set; } = null!;
    public virtual Warehouse Warehouse { get; set; } = null!;
    public virtual InventoryTransaction? RelatedTransaction { get; set; }
}

/// <summary>
/// Represents current stock levels by product and warehouse
/// </summary>
public class StockLevel : BaseEntity, ITenantEntity
{
    public Guid CompanyId { get; set; }
    public Guid ProductId { get; set; }
    public Guid WarehouseId { get; set; }
    public decimal QuantityOnHand { get; set; }
    public decimal QuantityReserved { get; set; } = 0;
    public decimal QuantityAvailable => QuantityOnHand - QuantityReserved;
    public decimal AverageCost { get; set; }
    public decimal TotalValue => QuantityOnHand * AverageCost;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Product Product { get; set; } = null!;
    public virtual Warehouse Warehouse { get; set; } = null!;
}
