using DevExpress.Xpo;
using Sivar.Erp.Documents;
using Sivar.Erp.FinancialStatements;
using Sivar.Erp.Xpo.Core;
using System;

namespace Sivar.Erp.Xpo.FinancialStatements
{
    /// <summary>
    /// XPO implementation of cash flow statement line
    /// </summary>
    [Persistent("CashFlowLines")]
    public class XpoCashFlowLine : XpoPersistentBase, ICashFlowLine
    {
        /// <summary>
        /// Default constructor required by XPO
        /// </summary>
        public XpoCashFlowLine(Session session) : base(session) { }

        private CashFlowLineType _lineType;

        /// <summary>
        /// Type of line (header or data line)
        /// </summary>
        [Persistent("LineType")]
        public CashFlowLineType LineType
        {
            get => _lineType;
            set => SetPropertyValue(nameof(LineType), ref _lineType, value);
        }

        private bool _isNetIncome;

        /// <summary>
        /// Indicates if this line represents net income (only one line can have this flag)
        /// </summary>
        [Persistent("IsNetIncome")]
        public bool IsNetIncome
        {
            get => _isNetIncome;
            set => SetPropertyValue(nameof(IsNetIncome), ref _isNetIncome, value);
        }

        private int _visibleIndex;

        /// <summary>
        /// Index for controlling display order
        /// </summary>
        [Persistent("VisibleIndex")]
        [Indexed]
        public int VisibleIndex
        {
            get => _visibleIndex;
            set => SetPropertyValue(nameof(VisibleIndex), ref _visibleIndex, value);
        }

        private string _printedNo = string.Empty;

        /// <summary>
        /// Number to print in the report (e.g., "I", "II", "A", "1.")
        /// </summary>
        [Persistent("PrintedNo"), Size(20)]
        public string PrintedNo
        {
            get => _printedNo;
            set => SetPropertyValue(nameof(PrintedNo), ref _printedNo, value);
        }

        private string _lineText = string.Empty;

        /// <summary>
        /// Text to display for this line
        /// </summary>
        [Persistent("LineText"), Size(255)]
        public string LineText
        {
            get => _lineText;
            set => SetPropertyValue(nameof(LineText), ref _lineText, value);
        }

        private FinacialStatementValueType _valueType;

        /// <summary>
        /// Whether this line represents a debit or credit value
        /// </summary>
        [Persistent("ValueType")]
        public FinacialStatementValueType ValueType
        {
            get => _valueType;
            set => SetPropertyValue(nameof(ValueType), ref _valueType, value);
        }

        private BalanceType _balanceType;

        /// <summary>
        /// How the balance is calculated for this line
        /// </summary>
        [Persistent("BalanceType")]
        public BalanceType BalanceType
        {
            get => _balanceType;
            set => SetPropertyValue(nameof(BalanceType), ref _balanceType, value);
        }

        private int _leftIndex;

        /// <summary>
        /// Left index for nested set model
        /// </summary>
        [Persistent("LeftIndex")]
        [Indexed]
        public int LeftIndex
        {
            get => _leftIndex;
            set => SetPropertyValue(nameof(LeftIndex), ref _leftIndex, value);
        }

        private int _rightIndex;

        /// <summary>
        /// Right index for nested set model
        /// </summary>
        [Persistent("RightIndex")]
        [Indexed]
        public int RightIndex
        {
            get => _rightIndex;
            set => SetPropertyValue(nameof(RightIndex), ref _rightIndex, value);
        }

        // XPO collections for related entities
        [Association("CashFlowLine-Assignments")]
        public XPCollection<XpoCashFlowLineAssignment> Assignments =>
            GetCollection<XpoCashFlowLineAssignment>(nameof(Assignments));

        /// <summary>
        /// Validates the line according to business rules
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        public bool Validate()
        {
            // Line text is required
            if (string.IsNullOrWhiteSpace(LineText))
            {
                return false;
            }

            // Left index must be less than right index
            if (LeftIndex >= RightIndex)
            {
                return false;
            }

            // Visible index cannot be negative
            if (VisibleIndex < 0)
            {
                return false;
            }

            // Net income lines must be of type Line, not Header
            if (IsNetIncome && LineType != CashFlowLineType.Line)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines if this line can be moved to a new parent
        /// </summary>
        /// <param name="newParent">Potential new parent</param>
        /// <returns>True if move is valid</returns>
        public bool CanMoveTo(ICashFlowLine newParent)
        {
            // Cannot move to self
            if (newParent.Id == this.Id)
            {
                return false;
            }

            // Cannot move to a child of itself
            if (this.IsParentOf(newParent))
            {
                return false;
            }

            // Can only move under headers
            return newParent.LineType == CashFlowLineType.Header;
        }
    }

    /// <summary>
    /// XPO implementation of cash flow line assignment
    /// </summary>
    [Persistent("CashFlowLineAssignments")]
    public class XpoCashFlowLineAssignment : XPObject, ICashFlowLineAssignment
    {
        /// <summary>
        /// Default constructor required by XPO
        /// </summary>
        public XpoCashFlowLineAssignment(Session session) : base(session) { }

        private Guid _guid;

        /// <summary>
        /// Unique identifier for the assignment
        /// </summary>
        [Persistent("Guid")]
        [Indexed(Unique = true)]
        public Guid Id
        {
            get => _guid;
            set => SetPropertyValue(nameof(Id), ref _guid, value);
        }

        private Guid _accountId;

        /// <summary>
        /// Reference to the account
        /// </summary>
        [Persistent("AccountId")]
        [Indexed]
        public Guid AccountId
        {
            get => _accountId;
            set => SetPropertyValue(nameof(AccountId), ref _accountId, value);
        }

        private XpoCashFlowLine _cashFlowLine;

        /// <summary>
        /// The cash flow line that this assignment belongs to
        /// </summary>
        [Association("CashFlowLine-Assignments")]
        public XpoCashFlowLine CashFlowLine
        {
            get => _cashFlowLine;
            set => SetPropertyValue(nameof(CashFlowLine), ref _cashFlowLine, value);
        }

        /// <summary>
        /// Reference to the cash flow line
        /// </summary>
        [PersistentAlias("CashFlowLine.Id")]
        public Guid CashFlowLineId
        {
            get => (Guid)EvaluateAlias(nameof(CashFlowLineId));
            set
            {
                if (value != CashFlowLineId)
                {
                    CashFlowLine = Session.GetObjectByKey<XpoCashFlowLine>(value);
                }
            }
        }

        /// <summary>
        /// Override to initialize new objects
        /// </summary>
        protected override void OnSaving()
        {
            base.OnSaving();

            if (Session.IsNewObject(this) && Id == Guid.Empty)
            {
                Id = Guid.NewGuid();
            }
        }

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
    /// XPO implementation of cash flow adjustment
    /// </summary>
    [Persistent("CashFlowAdjustments")]
    public class XpoCashFlowAdjustment : XPObject, ICashFlowAdjustment
    {
        /// <summary>
        /// Default constructor required by XPO
        /// </summary>
        public XpoCashFlowAdjustment(Session session) : base(session) { }

        private Guid _guid;

        /// <summary>
        /// Unique identifier for the adjustment
        /// </summary>
        [Persistent("Guid")]
        [Indexed(Unique = true)]
        public Guid Id
        {
            get => _guid;
            set => SetPropertyValue(nameof(Id), ref _guid, value);
        }

        private Guid _transactionId;

        /// <summary>
        /// Reference to the transaction being adjusted
        /// </summary>
        [Persistent("TransactionId")]
        [Indexed]
        public Guid TransactionId
        {
            get => _transactionId;
            set => SetPropertyValue(nameof(TransactionId), ref _transactionId, value);
        }

        private Guid _accountId;

        /// <summary>
        /// Reference to the account being adjusted
        /// </summary>
        [Persistent("AccountId")]
        [Indexed]
        public Guid AccountId
        {
            get => _accountId;
            set => SetPropertyValue(nameof(AccountId), ref _accountId, value);
        }

        private EntryType _entryType;

        /// <summary>
        /// Type of adjustment (debit or credit)
        /// </summary>
        [Persistent("EntryType")]
        public EntryType EntryType
        {
            get => _entryType;
            set => SetPropertyValue(nameof(EntryType), ref _entryType, value);
        }

        private decimal _amount;

        /// <summary>
        /// Amount of the adjustment
        /// </summary>
        [Persistent("Amount")]
        public decimal Amount
        {
            get => _amount;
            set => SetPropertyValue(nameof(Amount), ref _amount, value);
        }

        /// <summary>
        /// Override to initialize new objects
        /// </summary>
        protected override void OnSaving()
        {
            base.OnSaving();

            if (Session.IsNewObject(this) && Id == Guid.Empty)
            {
                Id = Guid.NewGuid();
            }
        }

        /// <summary>
        /// Validates the adjustment
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        public bool Validate()
        {
            // All required fields must be set
            if (TransactionId == Guid.Empty || AccountId == Guid.Empty)
            {
                return false;
            }

            // Amount must be positive
            if (Amount <= 0)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Calculates the effective amount based on entry type
        /// </summary>
        /// <returns>Positive for debit, negative for credit</returns>
        public decimal GetEffectiveAmount()
        {
            return EntryType == EntryType.Debit ? Amount : -Amount;
        }
    }
}