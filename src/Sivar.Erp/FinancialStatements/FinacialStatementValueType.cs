namespace Sivar.Erp.FinancialStatements
{
    /// <summary>
    /// Enumeration of value types for financial statement lines
    /// </summary>
    public enum FinacialStatementValueType
    {
        /// <summary>
        /// Debit value type - positive balance increases the value
        /// </summary>
        Debit = 'D',

        /// <summary>
        /// Credit value type - positive balance decreases the value
        /// </summary>
        Credit = 'C'
    }
}