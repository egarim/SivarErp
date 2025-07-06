using Sivar.Erp.Core.Enums;

namespace Sivar.Erp.Modules.Accounting.Domain.ChartOfAccounts
{
    /// <summary>
    /// Implementation of an account in the chart of accounts
    /// </summary>
    public class AccountDto : IAccount
    {
        ///// <summary>
        ///// Unique identifier for the account
        ///// </summary>
        //public Guid Oid { get; set; }

        /// <summary>
        /// Optional reference to balance sheet or income statement line
        /// </summary>
        public Guid? BalanceAndIncomeLineId { get; set; }

        /// <summary>
        /// Name of the account
        /// </summary>
        public string AccountName { get; set; } = string.Empty;

        /// <summary>
        /// Type of account (asset, liability, etc.)
        /// </summary>
        public AccountType AccountType { get; set; }

        /// <summary>
        /// Official code/identifier for the account (e.g., for SAF-T reporting)
        /// </summary>
        public string OfficialCode { get; set; } = string.Empty;

        /// <summary>
        /// UTC timestamp when the account was created
        /// </summary>
        public DateTime InsertedAt { get; set; }

        /// <summary>
        /// User who created the account
        /// </summary>
        public string InsertedBy { get; set; } = string.Empty;

        /// <summary>
        /// UTC timestamp when the account was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// User who last updated the account
        /// </summary>
        public string UpdatedBy { get; set; } = string.Empty;

        /// <summary>
        /// Indicates whether the account is archived
        /// </summary>
        public bool IsArchived { get; set; }

        /// <summary>
        /// Official code/identifier for the parent account 
        /// </summary>
        public string? ParentOfficialCode { get; set; }

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

        /// <summary>
        /// Archives the account
        /// </summary>
        public void Archive()
        {
            IsArchived = true;
        }

        /// <summary>
        /// Restores a previously archived account
        /// </summary>
        public void Restore()
        {
            IsArchived = false;
        }
    }
}