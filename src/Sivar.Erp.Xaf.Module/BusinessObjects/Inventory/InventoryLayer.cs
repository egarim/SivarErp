using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System;
using System.ComponentModel;

namespace Sivar.Erp.Xaf.Module.BusinessObjects.Inventory
{
    /// <summary>
    /// XAF persistent implementation for inventory layers used in FIFO/LIFO costing
    /// Represents cost layers for inventory items with remaining quantities
    /// </summary>
    [DefaultClassOptions]
    [NavigationItem("Inventory")]
    [DefaultProperty(nameof(DisplayName))]
    [XafDisplayName("Inventory Layer")]
    [XafDefaultProperty(nameof(DisplayName))]
    public class InventoryLayer : ErpBaseObject
    {
        public InventoryLayer(Session session) : base(session) { }

        InventoryItem item;
        /// <summary>
        /// The inventory item this layer represents
        /// </summary>
        [Association("InventoryItem-InventoryLayers")]
        [RuleRequiredField("InventoryLayer_Item_Required", DefaultContexts.Save)]
        [XafDisplayName("Item")]
        [Index(0)]
        public InventoryItem Item
        {
            get => item;
            set => SetPropertyValue(nameof(Item), ref item, value);
        }

        string warehouseCode = string.Empty;
        /// <summary>
        /// Warehouse code where the layer is located
        /// </summary>
        [RuleRequiredField("InventoryLayer_WarehouseCode_Required", DefaultContexts.Save)]
        [Size(50)]
        [XafDisplayName("Warehouse")]
        [Index(1)]
        public string WarehouseCode
        {
            get => warehouseCode;
            set => SetPropertyValue(nameof(WarehouseCode), ref warehouseCode, value?.ToUpperInvariant());
        }

        decimal? originalQuantity;
        /// <summary>
        /// Original quantity when the layer was created
        /// </summary>
        [RuleRequiredField("InventoryLayer_OriginalQuantity_Required", DefaultContexts.Save)]
        [XafDisplayName("Original Quantity")]
        [Index(2)]
        [ModelDefault("DisplayFormat", "n2")]
        [ModelDefault("EditMask", "n2")]
        [ModelDefault("AllowEdit", "False")]
        public decimal? OriginalQuantity
        {
            get => originalQuantity;
            set => SetPropertyValue(nameof(OriginalQuantity), ref originalQuantity, value);
        }

        decimal remainingQuantity = 0m;
        /// <summary>
        /// Remaining quantity in the layer
        /// </summary>
        [XafDisplayName("Remaining Quantity")]
        [Index(3)]
        [ModelDefault("DisplayFormat", "n2")]
        [ModelDefault("EditMask", "n2")]
        public decimal RemainingQuantity
        {
            get => remainingQuantity;
            set => SetPropertyValue(nameof(RemainingQuantity), ref remainingQuantity, value);
        }

        decimal unitCost = 0m;
        /// <summary>
        /// Unit cost for this layer
        /// </summary>
        [XafDisplayName("Unit Cost")]
        [Index(4)]
        [ModelDefault("DisplayFormat", "c4")]
        [ModelDefault("EditMask", "c4")]
        public decimal UnitCost
        {
            get => unitCost;
            set => SetPropertyValue(nameof(UnitCost), ref unitCost, value);
        }

        string sourceTransactionId = string.Empty;
        /// <summary>
        /// Source transaction that created this layer
        /// </summary>
        [Size(50)]
        [XafDisplayName("Source Transaction")]
        [Index(5)]
        [ModelDefault("AllowEdit", "False")]
        public string SourceTransactionId
        {
            get => sourceTransactionId;
            set => SetPropertyValue(nameof(SourceTransactionId), ref sourceTransactionId, value);
        }

        DateTime layerDate = DateTime.UtcNow;
        /// <summary>
        /// Date when the layer was created
        /// </summary>
        [XafDisplayName("Layer Date")]
        [Index(6)]
        [ModelDefault("AllowEdit", "False")]
        public DateTime LayerDate
        {
            get => layerDate;
            set => SetPropertyValue(nameof(LayerDate), ref layerDate, value);
        }

        string lotNumber = string.Empty;
        /// <summary>
        /// Lot/batch number if lot tracked
        /// </summary>
        [Size(50)]
        [XafDisplayName("Lot Number")]
        [Index(7)]
        public string LotNumber
        {
            get => lotNumber;
            set => SetPropertyValue(nameof(LotNumber), ref lotNumber, value);
        }

        DateTime? expirationDate;
        /// <summary>
        /// Expiration date for the lot (if applicable)
        /// </summary>
        [XafDisplayName("Expiration Date")]
        [Index(8)]
        public DateTime? ExpirationDate
        {
            get => expirationDate;
            set => SetPropertyValue(nameof(ExpirationDate), ref expirationDate, value);
        }

        string notes = string.Empty;
        /// <summary>
        /// Additional notes about the layer
        /// </summary>
        [Size(SizeAttribute.Unlimited)]
        [XafDisplayName("Notes")]
        [Index(9)]
        public string Notes
        {
            get => notes;
            set => SetPropertyValue(nameof(Notes), ref notes, value);
        }

        /// <summary>
        /// Total value of remaining quantity in this layer
        /// </summary>
        [PersistentAlias("RemainingQuantity * UnitCost")]
        [XafDisplayName("Remaining Value")]
        [Index(10)]
        [ModelDefault("DisplayFormat", "c2")]
        [ModelDefault("AllowEdit", "False")]
        public decimal RemainingValue => RemainingQuantity * UnitCost;

        /// <summary>
        /// Consumed quantity from this layer
        /// </summary>
        [PersistentAlias("(OriginalQuantity ?? 0) - RemainingQuantity")]
        [XafDisplayName("Consumed Quantity")]
        [Index(11)]
        [ModelDefault("DisplayFormat", "n2")]
        [ModelDefault("AllowEdit", "False")]
        public decimal ConsumedQuantity => (OriginalQuantity ?? 0m) - RemainingQuantity;

        /// <summary>
        /// Display name for the layer
        /// </summary>
        [PersistentAlias("Concat(Item.Code, ' - ', WarehouseCode, ' (', LayerDate, ')')")]
        [XafDisplayName("Display Name")]
        [ModelDefault("AllowEdit", "False")]
        public string DisplayName => $"{Item?.Code} - {WarehouseCode} ({LayerDate:yyyy-MM-dd})";

        /// <summary>
        /// Whether the layer is fully consumed
        /// </summary>
        [PersistentAlias("RemainingQuantity <= 0")]
        [XafDisplayName("Is Consumed")]
        [ModelDefault("AllowEdit", "False")]
        public bool IsConsumed => RemainingQuantity <= 0;

        /// <summary>
        /// Whether the lot is expired (if applicable)
        /// </summary>
        [PersistentAlias("ExpirationDate.HasValue AND ExpirationDate < Now()")]
        [XafDisplayName("Is Expired")]
        [ModelDefault("AllowEdit", "False")]
        public bool IsExpired => ExpirationDate.HasValue && ExpirationDate < DateTime.UtcNow;

        // Business rule validations
        [RuleFromBoolProperty("InventoryLayer_OriginalQuantity_Positive", DefaultContexts.Save,
            "Original quantity must be greater than zero")]
        public bool IsOriginalQuantityValid => OriginalQuantity.HasValue && OriginalQuantity.Value > 0;

        [RuleFromBoolProperty("InventoryLayer_RemainingQuantity_NotExceedOriginal", DefaultContexts.Save,
            "Remaining quantity cannot exceed original quantity")]
        public bool IsRemainingQuantityValid => RemainingQuantity <= (OriginalQuantity ?? 0m);

        [RuleFromBoolProperty("InventoryLayer_RemainingQuantity_NotNegative", DefaultContexts.Save,
            "Remaining quantity cannot be negative")]
        public bool IsRemainingQuantityNotNegative => RemainingQuantity >= 0;

        [RuleFromBoolProperty("InventoryLayer_UnitCost_NotNegative", DefaultContexts.Save,
            "Unit cost cannot be negative")]
        public bool IsUnitCostValid => UnitCost >= 0;

        [RuleFromBoolProperty("InventoryLayer_LotNumber_Required_When_LotTracked", DefaultContexts.Save,
            "Lot number is required when item is lot tracked")]
        public bool IsLotNumberValid => Item == null || !Item.IsLotTracked || !string.IsNullOrEmpty(LotNumber);

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            LayerDate = DateTime.UtcNow;
        }

        protected override void OnSaving()
        {
            base.OnSaving();
            
            // Set remaining quantity to original quantity if not set (new layer)
            if (Session.IsNewObject(this) && RemainingQuantity == 0 && OriginalQuantity.HasValue && OriginalQuantity.Value > 0)
            {
                RemainingQuantity = OriginalQuantity.Value;
            }
        }
    }
}