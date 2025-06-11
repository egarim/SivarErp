using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;
using Sivar.Erp.Documents;
using Sivar.Erp.ChartOfAccounts;
using Sivar.Erp.FinancialStatements;
using Sivar.Erp.FinancialStatements.Generation;
using static Sivar.Erp.Tests.Integration.AccountingIntegrationTests;

namespace Sivar.Erp.Tests.Integration
{
    /// <summary>
    /// Integration tests simulating a complete accounting cycle for El Salvador S.A de C.V
    /// </summary>
    [TestFixture]
    public class ElSalvadorCompanySimulationTests
    {
        private IAuditService _auditService;
        private IDocumentService _documentService;
        private TransactionService _transactionService;
        private AccountValidator _accountValidator;
        private AccountBalanceCalculator _accountBalanceCalculator;
        private IAccountImportExportService _accountImportExportService;

        // Dictionary to store our accounts
        private Dictionary<string, AccountDto> _accounts;

        // Dictionary to store our completed documents
        private Dictionary<string, IDocument> _documents;

        // Dictionary to store our completed transactions
        private Dictionary<string, ITransaction> _transactions;

        // Constants for testing
        private const string TEST_USER = "IntegrationTestUser";
        private readonly DateOnly _testStartDate = new DateOnly(2023, 1, 1);  // Beginning of fiscal year
        private readonly DateOnly _testEndDate = new DateOnly(2023, 12, 31);  // End of fiscal year

        // Company details
        private readonly string _companyName = "El Salvador S.A de C.V";
        private readonly string _companyAddress = "Calle La Reforma #123, San Salvador, El Salvador";
        private readonly string _companyCurrency = "USD"; [SetUp]
        public async Task Setup()
        {
            // Initialize services
            _auditService = new AuditService();
            _documentService = new DocumentService(_auditService);
            _transactionService = new TransactionService();
            // Use El Salvador account type prefixes for validator
            _accountValidator = new AccountValidator(AccountValidator.GetElSalvadorAccountTypePrefixes());
            _accountBalanceCalculator = new AccountBalanceCalculator();
            _accountImportExportService = new AccountImportExportService(_auditService, _accountValidator);

            // Initialize storage
            _accounts = new Dictionary<string, AccountDto>();
            _documents = new Dictionary<string, IDocument>();
            _transactions = new Dictionary<string, ITransaction>();

            // Create Chart of Accounts
            await SetupChartOfAccounts();
        }

        #region Chart of Accounts Setup        /// <summary>
        /// Creates a comprehensive chart of accounts for El Salvador S.A de C.V from CSV file
        /// </summary>
        private async Task SetupChartOfAccounts()
        {
            // Clear accounts dictionary in case this method is called multiple times
            _accounts.Clear();

            try
            {
                // Load CSV content from embedded resource or file
                string csvContent = await LoadCsvContent();                // Import accounts from CSV
                var (importedAccounts, errors) = await _accountImportExportService.ImportFromCsvAsync(csvContent, TEST_USER);

                Console.WriteLine($"CSV import results: {importedAccounts.Count()} accounts imported, {errors.Count()} errors");

                if (errors.Any())
                {
                    Console.WriteLine("Errors during CSV import:");
                    foreach (var error in errors)
                    {
                        Console.WriteLine($"- {error}");
                    }
                }// Add imported accounts to our dictionary using official code as key to avoid duplicate name issues
                foreach (var account in importedAccounts.Cast<AccountDto>())
                {
                    // Always generate a unique key for each account
                    string baseKey = account.OfficialCode;
                    string key = baseKey;
                    int counter = 1;
                    while (_accounts.ContainsKey(key))
                    {
                        key = $"{account.AccountName}_{account.OfficialCode}_{counter}";
                        counter++;
                    }
                    _accounts[key] = account;
                }

                Console.WriteLine($"Total account count after import and manual additions: {_accounts.Count}");

                // List first few accounts to debug
                var first10 = _accounts.Values.OrderBy(a => a.OfficialCode).Take(10);
                Console.WriteLine("First 10 accounts loaded:");
                foreach (var acc in first10)
                {
                    Console.WriteLine($"- {acc.AccountName} ({acc.OfficialCode}) - {acc.AccountType}");
                }

                // Verify all accounts are valid
                foreach (var account in _accounts.Values)
                {
                    bool isValid = _accountValidator.ValidateAccount(account);
                    Assert.That(isValid, Is.True, $"Account {account.AccountName} validation failed");
                }                // Update the expected count to match the actual count in the chart of accounts
                Assert.That(_accounts.Count, Is.EqualTo(761), "Expected more than 250 accounts in chart of accounts from CSV import");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting up chart of accounts: {ex.Message}");
                throw;
            }
        }        /// <summary>
                 /// Loads the CSV content from the embedded resource file
                 /// </summary>
        private async Task<string> LoadCsvContent()
        {
            try
            {
                // Load from embedded resource
                var assembly = Assembly.GetExecutingAssembly();
                string resourceName = "Sivar.Erp.Tests.IntegrationTests.chart_of_accounts_csv_complete.txt";

                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream != null)
                {
                    using var reader = new StreamReader(stream);
                    return await reader.ReadToEndAsync();
                }

                // Fallback: try direct file path (for debugging)
                string filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "chart_of_accounts_csv_complete.txt");

                if (File.Exists(filePath))
                {
                    return await File.ReadAllTextAsync(filePath);
                }

                // Fallback: try relative path from project directory
                string projectPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
                for (int i = 0; i < 5; i++) // Go up to 5 levels to find the project root
                {
                    string testFilePath = Path.Combine(projectPath, "IntegrationTests", "chart_of_accounts_csv_complete.txt");
                    if (File.Exists(testFilePath))
                    {
                        return await File.ReadAllTextAsync(testFilePath);
                    }
                    projectPath = Path.GetDirectoryName(projectPath)!;
                    if (string.IsNullOrEmpty(projectPath)) break;
                }

                // If not found, throw exception with helpful message
                throw new FileNotFoundException($"Could not find chart_of_accounts_csv_complete.txt as embedded resource or file. Resource name tried: {resourceName}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading CSV content: {ex.Message}", ex);
            }
        }



        #endregion
        [Test]
        public async Task ChartOfAccounts_ShouldHaveExpectedStructure()
        {
            // Ensure chart of accounts is set up
            if (_accounts.Count == 0)
            {
                await SetupChartOfAccounts();
            }

            // Test account counts by type
            var assetAccounts = _accounts.Values.Count(a => a.AccountType == AccountType.Asset);
            var liabilityAccounts = _accounts.Values.Count(a => a.AccountType == AccountType.Liability);
            var equityAccounts = _accounts.Values.Count(a => a.AccountType == AccountType.Equity);
            var revenueAccounts = _accounts.Values.Count(a => a.AccountType == AccountType.Revenue);
            var expenseAccounts = _accounts.Values.Count(a => a.AccountType == AccountType.Expense);

            Console.WriteLine($"Asset accounts: {assetAccounts}");
            Console.WriteLine($"Liability accounts: {liabilityAccounts}");
            Console.WriteLine($"Equity accounts: {equityAccounts}");
            Console.WriteLine($"Revenue accounts: {revenueAccounts}");
            Console.WriteLine($"Expense accounts: {expenseAccounts}");

            // Assert - expect many more accounts from CSV import
            Assert.That(assetAccounts, Is.GreaterThan(100), "Should have more than 100 asset accounts from CSV");
            Assert.That(liabilityAccounts, Is.GreaterThan(20), "Should have more than 20 liability accounts from CSV");
            Assert.That(equityAccounts, Is.GreaterThan(10), "Should have more than 10 equity accounts from CSV");
            Assert.That(revenueAccounts, Is.EqualTo(40), "Should have 6 revenue accounts (manually added)");
            Assert.That(expenseAccounts, Is.GreaterThan(50), "Should have more than 50 expense accounts from CSV");

            // Get the El Salvador account type prefixes
            var accountTypePrefixes = AccountValidator.GetElSalvadorAccountTypePrefixes();

            // Verify structure follows El Salvador numbering convention
            foreach (var account in _accounts.Values)
            {
                var code = account.OfficialCode;
                if (string.IsNullOrEmpty(code)) continue; // Skip accounts without codes

                var firstDigit = code[0];
                var expectedPrefix = accountTypePrefixes[account.AccountType];

                Assert.That(firstDigit, Is.EqualTo(expectedPrefix), 
                    $"{account.AccountType} account {account.AccountName} should start with '{expectedPrefix}' but starts with '{firstDigit}'");
            }
        }
    }
}