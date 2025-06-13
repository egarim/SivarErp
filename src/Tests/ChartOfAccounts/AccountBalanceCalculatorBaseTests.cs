using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Sivar.Erp.ChartOfAccounts;
using Sivar.Erp.Documents;

namespace Tests.ChartOfAccounts
{
    [TestFixture]
    public class AccountBalanceCalculatorBaseTests
    {
        private Guid _assetAccountId;
        private Guid _liabilityAccountId;
        private Guid _revenueAccountId;
        private Guid _expenseAccountId;
        private Guid _documentId;
        private List<ITransaction> _transactions;

        [SetUp]
        public void Setup()
        {
            // Generate GUIDs for accounts
            _assetAccountId = Guid.NewGuid();
            _liabilityAccountId = Guid.NewGuid();
            _revenueAccountId = Guid.NewGuid();
            _expenseAccountId = Guid.NewGuid();
            _documentId = Guid.NewGuid();
            
            // Create test transactions with ledger entries
            _transactions = CreateTestTransactions();
        }

        #region Test Empty Collection Behavior

        [Test]
        public void CalculateAccountBalance_WithEmptyCollection_ReturnsZero()
        {
            // Arrange
            var calculator = new AccountBalanceCalculatorBase();
            var accountId = Guid.NewGuid();
            var asOfDate = DateOnly.FromDateTime(DateTime.Today);

            // Act
            decimal balance = calculator.CalculateAccountBalance(accountId, asOfDate);

            // Assert
            Assert.That(balance, Is.EqualTo(0m));
        }

        [Test]
        public void CalculateAccountTurnover_WithEmptyCollection_ReturnsZeros()
        {
            // Arrange
            var calculator = new AccountBalanceCalculatorBase();
            var accountId = Guid.NewGuid();
            var startDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-30));
            var endDate = DateOnly.FromDateTime(DateTime.Today);

            // Act
            var (debitTurnover, creditTurnover) = calculator.CalculateAccountTurnover(accountId, startDate, endDate);

            // Assert
            Assert.That(debitTurnover, Is.EqualTo(0m));
            Assert.That(creditTurnover, Is.EqualTo(0m));
        }

        [Test]
        public void HasTransactions_WithEmptyCollection_ReturnsFalse()
        {
            // Arrange
            var calculator = new AccountBalanceCalculatorBase();
            var accountId = Guid.NewGuid();

            // Act
            bool hasTransactions = calculator.HasTransactions(accountId);

            // Assert
            Assert.That(hasTransactions, Is.False);
        }

        #endregion
        
        #region Test with Transactions

        [Test]
        public void CalculateAccountBalance_WithTransactions_ReturnsCorrectBalance()
        {
            // Arrange
            var calculator = new AccountBalanceCalculatorBase(_transactions);
            var asOfDate = DateOnly.FromDateTime(DateTime.Today.AddDays(10));

            // Act
            decimal assetBalance = calculator.CalculateAccountBalance(_assetAccountId, asOfDate);
            decimal liabilityBalance = calculator.CalculateAccountBalance(_liabilityAccountId, asOfDate);
            decimal revenueBalance = calculator.CalculateAccountBalance(_revenueAccountId, asOfDate);
            decimal expenseBalance = calculator.CalculateAccountBalance(_expenseAccountId, asOfDate);

            // Assert - checking the expected balances based on our test data
            Assert.That(assetBalance, Is.EqualTo(1400m)); // 2000 debit - 600 credit
            Assert.That(liabilityBalance, Is.EqualTo(-1000m)); // 0 debit - 1000 credit
            Assert.That(revenueBalance, Is.EqualTo(-1500m)); // 0 debit - 1500 credit
            Assert.That(expenseBalance, Is.EqualTo(1100m)); // 1100 debit - 0 credit
        }

        [Test]
        public void CalculateAccountBalance_WithDateFiltering_ReturnsCorrectBalance()
        {
            // Arrange
            var calculator = new AccountBalanceCalculatorBase(_transactions);
            
            // Use a date that only includes the first transaction
            var earlyDate = DateOnly.FromDateTime(DateTime.Today);

            // Act
            decimal assetBalance = calculator.CalculateAccountBalance(_assetAccountId, earlyDate);
            decimal liabilityBalance = calculator.CalculateAccountBalance(_liabilityAccountId, earlyDate);

            // Assert
            Assert.That(assetBalance, Is.EqualTo(500m)); // Only include first transaction: 500 debit
            Assert.That(liabilityBalance, Is.EqualTo(-500m)); // Only include first transaction: 500 credit
        }

        [Test]
        public void CalculateAccountTurnover_WithTransactions_ReturnsCorrectTurnover()
        {
            // Arrange
            var calculator = new AccountBalanceCalculatorBase(_transactions);
            var startDate = DateOnly.FromDateTime(DateTime.Today);
            var endDate = DateOnly.FromDateTime(DateTime.Today.AddDays(10));

            // Act
            var (assetDebitTurnover, assetCreditTurnover) = 
                calculator.CalculateAccountTurnover(_assetAccountId, startDate, endDate);
            var (liabilityDebitTurnover, liabilityCreditTurnover) = 
                calculator.CalculateAccountTurnover(_liabilityAccountId, startDate, endDate);
            var (revenueDebitTurnover, revenueCreditTurnover) = 
                calculator.CalculateAccountTurnover(_revenueAccountId, startDate, endDate);
            var (expenseDebitTurnover, expenseCreditTurnover) = 
                calculator.CalculateAccountTurnover(_expenseAccountId, startDate, endDate);

            // Assert
            Assert.That(assetDebitTurnover, Is.EqualTo(2000m));
            Assert.That(assetCreditTurnover, Is.EqualTo(600m));
            
            Assert.That(liabilityDebitTurnover, Is.EqualTo(0m));
            Assert.That(liabilityCreditTurnover, Is.EqualTo(1000m));
            
            Assert.That(revenueDebitTurnover, Is.EqualTo(0m));
            Assert.That(revenueCreditTurnover, Is.EqualTo(1500m));
            
            Assert.That(expenseDebitTurnover, Is.EqualTo(1100m));
            Assert.That(expenseCreditTurnover, Is.EqualTo(0m));
        }

        [Test]
        public void CalculateAccountTurnover_WithDateRangeFiltering_ReturnsCorrectTurnover()
        {
            // Arrange
            var calculator = new AccountBalanceCalculatorBase(_transactions);
            
            // Only include transactions on day 5
            var startDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5));
            var endDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5));

            // Act
            var (assetDebitTurnover, assetCreditTurnover) = 
                calculator.CalculateAccountTurnover(_assetAccountId, startDate, endDate);
            var (expenseDebitTurnover, expenseCreditTurnover) = 
                calculator.CalculateAccountTurnover(_expenseAccountId, startDate, endDate);

            // Assert - only includes the third transaction
            Assert.That(assetDebitTurnover, Is.EqualTo(0m));
            Assert.That(assetCreditTurnover, Is.EqualTo(600m));
            Assert.That(expenseDebitTurnover, Is.EqualTo(600m));
            Assert.That(expenseCreditTurnover, Is.EqualTo(0m));
        }

        [Test]
        public void HasTransactions_WithTransactions_ReturnsCorrectResult()
        {
            // Arrange
            var calculator = new AccountBalanceCalculatorBase(_transactions);
            var nonExistingAccountId = Guid.NewGuid();

            // Act
            bool assetHasTransactions = calculator.HasTransactions(_assetAccountId);
            bool nonExistingHasTransactions = calculator.HasTransactions(nonExistingAccountId);

            // Assert
            Assert.That(assetHasTransactions, Is.True);
            Assert.That(nonExistingHasTransactions, Is.False);
        }

        [Test]
        public void ConstructorWithNullTransactions_ShouldUseEmptyCollection()
        {
            // Arrange
            var calculator = new AccountBalanceCalculatorBase(null);
            var accountId = Guid.NewGuid();

            // Act
            bool hasTransactions = calculator.HasTransactions(accountId);
            decimal balance = calculator.CalculateAccountBalance(accountId, DateOnly.FromDateTime(DateTime.Today));

            // Assert
            Assert.That(hasTransactions, Is.False);
            Assert.That(balance, Is.EqualTo(0m));
        }

        #endregion
        
        #region Helper Methods

        private List<ITransaction> CreateTestTransactions()
        {
            var transactions = new List<ITransaction>();
            
            // Transaction 1 - Initial funding (today)
            var transaction1 = new TransactionDto
            {
                Id = Guid.NewGuid(),
                DocumentId = _documentId,
                TransactionDate = DateOnly.FromDateTime(DateTime.Today),
                Description = "Initial funding",
                LedgerEntries = new List<ILedgerEntry>
                {
                    new LedgerEntryDto
                    {
                        Id = Guid.NewGuid(),
                        TransactionId = Guid.NewGuid(),
                        AccountId = _assetAccountId,
                        EntryType = EntryType.Debit,
                        Amount = 500m,
                        AccountName = "Cash",
                        OfficialCode = "10100"
                    },
                    new LedgerEntryDto
                    {
                        Id = Guid.NewGuid(),
                        TransactionId = Guid.NewGuid(),
                        AccountId = _liabilityAccountId,
                        EntryType = EntryType.Credit,
                        Amount = 500m,
                        AccountName = "Accounts Payable",
                        OfficialCode = "20000"
                    }
                }
            };
            
            // Transaction 2 - Sale (today + 3 days)
            var transaction2 = new TransactionDto
            {
                Id = Guid.NewGuid(),
                DocumentId = _documentId,
                TransactionDate = DateOnly.FromDateTime(DateTime.Today.AddDays(3)),
                Description = "Sales transaction",
                LedgerEntries = new List<ILedgerEntry>
                {
                    new LedgerEntryDto
                    {
                        Id = Guid.NewGuid(),
                        TransactionId = Guid.NewGuid(),
                        AccountId = _assetAccountId,
                        EntryType = EntryType.Debit,
                        Amount = 1500m,
                        AccountName = "Cash",
                        OfficialCode = "10100"
                    },
                    new LedgerEntryDto
                    {
                        Id = Guid.NewGuid(),
                        TransactionId = Guid.NewGuid(),
                        AccountId = _revenueAccountId,
                        EntryType = EntryType.Credit,
                        Amount = 1500m,
                        AccountName = "Sales Revenue",
                        OfficialCode = "40000"
                    }
                }
            };
            
            // Transaction 3 - Expense (today + 5 days)
            var transaction3 = new TransactionDto
            {
                Id = Guid.NewGuid(),
                DocumentId = _documentId,
                TransactionDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5)),
                Description = "Office supplies expense",
                LedgerEntries = new List<ILedgerEntry>
                {
                    new LedgerEntryDto
                    {
                        Id = Guid.NewGuid(),
                        TransactionId = Guid.NewGuid(),
                        AccountId = _expenseAccountId,
                        EntryType = EntryType.Debit,
                        Amount = 600m,
                        AccountName = "Office Supplies Expense",
                        OfficialCode = "65000"
                    },
                    new LedgerEntryDto
                    {
                        Id = Guid.NewGuid(),
                        TransactionId = Guid.NewGuid(),
                        AccountId = _assetAccountId,
                        EntryType = EntryType.Credit,
                        Amount = 600m,
                        AccountName = "Cash",
                        OfficialCode = "10100"
                    }
                }
            };
            
            // Transaction 4 - Another expense (today + 7 days)
            var transaction4 = new TransactionDto
            {
                Id = Guid.NewGuid(),
                DocumentId = _documentId,
                TransactionDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7)),
                Description = "Utility bill payment",
                LedgerEntries = new List<ILedgerEntry>
                {
                    new LedgerEntryDto
                    {
                        Id = Guid.NewGuid(),
                        TransactionId = Guid.NewGuid(),
                        AccountId = _expenseAccountId,
                        EntryType = EntryType.Debit,
                        Amount = 500m,
                        AccountName = "Utilities Expense",
                        OfficialCode = "66000"
                    },
                    new LedgerEntryDto
                    {
                        Id = Guid.NewGuid(),
                        TransactionId = Guid.NewGuid(),
                        AccountId = _liabilityAccountId,
                        EntryType = EntryType.Credit,
                        Amount = 500m,
                        AccountName = "Accounts Payable",
                        OfficialCode = "20000"
                    }
                }
            };
            
            transactions.Add(transaction1);
            transactions.Add(transaction2);
            transactions.Add(transaction3);
            transactions.Add(transaction4);
            
            return transactions;
        }

        #endregion
    }
}