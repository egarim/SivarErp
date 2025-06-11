using Sivar.Erp.ChartOfAccounts;
using Sivar.Erp.Documents;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Mock implementation of AccountBalanceCalculator that works with in-memory collections for testing
/// </summary>
public class MockAccountBalanceCalculator : AccountBalanceCalculator
{
    private readonly Dictionary<string, AccountDto> _accounts;
    private readonly Dictionary<string, ITransaction> _transactions;
    private readonly List<ILedgerEntry> _ledgerEntries;

    /// <summary>
    /// Initializes a new instance of MockAccountBalanceCalculator using in-memory test data
    /// </summary>
    /// <param name="accounts">Dictionary of test accounts</param>
    /// <param name="transactions">Dictionary of test transactions</param>
    public MockAccountBalanceCalculator(
        Dictionary<string, AccountDto> accounts,
        Dictionary<string, ITransaction> transactions)
    {
        _accounts = accounts;
        _transactions = transactions;

        // Extract all ledger entries from transactions
        _ledgerEntries = new List<ILedgerEntry>();

        foreach (var transaction in _transactions.Values)
        {
            // Use reflection or similar approach to access _ledgerEntries collection
            // This is a simplified mock for testing - in real code we'd have proper access
            var transactionDto = transaction as TransactionDto;


            //TODO uncomment after test
            //if (transactionDto != null && transactionDto.LedgerEntries != null)
            //{
            //    //_ledgerEntries.AddRange(transactionDto.LedgerEntries);
            //}


        }
    }

    /// <summary>
    /// Calculates the balance of an account as of a specific date
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="asOfDate">Date to calculate balance for</param>
    /// <returns>Account balance (positive for debit balance, negative for credit balance)</returns>
    public new decimal CalculateAccountBalance(Guid accountId, DateOnly asOfDate)
    {
        var relevantEntries = _ledgerEntries.Where(e =>
            e.AccountId == accountId &&
            GetTransactionDateForEntry(e) <= asOfDate);

        decimal debitSum = relevantEntries.Where(e => e.EntryType == EntryType.Debit).Sum(e => e.Amount);
        decimal creditSum = relevantEntries.Where(e => e.EntryType == EntryType.Credit).Sum(e => e.Amount);

        return debitSum - creditSum;
    }

    /// <summary>
    /// Calculates the turnover (sum of all transactions) for an account within a date range
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="startDate">Start date (inclusive)</param>
    /// <param name="endDate">End date (inclusive)</param>
    /// <returns>Tuple containing debit turnover and credit turnover</returns>
    public new (decimal DebitTurnover, decimal CreditTurnover) CalculateAccountTurnover(
        Guid accountId, DateOnly startDate, DateOnly endDate)
    {
        var relevantEntries = _ledgerEntries.Where(e =>
        {
            var transactionDate = GetTransactionDateForEntry(e);
            return e.AccountId == accountId &&
                   transactionDate >= startDate &&
                   transactionDate <= endDate;
        });

        decimal debitTurnover = relevantEntries.Where(e => e.EntryType == EntryType.Debit).Sum(e => e.Amount);
        decimal creditTurnover = relevantEntries.Where(e => e.EntryType == EntryType.Credit).Sum(e => e.Amount);

        return (debitTurnover, creditTurnover);
    }

    /// <summary>
    /// Determines if an account has any transactions
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <returns>True if account has transactions, false otherwise</returns>
    public new bool HasTransactions(Guid accountId)
    {
        return _ledgerEntries.Any(e => e.AccountId == accountId);
    }

    /// <summary>
    /// Looks up the transaction date for a given ledger entry
    /// </summary>
    /// <param name="entry">The ledger entry</param>
    /// <returns>The transaction date</returns>
    private DateOnly GetTransactionDateForEntry(ILedgerEntry entry)
    {
        var transaction = _transactions.Values.FirstOrDefault(t => t.Id == entry.TransactionId);
        return transaction?.TransactionDate ?? new DateOnly(1900, 1, 1);
    }
}