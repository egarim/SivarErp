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
    /// XAF persistent implementation of IDocumentType
    /// Defines the types of documents that can be created in the system
    /// </summary>
    [DefaultClassOptions]
    [NavigationItem("Configuration")]
    [DefaultProperty(nameof(Name))]
    [XafDisplayName("Document Type")]
    [XafDefaultProperty(nameof(Name))]
    public class DocumentType : ErpBaseObject, IDocumentType
    {
        public DocumentType(Session session) : base(session) { }

        string code = string.Empty;
        /// <summary>
        /// Unique code for the document type (e.g., "INV", "PO", "SO")
        /// </summary>
        [RuleRequiredField("DocumentType_Code_Required", DefaultContexts.Save)]
        [RuleUniqueValue("DocumentType_Code_Unique", DefaultContexts.Save)]
        [Size(10)]
        [XafDisplayName("Code")]
        [Index(0)]
        public string Code
        {
            get => code;
            set => SetPropertyValue(nameof(Code), ref code, value?.ToUpperInvariant());
        }

        string name = string.Empty;
        /// <summary>
        /// Display name of the document type
        /// </summary>
        [RuleRequiredField("DocumentType_Name_Required", DefaultContexts.Save)]
        [Size(100)]
        [XafDisplayName("Name")]
        [Index(1)]
        public string Name
        {
            get => name;
            set => SetPropertyValue(nameof(Name), ref name, value);
        }

        bool isEnabled = true;
        /// <summary>
        /// Whether this document type is currently active and can be used
        /// </summary>
        [XafDisplayName("Enabled")]
        [Index(2)]
        public bool IsEnabled
        {
            get => isEnabled;
            set => SetPropertyValue(nameof(IsEnabled), ref isEnabled, value);
        }

        DocumentOperation documentOperation = DocumentOperation.SalesInvoice;
        /// <summary>
        /// The category/operation this document type represents
        /// </summary>
        [XafDisplayName("Document Operation")]
        [Index(3)]
        public DocumentOperation DocumentOperation
        {
            get => documentOperation;
            set => SetPropertyValue(nameof(DocumentOperation), ref documentOperation, value);
        }

        string description = string.Empty;
        /// <summary>
        /// Optional description of the document type
        /// </summary>
        [Size(500)]
        [XafDisplayName("Description")]
        [Index(4)]
        public string Description
        {
            get => description;
            set => SetPropertyValue(nameof(Description), ref description, value);
        }

        string numberSequence = string.Empty;
        /// <summary>
        /// Number sequence template for automatic document numbering
        /// </summary>
        [Size(50)]
        [XafDisplayName("Number Sequence")]
        [Index(5)]
        [ToolTip("Template for automatic document numbering (e.g., 'INV-{YYYY}-{####}')")]
        public string NumberSequence
        {
            get => numberSequence;
            set => SetPropertyValue(nameof(NumberSequence), ref numberSequence, value);
        }

        bool requiresApproval = false;
        /// <summary>
        /// Whether documents of this type require approval workflow
        /// </summary>
        [XafDisplayName("Requires Approval")]
        [Index(6)]
        public bool RequiresApproval
        {
            get => requiresApproval;
            set => SetPropertyValue(nameof(RequiresApproval), ref requiresApproval, value);
        }

        bool allowsManualNumbers = true;
        /// <summary>
        /// Whether manual document numbers are allowed for this type
        /// </summary>
        [XafDisplayName("Allow Manual Numbers")]
        [Index(7)]
        public bool AllowsManualNumbers
        {
            get => allowsManualNumbers;
            set => SetPropertyValue(nameof(AllowsManualNumbers), ref allowsManualNumbers, value);
        }

        /// <summary>
        /// Returns a string representation of the document type
        /// </summary>
        public override string ToString()
        {
            return $"{Code} - {Name}";
        }

        /// <summary>
        /// Validation to ensure number sequence is provided when manual numbers are not allowed
        /// </summary>
        [RuleFromBoolProperty("DocumentType_NumberSequence_Required", DefaultContexts.Save,
            "Number Sequence is required when manual numbers are not allowed")]
        public bool IsNumberSequenceValid
        {
            get => AllowsManualNumbers || !string.IsNullOrWhiteSpace(NumberSequence);
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            IsEnabled = true;
            AllowsManualNumbers = true;
            RequiresApproval = false;
        }

        /// <summary>
        /// Get the display name for the document operation
        /// </summary>
        [PersistentAlias("DocumentOperation")]
        public string DocumentOperationDisplayName
        {
            get => DocumentOperation.ToString();
        }

        // Property required for IDocumentType interface but not exposed in UI
        Guid IDocumentType.Oid
        {
            get => Oid;
            set => throw new NotSupportedException("Use XPO's Oid property for persistence");
        }

        // INotifyPropertyChanged is handled by XPO BaseObject
        event PropertyChangedEventHandler? INotifyPropertyChanged.PropertyChanged
        {
            add { /* XPO handles this */ }
            remove { /* XPO handles this */ }
        }
    }
}