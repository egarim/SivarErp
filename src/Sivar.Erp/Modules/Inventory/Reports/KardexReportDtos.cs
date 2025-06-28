using System;
using System.Collections.Generic;
using Sivar.Erp.Documents;

namespace Sivar.Erp.Modules.Inventory.Reports
{
    /// <summary>
    /// Data Transfer Object for a kardex report
    /// </summary>
    public class KardexReportDto
    {
        /// <summary>
        /// Gets or sets the report ID
        /// </summary>
        public string ReportId { get; set; }
        
        /// <summary>
        /// Gets or sets the item this report is for
        /// </summary>
        public IInventoryItem Item { get; set; }
        
        /// <summary>
        /// Gets or sets the warehouse code this report is for
        /// </summary>
        public string WarehouseCode { get; set; }
        
        /// <summary>
        /// Gets or sets the start date of the report period
        /// </summary>
        public DateOnly StartDate { get; set; }
        
        /// <summary>
        /// Gets or sets the end date of the report period
        /// </summary>
        public DateOnly EndDate { get; set; }
        
        /// <summary>
        /// Gets or sets the opening balance quantity
        /// </summary>
        public decimal OpeningQuantity { get; set; }
        
        /// <summary>
        /// Gets or sets the opening balance value
        /// </summary>
        public decimal OpeningValue { get; set; }
        
        /// <summary>
        /// Gets or sets the closing balance quantity
        /// </summary>
        public decimal ClosingQuantity { get; set; }
        
        /// <summary>
        /// Gets or sets the closing balance value
        /// </summary>
        public decimal ClosingValue { get; set; }
        
        /// <summary>
        /// Gets or sets the total inbound quantity
        /// </summary>
        public decimal TotalInboundQuantity { get; set; }
        
        /// <summary>
        /// Gets or sets the total inbound value
        /// </summary>
        public decimal TotalInboundValue { get; set; }
        
        /// <summary>
        /// Gets or sets the total outbound quantity
        /// </summary>
        public decimal TotalOutboundQuantity { get; set; }
        
        /// <summary>
        /// Gets or sets the total outbound value
        /// </summary>
        public decimal TotalOutboundValue { get; set; }
        
        /// <summary>
        /// Gets or sets the generated date
        /// </summary>
        public DateTime GeneratedDate { get; set; }
        
        /// <summary>
        /// Gets or sets the user who generated the report
        /// </summary>
        public string GeneratedBy { get; set; }
        
        /// <summary>
        /// Gets or sets the movements in the report
        /// </summary>
        public List<KardexMovementDto> Movements { get; set; } = new List<KardexMovementDto>();
    }
    
    /// <summary>
    /// Data Transfer Object for a kardex movement entry
    /// </summary>
    public class KardexMovementDto
    {
        /// <summary>
        /// Gets or sets the movement ID
        /// </summary>
        public string MovementId { get; set; }
        
        /// <summary>
        /// Gets or sets the underlying inventory transaction
        /// </summary>
        public IInventoryTransaction Transaction { get; set; }
        
        /// <summary>
        /// Gets or sets the transaction date
        /// </summary>
        public DateOnly TransactionDate { get; set; }
        
        /// <summary>
        /// Gets or sets the transaction reference document
        /// </summary>
        public string ReferenceDocument { get; set; }
        
        /// <summary>
        /// Gets or sets the description of this movement
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Gets or sets the inbound quantity (receipts)
        /// </summary>
        public decimal InboundQuantity { get; set; }
        
        /// <summary>
        /// Gets or sets the inbound value
        /// </summary>
        public decimal InboundValue { get; set; }
        
        /// <summary>
        /// Gets or sets the outbound quantity (issues)
        /// </summary>
        public decimal OutboundQuantity { get; set; }
        
        /// <summary>
        /// Gets or sets the outbound value
        /// </summary>
        public decimal OutboundValue { get; set; }
        
        /// <summary>
        /// Gets or sets the balance quantity after this movement
        /// </summary>
        public decimal BalanceQuantity { get; set; }
        
        /// <summary>
        /// Gets or sets the balance value after this movement
        /// </summary>
        public decimal BalanceValue { get; set; }
        
        /// <summary>
        /// Gets or sets the average unit cost after this movement
        /// </summary>
        public decimal AverageUnitCost { get; set; }
    }
    
    /// <summary>
    /// Data Transfer Object for an inventory valuation report
    /// </summary>
    public class InventoryValuationReportDto
    {
        /// <summary>
        /// Gets or sets the report ID
        /// </summary>
        public string ReportId { get; set; }
        
        /// <summary>
        /// Gets or sets the date of the valuation
        /// </summary>
        public DateOnly AsOfDate { get; set; }
        
        /// <summary>
        /// Gets or sets the warehouse code filter (if any)
        /// </summary>
        public string WarehouseCode { get; set; }
        
        /// <summary>
        /// Gets or sets the generated date
        /// </summary>
        public DateTime GeneratedDate { get; set; }
        
        /// <summary>
        /// Gets or sets the user who generated the report
        /// </summary>
        public string GeneratedBy { get; set; }
        
        /// <summary>
        /// Gets or sets the total inventory value
        /// </summary>
        public decimal TotalInventoryValue { get; set; }
        
        /// <summary>
        /// Gets or sets the report line items
        /// </summary>
        public List<InventoryValuationItemDto> Items { get; set; } = new List<InventoryValuationItemDto>();
    }
    
    /// <summary>
    /// Data Transfer Object for an inventory valuation item
    /// </summary>
    public class InventoryValuationItemDto
    {
        /// <summary>
        /// Gets or sets the inventory item
        /// </summary>
        public IInventoryItem Item { get; set; }
        
        /// <summary>
        /// Gets or sets the warehouse code
        /// </summary>
        public string WarehouseCode { get; set; }
        
        /// <summary>
        /// Gets or sets the quantity on hand
        /// </summary>
        public decimal QuantityOnHand { get; set; }
        
        /// <summary>
        /// Gets or sets the average cost per unit
        /// </summary>
        public decimal AverageCost { get; set; }
        
        /// <summary>
        /// Gets or sets the total value of this item
        /// </summary>
        public decimal TotalValue { get; set; }
    }
    
    /// <summary>
    /// Data Transfer Object for an item that needs reordering
    /// </summary>
    public class ReorderItemDto
    {
        /// <summary>
        /// Gets or sets the inventory item
        /// </summary>
        public IInventoryItem Item { get; set; }
        
        /// <summary>
        /// Gets or sets the warehouse code
        /// </summary>
        public string WarehouseCode { get; set; }
        
        /// <summary>
        /// Gets or sets the quantity on hand
        /// </summary>
        public decimal QuantityOnHand { get; set; }
        
        /// <summary>
        /// Gets or sets the available quantity (on hand minus reserved)
        /// </summary>
        public decimal AvailableQuantity { get; set; }
        
        /// <summary>
        /// Gets or sets the reorder point
        /// </summary>
        public decimal ReorderPoint { get; set; }
        
        /// <summary>
        /// Gets or sets the suggested reorder quantity
        /// </summary>
        public decimal SuggestedOrderQuantity { get; set; }
        
        /// <summary>
        /// Gets or sets the last purchase date
        /// </summary>
        public DateOnly? LastPurchaseDate { get; set; }
        
        /// <summary>
        /// Gets or sets the last purchase cost
        /// </summary>
        public decimal? LastPurchaseCost { get; set; }
    }
}