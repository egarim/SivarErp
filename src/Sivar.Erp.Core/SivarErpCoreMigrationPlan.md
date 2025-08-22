# Sivar ERP Core Migration Plan

## Executive Summary

This document outlines the complete migration strategy from Sivar.Erp (IObjectDb-based) to Sivar.Erp.Core (Repository pattern-based). The goal is to eliminate IObjectDb dependency while maintaining all functionality and eventually recreating the CompleteAccountingWorkflowTest using Sivar.Erp.Core infrastructure.

## Migration Objectives

- ✅ Remove IObjectDb dependency completely
- ✅ Implement Repository pattern throughout
- ✅ Maintain all current functionality
- ✅ Optimize for reading speed
- ✅ Implement Microsoft Logging infrastructure
- ✅ Enable CompleteAccountingWorkflowTest execution
- ❌ No backward compatibility required

## Current State Analysis

### Sivar.Erp Dependencies
- **Heavy IObjectDb Usage**: 90+ direct references in CompleteAccountingWorkflowTest
- **Collection-based Storage**: Direct access to `_objectDb.Accounts`, `_objectDb.BusinessEntities`, etc.
- **Service Constructor Dependencies**: Services expect IObjectDb parameters
- **Data Import Process**: `DataImportHelper.ImportAllDataAsync(_objectDb, dataDirectory)`

### Sivar.Erp.Core Advantages
- **Repository Pattern**: `IRepository` with `GetObjects<T>()`, `CreateObject<T>()`
- **Dependency Injection Ready**: Modern DI container support
- **Microsoft Logging**: Built-in logging infrastructure
- **Clean Architecture**: Better separation of concerns

## Migration Phases

## Phase 1: Foundation Setup (Week 1)

### 1.1 Enhanced Repository Pattern
```csharp
// Target: Sivar.Erp.Core/Core/IRepository.cs
public interface IRepository
{
    // Reading operations (optimized for speed)
    IQueryable<T> GetObjects<T>() where T : class;
    T? GetByKey<T>(object key) where T : class;
    Task<T?> GetByKeyAsync<T>(object key) where T : class;
    
    // Writing operations
    T CreateObject<T>() where T : class, new();
    void UpdateObject<T>(T obj) where T : class;
    void DeleteObject<T>(T obj) where T : class;
    
    // Bulk operations for performance
    Task<IEnumerable<T>> GetBatchAsync<T>(IEnumerable<object> keys) where T : class;
    Task BulkInsertAsync<T>(IEnumerable<T> objects) where T : class;
    
    // Transaction support
    Task SaveChangesAsync();
    IRepositoryTransaction BeginTransaction();
}
```

### 1.2 Microsoft Logging Infrastructure
```csharp
// Target: Sivar.Erp.Core/Configuration/ServiceCollectionExtensions.cs
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSivarErpCore(
        this IServiceCollection services,
        Action<SivarErpOptions>? configureOptions = null)
    {
        // Microsoft Logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // Repository Pattern
        services.AddScoped<IRepository, InMemoryRepository>();
        
        // Core Services
        services.AddScoped<IAccountingService, AccountingService>();
        services.AddScoped<ITaxService, TaxService>();
        services.AddScoped<IDataImportService, DataImportService>();
        
        return services;
    }
}
```

### 1.3 Performance-Optimized InMemoryRepository
```csharp
// Target: Sivar.Erp.Core/Infrastructure/Data/InMemoryRepository.cs
public class InMemoryRepository : IRepository
{
    private readonly ConcurrentDictionary<Type, ConcurrentDictionary<object, object>> _collections;
    private readonly ConcurrentDictionary<Type, object> _queryableCollections;
    private readonly ILogger<InMemoryRepository> _logger;

    // Optimized for reading speed with concurrent collections
    public IQueryable<T> GetObjects<T>() where T : class
    {
        var type = typeof(T);
        if (!_queryableCollections.TryGetValue(type, out var collection))
        {
            collection = new List<T>().AsQueryable();
            _queryableCollections[type] = collection;
        }
        return (IQueryable<T>)collection;
    }
}
```

**Deliverables Phase 1:**
- Enhanced IRepository interface
- Performance-optimized InMemoryRepository implementation
- Microsoft Logging integration
- ServiceCollectionExtensions for DI setup

## Phase 2: Core Services Migration (Week 2)

### 2.1 Accounting Service Migration
```csharp
// Target: Sivar.Erp.Core/Modules/Accounting/AccountingService.cs
public class AccountingService : IAccountingService
{
    private readonly IRepository _repository;
    private readonly ILogger<AccountingService> _logger;

    // Replace: _objectDb.Accounts.FirstOrDefault()
    // With: _repository.GetObjects<AccountDto>().FirstOrDefault()
    
    public async Task<ITransaction> CreateTransactionFromDocumentAsync(IDocument document)
    {
        _logger.LogInformation("Creating transaction from document {DocumentNumber}", document.DocumentNumber);
        
        var accounts = _repository.GetObjects<AccountDto>();
        var transaction = _repository.CreateObject<TransactionDto>();
        
        // Implementation using Repository pattern
        return transaction;
    }
}
```

### 2.2 Tax Service Migration
```csharp
// Target: Sivar.Erp.Core/Modules/Taxes/TaxService.cs
public class TaxService : ITaxService
{
    private readonly IRepository _repository;
    private readonly ILogger<TaxService> _logger;

    public async Task<IEnumerable<IDocumentTotal>> CreateTaxTotalsAsync(IDocument document)
    {
        _logger.LogDebug("Calculating taxes for document {DocumentNumber}", document.DocumentNumber);
        
        var taxes = _repository.GetObjects<TaxDto>();
        var taxGroups = _repository.GetObjects<TaxGroupDto>();
        
        // Tax calculation logic
        return taxTotals;
    }
}
```

### 2.3 Data Import Service Migration
```csharp
// Target: Sivar.Erp.Core/Modules/DataImport/DataImportService.cs
public class DataImportService : IDataImportService
{
    private readonly IRepository _repository;
    private readonly ICsvImportService _csvImportService;
    private readonly ILogger<DataImportService> _logger;

    // Replace: ImportAllDataAsync(IObjectDb objectDb, string dataDirectory)
    // With: ImportAllDataAsync(string dataDirectory)
    
    public async Task<IDictionary<string, EntityImportResult>> ImportAllDataAsync(string dataDirectory)
    {
        _logger.LogInformation("Starting data import from {DataDirectory}", dataDirectory);
        
        var results = new Dictionary<string, EntityImportResult>();
        
        // Import each entity type using Repository pattern
        await ImportAccountsAsync(dataDirectory, results);
        await ImportTaxesAsync(dataDirectory, results);
        await ImportBusinessEntitiesAsync(dataDirectory, results);
        
        return results;
    }
}
```

**Deliverables Phase 2:**
- AccountingService migrated to Repository pattern
- TaxService migrated to Repository pattern
- DataImportService migrated to Repository pattern
- All services using Microsoft Logging

## Phase 3: Missing Module Implementation (Week 3)

### 3.1 Payment Module Implementation
```csharp
// Target: Sivar.Erp.Core/Modules/Payments/PaymentService.cs
public class PaymentService : IPaymentService
{
    private readonly IRepository _repository;
    private readonly ILogger<PaymentService> _logger;
    private readonly Dictionary<string, string> _accountMappings;

    public async Task<PaymentDto> CreatePaymentAsync(PaymentDto payment, string userId)
    {
        _logger.LogInformation("Creating payment for document {DocumentNumber}", payment.DocumentNumber);
        
        var paymentEntity = _repository.CreateObject<PaymentDto>();
        // Map properties and save
        await _repository.SaveChangesAsync();
        
        return paymentEntity;
    }

    public async Task<(ITransaction Transaction, IList<LedgerEntryDto> LedgerEntries)> 
        GeneratePaymentTransactionAsync(PaymentDto payment, IDocument document)
    {
        _logger.LogDebug("Generating payment transaction for {PaymentId}", payment.PaymentId);
        
        // Generate ledger entries based on payment type
        var ledgerEntries = new List<LedgerEntryDto>();
        var transaction = _repository.CreateObject<TransactionDto>();
        
        return (transaction, ledgerEntries);
    }
}
```

### 3.2 Inventory Module Implementation
```csharp
// Target: Sivar.Erp.Core/Modules/Inventory/InventoryService.cs
public class InventoryService : IInventoryService
{
    private readonly IRepository _repository;
    private readonly ILogger<InventoryService> _logger;

    public async Task<IInventoryTransaction> ReceiveInventoryAsync(
        IInventoryItem item, decimal quantity, string warehouseCode, 
        decimal unitCost, string userName)
    {
        _logger.LogInformation("Receiving inventory {ItemCode} quantity {Quantity}", 
            item.Code, quantity);
        
        var transaction = _repository.CreateObject<InventoryTransactionDto>();
        var stockLevel = _repository.GetObjects<StockLevelDto>()
            .FirstOrDefault(s => s.ItemCode == item.Code && s.WarehouseCode == warehouseCode);
        
        // Update stock levels and create transaction
        await _repository.SaveChangesAsync();
        
        return transaction;
    }
}
```

### 3.3 Security Module Implementation
```csharp
// Target: Sivar.Erp.Core/Modules/Security/SecurityService.cs
public class SecurityService : ISecurityService
{
    private readonly IRepository _repository;
    private readonly ILogger<SecurityService> _logger;

    public async Task<AuthenticationResult> AuthenticateAsync(string username, string password)
    {
        _logger.LogInformation("Authenticating user {Username}", username);
        
        var user = _repository.GetObjects<UserDto>()
            .FirstOrDefault(u => u.Username == username && u.IsActive);
        
        // Authentication logic
        return new AuthenticationResult { IsSuccessful = true, User = user };
    }
}
```

**Deliverables Phase 3:**
- Complete Payment module with Repository pattern
- Complete Inventory module with Repository pattern
- Complete Security module with Repository pattern
- All modules using Microsoft Logging

## Phase 4: Journal Entry Services Migration (Week 4)

### 4.1 Journal Entry Service Migration
```csharp
// Target: Sivar.Erp.Core/Modules/Accounting/JournalEntryService.cs
public class JournalEntryService : IJournalEntryService
{
    private readonly IRepository _repository;
    private readonly ILogger<JournalEntryService> _logger;

    public async Task<IEnumerable<ILedgerEntry>> GetJournalEntriesAsync(JournalEntryQueryOptions options)
    {
        _logger.LogDebug("Querying journal entries with options: {@Options}", options);
        
        var query = _repository.GetObjects<LedgerEntryDto>().AsQueryable();
        
        if (!string.IsNullOrEmpty(options.TransactionNumber))
            query = query.Where(e => e.TransactionNumber == options.TransactionNumber);
            
        if (!string.IsNullOrEmpty(options.AccountCode))
            query = query.Where(e => e.OfficialCode == options.AccountCode);
            
        if (options.EntryType.HasValue)
            query = query.Where(e => e.EntryType == options.EntryType.Value);
            
        if (options.OnlyPosted)
        {
            var postedTransactions = _repository.GetObjects<TransactionDto>()
                .Where(t => t.IsPosted)
                .Select(t => t.TransactionNumber);
            query = query.Where(e => postedTransactions.Contains(e.TransactionNumber));
        }
        
        return await Task.FromResult(query.ToList());
    }
}
```

### 4.2 Journal Entry Report Service Migration
```csharp
// Target: Sivar.Erp.Core/Modules/Accounting/Reports/JournalEntryReportService.cs
public class JournalEntryReportService : IJournalEntryReportService
{
    private readonly IRepository _repository;
    private readonly IJournalEntryService _journalEntryService;
    private readonly ILogger<JournalEntryReportService> _logger;

    public async Task<JournalEntryReportDto> GenerateJournalReportAsync(JournalEntryQueryOptions options)
    {
        _logger.LogInformation("Generating journal report from {FromDate} to {ToDate}", 
            options.FromDate, options.ToDate);
        
        var entries = await _journalEntryService.GetJournalEntriesAsync(options);
        
        return new JournalEntryReportDto
        {
            ReportTitle = $"Journal Entries Report - {options.FromDate} to {options.ToDate}",
            FromDate = options.FromDate ?? DateOnly.MinValue,
            ToDate = options.ToDate ?? DateOnly.MaxValue,
            Entries = entries.ToList(),
            TotalEntries = entries.Count(),
            TotalDebits = entries.Where(e => e.EntryType == EntryType.Debit).Sum(e => e.Amount),
            TotalCredits = entries.Where(e => e.EntryType == EntryType.Credit).Sum(e => e.Amount),
            IsBalanced = Math.Abs(entries.Where(e => e.EntryType == EntryType.Debit).Sum(e => e.Amount) - 
                                entries.Where(e => e.EntryType == EntryType.Credit).Sum(e => e.Amount)) < 0.01m
        };
    }
}
```

**Deliverables Phase 4:**
- Complete Journal Entry services with Repository pattern
- Report generation services
- Query optimization for reading speed
- Comprehensive logging

## Phase 5: Test Infrastructure Migration (Week 5)

### 5.1 Core Test Service Factory
```csharp
// Target: Sivar.Erp.Core.Tests/Infrastructure/CoreTestServiceFactory.cs
public static class CoreTestServiceFactory
{
    public static IServiceProvider CreateServiceProvider(Action<IServiceCollection>? configureServices = null)
    {
        var services = new ServiceCollection();
        
        // Microsoft Logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Debug);
        });
        
        // Core ERP Services
        services.AddSivarErpCore();
        
        // Test-specific services
        services.AddScoped<TestDataImportHelper>();
        
        // Allow additional service configuration
        configureServices?.Invoke(services);
        
        return services.BuildServiceProvider();
    }
}
```

### 5.2 Test Data Import Helper
```csharp
// Target: Sivar.Erp.Core.Tests/Infrastructure/TestDataImportHelper.cs
public class TestDataImportHelper
{
    private readonly IDataImportService _dataImportService;
    private readonly ILogger<TestDataImportHelper> _logger;

    public async Task<IDictionary<string, EntityImportResult>> ImportAllTestDataAsync(string dataDirectory)
    {
        _logger.LogInformation("Importing test data from {DataDirectory}", dataDirectory);
        
        // Use the Core DataImportService instead of ObjectDb-based import
        return await _dataImportService.ImportAllDataAsync(dataDirectory);
    }
}
```

**Deliverables Phase 5:**
- Core test infrastructure
- Test service factory using Repository pattern
- Test data import utilities
- Migration validation tests

## Phase 6: CompleteAccountingWorkflowTest Migration (Week 6)

### 6.1 Test Class Migration Strategy
```csharp
// Target: Sivar.Erp.Core.Tests/Integration/CompleteAccountingWorkflowCoreTest.cs
[TestFixture]
public class CompleteAccountingWorkflowCoreTest
{
    private IServiceProvider _serviceProvider = null!;
    private IRepository _repository = null!;  // Replace IObjectDb
    private IAccountingService _accountingService = null!;  // Use Core services
    private ILogger<CompleteAccountingWorkflowCoreTest> _logger = null!;

    [SetUp]
    public void Setup()
    {
        _serviceProvider = CoreTestServiceFactory.CreateServiceProvider(services =>
        {
            // Configure test-specific services
            services.AddSingleton<TestUserContext>();
        });
        
        _repository = _serviceProvider.GetRequiredService<IRepository>();
        _accountingService = _serviceProvider.GetRequiredService<IAccountingService>();
        _logger = _serviceProvider.GetRequiredService<ILogger<CompleteAccountingWorkflowCoreTest>>();
    }

    [Test]
    public async Task ExecuteCompleteWorkflowTest()
    {
        _logger.LogInformation("Starting complete accounting workflow test");
        
        try
        {
            // Step 1: Data Import (Repository-based)
            await SetupDataAndServices();
            
            // Step 2: Document Creation (Repository-based)
            var purchaseDocument = await CreatePurchaseInvoiceDocument();
            var salesDocument = await CreateSalesInvoiceDocument();
            
            // Step 3: Tax Calculation (Core TaxService)
            await CalculateDocumentTaxes(purchaseDocument);
            await CalculateDocumentTaxes(salesDocument);
            
            // Step 4: Transaction Generation (Core AccountingService)
            var purchaseTransaction = await _accountingService.CreateTransactionFromDocumentAsync(purchaseDocument);
            var salesTransaction = await _accountingService.CreateTransactionFromDocumentAsync(salesDocument);
            
            // Step 5: Transaction Posting
            await _accountingService.PostTransactionAsync(purchaseTransaction);
            await _accountingService.PostTransactionAsync(salesTransaction);
            
            _logger.LogInformation("Workflow completed successfully");
            Assert.Pass("Complete accounting workflow executed successfully using Sivar.Erp.Core!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Workflow failed");
            Assert.Fail($"Workflow failed: {ex.Message}");
        }
    }

    private async Task SetupDataAndServices()
    {
        // Replace: dataImportHelper.ImportAllDataAsync(_objectDb, dataDirectory)
        // With: dataImportService.ImportAllDataAsync(dataDirectory)
        
        var dataImportHelper = _serviceProvider.GetRequiredService<TestDataImportHelper>();
        var dataDirectory = "C:\\Users\\joche\\Documents\\GitHub\\SivarErp\\src\\Tests\\ElSalvador\\Data\\New\\";
        
        var importResults = await dataImportHelper.ImportAllTestDataAsync(dataDirectory);
        
        foreach (var result in importResults)
        {
            _logger.LogInformation("Imported {EntityType}: {SuccessCount} successful, {ErrorCount} errors",
                result.Key, result.Value.SuccessCount, result.Value.ErrorCount);
        }
    }

    private async Task<DocumentDto> CreateSalesInvoiceDocument()
    {
        // Replace: _objectDb.BusinessEntities.FirstOrDefault(be => be.Code == "CL001")
        // With: _repository.GetObjects<BusinessEntityDto>().FirstOrDefault(be => be.Code == "CL001")
        
        var businessEntity = _repository.GetObjects<BusinessEntityDto>()
            .FirstOrDefault(be => be.Code == "CL001");
        var documentType = _repository.GetObjects<DocumentTypeDto>()
            .FirstOrDefault(dt => dt.Code == "CCF");

        if (businessEntity == null || documentType == null)
        {
            throw new InvalidOperationException("Required business entity or document type not found");
        }

        var document = _repository.CreateObject<DocumentDto>();
        document.DocumentType = documentType;
        document.DocumentNumber = "CCF-2025-001";
        document.Date = new DateOnly(2025, 6, 18);
        document.BusinessEntity = businessEntity;
        document.Lines = new List<IDocumentLine>();
        document.DocumentTotals = new List<ITotal>();

        // Add document lines using Repository pattern
        var item1 = _repository.GetObjects<ItemDto>().FirstOrDefault(i => i.Code == "PR001");
        var item2 = _repository.GetObjects<ItemDto>().FirstOrDefault(i => i.Code == "PR002");

        if (item1 != null)
        {
            var line1 = _repository.CreateObject<LineDto>();
            line1.LineNumber = 1;
            line1.Item = item1;
            line1.Quantity = 2;
            line1.UnitPrice = 150.0m;
            line1.Amount = 300.0m;
            document.Lines.Add(line1);
        }

        if (item2 != null)
        {
            var line2 = _repository.CreateObject<LineDto>();
            line2.LineNumber = 2;
            line2.Item = item2;
            line2.Quantity = 1;
            line2.UnitPrice = 150.0m;
            line2.Amount = 150.0m;
            document.Lines.Add(line2);
        }

        return document;
    }

    // Continue with other methods following the same pattern...
}
```

**Deliverables Phase 6:**
- Complete test migration to Repository pattern
- All IObjectDb references replaced
- Microsoft Logging integration in tests
- Validation that all functionality works

## Implementation Guidelines

### Performance Optimization for Reading Speed

1. **Concurrent Collections**: Use `ConcurrentDictionary` for thread-safe operations
2. **Queryable Interface**: Implement `IQueryable<T>` for efficient filtering
3. **Batch Operations**: Implement bulk read/write operations
4. **Indexing Strategy**: Create indexes for frequently queried fields
5. **Lazy Loading**: Implement lazy loading for related entities

### Microsoft Logging Best Practices

1. **Structured Logging**: Use structured logging with parameters
2. **Log Levels**: Appropriate log levels (Debug, Information, Warning, Error)
3. **Performance Logging**: Log method execution times and performance metrics
4. **Correlation IDs**: Use correlation IDs for tracking requests
5. **Scoped Logging**: Use scoped logging for contextual information

### Repository Pattern Best Practices

1. **Generic Interface**: Use generic `IRepository` for all entity types
2. **Unit of Work**: Implement Unit of Work pattern for transactions
3. **Query Optimization**: Optimize queries for reading performance
4. **Caching Strategy**: Implement appropriate caching mechanisms
5. **Error Handling**: Comprehensive error handling and logging

## Migration Validation

### Phase Completion Criteria

**Phase 1:** ✅ Repository pattern foundation established with Microsoft Logging
**Phase 2:** ✅ Core services (Accounting, Tax, DataImport) migrated successfully
**Phase 3:** ✅ All missing modules (Payment, Inventory, Security) implemented
**Phase 4:** ✅ Journal Entry services fully functional
**Phase 5:** ✅ Test infrastructure ready
**Phase 6:** ✅ CompleteAccountingWorkflowTest passes using Sivar.Erp.Core

### Success Metrics

1. **Zero IObjectDb Dependencies**: No references to IObjectDb anywhere in Core
2. **Performance**: Reading operations 20% faster than original
3. **Test Coverage**: 100% of original functionality covered
4. **Logging**: Comprehensive logging throughout all operations
5. **Maintainability**: Clean, testable code architecture

## Risk Mitigation

### Potential Risks and Solutions

1. **Performance Degradation**
   - Mitigation: Benchmark each phase and optimize queries
   - Solution: Implement caching and batch operations

2. **Data Consistency Issues**
   - Mitigation: Implement transaction support in Repository
   - Solution: Unit of Work pattern with rollback capabilities

3. **Missing Functionality**
   - Mitigation: Comprehensive testing after each phase
   - Solution: Feature parity validation checklist

4. **Complex Dependencies**
   - Mitigation: Incremental migration with small stages
   - Solution: Dependency injection container management

## Timeline Summary

- **Week 1**: Foundation Setup (Repository, Logging, DI)
- **Week 2**: Core Services Migration (Accounting, Tax, Import)
- **Week 3**: Missing Modules Implementation (Payment, Inventory, Security)
- **Week 4**: Journal Entry Services Migration
- **Week 5**: Test Infrastructure Migration
- **Week 6**: CompleteAccountingWorkflowTest Migration and Validation

**Total Duration**: 6 weeks
**Final Goal**: Complete Sivar.Erp functionality in Sivar.Erp.Core with Repository pattern, optimized for reading speed, using Microsoft Logging, and passing the CompleteAccountingWorkflowTest.
