using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using Sivar.Erp.BusinessEntities;
using Sivar.Erp.Documents;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Sivar.Erp.Xaf.Module.BusinessObjects.Documents
{
    /// <summary>
    /// XAF persistent implementation of IDocument
    /// Represents business documents like invoices, purchase orders, etc.
    /// </summary>
    [DefaultClassOptions]
    [NavigationItem("Documents")]
    [DefaultProperty(nameof(DocumentNumber))]
    [XafDisplayName("Document")]
    [XafDefaultProperty(nameof(DocumentNumber))]
    public class Document : ErpBaseObject, IDocument
    {
        public Document(Session session) : base(session) { }

        string documentNumber = string.Empty;
        /// <summary>
        /// Unique document number
        /// </summary>
        [RuleRequiredField("Document_DocumentNumber_Required", DefaultContexts.Save)]
        [RuleUniqueValue("Document_DocumentNumber_Unique", DefaultContexts.Save)]
        [Size(50)]
        [XafDisplayName("Document Number")]
        [Index(0)]
        public string DocumentNumber
        {
            get => documentNumber;
            set => SetPropertyValue(nameof(DocumentNumber), ref documentNumber, value);
        }

        DateOnly date = DateOnly.FromDateTime(DateTime.Today);
        /// <summary>
        /// Date of the document
        /// </summary>
        [XafDisplayName("Date")]
        [Index(1)]
        public DateOnly Date
        {
            get => date;
            set => SetPropertyValue(nameof(Date), ref date, value);
        }

        TimeOnly time = TimeOnly.FromDateTime(DateTime.Now);
        /// <summary>
        /// Time of the document
        /// </summary>
        [XafDisplayName("Time")]
        [Index(2)]
        public TimeOnly Time
        {
            get => time;
            set => SetPropertyValue(nameof(Time), ref time, value);
        }

        BusinessEntity businessEntity;
        /// <summary>
        /// Business entity associated with this document (customer, vendor, etc.)
        /// </summary>
        [RuleRequiredField("Document_BusinessEntity_Required", DefaultContexts.Save)]
        [XafDisplayName("Business Entity")]
        [Index(3)]
        public BusinessEntity BusinessEntity
        {
            get => businessEntity;
            set => SetPropertyValue(nameof(BusinessEntity), ref businessEntity, value);
        }

        DocumentType documentType;
        /// <summary>
        /// Document type information
        /// </summary>
        [RuleRequiredField("Document_DocumentType_Required", DefaultContexts.Save)]
        [XafDisplayName("Document Type")]
        [Index(4)]
        public DocumentType DocumentType
        {
            get => documentType;
            set => SetPropertyValue(nameof(DocumentType), ref documentType, value);
        }

        string reference = string.Empty;
        /// <summary>
        /// External reference or customer/vendor reference
        /// </summary>
        [Size(100)]
        [XafDisplayName("Reference")]
        [Index(5)]
        public string Reference
        {
            get => reference;
            set => SetPropertyValue(nameof(Reference), ref reference, value);
        }

        string notes = string.Empty;
        /// <summary>
        /// Additional notes or comments
        /// </summary>
        [Size(1000)]
        [XafDisplayName("Notes")]
        [Index(6)]
        public string Notes
        {
            get => notes;
            set => SetPropertyValue(nameof(Notes), ref notes, value);
        }

        DocumentStatus status = DocumentStatus.Draft;
        /// <summary>
        /// Current status of the document
        /// </summary>
        [XafDisplayName("Status")]
        [Index(7)]
        public DocumentStatus Status
        {
            get => status;
            set => SetPropertyValue(nameof(Status), ref status, value);
        }

        /// <summary>
        /// Lines contained in this document
        /// </summary>
        [Association("Document-Lines")]
        [XafDisplayName("Lines")]
        public XPCollection<DocumentLine> Lines => GetCollection<DocumentLine>(nameof(Lines));

        /// <summary>
        /// Totals for this document (subtotal, taxes, total, etc.)
        /// </summary>
        [Association("Document-Totals")]
        [XafDisplayName("Document Totals")]
        public XPCollection<Total> DocumentTotals => GetCollection<Total>(nameof(DocumentTotals));

        /// <summary>
        /// Calculated subtotal of all lines
        /// </summary>
        [PersistentAlias("Lines.Sum(Amount)")]
        [XafDisplayName("Subtotal")]
        [ModelDefault("DisplayFormat", "c2")]
        [ModelDefault("AllowEdit", "False")]
        public decimal Subtotal
        {
            get => Convert.ToDecimal(EvaluateAlias(nameof(Subtotal)));
        }

        /// <summary>
        /// Calculated total including all document totals
        /// </summary>
        [PersistentAlias("Lines.Sum(Amount) + DocumentTotals.Sum(Total)")]
        [XafDisplayName("Total")]
        [ModelDefault("DisplayFormat", "c2")]
        [ModelDefault("AllowEdit", "False")]
        public decimal Total
        {
            get => Convert.ToDecimal(EvaluateAlias(nameof(Total)));
        }

        /// <summary>
        /// Returns a string representation of the document
        /// </summary>
        public override string ToString()
        {
            return $"{DocumentNumber} - {BusinessEntity?.Name} ({Date})";
        }

        /// <summary>
        /// Validation to ensure document has at least one line
        /// </summary>
        [RuleFromBoolProperty("Document_Lines_Required", DefaultContexts.Save,
            "Document must have at least one line")]
        public bool HasLines
        {
            get => Lines.Count > 0;
        }

        /// <summary>
        /// Updates line numbers when lines are added
        /// </summary>
        protected override void OnChanged(string propertyName, object oldValue, object newValue)
        {
            base.OnChanged(propertyName, oldValue, newValue);
            
            if (propertyName == nameof(Lines))
            {
                RenumberLines();
            }
        }

        /// <summary>
        /// Renumbers lines in increments of 10
        /// </summary>
        private void RenumberLines()
        {
            var linesList = Lines.OrderBy(l => l.LineNumber).ToList();
            for (int i = 0; i < linesList.Count; i++)
            {
                linesList[i].LineNumber = (i + 1) * 10;
            }
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            Date = DateOnly.FromDateTime(DateTime.Today);
            Time = TimeOnly.FromDateTime(DateTime.Now);
            Status = DocumentStatus.Draft;
        }

        // Interface implementations for IDocument
        IBusinessEntity IDocument.BusinessEntity
        {
            get => BusinessEntity;
            set => BusinessEntity = value as BusinessEntity;
        }

        IDocumentType IDocument.DocumentType
        {
            get => DocumentType;
            set => DocumentType = value as DocumentType;
        }

        IList<IDocumentLine> IDocument.Lines
        {
            get => Lines.Cast<IDocumentLine>().ToList();
            set => throw new NotSupportedException("Use XPO collection Lines");
        }

        IList<ITotal> IDocument.DocumentTotals
        {
            get => DocumentTotals.Cast<ITotal>().ToList();
            set => throw new NotSupportedException("Use XPO collection DocumentTotals");
        }

        // Property required for IDocument interface but not exposed in UI
        Guid IDocument.Oid
        {
            get => Oid;
            set => throw new NotSupportedException("Use XPO's Oid property for persistence");
        }
    }

    /// <summary>
    /// Enumeration for document status
    /// </summary>
    public enum DocumentStatus
    {
        /// <summary>
        /// Document is being created/edited
        /// </summary>
        Draft,
        
        /// <summary>
        /// Document is pending approval
        /// </summary>
        PendingApproval,
        
        /// <summary>
        /// Document has been approved and is active
        /// </summary>
        Approved,
        
        /// <summary>
        /// Document has been posted/completed
        /// </summary>
        Posted,
        
        /// <summary>
        /// Document has been cancelled
        /// </summary>
        Cancelled,
        
        /// <summary>
        /// Document has been voided
        /// </summary>
        Voided
    }
}