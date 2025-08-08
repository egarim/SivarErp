using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using Sivar.Erp.Documents;
using System.ComponentModel;

namespace Sivar.Erp.Xaf.Module.BusinessObjects.Documents
{
    /// <summary>
    /// XAF persistent implementation of IItem
    /// Represents items/products that can be used in documents
    /// </summary>
    [DefaultClassOptions]
    [NavigationItem("Master Data")]
    [DefaultProperty(nameof(Description))]
    [XafDisplayName("Item")]
    [XafDefaultProperty(nameof(Description))]
    public class Item : ErpBaseObject, IItem
    {
        public Item(Session session) : base(session) { }

        string code = string.Empty;
        /// <summary>
        /// Unique item code (e.g., SKU, Part Number)
        /// </summary>
        [RuleRequiredField("Item_Code_Required", DefaultContexts.Save)]
        [RuleUniqueValue("Item_Code_Unique", DefaultContexts.Save)]
        [Size(50)]
        [XafDisplayName("Code")]
        [Index(0)]
        public string Code
        {
            get => code;
            set => SetPropertyValue(nameof(Code), ref code, value?.ToUpperInvariant());
        }

        string type = string.Empty;
        /// <summary>
        /// Item type/category (e.g., Product, Service, Asset)
        /// </summary>
        [RuleRequiredField("Item_Type_Required", DefaultContexts.Save)]
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
        [RuleRequiredField("Item_Description_Required", DefaultContexts.Save)]
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

        string uom = string.Empty;
        /// <summary>
        /// Unit of measure
        /// </summary>
        [Size(20)]
        [XafDisplayName("Unit of Measure")]
        [Index(4)]
        public string UnitOfMeasure
        {
            get => uom;
            set => SetPropertyValue(nameof(UnitOfMeasure), ref uom, value);
        }

        bool isActive = true;
        /// <summary>
        /// Indicates whether this item is active
        /// </summary>
        [XafDisplayName("Active")]
        [Index(5)]
        public bool IsActive
        {
            get => isActive;
            set => SetPropertyValue(nameof(IsActive), ref isActive, value);
        }

        bool isSellable = true;
        /// <summary>
        /// Indicates whether this item can be sold
        /// </summary>
        [XafDisplayName("Sellable")]
        [Index(6)]
        public bool IsSellable
        {
            get => isSellable;
            set => SetPropertyValue(nameof(IsSellable), ref isSellable, value);
        }

        bool isPurchasable = true;
        /// <summary>
        /// Indicates whether this item can be purchased
        /// </summary>
        [XafDisplayName("Purchasable")]
        [Index(7)]
        public bool IsPurchasable
        {
            get => isPurchasable;
            set => SetPropertyValue(nameof(IsPurchasable), ref isPurchasable, value);
        }

        /// <summary>
        /// Returns a string representation of the item
        /// </summary>
        public override string ToString()
        {
            return $"{Code} - {Description}";
        }

        /// <summary>
        /// Validation to ensure base price is positive for sellable items
        /// </summary>
        [RuleFromBoolProperty("Item_BasePrice_Required", DefaultContexts.Save,
            "Base Price must be greater than zero for sellable items")]
        public bool IsBasePriceValid
        {
            get => !IsSellable || BasePrice > 0;
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            IsActive = true;
            IsSellable = true;
            IsPurchasable = true;
        }
    }
}