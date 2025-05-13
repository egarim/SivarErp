using DevExpress.Xpo;
using Sivar.Erp.Xpo.Core;
using Sivar.Erp.Xpo.FinancialStatements;
using System;

namespace Sivar.Erp.FinancialStatements.CashFlow
{
    /// <summary>
    /// XPO implementation of cash flow line assignment
    /// </summary>
    [Persistent("CashFlowLineAssignments")]
    public class XpoCashFlowLineAssignment : XpoPersistentBase, ICashFlowLineAssignment
    {
        /// <summary>
        /// Creates a new instance
        /// </summary>`
        /// <param name="session">XPO session</param>
        public XpoCashFlowLineAssignment(Session session) : base(session) { }
        
        /// <summary>
        /// Reference to the account
        /// </summary>
        [Indexed]
        public Guid AccountId { get; set; }
        
        /// <summary>
        /// Reference to the cash flow line
        /// </summary>
        [Association("CashFlowLine-Assignments")]
        public XpoCashFlowLine CashFlowLine { get; set; }
        
        /// <summary>
        /// Gets the cash flow line ID
        /// </summary>
        [PersistentAlias("CashFlowLine.Oid")]
        public Guid CashFlowLineId
        {
            get { return (Guid)EvaluateAlias("CashFlowLineId"); }
        }
        //TODO fix
        Guid ICashFlowLineAssignment.AccountId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        Guid ICashFlowLineAssignment.CashFlowLineId { get => CashFlowLineId; set => throw new NotImplementedException(); }

        /// <summary>
        /// Validates the assignment
        /// </summary>
        protected override void OnSaving()
        {
            base.OnSaving();
            
            if (AccountId == Guid.Empty)
            {
                throw new InvalidOperationException("Account ID is required");
            }
            
            if (CashFlowLine == null)
            {
                throw new InvalidOperationException("Cash flow line is required");
            }
        }
    }
}