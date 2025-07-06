using System;

namespace Sivar.Erp.Core.Enums
{
    /// <summary>
    /// Defines the different types of tax calculations
    /// </summary>
    public enum TaxType
    {
        /// <summary>
        /// Tax is calculated as a percentage of the base amount
        /// </summary>
        Percentage = 0,
        
        /// <summary>
        /// Tax is a fixed amount regardless of the base amount
        /// </summary>
        FixedAmount = 1,
        
        /// <summary>
        /// Tax is calculated as an amount per unit of quantity
        /// </summary>
        AmountPerUnit = 2
    }
}