using NUnit.Framework;
using Sivar.Erp.Documents;
using Sivar.Erp.FinancialStatements;
using Sivar.Erp.FinancialStatements.Equity;
using System;
using System.Linq;

namespace Tests.FinancialStatements
{
    [TestFixture]
    public class EquityStatementBuilderTests
    {
        private EquityStatementBuilder _builder;

        [SetUp]
        public void Setup()
        {
            _builder = new EquityStatementBuilder();
        }

        [Test]
        public void AddLine_AddsLineWithCorrectProperties()
        {
            // Arrange
            string lineText = "Test Line";
            EquityLineType lineType = EquityLineType.FirstDelta;
            string printedNo = "1.2";

            // Act
            _builder.AddLine(lineText, lineType, printedNo);
            var result = _builder.Build();

            // Assert
            var line = result.Lines.First();
            Assert.That(line.LineText, Is.EqualTo(lineText));
            Assert.That(line.LineType, Is.EqualTo(lineType));
            Assert.That(line.PrintedNo, Is.EqualTo(printedNo));
            Assert.That(line.VisibleIndex, Is.EqualTo(0));
            Assert.That(line.Id, Is.Not.EqualTo(Guid.Empty));
        }

        [Test]
        public void AddColumn_AddsColumnWithCorrectProperties()
        {
            // Arrange
            string columnText = "Test Column";

            // Act
            _builder.AddColumn(columnText);
            var result = _builder.Build();

            // Assert
            var column = result.Columns.First();
            Assert.That(column.ColumnText, Is.EqualTo(columnText));
            Assert.That(column.VisibleIndex, Is.EqualTo(0));
            Assert.That(column.Id, Is.Not.EqualTo(Guid.Empty));
        }

        [Test]
        public void AssignDocumentType_WhenNoLines_ThrowsInvalidOperationException()
        {
            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() =>
                _builder.AssignDocumentType(DocumentType.Miscellaneous));

            Assert.That(ex.Message, Is.EqualTo("No lines available to assign document type to"));
        }

        [Test]
        public void AssignDocumentType_AssignsToLastAddedLine()
        {
            // Arrange
            _builder.AddLine("Line 1", EquityLineType.FirstDelta, "1");
            _builder.AddLine("Line 2", EquityLineType.FirstDelta, "2");
            DocumentType docType = DocumentType.FixedAssetsDepreciation;
            Guid extendedTypeId = Guid.NewGuid();

            // Act
            _builder.AssignDocumentType(docType, extendedTypeId);
            var result = _builder.Build();

            // Assert
            var assignment = result.Assignments.First();
            var lastLine = result.Lines.Last();
            Assert.That(assignment.EquityLineId, Is.EqualTo(lastLine.Id));
            Assert.That(assignment.DocumentType, Is.EqualTo(docType));
            Assert.That(assignment.ExtendedDocumentTypeId, Is.EqualTo(extendedTypeId));
        }

        [Test]
        public void Build_ReturnsCorrectStructure()
        {
            // Arrange
            _builder.AddLine("Line 1", EquityLineType.FirstDelta, "1")
                    .AssignDocumentType(DocumentType.Miscellaneous)
                    .AddLine("Line 2", EquityLineType.FirstDelta, "2")
                    .AddColumn("Column 1")
                    .AddColumn("Column 2");

            // Act
            var result = _builder.Build();

            // Assert
            Assert.That(result.Lines.Count(), Is.EqualTo(2));
            Assert.That(result.Columns.Count(), Is.EqualTo(2));
            Assert.That(result.Assignments.Count(), Is.EqualTo(1));
        }

        [Test]
        public void Reset_ClearsAllCollections()
        {
            // Arrange
            _builder.AddLine("Line 1", EquityLineType.FirstDelta, "1")
                    .AssignDocumentType(DocumentType.Miscellaneous)
                    .AddColumn("Column 1");

            // Act
            _builder.Reset();
            var result = _builder.Build();

            // Assert
            Assert.That(result.Lines, Is.Empty);
            Assert.That(result.Columns, Is.Empty);
            Assert.That(result.Assignments, Is.Empty);
        }

        [Test]
        public void CreateStandardStructure_ReturnsCompleteStructure()
        {
            // Act
            var standardBuilder = EquityStatementBuilder.CreateStandardStructure();
            var result = standardBuilder.Build();

            // Assert
            Assert.That(result.Lines.Count(), Is.EqualTo(17));
            Assert.That(result.Columns.Count(), Is.EqualTo(4));
            Assert.That(result.Assignments.Count(), Is.EqualTo(8));

            // Verify specific elements
            Assert.That(result.Columns.ElementAt(0).ColumnText, Is.EqualTo("Share Capital"));
            Assert.That(result.Lines.ElementAt(0).LineText, Is.EqualTo("Balance at the beginning of the first period"));
            Assert.That(result.Lines.ElementAt(0).LineType, Is.EqualTo(EquityLineType.InitialBalance));
        }

        [Test]
        public void MultipleLines_MaintainCorrectVisibleIndex()
        {
            // Act
            _builder.AddLine("Line 1", EquityLineType.FirstDelta, "1")
                    .AddLine("Line 2", EquityLineType.FirstDelta, "2")
                    .AddLine("Line 3", EquityLineType.FirstDelta, "3");

            var result = _builder.Build();

            // Assert
            var lines = result.Lines.ToList();
            Assert.That(lines[0].VisibleIndex, Is.EqualTo(0));
            Assert.That(lines[1].VisibleIndex, Is.EqualTo(1));
            Assert.That(lines[2].VisibleIndex, Is.EqualTo(2));
        }

        [Test]
        public void MultipleColumns_MaintainCorrectVisibleIndex()
        {
            // Act
            _builder.AddColumn("Column 1")
                    .AddColumn("Column 2")
                    .AddColumn("Column 3");

            var result = _builder.Build();

            // Assert
            var columns = result.Columns.ToList();
            Assert.That(columns[0].VisibleIndex, Is.EqualTo(0));
            Assert.That(columns[1].VisibleIndex, Is.EqualTo(1));
            Assert.That(columns[2].VisibleIndex, Is.EqualTo(2));
        }
    }
}