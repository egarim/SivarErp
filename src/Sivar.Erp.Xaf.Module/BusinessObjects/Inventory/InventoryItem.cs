using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using Sivar.Erp.Documents;
using System.ComponentModel;
using DevExpress.ExpressApp.Editors;

namespace Sivar.Erp.Xaf.Module.BusinessObjects.Inventory
{
    /// <summary>
    /// XAF persistent implementation of IInventoryItem
    /// Represents inventory items with stock management capabilities
    /// </summary>
    [DefaultClassOptions]
    [NavigationItem("Inventory")]
    [DefaultProperty(nameof(Description))]
    [XafDisplayName("Inventory Item")]
    [XafDefaultProperty(nameof(Description))]
    [Appearance("Hide_TrackingFields_When_NoTracking", AppearanceItemType = "ViewItem",
        Criteria = "IsLotTracked = false AND IsSerialTracked = false",
        TargetItems = "LotSize;SerialNumberLength",
        Visibility = ViewItemVisibility.Hide)]
    [Appearance("Enable_CostFields_When_StandardCost", AppearanceItemType = "ViewItem",
        Criteria = "ValuationMethod = 'StandardCost'",
        TargetItems = "StandardCost",
        Enabled = true)]
    [Appearance("Disable_CostFields_When_NotStandardCost", AppearanceItemType = "ViewItem",
        Criteria = "ValuationMethod <> 'StandardCost'",
        TargetItems = "StandardCost",
        Enabled = false)]
    public class InventoryItem : ErpBaseObject, IInventoryItem
    {
        public InventoryItem(Session session) : base(session) { }

        string code = string.Empty;
        /// <summary>
        /// Unique inventory item code (SKU)
        /// </summary>
        [RuleRequiredField("InventoryItem_Code_Required", DefaultContexts.Save)]
        [RuleUniqueValue("InventoryItem_Code_Unique", DefaultContexts.Save)]
        [Size(50)]
        [XafDisplayName("Item Code")]
        [Index(0)]
        public string Code
        {
            get => code;
            set => SetPropertyValue(nameof(Code), ref code, value?.ToUpperInvariant());
        }

        string type = string.Empty;
        /// <summary>
        /// Item type/category classification
        /// </summary>
        [RuleRequiredField("InventoryItem_Type_Required", DefaultContexts.Save)]
        [Size(50)]
        [XafDisplayName("Type")]
        [Index(1)]
        public string Type
        {
            get => type;
            set => SetPropertyValue(nameof(Type), ref type, value);
        }

        string description = string.Empty;
        /// <summary>
        /// Item description/name
        /// </summary>
        [RuleRequiredField("InventoryItem_Description_Required", DefaultContexts.Save)]
        [Size(500)]
        [XafDisplayName("Description")]
        [Index(2)]
        public string Description
        {
            get => description;
            set => SetPropertyValue(nameof(Description), ref description, value);
        }

        decimal basePrice = 0m;
        /// <summary>
        /// Base price of the item
        /// </summary>
        [XafDisplayName("Base Price")]
        [Index(3)]
        [ModelDefault("DisplayFormat", "c2")]
        [ModelDefault("EditMask", "c2")]
        public decimal BasePrice
        {
            get => basePrice;
            set => SetPropertyValue(nameof(BasePrice), ref basePrice, value);
        }

        bool isInventoryTracked = true;
        /// <summary>
        /// Whether the item is tracked in inventory
        /// </summary>
        [XafDisplayName("Inventory Tracked")]
        [Index(4)]
        public bool IsInventoryTracked
        {
            get => isInventoryTracked;
            set => SetPropertyValue(nameof(IsInventoryTracked), ref isInventoryTracked, value);
        }

        string unitOfMeasure = string.Empty;
        /// <summary>
        /// Unit of measure for the item
        /// </summary>
        [RuleRequiredField("InventoryItem_UnitOfMeasure_Required", DefaultContexts.Save)]
        [Size(20)]
        [XafDisplayName("Unit of Measure")]
        [Index(5)]
        public string UnitOfMeasure
        {
            get => unitOfMeasure;
            set => SetPropertyValue(nameof(UnitOfMeasure), ref unitOfMeasure, value);
        }

        decimal reorderPoint = 0m;
        /// <summary>
        /// Minimum stock level before reordering
        /// </summary>
        [XafDisplayName("Reorder Point")]
        [Index(6)]
        public decimal ReorderPoint
        {
            get => reorderPoint;
            set => SetPropertyValue(nameof(ReorderPoint), ref reorderPoint, value);
        }

        decimal reorderQuantity = 0m;
        /// <summary>
        /// Default quantity to order when reordering
        /// </summary>
        [XafDisplayName("Reorder Quantity")]
        [Index(7)]
        public decimal ReorderQuantity
        {
            get => reorderQuantity;
            set => SetPropertyValue(nameof(ReorderQuantity), ref reorderQuantity, value);
        }

        decimal averageCost = 0m;
        /// <summary>
        /// Moving average cost calculated from transactions
        /// </summary>
        [XafDisplayName("Average Cost")]
        [Index(8)]
        [ModelDefault("DisplayFormat", "c4")]
        [ModelDefault("EditMask", "c4")]
        [ModelDefault("AllowEdit", "False")]
        public decimal AverageCost
        {
            get => averageCost;
            set => SetPropertyValue(nameof(AverageCost), ref averageCost, value);
        }

        string location = string.Empty;
        /// <summary>
        /// Default warehouse location
        /// </summary>
        [Size(50)]
        [XafDisplayName("Location")]
        [Index(9)]
        public string Location
        {
            get => location;
            set => SetPropertyValue(nameof(Location), ref location, value);
        }

        InventoryValuationMethod valuationMethod = InventoryValuationMethod.WeightedAverage;
        /// <summary>
        /// Inventory valuation method (AverageCost, FIFO, LIFO)
        /// </summary>
        [XafDisplayName("Valuation Method")]
        [Index(10)]
        public InventoryValuationMethod ValuationMethod
        {
            get => valuationMethod;
            set => SetPropertyValue(nameof(ValuationMethod), ref valuationMethod, value);
        }

        // Additional XAF-specific properties
        string category = string.Empty;
        /// <summary>
        /// Item category for classification
        /// </summary>
        [Size(100)]
        [XafDisplayName("Category")]
        [Index(11)]
        public string Category
        {
            get => category;
            set => SetPropertyValue(nameof(Category), ref category, value);
        }

        decimal standardCost = 0m;
        /// <summary>
        /// Standard cost for inventory valuation
        /// </summary>
        [XafDisplayName("Standard Cost")]
        [Index(12)]
        [ModelDefault("DisplayFormat", "c4")]
        [ModelDefault("EditMask", "c4")]
        public decimal StandardCost
        {
            get => standardCost;
            set => SetPropertyValue(nameof(StandardCost), ref standardCost, value);
        }

        bool isActive = true;
        /// <summary>
        /// Whether the item is active for transactions
        /// </summary>
        [XafDisplayName("Active")]
        [Index(13)]
        public bool IsActive
        {
            get => isActive;
            set => SetPropertyValue(nameof(IsActive), ref isActive, value);
        }

        bool isStockable = true;
        /// <summary>
        /// Whether the item maintains inventory levels
        /// </summary>
        [XafDisplayName("Stockable")]
        [Index(14)]
        public bool IsStockable
        {
            get => isStockable;
            set => SetPropertyValue(nameof(IsStockable), ref isStockable, value);
        }

        bool isPurchasable = true;
        /// <summary>
        /// Whether the item can be purchased
        /// </summary>
        [XafDisplayName("Purchasable")]
        [Index(15)]
        public bool IsPurchasable
        {
            get => isPurchasable;
            set => SetPropertyValue(nameof(IsPurchasable), ref isPurchasable, value);
        }

        bool isSellable = true;
        /// <summary>
        /// Whether the item can be sold
        /// </summary>
        [XafDisplayName("Sellable")]
        [Index(16)]
        public bool IsSellable
        {
            get => isSellable;
            set => SetPropertyValue(nameof(IsSellable), ref isSellable, value);
        }

        bool isLotTracked = false;
        /// <summary>
        /// Whether the item requires lot/batch tracking
        /// </summary>
        [XafDisplayName("Lot Tracked")]
        [Index(17)]
        public bool IsLotTracked
        {
            get => isLotTracked;
            set => SetPropertyValue(nameof(IsLotTracked), ref isLotTracked, value);
        }

        bool isSerialTracked = false;
        /// <summary>
        /// Whether the item requires serial number tracking
        /// </summary>
        [XafDisplayName("Serial Tracked")]
        [Index(18)]
        public bool IsSerialTracked
        {
            get => isSerialTracked;
            set => SetPropertyValue(nameof(IsSerialTracked), ref isSerialTracked, value);
        }

        int lotSize = 1;
        /// <summary>
        /// Default lot size for lot-tracked items
        /// </summary>
        [XafDisplayName("Lot Size")]
        [Index(19)]
        [RuleRange("InventoryItem_LotSize_Range", DefaultContexts.Save, 1, int.MaxValue)]
        public int LotSize
        {
            get => lotSize;
            set => SetPropertyValue(nameof(LotSize), ref lotSize, value);
        }

        int serialNumberLength = 0;
        /// <summary>
        /// Required length for serial numbers
        /// </summary>
        [XafDisplayName("Serial Number Length")]
        [Index(20)]
        [RuleRange("InventoryItem_SerialLength_Range", DefaultContexts.Save, 0, 50)]
        public int SerialNumberLength
        {
            get => serialNumberLength;
            set => SetPropertyValue(nameof(SerialNumberLength), ref serialNumberLength, value);
        }

        string notes = string.Empty;
        /// <summary>
        /// Additional notes about the item
        /// </summary>
        [Size(SizeAttribute.Unlimited)]
        [XafDisplayName("Notes")]
        [Index(21)]
        public string Notes
        {
            get => notes;
            set => SetPropertyValue(nameof(Notes), ref notes, value);
        }

        // Navigation properties for relationships
        [Association("InventoryItem-StockLevels")]
        [XafDisplayName("Stock Levels")]
        public XPCollection<StockLevel> StockLevels => GetCollection<StockLevel>(nameof(StockLevels));

        [Association("InventoryItem-InventoryTransactions")]
        [XafDisplayName("Transactions")]
        public XPCollection<InventoryTransaction> Transactions => GetCollection<InventoryTransaction>(nameof(Transactions));

        [Association("InventoryItem-InventoryReservations")]
        [XafDisplayName("Reservations")]
        public XPCollection<InventoryReservation> Reservations => GetCollection<InventoryReservation>(nameof(Reservations));

        [Association("InventoryItem-InventoryLayers")]
        [XafDisplayName("Inventory Layers")]
        public XPCollection<InventoryLayer> InventoryLayers => GetCollection<InventoryLayer>(nameof(InventoryLayers));

        // Business rule validation
        [RuleFromBoolProperty("InventoryItem_StandardCost_Required_When_StandardCosting", DefaultContexts.Save,
            "Standard cost is required when using Standard costing method")]
        public bool IsStandardCostValid => ValuationMethod != InventoryValuationMethod.StandardCost || StandardCost > 0;

        [RuleFromBoolProperty("InventoryItem_LotSize_Required_When_LotTracked", DefaultContexts.Save,
            "Lot size must be greater than 0 when item is lot tracked")]
        public bool IsLotSizeValid => !IsLotTracked || LotSize > 0;

        [RuleFromBoolProperty("InventoryItem_SerialLength_Required_When_SerialTracked", DefaultContexts.Save,
            "Serial number length must be greater than 0 when item is serial tracked")]
        public bool IsSerialLengthValid => !IsSerialTracked || SerialNumberLength > 0;

        [RuleFromBoolProperty("InventoryItem_BasePrice_Required_When_Sellable", DefaultContexts.Save,
            "Base price is required when item is sellable")]
        public bool IsBasePriceValid => !IsSellable || BasePrice > 0;
    }
}