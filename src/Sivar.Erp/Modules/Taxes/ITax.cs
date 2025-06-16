using System;

namespace Sivar.Erp.Services.Taxes
{
    public interface ITax
    {
        /// <summary>
        /// Fixed amount to apply when TaxType is FixedAmount or AmountPerUnit
        /// </summary>
        decimal Amount { get; set; }
        /// <summary>
        /// Level at which the tax should be applied (line or document)
        /// </summary>
        TaxApplicationLevel ApplicationLevel { get; set; }
        /// <summary>
        /// Short code for the tax (e.g., VAT, GST)
        /// </summary>
        string Code { get; set; }
        /// <summary>
        /// Whether this tax is currently active
        /// </summary>
        bool IsEnabled { get; set; }
        /// <summary>
        /// Whether the tax is already included in the line price
        /// </summary>
        bool IsIncludedInPrice { get; set; }
        /// <summary>
        /// Display name of the tax
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// Unique identifier for the tax
        /// </summary>
        Guid Oid { get; set; }
        /// <summary>
        /// Percentage to apply when TaxType is Percentage
        /// </summary>
        decimal Percentage { get; set; }
        /// <summary>
        /// Type of tax calculation (by percentage, fixed amount, or per quantity)
        /// </summary>
        TaxType TaxType { get; set; }
    }
}