using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using Sivar.Erp.ChartOfAccounts;
using Sivar.Erp.Xpo.Core;
using Sivar.Erp.Xpo.Documents;
using System;
using System.Collections.Generic;

namespace Sivar.Erp.Xpo.ChartOfAccounts
{
    /// <summary>
    /// Implementation of account entity using XPO
    /// </summary>
    [Persistent("Accounts")]
    public class XpoAccount : XpoArchivableBase, IAccount
    {
        /// <summary>
        /// Default constructor required by XPO
        /// </summary>
        public XpoAccount(Session session) : base(session) { }

        string parentOfficialCode;
        private Guid? _balanceAndIncomeLineId;

        /// <summary>
        /// Optional reference to balance sheet or income statement line
        /// </summary>
        [Persistent("BalanceAndIncomeLineId")]
        public Guid? BalanceAndIncomeLineId
        {
            get => _balanceAndIncomeLineId;
            set => SetPropertyValue(nameof(BalanceAndIncomeLineId), ref _balanceAndIncomeLineId, value);
        }

        private string _accountName = string.Empty;

        /// <summary>
        /// Name of the account
        /// </summary>
        [Persistent("AccountName"), Size(255)]
        public string AccountName
        {
            get => _accountName;
            set => SetPropertyValue(nameof(AccountName), ref _accountName, value);
        }

        private AccountType _accountType;

        /// <summary>
        /// Type of account (asset, liability, etc.)
        /// </summary>
        [Persistent("AccountType")]
        public AccountType AccountType
        {
            get => _accountType;
            set => SetPropertyValue(nameof(AccountType), ref _accountType, value);
        }

        private string _officialCode = string.Empty;

        /// <summary>
        /// Official code/identifier for the account (e.g., for SAF-T reporting)
        /// </summary>
        [Persistent("OfficialCode"), Size(100)]
        [Indexed(Name = "IDX_Account_OfficialCode", Unique = true)]
        public string OfficialCode
        {
            get => _officialCode;
            set => SetPropertyValue(nameof(OfficialCode), ref _officialCode, value);
        }

        private string _parentAccountCode = string.Empty;

        /// <summary>
        /// Optional reference to the parent account's official code (null for top-level accounts)
        /// </summary>
        [Persistent("ParentAccountCode"), Size(100)]
        public string ParentAccountCode
        {
            get => _parentAccountCode;
            set => SetPropertyValue(nameof(ParentAccountCode), ref _parentAccountCode, value);
        }
        
        [Size(SizeAttribute.DefaultStringMappingFieldSize)]
        public string ParentOfficialCode
        {
            get => parentOfficialCode;
            set => SetPropertyValue(nameof(ParentOfficialCode), ref parentOfficialCode, value);
        }
        // XPO collections for related entities
        [Association("Account-LedgerEntries")]
        public XPCollection<XpoLedgerEntry> LedgerEntries =>
            GetCollection<XpoLedgerEntry>(nameof(LedgerEntries));

        #region Parent-Child Relationships

        /// <summary>
        /// Returns the parent account based on ParentAccountCode
        /// </summary>
        [NonPersistent]
        public XpoAccount ParentAccount
        {
            get
            {
                if (string.IsNullOrEmpty(ParentAccountCode))
                    return null;

                return Session.FindObject<XpoAccount>(
                    CriteriaOperator.Parse($"{nameof(OfficialCode)} = ?", ParentAccountCode));
            }
        }

        /// <summary>
        /// Returns all child accounts that have this account's official code as their parent account code
        /// </summary>
        [NonPersistent]
        public XPCollection<XpoAccount> ChildAccounts
        {
            get
            {
                return new XPCollection<XpoAccount>(Session,
                    CriteriaOperator.Parse($"{nameof(ParentAccountCode)} = ?", OfficialCode));
            }
        }

        #endregion

        /// <summary>
        /// Validates the account object according to business rules
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        public bool Validate()
        {
            // Account name is required
            if (string.IsNullOrWhiteSpace(AccountName))
            {
                return false;
            }

            // Official code is required
            if (string.IsNullOrWhiteSpace(OfficialCode))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines if the account can be used for transactions
        /// </summary>
        /// <returns>True if account can be used, false otherwise</returns>
        public bool CanUseForTransactions()
        {
            // Archived accounts cannot be used
            if (IsArchived)
            {
                return false;
            }

            // If this account has child accounts, it's a summary account and shouldn't be used directly
            // Note: This requires database access, so it might be better to add an IsSummary flag
            if (ChildAccounts.Count > 0)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines if an account typically has a debit balance
        /// </summary>
        /// <returns>True if typically debit balance, false if typically credit balance</returns>
        public bool HasDebitBalance()
        {
            return AccountType == AccountType.Asset || AccountType == AccountType.Expense;
        }
    }
}