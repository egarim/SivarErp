using Sivar.Erp.Core.Contracts;
using Sivar.Erp.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Explicit imports to resolve ambiguity
using CoreITransaction = Sivar.Erp.Core.Contracts.ITransaction;
using CoreIAccount = Sivar.Erp.Core.Contracts.IAccount;
using CoreAccountType = Sivar.Erp.Core.Enums.AccountType;

namespace Sivar.Erp.Modules.Accounting.Services.BalanceCalculators
{
    /// <summary>
    /// Utility class for account balance calculations (.NET 9 modernized)
    /// </summary>
    public class AccountBalanceCalculatorServiceBase : IAccountBalanceCalculator
    {
        private readonly IObjectDb? _objectDb;
        private IEnumerable<CoreITransaction> _transactions;

        /// <summary>
        /// Initializes a new instance with an empty collection of transactions
        /// </summary>
        public AccountBalanceCalculatorServiceBase()
        {
            _transactions = Array.Empty<CoreITransaction>();
        }

        /// <summary>
        /// Initializes a new instance with the specified collection of transactions
        /// </summary>
        /// <param name="transactions">Collection of transactions to use for calculations</param>
        public AccountBalanceCalculatorServiceBase(IEnumerable<CoreITransaction> transactions)
        {
            _transactions = transactions ?? Array.Empty<CoreITransaction>();
        }

        /// <summary>
        /// Initializes a new instance with IObjectDb to get transactions from
        /// </summary>
        /// <param name="objectDb">Object database containing transactions</param>
        public AccountBalanceCalculatorServiceBase(IObjectDb objectDb)
        {
            _objectDb = objectDb ?? throw new ArgumentNullException(nameof(objectDb));
            _transactions = Array.Empty<CoreITransaction>(); // Will be loaded from objectDb
        }

        /// <summary>
        /// Gets all transactions from the configured source
        /// </summary>
        private IEnumerable<CoreITransaction> GetTransactions()
        {
            if (_objectDb != null)
            {
                // Get transactions from IObjectDb
                return _objectDb.Transactions ?? Array.Empty<CoreITransaction>();
            }

            return _transactions;
        }

        /// <summary>
        /// Gets account balance async (interface implementation)
        /// </summary>
        /// <param name="accountCode">Account code</param>
        /// <returns>Account balance</returns>
        public Task<decimal> GetAccountBalanceAsync(string accountCode)
        {
            var balance = CalculateAccountBalance(accountCode, DateOnly.FromDateTime(DateTime.Today));
            return Task.FromResult(balance);
        }

        /// <summary>
        /// Gets account balance async as of specific date (interface implementation)
        /// </summary>
        /// <param name="accountCode">Account code</param>
        /// <param name="asOfDate">Date to calculate balance for</param>
        /// <returns>Account balance</returns>
        public Task<decimal> GetAccountBalanceAsync(string accountCode, DateOnly asOfDate)
        {
            var balance = CalculateAccountBalance(accountCode, asOfDate);
            return Task.FromResult(balance);
        }

        /// <summary>
        /// Calculates the balance of an account as of a specific date
        /// </summary>
        /// <param name="accountOfficialCode">Account code</param>
        /// <param name="asOfDate">Date to calculate balance for</param>
        /// <returns>Account balance (positive for debit balance, negative for credit balance)</returns>
        public decimal CalculateAccountBalance(string accountOfficialCode, DateOnly asOfDate)
        {
            // Get all relevant ledger entries for this account up to the specified date
            var relevantTransactions = GetTransactions()
                .Where(t => t.TransactionDate <= asOfDate && t.IsPosted)
                .ToList();

            // Find the ledger entries for the specified account
            var ledgerEntries = relevantTransactions
                .SelectMany(t => t.LedgerEntries ?? Enumerable.Empty<ILedgerEntry>())
                .Where(e => e.OfficialCode == accountOfficialCode)
                .ToList();

            // Calculate balance (debit sum - credit sum)
            decimal debitSum = ledgerEntries
                .Where(e => e.EntryType == EntryType.Debit)
                .Sum(e => e.Amount);

            decimal creditSum = ledgerEntries
                .Where(e => e.EntryType == EntryType.Credit)
                .Sum(e => e.Amount);

            // Return the net balance (positive for debit, negative for credit)
            return debitSum - creditSum;
        }

        /// <summary>
        /// Calculates the turnover (sum of all transactions) for an account within a date range
        /// </summary>
        /// <param name="accountOfficialCode">Account code</param>
        /// <param name="startDate">Start date (inclusive)</param>
        /// <param name="endDate">End date (inclusive)</param>
        /// <returns>Tuple containing debit turnover and credit turnover</returns>
        public (decimal DebitTurnover, decimal CreditTurnover) CalculateAccountTurnover(
            string accountOfficialCode, DateOnly startDate, DateOnly endDate)
        {
            // Get all relevant ledger entries for this account within the date range
            var relevantTransactions = GetTransactions()
                .Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate && t.IsPosted)
                .ToList();

            // Find the ledger entries for the specified account
            var ledgerEntries = relevantTransactions
                .SelectMany(t => t.LedgerEntries ?? Enumerable.Empty<ILedgerEntry>())
                .Where(e => e.OfficialCode == accountOfficialCode)
                .ToList();

            // Calculate debit and credit turnovers
            decimal debitTurnover = ledgerEntries
                .Where(e => e.EntryType == EntryType.Debit)
                .Sum(e => e.Amount);

            decimal creditTurnover = ledgerEntries
                .Where(e => e.EntryType == EntryType.Credit)
                .Sum(e => e.Amount);

            return (debitTurnover, creditTurnover);
        }

        /// <summary>
        /// Determines if an account has any transactions
        /// </summary>
        /// <param name="accountOfficialCode">Account code</param>
        /// <returns>True if account has transactions, false otherwise</returns>
        public bool HasTransactions(string accountOfficialCode)
        {
            // Check if there are any posted ledger entries for this account
            return GetTransactions()
                .Where(t => t.IsPosted)
                .SelectMany(t => t.LedgerEntries ?? Enumerable.Empty<ILedgerEntry>())
                .Any(e => e.OfficialCode == accountOfficialCode);
        }

        /// <summary>
        /// Gets all accounts that have transactions
        /// </summary>
        /// <returns>List of account codes that have transactions</returns>
        public IEnumerable<string> GetAccountsWithTransactions()
        {
            return GetTransactions()
                .Where(t => t.IsPosted)
                .SelectMany(t => t.LedgerEntries ?? Enumerable.Empty<ILedgerEntry>())
                .Select(e => e.OfficialCode)
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// Gets a summary of all account balances as of a specific date
        /// </summary>
        /// <param name="asOfDate">Date to calculate balances for</param>
        /// <returns>Dictionary of account codes and their balances</returns>
        public Dictionary<string, decimal> GetAllAccountBalances(DateOnly asOfDate)
        {
            var accountsWithTransactions = GetAccountsWithTransactions();
            var balances = new Dictionary<string, decimal>();

            foreach (var accountCode in accountsWithTransactions)
            {
                balances[accountCode] = CalculateAccountBalance(accountCode, asOfDate);
            }

            return balances;
        }

        /// <summary>
        /// Verifies that the total of all asset and expense accounts equals 
        /// the total of all liability, equity, and revenue accounts (basic accounting equation)
        /// </summary>
        /// <param name="asOfDate">Date to check the equation for</param>
        /// <param name="accounts">Chart of accounts for account type lookup</param>
        /// <returns>True if the accounting equation balances</returns>
        public bool VerifyAccountingEquation(DateOnly asOfDate, IEnumerable<CoreIAccount> accounts)
        {
            var accountBalances = GetAllAccountBalances(asOfDate);
            var accountLookup = new Dictionary<string, CoreAccountType>();
            
            // Safely build account lookup dictionary
            foreach (var account in accounts)
            {
                // Convert account type safely
                if (account is Sivar.Erp.Services.Accounting.ChartOfAccounts.AccountDto accountDto)
                {
                    // Convert legacy AccountType to Core AccountType
                    accountLookup[account.OfficialCode] = ConvertAccountType(accountDto.AccountType);
                }
                else
                {
                    // Try to use reflection to get AccountType if available
                    try
                    {
                        var accountTypeProperty = account.GetType().GetProperty("AccountType");
                        if (accountTypeProperty != null)
                        {
                            var accountTypeValue = accountTypeProperty.GetValue(account);
                            if (accountTypeValue is Enum enumValue)
                            {
                                // Try to parse as Core AccountType
                                if (Enum.TryParse<CoreAccountType>(enumValue.ToString(), out var coreAccountType))
                                {
                                    accountLookup[account.OfficialCode] = coreAccountType;
                                }
                                else
                                {
                                    accountLookup[account.OfficialCode] = CoreAccountType.Asset; // Default
                                }
                            }
                        }
                        else
                        {
                            accountLookup[account.OfficialCode] = CoreAccountType.Asset; // Default
                        }
                    }
                    catch
                    {
                        accountLookup[account.OfficialCode] = CoreAccountType.Asset; // Default
                    }
                }
            }

            decimal assetsAndExpenses = 0;
            decimal liabilitiesEquityRevenue = 0;

            foreach (var kvp in accountBalances)
            {
                var accountCode = kvp.Key;
                var balance = kvp.Value;

                if (accountLookup.TryGetValue(accountCode, out var accountType))
                {
                    switch (accountType)
                    {
                        case CoreAccountType.Asset:
                        case CoreAccountType.Expense:
                            assetsAndExpenses += balance;
                            break;
                        case CoreAccountType.Liability:
                        case CoreAccountType.Equity:
                        case CoreAccountType.Revenue:
                            liabilitiesEquityRevenue -= balance; // Convert credit balances to positive
                            break;
                    }
                }
            }

            // Allow for small rounding differences
            return Math.Abs(assetsAndExpenses - liabilitiesEquityRevenue) < 0.01m;
        }

        /// <summary>
        /// Converts legacy AccountType to Core AccountType
        /// </summary>
        private CoreAccountType ConvertAccountType(Sivar.Erp.Services.Accounting.ChartOfAccounts.AccountType legacyAccountType)
        {
            return legacyAccountType switch
            {
                Sivar.Erp.Services.Accounting.ChartOfAccounts.AccountType.Asset => CoreAccountType.Asset,
                Sivar.Erp.Services.Accounting.ChartOfAccounts.AccountType.Liability => CoreAccountType.Liability,
                Sivar.Erp.Services.Accounting.ChartOfAccounts.AccountType.Equity => CoreAccountType.Equity,
                Sivar.Erp.Services.Accounting.ChartOfAccounts.AccountType.Revenue => CoreAccountType.Revenue,
                Sivar.Erp.Services.Accounting.ChartOfAccounts.AccountType.Expense => CoreAccountType.Expense,
                _ => CoreAccountType.Asset // Default fallback
            };
        }
    }
}