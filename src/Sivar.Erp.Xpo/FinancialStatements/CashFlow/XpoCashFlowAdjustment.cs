using DevExpress.Xpo;
using Sivar.Erp.Documents;
using Sivar.Erp.Xpo.Core;
using System;

namespace Sivar.Erp.FinancialStatements.CashFlow
{
    /// <summary>
    /// XPO implementation of cash flow adjustment
    /// </summary>
    [Persistent("CashFlowAdjustments")]
    public class XpoCashFlowAdjustment : XpoPersistentBase, ICashFlowAdjustment
    {
        /// <summary>
        /// Creates a new instance
        /// </summary>
        /// <param name="session">XPO session</param>
        public XpoCashFlowAdjustment(Session session) : base(session) { }

        /// <summary>
        /// Reference to the transaction
        /// </summary>
        [Indexed]
        public Guid TransactionId { get; set; }

        /// <summary>
        /// Reference to the account
        /// </summary>
        [Indexed]
        public Guid AccountId { get; set; }

        /// <summary>
        /// Type of entry
        /// </summary>
        public EntryType EntryType { get; set; }

        /// <summary>
        /// Amount of the adjustment
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Validates the adjustment
        /// </summary>
        protected override void OnSaving()
        {
            base.OnSaving();

            if (TransactionId == Guid.Empty)
            {
                throw new InvalidOperationException("Transaction ID is required");
            }

            if (AccountId == Guid.Empty)
            {
                throw new InvalidOperationException("Account ID is required");
            }
        }
    }
}