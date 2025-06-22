using Sivar.Erp.Services.Accounting.Transactions;

namespace Sivar.Erp.Modules.Accounting.JournalEntries;

/// <summary>
/// Service for querying and analyzing journal entries (ledger entries)
/// </summary>
public interface IJournalEntryService
{
    /// <summary>
    /// Gets journal entries based on query options
    /// </summary>
    /// <param name="options">Query options for filtering and sorting</param>
    /// <returns>Collection of journal entries</returns>
    Task<IEnumerable<ILedgerEntry>> GetJournalEntriesAsync(JournalEntryQueryOptions options);

    /// <summary>
    /// Gets all journal entries for a specific transaction
    /// </summary>
    /// <param name="transactionNumber">Transaction number to filter by</param>
    /// <returns>Collection of journal entries for the transaction</returns>
    Task<IEnumerable<ILedgerEntry>> GetJournalEntriesByTransactionAsync(string transactionNumber);

    /// <summary>
    /// Gets a specific journal entry by its number
    /// </summary>
    /// <param name="ledgerEntryNumber">Ledger entry number</param>
    /// <returns>The journal entry or null if not found</returns>
    Task<ILedgerEntry?> GetJournalEntryByNumberAsync(string ledgerEntryNumber);

    /// <summary>
    /// Checks if a transaction has posted journal entries
    /// </summary>
    /// <param name="transactionNumber">Transaction number to check</param>
    /// <returns>True if transaction is posted</returns>
    Task<bool> IsTransactionPostedAsync(string transactionNumber);

    /// <summary>
    /// Gets the total debit amount for a transaction
    /// </summary>
    /// <param name="transactionNumber">Transaction number</param>
    /// <returns>Total debit amount</returns>
    Task<decimal> GetTransactionTotalDebitAsync(string transactionNumber);

    /// <summary>
    /// Gets the total credit amount for a transaction
    /// </summary>
    /// <param name="transactionNumber">Transaction number</param>
    /// <returns>Total credit amount</returns>
    Task<decimal> GetTransactionTotalCreditAsync(string transactionNumber);

    /// <summary>
    /// Checks if a transaction is balanced (total debits = total credits)
    /// </summary>
    /// <param name="transactionNumber">Transaction number</param>
    /// <returns>True if transaction is balanced</returns>
    Task<bool> IsTransactionBalancedAsync(string transactionNumber);

    /// <summary>
    /// Gets all account codes affected by a transaction
    /// </summary>
    /// <param name="transactionNumber">Transaction number</param>
    /// <returns>Collection of account codes</returns>
    Task<IEnumerable<string>> GetAffectedAccountsAsync(string transactionNumber);
}
