using Sivar.Erp.Core.Core;

namespace Sivar.Erp.Core.Modules.Documents.Models
{
    /// <summary>
    /// Implementation of the IItem interface
    /// </summary>
    public class ItemDto : Entity, IItem
    {
        /// <summary>
        /// Gets or sets the item code
        /// </summary>
        public string Code { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the item description
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the item type
        /// </summary>
        public ItemType ItemType { get; set; }
        
        /// <summary>
        /// Gets or sets the unit of measure
        /// </summary>
        public string UnitOfMeasure { get; set; } = "UNIT";
        
        /// <summary>
        /// Gets or sets whether the item is taxable
        /// </summary>
        public bool IsTaxable { get; set; } = true;
    }
}