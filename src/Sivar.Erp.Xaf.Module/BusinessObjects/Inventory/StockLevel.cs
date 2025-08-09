using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using Sivar.Erp.Modules.Inventory;
using System;
using System.ComponentModel;
using DevExpress.ExpressApp.Editors;

namespace Sivar.Erp.Xaf.Module.BusinessObjects.Inventory
{
    /// <summary>
    /// XAF persistent implementation of IStockLevel
    /// Represents current stock levels for items in warehouses
    /// </summary>
    [DefaultClassOptions]
    [NavigationItem("Inventory")]
    [DefaultProperty(nameof(DisplayName))]
    [XafDisplayName("Stock Level")]
    [XafDefaultProperty(nameof(DisplayName))]
    [Appearance("Highlight_LowStock", AppearanceItemType = "ViewItem",
        Criteria = "AvailableQuantity <= Item.ReorderPoint AND Item.ReorderPoint > 0",
        BackColor = "255, 255, 200", FontColor = "255, 0, 0",
        TargetItems = "QuantityOnHand;AvailableQuantity")]
    [Appearance("Hide_ReservedFields_When_NoReserved", AppearanceItemType = "ViewItem",
        Criteria = "QuantityReserved = 0",
        TargetItems = "QuantityReserved",
        Visibility = ViewItemVisibility.Hide)]
    public class StockLevel : ErpBaseObject, IStockLevel
    {
        public StockLevel(Session session) : base(session) { }

        string id = string.Empty;
        /// <summary>
        /// ID of the stock level entry
        /// </summary>
        [Size(50)]
        [XafDisplayName("ID")]
        [Index(0)]
        [ModelDefault("AllowEdit", "False")]
        public string Id
        {
            get => id;
            set => SetPropertyValue(nameof(Id), ref id, value);
        }

        InventoryItem item;
        /// <summary>
        /// The inventory item this stock level represents
        /// </summary>
        [Association("InventoryItem-StockLevels")]
        [RuleRequiredField("StockLevel_Item_Required", DefaultContexts.Save)]
        [XafDisplayName("Item")]
        [Index(1)]
        public InventoryItem Item
        {
            get => item;
            set => SetPropertyValue(nameof(Item), ref item, value);
        }

        string warehouseCode = string.Empty;
        /// <summary>
        /// Warehouse code where the stock is located
        /// </summary>
        [RuleRequiredField("StockLevel_WarehouseCode_Required", DefaultContexts.Save)]
        [Size(50)]
        [XafDisplayName("Warehouse")]
        [Index(2)]
        public string WarehouseCode
        {
            get => warehouseCode;
            set => SetPropertyValue(nameof(WarehouseCode), ref warehouseCode, value?.ToUpperInvariant());
        }

        decimal quantityOnHand = 0m;
        /// <summary>
        /// Total quantity physically on hand
        /// </summary>
        [XafDisplayName("Quantity On Hand")]
        [Index(3)]
        [ModelDefault("DisplayFormat", "n2")]
        [ModelDefault("EditMask", "n2")]
        public decimal QuantityOnHand
        {
            get => quantityOnHand;
            set => SetPropertyValue(nameof(QuantityOnHand), ref quantityOnHand, value);
        }

        decimal quantityReserved = 0m;
        /// <summary>
        /// Quantity reserved for orders/documents
        /// </summary>
        [XafDisplayName("Quantity Reserved")]
        [Index(4)]
        [ModelDefault("DisplayFormat", "n2")]
        [ModelDefault("EditMask", "n2")]
        [ModelDefault("AllowEdit", "False")]
        public decimal QuantityReserved
        {
            get => quantityReserved;
            set => SetPropertyValue(nameof(QuantityReserved), ref quantityReserved, value);
        }

        decimal quantityOnOrder = 0m;
        /// <summary>
        /// Quantity on purchase orders (expected receipts)
        /// </summary>
        [XafDisplayName("Quantity On Order")]
        [Index(5)]
        [ModelDefault("DisplayFormat", "n2")]
        [ModelDefault("EditMask", "n2")]
        [ModelDefault("AllowEdit", "False")]
        public decimal QuantityOnOrder
        {
            get => quantityOnOrder;
            set => SetPropertyValue(nameof(QuantityOnOrder), ref quantityOnOrder, value);
        }

        DateTime lastUpdated = DateTime.UtcNow;
        /// <summary>
        /// Stock level last updated date
        /// </summary>
        [XafDisplayName("Last Updated")]
        [Index(6)]
        [ModelDefault("AllowEdit", "False")]
        public DateTime LastUpdated
        {
            get => lastUpdated;
            set => SetPropertyValue(nameof(LastUpdated), ref lastUpdated, value);
        }

        decimal minimumLevel = 0m;
        /// <summary>
        /// Minimum stock level for this warehouse
        /// </summary>
        [XafDisplayName("Minimum Level")]
        [Index(7)]
        [ModelDefault("DisplayFormat", "n2")]
        [ModelDefault("EditMask", "n2")]
        public decimal MinimumLevel
        {
            get => minimumLevel;
            set => SetPropertyValue(nameof(MinimumLevel), ref minimumLevel, value);
        }

        decimal maximumLevel = 0m;
        /// <summary>
        /// Maximum stock level for this warehouse
        /// </summary>
        [XafDisplayName("Maximum Level")]
        [Index(8)]
        [ModelDefault("DisplayFormat", "n2")]
        [ModelDefault("EditMask", "n2")]
        public decimal MaximumLevel
        {
            get => maximumLevel;
            set => SetPropertyValue(nameof(MaximumLevel), ref maximumLevel, value);
        }

        DateTime lastMovementDate = DateTime.UtcNow;
        /// <summary>
        /// Date of last inventory movement
        /// </summary>
        [XafDisplayName("Last Movement")]
        [Index(9)]
        [ModelDefault("AllowEdit", "False")]
        public DateTime LastMovementDate
        {
            get => lastMovementDate;
            set => SetPropertyValue(nameof(LastMovementDate), ref lastMovementDate, value);
        }

        DateTime lastCountDate = DateTime.UtcNow;
        /// <summary>
        /// Date of last physical count
        /// </summary>
        [XafDisplayName("Last Count")]
        [Index(10)]
        public DateTime LastCountDate
        {
            get => lastCountDate;
            set => SetPropertyValue(nameof(LastCountDate), ref lastCountDate, value);
        }

        string binLocation = string.Empty;
        /// <summary>
        /// Default bin/location within the warehouse
        /// </summary>
        [Size(50)]
        [XafDisplayName("Bin Location")]
        [Index(11)]
        public string BinLocation
        {
            get => binLocation;
            set => SetPropertyValue(nameof(BinLocation), ref binLocation, value);
        }

        // Interface implementation - explicit for IStockLevel.Item
        Sivar.Erp.Documents.IInventoryItem IStockLevel.Item 
        { 
            get => Item; 
            set => Item = value as InventoryItem; 
        }

        /// <summary>
        /// Available quantity (on hand minus reserved)
        /// </summary>
        [PersistentAlias("QuantityOnHand - QuantityReserved")]
        [XafDisplayName("Available Quantity")]
        [Index(12)]
        [ModelDefault("DisplayFormat", "n2")]
        [ModelDefault("AllowEdit", "False")]
        public decimal AvailableQuantity => QuantityOnHand - QuantityReserved;

        /// <summary>
        /// Display name for the stock level
        /// </summary>
        [PersistentAlias("Concat(Item.Code, ' - ', WarehouseCode)")]
        [XafDisplayName("Display Name")]
        [ModelDefault("AllowEdit", "False")]
        public string DisplayName => $"{Item?.Code} - {WarehouseCode}";

        /// <summary>
        /// Whether stock is below reorder point
        /// </summary>
        [PersistentAlias("AvailableQuantity <= Item.ReorderPoint AND Item.ReorderPoint > 0")]
        [XafDisplayName("Low Stock")]
        [ModelDefault("AllowEdit", "False")]
        public bool IsLowStock => AvailableQuantity <= (Item?.ReorderPoint ?? 0) && (Item?.ReorderPoint ?? 0) > 0;

        // Navigation properties
        [Association("StockLevel-InventoryTransactions")]
        [XafDisplayName("Transactions")]
        public XPCollection<InventoryTransaction> Transactions => GetCollection<InventoryTransaction>(nameof(Transactions));

        [Association("StockLevel-InventoryReservations")]
        [XafDisplayName("Reservations")]
        public XPCollection<InventoryReservation> Reservations => GetCollection<InventoryReservation>(nameof(Reservations));

        // Business rule validations
        [RuleFromBoolProperty("StockLevel_QuantityReserved_NotExceedOnHand", DefaultContexts.Save,
            "Reserved quantity cannot exceed quantity on hand")]
        public bool IsReservedQuantityValid => QuantityReserved <= QuantityOnHand;

        [RuleFromBoolProperty("StockLevel_MinimumLevel_NotExceedMaximum", DefaultContexts.Save,
            "Minimum level cannot exceed maximum level")]
        public bool IsMinMaxLevelValid => MinimumLevel <= MaximumLevel || MaximumLevel == 0;

        [RuleFromBoolProperty("StockLevel_ItemWarehouse_Unique", DefaultContexts.Save,
            "Each item can only have one stock level per warehouse")]
        public bool IsItemWarehouseUnique
        {
            get
            {
                if (Item == null || string.IsNullOrEmpty(WarehouseCode))
                    return true;

                var existing = Session.FindObject<StockLevel>(
                    CriteriaOperator.And(
                        CriteriaOperator.Parse("Item = ?", Item),
                        CriteriaOperator.Parse("WarehouseCode = ?", WarehouseCode),
                        CriteriaOperator.Parse("Oid != ?", Oid)));

                return existing == null;
            }
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            Id = Guid.NewGuid().ToString();
            LastMovementDate = DateTime.UtcNow;
            LastCountDate = DateTime.UtcNow;
            LastUpdated = DateTime.UtcNow;
        }

        protected override void OnSaving()
        {
            base.OnSaving();
            
            // Update last movement date when quantities change
            if (Session.IsNewObject(this))
            {
                LastMovementDate = DateTime.UtcNow;
            }
            
            LastUpdated = DateTime.UtcNow;
        }
    }
}