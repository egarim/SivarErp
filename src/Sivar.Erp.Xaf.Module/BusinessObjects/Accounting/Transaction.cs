using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using Sivar.Erp.Services.Accounting.Transactions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Sivar.Erp.Xaf.Module.BusinessObjects.Accounting
{
    /// <summary>
    /// XAF persistent implementation of ITransaction
    /// Represents financial transactions containing multiple ledger entries
    /// </summary>
    [DefaultClassOptions]
    [NavigationItem("Accounting")]
    [DefaultProperty(nameof(TransactionNumber))]
    [XafDisplayName("Transaction")]
    [XafDefaultProperty(nameof(TransactionNumber))]
    [Appearance("Transaction_PostedTransaction", 
        TargetItems = "*", 
        Criteria = "IsPosted = true",
        BackColor = "LightGreen",
        FontColor = "DarkGreen")]
    [Appearance("Transaction_ReadOnlyWhenPosted", 
        TargetItems = "TransactionNumber;TransactionDate;Description;DocumentNumber", 
        Criteria = "IsPosted = true",
        Enabled = false)]
    public class Transaction : ErpBaseObject, ITransaction
    {
        public Transaction(Session session) : base(session) { }

        string transactionNumber = string.Empty;
        /// <summary>
        /// Unique transaction number
        /// </summary>
        [RuleRequiredField("Transaction_TransactionNumber_Required", DefaultContexts.Save)]
        [RuleUniqueValue("Transaction_TransactionNumber_Unique", DefaultContexts.Save)]
        [Size(50)]
        [XafDisplayName("Transaction Number")]
        [Index(0)]
        public string TransactionNumber
        {
            get => transactionNumber;
            set => SetPropertyValue(nameof(TransactionNumber), ref transactionNumber, value?.ToUpperInvariant());
        }

        string documentNumber = string.Empty;
        /// <summary>
        /// Reference to the parent document
        /// </summary>
        [Size(50)]
        [XafDisplayName("Document Number")]
        [Index(1)]
        public string DocumentNumber
        {
            get => documentNumber;
            set => SetPropertyValue(nameof(DocumentNumber), ref documentNumber, value);
        }

        DateOnly transactionDate;
        /// <summary>
        /// Date of the transaction (may differ from document date)
        /// </summary>
        [RuleRequiredField("Transaction_TransactionDate_Required", DefaultContexts.Save)]
        [XafDisplayName("Transaction Date")]
        [Index(2)]
        public DateOnly TransactionDate
        {
            get => transactionDate;
            set => SetPropertyValue(nameof(TransactionDate), ref transactionDate, value);
        }

        string description = string.Empty;
        /// <summary>
        /// Description of the transaction
        /// </summary>
        [RuleRequiredField("Transaction_Description_Required", DefaultContexts.Save)]
        [Size(500)]
        [XafDisplayName("Description")]
        [Index(3)]
        public string Description
        {
            get => description;
            set => SetPropertyValue(nameof(Description), ref description, value);
        }

        bool isPosted = false;
        /// <summary>
        /// Indicates whether the transaction has been posted to the ledger
        /// </summary>
        [XafDisplayName("Posted")]
        [Index(4)]
        [ModelDefault("AllowEdit", "False")]
        public bool IsPosted
        {
            get => isPosted;
            set => SetPropertyValue(nameof(IsPosted), ref isPosted, value);
        }

        /// <summary>
        /// Ledger entries that make up this transaction
        /// </summary>
        [Association("Transaction-LedgerEntries")]
        [XafDisplayName("Ledger Entries")]
        [DevExpress.Xpo.Aggregated]
        public XPCollection<LedgerEntry> LedgerEntries => GetCollection<LedgerEntry>();

        /// <summary>
        /// Parent batch that contains this transaction (if any)
        /// </summary>
        [Association("TransactionBatch-Transactions")]
        [XafDisplayName("Transaction Batch")]
        [Index(8)]
        public TransactionBatch TransactionBatch { get; set; }

        /// <summary>
        /// Calculated total of debit entries
        /// </summary>
        [PersistentAlias("LedgerEntries[EntryType = 0].Sum(Amount)")]
        [XafDisplayName("Total Debits")]
        [ModelDefault("DisplayFormat", "c2")]
        [ModelDefault("AllowEdit", "False")]
        [Index(5)]
        public decimal TotalDebits => (decimal)(EvaluateAlias(nameof(TotalDebits)) ?? 0m);

        /// <summary>
        /// Calculated total of credit entries
        /// </summary>
        [PersistentAlias("LedgerEntries[EntryType = 1].Sum(Amount)")]
        [XafDisplayName("Total Credits")]
        [ModelDefault("DisplayFormat", "c2")]
        [ModelDefault("AllowEdit", "False")]
        [Index(6)]
        public decimal TotalCredits => (decimal)(EvaluateAlias(nameof(TotalCredits)) ?? 0m);

        /// <summary>
        /// Calculated out of balance amount (should be zero for valid transactions)
        /// </summary>
        [PersistentAlias("LedgerEntries[EntryType = 0].Sum(Amount) - LedgerEntries[EntryType = 1].Sum(Amount)")]
        [XafDisplayName("Out of Balance")]
        [ModelDefault("DisplayFormat", "c2")]
        [ModelDefault("AllowEdit", "False")]
        [Index(7)]
        public decimal OutOfBalance => (decimal)(EvaluateAlias(nameof(OutOfBalance)) ?? 0m);

        /// <summary>
        /// Posts the transaction to the ledger
        /// </summary>
        public void Post()
        {
            if (IsPosted)
                throw new InvalidOperationException("Transaction is already posted");

            if (!IsBalanced)
                throw new InvalidOperationException("Transaction must be balanced before posting");

            IsPosted = true;
        }

        /// <summary>
        /// Unposts the transaction from the ledger
        /// </summary>
        public void UnPost()
        {
            if (!IsPosted)
                throw new InvalidOperationException("Transaction is not posted");

            IsPosted = false;
        }

        /// <summary>
        /// Checks if the transaction is balanced (total debits = total credits)
        /// </summary>
        public bool IsBalanced => Math.Abs(OutOfBalance) < 0.01m;

        /// <summary>
        /// Returns a string representation of the transaction
        /// </summary>
        public override string ToString()
        {
            return $"{TransactionNumber} - {Description} ({TransactionDate:yyyy-MM-dd})";
        }

        /// <summary>
        /// Validation to ensure transaction has at least two ledger entries
        /// </summary>
        [RuleFromBoolProperty("Transaction_MinimumEntries", DefaultContexts.Save,
            "Transaction must have at least two ledger entries")]
        public bool HasMinimumEntries => LedgerEntries.Count >= 2;

        /// <summary>
        /// Validation to ensure transaction is balanced
        /// </summary>
        [RuleFromBoolProperty("Transaction_Balanced", DefaultContexts.Save,
            "Transaction must be balanced (total debits must equal total credits)")]
        public bool IsTransactionBalanced => IsBalanced;

        /// <summary>
        /// Validation to ensure posted transaction cannot be modified
        /// </summary>
        [RuleFromBoolProperty("Transaction_CannotModifyPosted", DefaultContexts.Save,
            "Posted transaction cannot be modified")]
        public bool CanModifyTransaction
        {
            get
            {
                // Allow modification if not posted or if we're just setting IsPosted flag
                return !IsPosted || IsDeleted;
            }
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            TransactionDate = DateOnly.FromDateTime(DateTime.Today);
            IsPosted = false;
        }

        protected override void OnChanged(string propertyName, object oldValue, object newValue)
        {
            base.OnChanged(propertyName, oldValue, newValue);

            // Prevent modification of posted transactions except for IsPosted flag
            if (IsPosted && propertyName != nameof(IsPosted) && !IsLoading && !IsDeleted)
            {
                SetPropertyValue(propertyName, ref oldValue, oldValue);
                throw new InvalidOperationException("Cannot modify posted transaction");
            }
        }

        // Interface implementation
        IEnumerable<ILedgerEntry> ITransaction.LedgerEntries 
        { 
            get => LedgerEntries.Cast<ILedgerEntry>();
            set
            {
                // Remove existing entries
                while (LedgerEntries.Count > 0)
                {
                    LedgerEntries[0].Delete();
                }
                
                if (value != null)
                {
                    foreach (var entry in value)
                    {
                        if (entry is LedgerEntry xafEntry)
                            LedgerEntries.Add(xafEntry);
                        else
                        {
                            // Convert from DTO to XAF object if needed
                            var newEntry = new LedgerEntry(Session)
                            {
                                LedgerEntryNumber = entry.LedgerEntryNumber,
                                TransactionNumber = entry.TransactionNumber,
                                EntryType = entry.EntryType,
                                Amount = entry.Amount,
                                AccountName = entry.AccountName,
                                OfficialCode = entry.OfficialCode
                            };
                            LedgerEntries.Add(newEntry);
                        }
                    }
                }
            }
        }
    }
}