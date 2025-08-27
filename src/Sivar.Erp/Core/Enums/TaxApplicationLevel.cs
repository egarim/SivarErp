using System;

namespace Sivar.Erp.Core.Enums
{
    /// <summary>
    /// Defines the level at which tax should be applied
    /// </summary>
    public enum TaxApplicationLevel
    {
        /// <summary>
        /// Tax is applied at the individual line level
        /// </summary>
        Line = 0,
        
        /// <summary>
        /// Tax is applied at the document level (after line totals)
        /// </summary>
        Document = 1
    }
}