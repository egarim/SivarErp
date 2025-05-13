using NUnit.Framework;
using Sivar.Erp.FinancialStatements;
using Sivar.Erp.FinancialStatements.Generation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests.FinancialStatements
{
    [TestFixture]
    public class BalanceSheetBuilderTests
    {
        [Test]
        public void Build_ShouldReturnBalanceSheetWithCorrectAsOfDate()
        {
            // Arrange
            var asOfDate = new DateOnly(2023, 12, 31);
            var builder = new BalanceSheetBuilder(asOfDate);

            // Act
            var result = builder.Build();

            // Assert
            Assert.That(result.AsOfDate, Is.EqualTo(asOfDate));
        }

        [Test]
        public void AddLine_ShouldIncludeLinesInOutput()
        {
            // Arrange
            var builder = new BalanceSheetBuilder(new DateOnly(2023, 12, 31));
            var line1 = new BalanceSheetLineDto { PrintedNo = "1000", LineText = "Cash" };
            var line2 = new BalanceSheetLineDto { PrintedNo = "2000", LineText = "Accounts Payable" };

            // Act
            builder.AddLine(line1).AddLine(line2);
            var result = builder.Build();

            // Assert
            Assert.That(result.Lines.Count(), Is.EqualTo(2));
            Assert.That(result.Lines.Any(l => l.PrintedNo == "1000"), Is.True);
            Assert.That(result.Lines.Any(l => l.PrintedNo == "2000"), Is.True);
        }

        [Test]
        public void SetCompanyHeader_ShouldSetHeaderInOutput()
        {
            // Arrange
            var builder = new BalanceSheetBuilder(new DateOnly(2023, 12, 31));
            var header = new CompanyHeaderDto
            {
                CompanyName = "Test Company",
                Address = "123 Test St",
                Currency = "USD"
            };

            // Act
            builder.SetCompanyHeader(header);
            var result = builder.Build();

            // Assert
            Assert.That(result.CompanyHeader.CompanyName, Is.EqualTo("Test Company"));
            Assert.That(result.CompanyHeader.Address, Is.EqualTo("123 Test St"));
            Assert.That(result.CompanyHeader.Currency, Is.EqualTo("USD"));
        }

        [Test]
        public void AddNotes_ShouldAddNotesToOutput()
        {
            // Arrange
            var builder = new BalanceSheetBuilder(new DateOnly(2023, 12, 31));
            var notes = new[] { "Note 1", "Note 2" };

            // Act
            builder.AddNotes(notes);
            var result = builder.Build();

            // Assert
            Assert.That(result.Notes.Count(), Is.EqualTo(2));
            Assert.That(result.Notes.Contains("Note 1"), Is.True);
            Assert.That(result.Notes.Contains("Note 2"), Is.True);
        }

        [Test]
        public void AddAccountBalances_ShouldCalculateLineAmounts()
        {
            // Arrange
            var builder = new BalanceSheetBuilder(new DateOnly(2023, 12, 31));
            var accountId1 = Guid.NewGuid();
            var accountId2 = Guid.NewGuid();

            var line = new BalanceSheetLineDto
            {
                PrintedNo = "1000",
                LineText = "Cash",
                AccountIds = new[] { accountId1, accountId2 }
            };

            var balances = new Dictionary<string, decimal>
            {
                { accountId1.ToString(), 100M },
                { accountId2.ToString(), 50M }
            };

            // Act
            builder.AddLine(line);
            builder.AddAccountBalances(balances);
            var result = builder.Build();

            // Assert
            var resultLine = result.Lines.FirstOrDefault(l => l.PrintedNo == "1000");
            Assert.That(resultLine, Is.Not.Null);
            Assert.That(resultLine.Amount, Is.EqualTo(150M));
        }

        [Test]
        public void AddAccountGrouping_ShouldCalculateGroupedAmounts()
        {
            // Arrange
            var builder = new BalanceSheetBuilder(new DateOnly(2023, 12, 31));
            var accountId1 = Guid.NewGuid();
            var accountId2 = Guid.NewGuid();

            var line = new BalanceSheetLineDto
            {
                PrintedNo = "CASH_GROUP",
                LineText = "Cash and Equivalents"
            };

            var balances = new Dictionary<string, decimal>
            {
                { accountId1.ToString(), 100M },
                { accountId2.ToString(), 50M }
            };

            // Act
            builder.AddLine(line);
            builder.AddAccountGrouping("CASH_GROUP", new[] { accountId1, accountId2 });
            builder.AddAccountBalances(balances);
            var result = builder.Build();

            // Assert
            var resultLine = result.Lines.FirstOrDefault(l => l.PrintedNo == "CASH_GROUP");
            Assert.That(resultLine, Is.Not.Null);
            Assert.That(resultLine.Amount, Is.EqualTo(150M));
        }

        [Test]
        public void SetRetainedEarnings_ShouldSetAmountOnSpecificLine()
        {
            // Arrange
            var builder = new BalanceSheetBuilder(new DateOnly(2023, 12, 31));
            var line = new BalanceSheetLineDto
            {
                PrintedNo = "3000",
                LineText = "Retained Earnings"
            };

            // Act
            builder.AddLine(line);
            builder.SetRetainedEarnings(75000M, "3000");
            var result = builder.Build();

            // Assert
            var resultLine = result.Lines.FirstOrDefault(l => l.PrintedNo == "3000");
            Assert.That(resultLine, Is.Not.Null);
            Assert.That(resultLine.Amount, Is.EqualTo(75000M));
        }

        [Test]
        public void CalculateHierarchy_ShouldSumUpChildrenToParents()
        {
            // Arrange
            var builder = new BalanceSheetBuilder(new DateOnly(2023, 12, 31));

            // Header line
            var headerLine = new BalanceSheetLineDto
            {
                PrintedNo = "1000",
                LineText = "Current Assets",
                IsHeader = true,
                IndentLevel = 0
            };

            // Child lines
            var childLine1 = new BalanceSheetLineDto
            {
                PrintedNo = "1100",
                LineText = "Cash",
                Amount = 5000M,
                IndentLevel = 1
            };

            var childLine2 = new BalanceSheetLineDto
            {
                PrintedNo = "1200",
                LineText = "Accounts Receivable",
                Amount = 7500M,
                IndentLevel = 1
            };

            // Act
            builder.AddLine(headerLine).AddLine(childLine1).AddLine(childLine2);
            var result = builder.Build();

            // Assert
            var resultHeader = result.Lines.FirstOrDefault(l => l.PrintedNo == "1000");
            Assert.That(resultHeader, Is.Not.Null);
            Assert.That(resultHeader.Amount, Is.EqualTo(12500M)); // Sum of child lines
        }

        [Test]
        public void CalculateTotals_ShouldComputeCorrectTotals()
        {
            // Arrange
            var builder = new BalanceSheetBuilder(new DateOnly(2023, 12, 31));

            // Assets
            builder.AddLine(new BalanceSheetLineDto
            {
                PrintedNo = "1000",
                LineText = "Cash",
                Amount = 10000M,
                LineType = BalanceIncomeLineType.BalanceLine
            });

            builder.AddLine(new BalanceSheetLineDto
            {
                PrintedNo = "1100",
                LineText = "Accounts Receivable",
                Amount = 5000M,
                LineType = BalanceIncomeLineType.BalanceLine
            });

            // Liabilities (negative amounts)
            builder.AddLine(new BalanceSheetLineDto
            {
                PrintedNo = "2000",
                LineText = "Accounts Payable",
                Amount = -8000M,
                LineType = BalanceIncomeLineType.BalanceLine
            });

            // Equity (negative amount)
            builder.AddLine(new BalanceSheetLineDto
            {
                PrintedNo = "3000",
                LineText = "Owner's Equity",
                Amount = -7000M,
                LineType = BalanceIncomeLineType.BalanceLine
            });

            // Act
            var result = builder.Build();

            // Assert
            Assert.That(result.TotalAssets, Is.EqualTo(15000M));
            Assert.That(result.TotalLiabilitiesAndEquity, Is.EqualTo(15000M));
            Assert.That(result.GetBalancingDifference(), Is.EqualTo(0M));
            Assert.That(result.IsBalanced(), Is.True);
        }
    }
}