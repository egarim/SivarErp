namespace Sivar.Erp.Services.Accounting.BalanceCalculators
{
    public interface IAccountBalanceCalculator
    {
        /// <summary>
        /// Calculates the balance of an account as of a specific date
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="asOfDate">Date to calculate balance for</param>
        /// <returns>Account balance (positive for debit balance, negative for credit balance)</returns>
        decimal CalculateAccountBalance(Guid accountId, DateOnly asOfDate);

        /// <summary>
        /// Calculates the turnover (sum of all transactions) for an account within a date range
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="startDate">Start date (inclusive)</param>
        /// <param name="endDate">End date (inclusive)</param>
        /// <returns>Tuple containing debit turnover and credit turnover</returns>
        (decimal DebitTurnover, decimal CreditTurnover) CalculateAccountTurnover(Guid accountId, DateOnly startDate, DateOnly endDate);

        /// <summary>
        /// Determines if an account has any transactions
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>True if account has transactions, false otherwise</returns>
        bool HasTransactions(Guid accountId);
    }
}