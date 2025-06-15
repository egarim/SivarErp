using Sivar.Erp.Accounting.Transactions;

namespace Sivar.Erp.Services.BalanceCalculators
{
    /// <summary>
    /// Utility class for account balance calculations
    /// </summary>
    public class AccountBalanceCalculatorServiceBase : IAccountBalanceCalculator
    {
        IEnumerable<ITransaction> transactions;

        /// <summary>
        /// Initializes a new instance with an empty collection of transactions
        /// </summary>
        public AccountBalanceCalculatorServiceBase()
        {
            transactions = Array.Empty<ITransaction>();
        }

        /// <summary>
        /// Initializes a new instance with the specified collection of transactions
        /// </summary>
        /// <param name="transactions">Collection of transactions to use for calculations</param>
        public AccountBalanceCalculatorServiceBase(IEnumerable<ITransaction> transactions)
        {
            this.transactions = transactions ?? Array.Empty<ITransaction>();
        }

        /// <summary>
        /// Calculates the balance of an account as of a specific date
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="asOfDate">Date to calculate balance for</param>
        /// <returns>Account balance (positive for debit balance, negative for credit balance)</returns>
        public decimal CalculateAccountBalance(Guid accountId, DateOnly asOfDate)
        {
            // Get all relevant ledger entries for this account up to the specified date
            var relevantTransactions = transactions
                .Where(t => t.TransactionDate <= asOfDate)
                .ToList();

            // Find the ledger entries for the specified account
            var ledgerEntries = relevantTransactions
                .SelectMany(t => t.LedgerEntries)
                .Where(e => e.AccountId == accountId)
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
        /// <param name="accountId">Account ID</param>
        /// <param name="startDate">Start date (inclusive)</param>
        /// <param name="endDate">End date (inclusive)</param>
        /// <returns>Tuple containing debit turnover and credit turnover</returns>
        public (decimal DebitTurnover, decimal CreditTurnover) CalculateAccountTurnover(
            Guid accountId, DateOnly startDate, DateOnly endDate)
        {
            // Get all relevant ledger entries for this account within the date range
            var relevantTransactions = transactions
                .Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate)
                .ToList();

            // Find the ledger entries for the specified account
            var ledgerEntries = relevantTransactions
                .SelectMany(t => t.LedgerEntries)
                .Where(e => e.AccountId == accountId)
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
        /// <param name="accountId">Account ID</param>
        /// <returns>True if account has transactions, false otherwise</returns>
        public bool HasTransactions(Guid accountId)
        {
            // Check if there are any ledger entries for this account
            return transactions
                .SelectMany(t => t.LedgerEntries)
                .Any(e => e.AccountId == accountId);
        }
    }
}