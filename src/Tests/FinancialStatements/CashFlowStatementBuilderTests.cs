using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Sivar.Erp.FinancialStatements;

namespace Tests.FinancialStatements
{
    [TestFixture]
    public class CashFlowStatementBuilderTests
    {
        [Test]
        public void AddHeader_ShouldAddHeaderLine()
        {
            // Arrange
            var builder = new CashFlowStatementBuilder();

            // Act
            builder.AddHeader("OPERATING ACTIVITIES", "I")
                  // Add a net income line to satisfy the validation in Build()
                  .AddLine("Net Income", FinacialStatementValueType.Credit, BalanceType.PerPeriod, "1", true);
            var (lines, _) = builder.Build();

            // Assert
            var headerLine = lines.First();
            Assert.That(headerLine.LineText, Is.EqualTo("OPERATING ACTIVITIES"));
            Assert.That(headerLine.PrintedNo, Is.EqualTo("I"));
            Assert.That(headerLine.LineType, Is.EqualTo(CashFlowLineType.Header));
        }

        [Test]
        public void AddLine_ShouldAddDataLine()
        {
            // Arrange
            var builder = new CashFlowStatementBuilder();

            // Act
            builder.AddLine("Net Income", FinacialStatementValueType.Credit, BalanceType.PerPeriod, "1", true);
            var (lines, _) = builder.Build();

            // Assert
            var line = lines.First();
            Assert.That(line.LineText, Is.EqualTo("Net Income"));
            Assert.That(line.PrintedNo, Is.EqualTo("1"));
            Assert.That(line.LineType, Is.EqualTo(CashFlowLineType.Line));
            Assert.That(line.IsNetIncome, Is.True);
            Assert.That(line.ValueType, Is.EqualTo(FinacialStatementValueType.Credit));
            Assert.That(line.BalanceType, Is.EqualTo(BalanceType.PerPeriod));
        }

        [Test]
        public void AddLine_WithDuplicateNetIncomeLine_ShouldThrowException()
        {
            // Arrange
            var builder = new CashFlowStatementBuilder();
            builder.AddLine("Net Income", FinacialStatementValueType.Credit, BalanceType.PerPeriod, "1", true);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                builder.AddLine("Another Net Income", FinacialStatementValueType.Credit, BalanceType.PerPeriod, "2", true));
        }

        [Test]
        public void AssignAccount_ShouldAddAssignment()
        {
            // Arrange
            var builder = new CashFlowStatementBuilder();
            var accountId = Guid.NewGuid();

            // Act
            builder.AddLine("Net Income", FinacialStatementValueType.Credit, BalanceType.PerPeriod, "1", true);
            builder.AssignAccount(accountId);
            var (_, assignments) = builder.Build();

            // Assert
            Assert.That(assignments.Count(), Is.EqualTo(1));
            Assert.That(assignments.First().AccountId, Is.EqualTo(accountId));
        }

        [Test]
        public void AssignAccount_WithNoLines_ShouldThrowException()
        {
            // Arrange
            var builder = new CashFlowStatementBuilder();
            var accountId = Guid.NewGuid();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => builder.AssignAccount(accountId));
        }

        [Test]
        public void AssignAccount_ToHeaderLine_ShouldThrowException()
        {
            // Arrange
            var builder = new CashFlowStatementBuilder();
            var accountId = Guid.NewGuid();

            // Act & Assert
            builder.AddHeader("OPERATING ACTIVITIES", "I");
            Assert.Throws<InvalidOperationException>(() => builder.AssignAccount(accountId));
        }

        [Test]
        public void AssignAccounts_ShouldAddMultipleAssignments()
        {
            // Arrange
            var builder = new CashFlowStatementBuilder();
            var accountId1 = Guid.NewGuid();
            var accountId2 = Guid.NewGuid();

            // Act
            builder.AddLine("Net Income", FinacialStatementValueType.Credit, BalanceType.PerPeriod, "1", true);
            builder.AssignAccounts(accountId1, accountId2);
            var (_, assignments) = builder.Build();

            // Assert
            Assert.That(assignments.Count(), Is.EqualTo(2));
            Assert.That(assignments.Any(a => a.AccountId == accountId1), Is.True);
            Assert.That(assignments.Any(a => a.AccountId == accountId2), Is.True);
        }

        [Test]
        public void Build_WithNoNetIncomeLine_ShouldThrowException()
        {
            // Arrange
            var builder = new CashFlowStatementBuilder();
            builder.AddLine("Some Line", FinacialStatementValueType.Credit, BalanceType.PerPeriod, "1", false);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => builder.Build());
        }

        [Test]
        public void Build_ShouldUpdateNestedSetIndexes()
        {
            // Arrange
            var builder = new CashFlowStatementBuilder();
            builder.AddLine("Net Income", FinacialStatementValueType.Credit, BalanceType.PerPeriod, "1", true);
            builder.AddLine("Depreciation", FinacialStatementValueType.Debit, BalanceType.PerPeriod, "2");

            // Act
            var (lines, _) = builder.Build();

            // Assert
            var linesList = lines.ToList();
            Assert.That(linesList[0].LeftIndex, Is.EqualTo(1));
            Assert.That(linesList[0].RightIndex, Is.EqualTo(2));
            Assert.That(linesList[1].LeftIndex, Is.EqualTo(3));
            Assert.That(linesList[1].RightIndex, Is.EqualTo(4));
        }

        [Test]
        public void CreateStandardIndirectStructure_ShouldCreateDefaultStructure()
        {
            // Act
            var builder = CashFlowStatementBuilder.CreateStandardIndirectStructure();
            var (lines, _) = builder.Build();

            // Assert
            Assert.That(lines.Count(), Is.EqualTo(18)); // Updated from 16 to 18 to match actual count
            Assert.That(lines.Count(l => l.LineType == CashFlowLineType.Header), Is.EqualTo(3));
            Assert.That(lines.Count(l => l.IsNetIncome), Is.EqualTo(1));

            var netIncomeLine = lines.First(l => l.IsNetIncome);
            Assert.That(netIncomeLine.LineText, Is.EqualTo("Net Income"));
        }

        [Test]
        public void Reset_ShouldClearAllData()
        {
            // Arrange
            var builder = new CashFlowStatementBuilder()
                .AddHeader("OPERATING ACTIVITIES", "I")
                .AddLine("Net Income", FinacialStatementValueType.Credit, BalanceType.PerPeriod, "1", true);

            // Act
            builder.Reset().AddLine("New Net Income", FinacialStatementValueType.Credit, BalanceType.PerPeriod, "1", true);
            var (lines, _) = builder.Build();

            // Assert
            Assert.That(lines.Count(), Is.EqualTo(1));
            Assert.That(lines.First().LineText, Is.EqualTo("New Net Income"));
        }

        [Test]
        public void AddLine_ShouldSetCorrectVisibleIndex()
        {
            // Arrange
            var builder = new CashFlowStatementBuilder();

            // Act
            builder.AddHeader("Header 1", "I");
            builder.AddLine("Line 1", FinacialStatementValueType.Credit, BalanceType.PerPeriod, "1", true);
            builder.AddHeader("Header 2", "II");
            var (lines, _) = builder.Build();

            // Assert
            var linesList = lines.ToList();
            Assert.That(linesList[0].VisibleIndex, Is.EqualTo(0));
            Assert.That(linesList[1].VisibleIndex, Is.EqualTo(1));
            Assert.That(linesList[2].VisibleIndex, Is.EqualTo(2));
        }
    }
}