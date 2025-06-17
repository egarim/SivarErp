using System;
using Sivar.Erp.Documents;

namespace Sivar.Erp.Services.Taxes.TaxRule
{
    /// <summary>
    /// Interface for tax rule entities that determine when and how taxes are applied
    /// </summary>
    public interface ITaxRule
    {
        /// <summary>
        /// Unique identifier for the tax rule
        /// </summary>
        Guid Oid { get; set; }
        
        /// <summary>
        /// Reference to the tax that should be applied
        /// </summary>
        string TaxId { get; set; }
        
        /// <summary>
        /// Document operation this rule applies to (null means any document operation)
        /// </summary>
        DocumentOperation? DocumentOperation { get; set; }
        
        /// <summary>
        /// Business entity group ID this rule applies to (null means any entity)
        /// </summary>
        string BusinessEntityGroupId { get; set; }
        
        /// <summary>
        /// Item group ID this rule applies to (null means any item)
        /// </summary>
        string ItemGroupId { get; set; }
        
        /// <summary>
        /// Whether this tax rule is currently active
        /// </summary>
        bool IsEnabled { get; set; }
        
        /// <summary>
        /// Priority of the rule (lower numbers = higher priority)
        /// </summary>
        int Priority { get; set; }
    }
}