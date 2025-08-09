using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using Sivar.Erp.Services.Accounting.Transactions;
using System;
using System.ComponentModel;

namespace Sivar.Erp.Xaf.Module.BusinessObjects.Accounting
{
    /// <summary>
    /// XAF persistent implementation of ILedgerEntry
    /// Represents individual ledger entries within transactions
    /// </summary>
    [DefaultClassOptions]
    [XafDisplayName("Ledger Entry")]
    [XafDefaultProperty(nameof(LedgerEntryNumber))]
    [Appearance("LedgerEntry_DebitEntry", 
        TargetItems = "Amount", 
        Criteria = "EntryType = 'Debit'",
        BackColor = "LightBlue")]
    [Appearance("LedgerEntry_CreditEntry", 
        TargetItems = "Amount", 
        Criteria = "EntryType = 'Credit'",
        BackColor = "LightYellow")]
    public class LedgerEntry : ErpBaseObject, ILedgerEntry
    {
        public LedgerEntry(Session session) : base(session) { }

        string ledgerEntryNumber = string.Empty;
        /// <summary>
        /// Unique identifier for the ledger entry
        /// </summary>
        [RuleRequiredField("LedgerEntry_LedgerEntryNumber_Required", DefaultContexts.Save)]
        [RuleUniqueValue("LedgerEntry_LedgerEntryNumber_Unique", DefaultContexts.Save)]
        [Size(50)]
        [XafDisplayName("Entry Number")]
        [Index(0)]
        public string LedgerEntryNumber
        {
            get => ledgerEntryNumber;
            set => SetPropertyValue(nameof(LedgerEntryNumber), ref ledgerEntryNumber, value?.ToUpperInvariant());
        }

        string transactionNumber = string.Empty;
        /// <summary>
        /// Reference to the parent transaction
        /// </summary>
        [RuleRequiredField("LedgerEntry_TransactionNumber_Required", DefaultContexts.Save)]
        [Size(50)]
        [XafDisplayName("Transaction Number")]
        [Index(1)]
        public string TransactionNumber
        {
            get => transactionNumber;
            set => SetPropertyValue(nameof(TransactionNumber), ref transactionNumber, value);
        }

        EntryType entryType = EntryType.Debit;
        /// <summary>
        /// Type of entry (debit or credit)
        /// </summary>
        [XafDisplayName("Entry Type")]
        [Index(2)]
        public EntryType EntryType
        {
            get => entryType;
            set => SetPropertyValue(nameof(EntryType), ref entryType, value);
        }

        decimal amount = 0m;
        /// <summary>
        /// Amount of the entry
        /// </summary>
        [RuleRange("LedgerEntry_Amount_Range", DefaultContexts.Save, 0.01, double.MaxValue,
            "Amount must be greater than zero")]
        [XafDisplayName("Amount")]
        [ModelDefault("DisplayFormat", "c2")]
        [ModelDefault("EditMask", "c2")]
        [Index(3)]
        public decimal Amount
        {
            get => amount;
            set => SetPropertyValue(nameof(Amount), ref amount, value);
        }

        string accountName = string.Empty;
        /// <summary>
        /// Name of the account
        /// </summary>
        [RuleRequiredField("LedgerEntry_AccountName_Required", DefaultContexts.Save)]
        [Size(200)]
        [XafDisplayName("Account Name")]
        [Index(4)]
        public string AccountName
        {
            get => accountName;
            set => SetPropertyValue(nameof(AccountName), ref accountName, value);
        }

        string officialCode = string.Empty;
        /// <summary>
        /// Official code/identifier for the account
        /// </summary>
        [RuleRequiredField("LedgerEntry_OfficialCode_Required", DefaultContexts.Save)]
        [Size(50)]
        [XafDisplayName("Account Code")]
        [Index(5)]
        public string OfficialCode
        {
            get => officialCode;
            set => SetPropertyValue(nameof(OfficialCode), ref officialCode, value?.ToUpperInvariant());
        }

        string description = string.Empty;
        /// <summary>
        /// Optional description for the ledger entry
        /// </summary>
        [Size(500)]
        [XafDisplayName("Description")]
        [Index(6)]
        public string Description
        {
            get => description;
            set => SetPropertyValue(nameof(Description), ref description, value);
        }

        /// <summary>
        /// Parent transaction that contains this ledger entry
        /// </summary>
        [Association("Transaction-LedgerEntries")]
        [XafDisplayName("Transaction")]
        [Browsable(false)]
        public Transaction Transaction { get; set; }

        /// <summary>
        /// Reference to the chart of accounts entry
        /// </summary>
        [XafDisplayName("Account")]
        [DataSourceProperty("AvailableAccounts")]
        [Index(7)]
        public Account Account { get; set; }

        /// <summary>
        /// Available accounts for selection
        /// </summary>
        [Browsable(false)]
        public XPCollection<Account> AvailableAccounts => new XPCollection<Account>(Session);

        /// <summary>
        /// Returns a string representation of the ledger entry
        /// </summary>
        public override string ToString()
        {
            return $"{LedgerEntryNumber} - {AccountName} ({EntryType}) {Amount:C}";
        }

        /// <summary>
        /// Validation to ensure account code matches selected account
        /// </summary>
        [RuleFromBoolProperty("LedgerEntry_AccountCodeMatches", DefaultContexts.Save,
            "Account code must match selected account")]
        public bool IsAccountCodeValid
        {
            get
            {
                if (Account == null || string.IsNullOrWhiteSpace(OfficialCode))
                    return true;

                return string.Equals(Account.OfficialCode, OfficialCode, StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Validation to ensure account exists in chart of accounts
        /// </summary>
        [RuleFromBoolProperty("LedgerEntry_AccountExists", DefaultContexts.Save,
            "Account must exist in chart of accounts")]
        public bool DoesAccountExist
        {
            get
            {
                if (string.IsNullOrWhiteSpace(OfficialCode))
                    return true;

                var account = Session.FindObject<Account>(
                    new DevExpress.Data.Filtering.BinaryOperator(
                        nameof(Account.OfficialCode), OfficialCode));
                
                return account != null && account.IsActive;
            }
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            EntryType = EntryType.Debit;
            Amount = 0m;
        }

        protected override void OnChanged(string propertyName, object oldValue, object newValue)
        {
            base.OnChanged(propertyName, oldValue, newValue);

            // Auto-populate account information when account is selected
            if (propertyName == nameof(Account) && Account != null)
            {
                OfficialCode = Account.OfficialCode;
                AccountName = Account.AccountName;
            }

            // Update transaction number when parent transaction changes
            if (propertyName == nameof(Transaction) && Transaction != null)
            {
                TransactionNumber = Transaction.TransactionNumber;
            }
        }
    }
}