using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Sivar.Erp.Core.Core;
using Sivar.Erp.Core.Modules.Accounting;
using Sivar.Erp.Core.Modules.Documents;
using Sivar.Erp.Core.Modules.Domain;
using Sivar.Erp.Core.Modules.Domain.Models;
using Sivar.Erp.Core.Demo;

namespace Sivar.Erp.Core.Tests.Integration
{
    public class CompleteAccountingWorkflowIntegrationTest : IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IRepository _repository;
        private readonly IAccountingService _accountingService;
        private readonly IDocumentService _documentService;
        private readonly ISampleDataGenerator _sampleDataGenerator;

        public CompleteAccountingWorkflowIntegrationTest()
        {
            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
            services.AddSivarErpDemo("ElSalvador");
            
            _serviceProvider = services.BuildServiceProvider();
            _repository = _serviceProvider.GetRequiredService<IRepository>();
            _accountingService = _serviceProvider.GetRequiredService<IAccountingService>();
            _documentService = _serviceProvider.GetRequiredService<IDocumentService>();
            _sampleDataGenerator = _serviceProvider.GetRequiredService<ISampleDataGenerator>();
        }

        public void Dispose()
        {
            _repository?.Dispose();
            if (_serviceProvider is IDisposable disposable)
                disposable.Dispose();
        }

        [Fact]
        public async Task ExecuteCompleteAccountingWorkflow_ShouldCompleteSuccessfully()
        {
            // PHASE 1: Data Import
            await _sampleDataGenerator.GenerateSampleDataAsync(_repository);
            
            // PHASE 2: Create Purchase Document
            var supplier = _repository.GetObjects<BusinessEntityDto>().First();
            var docType = _repository.CreateObject<DocumentTypeDto>();
            docType.Code = "PUR";
            docType.Name = "Purchase Invoice";
            docType.GeneratesTransaction = true;

            var purchaseDocument = await _documentService.CreateDocumentAsync(docType, supplier);
            
            if (purchaseDocument is DocumentDto purchaseDto)
            {
                purchaseDto.DocumentNumber = "PUR-2025-001";
                purchaseDto.Date = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));
                
                var total = _repository.CreateObject<DocumentTotalDto>();
                total.Concept = "Merchandise";
                total.Total = 650.00m;
                total.DebitAccountCode = "1300";
                total.CreditAccountCode = "2100";
                total.IncludeInTransaction = true;
                purchaseDto.DocumentTotals.Add(total);
                
                purchaseDto.Status = DocumentStatus.Approved;
            }
            
            await _repository.CommitChanges();

            // PHASE 3: Process Purchase Transaction
            var purchaseTransaction = await _accountingService.CreateTransactionAsync(purchaseDocument);
            await _accountingService.PostTransactionAsync(purchaseTransaction);

            // PHASE 4: Validate Results
            Assert.True(purchaseTransaction.IsPosted);
            Assert.True(purchaseTransaction.IsBalanced);
            
            var trialBalance = await _accountingService.GetTrialBalanceAsync(DateOnly.FromDateTime(DateTime.Today));
            Assert.NotEmpty(trialBalance);
        }
    }
}