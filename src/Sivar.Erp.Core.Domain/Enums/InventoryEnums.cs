namespace Sivar.Erp.Core.Domain.Enums;

/// <summary>
/// Types of products in the inventory system
/// </summary>
public enum ProductType
{
    /// <summary>
    /// Raw materials used in production
    /// </summary>
    RawMaterial = 1,

    /// <summary>
    /// Work in progress items
    /// </summary>
    WorkInProgress = 2,

    /// <summary>
    /// Finished goods ready for sale
    /// </summary>
    Finished = 3,

    /// <summary>
    /// Service items (non-stockable)
    /// </summary>
    Service = 4,

    /// <summary>
    /// Consumable supplies
    /// </summary>
    Supply = 5
}

/// <summary>
/// Types of inventory transactions
/// </summary>
public enum InventoryTransactionType
{
    /// <summary>
    /// Initial stock entry
    /// </summary>
    Opening = 1,

    /// <summary>
    /// Receipt from purchase
    /// </summary>
    Purchase = 2,

    /// <summary>
    /// Issue for sale
    /// </summary>
    Sale = 3,

    /// <summary>
    /// Issue for production
    /// </summary>
    Production = 4,

    /// <summary>
    /// Transfer between warehouses
    /// </summary>
    Transfer = 5,

    /// <summary>
    /// Positive adjustment
    /// </summary>
    AdjustmentIn = 6,

    /// <summary>
    /// Negative adjustment
    /// </summary>
    AdjustmentOut = 7,

    /// <summary>
    /// Physical count adjustment
    /// </summary>
    PhysicalCount = 8,

    /// <summary>
    /// Reversal of previous transaction
    /// </summary>
    Reversal = 9
}

/// <summary>
/// Inventory valuation methods
/// </summary>
public enum InventoryValuationMethod
{
    /// <summary>
    /// First In, First Out
    /// </summary>
    FIFO = 1,

    /// <summary>
    /// Last In, First Out
    /// </summary>
    LIFO = 2,

    /// <summary>
    /// Weighted Average Cost
    /// </summary>
    WeightedAverage = 3,

    /// <summary>
    /// Standard Cost
    /// </summary>
    Standard = 4,

    /// <summary>
    /// Specific Identification
    /// </summary>
    SpecificIdentification = 5
}
