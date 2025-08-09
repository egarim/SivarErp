using System.ComponentModel;
using Sivar.Erp.Core.Modules.Domain;

namespace Sivar.Erp.Core.Modules.Accounting
{
    /// <summary>
    /// Service for managing accounting operations including transactions, ledger entries, and account balances
    /// </summary>
    [Description("Service for managing accounting operations")]
    public interface IAccountingService
    {
        /// <summary>
        /// Creates a transaction from a document with accounting entries based on document totals
        /// </summary>
        /// <param name="document">The source document for the transaction</param>
        /// <param name="description">Optional description for the transaction</param>
        /// <returns>A transaction ready for posting</returns>
        [Description("Creates a transaction from a document")]
        Task<ITransaction> CreateTransactionAsync(IDocument document, string? description = null);

        /// <summary>
        /// Posts a transaction to the ledger, making it permanent
        /// </summary>
        /// <param name="transaction">Transaction to post</param>
        /// <returns>Task representing the async operation</returns>
        [Description("Posts a transaction to the ledger")]
        Task PostTransactionAsync(ITransaction transaction);

        /// <summary>
        /// Reverses a previously posted transaction
        /// </summary>
        /// <param name="transaction">Transaction to reverse</param>
        /// <param name="reason">Reason for reversal</param>
        /// <returns>The reversal transaction</returns>
        [Description("Reverses a previously posted transaction")]
        Task<ITransaction> ReverseTransactionAsync(ITransaction transaction, string reason);

        /// <summary>
        /// Calculates account balance as of a specific date
        /// </summary>
        /// <param name="accountCode">Account code to query</param>
        /// <param name="asOfDate">Date for which to calculate the balance (optional, defaults to today)</param>
        /// <returns>The account balance</returns>
        [Description("Calculates account balances")]
        Task<decimal> CalculateAccountBalanceAsync(string accountCode, DateOnly? asOfDate = null);

        /// <summary>
        /// Gets all transactions for a specific account within a date range
        /// </summary>
        /// <param name="accountCode">Account code to query</param>
        /// <param name="fromDate">Start date for the query</param>
        /// <param name="toDate">End date for the query</param>
        /// <returns>Collection of transactions</returns>
        [Description("Gets all transactions for a specific account within a date range")]
        Task<IEnumerable<ITransaction>> GetAccountTransactionsAsync(string accountCode, DateOnly fromDate, DateOnly toDate);

        /// <summary>
        /// Gets all ledger entries for a specific account within a date range
        /// </summary>
        /// <param name="accountCode">Account code to query</param>
        /// <param name="fromDate">Start date for the query</param>
        /// <param name="toDate">End date for the query</param>
        /// <returns>Collection of ledger entries</returns>
        [Description("Gets all ledger entries for a specific account within a date range")]
        Task<IEnumerable<ILedgerEntry>> GetAccountLedgerEntriesAsync(string accountCode, DateOnly fromDate, DateOnly toDate);

        /// <summary>
        /// Validates a transaction before posting
        /// </summary>
        /// <param name="transaction">Transaction to validate</param>
        /// <returns>Validation result</returns>
        [Description("Validates a transaction before posting")]
        Task<ValidationResult> ValidateTransactionAsync(ITransaction transaction);

        /// <summary>
        /// Gets the trial balance for all accounts as of a specific date
        /// </summary>
        /// <param name="asOfDate">Date for which to get the trial balance</param>
        /// <returns>Collection of account balances</returns>
        [Description("Gets the trial balance for all accounts")]
        Task<IEnumerable<AccountBalance>> GetTrialBalanceAsync(DateOnly asOfDate);

        /// <summary>
        /// Creates a journal entry manually (not from a document)
        /// </summary>
        /// <param name="description">Description of the journal entry</param>
        /// <param name="entries">Collection of ledger entries</param>
        /// <param name="transactionDate">Date of the transaction</param>
        /// <returns>The created transaction</returns>
        [Description("Creates a journal entry manually")]
        Task<ITransaction> CreateJournalEntryAsync(string description, IEnumerable<ILedgerEntry> entries, DateOnly transactionDate);
    }

    /// <summary>
    /// Represents an account balance at a specific point in time
    /// </summary>
    [Description("Represents an account balance at a specific point in time")]
    public class AccountBalance
    {
        /// <summary>
        /// Account code
        /// </summary>
        [Description("Account code")]
        public string AccountCode { get; set; } = string.Empty;

        /// <summary>
        /// Account name
        /// </summary>
        [Description("Account name")]
        public string AccountName { get; set; } = string.Empty;

        /// <summary>
        /// Debit balance amount
        /// </summary>
        [Description("Debit balance amount")]
        public decimal DebitBalance { get; set; }

        /// <summary>
        /// Credit balance amount
        /// </summary>
        [Description("Credit balance amount")]
        public decimal CreditBalance { get; set; }

        /// <summary>
        /// Net balance (debit - credit)
        /// </summary>
        [Description("Net balance (debit - credit)")]
        public decimal NetBalance => DebitBalance - CreditBalance;

        /// <summary>
        /// Date as of which this balance was calculated
        /// </summary>
        [Description("Date as of which this balance was calculated")]
        public DateOnly AsOfDate { get; set; }
    }
}