using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Sivar.Erp.Documents;
using Sivar.Erp.Services.Accounting.FiscalPeriods;
using Sivar.Erp.Services.Accounting.Transactions;
using Sivar.Erp.Modules.Accounting.JournalEntries;
using Sivar.Erp.Modules.Accounting.Reports;

namespace Sivar.Erp.Modules.Accounting
{
    /// <summary>
    /// Interface for accounting operations that other modules can use to record financial transactions
    /// and manage fiscal periods
    /// </summary>
    public interface IAccountingModule
    {        /// <summary>
             /// Creates a transaction from a document with accounting entries based on document totals
             /// </summary>
             /// <param name="document">The source document for the transaction</param>
             /// <param name="description">Optional description for the transaction</param>
             /// <returns>A transaction ready for posting</returns>
        Task<ITransaction> CreateTransactionFromDocumentAsync(IDocument document, string? description = null);

        /// <summary>
        /// Posts a transaction after validating fiscal period is open
        /// </summary>
        /// <param name="transaction">Transaction to post</param>
        /// <returns>True if posted successfully</returns>
        Task<bool> PostTransactionAsync(ITransaction transaction);

        /// <summary>
        /// Unposts a previously posted transaction if fiscal period is still open
        /// </summary>
        /// <param name="transaction">Transaction to unpost</param>
        /// <returns>True if unposted successfully</returns>
        Task<bool> UnPostTransactionAsync(ITransaction transaction);

        /// <summary>
        /// Checks if a transaction is balanced (total debits = total credits) and valid for posting
        /// </summary>
        /// <param name="transaction">Transaction to validate</param>
        /// <returns>True if the transaction is valid</returns>
        Task<bool> ValidateTransactionAsync(ITransaction transaction);

        /// <summary>
        /// Gets the balance of an account as of a specific date
        /// </summary>
        /// <param name="accountCode">Account code to query</param>
        /// <param name="asOfDate">Date for which to get the balance</param>
        /// <returns>The account balance</returns>
        Task<decimal> GetAccountBalanceAsync(string accountCode, DateOnly asOfDate);

        /// <summary>
        /// Opens a fiscal period to allow transaction posting
        /// </summary>
        /// <param name="periodCode">Code of the fiscal period to open</param>
        /// <param name="userId">User opening the period</param>
        /// <returns>True if opened successfully</returns>
        /// <exception cref="InvalidOperationException">Thrown when fiscal period doesn't exist</exception>
        Task<bool> OpenFiscalPeriodAsync(string periodCode, string userId);

        /// <summary>
        /// Closes a fiscal period to prevent further transaction posting
        /// </summary>
        /// <param name="periodCode">Code of the fiscal period to close</param>
        /// <param name="userId">User closing the period</param>
        /// <returns>True if closed successfully</returns>
        /// <exception cref="InvalidOperationException">Thrown when fiscal period doesn't exist</exception>
        Task<bool> CloseFiscalPeriodAsync(string periodCode, string userId);

        /// <summary>
        /// Checks if a date falls within an open fiscal period
        /// </summary>
        /// <param name="date">Date to check</param>
        /// <returns>True if date is in an open fiscal period, false otherwise</returns>
        Task<bool> IsDateInOpenFiscalPeriodAsync(DateOnly date);
        /// <summary>
        /// Gets the fiscal period service for advanced operations
        /// </summary>
        /// <returns>The fiscal period service</returns>
        IFiscalPeriodService GetFiscalPeriodService();

        // Journal Entry Operations

        /// <summary>
        /// Gets journal entries for a specific transaction
        /// </summary>
        /// <param name="transactionNumber">Transaction number</param>
        /// <returns>Collection of journal entries</returns>
        Task<IEnumerable<ILedgerEntry>> GetTransactionJournalEntriesAsync(string transactionNumber);

        /// <summary>
        /// Validates if a transaction is balanced (total debits = total credits)
        /// </summary>
        /// <param name="transactionNumber">Transaction number</param>
        /// <returns>True if transaction is balanced</returns>
        Task<bool> ValidateTransactionBalanceAsync(string transactionNumber);

        /// <summary>
        /// Gets journal entries based on query criteria
        /// </summary>
        /// <param name="options">Query options for filtering</param>
        /// <returns>Collection of journal entries</returns>
        Task<IEnumerable<ILedgerEntry>> GetJournalEntriesAsync(JournalEntryQueryOptions options);

        /// <summary>
        /// Generates a journal entry report
        /// </summary>
        /// <param name="options">Query options for the report</param>
        /// <returns>Journal entry report data</returns>
        Task<JournalEntryReportDto> GenerateJournalReportAsync(JournalEntryQueryOptions options);

        /// <summary>
        /// Generates a transaction audit trail
        /// </summary>
        /// <param name="transactionNumber">Transaction number</param>
        /// <returns>Transaction audit trail data</returns>
        Task<TransactionAuditTrailDto> GenerateTransactionAuditTrailAsync(string transactionNumber);
    }
}