using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using Sivar.Erp.Documents;
using System;
using System.ComponentModel;

namespace Sivar.Erp.Xaf.Module.BusinessObjects.Documents
{
    /// <summary>
    /// XAF persistent implementation of IDocumentAccountingProfile
    /// Defines accounting rules for specific document operations
    /// </summary>
    [DefaultClassOptions]
    [NavigationItem("Configuration")]
    [DefaultProperty(nameof(DocumentOperation))]
    [XafDisplayName("Document Accounting Profile")]
    [XafDefaultProperty(nameof(DocumentOperation))]
    public class DocumentAccountingProfile : ErpBaseObject, IDocumentAccountingProfile
    {
        public DocumentAccountingProfile(Session session) : base(session) { }

        string documentOperation = string.Empty;
        /// <summary>
        /// Document operation this profile applies to (e.g. "SalesInvoice", "PurchaseInvoice")
        /// </summary>
        [RuleRequiredField("DocumentAccountingProfile_DocumentOperation_Required", DefaultContexts.Save)]
        [RuleUniqueValue("DocumentAccountingProfile_DocumentOperation_Unique", DefaultContexts.Save)]
        [Size(50)]
        [XafDisplayName("Document Operation")]
        [Index(0)]
        public string DocumentOperation
        {
            get => documentOperation;
            set => SetPropertyValue(nameof(DocumentOperation), ref documentOperation, value?.ToUpperInvariant());
        }

        string salesAccountCode = string.Empty;
        /// <summary>
        /// Account code to use for sales or revenue
        /// </summary>
        [Size(50)]
        [XafDisplayName("Sales Account Code")]
        [Index(1)]
        [ToolTip("Account code for recording sales revenue")]
        public string SalesAccountCode
        {
            get => salesAccountCode;
            set => SetPropertyValue(nameof(SalesAccountCode), ref salesAccountCode, value);
        }

        string accountsReceivableCode = string.Empty;
        /// <summary>
        /// Account code to use for accounts receivable
        /// </summary>
        [Size(50)]
        [XafDisplayName("Accounts Receivable Code")]
        [Index(2)]
        [ToolTip("Account code for recording customer balances")]
        public string AccountsReceivableCode
        {
            get => accountsReceivableCode;
            set => SetPropertyValue(nameof(AccountsReceivableCode), ref accountsReceivableCode, value);
        }

        string costOfGoodsSoldAccountCode = string.Empty;
        /// <summary>
        /// Account code to use for cost of goods sold
        /// </summary>
        [Size(50)]
        [XafDisplayName("Cost of Goods Sold Code")]
        [Index(3)]
        [ToolTip("Account code for recording cost of goods sold")]
        public string CostOfGoodsSoldAccountCode
        {
            get => costOfGoodsSoldAccountCode;
            set => SetPropertyValue(nameof(CostOfGoodsSoldAccountCode), ref costOfGoodsSoldAccountCode, value);
        }

        string inventoryAccountCode = string.Empty;
        /// <summary>
        /// Account code to use for inventory
        /// </summary>
        [Size(50)]
        [XafDisplayName("Inventory Account Code")]
        [Index(4)]
        [ToolTip("Account code for recording inventory movements")]
        public string InventoryAccountCode
        {
            get => inventoryAccountCode;
            set => SetPropertyValue(nameof(InventoryAccountCode), ref inventoryAccountCode, value);
        }

        decimal costRatio = 0m;
        /// <summary>
        /// Cost ratio used for calculating cost of goods sold (e.g. 0.6 for 60%)
        /// </summary>
        [XafDisplayName("Cost Ratio")]
        [Index(5)]
        [ModelDefault("DisplayFormat", "P2")]
        [ToolTip("Percentage of sales to be recorded as cost of goods sold (e.g., 0.6 = 60%)")]
        public decimal CostRatio
        {
            get => costRatio;
            set => SetPropertyValue(nameof(CostRatio), ref costRatio, value);
        }

        // Interface implementation - these are handled by ErpBaseObject
        string IDocumentAccountingProfile.CreatedBy
        {
            get => CreatedBy;
            set => CreatedBy = value;
        }

        DateTimeOffset IDocumentAccountingProfile.CreatedDate
        {
            get => new DateTimeOffset(CreatedOn);
            set => CreatedOn = value.DateTime;
        }

        // Explicit interface implementation for Oid
        Guid IDocumentAccountingProfile.Oid
        {
            get => Oid;
            set => throw new NotSupportedException("Use XPO's Oid property for persistence");
        }
    }
}