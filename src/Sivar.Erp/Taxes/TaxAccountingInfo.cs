using System;

namespace Sivar.Erp.Taxes
{
    /// <summary>
    /// Holds accounting information for a specific tax
    /// </summary>
    public class TaxAccountingInfo
    {
        /// <summary>
        /// Account code to post to when this tax is a debit
        /// </summary>
        public string DebitAccountCode { get; set; }

        /// <summary>
        /// Account code to post to when this tax is a credit
        /// </summary>
        public string CreditAccountCode { get; set; }

        /// <summary>
        /// Indicates if this tax should be included in transaction generation
        /// </summary>
        public bool IncludeInTransaction { get; set; } = true;

        /// <summary>
        /// Description for the account entry
        /// </summary>
        public string AccountDescription { get; set; }
    }
}