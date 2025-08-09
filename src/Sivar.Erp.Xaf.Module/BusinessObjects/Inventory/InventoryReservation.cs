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
    /// XAF persistent implementation of IInventoryReservation
    /// Represents reserved inventory quantities for specific documents
    /// </summary>
    [DefaultClassOptions]
    [NavigationItem("Inventory")]
    [DefaultProperty(nameof(DisplayName))]
    [XafDisplayName("Inventory Reservation")]
    [XafDefaultProperty(nameof(DisplayName))]
    [Appearance("Highlight_Active", AppearanceItemType = "ViewItem",
        Criteria = "Status = 'Active'",
        BackColor = "200, 255, 200", FontColor = "0, 100, 0")]
    [Appearance("Highlight_Expired", AppearanceItemType = "ViewItem",
        Criteria = "Status = 'Expired' OR (Status = 'Active' AND ExpiresAt < Now())",
        BackColor = "255, 200, 200", FontColor = "128, 0, 0")]
    [Appearance("Highlight_Fulfilled", AppearanceItemType = "ViewItem",
        Criteria = "Status = 'Fulfilled'",
        BackColor = "200, 200, 255", FontColor = "0, 0, 128")]
    [Appearance("ReadOnly_When_NotActive", AppearanceItemType = "ViewItem",
        Criteria = "Status <> 'Active'",
        TargetItems = "Quantity;WarehouseCode;ExpiresAt",
        Enabled = false)]
    public class InventoryReservation : ErpBaseObject, IInventoryReservation
    {
        public InventoryReservation(Session session) : base(session) { }

        string reservationId = string.Empty;
        /// <summary>
        /// Unique reservation identifier
        /// </summary>
        [RuleRequiredField("InventoryReservation_ReservationId_Required", DefaultContexts.Save)]
        [RuleUniqueValue("InventoryReservation_ReservationId_Unique", DefaultContexts.Save)]
        [Size(50)]
        [XafDisplayName("Reservation ID")]
        [Index(0)]
        public string ReservationId
        {
            get => reservationId;
            set => SetPropertyValue(nameof(ReservationId), ref reservationId, value);
        }

        InventoryItem item;
        /// <summary>
        /// The inventory item being reserved
        /// </summary>
        [Association("InventoryItem-InventoryReservations")]
        [RuleRequiredField("InventoryReservation_Item_Required", DefaultContexts.Save)]
        [XafDisplayName("Item")]
        [Index(1)]
        public InventoryItem Item
        {
            get => item;
            set => SetPropertyValue(nameof(Item), ref item, value);
        }

        decimal? quantity;
        /// <summary>
        /// Quantity being reserved
        /// </summary>
        [RuleRequiredField("InventoryReservation_Quantity_Required", DefaultContexts.Save)]
        [XafDisplayName("Quantity")]
        [Index(2)]
        [ModelDefault("DisplayFormat", "n2")]
        [ModelDefault("EditMask", "n2")]
        public decimal? Quantity
        {
            get => quantity;
            set => SetPropertyValue(nameof(Quantity), ref quantity, value);
        }

        string warehouseCode = string.Empty;
        /// <summary>
        /// Warehouse code where the item is reserved
        /// </summary>
        [RuleRequiredField("InventoryReservation_WarehouseCode_Required", DefaultContexts.Save)]
        [Size(50)]
        [XafDisplayName("Warehouse")]
        [Index(3)]
        public string WarehouseCode
        {
            get => warehouseCode;
            set => SetPropertyValue(nameof(WarehouseCode), ref warehouseCode, value?.ToUpperInvariant());
        }

        string sourceDocumentNumber = string.Empty;
        /// <summary>
        /// Source document number (e.g., sales order number)
        /// </summary>
        [RuleRequiredField("InventoryReservation_SourceDocument_Required", DefaultContexts.Save)]
        [Size(100)]
        [XafDisplayName("Source Document")]
        [Index(4)]
        public string SourceDocumentNumber
        {
            get => sourceDocumentNumber;
            set => SetPropertyValue(nameof(SourceDocumentNumber), ref sourceDocumentNumber, value);
        }

        ReservationStatus status = ReservationStatus.Active;
        /// <summary>
        /// Current status of the reservation
        /// </summary>
        [XafDisplayName("Status")]
        [Index(5)]
        public ReservationStatus Status
        {
            get => status;
            set => SetPropertyValue(nameof(Status), ref status, value);
        }

        DateTime createdAt = DateTime.UtcNow;
        /// <summary>
        /// When the reservation was created
        /// </summary>
        [XafDisplayName("Created At")]
        [Index(6)]
        [ModelDefault("AllowEdit", "False")]
        public DateTime CreatedAt
        {
            get => createdAt;
            set => SetPropertyValue(nameof(CreatedAt), ref createdAt, value);
        }

        DateTime expiresAt = DateTime.UtcNow.AddDays(1);
        /// <summary>
        /// When the reservation expires
        /// </summary>
        [XafDisplayName("Expires At")]
        [Index(7)]
        public DateTime ExpiresAt
        {
            get => expiresAt;
            set => SetPropertyValue(nameof(ExpiresAt), ref expiresAt, value);
        }

        DateTime lastUpdated = DateTime.UtcNow;
        /// <summary>
        /// When the reservation was last updated
        /// </summary>
        [XafDisplayName("Last Updated")]
        [Index(8)]
        [ModelDefault("AllowEdit", "False")]
        public DateTime LastUpdated
        {
            get => lastUpdated;
            set => SetPropertyValue(nameof(LastUpdated), ref lastUpdated, value);
        }

        string notes = string.Empty;
        /// <summary>
        /// Additional notes about the reservation
        /// </summary>
        [Size(SizeAttribute.Unlimited)]
        [XafDisplayName("Notes")]
        [Index(9)]
        public string Notes
        {
            get => notes;
            set => SetPropertyValue(nameof(Notes), ref notes, value);
        }

        StockLevel stockLevel;
        /// <summary>
        /// Associated stock level record
        /// </summary>
        [Association("StockLevel-InventoryReservations")]
        [XafDisplayName("Stock Level")]
        [Index(10)]
        public StockLevel StockLevel
        {
            get => stockLevel;
            set => SetPropertyValue(nameof(StockLevel), ref stockLevel, value);
        }

        // Interface implementation - explicit for IInventoryReservation.Item
        Sivar.Erp.Documents.IInventoryItem IInventoryReservation.Item 
        { 
            get => Item; 
            set => Item = value as InventoryItem; 
        }

        // Interface implementation - non-nullable quantity for interface
        decimal IInventoryReservation.Quantity
        {
            get => Quantity ?? 0m;
            set => Quantity = value;
        }

        /// <summary>
        /// Whether the reservation is expired based on current time
        /// </summary>
        [PersistentAlias("ExpiresAt < Now()")]
        [XafDisplayName("Is Expired")]
        [Index(11)]
        [ModelDefault("AllowEdit", "False")]
        public bool IsExpired => DateTime.UtcNow > ExpiresAt;

        /// <summary>
        /// Display name for the reservation
        /// </summary>
        [PersistentAlias("Concat(Item.Code, ' - ', SourceDocumentNumber)")]
        [XafDisplayName("Display Name")]
        [ModelDefault("AllowEdit", "False")]
        public string DisplayName => $"{Item?.Code} - {SourceDocumentNumber}";

        /// <summary>
        /// Time remaining until expiration
        /// </summary>
        [PersistentAlias("ExpiresAt - Now()")]
        [XafDisplayName("Time Remaining")]
        [Index(12)]
        [ModelDefault("AllowEdit", "False")]
        public TimeSpan TimeRemaining => ExpiresAt > DateTime.UtcNow ? ExpiresAt - DateTime.UtcNow : TimeSpan.Zero;

        /// <summary>
        /// Whether the reservation can be modified
        /// </summary>
        [PersistentAlias("Status = 'Active' AND ExpiresAt > Now()")]
        [XafDisplayName("Can Modify")]
        [ModelDefault("AllowEdit", "False")]
        public bool CanModify => Status == ReservationStatus.Active && !IsExpired;

        // Business rule validations
        [RuleFromBoolProperty("InventoryReservation_Quantity_Positive", DefaultContexts.Save,
            "Reservation quantity must be greater than zero")]
        public bool IsQuantityValid => Quantity.HasValue && Quantity.Value > 0;

        [RuleFromBoolProperty("InventoryReservation_ExpiresAt_Future", DefaultContexts.Save,
            "Expiration date must be in the future for new reservations")]
        public bool IsExpirationValid => Session.IsNewObject(this) == false || ExpiresAt > CreatedOn;

        [RuleFromBoolProperty("InventoryReservation_Item_Active", DefaultContexts.Save,
            "Cannot reserve inactive items")]
        public bool IsItemActive => Item == null || Item.IsActive;

        [RuleFromBoolProperty("InventoryReservation_Item_Stockable", DefaultContexts.Save,
            "Cannot reserve non-stockable items")]
        public bool IsItemStockable => Item == null || Item.IsStockable;

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            ReservationId = Guid.NewGuid().ToString();
            CreatedAt = DateTime.UtcNow;
            ExpiresAt = DateTime.UtcNow.AddDays(1); // Default 24 hour expiration
            LastUpdated = DateTime.UtcNow;
            Status = ReservationStatus.Active;
        }

        protected override void OnSaving()
        {
            base.OnSaving();
            LastUpdated = DateTime.UtcNow;

            // Auto-expire if past expiration date
            if (Status == ReservationStatus.Active && IsExpired)
            {
                Status = ReservationStatus.Expired;
            }
        }
    }
}