using System.ComponentModel;

namespace Sivar.Erp.Core.Modules.Documents
{
    /// <summary>
    /// Represents an inventory item
    /// </summary>
    [Description("Represents an inventory item")]
    public interface IItem
    {
        /// <summary>
        /// Gets or sets the item code
        /// </summary>
        string Code { get; set; }
        
        /// <summary>
        /// Gets or sets the item description
        /// </summary>
        string Description { get; set; }
        
        /// <summary>
        /// Gets or sets the item type
        /// </summary>
        ItemType ItemType { get; set; }
        
        /// <summary>
        /// Gets or sets the unit of measure
        /// </summary>
        string UnitOfMeasure { get; set; }
        
        /// <summary>
        /// Gets or sets whether the item is taxable
        /// </summary>
        bool IsTaxable { get; set; }
    }
    
    /// <summary>
    /// Types of items
    /// </summary>
    public enum ItemType
    {
        /// <summary>
        /// Physical product
        /// </summary>
        Product,
        
        /// <summary>
        /// Service
        /// </summary>
        Service,
        
        /// <summary>
        /// Raw material
        /// </summary>
        RawMaterial,
        
        /// <summary>
        /// Finished good
        /// </summary>
        FinishedGood
    }
}