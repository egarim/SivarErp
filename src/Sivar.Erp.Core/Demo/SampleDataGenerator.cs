using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sivar.Erp.Core.Core;
using Sivar.Erp.Core.Configuration;
using Sivar.Erp.Core.Modules.Domain.Models;
using Sivar.Erp.Core.Modules.Domain;
using Sivar.Erp.Core.Modules.DataImport;
using System.ComponentModel;
using System.Diagnostics;

namespace Sivar.Erp.Core.Demo
{
    /// <summary>
    /// Generates comprehensive sample data for demos and testing with enhanced CSV import capabilities
    /// </summary>
    [Description("Generates comprehensive sample data for demos and testing")]
    public class SampleDataGenerator : ISampleDataGenerator
    {
        private readonly ILogger<SampleDataGenerator> _logger;
        private readonly DemoOptions _options;
        private readonly IDataImportService _dataImportService;

        /// <summary>
        /// Initializes a new instance of the SampleDataGenerator
        /// </summary>
        /// <param name="logger">Logger for diagnostic information</param>
        /// <param name="options">Demo configuration options</param>
        /// <param name="dataImportService">Data import service for CSV processing</param>
        public SampleDataGenerator(
            ILogger<SampleDataGenerator> logger,
            IOptions<DemoOptions> options,
            IDataImportService dataImportService)
        {
            _logger = logger;
            _options = options.Value;
            _dataImportService = dataImportService;
        }

        /// <inheritdoc/>
        public async Task GenerateSampleDataAsync(IRepository repository)
        {
            _logger.LogInformation("Starting enhanced sample data generation for {TestDataSet}", _options.TestDataSet);
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Use the enhanced DataImportService to import all embedded CSV data
                var importResult = await _dataImportService.ImportTestDataSetAsync(_options.TestDataSet, "SampleDataGenerator");

                if (importResult.Success)
                {
                    _logger.LogInformation("Successfully imported test data set {DataSet}: {Summary}", 
                        _options.TestDataSet, importResult.Summary);

                    // Generate additional sample data if configured
                    if (_options.IncludeTestTransactions)
                    {
                        await GenerateTestTransactionsAsync(repository);
                    }

                    if (_options.IncludeTestDocuments)
                    {
                        await GenerateTestDocumentsAsync(repository);
                    }

                    await repository.CommitChanges();
                    _logger.LogInformation("Sample data generation completed successfully in {Duration}ms", stopwatch.ElapsedMilliseconds);
                }
                else
                {
                    _logger.LogError("Failed to import test data set {DataSet}: {Summary}", 
                        _options.TestDataSet, importResult.Summary);
                    
                    // Fallback to manual data creation
                    await GenerateMinimalSampleDataAsync(repository);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate sample data, attempting fallback");
                repository.Rollback();
                
                try
                {
                    await GenerateMinimalSampleDataAsync(repository);
                }
                catch (Exception fallbackEx)
                {
                    _logger.LogError(fallbackEx, "Fallback sample data generation also failed");
                    throw;
                }
            }
        }

        /// <summary>
        /// Generates minimal sample data when CSV import fails
        /// </summary>
        /// <param name="repository">Repository for data storage</param>
        private async Task GenerateMinimalSampleDataAsync(IRepository repository)
        {
            _logger.LogInformation("Generating minimal sample data as fallback");

            try
            {
                // Create basic chart of accounts
                await CreateBasicChartOfAccountsAsync(repository);
                
                // Create basic tax definitions
                await CreateBasicTaxesAsync(repository);
                
                // Create basic business entities
                await CreateBasicBusinessEntitiesAsync(repository);
                
                // Create basic document types
                await CreateBasicDocumentTypesAsync(repository);

                if (_options.IncludeTestTransactions)
                {
                    await GenerateTestTransactionsAsync(repository);
                }

                if (_options.IncludeTestDocuments)
                {
                    await GenerateTestDocumentsAsync(repository);
                }

                await repository.CommitChanges();
                _logger.LogInformation("Minimal sample data generated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate minimal sample data");
                repository.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Creates a basic chart of accounts
        /// </summary>
        private async Task CreateBasicChartOfAccountsAsync(IRepository repository)
        {
            _logger.LogDebug("Creating basic chart of accounts");

            var accounts = new[]
            {
                new { Code = "1000", Name = "Cash", Type = AccountType.Asset },
                new { Code = "1200", Name = "Accounts Receivable", Type = AccountType.Asset },
                new { Code = "1300", Name = "Inventory", Type = AccountType.Asset },
                new { Code = "2000", Name = "Accounts Payable", Type = AccountType.Liability },
                new { Code = "2100", Name = "Accrued Liabilities", Type = AccountType.Liability },
                new { Code = "3000", Name = "Equity", Type = AccountType.Equity },
                new { Code = "4000", Name = "Sales Revenue", Type = AccountType.Revenue },
                new { Code = "5000", Name = "Cost of Goods Sold", Type = AccountType.Expense },
                new { Code = "6000", Name = "Operating Expenses", Type = AccountType.Expense }
            };

            foreach (var accountDef in accounts)
            {
                var account = repository.CreateObject<AccountDto>();
                account.Id = Guid.NewGuid();
                account.CreatedAt = DateTime.UtcNow;
                account.UpdatedAt = DateTime.UtcNow;
                account.OfficialCode = accountDef.Code;
                account.AccountName = accountDef.Name;
                account.AccountType = accountDef.Type;
                account.IsActive = true;
                account.Balance = 0m;
            }

            _logger.LogDebug("Created {AccountCount} basic accounts", accounts.Length);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Creates basic tax definitions
        /// </summary>
        private async Task CreateBasicTaxesAsync(IRepository repository)
        {
            _logger.LogDebug("Creating basic tax definitions");

            var taxes = new[]
            {
                new { Code = "VAT", Name = "Value Added Tax", Rate = 13.00m, Type = TaxType.VAT },
                new { Code = "SALES", Name = "Sales Tax", Rate = 10.00m, Type = TaxType.Sales },
                new { Code = "WITH", Name = "Withholding Tax", Rate = 5.00m, Type = TaxType.Withholding }
            };

            foreach (var taxDef in taxes)
            {
                var tax = repository.CreateObject<TaxDto>();
                tax.Id = Guid.NewGuid();
                tax.CreatedAt = DateTime.UtcNow;
                tax.UpdatedAt = DateTime.UtcNow;
                tax.Code = taxDef.Code;
                tax.Name = taxDef.Name;
                tax.Rate = taxDef.Rate;
                tax.TaxType = taxDef.Type;
                tax.IsActive = true;
            }

            _logger.LogDebug("Created {TaxCount} basic taxes", taxes.Length);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Creates basic business entities
        /// </summary>
        private async Task CreateBasicBusinessEntitiesAsync(IRepository repository)
        {
            _logger.LogDebug("Creating basic business entities");

            var entities = new[]
            {
                new { Code = "CUST001", Name = "Sample Customer", Type = BusinessEntityType.Customer, Email = "customer@example.com" },
                new { Code = "SUPP001", Name = "Sample Supplier", Type = BusinessEntityType.Supplier, Email = "supplier@example.com" },
                new { Code = "EMP001", Name = "Sample Employee", Type = BusinessEntityType.Employee, Email = "employee@example.com" }
            };

            foreach (var entityDef in entities)
            {
                var entity = repository.CreateObject<BusinessEntityDto>();
                entity.Id = Guid.NewGuid();
                entity.CreatedAt = DateTime.UtcNow;
                entity.UpdatedAt = DateTime.UtcNow;
                entity.Code = entityDef.Code;
                entity.Name = entityDef.Name;
                entity.EntityType = entityDef.Type;
                entity.Email = entityDef.Email;
            }

            _logger.LogDebug("Created {EntityCount} basic business entities", entities.Length);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Creates basic document types
        /// </summary>
        private async Task CreateBasicDocumentTypesAsync(IRepository repository)
        {
            _logger.LogDebug("Creating basic document types");

            var documentTypes = new[]
            {
                new { Code = "INV", Name = "Sales Invoice", GeneratesTransaction = true },
                new { Code = "PUR", Name = "Purchase Invoice", GeneratesTransaction = true },
                new { Code = "PAY", Name = "Payment", GeneratesTransaction = true },
                new { Code = "ADJ", Name = "Adjustment", GeneratesTransaction = true },
                new { Code = "QUO", Name = "Quotation", GeneratesTransaction = false }
            };

            foreach (var docTypeDef in documentTypes)
            {
                var docType = repository.CreateObject<DocumentTypeDto>();
                docType.Id = Guid.NewGuid();
                docType.CreatedAt = DateTime.UtcNow;
                docType.UpdatedAt = DateTime.UtcNow;
                docType.Code = docTypeDef.Code;
                docType.Name = docTypeDef.Name;
                docType.GeneratesTransaction = docTypeDef.GeneratesTransaction;
            }

            _logger.LogDebug("Created {DocumentTypeCount} basic document types", documentTypes.Length);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Generates test transactions
        /// </summary>
        private async Task GenerateTestTransactionsAsync(IRepository repository)
        {
            _logger.LogDebug("Generating test transactions");

            // Create a few sample transactions
            for (int i = 1; i <= 5; i++)
            {
                var transaction = repository.CreateObject<TransactionDto>();
                transaction.Id = Guid.NewGuid();
                transaction.CreatedAt = DateTime.UtcNow;
                transaction.UpdatedAt = DateTime.UtcNow;
                transaction.TransactionNumber = $"TXN-DEMO-{i:D3}";
                transaction.TransactionDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-i));
                transaction.Description = $"Demo transaction {i}";
                transaction.IsPosted = false;

                // Add some ledger entries
                var debitEntry = repository.CreateObject<LedgerEntryDto>();
                debitEntry.Id = Guid.NewGuid();
                debitEntry.CreatedAt = DateTime.UtcNow;
                debitEntry.UpdatedAt = DateTime.UtcNow;
                debitEntry.TransactionNumber = transaction.TransactionNumber;
                debitEntry.AccountCode = "1000"; // Cash
                debitEntry.EntryType = EntryType.Debit;
                debitEntry.Amount = 100m * i;
                debitEntry.Description = $"Debit entry for demo transaction {i}";
                transaction.LedgerEntries.Add(debitEntry);

                var creditEntry = repository.CreateObject<LedgerEntryDto>();
                creditEntry.Id = Guid.NewGuid();
                creditEntry.CreatedAt = DateTime.UtcNow;
                creditEntry.UpdatedAt = DateTime.UtcNow;
                creditEntry.TransactionNumber = transaction.TransactionNumber;
                creditEntry.AccountCode = "4000"; // Sales Revenue
                creditEntry.EntryType = EntryType.Credit;
                creditEntry.Amount = 100m * i;
                creditEntry.Description = $"Credit entry for demo transaction {i}";
                transaction.LedgerEntries.Add(creditEntry);
            }

            _logger.LogDebug("Generated 5 test transactions");
            await Task.CompletedTask;
        }

        /// <summary>
        /// Generates test documents
        /// </summary>
        private async Task GenerateTestDocumentsAsync(IRepository repository)
        {
            _logger.LogDebug("Generating test documents");

            // Get existing entities to reference
            var documentTypes = repository.GetObjects<DocumentTypeDto>().Take(2).ToList();
            var businessEntities = repository.GetObjects<BusinessEntityDto>().Take(2).ToList();

            if (documentTypes.Any() && businessEntities.Any())
            {
                for (int i = 1; i <= 3; i++)
                {
                    var document = repository.CreateObject<DocumentDto>();
                    document.Id = Guid.NewGuid();
                    document.CreatedAt = DateTime.UtcNow;
                    document.UpdatedAt = DateTime.UtcNow;
                    document.DocumentNumber = $"DOC-DEMO-{i:D3}";
                    document.Date = DateOnly.FromDateTime(DateTime.Today.AddDays(-i));
                    document.Status = DocumentStatus.Draft;

                    // Add a document total
                    var total = repository.CreateObject<DocumentTotalDto>();
                    total.Id = Guid.NewGuid();
                    total.CreatedAt = DateTime.UtcNow;
                    total.UpdatedAt = DateTime.UtcNow;
                    total.Concept = "Demo Transaction";
                    total.Total = 150m * i;
                    total.DebitAccountCode = "1000";
                    total.CreditAccountCode = "4000";
                    total.IncludeInTransaction = true;
                    document.DocumentTotals.Add(total);
                }

                _logger.LogDebug("Generated 3 test documents");
            }
            else
            {
                _logger.LogWarning("Cannot generate test documents: missing document types or business entities");
            }

            await Task.CompletedTask;
        }
    }
}