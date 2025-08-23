using System.ComponentModel;

namespace Sivar.Erp.Core.Modules.Inventory.Models
{
    /// <summary>
    /// Inventory valuation methods
    /// </summary>
    [Description("Inventory valuation methods")]
    public enum InventoryValuationMethod
    {
        /// <summary>
        /// First In First Out
        /// </summary>
        [Description("First In First Out")]
        FIFO = 0,

        /// <summary>
        /// Last In First Out
        /// </summary>
        [Description("Last In First Out")]
        LIFO = 1,

        /// <summary>
        /// Weighted Average Cost
        /// </summary>
        [Description("Weighted Average Cost")]
        WeightedAverage = 2,

        /// <summary>
        /// Standard Cost
        /// </summary>
        [Description("Standard Cost")]
        StandardCost = 3,

        /// <summary>
        /// Specific Identification
        /// </summary>
        [Description("Specific Identification")]
        SpecificIdentification = 4
    }
}
