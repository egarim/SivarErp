using DevExpress.Xpo;
using Sivar.Erp.Documents;
using Sivar.Erp.Xpo.Core;
using System;
using System.Collections.Generic;

namespace Sivar.Erp.Xpo.Documents
{
    /// <summary>
    /// Implementation of transaction entity using XPO
    /// </summary>
    [Persistent("Transactions")]
    public class XpoTransaction : XpoPersistentBase, ITransaction
    {
        /// <summary>
        /// Default constructor required by XPO
        /// </summary>
        public XpoTransaction(Session session) : base(session) { }

       

        private XpoDocument _document;

        /// <summary>
        /// The document that this transaction belongs to
        /// </summary>
        [Association("Document-Transactions")]
        public XpoDocument Document
        {
            get => _document;
            set => SetPropertyValue(nameof(Document), ref _document, value);
        }

        /// <summary>
        /// Reference to the parent document
        /// </summary>
        [PersistentAlias("Document.Id")]
        public Guid DocumentId
        {
            get => (Guid)EvaluateAlias(nameof(DocumentId));
            set
            {
                if (value != DocumentId)
                {
                    Document = Session.GetObjectByKey<XpoDocument>(value);
                }
            }
        }

        private DateOnly _transactionDate;

        /// <summary>
        /// Date of the transaction (may differ from document date)
        /// </summary>
        [Persistent("TransactionDate")]
        public DateOnly TransactionDate
        {
            get => _transactionDate;
            set => SetPropertyValue(nameof(TransactionDate), ref _transactionDate, value);
        }

        private string _description = string.Empty;

        /// <summary>
        /// Description of the transaction
        /// </summary>
        [Persistent("Description"), Size(255)]
        public string Description
        {
            get => _description;
            set => SetPropertyValue(nameof(Description), ref _description, value);
        }

        // XPO collections for related entities
        [Association("Transaction-LedgerEntries")]
        public XPCollection<XpoLedgerEntry> LedgerEntries =>
            GetCollection<XpoLedgerEntry>(nameof(LedgerEntries));
    }
}