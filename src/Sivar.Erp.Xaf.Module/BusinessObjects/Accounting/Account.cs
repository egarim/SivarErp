using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using Sivar.Erp.Services.Accounting.ChartOfAccounts;
using System;
using System.ComponentModel;

namespace Sivar.Erp.Xaf.Module.BusinessObjects.Accounting
{
    /// <summary>
    /// XAF persistent implementation of IAccount
    /// Represents chart of accounts entries with hierarchical structure
    /// </summary>
    [DefaultClassOptions]
    [NavigationItem("Accounting")]
    [DefaultProperty(nameof(AccountName))]
    [XafDisplayName("Account")]
    [XafDefaultProperty(nameof(AccountName))]
    [Appearance("Account_ReadOnlyAuditFields", 
        TargetItems = "InsertedAt;InsertedBy;UpdatedAt;UpdatedBy", 
        Enabled = false)]
    public class Account : ErpBaseObject, IAccount
    {
        public Account(Session session) : base(session) { }

        string officialCode = string.Empty;
        /// <summary>
        /// Official code/identifier for the account (e.g., for SAF-T reporting)
        /// </summary>
        [RuleRequiredField("Account_OfficialCode_Required", DefaultContexts.Save)]
        [RuleUniqueValue("Account_OfficialCode_Unique", DefaultContexts.Save)]
        [Size(50)]
        [XafDisplayName("Official Code")]
        [Index(0)]
        public string OfficialCode
        {
            get => officialCode;
            set => SetPropertyValue(nameof(OfficialCode), ref officialCode, value?.ToUpperInvariant());
        }

        string accountName = string.Empty;
        /// <summary>
        /// Name of the account
        /// </summary>
        [RuleRequiredField("Account_AccountName_Required", DefaultContexts.Save)]
        [Size(200)]
        [XafDisplayName("Account Name")]
        [Index(1)]
        public string AccountName
        {
            get => accountName;
            set => SetPropertyValue(nameof(AccountName), ref accountName, value);
        }

        AccountType accountType = AccountType.Asset;
        /// <summary>
        /// Type of account (asset, liability, equity, revenue, expense, settlement)
        /// </summary>
        [XafDisplayName("Account Type")]
        [Index(2)]
        public AccountType AccountType
        {
            get => accountType;
            set => SetPropertyValue(nameof(AccountType), ref accountType, value);
        }

        string parentOfficialCode = string.Empty;
        /// <summary>
        /// Official code/identifier for the parent account (for hierarchical structure)
        /// </summary>
        [Size(50)]
        [XafDisplayName("Parent Account Code")]
        [Index(3)]
        public string ParentOfficialCode
        {
            get => parentOfficialCode;
            set => SetPropertyValue(nameof(ParentOfficialCode), ref parentOfficialCode, value?.ToUpperInvariant());
        }

        Guid? balanceAndIncomeLineId;
        /// <summary>
        /// Optional reference to balance sheet or income statement line
        /// </summary>
        [XafDisplayName("Balance & Income Line")]
        [Index(4)]
        [Browsable(false)] // Hidden from UI unless specifically needed
        public Guid? BalanceAndIncomeLineId
        {
            get => balanceAndIncomeLineId;
            set => SetPropertyValue(nameof(BalanceAndIncomeLineId), ref balanceAndIncomeLineId, value);
        }

        bool isActive = true;
        /// <summary>
        /// Indicates whether this account is active and can be used
        /// </summary>
        [XafDisplayName("Active")]
        [Index(5)]
        public bool IsActive
        {
            get => isActive;
            set => SetPropertyValue(nameof(IsActive), ref isActive, value);
        }

        /// <summary>
        /// UTC timestamp when the entity was created
        /// </summary>
        [XafDisplayName("Inserted At")]
        [ModelDefault("AllowEdit", "False")]
        [ModelDefault("DisplayFormat", "G")]
        [Index(1000)]
        public DateTime InsertedAt { get; set; }

        /// <summary>
        /// User who created the entity
        /// </summary>
        [Size(100)]
        [XafDisplayName("Inserted By")]
        [ModelDefault("AllowEdit", "False")]
        [Index(1001)]
        public string InsertedBy { get; set; } = string.Empty;

        /// <summary>
        /// UTC timestamp when the entity was last updated
        /// </summary>
        [XafDisplayName("Updated At")]
        [ModelDefault("AllowEdit", "False")]
        [ModelDefault("DisplayFormat", "G")]
        [Index(1002)]
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// User who last updated the entity
        /// </summary>
        [Size(100)]
        [XafDisplayName("Updated By")]
        [ModelDefault("AllowEdit", "False")]
        [Index(1003)]
        public string UpdatedBy { get; set; } = string.Empty;

        /// <summary>
        /// Returns a string representation of the account
        /// </summary>
        public override string ToString()
        {
            return $"{OfficialCode} - {AccountName}";
        }

        /// <summary>
        /// Validation to ensure parent account exists when specified
        /// </summary>
        [RuleFromBoolProperty("Account_ParentExists", DefaultContexts.Save,
            "Parent account must exist if specified")]
        public bool IsParentAccountValid
        {
            get
            {
                if (string.IsNullOrWhiteSpace(ParentOfficialCode))
                    return true;

                // Check if parent account exists
                var parentAccount = Session.FindObject<Account>(
                    new DevExpress.Data.Filtering.BinaryOperator(
                        nameof(OfficialCode), ParentOfficialCode));
                
                return parentAccount != null;
            }
        }

        /// <summary>
        /// Validation to prevent self-referencing parent
        /// </summary>
        [RuleFromBoolProperty("Account_NoSelfReference", DefaultContexts.Save,
            "Account cannot be its own parent")]
        public bool IsNotSelfReferencing
        {
            get
            {
                return string.IsNullOrWhiteSpace(ParentOfficialCode) || 
                       !string.Equals(OfficialCode, ParentOfficialCode, StringComparison.OrdinalIgnoreCase);
            }
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            IsActive = true;
            InsertedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        protected override void OnSaving()
        {
            UpdatedAt = DateTime.UtcNow;
            base.OnSaving();
        }

        // Property required for IAccount interface but not exposed in UI
        string? IAccount.ParentOfficialCode 
        { 
            get => string.IsNullOrEmpty(ParentOfficialCode) ? null : ParentOfficialCode;
            set => ParentOfficialCode = value ?? string.Empty;
        }
    }
}