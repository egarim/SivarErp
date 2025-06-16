namespace Sivar.Erp.Services.Taxes
{
    /// <summary>
    /// Defines how a tax amount is calculated
    /// </summary>
    public enum TaxType
    {
        /// <summary>
        /// Tax calculated as a percentage of line amount
        /// </summary>
        Percentage,
        
        /// <summary>
        /// Tax as a fixed amount per line
        /// </summary>
        FixedAmount,
        
        /// <summary>
        /// Tax as an amount per quantity unit
        /// </summary>
        AmountPerUnit
    }
}