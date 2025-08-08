using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using Sivar.Erp.Documents;
using Sivar.Erp.Services.Taxes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Sivar.Erp.Xaf.Module.BusinessObjects.Documents
{
    /// <summary>
    /// XAF persistent implementation of IDocumentLine
    /// Represents line items within documents
    /// </summary>
    [DefaultClassOptions]
    [XafDisplayName("Document Line")]
    [XafDefaultProperty(nameof(Description))]
    public class DocumentLine : ErpBaseObject, IDocumentLine
    {
        public DocumentLine(Session session) : base(session) { }

        double lineNumber = 0;
        /// <summary>
        /// Line number for ordering within the document
        /// </summary>
        [XafDisplayName("Line Number")]
        [Index(0)]
        public double LineNumber
        {
            get => lineNumber;
            set => SetPropertyValue(nameof(LineNumber), ref lineNumber, value);
        }

        string description = string.Empty;
        /// <summary>
        /// Description of the line item
        /// </summary>
        [RuleRequiredField("DocumentLine_Description_Required", DefaultContexts.Save)]
        [Size(500)]
        [XafDisplayName("Description")]
        [Index(1)]
        public string Description
        {
            get => description;
            set => SetPropertyValue(nameof(Description), ref description, value);
        }

        Item item;
        /// <summary>
        /// Item associated with this line
        /// </summary>
        [XafDisplayName("Item")]
        [Index(2)]
        public Item Item
        {
            get => item;
            set => SetPropertyValue(nameof(Item), ref item, value);
        }

        decimal quantity = 1m;
        /// <summary>
        /// Quantity of the line item
        /// </summary>
        [RuleRange("DocumentLine_Quantity_Range", DefaultContexts.Save, 0.0001, double.MaxValue,
            "Quantity must be greater than zero")]
        [XafDisplayName("Quantity")]
        [Index(3)]
        [ModelDefault("DisplayFormat", "n4")]
        [ModelDefault("EditMask", "n4")]
        public decimal Quantity
        {
            get => quantity;
            set
            {
                SetPropertyValue(nameof(Quantity), ref quantity, value);
                RecalculateAmount();
            }
        }

        decimal unitPrice = 0m;
        /// <summary>
        /// Unit price of the line item
        /// </summary>
        [XafDisplayName("Unit Price")]
        [Index(4)]
        [ModelDefault("DisplayFormat", "c2")]
        [ModelDefault("EditMask", "c2")]
        public decimal UnitPrice
        {
            get => unitPrice;
            set
            {
                SetPropertyValue(nameof(UnitPrice), ref unitPrice, value);
                RecalculateAmount();
            }
        }

        decimal amount = 0m;
        /// <summary>
        /// Total amount for this line (Quantity × Unit Price)
        /// </summary>
        [XafDisplayName("Amount")]
        [Index(5)]
        [ModelDefault("DisplayFormat", "c2")]
        [ModelDefault("AllowEdit", "False")]
        public decimal Amount
        {
            get => amount;
            set => SetPropertyValue(nameof(Amount), ref amount, value);
        }

        Document document;
        /// <summary>
        /// Parent document that contains this line
        /// </summary>
        [Association("Document-Lines")]
        [XafDisplayName("Document")]
        [Browsable(false)]
        public Document Document
        {
            get => document;
            set => SetPropertyValue(nameof(Document), ref document, value);
        }

        /// <summary>
        /// Collection of totals for this line (taxes, discounts, etc.)
        /// </summary>
        [Association("DocumentLine-Totals")]
        [XafDisplayName("Line Totals")]
        public XPCollection<Total> LineTotals => GetCollection<Total>(nameof(LineTotals));

        /// <summary>
        /// Collection of taxes that apply to this line
        /// </summary>
        [Association("DocumentLine-Taxes")]
        [XafDisplayName("Taxes")]
        public XPCollection<Taxes.Tax> Taxes => GetCollection<Taxes.Tax>(nameof(Taxes));

        /// <summary>
        /// Returns a string representation of the document line
        /// </summary>
        public override string ToString()
        {
            return $"Line {LineNumber}: {Description} - {Amount:C}";
        }

        /// <summary>
        /// Recalculates the line amount when quantity or unit price changes
        /// </summary>
        private void RecalculateAmount()
        {
            Amount = Quantity * UnitPrice;
        }

        /// <summary>
        /// Updates description and unit price when item is changed
        /// </summary>
        protected override void OnChanged(string propertyName, object oldValue, object newValue)
        {
            base.OnChanged(propertyName, oldValue, newValue);
            
            if (propertyName == nameof(Item) && Item != null)
            {
                if (string.IsNullOrWhiteSpace(Description))
                {
                    Description = Item.Description;
                }
                if (UnitPrice == 0)
                {
                    UnitPrice = Item.BasePrice;
                }
            }
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            Quantity = 1m;
            LineNumber = 10; // Default line number increment
        }

        // Interface implementations for IDocumentLine
        IItem IDocumentLine.Item
        {
            get => Item;
            set => Item = value as Item;
        }

        IList<ITotal> IDocumentLine.LineTotals
        {
            get => LineTotals.Cast<ITotal>().ToList();
            set => throw new NotSupportedException("Use XPO collection LineTotals");
        }

        IList<TaxDto> IDocumentLine.Taxes
        {
            get => Taxes.Select(t => new TaxDto
            {
                Oid = t.Oid,
                Name = t.Name,
                Code = t.Code,
                TaxType = t.TaxType,
                ApplicationLevel = t.ApplicationLevel,
                Amount = t.Amount,
                Percentage = t.Percentage,
                IsEnabled = t.IsEnabled,
                IsIncludedInPrice = t.IsIncludedInPrice
            }).Cast<TaxDto>().ToList();
            set => throw new NotSupportedException("Use XPO collection Taxes");
        }
    }
}