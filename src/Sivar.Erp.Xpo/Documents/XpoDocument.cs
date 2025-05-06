using DevExpress.Xpo;
using Sivar.Erp.Documents;
using Sivar.Erp.Xpo.Core;
using System;
using System.Collections.Generic;

namespace Sivar.Erp.Xpo.Documents
{
    /// <summary>
    /// Implementation of document entity using XPO
    /// </summary>
    [Persistent("Documents")]
    public class XpoDocument : XpoPersistentBase, IDocument
    {
        /// <summary>
        /// Default constructor required by XPO
        /// </summary>
        public XpoDocument(Session session) : base(session) { }

      

        private DateOnly _documentDate;

        /// <summary>
        /// Date of the document
        /// </summary>
        [Persistent("DocumentDate")]
        public DateOnly DocumentDate
        {
            get => _documentDate;
            set => SetPropertyValue(nameof(DocumentDate), ref _documentDate, value);
        }

        private string _documentNo = string.Empty;

        /// <summary>
        /// Document number or reference
        /// </summary>
        [Persistent("DocumentNo"), Size(100)]
        public string DocumentNo
        {
            get => _documentNo;
            set => SetPropertyValue(nameof(DocumentNo), ref _documentNo, value);
        }

        private string _description = string.Empty;

        /// <summary>
        /// Short description of the document
        /// </summary>
        [Persistent("Description"), Size(500)]
        public string Description
        {
            get => _description;
            set => SetPropertyValue(nameof(Description), ref _description, value);
        }

        private string _documentComments = string.Empty;

        /// <summary>
        /// Comments that appear on the document itself
        /// </summary>
        [Persistent("DocumentComments"), Size(4000)]
        public string DocumentComments
        {
            get => _documentComments;
            set => SetPropertyValue(nameof(DocumentComments), ref _documentComments, value);
        }

        private string _internalComments = string.Empty;

        /// <summary>
        /// Comments for internal use only
        /// </summary>
        [Persistent("InternalComments"), Size(500)]
        public string InternalComments
        {
            get => _internalComments;
            set => SetPropertyValue(nameof(InternalComments), ref _internalComments, value);
        }

        private DocumentType _documentType;

        /// <summary>
        /// Type of document
        /// </summary>
        [Persistent("DocumentType")]
        public DocumentType DocumentType
        {
            get => _documentType;
            set => SetPropertyValue(nameof(DocumentType), ref _documentType, value);
        }

        private Guid? _extendedDocumentTypeId;

        /// <summary>
        /// ID of extended document type (if from extension)
        /// </summary>
        [Persistent("ExtendedDocumentTypeId")]
        public Guid? ExtendedDocumentTypeId
        {
            get => _extendedDocumentTypeId;
            set => SetPropertyValue(nameof(ExtendedDocumentTypeId), ref _extendedDocumentTypeId, value);
        }

        private string _externalId = string.Empty;

        /// <summary>
        /// External identifier for the document (if from external system)
        /// </summary>
        [Persistent("ExternalId"), Size(255)]
        [Indexed]
        public string ExternalId
        {
            get => _externalId;
            set => SetPropertyValue(nameof(ExternalId), ref _externalId, value);
        }

        // XPO collections for related entities
        [Association("Document-Transactions")]
        public XPCollection<XpoTransaction> Transactions =>
            GetCollection<XpoTransaction>(nameof(Transactions));
    }
}