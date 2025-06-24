using Microsoft.Extensions.Logging;
using Sivar.Erp.Services;
using Sivar.Erp.Services.Accounting.Transactions;
using Sivar.Erp.Modules.Accounting.JournalEntries;
using Sivar.Erp.ErpSystem.Diagnostics;

namespace Sivar.Erp.Modules.Accounting.Reports;

/// <summary>
/// Service implementation for generating journal entry and transaction reports
/// </summary>
public class JournalEntryReportService : IJournalEntryReportService
{
    private readonly IObjectDb _objectDb;
    private readonly IJournalEntryService _journalEntryService;
    private readonly PerformanceLogger<JournalEntryReportService> _performanceLogger;

    public JournalEntryReportService(ILogger<JournalEntryReportService> logger, IObjectDb objectDb, IJournalEntryService journalEntryService)
    {
        _objectDb = objectDb;
        _journalEntryService = journalEntryService;
        _performanceLogger = new PerformanceLogger<JournalEntryReportService>(logger, PerformanceLogMode.All, 100, 10_000_000, objectDb);
    }
    public async Task<JournalEntryReportDto> GenerateJournalEntryReportAsync(JournalEntryQueryOptions options)
    {
        return await _performanceLogger.Track(nameof(GenerateJournalEntryReportAsync), async () =>
        {
            var entries = await _journalEntryService.GetJournalEntriesAsync(options);
            var entriesList = entries.ToList();

            var totalDebits = entriesList
                .Where(e => e.EntryType == EntryType.Debit)
                .Sum(e => e.Amount);

            var totalCredits = entriesList
                .Where(e => e.EntryType == EntryType.Credit)
                .Sum(e => e.Amount);

            return new JournalEntryReportDto
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
        });
    }
    public async Task<TransactionAuditTrailDto> GenerateTransactionAuditTrailAsync(string transactionNumber)
    {
        return await _performanceLogger.Track(nameof(GenerateTransactionAuditTrailAsync), async () =>
        {
            var transaction = _objectDb.Transactions
                .FirstOrDefault(t => t.TransactionNumber == transactionNumber);

            if (transaction == null)
            {
                return new TransactionAuditTrailDto
                {
                    TransactionNumber = transactionNumber,
                    GeneratedAt = DateTime.UtcNow
                };
            }

            var entries = await _journalEntryService.GetJournalEntriesByTransactionAsync(transactionNumber);
            var entriesList = entries.ToList();

            var totalDebits = entriesList
                .Where(e => e.EntryType == EntryType.Debit)
                .Sum(e => e.Amount);

            var totalCredits = entriesList
                .Where(e => e.EntryType == EntryType.Credit)
                .Sum(e => e.Amount);

            var affectedAccounts = await _journalEntryService.GetAffectedAccountsAsync(transactionNumber);

            return new TransactionAuditTrailDto
            {
                TransactionNumber = transactionNumber,
                DocumentNumber = transaction.DocumentNumber,
                TransactionDate = transaction.TransactionDate,
                Description = transaction.Description,
                IsPosted = transaction.IsPosted,
                JournalEntries = entriesList,
                TotalDebits = totalDebits,
                TotalCredits = totalCredits,
                AffectedAccounts = affectedAccounts,
                GeneratedAt = DateTime.UtcNow
            };
        });
    }
    public async Task<AccountActivityReportDto> GenerateAccountActivityReportAsync(string accountCode, DateOnly fromDate, DateOnly toDate)
    {
        return await _performanceLogger.Track(nameof(GenerateAccountActivityReportAsync), async () =>
        {
            var account = _objectDb.Accounts.FirstOrDefault(a => a.OfficialCode == accountCode);
            var accountName = account?.AccountName ?? "Unknown Account";

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

            // Get opening balance (entries before fromDate)
            var openingBalanceOptions = new JournalEntryQueryOptions
            {
                AccountCode = accountCode,
                ToDate = fromDate.AddDays(-1),
                OnlyPosted = true
            };

            var openingEntries = await _journalEntryService.GetJournalEntriesAsync(openingBalanceOptions);
            var openingDebits = openingEntries.Where(e => e.EntryType == EntryType.Debit).Sum(e => e.Amount);
            var openingCredits = openingEntries.Where(e => e.EntryType == EntryType.Credit).Sum(e => e.Amount);
            var openingBalance = openingDebits - openingCredits;

            var closingBalance = openingBalance + totalDebits - totalCredits;

            var uniqueTransactions = entriesList
                .Select(e => e.TransactionNumber)
                .Distinct()
                .Count();

            return new AccountActivityReportDto
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
        });
    }
    public async Task<TrialBalanceReportDto> GenerateTrialBalanceFromJournalEntriesAsync(DateOnly asOfDate, bool onlyPosted = true)
    {
        return await _performanceLogger.Track(nameof(GenerateTrialBalanceFromJournalEntriesAsync), async () =>
        {
            var options = new JournalEntryQueryOptions
            {
                ToDate = asOfDate,
                OnlyPosted = onlyPosted
            };

            var entries = await _journalEntryService.GetJournalEntriesAsync(options);
            var entriesList = entries.ToList();

            // Group by account
            var accountGroups = entriesList
                .GroupBy(e => e.OfficialCode)
                .Select(g => new TrialBalanceAccountDto
                {
                    AccountCode = g.Key,
                    AccountName = _objectDb.Accounts.FirstOrDefault(a => a.OfficialCode == g.Key)?.AccountName ?? "Unknown",
                    DebitBalance = g.Where(e => e.EntryType == EntryType.Debit).Sum(e => e.Amount),
                    CreditBalance = g.Where(e => e.EntryType == EntryType.Credit).Sum(e => e.Amount)
                })
                .Where(a => a.DebitBalance != 0 || a.CreditBalance != 0)
                .OrderBy(a => a.AccountCode)
                .ToList();

            var totalDebits = accountGroups.Sum(a => a.DebitBalance);
            var totalCredits = accountGroups.Sum(a => a.CreditBalance);

            return new TrialBalanceReportDto
            {
                AsOfDate = asOfDate,
                OnlyPostedTransactions = onlyPosted,
                Accounts = accountGroups,
                TotalDebits = totalDebits,
                TotalCredits = totalCredits,
                GeneratedAt = DateTime.UtcNow
            };
        });
    }

    private string BuildReportTitle(JournalEntryQueryOptions options)
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
}
