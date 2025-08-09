using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using Sivar.Erp.Services.Accounting.Transactions;
using System;
using System.ComponentModel;
using System.Linq;

namespace Sivar.Erp.Xaf.Module.BusinessObjects.Accounting
{
    /// <summary>
    /// XAF persistent implementation of ITransactionBatch
    /// Represents batches of transactions for bulk processing
    /// </summary>
    [DefaultClassOptions]
    [NavigationItem("Accounting")]
    [DefaultProperty(nameof(ReferenceCode))]
    [XafDisplayName("Transaction Batch")]
    [XafDefaultProperty(nameof(ReferenceCode))]
    [Appearance("TransactionBatch_ProcessedBatch", 
        TargetItems = "*", 
        Criteria = "IsPosted = true",
        BackColor = "LightGreen",
        FontColor = "DarkGreen")]
    [Appearance("TransactionBatch_ApprovedBatch", 
        TargetItems = "*", 
        Criteria = "Status = 'Approved'",
        BackColor = "LightBlue")]
    [Appearance("TransactionBatch_RejectedBatch", 
        TargetItems = "*", 
        Criteria = "Status = 'Rejected'",
        BackColor = "LightPink",
        FontColor = "DarkRed")]
    [Appearance("TransactionBatch_ReadOnlyWhenPosted", 
        TargetItems = "ReferenceCode;TransactionNumber;BatchDate;Description;Status", 
        Criteria = "IsPosted = true",
        Enabled = false)]
    public class TransactionBatch : ErpBaseObject, ITransactionBatch
    {
        public TransactionBatch(Session session) : base(session) { }

        string referenceCode = string.Empty;
        /// <summary>
        /// Batch reference code
        /// </summary>
        [RuleRequiredField("TransactionBatch_ReferenceCode_Required", DefaultContexts.Save)]
        [RuleUniqueValue("TransactionBatch_ReferenceCode_Unique", DefaultContexts.Save)]
        [Size(50)]
        [XafDisplayName("Reference Code")]
        [Index(0)]
        public string ReferenceCode
        {
            get => referenceCode;
            set => SetPropertyValue(nameof(ReferenceCode), ref referenceCode, value?.ToUpperInvariant());
        }

        string transactionNumber = string.Empty;
        /// <summary>
        /// Associated transaction number
        /// </summary>
        [Size(50)]
        [XafDisplayName("Transaction Number")]
        [Index(1)]
        public string TransactionNumber
        {
            get => transactionNumber;
            set => SetPropertyValue(nameof(TransactionNumber), ref transactionNumber, value);
        }

        DateOnly batchDate;
        /// <summary>
        /// Date when the batch was created
        /// </summary>
        [RuleRequiredField("TransactionBatch_BatchDate_Required", DefaultContexts.Save)]
        [XafDisplayName("Batch Date")]
        [Index(2)]
        public DateOnly BatchDate
        {
            get => batchDate;
            set => SetPropertyValue(nameof(BatchDate), ref batchDate, value);
        }

        string description = string.Empty;
        /// <summary>
        /// Description of the transaction batch
        /// </summary>
        [RuleRequiredField("TransactionBatch_Description_Required", DefaultContexts.Save)]
        [Size(500)]
        [XafDisplayName("Description")]
        [Index(3)]
        public string Description
        {
            get => description;
            set => SetPropertyValue(nameof(Description), ref description, value);
        }

        BatchStatus status = BatchStatus.Draft;
        /// <summary>
        /// Status of the batch
        /// </summary>
        [XafDisplayName("Status")]
        [Index(4)]
        public BatchStatus Status
        {
            get => status;
            set => SetPropertyValue(nameof(Status), ref status, value);
        }

        bool isPosted = false;
        /// <summary>
        /// Indicates whether the batch has been processed
        /// </summary>
        [XafDisplayName("Posted")]
        [Index(5)]
        [ModelDefault("AllowEdit", "False")]
        public bool IsPosted
        {
            get => isPosted;
            set => SetPropertyValue(nameof(IsPosted), ref isPosted, value);
        }

        string processedBy = string.Empty;
        /// <summary>
        /// User who processed the batch
        /// </summary>
        [Size(100)]
        [XafDisplayName("Processed By")]
        [ModelDefault("AllowEdit", "False")]
        [Index(6)]
        public string ProcessedBy
        {
            get => processedBy;
            set => SetPropertyValue(nameof(ProcessedBy), ref processedBy, value);
        }

        DateTime? processedAt;
        /// <summary>
        /// Date and time when the batch was processed
        /// </summary>
        [XafDisplayName("Processed At")]
        [ModelDefault("AllowEdit", "False")]
        [ModelDefault("DisplayFormat", "G")]
        [Index(7)]
        public DateTime? ProcessedAt
        {
            get => processedAt;
            set => SetPropertyValue(nameof(ProcessedAt), ref processedAt, value);
        }

        /// <summary>
        /// Transactions associated with this batch
        /// </summary>
        [Association("TransactionBatch-Transactions")]
        [XafDisplayName("Transactions")]
        [DevExpress.Xpo.Aggregated]
        public XPCollection<Transaction> Transactions => GetCollection<Transaction>();

        /// <summary>
        /// Posts the batch and all its transactions
        /// </summary>
        public void Post()
        {
            if (IsPosted)
                throw new InvalidOperationException("Batch is already posted");

            if (Status != BatchStatus.Approved)
                throw new InvalidOperationException("Batch must be approved before posting");

            // Post all transactions in the batch
            foreach (var transaction in Transactions)
            {
                if (!transaction.IsPosted)
                    transaction.Post();
            }

            IsPosted = true;
            Status = BatchStatus.Processed;
            ProcessedAt = DateTime.UtcNow;
            // ProcessedBy should be set by the service calling this method
        }

        /// <summary>
        /// Unposts the batch and all its transactions
        /// </summary>
        public void UnPost()
        {
            if (!IsPosted)
                throw new InvalidOperationException("Batch is not posted");

            // Unpost all transactions in the batch
            foreach (var transaction in Transactions)
            {
                if (transaction.IsPosted)
                    transaction.UnPost();
            }

            IsPosted = false;
            Status = BatchStatus.Approved; // Return to approved state
            ProcessedAt = null;
            ProcessedBy = string.Empty;
        }

        /// <summary>
        /// Approves the batch for processing
        /// </summary>
        public void Approve()
        {
            if (Status == BatchStatus.Approved || Status == BatchStatus.Processed)
                return;

            Status = BatchStatus.Approved;
        }

        /// <summary>
        /// Rejects the batch
        /// </summary>
        public void Reject()
        {
            if (IsPosted)
                throw new InvalidOperationException("Cannot reject a posted batch");

            Status = BatchStatus.Rejected;
        }

        /// <summary>
        /// Returns a string representation of the transaction batch
        /// </summary>
        public override string ToString()
        {
            return $"{ReferenceCode} - {Description} ({BatchDate:yyyy-MM-dd}) [{Status}]";
        }

        /// <summary>
        /// Validation to ensure posted batches cannot be modified
        /// </summary>
        [RuleFromBoolProperty("TransactionBatch_CannotModifyPosted", DefaultContexts.Save,
            "Posted batch cannot be modified")]
        public bool CanModifyBatch
        {
            get
            {
                // Allow modification if not posted or if we're just setting posting-related flags
                return !IsPosted || IsDeleted;
            }
        }

        /// <summary>
        /// Validation to ensure batch has at least one transaction
        /// </summary>
        [RuleFromBoolProperty("TransactionBatch_MustHaveTransactions", DefaultContexts.Save,
            "Batch must contain at least one transaction")]
        public bool HasTransactions => Transactions.Count > 0;

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            BatchDate = DateOnly.FromDateTime(DateTime.Today);
            Status = BatchStatus.Draft;
            IsPosted = false;
        }

        protected override void OnChanged(string propertyName, object oldValue, object newValue)
        {
            base.OnChanged(propertyName, oldValue, newValue);

            // Prevent modification of posted batches except for posting-related flags
            if (IsPosted && !new[] { nameof(IsPosted), nameof(ProcessedBy), nameof(ProcessedAt) }.Contains(propertyName) 
                && !IsLoading && !IsDeleted)
            {
                SetPropertyValue(propertyName, ref oldValue, oldValue);
                throw new InvalidOperationException("Cannot modify posted batch");
            }
        }
    }
}