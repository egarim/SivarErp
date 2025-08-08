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
    /// XAF persistent implementation of ITotal
    /// Represents calculated totals for documents and lines
    /// </summary>
    [DefaultClassOptions]
    [XafDisplayName("Total")]
    [XafDefaultProperty(nameof(Concept))]
    public class Total : ErpBaseObject, ITotal
    {
        public Total(Session session) : base(session) { }

        string concept = string.Empty;
        /// <summary>
        /// Description of what this total represents (e.g., "Subtotal", "Tax", "Discount")
        /// </summary>
        [RuleRequiredField("Total_Concept_Required", DefaultContexts.Save)]
        [Size(100)]
        [XafDisplayName("Concept")]
        [Index(0)]
        public string Concept
        {
            get => concept;
            set => SetPropertyValue(nameof(Concept), ref concept, value);
        }

        decimal amount = 0m;
        /// <summary>
        /// The calculated total amount
        /// </summary>
        [XafDisplayName("Amount")]
        [Index(1)]
        [ModelDefault("DisplayFormat", "c2")]
        [ModelDefault("EditMask", "c2")]
        public decimal Amount
        {
            get => amount;
            set => SetPropertyValue(nameof(Amount), ref amount, value);
        }

        string debitAccountCode = string.Empty;
        /// <summary>
        /// Account code to post to when this total represents a debit amount
        /// </summary>
        [Size(50)]
        [XafDisplayName("Debit Account Code")]
        [Index(2)]
        public string DebitAccountCode
        {
            get => debitAccountCode;
            set => SetPropertyValue(nameof(DebitAccountCode), ref debitAccountCode, value);
        }

        string creditAccountCode = string.Empty;
        /// <summary>
        /// Account code to post to when this total represents a credit amount
        /// </summary>
        [Size(50)]
        [XafDisplayName("Credit Account Code")]
        [Index(3)]
        public string CreditAccountCode
        {
            get => creditAccountCode;
            set => SetPropertyValue(nameof(CreditAccountCode), ref creditAccountCode, value);
        }

        bool includeInTransaction = true;
        /// <summary>
        /// Indicates if this total should be included in transaction generation
        /// </summary>
        [XafDisplayName("Include in Transaction")]
        [Index(4)]
        public bool IncludeInTransaction
        {
            get => includeInTransaction;
            set => SetPropertyValue(nameof(IncludeInTransaction), ref includeInTransaction, value);
        }

        // Navigation properties for associations
        Document document;
        /// <summary>
        /// Document this total belongs to (if it's a document-level total)
        /// </summary>
        [Association("Document-Totals")]
        [XafDisplayName("Document")]
        [Browsable(false)]
        public Document Document
        {
            get => document;
            set => SetPropertyValue(nameof(Document), ref document, value);
        }

        DocumentLine documentLine;
        /// <summary>
        /// Document line this total belongs to (if it's a line-level total)
        /// </summary>
        [Association("DocumentLine-Totals")]
        [XafDisplayName("Document Line")]
        [Browsable(false)]
        public DocumentLine DocumentLine
        {
            get => documentLine;
            set => SetPropertyValue(nameof(DocumentLine), ref documentLine, value);
        }

        /// <summary>
        /// Returns a string representation of the total
        /// </summary>
        public override string ToString()
        {
            return $"{Concept}: {Amount:C}";
        }

        /// <summary>
        /// Validation to ensure at least one account code is provided when including in transaction
        /// </summary>
        [RuleFromBoolProperty("Total_AccountCode_Required", DefaultContexts.Save,
            "Either Debit or Credit Account Code must be provided when including in transaction")]
        public bool IsAccountCodeValid
        {
            get => !IncludeInTransaction || 
                   !string.IsNullOrWhiteSpace(DebitAccountCode) || 
                   !string.IsNullOrWhiteSpace(CreditAccountCode);
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            IncludeInTransaction = true;
        }

        // Property required for ITotal interface but not exposed in UI
        Guid ITotal.Oid
        {
            get => Oid;
            set => throw new NotSupportedException("Use XPO's Oid property for persistence");
        }

        // Interface implementation for Total property
        decimal ITotal.Total
        {
            get => Amount;
            set => Amount = value;
        }
    }
}