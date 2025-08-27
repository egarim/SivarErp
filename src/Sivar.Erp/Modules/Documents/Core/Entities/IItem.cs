using System;

namespace Sivar.Erp.Modules.Documents.Core.Entities
{
    /// <summary>
    /// Interface for basic item entities
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
    }
}
