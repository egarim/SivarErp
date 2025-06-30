namespace Sivar.Erp.Documents
{
    /// <summary>
    /// Defines methods for inventory valuation
    /// </summary>
    public enum InventoryValuationMethod
    {
        /// <summary>
        /// Weighted Average Cost method
        /// </summary>
        WeightedAverage,
        
        /// <summary>
        /// First In, First Out method
        /// </summary>
        FIFO,
        
        /// <summary>
        /// Last In, First Out method
        /// </summary>
        LIFO,
        
        /// <summary>
        /// Standard Cost method
        /// </summary>
        StandardCost,
        
        /// <summary>
        /// Specific Identification method
        /// </summary>
        SpecificIdentification
    }
}