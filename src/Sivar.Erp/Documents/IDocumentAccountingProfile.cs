using System;

namespace Sivar.Erp.Documents
{
    /// <summary>
    /// Represents a profile that defines accounting rules for a specific document operation
    /// </summary>
    public interface IDocumentAccountingProfile
    {
        /// <summary>
        /// Unique identifier for the profile
        /// </summary>
        Guid Oid { get; set; }

        /// <summary>
        /// Document operation this profile applies to (e.g. "SalesInvoice", "PurchaseInvoice")
        /// </summary>
        string DocumentOperation { get; set; }

        /// <summary>
        /// Account code to use for sales or revenue
        /// </summary>
        string SalesAccountCode { get; set; }

        /// <summary>
        /// Account code to use for accounts receivable
        /// </summary>
        string AccountsReceivableCode { get; set; }

        /// <summary>
        /// Account code to use for cost of goods sold
        /// </summary>
        string CostOfGoodsSoldAccountCode { get; set; }

        /// <summary>
        /// Account code to use for inventory
        /// </summary>
        string InventoryAccountCode { get; set; }

        /// <summary>
        /// Cost ratio used for calculating cost of goods sold (e.g. 0.6 for 60%)
        /// </summary>
        decimal CostRatio { get; set; }

        /// <summary>
        /// Created by user
        /// </summary>
        string CreatedBy { get; set; }

        /// <summary>
        /// Created date
        /// </summary>
        DateTimeOffset CreatedDate { get; set; }
    }
}