using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using Sivar.Erp.Services.Taxes;
using System;
using System.ComponentModel;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Editors; // Add this line for ViewItemVisibility

namespace Sivar.Erp.Xaf.Module.BusinessObjects.Taxes
{
    /// <summary>
    /// XAF persistent implementation of ITax
    /// Represents tax definitions that can be applied to documents
    /// </summary>
    [DefaultClassOptions]
    [NavigationItem("Configuration")]
    [DefaultProperty(nameof(Name))]
    [XafDisplayName("Tax")]
    [XafDefaultProperty(nameof(Name))]
    public class Tax : ErpBaseObject, ITax
    {
        public Tax(Session session) : base(session) { }

        string name = string.Empty;
        /// <summary>
        /// Display name of the tax
        /// </summary>
        [RuleRequiredField("Tax_Name_Required", DefaultContexts.Save)]
        [Size(100)]
        [XafDisplayName("Name")]
        [Index(0)]
        public string Name
        {
            get => name;
            set => SetPropertyValue(nameof(Name), ref name, value);
        }

        string code = string.Empty;
        /// <summary>
        /// Short code for the tax (e.g., VAT, GST, IVA)
        /// </summary>
        [RuleRequiredField("Tax_Code_Required", DefaultContexts.Save)]
        [RuleUniqueValue("Tax_Code_Unique", DefaultContexts.Save)]
        [Size(20)]
        [XafDisplayName("Code")]
        [Index(1)]
        public string Code
        {
            get => code;
            set => SetPropertyValue(nameof(Code), ref code, value?.ToUpperInvariant());
        }

        TaxType taxType = TaxType.Percentage;
        /// <summary>
        /// Type of tax calculation (by percentage, fixed amount, or per quantity)
        /// </summary>
        [XafDisplayName("Tax Type")]
        [Index(2)]
        public TaxType TaxType
        {
            get => taxType;
            set => SetPropertyValue(nameof(TaxType), ref taxType, value);
        }

        TaxApplicationLevel applicationLevel = TaxApplicationLevel.Line;
        /// <summary>
        /// Level at which the tax should be applied (line or document)
        /// </summary>
        [XafDisplayName("Application Level")]
        [Index(3)]
        public TaxApplicationLevel ApplicationLevel
        {
            get => applicationLevel;
            set => SetPropertyValue(nameof(ApplicationLevel), ref applicationLevel, value);
        }

        decimal percentage = 0m;
        /// <summary>
        /// Percentage to apply when TaxType is Percentage
        /// </summary>
        [XafDisplayName("Percentage")]
        [Index(4)]
        [ModelDefault("DisplayFormat", "p2")]
        [ModelDefault("EditMask", "p2")]
        [Appearance("Tax_Percentage_Visible", AppearanceItemType.LayoutItem, "TaxType = 'Percentage'", Visibility = ViewItemVisibility.Show)]
        [Appearance("Tax_Percentage_Hidden", AppearanceItemType.LayoutItem, "TaxType != 'Percentage'", Visibility = ViewItemVisibility.Hide)]
        public decimal Percentage
        {
            get => percentage;
            set => SetPropertyValue(nameof(Percentage), ref percentage, value);
        }

        decimal amount = 0m;
        /// <summary>
        /// Fixed amount to apply when TaxType is FixedAmount or AmountPerUnit
        /// </summary>
        [XafDisplayName("Amount")]
        [Index(5)]
        [ModelDefault("DisplayFormat", "c2")]
        [ModelDefault("EditMask", "c2")]
        [Appearance("Tax_Amount_Visible", AppearanceItemType.LayoutItem, "TaxType = 'FixedAmount' Or TaxType = 'AmountPerUnit'", Visibility = ViewItemVisibility.Show)]
        [Appearance("Tax_Amount_Hidden", AppearanceItemType.LayoutItem, "TaxType = 'Percentage'", Visibility = ViewItemVisibility.Hide)]
        public decimal Amount
        {
            get => amount;
            set => SetPropertyValue(nameof(Amount), ref amount, value);
        }

        bool isEnabled = true;
        /// <summary>
        /// Whether this tax is currently active
        /// </summary>
        [XafDisplayName("Enabled")]
        [Index(6)]
        public bool IsEnabled
        {
            get => isEnabled;
            set => SetPropertyValue(nameof(IsEnabled), ref isEnabled, value);
        }

        bool isIncludedInPrice = false;
        /// <summary>
        /// Whether the tax is already included in the line price
        /// </summary>
        [XafDisplayName("Included in Price")]
        [Index(7)]
        public bool IsIncludedInPrice
        {
            get => isIncludedInPrice;
            set => SetPropertyValue(nameof(IsIncludedInPrice), ref isIncludedInPrice, value);
        }

        string description = string.Empty;
        /// <summary>
        /// Optional description of the tax
        /// </summary>
        [Size(500)]
        [XafDisplayName("Description")]
        [Index(8)]
        public string Description
        {
            get => description;
            set => SetPropertyValue(nameof(Description), ref description, value);
        }

        /// <summary>
        /// Document lines that use this tax
        /// </summary>
        [Association("DocumentLine-Taxes")]
        [XafDisplayName("Document Lines")]
        public XPCollection<Documents.DocumentLine> DocumentLines => GetCollection<Documents.DocumentLine>(nameof(DocumentLines));

        /// <summary>
        /// Returns a string representation of the tax
        /// </summary>
        public override string ToString()
        {
            return $"{Code} - {Name}";
        }

        /// <summary>
        /// Validation to ensure percentage is provided for percentage-based taxes
        /// </summary>
        [RuleFromBoolProperty("Tax_Percentage_Required", DefaultContexts.Save,
            "Percentage must be greater than zero for percentage-based taxes")]
        public bool IsPercentageValid
        {
            get => TaxType != TaxType.Percentage || Percentage > 0;
        }

        /// <summary>
        /// Validation to ensure amount is provided for fixed amount taxes
        /// </summary>
        [RuleFromBoolProperty("Tax_Amount_Required", DefaultContexts.Save,
            "Amount must be greater than zero for fixed amount taxes")]
        public bool IsAmountValid
        {
            get => (TaxType != TaxType.FixedAmount && TaxType != TaxType.AmountPerUnit) || Amount > 0;
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            IsEnabled = true;
            IsIncludedInPrice = false;
            TaxType = TaxType.Percentage;
            ApplicationLevel = TaxApplicationLevel.Line;
        }

        // Property required for ITax interface but not exposed in UI
        Guid ITax.Oid
        {
            get => Oid;
            set => throw new NotSupportedException("Use XPO's Oid property for persistence");
        }
    }
}