using System;
using Sivar.Erp.Modules.Inventory.Core.Interfaces;
using Sivar.Erp.Modules.Documents.Core.Interfaces;
using Sivar.Erp.Core.Contracts;

namespace Sivar.Erp.Modules.Documents.Application.DTOs
{
    /// <summary>
    /// DTO implementation of IDocumentAccountingProfile
    /// </summary>
    public class DocumentAccountingProfileDto : Modules.Documents.Core.Interfaces.IDocumentAccountingProfile, Sivar.Erp.Core.Contracts.IDocumentAccountingProfile
    {
        /// <summary>
        /// Unique identifier for the accounting profile
        /// </summary>
        public Guid ID { get; set; } = Guid.NewGuid();
        
        /// <summary>
        /// Document operation this profile applies to (e.g., "SalesInvoice", "PurchaseOrder")
        /// </summary>
        public string DocumentOperation { get; set; } = string.Empty;
        
        /// <summary>
        /// Account code for sales transactions
        /// </summary>
        public string SalesAccountCode { get; set; } = string.Empty;
        
        /// <summary>
        /// Account code for accounts receivable
        /// </summary>
        public string AccountsReceivableCode { get; set; } = string.Empty;
        
        /// <summary>
        /// Account code for cost of goods sold
        /// </summary>
        public string CostOfGoodsSoldAccountCode { get; set; } = string.Empty;
        
        /// <summary>
        /// Account code for inventory
        /// </summary>
        public string InventoryAccountCode { get; set; } = string.Empty;
        
        /// <summary>
        /// Cost ratio for calculations
        /// </summary>
        public decimal CostRatio { get; set; }
        
        /// <summary>
        /// User who created this profile
        /// </summary>
        public string CreatedBy { get; set; } = string.Empty;
        
        /// <summary>
        /// Date and time when this profile was created
        /// </summary>
        public DateTimeOffset CreatedDate { get; set; } = DateTimeOffset.Now;
    }
}
