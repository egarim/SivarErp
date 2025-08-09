using Sivar.Erp.Core.Core;

namespace Sivar.Erp.Core.Modules.Taxes.Models
{
    /// <summary>
    /// Implementation of the ITax interface
    /// </summary>
    public class TaxDto : Entity, ITax
    {
        /// <summary>
        /// Gets or sets the tax code
        /// </summary>
        public string Code { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the tax name
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the tax description
        /// </summary>
        public string? Description { get; set; }
        
        /// <summary>
        /// Gets or sets the tax rate
        /// </summary>
        public decimal Rate { get; set; }
        
        /// <summary>
        /// Gets or sets the tax type
        /// </summary>
        public TaxType Type { get; set; }
        
        /// <summary>
        /// Gets or sets the tax application level
        /// </summary>
        public TaxApplicationLevel ApplicationLevel { get; set; }
        
        /// <summary>
        /// Gets or sets whether the tax is active
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}