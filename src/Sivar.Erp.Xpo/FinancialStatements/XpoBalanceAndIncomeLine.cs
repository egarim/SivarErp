using DevExpress.Xpo;
using Sivar.Erp.FinancialStatements;
using Sivar.Erp.FinancialStatements.BalanceAndIncome;
using Sivar.Erp.Xpo.Core;
using System;

namespace Sivar.Erp.Xpo.FinancialStatements
{
    /// <summary>
    /// XPO implementation of balance sheet and income statement line
    /// </summary>
    [Persistent("BalanceAndIncomeLines")]
    public class XpoBalanceAndIncomeLine : XpoPersistentBase, IBalanceAndIncomeLine
    {
        /// <summary>
        /// Default constructor required by XPO
        /// </summary>
        public XpoBalanceAndIncomeLine(Session session) : base(session) { }

        private BalanceIncomeLineType _lineType;

        /// <summary>
        /// Type of line (header or data line, balance or income)
        /// </summary>
        [Persistent("LineType")]
        public BalanceIncomeLineType LineType
        {
            get => _lineType;
            set => SetPropertyValue(nameof(LineType), ref _lineType, value);
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

        private Erp.FinancialStatements.FinacialStatementValueType _valueType;

        /// <summary>
        /// Whether this line represents a debit or credit balance
        /// </summary>
        [Persistent("ValueType")]
        public Erp.FinancialStatements.FinacialStatementValueType ValueType
        {
            get => _valueType;
            set => SetPropertyValue(nameof(ValueType), ref _valueType, value);
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

            return true;
        }

        /// <summary>
        /// Calculates the depth of this line in the tree
        /// </summary>
        /// <param name="allLines">All lines in the tree</param>
        /// <returns>Depth level (0 = root)</returns>
        public int CalculateDepth(IEnumerable<IBalanceAndIncomeLine> allLines)
        {
            int depth = 0;
            foreach (var otherLine in allLines)
            {
                if (otherLine.IsParentOf(this))
                {
                    depth++;
                }
            }
            return depth;
        }

        /// <summary>
        /// Determines if this line is a parent of another line
        /// </summary>
        /// <param name="childLine">Potential child line</param>
        /// <returns>True if this line is parent of childLine</returns>
        public bool IsParentOf(IBalanceAndIncomeLine childLine)
        {
            return LeftIndex < childLine.LeftIndex && RightIndex > childLine.RightIndex;
        }

        /// <summary>
        /// Determines if this line is a child of another line
        /// </summary>
        /// <param name="parentLine">Potential parent line</param>
        /// <returns>True if this line is child of parentLine</returns>
        public bool IsChildOf(IBalanceAndIncomeLine parentLine)
        {
            return parentLine.LeftIndex < LeftIndex && parentLine.RightIndex > RightIndex;
        }

        /// <summary>
        /// Determines if this line can have children (not a data line)
        /// </summary>
        /// <returns>True if can have children</returns>
        public bool CanHaveChildren()
        {
            // Only headers can have children, not data lines
            return LineType == BalanceIncomeLineType.BaseHeader ||
                   LineType == BalanceIncomeLineType.BalanceHeader ||
                   LineType == BalanceIncomeLineType.IncomeHeader ||
                   // Allow intermediate grouping lines
                   (LineType == BalanceIncomeLineType.BalanceLine && RightIndex - LeftIndex > 1) ||
                   (LineType == BalanceIncomeLineType.IncomeLine && RightIndex - LeftIndex > 1);
        }
    }
}