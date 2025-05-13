namespace Sivar.Erp.FinancialStatements
{
    /// <summary>
    /// Enumeration of balance types for cash flow statement calculations
    /// </summary>
    public enum BalanceType
    {
        /// <summary>
        /// Total balance including all periods up to the date
        /// </summary>
        Total,

        /// <summary>
        /// Balance changes for the specified period only
        /// </summary>
        PerPeriod,

        /// <summary>
        /// Only debit balance changes are included
        /// </summary>
        Debit,

        /// <summary>
        /// Only credit balance changes are included
        /// </summary>
        Credit
    }
}