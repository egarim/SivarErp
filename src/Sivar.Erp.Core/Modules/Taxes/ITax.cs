using System.ComponentModel;
using Sivar.Erp.Core.Core;

namespace Sivar.Erp.Core.Modules.Taxes
{
    /// <summary>
    /// Interface for tax entities
    /// </summary>
    [Description("Interface for tax entities")]
    public interface ITax : IEntity
    {
        /// <summary>
        /// Gets or sets the tax code
        /// </summary>
        [Description("Tax code")]
        string TaxCode { get; set; }

        /// <summary>
        /// Gets or sets the tax name
        /// </summary>
        [Description("Tax name")]
        string TaxName { get; set; }

        /// <summary>
        /// Gets or sets the tax description
        /// </summary>
        [Description("Tax description")]
        string? Description { get; set; }

        /// <summary>
        /// Gets or sets the tax rate
        /// </summary>
        [Description("Tax rate")]
        decimal TaxRate { get; set; }

        /// <summary>
        /// Gets or sets the tax type
        /// </summary>
        [Description("Tax type")]
        string TaxType { get; set; }

        /// <summary>
        /// Gets or sets whether the tax is active
        /// </summary>
        [Description("Whether the tax is active")]
        bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the effective date
        /// </summary>
        [Description("Effective date")]
        DateTime EffectiveDate { get; set; }

        /// <summary>
        /// Gets or sets the expiration date
        /// </summary>
        [Description("Expiration date")]
        DateTime? ExpirationDate { get; set; }

        /// <summary>
        /// Gets or sets the calculation method
        /// </summary>
        [Description("Calculation method")]
        string CalculationMethod { get; set; }

        /// <summary>
        /// Gets or sets whether this is a default tax
        /// </summary>
        [Description("Whether this is a default tax")]
        bool IsDefault { get; set; }

        /// <summary>
        /// Gets or sets the tax configuration
        /// </summary>
        [Description("Tax configuration")]
        Dictionary<string, object> Configuration { get; set; }
    }
}
