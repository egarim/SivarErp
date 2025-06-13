using System;

namespace Sivar.Erp.Documents
{
    /// <summary>
    /// Defines the category of a document.
    /// </summary>
    public enum DocumentCategory
    {
        /// <summary>
        /// Purchase Requisition
        /// </summary>
        PurchaseRequisition,
        /// <summary>
        /// Request for Quotation
        /// </summary>
        RequestForQuotation,
        /// <summary>
        /// Purchase Order
        /// </summary>
        PurchaseOrder,
        /// <summary>
        /// Goods Receipt Note
        /// </summary>
        GoodsReceiptNote,
        /// <summary>
        /// Purchase Invoice
        /// </summary>
        PurchaseInvoice,
        /// <summary>
        /// Debit Note / Purchase Return
        /// </summary>
        DebitNote,
        /// <summary>
        /// Quotation / Sales Proposal
        /// </summary>
        Quotation,
        /// <summary>
        /// Sales Order
        /// </summary>
        SalesOrder,
        /// <summary>
        /// Delivery Note / Packing Slip
        /// </summary>
        DeliveryNote,
        /// <summary>
        /// Sales Invoice
        /// </summary>
        SalesInvoice,
        /// <summary>
        /// Credit Note / Sales Return
        /// </summary>
        CreditNote,
        /// <summary>
        /// Receipt / Payment Confirmation
        /// </summary>
        Receipt
    }
}