using NUnit.Framework;
using Sivar.Erp.Services.Accounting.BalanceCalculators;
using Sivar.Erp.Services.Accounting.ChartOfAccounts;

namespace Tests.ChartOfAccounts
{
    /// <summary>
    /// Unit tests for the Chart of Accounts module
    /// </summary>
    [TestFixture]
    public class AccountTests
    {
        #region AccountDto Tests

        [Test]
        public void AccountDto_NewAccount_HasEmptyGuid()
        {
            // Arrange & Act
            var account = new AccountDto();

            // Assert
            Assert.That(account.Oid, Is.EqualTo(Guid.Empty));
        }

        [Test]
        public void AccountDto_Validate_RequiresAccountName()
        {
            // Arrange
            var account = new AccountDto
            {
                AccountName = "",
                OfficialCode = "12345",
                AccountType = AccountType.Asset
            };

            // Act
            bool isValid = account.Validate();

            // Assert
            Assert.That(isValid, Is.False);
        }

        [Test]
        public void AccountDto_Validate_RequiresOfficialCode()
        {
            // Arrange
            var account = new AccountDto
            {
                AccountName = "Cash",
                OfficialCode = "",
                AccountType = AccountType.Asset
            };

            // Act
            bool isValid = account.Validate();

            // Assert
            Assert.That(isValid, Is.False);
        }

        [Test]
        public void AccountDto_Validate_ValidAccount_ReturnsTrue()
        {
            // Arrange
            var account = new AccountDto
            {
                AccountName = "Cash",
                OfficialCode = "12345",
                AccountType = AccountType.Asset
            };

            // Act
            bool isValid = account.Validate();

            // Assert
            Assert.That(isValid, Is.True);
        }

        [Test]
        public void AccountDto_HasDebitBalance_ForAssetAccounts_ReturnsTrue()
        {
            // Arrange
            var account = new AccountDto
            {
                AccountType = AccountType.Asset
            };

            // Act
            bool hasDebitBalance = account.HasDebitBalance();

            // Assert
            Assert.That(hasDebitBalance, Is.True);
        }

        [Test]
        public void AccountDto_HasDebitBalance_ForExpenseAccounts_ReturnsTrue()
        {
            // Arrange
            var account = new AccountDto
            {
                AccountType = AccountType.Expense
            };

            // Act
            bool hasDebitBalance = account.HasDebitBalance();

            // Assert
            Assert.That(hasDebitBalance, Is.True);
        }

        [Test]
        public void AccountDto_HasDebitBalance_ForLiabilityAccounts_ReturnsFalse()
        {
            // Arrange
            var account = new AccountDto
            {
                AccountType = AccountType.Liability
            };

            // Act
            bool hasDebitBalance = account.HasDebitBalance();

            // Assert
            Assert.That(hasDebitBalance, Is.False);
        }

        [Test]
        public void AccountDto_HasDebitBalance_ForEquityAccounts_ReturnsFalse()
        {
            // Arrange
            var account = new AccountDto
            {
                AccountType = AccountType.Equity
            };

            // Act
            bool hasDebitBalance = account.HasDebitBalance();

            // Assert
            Assert.That(hasDebitBalance, Is.False);
        }

        [Test]
        public void AccountDto_HasDebitBalance_ForRevenueAccounts_ReturnsFalse()
        {
            // Arrange
            var account = new AccountDto
            {
                AccountType = AccountType.Revenue
            };

            // Act
            bool hasDebitBalance = account.HasDebitBalance();

            // Assert
            Assert.That(hasDebitBalance, Is.False);
        }

        [Test]
        public void AccountDto_Archive_SetsIsArchivedToTrue()
        {
            // Arrange
            var account = new AccountDto
            {
                IsArchived = false
            };

            // Act
            account.Archive();

            // Assert
            Assert.That(account.IsArchived, Is.True);
        }

        [Test]
        public void AccountDto_Restore_SetsIsArchivedToFalse()
        {
            // Arrange
            var account = new AccountDto
            {
                IsArchived = true
            };

            // Act
            account.Restore();

            // Assert
            Assert.That(account.IsArchived, Is.False);
        }

        [Test]
        public void AccountDto_CanUseForTransactions_WhenArchived_ReturnsFalse()
        {
            // Arrange
            var account = new AccountDto
            {
                IsArchived = true
            };

            // Act
            bool canUse = account.CanUseForTransactions();

            // Assert
            Assert.That(canUse, Is.False);
        }

        [Test]
        public void AccountDto_CanUseForTransactions_WhenNotArchived_ReturnsTrue()
        {
            // Arrange
            var account = new AccountDto
            {
                IsArchived = false
            };

            // Act
            bool canUse = account.CanUseForTransactions();

            // Assert
            Assert.That(canUse, Is.True);
        }

        #endregion

        #region AccountValidator Tests

        [Test]
        public void AccountValidator_ValidateAccountCode_EmptyCode_ReturnsFalse()
        {
            // Arrange
            var validator = new AccountValidator();
            string accountCode = "";
            var accountType = AccountType.Asset;

            // Act
            bool isValid = validator.ValidateAccountCode(accountCode, accountType);

            // Assert
            Assert.That(isValid, Is.False);
        }

        [Test]
        public void AccountValidator_ValidateAccountCode_NonNumericCode_ReturnsFalse()
        {
            // Arrange
            var validator = new AccountValidator();
            string accountCode = "1A234";
            var accountType = AccountType.Asset;

            // Act
            bool isValid = validator.ValidateAccountCode(accountCode, accountType);

            // Assert
            Assert.That(isValid, Is.False);
        }

        [Test]
        public void AccountValidator_ValidateAccountCode_AssetWithCorrectPrefix_ReturnsTrue()
        {
            // Arrange
            var validator = new AccountValidator();
            string accountCode = "12345";
            var accountType = AccountType.Asset;

            // Act
            bool isValid = validator.ValidateAccountCode(accountCode, accountType);

            // Assert
            Assert.That(isValid, Is.True);
        }

        [Test]
        public void AccountValidator_ValidateAccountCode_AssetWithIncorrectPrefix_ReturnsFalse()
        {
            // Arrange
            var validator = new AccountValidator();
            string accountCode = "22345"; // Starts with 2, which is for Liability
            var accountType = AccountType.Asset;

            // Act
            bool isValid = validator.ValidateAccountCode(accountCode, accountType);

            // Assert
            Assert.That(isValid, Is.False);
        }

        [Test]
        public void AccountValidator_ValidateAccountCode_LiabilityWithCorrectPrefix_ReturnsTrue()
        {
            // Arrange
            var validator = new AccountValidator();
            string accountCode = "22345";
            var accountType = AccountType.Liability;

            // Act
            bool isValid = validator.ValidateAccountCode(accountCode, accountType);

            // Assert
            Assert.That(isValid, Is.True);
        }

        [Test]
        public void AccountValidator_ValidateFinancialStatementLine_NoLine_ReturnsTrue()
        {
            // Arrange
            var validator = new AccountValidator();
            Guid? lineId = null;
            var accountType = AccountType.Asset;

            // Act
            bool isValid = validator.ValidateFinancialStatementLine(accountType, lineId);

            // Assert
            Assert.That(isValid, Is.True);
        }

        [Test]
        public void AccountValidator_ValidateAccount_ValidAccount_ReturnsTrue()
        {
            // Arrange
            var validator = new AccountValidator();
            var account = new AccountDto
            {
                AccountName = "Cash",
                OfficialCode = "12345",
                AccountType = AccountType.Asset
            };

            // Act
            bool isValid = validator.ValidateAccount(account);

            // Assert
            Assert.That(isValid, Is.True);
        }

        [Test]
        public void AccountValidator_ValidateAccount_InvalidAccountCode_ReturnsFalse()
        {
            // Arrange
            var validator = new AccountValidator();
            var account = new AccountDto
            {
                AccountName = "Cash",
                OfficialCode = "22345", // Invalid prefix for Asset
                AccountType = AccountType.Asset
            };

            // Act
            bool isValid = validator.ValidateAccount(account);

            // Assert
            Assert.That(isValid, Is.False);
        }

        #endregion

        #region AccountBalanceCalculator Tests

        [Test]
        public void AccountBalanceCalculator_CalculateAccountBalance_ReturnsZeroForNewAccount()
        {
            // Arrange
            var calculator = new AccountBalanceCalculatorServiceBase();
            var accountId = Guid.NewGuid();
            var asOfDate = DateOnly.FromDateTime(DateTime.Today);

            // Act
            decimal balance = calculator.CalculateAccountBalance(accountId, asOfDate);

            // Assert
            Assert.That(balance, Is.EqualTo(0m));
        }

        [Test]
        public void AccountBalanceCalculator_CalculateAccountTurnover_ReturnsZerosForNewAccount()
        {
            // Arrange
            var calculator = new AccountBalanceCalculatorServiceBase();
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
        public void AccountBalanceCalculator_HasTransactions_ReturnsFalseForNewAccount()
        {
            // Arrange
            var calculator = new AccountBalanceCalculatorServiceBase();
            var accountId = Guid.NewGuid();

            // Act
            bool hasTransactions = calculator.HasTransactions(accountId);

            // Assert
            Assert.That(hasTransactions, Is.False);
        }

        #endregion

        #region Integration Tests

        // These tests would require a test database or mock repository
        // For a real implementation, you might use an in-memory database or mocks

        [Test]
        public void Integration_CreateAndValidateAccount()
        {
            // Arrange
            var account = new AccountDto
            {
                Oid = Guid.NewGuid(),
                AccountName = "Cash",
                OfficialCode = "12345",
                AccountType = AccountType.Asset,
                InsertedAt = DateTime.UtcNow,
                InsertedBy = "Test User",
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = "Test User",
                IsArchived = false
            };

            var validator = new AccountValidator();

            // Act
            bool isValid = validator.ValidateAccount(account);

            // Assert
            Assert.That(isValid, Is.True);
            Assert.That(account.HasDebitBalance(), Is.True);
            Assert.That(account.CanUseForTransactions(), Is.True);
        }

        #endregion
    }
}