namespace Sivar.Erp.FinancialStatements
{
    /// <summary>
    /// Enumeration of balance sheet and income statement line types
    /// </summary>
    public enum BalanceIncomeLineType
    {
        /// <summary>
        /// Base header that all other items are children of
        /// </summary>
        BaseHeader,

        /// <summary>
        /// Balance sheet header that all balance sheet items are children of
        /// </summary>
        BalanceHeader,

        /// <summary>
        /// Income statement header that all income statement items are children of
        /// </summary>
        IncomeHeader,

        /// <summary>
        /// A line item in the balance sheet
        /// </summary>
        BalanceLine,

        /// <summary>
        /// A line item in the income statement
        /// </summary>
        IncomeLine
    }
}