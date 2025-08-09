using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using Sivar.Erp.Modules.Inventory;
using System;
using System.ComponentModel;
using DevExpress.ExpressApp.Editors;

namespace Sivar.Erp.Xaf.Module.BusinessObjects.Inventory
{
    /// <summary>
    /// XAF persistent implementation of IInventoryTransaction
    /// Represents inventory movements (receipts, issues, adjustments)
    /// </summary>
    [DefaultClassOptions]
    [NavigationItem("Inventory")]
    [DefaultProperty(nameof(DisplayName))]
    [XafDisplayName("Inventory Transaction")]
    [XafDefaultProperty(nameof(DisplayName))]
    [Appearance("Highlight_Receipts", AppearanceItemType = "ViewItem",
        Criteria = "Quantity > 0",
        BackColor = "200, 255, 200", FontColor = "0, 128, 0",
        TargetItems = "Quantity;TotalValue")]
    [Appearance("Highlight_Issues", AppearanceItemType = "ViewItem",
        Criteria = "Quantity < 0",
        BackColor = "255, 200, 200", FontColor = "128, 0, 0",
        TargetItems = "Quantity;TotalValue")]
    [Appearance("Show_DestinationWarehouse_When_Transfer", AppearanceItemType = "ViewItem",
        Criteria = "TransactionType = 'Transfer'",
        TargetItems = "DestinationWarehouseCode",
        Visibility = ViewItemVisibility.Show)]
    [Appearance("Hide_DestinationWarehouse_When_NotTransfer", AppearanceItemType = "ViewItem",
        Criteria = "TransactionType <> 'Transfer'",
        TargetItems = "DestinationWarehouseCode",
        Visibility = ViewItemVisibility.Hide)]
    public class InventoryTransaction : ErpBaseObject, IInventoryTransaction
    {
        public InventoryTransaction(Session session) : base(session) { }

        string transactionId = string.Empty;
        /// <summary>
        /// Unique transaction identifier
        /// </summary>
        [RuleRequiredField("InventoryTransaction_TransactionId_Required", DefaultContexts.Save)]
        [RuleUniqueValue("InventoryTransaction_TransactionId_Unique", DefaultContexts.Save)]
        [Size(50)]
        [XafDisplayName("Transaction ID")]
        [Index(0)]
        public string TransactionId
        {
            get => transactionId;
            set => SetPropertyValue(nameof(TransactionId), ref transactionId, value);
        }

        string transactionNumber = string.Empty;
        /// <summary>
        /// User-friendly transaction number
        /// </summary>
        [Size(50)]
        [XafDisplayName("Transaction Number")]
        [Index(1)]
        public string TransactionNumber
        {
            get => transactionNumber;
            set => SetPropertyValue(nameof(TransactionNumber), ref transactionNumber, value);
        }

        InventoryItem item;
        /// <summary>
        /// The inventory item being transacted
        /// </summary>
        [Association("InventoryItem-InventoryTransactions")]
        [RuleRequiredField("InventoryTransaction_Item_Required", DefaultContexts.Save)]
        [XafDisplayName("Item")]
        [Index(2)]
        public InventoryItem Item
        {
            get => item;
            set => SetPropertyValue(nameof(Item), ref item, value);
        }

        InventoryTransactionType transactionType = InventoryTransactionType.Adjustment;
        /// <summary>
        /// Type of inventory transaction
        /// </summary>
        [XafDisplayName("Transaction Type")]
        [Index(3)]
        public InventoryTransactionType TransactionType
        {
            get => transactionType;
            set => SetPropertyValue(nameof(TransactionType), ref transactionType, value);
        }

        decimal? quantity;
        /// <summary>
        /// Quantity (positive for receipts, negative for issues)
        /// </summary>
        [RuleRequiredField("InventoryTransaction_Quantity_Required", DefaultContexts.Save)]
        [XafDisplayName("Quantity")]
        [Index(4)]
        [ModelDefault("DisplayFormat", "n2")]
        [ModelDefault("EditMask", "n2")]
        public decimal? Quantity
        {
            get => quantity;
            set => SetPropertyValue(nameof(Quantity), ref quantity, value);
        }

        decimal unitCost = 0m;
        /// <summary>
        /// Unit cost for this transaction
        /// </summary>
        [XafDisplayName("Unit Cost")]
        [Index(5)]
        [ModelDefault("DisplayFormat", "c4")]
        [ModelDefault("EditMask", "c4")]
        public decimal UnitCost
        {
            get => unitCost;
            set => SetPropertyValue(nameof(UnitCost), ref unitCost, value);
        }

        string sourceWarehouseCode = string.Empty;
        /// <summary>
        /// Source warehouse code
        /// </summary>
        [RuleRequiredField("InventoryTransaction_SourceWarehouse_Required", DefaultContexts.Save)]
        [Size(50)]
        [XafDisplayName("Source Warehouse")]
        [Index(6)]
        public string SourceWarehouseCode
        {
            get => sourceWarehouseCode;
            set => SetPropertyValue(nameof(SourceWarehouseCode), ref sourceWarehouseCode, value?.ToUpperInvariant());
        }

        string destinationWarehouseCode = string.Empty;
        /// <summary>
        /// Destination warehouse code (for transfers)
        /// </summary>
        [Size(50)]
        [XafDisplayName("Destination Warehouse")]
        [Index(7)]
        public string DestinationWarehouseCode
        {
            get => destinationWarehouseCode;
            set => SetPropertyValue(nameof(DestinationWarehouseCode), ref destinationWarehouseCode, value?.ToUpperInvariant());
        }

        string referenceDocumentNumber = string.Empty;
        /// <summary>
        /// Reference document number
        /// </summary>
        [Size(100)]
        [XafDisplayName("Reference Document")]
        [Index(8)]
        public string ReferenceDocumentNumber
        {
            get => referenceDocumentNumber;
            set => SetPropertyValue(nameof(ReferenceDocumentNumber), ref referenceDocumentNumber, value);
        }

        DateOnly transactionDate = DateOnly.FromDateTime(DateTime.UtcNow);
        /// <summary>
        /// Transaction date
        /// </summary>
        [XafDisplayName("Transaction Date")]
        [Index(9)]
        public DateOnly TransactionDate
        {
            get => transactionDate;
            set => SetPropertyValue(nameof(TransactionDate), ref transactionDate, value);
        }

        DateTime createdAt = DateTime.UtcNow;
        /// <summary>
        /// When the transaction was created
        /// </summary>
        [XafDisplayName("Created At")]
        [Index(10)]
        [ModelDefault("AllowEdit", "False")]
        public DateTime CreatedAt
        {
            get => createdAt;
            set => SetPropertyValue(nameof(CreatedAt), ref createdAt, value);
        }

        string notes = string.Empty;
        /// <summary>
        /// Additional notes about the transaction
        /// </summary>
        [Size(SizeAttribute.Unlimited)]
        [XafDisplayName("Notes")]
        [Index(11)]
        public string Notes
        {
            get => notes;
            set => SetPropertyValue(nameof(Notes), ref notes, value);
        }

        StockLevel stockLevel;
        /// <summary>
        /// Associated stock level record
        /// </summary>
        [Association("StockLevel-InventoryTransactions")]
        [XafDisplayName("Stock Level")]
        [Index(12)]
        public StockLevel StockLevel
        {
            get => stockLevel;
            set => SetPropertyValue(nameof(StockLevel), ref stockLevel, value);
        }

        // Interface implementation - explicit for IInventoryTransaction.Item
        Sivar.Erp.Documents.IInventoryItem IInventoryTransaction.Item 
        { 
            get => Item; 
            set => Item = value as InventoryItem; 
        }

        // Interface implementation - non-nullable quantity for interface
        decimal IInventoryTransaction.Quantity
        {
            get => Quantity ?? 0m;
            set => Quantity = value;
        }

        /// <summary>
        /// Total value of this transaction
        /// </summary>
        [PersistentAlias("(Quantity ?? 0) * UnitCost")]
        [XafDisplayName("Total Value")]
        [Index(13)]
        [ModelDefault("DisplayFormat", "c2")]
        [ModelDefault("AllowEdit", "False")]
        public decimal TotalValue => (Quantity ?? 0m) * UnitCost;

        /// <summary>
        /// Display name for the transaction
        /// </summary>
        [PersistentAlias("Concat(TransactionNumber, ' - ', Item.Code)")]
        [XafDisplayName("Display Name")]
        [ModelDefault("AllowEdit", "False")]
        public string DisplayName => $"{TransactionNumber} - {Item?.Code}";

        /// <summary>
        /// Whether this is a receipt transaction (positive quantity)
        /// </summary>
        [PersistentAlias("(Quantity ?? 0) > 0")]
        [XafDisplayName("Is Receipt")]
        [ModelDefault("AllowEdit", "False")]
        public bool IsReceipt => (Quantity ?? 0m) > 0;

        /// <summary>
        /// Whether this is an issue transaction (negative quantity)
        /// </summary>
        [PersistentAlias("(Quantity ?? 0) < 0")]
        [XafDisplayName("Is Issue")]
        [ModelDefault("AllowEdit", "False")]
        public bool IsIssue => (Quantity ?? 0m) < 0;

        // Business rule validations
        [RuleFromBoolProperty("InventoryTransaction_Quantity_NotZero", DefaultContexts.Save,
            "Transaction quantity cannot be zero")]
        public bool IsQuantityValid => Quantity.HasValue && Quantity.Value != 0;

        [RuleFromBoolProperty("InventoryTransaction_UnitCost_NotNegative", DefaultContexts.Save,
            "Unit cost cannot be negative")]
        public bool IsUnitCostValid => UnitCost >= 0;

        [RuleFromBoolProperty("InventoryTransaction_Transfer_RequiresDestination", DefaultContexts.Save,
            "Transfer transactions require a destination warehouse")]
        public bool IsTransferValid => TransactionType != InventoryTransactionType.Transfer || 
                                      !string.IsNullOrEmpty(DestinationWarehouseCode);

        [RuleFromBoolProperty("InventoryTransaction_Transfer_DifferentWarehouses", DefaultContexts.Save,
            "Transfer source and destination warehouses must be different")]
        public bool IsTransferWarehousesValid => TransactionType != InventoryTransactionType.Transfer ||
                                               SourceWarehouseCode != DestinationWarehouseCode;

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow);
            CreatedAt = DateTime.UtcNow;
            TransactionId = Guid.NewGuid().ToString();
        }

        protected override void OnSaving()
        {
            base.OnSaving();
            
            // Auto-generate transaction number if not set
            if (string.IsNullOrEmpty(TransactionNumber) && !string.IsNullOrEmpty(TransactionId))
            {
                TransactionNumber = $"TXN-{TransactionId.Substring(0, 8).ToUpper()}";
            }
        }
    }
}