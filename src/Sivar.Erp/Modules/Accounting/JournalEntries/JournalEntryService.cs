using Sivar.Erp.Services;
using Sivar.Erp.Services.Accounting.Transactions;
using Microsoft.Extensions.Logging;
using Sivar.Erp.ErpSystem.Diagnostics;

namespace Sivar.Erp.Modules.Accounting.JournalEntries;

/// <summary>
/// Service implementation for querying and analyzing journal entries
/// </summary>
public class JournalEntryService : IJournalEntryService
{
    private readonly PerformanceLogger<JournalEntryService> _performanceLogger;
    private readonly IObjectDb _objectDb;

    public JournalEntryService(ILogger<JournalEntryService> logger, IObjectDb objectDb)
    {
        _objectDb = objectDb;
        _performanceLogger = new PerformanceLogger<JournalEntryService>(logger, PerformanceLogMode.All, 100, 10_000_000, objectDb);
    }
    public async Task<IEnumerable<ILedgerEntry>> GetJournalEntriesAsync(JournalEntryQueryOptions options)
    {
        return await _performanceLogger.Track(nameof(GetJournalEntriesAsync), async () =>
        {
            var query = _objectDb.LedgerEntries.AsQueryable();

            // Apply filters
            if (options.OnlyPosted.HasValue && options.OnlyPosted.Value)
            {
                var postedTransactionNumbers = _objectDb.Transactions
                    .Where(t => t.IsPosted)
                    .Select(t => t.TransactionNumber)
                    .ToHashSet();

                query = query.Where(e => postedTransactionNumbers.Contains(e.TransactionNumber));
            }

            if (!string.IsNullOrEmpty(options.AccountCode))
                query = query.Where(e => e.OfficialCode == options.AccountCode);

            if (!string.IsNullOrEmpty(options.TransactionNumber))
                query = query.Where(e => e.TransactionNumber == options.TransactionNumber);

            if (!string.IsNullOrEmpty(options.DocumentNumber))
            {
                var transactionNumbers = _objectDb.Transactions
                    .Where(t => t.DocumentNumber == options.DocumentNumber)
                    .Select(t => t.TransactionNumber)
                    .ToHashSet();

                query = query.Where(e => transactionNumbers.Contains(e.TransactionNumber));
            }

            if (options.EntryType.HasValue)
                query = query.Where(e => e.EntryType == options.EntryType.Value);

            // Date filtering requires matching transaction dates
            if (options.FromDate.HasValue || options.ToDate.HasValue)
            {
                var transactionQuery = _objectDb.Transactions.AsQueryable();

                if (options.FromDate.HasValue)
                    transactionQuery = transactionQuery.Where(t => t.TransactionDate >= options.FromDate.Value);

                if (options.ToDate.HasValue)
                    transactionQuery = transactionQuery.Where(t => t.TransactionDate <= options.ToDate.Value);

                var validTransactionNumbers = transactionQuery
                    .Select(t => t.TransactionNumber)
                    .ToHashSet();

                query = query.Where(e => validTransactionNumbers.Contains(e.TransactionNumber));
            }

            // Apply sorting
            query = options.SortBy?.ToLower() switch
            {
                "transactionnumber" => options.SortDescending ?
                    query.OrderByDescending(e => e.TransactionNumber) :
                    query.OrderBy(e => e.TransactionNumber),
                "accountcode" => options.SortDescending ?
                    query.OrderByDescending(e => e.OfficialCode) :
                    query.OrderBy(e => e.OfficialCode),
                "amount" => options.SortDescending ?
                    query.OrderByDescending(e => e.Amount) :
                    query.OrderBy(e => e.Amount),
                _ => options.SortDescending ?
                    query.OrderByDescending(e => e.LedgerEntryNumber) :
                    query.OrderBy(e => e.LedgerEntryNumber)
            };

            // Apply pagination
            if (options.Skip.HasValue)
                query = query.Skip(options.Skip.Value);

            if (options.Take.HasValue)
                query = query.Take(options.Take.Value);

            return await Task.FromResult(query.ToList());
        });
    }
    public async Task<IEnumerable<ILedgerEntry>> GetJournalEntriesByTransactionAsync(string transactionNumber)
    {
        return await _performanceLogger.Track(nameof(GetJournalEntriesByTransactionAsync), async () =>
        {
            var entries = _objectDb.LedgerEntries
                .Where(e => e.TransactionNumber == transactionNumber)
                .OrderBy(e => e.LedgerEntryNumber)
                .ToList();

            return await Task.FromResult(entries);
        });
    }
    public async Task<ILedgerEntry?> GetJournalEntryByNumberAsync(string ledgerEntryNumber)
    {
        return await _performanceLogger.Track(nameof(GetJournalEntryByNumberAsync), async () =>
        {
            var entry = _objectDb.LedgerEntries
                .FirstOrDefault(e => e.LedgerEntryNumber == ledgerEntryNumber);

            return await Task.FromResult(entry);
        });
    }
    public async Task<bool> IsTransactionPostedAsync(string transactionNumber)
    {
        return await _performanceLogger.Track(nameof(IsTransactionPostedAsync), async () =>
        {
            var transaction = _objectDb.Transactions
                .FirstOrDefault(t => t.TransactionNumber == transactionNumber);

            return await Task.FromResult(transaction?.IsPosted ?? false);
        });
    }
    public async Task<decimal> GetTransactionTotalDebitAsync(string transactionNumber)
    {
        return await _performanceLogger.Track(nameof(GetTransactionTotalDebitAsync), async () =>
        {
            var total = _objectDb.LedgerEntries
                .Where(e => e.TransactionNumber == transactionNumber && e.EntryType == EntryType.Debit)
                .Sum(e => e.Amount);

            return await Task.FromResult(total);
        });
    }
    public async Task<decimal> GetTransactionTotalCreditAsync(string transactionNumber)
    {
        return await _performanceLogger.Track(nameof(GetTransactionTotalCreditAsync), async () =>
        {
            var total = _objectDb.LedgerEntries
                .Where(e => e.TransactionNumber == transactionNumber && e.EntryType == EntryType.Credit)
                .Sum(e => e.Amount);

            return await Task.FromResult(total);
        });
    }
    public async Task<bool> IsTransactionBalancedAsync(string transactionNumber)
    {
        return await _performanceLogger.Track(nameof(IsTransactionBalancedAsync), async () =>
        {
            var totalDebit = await GetTransactionTotalDebitAsync(transactionNumber);
            var totalCredit = await GetTransactionTotalCreditAsync(transactionNumber);

            return Math.Abs(totalDebit - totalCredit) < 0.01m; // Allow for minor rounding differences
        });
    }
    public async Task<IEnumerable<string>> GetAffectedAccountsAsync(string transactionNumber)
    {
        return await _performanceLogger.Track(nameof(GetAffectedAccountsAsync), async () =>
        {
            var accounts = _objectDb.LedgerEntries
                .Where(e => e.TransactionNumber == transactionNumber)
                .Select(e => e.OfficialCode)
                .Distinct()
                .ToList();

            return await Task.FromResult(accounts);
        });
    }
}
