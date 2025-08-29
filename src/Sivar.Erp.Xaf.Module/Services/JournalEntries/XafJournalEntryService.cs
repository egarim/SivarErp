using DevExpress.ExpressApp;
using DevExpress.Data.Filtering;
using Microsoft.Extensions.Logging;
using Sivar.Erp.Core.Enums;
using Sivar.Erp.EfCore.Entities;
using Sivar.Erp.Modules.Accounting.JournalEntries;
using Sivar.Erp.Modules.Accounting.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#nullable enable

namespace Sivar.Erp.Xaf.Module.Services.JournalEntries
{
    /// <summary>
    /// XAF implementation of journal entry service using IObjectSpace
    /// </summary>
    public class XafJournalEntryService : IJournalEntryService
    {
        private readonly IObjectSpace _objectSpace;
        private readonly ILogger<XafJournalEntryService>? _logger;

        /// <summary>
        /// Initializes a new instance of the XafJournalEntryService class
        /// </summary>
        /// <param name="objectSpace">XAF ObjectSpace for data operations</param>
        /// <param name="logger">Optional logger for diagnostic information</param>
        public XafJournalEntryService(IObjectSpace objectSpace, ILogger<XafJournalEntryService>? logger = null)
        {
            _objectSpace = objectSpace ?? throw new ArgumentNullException(nameof(objectSpace));
            _logger = logger;
        }

        /// <summary>
        /// Gets journal entries based on query options
        /// </summary>
        /// <param name="options">Query options for filtering and sorting</param>
        /// <returns>Collection of journal entries</returns>
        public async Task<IEnumerable<ILedgerEntry>> GetJournalEntriesAsync(JournalEntryQueryOptions options)
        {
            try
            {
                _logger?.LogDebug("Getting journal entries with options: OnlyPosted={OnlyPosted}, AccountCode={AccountCode}, TransactionNumber={TransactionNumber}", 
                    options.OnlyPosted, options.AccountCode, options.TransactionNumber);

                return await Task.Run(() => GetJournalEntriesInternal(options));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting journal entries");
                throw;
            }
        }

        /// <summary>
        /// Gets all journal entries for a specific transaction
        /// </summary>
        /// <param name="transactionNumber">Transaction number to filter by</param>
        /// <returns>Collection of journal entries for the transaction</returns>
        public async Task<IEnumerable<ILedgerEntry>> GetJournalEntriesByTransactionAsync(string transactionNumber)
        {
            try
            {
                _logger?.LogDebug("Getting journal entries for transaction: {TransactionNumber}", transactionNumber);

                if (string.IsNullOrWhiteSpace(transactionNumber))
                {
                    _logger?.LogWarning("Transaction number is null or empty");
                    return Enumerable.Empty<ILedgerEntry>();
                }

                var criteria = new BinaryOperator("TransactionNumber", transactionNumber);
                var ledgerEntries = _objectSpace.GetObjects<LedgerEntry>(criteria)
                    .OrderBy(e => e.LedgerEntryNumber)
                    .Cast<ILedgerEntry>()
                    .ToList();

                _logger?.LogDebug("Found {Count} journal entries for transaction {TransactionNumber}", 
                    ledgerEntries.Count, transactionNumber);

                return await Task.FromResult(ledgerEntries);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting journal entries for transaction: {TransactionNumber}", transactionNumber);
                throw;
            }
        }

        /// <summary>
        /// Gets a specific journal entry by its number
        /// </summary>
        /// <param name="ledgerEntryNumber">Ledger entry number</param>
        /// <returns>The journal entry or null if not found</returns>
        public async Task<ILedgerEntry?> GetJournalEntryByNumberAsync(string ledgerEntryNumber)
        {
            try
            {
                _logger?.LogDebug("Getting journal entry by number: {LedgerEntryNumber}", ledgerEntryNumber);

                if (string.IsNullOrWhiteSpace(ledgerEntryNumber))
                {
                    _logger?.LogWarning("Ledger entry number is null or empty");
                    return null;
                }

                var criteria = new BinaryOperator("LedgerEntryNumber", ledgerEntryNumber);
                var ledgerEntry = _objectSpace.FindObject<LedgerEntry>(criteria);

                _logger?.LogDebug("Journal entry {LedgerEntryNumber} found: {Found}", 
                    ledgerEntryNumber, ledgerEntry != null);

                return await Task.FromResult((ILedgerEntry?)ledgerEntry);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting journal entry by number: {LedgerEntryNumber}", ledgerEntryNumber);
                throw;
            }
        }

        /// <summary>
        /// Checks if a transaction has posted journal entries
        /// </summary>
        /// <param name="transactionNumber">Transaction number to check</param>
        /// <returns>True if transaction is posted</returns>
        public async Task<bool> IsTransactionPostedAsync(string transactionNumber)
        {
            try
            {
                _logger?.LogDebug("Checking if transaction is posted: {TransactionNumber}", transactionNumber);

                if (string.IsNullOrWhiteSpace(transactionNumber))
                {
                    _logger?.LogWarning("Transaction number is null or empty");
                    return false;
                }

                var criteria = new BinaryOperator("TransactionNumber", transactionNumber);
                var transaction = _objectSpace.FindObject<Transaction>(criteria);

                var isPosted = transaction?.IsPosted ?? false;

                _logger?.LogDebug("Transaction {TransactionNumber} posted status: {IsPosted}", 
                    transactionNumber, isPosted);

                return await Task.FromResult(isPosted);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error checking transaction posted status: {TransactionNumber}", transactionNumber);
                throw;
            }
        }

        /// <summary>
        /// Gets the total debit amount for a transaction
        /// </summary>
        /// <param name="transactionNumber">Transaction number</param>
        /// <returns>Total debit amount</returns>
        public async Task<decimal> GetTransactionTotalDebitAsync(string transactionNumber)
        {
            try
            {
                _logger?.LogDebug("Getting total debit amount for transaction: {TransactionNumber}", transactionNumber);

                if (string.IsNullOrWhiteSpace(transactionNumber))
                {
                    _logger?.LogWarning("Transaction number is null or empty");
                    return 0m;
                }

                var criteria = CriteriaOperator.And(
                    new BinaryOperator("TransactionNumber", transactionNumber),
                    new BinaryOperator("EntryType", EntryType.Debit)
                );

                var debitEntries = _objectSpace.GetObjects<LedgerEntry>(criteria);
                var totalDebit = debitEntries.Sum(e => e.Amount);

                _logger?.LogDebug("Transaction {TransactionNumber} total debit: {TotalDebit}", 
                    transactionNumber, totalDebit);

                return await Task.FromResult(totalDebit);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting transaction total debit: {TransactionNumber}", transactionNumber);
                throw;
            }
        }

        /// <summary>
        /// Gets the total credit amount for a transaction
        /// </summary>
        /// <param name="transactionNumber">Transaction number</param>
        /// <returns>Total credit amount</returns>
        public async Task<decimal> GetTransactionTotalCreditAsync(string transactionNumber)
        {
            try
            {
                _logger?.LogDebug("Getting total credit amount for transaction: {TransactionNumber}", transactionNumber);

                if (string.IsNullOrWhiteSpace(transactionNumber))
                {
                    _logger?.LogWarning("Transaction number is null or empty");
                    return 0m;
                }

                var criteria = CriteriaOperator.And(
                    new BinaryOperator("TransactionNumber", transactionNumber),
                    new BinaryOperator("EntryType", EntryType.Credit)
                );

                var creditEntries = _objectSpace.GetObjects<LedgerEntry>(criteria);
                var totalCredit = creditEntries.Sum(e => e.Amount);

                _logger?.LogDebug("Transaction {TransactionNumber} total credit: {TotalCredit}", 
                    transactionNumber, totalCredit);

                return await Task.FromResult(totalCredit);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting transaction total credit: {TransactionNumber}", transactionNumber);
                throw;
            }
        }

        /// <summary>
        /// Checks if a transaction is balanced (total debits = total credits)
        /// </summary>
        /// <param name="transactionNumber">Transaction number</param>
        /// <returns>True if transaction is balanced</returns>
        public async Task<bool> IsTransactionBalancedAsync(string transactionNumber)
        {
            try
            {
                _logger?.LogDebug("Checking if transaction is balanced: {TransactionNumber}", transactionNumber);

                var totalDebit = await GetTransactionTotalDebitAsync(transactionNumber);
                var totalCredit = await GetTransactionTotalCreditAsync(transactionNumber);

                // Allow for minor rounding differences
                var isBalanced = Math.Abs(totalDebit - totalCredit) < 0.01m;

                _logger?.LogDebug("Transaction {TransactionNumber} balanced: {IsBalanced} (Debit: {TotalDebit}, Credit: {TotalCredit})", 
                    transactionNumber, isBalanced, totalDebit, totalCredit);

                return isBalanced;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error checking transaction balance: {TransactionNumber}", transactionNumber);
                throw;
            }
        }

        /// <summary>
        /// Gets all account codes affected by a transaction
        /// </summary>
        /// <param name="transactionNumber">Transaction number</param>
        /// <returns>Collection of account codes</returns>
        public async Task<IEnumerable<string>> GetAffectedAccountsAsync(string transactionNumber)
        {
            try
            {
                _logger?.LogDebug("Getting affected accounts for transaction: {TransactionNumber}", transactionNumber);

                if (string.IsNullOrWhiteSpace(transactionNumber))
                {
                    _logger?.LogWarning("Transaction number is null or empty");
                    return Enumerable.Empty<string>();
                }

                var criteria = new BinaryOperator("TransactionNumber", transactionNumber);
                var ledgerEntries = _objectSpace.GetObjects<LedgerEntry>(criteria);

                var affectedAccounts = ledgerEntries
                    .Select(e => e.OfficialCode)
                    .Where(code => !string.IsNullOrEmpty(code))
                    .Distinct()
                    .ToList();

                _logger?.LogDebug("Transaction {TransactionNumber} affects {Count} accounts: {Accounts}", 
                    transactionNumber, affectedAccounts.Count, string.Join(", ", affectedAccounts));

                return await Task.FromResult(affectedAccounts);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting affected accounts for transaction: {TransactionNumber}", transactionNumber);
                throw;
            }
        }

        /// <summary>
        /// Internal method to get journal entries using XAF ObjectSpace with complex filtering
        /// </summary>
        /// <param name="options">Query options</param>
        /// <returns>Collection of journal entries</returns>
        private IEnumerable<ILedgerEntry> GetJournalEntriesInternal(JournalEntryQueryOptions options)
        {
            var criteriaList = new List<CriteriaOperator>();

            // Filter by posted status
            if (options.OnlyPosted.HasValue && options.OnlyPosted.Value)
            {
                // Get posted transaction numbers
                var postedTransactionCriteria = new BinaryOperator("IsPosted", true);
                var postedTransactions = _objectSpace.GetObjects<Transaction>(postedTransactionCriteria);
                var postedTransactionNumbers = postedTransactions.Select(t => t.TransactionNumber).ToList();

                if (postedTransactionNumbers.Any())
                {
                    criteriaList.Add(new InOperator("TransactionNumber", postedTransactionNumbers));
                }
                else
                {
                    // No posted transactions, return empty result
                    return Enumerable.Empty<ILedgerEntry>();
                }
            }

            // Filter by account code
            if (!string.IsNullOrEmpty(options.AccountCode))
            {
                criteriaList.Add(new BinaryOperator("OfficialCode", options.AccountCode));
            }

            // Filter by transaction number
            if (!string.IsNullOrEmpty(options.TransactionNumber))
            {
                criteriaList.Add(new BinaryOperator("TransactionNumber", options.TransactionNumber));
            }

            // Filter by entry type
            if (options.EntryType.HasValue)
            {
                criteriaList.Add(new BinaryOperator("EntryType", options.EntryType.Value));
            }

            // Filter by document number (requires joining with transactions)
            if (!string.IsNullOrEmpty(options.DocumentNumber))
            {
                var transactionCriteria = new BinaryOperator("DocumentNumber", options.DocumentNumber);
                var transactions = _objectSpace.GetObjects<Transaction>(transactionCriteria);
                var transactionNumbers = transactions.Select(t => t.TransactionNumber).ToList();

                if (transactionNumbers.Any())
                {
                    criteriaList.Add(new InOperator("TransactionNumber", transactionNumbers));
                }
                else
                {
                    // No transactions with this document number
                    return Enumerable.Empty<ILedgerEntry>();
                }
            }

            // Filter by date range (requires joining with transactions)
            if (options.FromDate.HasValue || options.ToDate.HasValue)
            {
                var dateCriteriaList = new List<CriteriaOperator>();

                if (options.FromDate.HasValue)
                {
                    dateCriteriaList.Add(new BinaryOperator("TransactionDate", options.FromDate.Value, BinaryOperatorType.GreaterOrEqual));
                }

                if (options.ToDate.HasValue)
                {
                    dateCriteriaList.Add(new BinaryOperator("TransactionDate", options.ToDate.Value, BinaryOperatorType.LessOrEqual));
                }

                var dateCriteria = dateCriteriaList.Count == 1 ? dateCriteriaList[0] : CriteriaOperator.And(dateCriteriaList);
                var transactionsInRange = _objectSpace.GetObjects<Transaction>(dateCriteria);
                var transactionNumbersInRange = transactionsInRange.Select(t => t.TransactionNumber).ToList();

                if (transactionNumbersInRange.Any())
                {
                    criteriaList.Add(new InOperator("TransactionNumber", transactionNumbersInRange));
                }
                else
                {
                    // No transactions in date range
                    return Enumerable.Empty<ILedgerEntry>();
                }
            }

            // Combine all criteria
            CriteriaOperator? finalCriteria = null;
            if (criteriaList.Count == 1)
            {
                finalCriteria = criteriaList[0];
            }
            else if (criteriaList.Count > 1)
            {
                finalCriteria = CriteriaOperator.And(criteriaList);
            }

            // Get the ledger entries
            var ledgerEntries = finalCriteria != null 
                ? _objectSpace.GetObjects<LedgerEntry>(finalCriteria)
                : _objectSpace.GetObjects<LedgerEntry>();

            // Convert to list and apply sorting
            var results = ledgerEntries.Cast<ILedgerEntry>().ToList();

            // Apply sorting
            results = ApplySorting(results, options).ToList();

            // Apply pagination
            if (options.Skip.HasValue)
            {
                results = results.Skip(options.Skip.Value).ToList();
            }

            if (options.Take.HasValue)
            {
                results = results.Take(options.Take.Value).ToList();
            }

            _logger?.LogDebug("GetJournalEntriesInternal returned {Count} entries", results.Count);

            return results;
        }

        /// <summary>
        /// Applies sorting to the journal entries results
        /// </summary>
        /// <param name="entries">Entries to sort</param>
        /// <param name="options">Query options containing sorting preferences</param>
        /// <returns>Sorted entries</returns>
        private static IEnumerable<ILedgerEntry> ApplySorting(IEnumerable<ILedgerEntry> entries, JournalEntryQueryOptions options)
        {
            var sortBy = options.SortBy?.ToLower() ?? "ledgerentrynumber";

            return sortBy switch
            {
                "transactionnumber" => options.SortDescending
                    ? entries.OrderByDescending(e => e.TransactionNumber)
                    : entries.OrderBy(e => e.TransactionNumber),
                "accountcode" => options.SortDescending
                    ? entries.OrderByDescending(e => e.OfficialCode)
                    : entries.OrderBy(e => e.OfficialCode),
                "amount" => options.SortDescending
                    ? entries.OrderByDescending(e => e.Amount)
                    : entries.OrderBy(e => e.Amount),
                _ => options.SortDescending
                    ? entries.OrderByDescending(e => e.LedgerEntryNumber)
                    : entries.OrderBy(e => e.LedgerEntryNumber)
            };
        }
    }
}
