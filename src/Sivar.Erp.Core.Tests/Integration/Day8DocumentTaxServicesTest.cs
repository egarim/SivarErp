using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Sivar.Erp.Core.Core;
using Sivar.Erp.Core.Infrastructure.Data;
using Sivar.Erp.Core.Modules.Documents;
using Sivar.Erp.Core.Modules.Taxes;
using Sivar.Erp.Core.Modules.Domain;
using Sivar.Erp.Core.Modules.Domain.Models;
using Sivar.Erp.Core.Modules.DataImport;

namespace Sivar.Erp.Core.Tests.Integration
{
    /// <summary>
    /// Integration tests for Day 8-9: DocumentService and TaxService implementations
    /// Tests complete document processing workflows with tax calculations
    /// </summary>
    [Description("Integration tests for DocumentService and TaxService")]
    public class Day8DocumentTaxServicesTest
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IRepository _repository;
        private readonly IDocumentService _documentService;
        private readonly ITaxService _taxService;
        private readonly IDataImportService _dataImportService;

        public Day8DocumentTaxServicesTest()
        {
            // Setup DI container
            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole());
            services.AddScoped<IRepository, InMemoryRepository>();
            services.AddScoped<IDocumentService, DocumentService>();
            services.AddScoped<ITaxService, TaxService>();
            services.AddScoped<IDataImportService, DataImportService>();
            services.AddScoped<ICsvImportService, CsvImportService>();

            _serviceProvider = services.BuildServiceProvider();
            _repository = _serviceProvider.GetRequiredService<IRepository>();
            _documentService = _serviceProvider.GetRequiredService<IDocumentService>();
            _taxService = _serviceProvider.GetRequiredService<ITaxService>();
            _dataImportService = _serviceProvider.GetRequiredService<IDataImportService>();
        }

        [Fact]
        [Description("Test complete document creation workflow")]
        public async Task TestDocumentCreationWorkflow()
        {
            // Arrange - Setup test data
            await SetupTestDataAsync();

            var documentType = _repository.GetObjects<DocumentTypeDto>().First();
            var businessEntity = _repository.GetObjects<BusinessEntityDto>().First();

            // Act - Create document
            var document = await _documentService.CreateDocumentAsync(documentType, businessEntity);

            // Assert
            Assert.NotNull(document);
            Assert.False(string.IsNullOrEmpty(document.DocumentNumber));
            Assert.Equal(documentType.Id, document.DocumentType.Id);
            Assert.Equal(businessEntity.Id, document.BusinessEntity.Id);
            Assert.Equal(DocumentStatus.Draft, document.Status);
            Assert.True(document.Date >= DateOnly.FromDateTime(DateTime.Today.AddDays(-1)));
        }

        [Fact]
        [Description("Test document validation with valid document")]
        public async Task TestDocumentValidation_ValidDocument()
        {
            // Arrange
            await SetupTestDataAsync();
            var document = await CreateTestDocumentAsync();

            // Act
            var validationResult = await _documentService.ValidateDocumentAsync(document);

            // Assert
            Assert.True(validationResult.IsValid);
            Assert.Empty(validationResult.Errors);
        }

        [Fact]
        [Description("Test document validation with invalid document")]
        public async Task TestDocumentValidation_InvalidDocument()
        {
            // Arrange - Create invalid document
            var invalidDocument = new DocumentDto
            {
                DocumentNumber = "", // Invalid: empty number
                DocumentType = null!, // Invalid: null type
                BusinessEntity = null!, // Invalid: null entity
                Date = default(DateOnly) // Invalid: default date
            };

            // Act
            var validationResult = await _documentService.ValidateDocumentAsync(invalidDocument);

            // Assert
            Assert.False(validationResult.IsValid);
            Assert.Contains("Document number is required", validationResult.Errors);
            Assert.Contains("Document type is required", validationResult.Errors);
            Assert.Contains("Business entity is required", validationResult.Errors);
            Assert.Contains("Document date is required", validationResult.Errors);
        }

        [Fact]
        [Description("Test tax calculation for sales operation")]
        public async Task TestTaxCalculation_SalesOperation()
        {
            // Arrange
            await SetupTestDataAsync();
            var document = await CreateTestDocumentWithAmountAsync(1000m);

            // Act
            var applicableTaxes = await _taxService.GetApplicableTaxesAsync(document, DocumentOperation.Sale);
            var taxTotals = await _taxService.CreateTaxTotalsAsync(document, DocumentOperation.Sale);

            // Assert
            Assert.NotEmpty(applicableTaxes);
            Assert.NotEmpty(taxTotals);

            var vatTax = applicableTaxes.FirstOrDefault(t => t.TaxType == TaxType.VAT);
            if (vatTax != null)
            {
                var vatTotal = taxTotals.FirstOrDefault(tt => tt.Concept.Contains(vatTax.Name));
                Assert.NotNull(vatTotal);
                Assert.True(vatTotal.Total > 0);
            }
        }

        [Fact]
        [Description("Test tax calculation for purchase operation")]
        public async Task TestTaxCalculation_PurchaseOperation()
        {
            // Arrange
            await SetupTestDataAsync();
            var document = await CreateTestDocumentWithAmountAsync(500m);

            // Act
            var applicableTaxes = await _taxService.GetApplicableTaxesAsync(document, DocumentOperation.Purchase);
            var taxAmount = await _taxService.CalculateTaxAmountAsync(500m, 
                applicableTaxes.First(), DocumentOperation.Purchase);

            // Assert
            Assert.NotEmpty(applicableTaxes);
            Assert.True(taxAmount >= 0);
        }

        [Fact]
        [Description("Test complete document processing workflow")]
        public async Task TestCompleteDocumentProcessing()
        {
            // Arrange
            await SetupTestDataAsync();
            var document = await CreateTestDocumentWithAmountAsync(750m);

            // Act - Process document (validates, calculates taxes, updates status)
            var processedDocument = await _documentService.ProcessDocumentAsync(document, DocumentOperation.Sale);

            // Assert
            Assert.Equal(DocumentStatus.Pending, processedDocument.Status);
            Assert.True(processedDocument.DocumentTotals.Any());
            
            // Check if tax totals were added
            var taxTotals = processedDocument.DocumentTotals
                .Where(dt => dt.Concept.StartsWith("Tax:", StringComparison.OrdinalIgnoreCase));
            Assert.NotEmpty(taxTotals);
        }

        [Fact]
        [Description("Test document posting workflow")]
        public async Task TestDocumentPosting()
        {
            // Arrange
            await SetupTestDataAsync();
            var document = await CreateTestDocumentWithAmountAsync(300m);
            await _documentService.ProcessDocumentAsync(document, DocumentOperation.Sale);

            // Act
            await _documentService.PostDocumentAsync(document, "TestUser");

            // Assert
            Assert.Equal(DocumentStatus.Posted, document.Status);
        }

        [Fact]
        [Description("Test document cancellation")]
        public async Task TestDocumentCancellation()
        {
            // Arrange
            await SetupTestDataAsync();
            var document = await CreateTestDocumentWithAmountAsync(200m);

            // Act
            await _documentService.CancelDocumentAsync(document, "Test cancellation", "TestUser");

            // Assert
            Assert.Equal(DocumentStatus.Cancelled, document.Status);
        }

        [Fact]
        [Description("Test tax summary generation")]
        public async Task TestTaxSummaryGeneration()
        {
            // Arrange
            await SetupTestDataAsync();
            var document = await CreateTestDocumentWithAmountAsync(1000m);
            await _documentService.CalculateDocumentTaxesAsync(document, DocumentOperation.Sale);

            // Act
            var taxSummary = await _taxService.GetTaxSummaryAsync(document);

            // Assert
            Assert.NotNull(taxSummary);
            Assert.Equal(document.Id, taxSummary.DocumentId);
            Assert.True(taxSummary.SubtotalAmount > 0);
            Assert.True(taxSummary.TotalAmount >= taxSummary.SubtotalAmount);
        }

        [Fact]
        [Description("Test tax recalculation after document modification")]
        public async Task TestTaxRecalculation()
        {
            // Arrange
            await SetupTestDataAsync();
            var document = await CreateTestDocumentWithAmountAsync(500m);
            await _documentService.CalculateDocumentTaxesAsync(document, DocumentOperation.Sale);
            var originalTaxCount = document.DocumentTotals
                .Count(dt => dt.Concept.StartsWith("Tax:", StringComparison.OrdinalIgnoreCase));

            // Act - Recalculate taxes
            await _taxService.RecalculateDocumentTaxesAsync(document, DocumentOperation.Sale);

            // Assert
            var newTaxCount = document.DocumentTotals
                .Count(dt => dt.Concept.StartsWith("Tax:", StringComparison.OrdinalIgnoreCase));
            Assert.Equal(originalTaxCount, newTaxCount); // Should have same number of tax entries
        }

        [Fact]
        [Description("Test getting documents by business entity")]
        public async Task TestGetDocumentsByBusinessEntity()
        {
            // Arrange
            await SetupTestDataAsync();
            var businessEntity = _repository.GetObjects<BusinessEntityDto>().First();
            
            // Create multiple documents for the same entity
            var doc1 = await CreateTestDocumentForEntityAsync(businessEntity);
            var doc2 = await CreateTestDocumentForEntityAsync(businessEntity);

            // Act
            var documents = await _documentService.GetDocumentsByBusinessEntityAsync(businessEntity.Id);

            // Assert
            Assert.True(documents.Count() >= 2);
            Assert.All(documents, doc => Assert.Equal(businessEntity.Id, doc.BusinessEntity.Id));
        }

        [Fact]
        [Description("Test tax service validation")]
        public async Task TestTaxConfigurationValidation()
        {
            // Arrange
            await SetupTestDataAsync();
            var document = await CreateTestDocumentWithAmountAsync(100m);
            await _documentService.CalculateDocumentTaxesAsync(document, DocumentOperation.Sale);

            // Act
            var validationResult = await _taxService.ValidateTaxConfigurationAsync(document);

            // Assert
            Assert.True(validationResult.IsValid);
            Assert.Empty(validationResult.Errors);
        }

        [Fact]
        [Description("Test integration between DocumentService and TaxService")]
        public async Task TestDocumentTaxServiceIntegration()
        {
            // Arrange
            await SetupTestDataAsync();
            var document = await CreateTestDocumentWithAmountAsync(1200m);

            // Act - Document service should use tax service for calculations
            await _documentService.CalculateDocumentTaxesAsync(document, DocumentOperation.Sale);

            // Assert
            var taxTotals = document.DocumentTotals
                .Where(dt => dt.Concept.StartsWith("Tax:", StringComparison.OrdinalIgnoreCase))
                .ToList();

            Assert.NotEmpty(taxTotals);
            Assert.All(taxTotals, tt => 
            {
                Assert.True(tt.Total > 0);
                Assert.True(tt.IncludeInTransaction);
                Assert.False(string.IsNullOrEmpty(tt.DebitAccountCode) && string.IsNullOrEmpty(tt.CreditAccountCode));
            });
        }

        #region Helper Methods

        private async Task SetupTestDataAsync()
        {
            // Create document types
            var salesInvoiceType = _repository.CreateObject<DocumentTypeDto>();
            salesInvoiceType.Code = "CCF";
            salesInvoiceType.Name = "Comprobante de Crédito Fiscal";
            salesInvoiceType.GeneratesTransaction = true;

            var purchaseInvoiceType = _repository.CreateObject<DocumentTypeDto>();
            purchaseInvoiceType.Code = "PIF";
            purchaseInvoiceType.Name = "Purchase Invoice";
            purchaseInvoiceType.GeneratesTransaction = true;

            // Create business entities
            var customer = _repository.CreateObject<BusinessEntityDto>();
            customer.Code = "CL001";
            customer.Name = "Test Customer";
            customer.EntityType = BusinessEntityType.Customer;

            var supplier = _repository.CreateObject<BusinessEntityDto>();
            supplier.Code = "PR001";
            supplier.Name = "Test Supplier";
            supplier.EntityType = BusinessEntityType.Supplier;

            // Create taxes
            var vatTax = _repository.CreateObject<TaxDto>();
            vatTax.Code = "IVA13";
            vatTax.Name = "IVA 13%";
            vatTax.Rate = 13m;
            vatTax.TaxType = TaxType.VAT;
            vatTax.IsActive = true;

            var salesTax = _repository.CreateObject<TaxDto>();
            salesTax.Code = "ST5";
            salesTax.Name = "Sales Tax 5%";
            salesTax.Rate = 5m;
            salesTax.TaxType = TaxType.Sales;
            salesTax.IsActive = true;

            await _repository.CommitChanges();
        }

        private async Task<IDocument> CreateTestDocumentAsync()
        {
            var documentType = _repository.GetObjects<DocumentTypeDto>().First();
            var businessEntity = _repository.GetObjects<BusinessEntityDto>().First();
            return await _documentService.CreateDocumentAsync(documentType, businessEntity);
        }

        private async Task<IDocument> CreateTestDocumentWithAmountAsync(decimal amount)
        {
            var document = await CreateTestDocumentAsync();
            
            // Add a document total to simulate document content
            var subtotal = _repository.CreateObject<DocumentTotalDto>();
            subtotal.Concept = "Subtotal";
            subtotal.Total = amount;
            subtotal.IncludeInTransaction = true;
            
            document.DocumentTotals.Add(subtotal);
            
            return document;
        }

        private async Task<IDocument> CreateTestDocumentForEntityAsync(IBusinessEntity businessEntity)
        {
            var documentType = _repository.GetObjects<DocumentTypeDto>().First();
            return await _documentService.CreateDocumentAsync(documentType, businessEntity);
        }

        #endregion
    }
}