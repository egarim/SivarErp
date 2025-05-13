using System;

namespace Sivar.Erp.FinancialStatements
{
    /// <summary>
    /// Implementation of cash flow line assignment
    /// </summary>
    public class CashFlowLineAssignmentDto : ICashFlowLineAssignment
    {
        /// <summary>
        /// Unique identifier for the assignment
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Reference to the account
        /// </summary>
        public Guid AccountId { get; set; }

        /// <summary>
        /// Reference to the cash flow line
        /// </summary>
        public Guid CashFlowLineId { get; set; }

        /// <summary>
        /// Validates the assignment
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        public bool Validate()
        {
            return AccountId != Guid.Empty && CashFlowLineId != Guid.Empty;
        }
    }
    /// <summary>
    /// Validation result for cash flow lines
    /// </summary>
}