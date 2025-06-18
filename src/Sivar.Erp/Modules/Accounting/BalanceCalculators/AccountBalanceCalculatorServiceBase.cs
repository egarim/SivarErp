using Sivar.Erp.Services;
using Sivar.Erp.Services.Accounting.ChartOfAccounts;
using Sivar.Erp.Services.Accounting.Transactions;

namespace Sivar.Erp.Services.Accounting.BalanceCalculators
{
    /// <summary>
    /// Utility class for account balance calculations
    /// </summary>
    public class AccountBalanceCalculatorServiceBase : IAccountBalanceCalculator
    {
        private readonly IObjectDb _objectDb;
        private IEnumerable<ITransaction> _transactions;

        /// <summary>
        /// Initializes a new instance with an empty collection of transactions
        /// </summary>
        public AccountBalanceCalculatorServiceBase()
        {
            _transactions = Array.Empty<ITransaction>();
        }

        /// <summary>
        /// Initializes a new instance with the specified collection of transactions
        /// </summary>
        /// <param name="transactions">Collection of transactions to use for calculations</param>
        public AccountBalanceCalculatorServiceBase(IEnumerable<ITransaction> transactions)
        {
            _transactions = transactions ?? Array.Empty<ITransaction>();
        }

        /// <summary>
        /// Initializes a new instance with IObjectDb to get transactions from
        /// </summary>
        /// <param name="objectDb">Object database containing transactions</param>
        public AccountBalanceCalculatorServiceBase(IObjectDb objectDb)
        {
            _objectDb = objectDb ?? throw new ArgumentNullException(nameof(objectDb));
            _transactions = Array.Empty<ITransaction>(); // Will be loaded from objectDb
        }

        /// <summary>
        /// Gets all transactions from the configured source
        /// </summary>
        private IEnumerable<ITransaction> GetTransactions()
        {
            if (_objectDb != null)
            {
                // Get transactions from IObjectDb - we need to add Transactions property to IObjectDb
                // For now, we'll return empty collection until IObjectDb is updated
                return _objectDb.Transactions ?? Array.Empty<ITransaction>();
            }

            return _transactions;
        }

        /// <summary>
        /// Calculates the balance of an account as of a specific date
        /// </summary>
        /// <param name="AccountOfficialCode">Account ID</param>
        /// <param name="asOfDate">Date to calculate balance for</param>
        /// <returns>Account balance (positive for debit balance, negative for credit balance)</returns>
        public decimal CalculateAccountBalance(string AccountOfficialCode, DateOnly asOfDate)
        {
            // Get all relevant ledger entries for this account up to the specified date
            var relevantTransactions = GetTransactions()
                .Where(t => t.TransactionDate <= asOfDate && t.IsPosted)
                .ToList();

            // Find the ledger entries for the specified account
            var ledgerEntries = relevantTransactions
                .SelectMany(t => t.LedgerEntries)
                .Where(e => e.OfficialCode == AccountOfficialCode)
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
        /// <param name="AccountOfficialCode">Account ID</param>
        /// <param name="startDate">Start date (inclusive)</param>
        /// <param name="endDate">End date (inclusive)</param>
        /// <returns>Tuple containing debit turnover and credit turnover</returns>
        public (decimal DebitTurnover, decimal CreditTurnover) CalculateAccountTurnover(
            string AccountOfficialCode, DateOnly startDate, DateOnly endDate)
        {
            // Get all relevant ledger entries for this account within the date range
            var relevantTransactions = GetTransactions()
                .Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate && t.IsPosted)
                .ToList();

            // Find the ledger entries for the specified account
            var ledgerEntries = relevantTransactions
                .SelectMany(t => t.LedgerEntries)
                .Where(e => e.OfficialCode == AccountOfficialCode)
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
        /// <param name="AccountOfficialCode">Account ID</param>
        /// <returns>True if account has transactions, false otherwise</returns>
        public bool HasTransactions(string AccountOfficialCode)
        {
            // Check if there are any posted ledger entries for this account
            return GetTransactions()
                .Where(t => t.IsPosted)
                .SelectMany(t => t.LedgerEntries)
                .Any(e => e.OfficialCode == AccountOfficialCode);
        }

        /// <summary>
        /// Gets all accounts that have transactions
        /// </summary>
        /// <returns>List of account codes that have transactions</returns>
        public IEnumerable<string> GetAccountsWithTransactions()
        {
            return GetTransactions()
                .Where(t => t.IsPosted)
                .SelectMany(t => t.LedgerEntries)
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
        public bool VerifyAccountingEquation(DateOnly asOfDate, IEnumerable<IAccount> accounts)
        {
            var accountBalances = GetAllAccountBalances(asOfDate);
            var accountLookup = accounts.ToDictionary(a => a.OfficialCode, a => a.AccountType);

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
                        case AccountType.Asset:
                        case AccountType.Expense:
                            assetsAndExpenses += balance;
                            break;
                        case AccountType.Liability:
                        case AccountType.Equity:
                        case AccountType.Revenue:
                            liabilitiesEquityRevenue -= balance; // Convert credit balances to positive
                            break;
                    }
                }
            }

            // Allow for small rounding differences
            return Math.Abs(assetsAndExpenses - liabilitiesEquityRevenue) < 0.01m;
        }
    }
}