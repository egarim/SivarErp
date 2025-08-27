using System;

namespace Sivar.Erp.Core.Contracts
{
    /// <summary>
    /// Core contract interface for items
    /// </summary>
    public interface IItem
    {
        /// <summary>
        /// Unique code for the item
        /// </summary>
        string Code { get; set; }
        
        /// <summary>
        /// Type of item (Product, Service, etc.)
        /// </summary>
        string Type { get; set; }
        
        /// <summary>
        /// Description of the item
        /// </summary>
        string Description { get; set; }
        
        /// <summary>
        /// Base price of the item
        /// </summary>
        decimal BasePrice { get; set; }
        
        /// <summary>
        /// Category of the item
        /// </summary>
        string? Category { get; set; }
        
        /// <summary>
        /// Whether the item is active
        /// </summary>
        bool IsActive { get; set; }
        
        /// <summary>
        /// Date when the item was created
        /// </summary>
        DateTime CreatedDate { get; set; }
        
        /// <summary>
        /// User who created the item
        /// </summary>
        string CreatedBy { get; set; }
    }
}
