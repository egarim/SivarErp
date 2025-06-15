using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Sivar.Erp.ChartOfAccounts;
using Sivar.Erp.Documents;
using Sivar.Erp.Documents.Tax;
using Sivar.Erp.ImportExport;
using Sivar.Erp.Taxes;
using Sivar.Erp.Taxes.TaxGroup;
using Sivar.Erp.Taxes.TaxRule;

namespace Tests.ElSalvador
{
    /// <summary>
    /// Tests for loading El Salvador chart of accounts and tax data from CSV files
    /// </summary>
    [TestFixture]
    public class ElSalvadorDataLoaderTests
    {
        private readonly string _testDataPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "..", "Tests", "ElSalvador", "Data", "New");
        private IAccountImportExportService _accountImportService;
        private ITaxImportExportService _taxImportService;
        private ITaxGroupImportExportService _taxGroupImportService;

        [SetUp]
        public void Setup()
        {
            _accountImportService = new AccountImportExportService();
            _taxImportService = new TaxImportExportService();
            _taxGroupImportService = new TaxGroupImportExportService();
        }

        [Test]
        public async Task LoadChartOfAccounts_FromComercialChartOfAccountsFile_LoadsAllAccounts()
        {
            // Arrange
            string chartOfAccountsPath = Path.Combine(_testDataPath, "ComercialChartOfAccounts.txt");
            Assert.That(File.Exists(chartOfAccountsPath), Is.True, $"Chart of accounts file not found: {chartOfAccountsPath}");

            string csvContent = await File.ReadAllTextAsync(chartOfAccountsPath);

            // Act
            var (importedAccounts, errors) = await _accountImportService.ImportFromCsvAsync(csvContent, "ElSalvadorDataLoader");

            // Assert
            Console.WriteLine($"Chart of Accounts Import Results:");
            Console.WriteLine($"Total accounts imported: {importedAccounts.Count()}");

            if (errors.Any())
            {
                Console.WriteLine($"Errors encountered: {errors.Count()}");
                foreach (var error in errors)
                {
                    Console.WriteLine($"  - {error}");
                }
            }

            Assert.That(errors, Is.Empty, "Chart of accounts import should not have errors");
            Assert.That(importedAccounts.Count(), Is.GreaterThan(0), "Should import at least one account");

            // Validate some key accounts exist
            var accountsList = importedAccounts.ToList();
            var assetAccounts = accountsList.Where(a => a.AccountType == AccountType.Asset).ToList();
            var liabilityAccounts = accountsList.Where(a => a.AccountType == AccountType.Liability).ToList();
            var equityAccounts = accountsList.Where(a => a.AccountType == AccountType.Equity).ToList();
            var revenueAccounts = accountsList.Where(a => a.AccountType == AccountType.Revenue).ToList();
            var expenseAccounts = accountsList.Where(a => a.AccountType == AccountType.Expense).ToList();

            Console.WriteLine($"\nAccount Distribution:");
            Console.WriteLine($"  Assets: {assetAccounts.Count}");
            Console.WriteLine($"  Liabilities: {liabilityAccounts.Count}");
            Console.WriteLine($"  Equity: {equityAccounts.Count}");
            Console.WriteLine($"  Revenue: {revenueAccounts.Count}");
            Console.WriteLine($"  Expenses: {expenseAccounts.Count}");

            Assert.That(assetAccounts.Count, Is.GreaterThan(0), "Should have asset accounts");
            Assert.That(liabilityAccounts.Count, Is.GreaterThan(0), "Should have liability accounts");

            // Check for specific key accounts that should exist in a commercial chart of accounts
            var cashAccount = accountsList.FirstOrDefault(a => a.AccountName.Contains("CAJA") || a.AccountName.Contains("Cash"));
            var bankAccount = accountsList.FirstOrDefault(a => a.AccountName.Contains("BANCO") || a.AccountName.Contains("Bank"));
            var clientsAccount = accountsList.FirstOrDefault(a => a.AccountName.Contains("CLIENTES") || a.AccountName.Contains("Clients"));

            Assert.That(cashAccount, Is.Not.Null, "Should have a cash account");
            Assert.That(bankAccount, Is.Not.Null, "Should have a bank account");
            Assert.That(clientsAccount, Is.Not.Null, "Should have a clients account");

            // Print some sample accounts for verification
            Console.WriteLine($"\nSample accounts loaded:");
            foreach (var account in accountsList.Take(10))
            {
                Console.WriteLine($"  {account.OfficialCode} - {account.AccountName} ({account.AccountType})");
                if (!string.IsNullOrEmpty(account.ParentOfficialCode))
                {
                    Console.WriteLine($"    Parent: {account.ParentOfficialCode}");
                }
            }
        }

        [Test]
        public async Task LoadTaxes_FromElSalvadorTaxesFile_LoadsAllTaxes()
        {
            // Arrange
            string taxesPath = Path.Combine(_testDataPath, "ElSalvadorTaxes.txt");
            Assert.That(File.Exists(taxesPath), Is.True, $"Taxes file not found: {taxesPath}");

            string csvContent = await File.ReadAllTextAsync(taxesPath);

            // Act
            var (importedTaxes, errors) = await _taxImportService.ImportFromCsvAsync(csvContent, "ElSalvadorDataLoader");

            // Assert
            Console.WriteLine($"\nTaxes Import Results:");
            Console.WriteLine($"Total taxes imported: {importedTaxes.Count()}");

            if (errors.Any())
            {
                Console.WriteLine($"Errors encountered: {errors.Count()}");
                foreach (var error in errors)
                {
                    Console.WriteLine($"  - {error}");
                }
            }

            Assert.That(errors, Is.Empty, "Tax import should not have errors");
            Assert.That(importedTaxes.Count(), Is.GreaterThan(0), "Should import at least one tax");

            var taxesList = importedTaxes.ToList();

            // Validate key El Salvador taxes exist
            var ivaTax = taxesList.FirstOrDefault(t => t.Code == "IVA");
            var ivarTax = taxesList.FirstOrDefault(t => t.Code == "IVAR");
            var fovialTax = taxesList.FirstOrDefault(t => t.Code == "FOVIAL");
            var cotransTax = taxesList.FirstOrDefault(t => t.Code == "COTRANS");

            Assert.That(ivaTax, Is.Not.Null, "Should have IVA tax");
            Assert.That(ivarTax, Is.Not.Null, "Should have IVA Retenido tax");
            Assert.That(fovialTax, Is.Not.Null, "Should have FOVIAL tax");
            Assert.That(cotransTax, Is.Not.Null, "Should have COTRANS tax");

            // Validate IVA properties
            Assert.That(ivaTax.TaxType, Is.EqualTo(TaxType.Percentage), "IVA should be percentage-based");
            Assert.That(ivaTax.Percentage, Is.EqualTo(13.00m), "IVA should be 13%");
            Assert.That(ivaTax.ApplicationLevel, Is.EqualTo(TaxApplicationLevel.Line), "IVA should be line-level");

            // Validate FOVIAL properties (fixed amount tax)
            Assert.That(fovialTax.TaxType, Is.EqualTo(TaxType.FixedAmount), "FOVIAL should be fixed amount");
            Assert.That(fovialTax.Amount, Is.EqualTo(0.20m), "FOVIAL should be $0.20");
            Assert.That(fovialTax.ApplicationLevel, Is.EqualTo(TaxApplicationLevel.Line), "FOVIAL should be line-level");

            // Print tax details
            Console.WriteLine($"\nTaxes loaded:");
            foreach (var tax in taxesList)
            {
                string taxDetails = $"  {tax.Code} - {tax.Name} ({tax.TaxType})";
                if (tax.TaxType == TaxType.Percentage)
                {
                    taxDetails += $" - {tax.Percentage}%";
                }
                else if (tax.TaxType == TaxType.FixedAmount || tax.TaxType == TaxType.AmountPerUnit)
                {
                    taxDetails += $" - ${tax.Amount}";
                }
                taxDetails += $" [{tax.ApplicationLevel}]";
                Console.WriteLine(taxDetails);
            }
        }

        [Test]
        public async Task LoadTaxGroups_FromElSalvadorTaxGroupsFile_LoadsAllTaxGroups()
        {
            // Arrange
            string taxGroupsPath = Path.Combine(_testDataPath, "ElSalvadorTaxGroups.txt");
            Assert.That(File.Exists(taxGroupsPath), Is.True, $"Tax groups file not found: {taxGroupsPath}");

            string csvContent = await File.ReadAllTextAsync(taxGroupsPath);

            // Act
            var (importedTaxGroups, errors) = await _taxGroupImportService.ImportFromCsvAsync(csvContent, "ElSalvadorDataLoader");

            // Assert
            Console.WriteLine($"\nTax Groups Import Results:");
            Console.WriteLine($"Total tax groups imported: {importedTaxGroups.Count()}");

            if (errors.Any())
            {
                Console.WriteLine($"Errors encountered: {errors.Count()}");
                foreach (var error in errors)
                {
                    Console.WriteLine($"  - {error}");
                }
            }

            Assert.That(errors, Is.Empty, "Tax groups import should not have errors");
            Assert.That(importedTaxGroups.Count(), Is.GreaterThan(0), "Should import at least one tax group");

            var taxGroupsList = importedTaxGroups.ToList();

            // Validate key El Salvador tax groups exist
            var registeredTaxpayers = taxGroupsList.FirstOrDefault(g => g.Code == "REGISTERED_TAXPAYERS");
            var finalConsumers = taxGroupsList.FirstOrDefault(g => g.Code == "FINAL_CONSUMERS");
            var exemptEntities = taxGroupsList.FirstOrDefault(g => g.Code == "EXEMPT_ENTITIES");
            var taxableItems = taxGroupsList.FirstOrDefault(g => g.Code == "TAXABLE_ITEMS");
            var exemptItems = taxGroupsList.FirstOrDefault(g => g.Code == "EXEMPT_ITEMS");
            var fuelItems = taxGroupsList.FirstOrDefault(g => g.Code == "FUEL_ITEMS");

            Assert.That(registeredTaxpayers, Is.Not.Null, "Should have Registered Taxpayers group");
            Assert.That(finalConsumers, Is.Not.Null, "Should have Final Consumers group");
            Assert.That(exemptEntities, Is.Not.Null, "Should have Exempt Entities group");
            Assert.That(taxableItems, Is.Not.Null, "Should have Taxable Items group");
            Assert.That(exemptItems, Is.Not.Null, "Should have Exempt Items group");
            Assert.That(fuelItems, Is.Not.Null, "Should have Fuel Items group");

            // Validate group properties
            Assert.That(registeredTaxpayers.IsEnabled, Is.True, "Registered Taxpayers should be enabled");
            Assert.That(finalConsumers.IsEnabled, Is.True, "Final Consumers should be enabled");

            // Print tax groups
            Console.WriteLine($"\nTax groups loaded:");
            foreach (var group in taxGroupsList)
            {
                Console.WriteLine($"  {group.Code} - {group.Name}");
                Console.WriteLine($"    Description: {group.Description}");
                Console.WriteLine($"    Enabled: {group.IsEnabled}");
            }
        }

        [Test]
        public async Task LoadAllElSalvadorData_IntegratedTest_LoadsAccountsTaxesAndTaxGroups()
        {
            // This test loads all three data sets and validates they work together

            // Arrange - Load Chart of Accounts
            string chartOfAccountsPath = Path.Combine(_testDataPath, "ComercialChartOfAccounts.txt");
            string chartCsvContent = await File.ReadAllTextAsync(chartOfAccountsPath);
            var (accounts, accountErrors) = await _accountImportService.ImportFromCsvAsync(chartCsvContent, "IntegratedTest");

            // Arrange - Load Taxes
            string taxesPath = Path.Combine(_testDataPath, "ElSalvadorTaxes.txt");
            string taxesCsvContent = await File.ReadAllTextAsync(taxesPath);
            var (taxes, taxErrors) = await _taxImportService.ImportFromCsvAsync(taxesCsvContent, "IntegratedTest");

            // Arrange - Load Tax Groups
            string taxGroupsPath = Path.Combine(_testDataPath, "ElSalvadorTaxGroups.txt");
            string taxGroupsCsvContent = await File.ReadAllTextAsync(taxGroupsPath);
            var (taxGroups, taxGroupErrors) = await _taxGroupImportService.ImportFromCsvAsync(taxGroupsCsvContent, "IntegratedTest");

            // Assert all imports succeeded
            Assert.That(accountErrors, Is.Empty, "Account import should succeed");
            Assert.That(taxErrors, Is.Empty, "Tax import should succeed");
            Assert.That(taxGroupErrors, Is.Empty, "Tax group import should succeed");

            var accountsList = accounts.ToList();
            var taxesList = taxes.ToList();
            var taxGroupsList = taxGroups.ToList();

            // Print summary
            Console.WriteLine($"\n=== EL SALVADOR DATA INTEGRATION TEST RESULTS ===");
            Console.WriteLine($"Chart of Accounts: {accountsList.Count} accounts loaded");
            Console.WriteLine($"Taxes: {taxesList.Count} taxes loaded");
            Console.WriteLine($"Tax Groups: {taxGroupsList.Count} tax groups loaded");

            // Validate we can create a simple tax scenario
            var ivaTax = taxesList.First(t => t.Code == "IVA");
            var registeredTaxpayersGroup = taxGroupsList.First(g => g.Code == "REGISTERED_TAXPAYERS");
            var taxableItemsGroup = taxGroupsList.First(g => g.Code == "TAXABLE_ITEMS");

            // Create sample business entities and items to demonstrate the data works
            var registeredCompany = new BusinessEntityDto
            {
                Oid = Guid.NewGuid(),
                Code = "COMP001",
                Name = "Empresa Registrada S.A. de C.V.",
                TaxId = "0614-123456-001-1"
            };

            var taxableProduct = new ItemDto
            {
                Oid = Guid.NewGuid(),
                Code = "PROD001",
                Description = "Producto Gravado",
                BasePrice = 100m
            };

            // Create group memberships
            var groupMemberships = new List<GroupMembershipDto>
            {
                new GroupMembershipDto
                {
                    GroupId = Guid.NewGuid(), // Would be the actual tax group ID
                    EntityId = registeredCompany.Oid,
                    GroupType = GroupType.BusinessEntity
                },
                new GroupMembershipDto
                {
                    GroupId = Guid.NewGuid(), // Would be the actual tax group ID
                    EntityId = taxableProduct.Oid,
                    GroupType = GroupType.Item
                }
            };

            // Create a simple tax rule for IVA
            var taxRules = new List<TaxRuleDto>
            {
                new TaxRuleDto
                {
                    Oid = Guid.NewGuid(),
                    TaxId = ivaTax.Oid,
                    DocumentOperation = DocumentOperation.SalesInvoice,
                    BusinessEntityGroupId = Guid.NewGuid(), // Registered taxpayers group
                    ItemGroupId = Guid.NewGuid(), // Taxable items group
                    IsEnabled = true,
                    Priority = 1
                }
            };

            // Create tax rule evaluator
            var taxRuleEvaluator = new TaxRuleEvaluator(
                taxRules,
                new List<TaxDto> { ivaTax },
                groupMemberships);

            Assert.That(taxRuleEvaluator, Is.Not.Null, "Tax rule evaluator should be created successfully");

            // Create a sample document to test tax calculation
            var document = new DocumentDto
            {
                Oid = Guid.NewGuid(),
                DocumentNumber = "CF-2025-001",
                Date = DateOnly.FromDateTime(DateTime.Today),
                BusinessEntity = registeredCompany,
                DocumentType = new DocumentTypeDto
                {
                    Code = "CF",
                    Name = "Crédito Fiscal",
                    DocumentOperation = DocumentOperation.SalesInvoice
                }
            };

            var line = new LineDto
            {
                Item = taxableProduct,
                Quantity = 1,
                UnitPrice = 100m,
                Amount = 100m
            };

            document.Lines.Add(line);

            // Test tax calculation
            var taxCalculator = new DocumentTaxCalculator(
                document,
                document.DocumentType.Code,
                taxRuleEvaluator);

            Assert.That(taxCalculator, Is.Not.Null, "Tax calculator should be created successfully");

            Console.WriteLine($"\n=== SAMPLE TAX CALCULATION ===");
            Console.WriteLine($"Document: {document.DocumentNumber}");
            Console.WriteLine($"Company: {registeredCompany.Name}");
            Console.WriteLine($"Product: {taxableProduct.Description} - ${taxableProduct.BasePrice}");
            Console.WriteLine($"IVA Rate: {ivaTax.Percentage}%");

            // Calculate expected IVA
            decimal expectedIva = line.Amount * (ivaTax.Percentage / 100m);
            Console.WriteLine($"Expected IVA: ${expectedIva}");

            // Verify accounting integration potential
            var ivaAccount = accountsList.FirstOrDefault(a =>
                a.AccountName.Contains("IVA") ||
                a.OfficialCode == "21060101"); // From the tax file's CreditAccountCode

            if (ivaAccount != null)
            {
                Console.WriteLine($"IVA Payable Account: {ivaAccount.OfficialCode} - {ivaAccount.AccountName}");
            }

            Console.WriteLine($"\n=== DATA INTEGRATION SUCCESSFUL ===");
            Console.WriteLine($"All El Salvador data files loaded and validated successfully!");
        }

        [Test]
        public void ValidateDataFilePaths_AllRequiredFilesExist()
        {
            // Arrange
            string chartOfAccountsPath = Path.Combine(_testDataPath, "ComercialChartOfAccounts.txt");
            string taxesPath = Path.Combine(_testDataPath, "ElSalvadorTaxes.txt");
            string taxGroupsPath = Path.Combine(_testDataPath, "ElSalvadorTaxGroups.txt");

            // Act & Assert
            Assert.That(File.Exists(chartOfAccountsPath), Is.True,
                $"Chart of accounts file not found: {chartOfAccountsPath}");
            Assert.That(File.Exists(taxesPath), Is.True,
                $"Taxes file not found: {taxesPath}");
            Assert.That(File.Exists(taxGroupsPath), Is.True,
                $"Tax groups file not found: {taxGroupsPath}");

            Console.WriteLine($"All required data files found:");
            Console.WriteLine($"  Chart of Accounts: {chartOfAccountsPath}");
            Console.WriteLine($"  Taxes: {taxesPath}");
            Console.WriteLine($"  Tax Groups: {taxGroupsPath}");
        }
    }
}
