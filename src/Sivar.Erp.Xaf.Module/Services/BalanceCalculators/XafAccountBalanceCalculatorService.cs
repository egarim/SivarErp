using DevExpress.ExpressApp;
using DevExpress.Data.Filtering;
using Microsoft.Extensions.Logging;
using Sivar.Erp.Core.Contracts;
using Sivar.Erp.EfCore.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;
using Sivar.Erp.Modules.Accounting.Transactions;

#nullable enable

namespace Sivar.Erp.Xaf.Module.Services.BalanceCalculators
{
    /// <summary>
    /// XAF implementation of account balance calculator service using IObjectSpace
    /// </summary>
    public class XafAccountBalanceCalculatorService : IAccountBalanceCalculator
    {
        private readonly IObjectSpace _objectSpace;
        private readonly ILogger<XafAccountBalanceCalculatorService>? _logger;

        /// <summary>
        /// Initializes a new instance of the XafAccountBalanceCalculatorService class
        /// </summary>
        /// <param name="objectSpace">XAF ObjectSpace for data operations</param>
        /// <param name="logger">Optional logger for diagnostic information</param>
        public XafAccountBalanceCalculatorService(IObjectSpace objectSpace, ILogger<XafAccountBalanceCalculatorService>? logger = null)
        {
            _objectSpace = objectSpace ?? throw new ArgumentNullException(nameof(objectSpace));
            _logger = logger;
        }

        /// <summary>
        /// Gets account balance async as of today's date
        /// </summary>
        /// <param name="accountCode">Account code</param>
        /// <returns>Account balance</returns>
        public async Task<decimal> GetAccountBalanceAsync(string accountCode)
        {
            try
            {
                _logger?.LogDebug("Getting account balance for account code: {AccountCode}", accountCode);

                if (string.IsNullOrWhiteSpace(accountCode))
                {
                    _logger?.LogWarning("Account code is null or empty");
                    return 0m;
                }

                var today = DateOnly.FromDateTime(DateTime.Today);
                var balance = await GetAccountBalanceAsync(accountCode, today);

                _logger?.LogDebug("Account balance for {AccountCode} as of today: {Balance}", accountCode, balance);
                return balance;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting account balance for account code: {AccountCode}", accountCode);
                throw;
            }
        }

        /// <summary>
        /// Gets account balance async as of a specific date
        /// </summary>
        /// <param name="accountCode">Account code</param>
        /// <param name="asOfDate">Date to calculate balance for</param>
        /// <returns>Account balance</returns>
        public async Task<decimal> GetAccountBalanceAsync(string accountCode, DateOnly asOfDate)
        {
            try
            {
                _logger?.LogDebug("Getting account balance for account code: {AccountCode} as of date: {AsOfDate}", 
                    accountCode, asOfDate);

                if (string.IsNullOrWhiteSpace(accountCode))
                {
                    _logger?.LogWarning("Account code is null or empty");
                    return 0m;
                }

                var balance = await Task.Run(() => CalculateAccountBalance(accountCode, asOfDate));

                _logger?.LogDebug("Account balance for {AccountCode} as of {AsOfDate}: {Balance}", 
                    accountCode, asOfDate, balance);
                return balance;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting account balance for account code: {AccountCode} as of date: {AsOfDate}", 
                    accountCode, asOfDate);
                throw;
            }
        }

        /// <summary>
        /// Calculates the balance of an account as of a specific date using XAF ObjectSpace
        /// </summary>
        /// <param name="accountCode">Account code</param>
        /// <param name="asOfDate">Date to calculate balance for</param>
        /// <returns>Account balance (positive for debit balance, negative for credit balance)</returns>
        private decimal CalculateAccountBalance(string accountCode, DateOnly asOfDate)
        {
            try
            {
                _logger?.LogDebug("Calculating account balance for {AccountCode} as of {AsOfDate}", accountCode, asOfDate);

                // Build criteria for posted transactions up to the specified date
                var transactionCriteria = CriteriaOperator.And(
                    new BinaryOperator("IsPosted", true),
                    new BinaryOperator("TransactionDate", asOfDate, BinaryOperatorType.LessOrEqual)
                );

                // Get all posted transactions up to the specified date
                var transactions = _objectSpace.GetObjects<Transaction>(transactionCriteria);

                decimal debitSum = 0m;
                decimal creditSum = 0m;

                // Process each transaction to find ledger entries for the specified account
                foreach (var transaction in transactions)
                {
                    // Build criteria for ledger entries of this transaction for the specified account
                    var ledgerEntryCriteria = CriteriaOperator.And(
                        new BinaryOperator("TransactionNumber", transaction.TransactionNumber),
                        new BinaryOperator("OfficialCode", accountCode)
                    );

                    var ledgerEntries = _objectSpace.GetObjects<LedgerEntry>(ledgerEntryCriteria);

                    foreach (var entry in ledgerEntries)
                    {
                        if (entry.EntryType == EntryType.Debit)
                        {
                            debitSum += entry.Amount;
                        }
                        else if (entry.EntryType == EntryType.Credit)
                        {
                            creditSum += entry.Amount;
                        }
                    }
                }

                var balance = debitSum - creditSum;

                _logger?.LogDebug("Account {AccountCode} balance calculation: Debits={Debits}, Credits={Credits}, Balance={Balance}", 
                    accountCode, debitSum, creditSum, balance);

                return balance;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error calculating account balance for {AccountCode} as of {AsOfDate}", 
                    accountCode, asOfDate);
                throw;
            }
        }

        /// <summary>
        /// Validates that the account code exists in the system
        /// </summary>
        /// <param name="accountCode">Account code to validate</param>
        /// <returns>True if account exists, false otherwise</returns>
        public bool AccountExists(string accountCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(accountCode))
                    return false;

                var accountCriteria = new BinaryOperator("OfficialCode", accountCode);
                var account = _objectSpace.FindObject<Account>(accountCriteria);
                
                return account != null;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error checking if account exists: {AccountCode}", accountCode);
                return false;
            }
        }

        /// <summary>
        /// Gets the count of transactions affecting a specific account
        /// </summary>
        /// <param name="accountCode">Account code</param>
        /// <returns>Number of transactions affecting the account</returns>
        public int GetAccountTransactionCount(string accountCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(accountCode))
                    return 0;

                var ledgerEntryCriteria = new BinaryOperator("OfficialCode", accountCode);
                var ledgerEntries = _objectSpace.GetObjects<LedgerEntry>(ledgerEntryCriteria);

                // Count unique transaction numbers
                var uniqueTransactions = ledgerEntries
                    .Select(le => le.TransactionNumber)
                    .Distinct()
                    .Count();

                _logger?.LogDebug("Account {AccountCode} has {Count} transactions", accountCode, uniqueTransactions);

                return uniqueTransactions;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting transaction count for account: {AccountCode}", accountCode);
                return 0;
            }
        }

        /// <summary>
        /// Checks if an account has any transactions
        /// </summary>
        /// <param name="accountCode">Account code</param>
        /// <returns>True if account has transactions, false otherwise</returns>
        public bool HasTransactions(string accountCode)
        {
            return GetAccountTransactionCount(accountCode) > 0;
        }

        /// <summary>
        /// Gets account balance for a range of dates (useful for period analysis)
        /// </summary>
        /// <param name="accountCode">Account code</param>
        /// <param name="startDate">Start date (inclusive)</param>
        /// <param name="endDate">End date (inclusive)</param>
        /// <returns>Tuple containing opening balance, period activity, and closing balance</returns>
        public async Task<(decimal OpeningBalance, decimal PeriodDebit, decimal PeriodCredit, decimal ClosingBalance)> 
            GetAccountActivityAsync(string accountCode, DateOnly startDate, DateOnly endDate)
        {
            try
            {
                _logger?.LogDebug("Getting account activity for {AccountCode} from {StartDate} to {EndDate}", 
                    accountCode, startDate, endDate);

                // Get opening balance (day before start date)
                var dayBeforeStart = startDate.AddDays(-1);
                var openingBalance = await GetAccountBalanceAsync(accountCode, dayBeforeStart);

                // Get closing balance
                var closingBalance = await GetAccountBalanceAsync(accountCode, endDate);

                // Calculate period activity
                var (periodDebit, periodCredit) = await Task.Run(() => CalculateAccountActivity(accountCode, startDate, endDate));

                _logger?.LogDebug("Account {AccountCode} activity: Opening={Opening}, PeriodDebit={PeriodDebit}, PeriodCredit={PeriodCredit}, Closing={Closing}", 
                    accountCode, openingBalance, periodDebit, periodCredit, closingBalance);

                return (openingBalance, periodDebit, periodCredit, closingBalance);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting account activity for {AccountCode} from {StartDate} to {EndDate}", 
                    accountCode, startDate, endDate);
                throw;
            }
        }

        /// <summary>
        /// Calculates account activity (debit and credit totals) for a specific period
        /// </summary>
        /// <param name="accountCode">Account code</param>
        /// <param name="startDate">Start date (inclusive)</param>
        /// <param name="endDate">End date (inclusive)</param>
        /// <returns>Tuple containing total debit and credit amounts for the period</returns>
        private (decimal TotalDebit, decimal TotalCredit) CalculateAccountActivity(string accountCode, DateOnly startDate, DateOnly endDate)
        {
            try
            {
                // Build criteria for posted transactions within the date range
                var transactionCriteria = CriteriaOperator.And(
                    new BinaryOperator("IsPosted", true),
                    new BinaryOperator("TransactionDate", startDate, BinaryOperatorType.GreaterOrEqual),
                    new BinaryOperator("TransactionDate", endDate, BinaryOperatorType.LessOrEqual)
                );

                var transactions = _objectSpace.GetObjects<Transaction>(transactionCriteria);

                decimal totalDebit = 0m;
                decimal totalCredit = 0m;

                foreach (var transaction in transactions)
                {
                    var ledgerEntryCriteria = CriteriaOperator.And(
                        new BinaryOperator("TransactionNumber", transaction.TransactionNumber),
                        new BinaryOperator("OfficialCode", accountCode)
                    );

                    var ledgerEntries = _objectSpace.GetObjects<LedgerEntry>(ledgerEntryCriteria);

                    foreach (var entry in ledgerEntries)
                    {
                        if (entry.EntryType == EntryType.Debit)
                        {
                            totalDebit += entry.Amount;
                        }
                        else if (entry.EntryType == EntryType.Credit)
                        {
                            totalCredit += entry.Amount;
                        }
                    }
                }

                return (totalDebit, totalCredit);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error calculating account activity for {AccountCode} from {StartDate} to {EndDate}", 
                    accountCode, startDate, endDate);
                throw;
            }
        }
    }
}
