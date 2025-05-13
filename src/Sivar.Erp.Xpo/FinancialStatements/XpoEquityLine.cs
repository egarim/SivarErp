using DevExpress.Xpo;
using Sivar.Erp.Documents;
using Sivar.Erp.FinancialStatements;
using Sivar.Erp.FinancialStatements.Equity;
using Sivar.Erp.Xpo.Core;
using System;

namespace Sivar.Erp.Xpo.FinancialStatements
{
    /// <summary>
    /// XPO implementation of equity line entity
    /// </summary>
    [Persistent("EquityLines")]
    public class XpoEquityLine : XpoPersistentBase, IEquityLine
    {
        /// <summary>
        /// Default constructor required by XPO
        /// </summary>
        public XpoEquityLine(Session session) : base(session) { }

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
        /// Number to print in the report (e.g., "1", "2", "A", "B")
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

        private EquityLineType _lineType;

        /// <summary>
        /// Type of the equity line that defines overall structure
        /// </summary>
        [Persistent("LineType")]
        public EquityLineType LineType
        {
            get => _lineType;
            set => SetPropertyValue(nameof(LineType), ref _lineType, value);
        }

        // XPO collections for related entities
        [Association("EquityLine-Assignments")]
        public XPCollection<XpoEquityLineAssignment> Assignments =>
            GetCollection<XpoEquityLineAssignment>(nameof(Assignments));

        /// <summary>
        /// Validates the equity line according to business rules
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        public bool Validate()
        {
            // Line text is required
            if (string.IsNullOrWhiteSpace(LineText))
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
        /// Determines if this line is a first period delta type
        /// </summary>
        /// <returns>True if represents first period change</returns>
        public bool IsFirstPeriodDelta()
        {
            return LineType == EquityLineType.FirstDelta;
        }

        /// <summary>
        /// Determines if this line is a second period delta type
        /// </summary>
        /// <returns>True if represents second period change</returns>
        public bool IsSecondPeriodDelta()
        {
            return LineType == EquityLineType.SecondDelta;
        }

        /// <summary>
        /// Determines if this line represents a balance (not a change)
        /// </summary>
        /// <returns>True if represents a balance</returns>
        public bool IsBalanceLine()
        {
            return LineType == EquityLineType.InitialBalance ||
                   LineType == EquityLineType.ZeroBalance ||
                   LineType == EquityLineType.FirstBalance ||
                   LineType == EquityLineType.SecondBalance;
        }
    }

    /// <summary>
    /// XPO implementation of equity column entity
    /// </summary>
    [Persistent("EquityColumns")]
    public class XpoEquityColumn : XpoPersistentBase, IEquityColumn
    {
        /// <summary>
        /// Default constructor required by XPO
        /// </summary>
        public XpoEquityColumn(Session session) : base(session) { }

        private string _columnText = string.Empty;

        /// <summary>
        /// Text to display in the column header
        /// </summary>
        [Persistent("ColumnText"), Size(255)]
        public string ColumnText
        {
            get => _columnText;
            set => SetPropertyValue(nameof(ColumnText), ref _columnText, value);
        }

        private int _visibleIndex;

        /// <summary>
        /// Index for controlling display order of columns
        /// </summary>
        [Persistent("VisibleIndex")]
        [Indexed]
        public int VisibleIndex
        {
            get => _visibleIndex;
            set => SetPropertyValue(nameof(VisibleIndex), ref _visibleIndex, value);
        }

        /// <summary>
        /// Validates the equity column according to business rules
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        public bool Validate()
        {
            // Column text is required
            if (string.IsNullOrWhiteSpace(ColumnText))
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
        /// Determines if this is a total column
        /// </summary>
        /// <returns>True if this column represents totals</returns>
        public bool IsTotalColumn()
        {
            return ColumnText.ToUpper().Contains("TOTAL") ||
                   ColumnText.ToUpper().Contains("SUM");
        }
    }

    /// <summary>
    /// XPO implementation of equity line assignment to document types
    /// </summary>
    [Persistent("EquityLineAssignments")]
    public class XpoEquityLineAssignment : XPObject, IEquityLineAssignment
    {
        /// <summary>
        /// Default constructor required by XPO
        /// </summary>
        public XpoEquityLineAssignment(Session session) : base(session) { }

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

        private XpoEquityLine _equityLine;

        /// <summary>
        /// The equity line that this assignment belongs to
        /// </summary>
        [Association("EquityLine-Assignments")]
        public XpoEquityLine EquityLine
        {
            get => _equityLine;
            set => SetPropertyValue(nameof(EquityLine), ref _equityLine, value);
        }

        /// <summary>
        /// Reference to the equity line
        /// </summary>
        [PersistentAlias("EquityLine.Id")]
        public Guid EquityLineId
        {
            get => (Guid)EvaluateAlias(nameof(EquityLineId));
            set
            {
                if (value != EquityLineId)
                {
                    EquityLine = Session.GetObjectByKey<XpoEquityLine>(value);
                }
            }
        }

        private DocumentType _documentType;

        /// <summary>
        /// Base document type that this line is assigned to
        /// </summary>
        [Persistent("DocumentType")]
        public DocumentType DocumentType
        {
            get => _documentType;
            set => SetPropertyValue(nameof(DocumentType), ref _documentType, value);
        }

        private Guid? _extendedDocumentTypeId;

        /// <summary>
        /// Extended document type ID (if from extension)
        /// </summary>
        [Persistent("ExtendedDocumentTypeId")]
        public Guid? ExtendedDocumentTypeId
        {
            get => _extendedDocumentTypeId;
            set => SetPropertyValue(nameof(ExtendedDocumentTypeId), ref _extendedDocumentTypeId, value);
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
        /// Validates the equity line assignment
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        public bool Validate()
        {
            // Equity line ID must be set
            if (EquityLineId == Guid.Empty)
            {
                return false;
            }

            // Document type must be specified
            if (DocumentType == 0 && ExtendedDocumentTypeId == null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines if this assignment uses an extended document type
        /// </summary>
        /// <returns>True if extended document type is used</returns>
        public bool UsesExtendedDocumentType()
        {
            return ExtendedDocumentTypeId.HasValue;
        }
    }
}