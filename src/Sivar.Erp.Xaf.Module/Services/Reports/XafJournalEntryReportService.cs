using DevExpress.ExpressApp;
using DevExpress.Data.Filtering;
using Microsoft.Extensions.Logging;
using Sivar.Erp.Core.Enums;
using Sivar.Erp.EfCore.Entities;
using Sivar.Erp.Modules.Accounting.JournalEntries;
using Sivar.Erp.Modules.Accounting.Reports;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sivar.Erp.Modules.Accounting.Transactions;

#nullable enable

namespace Sivar.Erp.Xaf.Module.Services.Reports
{
    /// <summary>
    /// XAF implementation of journal entry report service using IObjectSpace
    /// </summary>
    public class XafJournalEntryReportService : IJournalEntryReportService
    {
        private readonly IObjectSpace _objectSpace;
        private readonly IJournalEntryService _journalEntryService;
        private readonly ILogger<XafJournalEntryReportService>? _logger;

        /// <summary>
        /// Initializes a new instance of the XafJournalEntryReportService class
        /// </summary>
        /// <param name="objectSpace">XAF ObjectSpace for data operations</param>
        /// <param name="journalEntryService">Journal entry service for data access</param>
        /// <param name="logger">Optional logger for diagnostic information</param>
        public XafJournalEntryReportService(
            IObjectSpace objectSpace, 
            IJournalEntryService journalEntryService,
            ILogger<XafJournalEntryReportService>? logger = null)
        {
            _objectSpace = objectSpace ?? throw new ArgumentNullException(nameof(objectSpace));
            _journalEntryService = journalEntryService ?? throw new ArgumentNullException(nameof(journalEntryService));
            _logger = logger;
        }

        /// <summary>
        /// Generates a comprehensive journal entry report
        /// </summary>
        /// <param name="options">Query options for the report</param>
        /// <returns>Journal entry report data</returns>
        public async Task<JournalEntryReportDto> GenerateJournalEntryReportAsync(JournalEntryQueryOptions options)
        {
            try
            {
                _logger?.LogDebug("Generating journal entry report with options: OnlyPosted={OnlyPosted}, AccountCode={AccountCode}, TransactionNumber={TransactionNumber}",
                    options.OnlyPosted, options.AccountCode, options.TransactionNumber);

                var entries = await _journalEntryService.GetJournalEntriesAsync(options);
                var entriesList = entries.ToList();

                var totalDebits = entriesList
                    .Where(e => e.EntryType == EntryType.Debit)
                    .Sum(e => e.Amount);

                var totalCredits = entriesList
                    .Where(e => e.EntryType == EntryType.Credit)
                    .Sum(e => e.Amount);

                var report = new JournalEntryReportDto
                {
                    ReportTitle = BuildReportTitle(options),
                    FromDate = options.FromDate,
                    ToDate = options.ToDate,
                    AccountCodeFilter = options.AccountCode,
                    TransactionNumberFilter = options.TransactionNumber,
                    Entries = entriesList,
                    TotalDebits = totalDebits,
                    TotalCredits = totalCredits,
                    TotalEntries = entriesList.Count,
                    GeneratedAt = DateTime.UtcNow
                };

                _logger?.LogDebug("Generated journal entry report with {Count} entries, TotalDebits: {TotalDebits}, TotalCredits: {TotalCredits}",
                    entriesList.Count, totalDebits, totalCredits);

                return report;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating journal entry report");
                throw;
            }
        }

        /// <summary>
        /// Generates an audit trail for a specific transaction
        /// </summary>
        /// <param name="transactionNumber">Transaction number</param>
        /// <returns>Transaction audit trail data</returns>
        public async Task<TransactionAuditTrailDto> GenerateTransactionAuditTrailAsync(string transactionNumber)
        {
            try
            {
                _logger?.LogDebug("Generating transaction audit trail for: {TransactionNumber}", transactionNumber);

                if (string.IsNullOrWhiteSpace(transactionNumber))
                {
                    _logger?.LogWarning("Transaction number is null or empty");
                    return new TransactionAuditTrailDto
                    {
                        TransactionNumber = transactionNumber ?? string.Empty,
                        GeneratedAt = DateTime.UtcNow
                    };
                }

                // Get transaction details
                var transactionCriteria = new BinaryOperator("TransactionNumber", transactionNumber);
                var transaction = _objectSpace.FindObject<Transaction>(transactionCriteria);

                if (transaction == null)
                {
                    _logger?.LogWarning("Transaction not found: {TransactionNumber}", transactionNumber);
                    return new TransactionAuditTrailDto
                    {
                        TransactionNumber = transactionNumber,
                        GeneratedAt = DateTime.UtcNow
                    };
                }

                // Get journal entries for the transaction
                var entries = await _journalEntryService.GetJournalEntriesByTransactionAsync(transactionNumber);
                var entriesList = entries.ToList();

                var totalDebits = entriesList
                    .Where(e => e.EntryType == EntryType.Debit)
                    .Sum(e => e.Amount);

                var totalCredits = entriesList
                    .Where(e => e.EntryType == EntryType.Credit)
                    .Sum(e => e.Amount);

                var affectedAccounts = await _journalEntryService.GetAffectedAccountsAsync(transactionNumber);

                var auditTrail = new TransactionAuditTrailDto
                {
                    TransactionNumber = transactionNumber,
                    DocumentNumber = transaction.DocumentNumber ?? string.Empty,
                    TransactionDate = transaction.TransactionDate,
                    Description = transaction.Description ?? string.Empty,
                    IsPosted = transaction.IsPosted,
                    JournalEntries = entriesList,
                    TotalDebits = totalDebits,
                    TotalCredits = totalCredits,
                    AffectedAccounts = affectedAccounts,
                    GeneratedAt = DateTime.UtcNow
                };

                _logger?.LogDebug("Generated audit trail for transaction {TransactionNumber} with {Count} entries, {AccountsCount} affected accounts",
                    transactionNumber, entriesList.Count, affectedAccounts.Count());

                return auditTrail;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating transaction audit trail for: {TransactionNumber}", transactionNumber);
                throw;
            }
        }

        /// <summary>
        /// Generates an account activity report showing all journal entries for an account
        /// </summary>
        /// <param name="accountCode">Account code</param>
        /// <param name="fromDate">Start date</param>
        /// <param name="toDate">End date</param>
        /// <returns>Account activity report data</returns>
        public async Task<AccountActivityReportDto> GenerateAccountActivityReportAsync(string accountCode, DateOnly fromDate, DateOnly toDate)
        {
            try
            {
                _logger?.LogDebug("Generating account activity report for account: {AccountCode}, from: {FromDate}, to: {ToDate}",
                    accountCode, fromDate, toDate);

                if (string.IsNullOrWhiteSpace(accountCode))
                {
                    _logger?.LogWarning("Account code is null or empty");
                    return new AccountActivityReportDto
                    {
                        AccountCode = accountCode ?? string.Empty,
                        AccountName = "Unknown Account",
                        FromDate = fromDate,
                        ToDate = toDate,
                        GeneratedAt = DateTime.UtcNow
                    };
                }

                // Get account details
                var accountCriteria = new BinaryOperator("OfficialCode", accountCode);
                var account = _objectSpace.FindObject<Account>(accountCriteria);
                var accountName = account?.AccountName ?? "Unknown Account";

                // Get entries for the period
                var options = new JournalEntryQueryOptions
                {
                    AccountCode = accountCode,
                    FromDate = fromDate,
                    ToDate = toDate,
                    OnlyPosted = true
                };

                var entries = await _journalEntryService.GetJournalEntriesAsync(options);
                var entriesList = entries.ToList();

                var totalDebits = entriesList
                    .Where(e => e.EntryType == EntryType.Debit)
                    .Sum(e => e.Amount);

                var totalCredits = entriesList
                    .Where(e => e.EntryType == EntryType.Credit)
                    .Sum(e => e.Amount);

                // Calculate opening balance (entries before fromDate)
                var openingBalance = await CalculateOpeningBalanceAsync(accountCode, fromDate);

                var closingBalance = openingBalance + totalDebits - totalCredits;

                var uniqueTransactions = entriesList
                    .Select(e => e.TransactionNumber)
                    .Distinct()
                    .Count();

                var report = new AccountActivityReportDto
                {
                    AccountCode = accountCode,
                    AccountName = accountName,
                    FromDate = fromDate,
                    ToDate = toDate,
                    OpeningBalance = openingBalance,
                    ClosingBalance = closingBalance,
                    Entries = entriesList,
                    TotalDebits = totalDebits,
                    TotalCredits = totalCredits,
                    TotalTransactions = uniqueTransactions,
                    GeneratedAt = DateTime.UtcNow
                };

                _logger?.LogDebug("Generated account activity report for {AccountCode} with {Count} entries, OpeningBalance: {OpeningBalance}, ClosingBalance: {ClosingBalance}",
                    accountCode, entriesList.Count, openingBalance, closingBalance);

                return report;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating account activity report for: {AccountCode}", accountCode);
                throw;
            }
        }

        /// <summary>
        /// Generates a trial balance report from journal entries
        /// </summary>
        /// <param name="asOfDate">Date for the trial balance</param>
        /// <param name="onlyPosted">Whether to include only posted transactions</param>
        /// <returns>Trial balance report data</returns>
        public async Task<TrialBalanceReportDto> GenerateTrialBalanceFromJournalEntriesAsync(DateOnly asOfDate, bool onlyPosted = true)
        {
            try
            {
                _logger?.LogDebug("Generating trial balance report as of: {AsOfDate}, onlyPosted: {OnlyPosted}", asOfDate, onlyPosted);

                var options = new JournalEntryQueryOptions
                {
                    ToDate = asOfDate,
                    OnlyPosted = onlyPosted
                };

                var entries = await _journalEntryService.GetJournalEntriesAsync(options);
                var entriesList = entries.ToList();

                // Group by account and calculate balances
                var accountGroups = entriesList
                    .GroupBy(e => e.OfficialCode)
                    .Where(g => !string.IsNullOrEmpty(g.Key))
                    .Select(g => new TrialBalanceAccountDto
                    {
                        AccountCode = g.Key,
                        AccountName = GetAccountName(g.Key),
                        DebitBalance = g.Where(e => e.EntryType == EntryType.Debit).Sum(e => e.Amount),
                        CreditBalance = g.Where(e => e.EntryType == EntryType.Credit).Sum(e => e.Amount)
                    })
                    .Where(a => a.DebitBalance != 0 || a.CreditBalance != 0)
                    .OrderBy(a => a.AccountCode)
                    .ToList();

                var totalDebits = accountGroups.Sum(a => a.DebitBalance);
                var totalCredits = accountGroups.Sum(a => a.CreditBalance);

                var report = new TrialBalanceReportDto
                {
                    AsOfDate = asOfDate,
                    OnlyPostedTransactions = onlyPosted,
                    Accounts = accountGroups,
                    TotalDebits = totalDebits,
                    TotalCredits = totalCredits,
                    GeneratedAt = DateTime.UtcNow
                };

                _logger?.LogDebug("Generated trial balance report with {Count} accounts, TotalDebits: {TotalDebits}, TotalCredits: {TotalCredits}, IsBalanced: {IsBalanced}",
                    accountGroups.Count, totalDebits, totalCredits, report.IsBalanced);

                return report;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating trial balance report");
                throw;
            }
        }

        /// <summary>
        /// Builds a descriptive report title based on query options
        /// </summary>
        /// <param name="options">Query options</param>
        /// <returns>Report title</returns>
        private static string BuildReportTitle(JournalEntryQueryOptions options)
        {
            var title = "Journal Entry Report";

            if (options.FromDate.HasValue || options.ToDate.HasValue)
            {
                title += " - ";
                if (options.FromDate.HasValue && options.ToDate.HasValue)
                    title += $"{options.FromDate.Value:yyyy-MM-dd} to {options.ToDate.Value:yyyy-MM-dd}";
                else if (options.FromDate.HasValue)
                    title += $"From {options.FromDate.Value:yyyy-MM-dd}";
                else
                    title += $"Up to {options.ToDate.Value:yyyy-MM-dd}";
            }

            if (!string.IsNullOrEmpty(options.AccountCode))
                title += $" - Account: {options.AccountCode}";

            if (!string.IsNullOrEmpty(options.TransactionNumber))
                title += $" - Transaction: {options.TransactionNumber}";

            return title;
        }

        /// <summary>
        /// Gets account name by account code using XAF ObjectSpace
        /// </summary>
        /// <param name="accountCode">Account code</param>
        /// <returns>Account name or "Unknown" if not found</returns>
        private string GetAccountName(string accountCode)
        {
            try
            {
                if (string.IsNullOrEmpty(accountCode))
                    return "Unknown";

                var criteria = new BinaryOperator("OfficialCode", accountCode);
                var account = _objectSpace.FindObject<Account>(criteria);
                return account?.AccountName ?? "Unknown";
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Error getting account name for code: {AccountCode}", accountCode);
                return "Unknown";
            }
        }

        /// <summary>
        /// Calculates opening balance for an account before a specific date
        /// </summary>
        /// <param name="accountCode">Account code</param>
        /// <param name="beforeDate">Date to calculate opening balance before</param>
        /// <returns>Opening balance</returns>
        private async Task<decimal> CalculateOpeningBalanceAsync(string accountCode, DateOnly beforeDate)
        {
            try
            {
                var openingBalanceOptions = new JournalEntryQueryOptions
                {
                    AccountCode = accountCode,
                    ToDate = beforeDate.AddDays(-1),
                    OnlyPosted = true
                };

                var openingEntries = await _journalEntryService.GetJournalEntriesAsync(openingBalanceOptions);
                var openingEntriesList = openingEntries.ToList();

                var openingDebits = openingEntriesList
                    .Where(e => e.EntryType == EntryType.Debit)
                    .Sum(e => e.Amount);

                var openingCredits = openingEntriesList
                    .Where(e => e.EntryType == EntryType.Credit)
                    .Sum(e => e.Amount);

                return openingDebits - openingCredits;
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Error calculating opening balance for account: {AccountCode}", accountCode);
                return 0m;
            }
        }
    }
}
