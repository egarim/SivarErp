using Sivar.Erp.Documents;
using System;

namespace Sivar.Erp.FinancialStatements.Equity
{
    /// <summary>
    /// Builder class for creating equity statement structure
    /// </summary>
    public class EquityStatementBuilder
    {
        private readonly List<IEquityLine> _lines = new List<IEquityLine>();
        private readonly List<IEquityColumn> _columns = new List<IEquityColumn>();
        private readonly List<IEquityLineAssignment> _assignments = new List<IEquityLineAssignment>();

        /// <summary>
        /// Adds a new equity line
        /// </summary>
        /// <param name="lineText">Line text</param>
        /// <param name="lineType">Line type</param>
        /// <param name="printedNo">Printed number</param>
        /// <returns>Builder for fluent interface</returns>
        public EquityStatementBuilder AddLine(string lineText, EquityLineType lineType, string printedNo = "")
        {
            var line = new EquityLineDto
            {
                Id = Guid.NewGuid(),
                LineText = lineText,
                LineType = lineType,
                PrintedNo = printedNo,
                VisibleIndex = _lines.Count,
                InsertedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _lines.Add(line);
            return this;
        }

        /// <summary>
        /// Adds a new equity column
        /// </summary>
        /// <param name="columnText">Column text</param>
        /// <returns>Builder for fluent interface</returns>
        public EquityStatementBuilder AddColumn(string columnText)
        {
            var column = new EquityColumnDto
            {
                Id = Guid.NewGuid(),
                ColumnText = columnText,
                VisibleIndex = _columns.Count,
                InsertedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _columns.Add(column);
            return this;
        }

        /// <summary>
        /// Assigns a document type to the last added line
        /// </summary>
        /// <param name="documentType">Document type</param>
        /// <param name="extendedDocumentTypeId">Extended document type ID</param>
        /// <returns>Builder for fluent interface</returns>
        public EquityStatementBuilder AssignDocumentType(DocumentType documentType, Guid? extendedDocumentTypeId = null)
        {
            if (_lines.Count == 0)
            {
                throw new InvalidOperationException("No lines available to assign document type to");
            }

            var lastLine = _lines.Last();
            var assignment = new EquityLineAssignmentDto
            {
                Id = Guid.NewGuid(),
                EquityLineId = lastLine.Id,
                DocumentType = documentType,
                ExtendedDocumentTypeId = extendedDocumentTypeId
            };

            _assignments.Add(assignment);
            return this;
        }

        /// <summary>
        /// Builds the equity statement structure
        /// </summary>
        /// <returns>Tuple of lines, columns, and assignments</returns>
        public (IEnumerable<IEquityLine> Lines, IEnumerable<IEquityColumn> Columns, IEnumerable<IEquityLineAssignment> Assignments) Build()
        {
            return (_lines, _columns, _assignments);
        }

        /// <summary>
        /// Creates a standard equity statement structure
        /// </summary>
        /// <returns>Builder with standard structure</returns>
        public static EquityStatementBuilder CreateStandardStructure()
        {
            return new EquityStatementBuilder()
                // Add standard columns
                .AddColumn("Share Capital")
                .AddColumn("Retained Earnings")
                .AddColumn("Revaluation Surplus")
                .AddColumn("Total Equity")

                // Add standard lines
                .AddLine("Balance at the beginning of the first period", EquityLineType.InitialBalance, "1")
                .AddLine("Changes in accounting policy", EquityLineType.CumulativeDelta, "2")
                .AddLine("Correction of prior period error", EquityLineType.CumulativeDelta, "3")
                .AddLine("Restated balance at the beginning of the first period", EquityLineType.ZeroBalance, "4")
                .AddLine("Changes in equity for the first period", EquityLineType.FirstDelta, "5")
                .AddLine("Issue of share capital", EquityLineType.FirstDelta, "5.1")
                .AssignDocumentType(DocumentType.Miscellaneous) // Example assignment
                .AddLine("Income for the year", EquityLineType.FirstDelta, "5.2")
                .AssignDocumentType(DocumentType.ClosingEntry)
                .AddLine("Revaluation gain", EquityLineType.FirstDelta, "5.3")
                .AssignDocumentType(DocumentType.Miscellaneous)
                .AddLine("Dividends", EquityLineType.FirstDelta, "5.4")
                .AssignDocumentType(DocumentType.Miscellaneous)
                .AddLine("Balance at the end of the first period", EquityLineType.FirstBalance, "6")
                .AddLine("Restated balance at the beginning of the second period", EquityLineType.FirstBalance, "6.1")  // Added missing line
                .AddLine("Changes in equity for the second period", EquityLineType.SecondDelta, "7")
                .AddLine("Issue of share capital", EquityLineType.SecondDelta, "7.1")
                .AssignDocumentType(DocumentType.Miscellaneous)
                .AddLine("Income for the year", EquityLineType.SecondDelta, "7.2")
                .AssignDocumentType(DocumentType.ClosingEntry)
                .AddLine("Revaluation gain", EquityLineType.SecondDelta, "7.3")
                .AssignDocumentType(DocumentType.Miscellaneous)
                .AddLine("Dividends", EquityLineType.SecondDelta, "7.4")
                .AssignDocumentType(DocumentType.Miscellaneous)
                .AddLine("Balance at the end of the second period", EquityLineType.SecondBalance, "8");
        }

        /// <summary>
        /// Resets the builder to empty state
        /// </summary>
        /// <returns>Reset builder</returns>
        public EquityStatementBuilder Reset()
        {
            _lines.Clear();
            _columns.Clear();
            _assignments.Clear();
            return this;
        }
    }
}