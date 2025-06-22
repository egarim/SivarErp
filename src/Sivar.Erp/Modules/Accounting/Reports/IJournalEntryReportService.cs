using Sivar.Erp.Services.Accounting.Transactions;

namespace Sivar.Erp.Modules.Accounting.Reports;

/// <summary>
/// Service for generating journal entry and transaction reports
/// </summary>
public interface IJournalEntryReportService
{
    /// <summary>
    /// Generates a comprehensive journal entry report
    /// </summary>
    /// <param name="options">Query options for the report</param>
    /// <returns>Journal entry report data</returns>
    Task<JournalEntryReportDto> GenerateJournalEntryReportAsync(JournalEntries.JournalEntryQueryOptions options);

    /// <summary>
    /// Generates an audit trail for a specific transaction
    /// </summary>
    /// <param name="transactionNumber">Transaction number</param>
    /// <returns>Transaction audit trail data</returns>
    Task<TransactionAuditTrailDto> GenerateTransactionAuditTrailAsync(string transactionNumber);

    /// <summary>
    /// Generates an account activity report showing all journal entries for an account
    /// </summary>
    /// <param name="accountCode">Account code</param>
    /// <param name="fromDate">Start date</param>
    /// <param name="toDate">End date</param>
    /// <returns>Account activity report data</returns>
    Task<AccountActivityReportDto> GenerateAccountActivityReportAsync(string accountCode, DateOnly fromDate, DateOnly toDate);

    /// <summary>
    /// Generates a trial balance report from journal entries
    /// </summary>
    /// <param name="asOfDate">Date for the trial balance</param>
    /// <param name="onlyPosted">Whether to include only posted transactions</param>
    /// <returns>Trial balance report data</returns>
    Task<TrialBalanceReportDto> GenerateTrialBalanceFromJournalEntriesAsync(DateOnly asOfDate, bool onlyPosted = true);
}
