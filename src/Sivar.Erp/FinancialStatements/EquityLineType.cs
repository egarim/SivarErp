namespace Sivar.Erp.FinancialStatements
{
    /// <summary>
    /// Enumeration of equity statement line types that define overall structure
    /// </summary>
    public enum EquityLineType
    {
        /// <summary>
        /// Balance at the beginning of the first period
        /// </summary>
        InitialBalance,

        /// <summary>
        /// Cumulative changes in accounting policy or prior period corrections
        /// </summary>
        CumulativeDelta,

        /// <summary>
        /// Restated balance at the beginning of the first period (after adjustments)
        /// </summary>
        ZeroBalance,

        /// <summary>
        /// Changes in equity for the first period
        /// </summary>
        FirstDelta,

        /// <summary>
        /// Balance at the end of the first period
        /// </summary>
        FirstBalance,

        /// <summary>
        /// Changes in equity for the second period
        /// </summary>
        SecondDelta,

        /// <summary>
        /// Balance at the end of the second period
        /// </summary>
        SecondBalance
    }
}