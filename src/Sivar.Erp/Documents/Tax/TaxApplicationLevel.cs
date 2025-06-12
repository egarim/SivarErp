namespace Sivar.Erp.Documents.Tax
{
    /// <summary>
    /// Defines where the tax should be applied
    /// </summary>
    public enum TaxApplicationLevel
    {
        /// <summary>
        /// Tax applies individually to each line
        /// </summary>
        Line,
        
        /// <summary>
        /// Tax applies to the document total
        /// </summary>
        Document
    }
}