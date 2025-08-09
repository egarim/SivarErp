using System.ComponentModel;

namespace Sivar.Erp.Core.Modules.Taxes.Models
{
    /// <summary>
    /// Represents a tax
    /// </summary>
    [Description("Represents a tax")]
    public interface ITax
    {
        /// <summary>
        /// Gets or sets the tax code
        /// </summary>
        string Code { get; set; }
        
        /// <summary>
        /// Gets or sets the tax name
        /// </summary>
        string Name { get; set; }
        
        /// <summary>
        /// Gets or sets the tax description
        /// </summary>
        string? Description { get; set; }
        
        /// <summary>
        /// Gets or sets the tax rate
        /// </summary>
        decimal Rate { get; set; }
        
        /// <summary>
        /// Gets or sets the tax type
        /// </summary>
        TaxType Type { get; set; }
        
        /// <summary>
        /// Gets or sets the tax application level
        /// </summary>
        TaxApplicationLevel ApplicationLevel { get; set; }
        
        /// <summary>
        /// Gets or sets whether the tax is active
        /// </summary>
        bool IsActive { get; set; }
    }
    
    /// <summary>
    /// Types of taxes
    /// </summary>
    public enum TaxType
    {
        /// <summary>
        /// Value-added tax
        /// </summary>
        VAT,
        
        /// <summary>
        /// Sales tax
        /// </summary>
        SalesTax,
        
        /// <summary>
        /// Withholding tax
        /// </summary>
        WithholdingTax,
        
        /// <summary>
        /// Excise tax
        /// </summary>
        ExciseTax,
        
        /// <summary>
        /// Other tax
        /// </summary>
        Other
    }
    
    /// <summary>
    /// Levels at which taxes can be applied
    /// </summary>
    public enum TaxApplicationLevel
    {
        /// <summary>
        /// Applied at the line level
        /// </summary>
        Line,
        
        /// <summary>
        /// Applied at the document level
        /// </summary>
        Document
    }
}