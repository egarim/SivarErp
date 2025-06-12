using DevExpress.Xpo;
using Sivar.Erp.Documents;
using Sivar.Erp.Xpo.ChartOfAccounts;
using Sivar.Erp.Xpo.Core;
using System;

namespace Sivar.Erp.Xpo.Documents
{
    /// <summary>
    /// Implementation of ledger entry entity using XPO
    /// </summary>
    [Persistent("LedgerEntries")]
    public class XpoLedgerEntry : XpoPersistentBase, ILedgerEntry
    {
        /// <summary>
        /// Default constructor required by XPO
        /// </summary>
        public XpoLedgerEntry(Session session) : base(session) { }



        string accountName;
        string officialCode;
        private XpoTransaction _transaction;

        /// <summary>
        /// The transaction that this ledger entry belongs to
        /// </summary>
        [Association("Transaction-LedgerEntries")]
        public XpoTransaction Transaction
        {
            get => _transaction;
            set => SetPropertyValue(nameof(Transaction), ref _transaction, value);
        }

        /// <summary>
        /// Reference to the parent transaction
        /// </summary>
        [PersistentAlias("Transaction.Id")]
        public Guid TransactionId
        {
            get => (Guid)EvaluateAlias(nameof(TransactionId));
            set
            {
                if (value != TransactionId)
                {
                    Transaction = Session.GetObjectByKey<XpoTransaction>(value);
                }
            }
        }

        private XpoAccount _account;

        /// <summary>
        /// The account that this ledger entry affects
        /// </summary>
        [Association("Account-LedgerEntries")]
        public XpoAccount Account
        {
            get => _account;
            set => SetPropertyValue(nameof(Account), ref _account, value);
        }

        /// <summary>
        /// Reference to the account
        /// </summary>
        [PersistentAlias("Account.Id")]
        public Guid AccountId
        {
            get => (Guid)EvaluateAlias(nameof(AccountId));
            set
            {
                if (value != AccountId)
                {
                    Account = Session.GetObjectByKey<XpoAccount>(value);
                }
            }
        }

        private EntryType _entryType;

        /// <summary>
        /// Type of entry (debit or credit)
        /// </summary>
        [Persistent("EntryType")]
        public EntryType EntryType
        {
            get => _entryType;
            set => SetPropertyValue(nameof(EntryType), ref _entryType, value);
        }

        private decimal _amount;

        /// <summary>
        /// Amount of the entry
        /// </summary>
        [Persistent("Amount")]
        public decimal Amount
        {
            get => _amount;
            set => SetPropertyValue(nameof(Amount), ref _amount, value);
        }

        private Guid? _personId;

        /// <summary>
        /// Optional reference to a person for analysis
        /// </summary>
        [Persistent("PersonId")]
        public Guid? PersonId
        {
            get => _personId;
            set => SetPropertyValue(nameof(PersonId), ref _personId, value);
        }

        private Guid? _costCentreId;

        /// <summary>
        /// Optional reference to a cost center for analysis
        /// </summary>
        [Persistent("CostCentreId")]
        public Guid? CostCentreId
        {
            get => _costCentreId;
            set => SetPropertyValue(nameof(CostCentreId), ref _costCentreId, value);
        }

        [Size(SizeAttribute.DefaultStringMappingFieldSize)]
        public string OfficialCode
        {
            get => officialCode;
            set => SetPropertyValue(nameof(OfficialCode), ref officialCode, value);
        }
        
        [Size(SizeAttribute.DefaultStringMappingFieldSize)]
        public string AccountName
        {
            get => accountName;
            set => SetPropertyValue(nameof(AccountName), ref accountName, value);
        }
    }
}
