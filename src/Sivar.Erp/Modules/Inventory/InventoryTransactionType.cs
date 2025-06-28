namespace Sivar.Erp.Modules.Inventory
{
    /// <summary>
    /// Defines the types of inventory transactions that can occur
    /// </summary>
    public enum InventoryTransactionType
    {
        /// <summary>
        /// Goods received from purchase order
        /// </summary>
        PurchaseReceipt,

        /// <summary>
        /// Sales order shipment
        /// </summary>
        SalesIssue,

        /// <summary>
        /// Adjustment to correct inventory
        /// </summary>
        Adjustment,

        /// <summary>
        /// Transfer between warehouses
        /// </summary>
        Transfer,

        /// <summary>
        /// Physical inventory count
        /// </summary>
        PhysicalCount,

        /// <summary>
        /// Return from customer
        /// </summary>
        CustomerReturn,

        /// <summary>
        /// Return to supplier
        /// </summary>
        SupplierReturn,

        /// <summary>
        /// Production input (raw materials)
        /// </summary>
        ProductionInput,

        /// <summary>
        /// Production output (finished goods)
        /// </summary>
        ProductionOutput,

        /// <summary>
        /// Damaged or obsolete goods
        /// </summary>
        WriteOff,

        /// <summary>
        /// Sample items for marketing or testing
        /// </summary>
        Samples,

        /// <summary>
        /// Reservation fulfillment
        /// </summary>
        ReservationFulfillment
    }
}