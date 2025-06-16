namespace Sivar.Erp.Services.Accounting.ChartOfAccounts
{
    /// <summary>
    /// Enumeration of account types in the chart of accounts
    /// </summary>
    public enum AccountType
    {
        /// <summary>
        /// Asset account - typically has a debit balance
        /// </summary>
        Asset = 1,

        /// <summary>
        /// Liability account - typically has a credit balance
        /// </summary>
        Liability = 2,

        /// <summary>
        /// Equity account - typically has a credit balance
        /// </summary>
        Equity = 3,

        /// <summary>
        /// Revenue account - typically has a credit balance
        /// </summary>
        Revenue = 4,

        /// <summary>
        /// Expense account - typically has a debit balance
        /// </summary>
        Expense = 5,

        /// <summary>
        /// Settlement account - used for year-end closing procedures
        /// </summary>
        Settlement = 6
    }
}