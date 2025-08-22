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

## Performance Tracking and KPI System with OpenTelemetry

### Overview
Implement comprehensive performance monitoring using **OpenTelemetry** (.NET's industry-standard observability framework) to establish baseline metrics and track system performance month-over-month. This enables capacity planning and performance optimization based on resource utilization patterns.

### OpenTelemetry Framework Choice

**Recommended Stack:**
- **OpenTelemetry .NET**: Industry standard, Microsoft-backed observability
- **Jaeger**: Open-source distributed tracing (free viewer)
- **Prometheus**: Open-source metrics collection
- **Grafana**: Open-source visualization dashboard (free)
- **Seq**: .NET-friendly structured logging (free for single user)

**Alternative Free Stack:**
- **OpenTelemetry .NET** + **OTLP Exporters**
- **Jaeger All-in-One**: Complete tracing solution (Docker)
- **Grafana + Prometheus**: Metrics and dashboards
- **Elastic Stack (ELK)**: Logs, metrics, traces (basic license free)

### OpenTelemetry Integration Setup

#### 1. Package Dependencies
```xml
<!-- Target: Sivar.Erp.Core/Sivar.Erp.Core.csproj -->
<PackageReference Include="OpenTelemetry" Version="1.9.0" />
<PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="1.9.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.AspNetCore" Version="1.9.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.Http" Version="1.9.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.SqlClient" Version="1.9.0-beta.1" />
<PackageReference Include="OpenTelemetry.Exporter.Jaeger" Version="1.5.1" />
<PackageReference Include="OpenTelemetry.Exporter.Prometheus.AspNetCore" Version="1.9.0-beta.2" />
<PackageReference Include="OpenTelemetry.Exporter.OpenTelemetryProtocol" Version="1.9.0" />
<PackageReference Include="System.Diagnostics.DiagnosticSource" Version="8.0.0" />
```

#### 2. OpenTelemetry Configuration
```csharp
// Target: Sivar.Erp.Core/Configuration/OpenTelemetryConfiguration.cs
public static class OpenTelemetryConfiguration
{
    public static IServiceCollection AddSivarErpTelemetry(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        var serviceName = "SivarErp.Core";
        var serviceVersion = "1.0.0";

        services.AddOpenTelemetry()
            .WithTracing(tracerProviderBuilder =>
            {
                tracerProviderBuilder
                    .SetSampler(new AlwaysOnSampler())
                    .AddSource(SivarErpTelemetry.ActivitySourceName)
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.EnrichWithHttpRequest = EnrichHttpRequest;
                        options.EnrichWithHttpResponse = EnrichHttpResponse;
                    })
                    .AddHttpClientInstrumentation()
                    .AddSqlClientInstrumentation(options =>
                    {
                        options.SetDbStatementForText = true;
                        options.RecordException = true;
                    })
                    .SetResourceBuilder(ResourceBuilder.CreateDefault()
                        .AddService(serviceName, serviceVersion)
                        .AddTelemetrySdk())
                    .AddJaegerExporter(options =>
                    {
                        options.AgentHost = configuration["Jaeger:AgentHost"] ?? "localhost";
                        options.AgentPort = int.Parse(configuration["Jaeger:AgentPort"] ?? "6831");
                    })
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(configuration["OTLP:Endpoint"] ?? "http://localhost:4317");
                    });
            })
            .WithMetrics(meterProviderBuilder =>
            {
                meterProviderBuilder
                    .AddMeter(SivarErpTelemetry.MeterName)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddProcessInstrumentation()
                    .SetResourceBuilder(ResourceBuilder.CreateDefault()
                        .AddService(serviceName, serviceVersion))
                    .AddPrometheusExporter()
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(configuration["OTLP:MetricsEndpoint"] ?? "http://localhost:4317");
                    });
            });

        // Custom telemetry services
        services.AddSingleton<SivarErpTelemetry>();
        services.AddScoped<IPerformanceTrackingService, OpenTelemetryPerformanceService>();

        return services;
    }

    private static void EnrichHttpRequest(Activity activity, HttpRequest httpRequest)
    {
        activity.SetTag("http.user_id", httpRequest.HttpContext.User?.Identity?.Name ?? "anonymous");
        activity.SetTag("http.user_agent", httpRequest.Headers.UserAgent.ToString());
    }

    private static void EnrichHttpResponse(Activity activity, HttpResponse httpResponse)
    {
        activity.SetTag("http.response.size", httpResponse.ContentLength);
    }
}
```

#### 3. Custom Telemetry Service
```csharp
// Target: Sivar.Erp.Core/Monitoring/SivarErpTelemetry.cs
public class SivarErpTelemetry
{
    public const string ActivitySourceName = "SivarErp.Core";
    public const string MeterName = "SivarErp.Core";

    private static readonly ActivitySource ActivitySource = new(ActivitySourceName);
    private static readonly Meter Meter = new(MeterName);

    // Custom Metrics
    public static readonly Counter<long> RequestCounter = Meter.CreateCounter<long>(
        "sivar_erp_requests_total",
        "requests",
        "Total number of requests processed");

    public static readonly Histogram<double> RequestDuration = Meter.CreateHistogram<double>(
        "sivar_erp_request_duration_ms",
        "milliseconds",
        "Duration of requests in milliseconds");

    public static readonly Counter<long> ErrorCounter = Meter.CreateCounter<long>(
        "sivar_erp_errors_total",
        "errors",
        "Total number of errors");

    public static readonly UpDownCounter<long> ActiveUsers = Meter.CreateUpDownCounter<long>(
        "sivar_erp_active_users",
        "users",
        "Number of currently active users");

    public static readonly Histogram<long> MemoryUsage = Meter.CreateHistogram<long>(
        "sivar_erp_memory_usage_bytes",
        "bytes",
        "Memory usage in bytes");

    public static readonly Gauge<double> CpuUsage = Meter.CreateGauge<double>(
        "sivar_erp_cpu_usage_percent",
        "percent",
        "CPU usage percentage");

    // Business-specific metrics
    public static readonly Counter<long> TransactionCounter = Meter.CreateCounter<long>(
        "sivar_erp_transactions_created",
        "transactions",
        "Number of accounting transactions created");

    public static readonly Histogram<double> TransactionProcessingTime = Meter.CreateHistogram<double>(
        "sivar_erp_transaction_processing_ms",
        "milliseconds",
        "Time to process accounting transactions");

    public static readonly Counter<long> DocumentCounter = Meter.CreateCounter<long>(
        "sivar_erp_documents_processed",
        "documents",
        "Number of documents processed");

    public static readonly Counter<long> PaymentCounter = Meter.CreateCounter<long>(
        "sivar_erp_payments_processed",
        "payments",
        "Number of payments processed");

    // Create custom activities for tracing
    public static Activity? StartActivity(string name, ActivityKind kind = ActivityKind.Internal)
    {
        return ActivitySource.StartActivity(name, kind);
    }

    public static void RecordException(Exception exception, Activity? activity = null)
    {
        activity ??= Activity.Current;
        activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
        activity?.RecordException(exception);
        
        ErrorCounter.Add(1, new TagList
        {
            ["exception.type"] = exception.GetType().Name,
            ["exception.message"] = exception.Message
        });
    }
}
```

#### 4. OpenTelemetry Performance Service
```csharp
// Target: Sivar.Erp.Core/Monitoring/OpenTelemetryPerformanceService.cs
public class OpenTelemetryPerformanceService : IPerformanceTrackingService
{
    private readonly IRepository _repository;
    private readonly ILogger<OpenTelemetryPerformanceService> _logger;

    public async Task TrackRequestAsync(string methodName, TimeSpan executionTime, string userId, bool isError = false)
    {
        // OpenTelemetry Metrics
        SivarErpTelemetry.RequestCounter.Add(1, new TagList
        {
            ["method"] = methodName,
            ["user_id"] = userId,
            ["status"] = isError ? "error" : "success"
        });

        SivarErpTelemetry.RequestDuration.Record(executionTime.TotalMilliseconds, new TagList
        {
            ["method"] = methodName,
            ["user_id"] = userId
        });

        if (isError)
        {
            SivarErpTelemetry.ErrorCounter.Add(1, new TagList
            {
                ["method"] = methodName,
                ["user_id"] = userId
            });
        }

        // Store in repository for monthly reports (supplement to real-time telemetry)
        var metric = _repository.CreateObject<PerformanceMetricDto>();
        metric.Timestamp = DateTime.UtcNow;
        metric.MethodName = methodName;
        metric.ExecutionTimeMs = executionTime.TotalMilliseconds;
        metric.UserId = userId;
        metric.IsError = isError;
        
        await _repository.SaveChangesAsync();

        _logger.LogDebug("Performance tracked via OpenTelemetry: {MethodName} - {ExecutionTime}ms", 
            methodName, executionTime.TotalMilliseconds);
    }

    public async Task TrackMemoryUsageAsync(long memoryUsageMB)
    {
        SivarErpTelemetry.MemoryUsage.Record(memoryUsageMB * 1024 * 1024); // Convert to bytes
    }

    public async Task TrackActiveUsersAsync(int activeUserCount)
    {
        SivarErpTelemetry.ActiveUsers.Add(activeUserCount - GetPreviousActiveUsers());
    }

    // Enhanced business metrics tracking
    public async Task TrackTransactionCreatedAsync(string transactionType, double processingTimeMs)
    {
        SivarErpTelemetry.TransactionCounter.Add(1, new TagList
        {
            ["transaction_type"] = transactionType
        });

        SivarErpTelemetry.TransactionProcessingTime.Record(processingTimeMs, new TagList
        {
            ["transaction_type"] = transactionType
        });
    }

    public async Task TrackDocumentProcessedAsync(string documentType, string operation)
    {
        SivarErpTelemetry.DocumentCounter.Add(1, new TagList
        {
            ["document_type"] = documentType,
            ["operation"] = operation
        });
    }
}
```

#### 5. Enhanced Service Integration with OpenTelemetry
```csharp
// Example: AccountingService with OpenTelemetry
public class AccountingService : IAccountingService
{
    private readonly IRepository _repository;
    private readonly ILogger<AccountingService> _logger;
    private readonly OpenTelemetryPerformanceService _performanceTracking;

    public async Task<ITransaction> CreateTransactionFromDocumentAsync(IDocument document)
    {
        using var activity = SivarErpTelemetry.StartActivity("AccountingService.CreateTransaction");
        activity?.SetTag("document.number", document.DocumentNumber);
        activity?.SetTag("document.type", document.DocumentType?.Code);
        
        var stopwatch = Stopwatch.StartNew();
        var userId = GetCurrentUserId();
        
        try
        {
            _logger.LogInformation("Creating transaction from document {DocumentNumber}", document.DocumentNumber);
            
            // Core business logic
            var transaction = await CreateTransactionLogic(document);
            
            stopwatch.Stop();
            
            // Track via OpenTelemetry
            await _performanceTracking.TrackRequestAsync(
                "AccountingService.CreateTransactionFromDocumentAsync", 
                stopwatch.Elapsed, 
                userId);
            
            await _performanceTracking.TrackTransactionCreatedAsync(
                document.DocumentType?.Code ?? "Unknown", 
                stopwatch.Elapsed.TotalMilliseconds);
            
            activity?.SetTag("transaction.number", transaction.TransactionNumber);
            activity?.SetStatus(ActivityStatusCode.Ok);
            
            return transaction;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            SivarErpTelemetry.RecordException(ex, activity);
            
            await _performanceTracking.TrackRequestAsync(
                "AccountingService.CreateTransactionFromDocumentAsync", 
                stopwatch.Elapsed, 
                userId, 
                isError: true);
            
            _logger.LogError(ex, "Failed to create transaction from document {DocumentNumber}", document.DocumentNumber);
            throw;
        }
    }
}
```

### Free Observability Stack Setup

#### Docker Compose for Development
```yaml
# Target: Sivar.Erp.Core/docker-compose.observability.yml
version: '3.8'
services:
  # Jaeger - Distributed Tracing
  jaeger:
    image: jaegertracing/all-in-one:1.57
    ports:
      - "16686:16686"  # Jaeger UI
      - "14268:14268"  # Jaeger collector
      - "6831:6831/udp"  # Jaeger agent
    environment:
      - COLLECTOR_OTLP_ENABLED=true

  # Prometheus - Metrics Collection
  prometheus:
    image: prom/prometheus:v2.45.0
    ports:
      - "9090:9090"
    volumes:
      - ./prometheus.yml:/etc/prometheus/prometheus.yml
    command:
      - '--config.file=/etc/prometheus/prometheus.yml'
      - '--storage.tsdb.path=/prometheus'
      - '--web.console.libraries=/etc/prometheus/console_libraries'
      - '--web.console.templates=/etc/prometheus/consoles'
      - '--web.enable-lifecycle'

  # Grafana - Visualization Dashboard
  grafana:
    image: grafana/grafana:10.0.0
    ports:
      - "3000:3000"
    environment:
      - GF_SECURITY_ADMIN_PASSWORD=admin
    volumes:
      - grafana-storage:/var/lib/grafana
      - ./grafana/dashboards:/etc/grafana/provisioning/dashboards
      - ./grafana/datasources:/etc/grafana/provisioning/datasources

  # OTEL Collector (Optional - for advanced scenarios)
  otel-collector:
    image: otel/opentelemetry-collector-contrib:0.88.0
    command: ["--config=/etc/otel-collector-config.yaml"]
    volumes:
      - ./otel-collector-config.yaml:/etc/otel-collector-config.yaml
    ports:
      - "4317:4317"   # OTLP gRPC receiver
      - "4318:4318"   # OTLP HTTP receiver
    depends_on:
      - jaeger
      - prometheus

volumes:
  grafana-storage:
```

#### Prometheus Configuration
```yaml
# Target: Sivar.Erp.Core/prometheus.yml
global:
  scrape_interval: 15s

scrape_configs:
  - job_name: 'sivar-erp-core'
    static_configs:
      - targets: ['host.docker.internal:5000']  # Your ASP.NET Core app
    metrics_path: '/metrics'
    scrape_interval: 5s

  - job_name: 'prometheus'
    static_configs:
      - targets: ['localhost:9090']
```

#### Grafana Dashboard Configuration
```json
// Target: Sivar.Erp.Core/grafana/dashboards/sivar-erp-dashboard.json
{
  "dashboard": {
    "title": "Sivar ERP Performance Dashboard",
    "panels": [
      {
        "title": "Request Rate",
        "type": "stat",
        "targets": [
          {
            "expr": "rate(sivar_erp_requests_total[5m])",
            "legendFormat": "Requests/sec"
          }
        ]
      },
      {
        "title": "Error Rate",
        "type": "stat",
        "targets": [
          {
            "expr": "rate(sivar_erp_errors_total[5m]) / rate(sivar_erp_requests_total[5m]) * 100",
            "legendFormat": "Error Rate %"
          }
        ]
      },
      {
        "title": "Response Time P95",
        "type": "stat",
        "targets": [
          {
            "expr": "histogram_quantile(0.95, sivar_erp_request_duration_ms_bucket)",
            "legendFormat": "P95 Response Time (ms)"
          }
        ]
      },
      {
        "title": "Active Users",
        "type": "stat",
        "targets": [
          {
            "expr": "sivar_erp_active_users",
            "legendFormat": "Active Users"
          }
        ]
      },
      {
        "title": "Memory Usage",
        "type": "graph",
        "targets": [
          {
            "expr": "sivar_erp_memory_usage_bytes",
            "legendFormat": "Memory Usage (bytes)"
          }
        ]
      },
      {
        "title": "Business Metrics - Transactions",
        "type": "graph",
        "targets": [
          {
            "expr": "rate(sivar_erp_transactions_created[5m])",
            "legendFormat": "Transactions/sec"
          }
        ]
      }
    ]
  }
}
```

### Benefits of OpenTelemetry Approach

✅ **Industry Standard**: Microsoft-backed, future-proof observability
✅ **Vendor Neutral**: Can switch between different backends (Jaeger, Zipkin, etc.)
✅ **Free Open Source**: Complete stack available without licensing costs
✅ **Rich Ecosystem**: Extensive .NET integration and community support
✅ **Automatic Instrumentation**: ASP.NET Core, HTTP, SQL automatically tracked
✅ **Custom Business Metrics**: Easy to add ERP-specific metrics
✅ **Distributed Tracing**: Track requests across service boundaries
✅ **Real-time Monitoring**: Live dashboards and alerting
✅ **Historical Analysis**: Long-term trend analysis and capacity planning

### Quick Start Commands
```bash
# Start the observability stack
docker-compose -f docker-compose.observability.yml up -d

# Access the dashboards
# Jaeger UI: http://localhost:16686
# Grafana: http://localhost:3000 (admin/admin)
# Prometheus: http://localhost:9090
```

This approach gives you enterprise-grade observability with zero licensing costs and seamless .NET integration!

### Core Performance KPIs

#### 1. System Health Metrics
```csharp
// Target: Sivar.Erp.Core/Monitoring/SystemHealthMetrics.cs
public class SystemHealthMetrics
{
    public DateTime Timestamp { get; set; }
    public int ActiveUsers { get; set; }
    public long MemoryUsageMB { get; set; }
    public double CpuUsagePercent { get; set; }
    public int ErrorCount { get; set; }
    public int WarningCount { get; set; }
    public int RequestCount { get; set; }
    public double AverageResponseTimeMs { get; set; }
    public Dictionary<string, long> MethodExecutionTimes { get; set; }
}
```

#### 2. Performance Tracking Service
```csharp
// Target: Sivar.Erp.Core/Monitoring/IPerformanceTrackingService.cs
public interface IPerformanceTrackingService
{
    Task TrackRequestAsync(string methodName, TimeSpan executionTime, string userId, bool isError = false);
    Task TrackMemoryUsageAsync(long memoryUsageMB);
    Task TrackActiveUsersAsync(int activeUserCount);
    Task<MonthlyPerformanceReport> GenerateMonthlyReportAsync(DateTime month);
    Task<ResourceProjection> ProjectResourceNeedsAsync(int projectedUsers, double growthFactor);
}

public class PerformanceTrackingService : IPerformanceTrackingService
{
    private readonly IRepository _repository;
    private readonly ILogger<PerformanceTrackingService> _logger;
    private readonly IMemoryMonitor _memoryMonitor;
    private readonly ICpuMonitor _cpuMonitor;

    public async Task TrackRequestAsync(string methodName, TimeSpan executionTime, string userId, bool isError = false)
    {
        var metric = _repository.CreateObject<PerformanceMetricDto>();
        metric.Timestamp = DateTime.UtcNow;
        metric.MethodName = methodName;
        metric.ExecutionTimeMs = executionTime.TotalMilliseconds;
        metric.UserId = userId;
        metric.IsError = isError;
        metric.MemoryUsageMB = _memoryMonitor.GetCurrentUsageMB();
        metric.CpuUsagePercent = _cpuMonitor.GetCurrentUsagePercent();
        
        await _repository.SaveChangesAsync();
        
        _logger.LogDebug("Performance metric tracked: {MethodName} - {ExecutionTime}ms - User: {UserId}", 
            methodName, executionTime.TotalMilliseconds, userId);
    }

    public async Task<MonthlyPerformanceReport> GenerateMonthlyReportAsync(DateTime month)
    {
        var startDate = new DateTime(month.Year, month.Month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);
        
        var metrics = _repository.GetObjects<PerformanceMetricDto>()
            .Where(m => m.Timestamp >= startDate && m.Timestamp <= endDate)
            .ToList();

        var report = new MonthlyPerformanceReport
        {
            Month = month,
            TotalRequests = metrics.Count,
            ErrorRate = (double)metrics.Count(m => m.IsError) / metrics.Count * 100,
            AverageResponseTime = metrics.Average(m => m.ExecutionTimeMs),
            PeakMemoryUsage = metrics.Max(m => m.MemoryUsageMB),
            AverageMemoryUsage = metrics.Average(m => m.MemoryUsageMB),
            PeakCpuUsage = metrics.Max(m => m.CpuUsagePercent),
            AverageCpuUsage = metrics.Average(m => m.CpuUsagePercent),
            UniqueActiveUsers = metrics.Select(m => m.UserId).Distinct().Count(),
            TopSlowMethods = metrics
                .GroupBy(m => m.MethodName)
                .Select(g => new MethodPerformance
                {
                    MethodName = g.Key,
                    AverageExecutionTime = g.Average(m => m.ExecutionTimeMs),
                    CallCount = g.Count(),
                    ErrorRate = (double)g.Count(m => m.IsError) / g.Count() * 100
                })
                .OrderByDescending(m => m.AverageExecutionTime)
                .Take(10)
                .ToList()
        };

        return report;
    }

    public async Task<ResourceProjection> ProjectResourceNeedsAsync(int projectedUsers, double growthFactor)
    {
        // Get last 3 months of data for baseline
        var threeMonthsAgo = DateTime.UtcNow.AddMonths(-3);
        var baselineMetrics = _repository.GetObjects<PerformanceMetricDto>()
            .Where(m => m.Timestamp >= threeMonthsAgo)
            .ToList();

        var currentAvgUsers = baselineMetrics.Select(m => m.UserId).Distinct().Count();
        var currentAvgMemory = baselineMetrics.Average(m => m.MemoryUsageMB);
        var currentAvgCpu = baselineMetrics.Average(m => m.CpuUsagePercent);
        var currentAvgResponseTime = baselineMetrics.Average(m => m.ExecutionTimeMs);

        var userMultiplier = (double)projectedUsers / currentAvgUsers * growthFactor;

        return new ResourceProjection
        {
            CurrentUsers = currentAvgUsers,
            ProjectedUsers = projectedUsers,
            GrowthFactor = growthFactor,
            CurrentMemoryUsageMB = (long)currentAvgMemory,
            ProjectedMemoryUsageMB = (long)(currentAvgMemory * userMultiplier),
            CurrentCpuUsagePercent = currentAvgCpu,
            ProjectedCpuUsagePercent = currentAvgCpu * userMultiplier,
            CurrentResponseTimeMs = currentAvgResponseTime,
            ProjectedResponseTimeMs = currentAvgResponseTime * Math.Sqrt(userMultiplier), // Response time increases sub-linearly
            RecommendedActions = GenerateRecommendations(userMultiplier, currentAvgMemory, currentAvgCpu)
        };
    }
}
```

#### 3. Performance Monitoring Models
```csharp
// Target: Sivar.Erp.Core/Monitoring/Models/PerformanceModels.cs
public class PerformanceMetricDto : IEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime Timestamp { get; set; }
    public string MethodName { get; set; } = string.Empty;
    public double ExecutionTimeMs { get; set; }
    public string UserId { get; set; } = string.Empty;
    public bool IsError { get; set; }
    public bool IsWarning { get; set; }
    public long MemoryUsageMB { get; set; }
    public double CpuUsagePercent { get; set; }
    public string? AdditionalContext { get; set; }
}

public class MonthlyPerformanceReport
{
    public DateTime Month { get; set; }
    public int TotalRequests { get; set; }
    public double ErrorRate { get; set; }
    public double WarningRate { get; set; }
    public double AverageResponseTime { get; set; }
    public double MedianResponseTime { get; set; }
    public double P95ResponseTime { get; set; }
    public double P99ResponseTime { get; set; }
    public long PeakMemoryUsage { get; set; }
    public long AverageMemoryUsage { get; set; }
    public double PeakCpuUsage { get; set; }
    public double AverageCpuUsage { get; set; }
    public int UniqueActiveUsers { get; set; }
    public int PeakConcurrentUsers { get; set; }
    public List<MethodPerformance> TopSlowMethods { get; set; } = new();
    public List<MethodPerformance> MostFrequentMethods { get; set; } = new();
    public List<DailyMetrics> DailyBreakdown { get; set; } = new();
    public PerformanceTrend Trend { get; set; } = new();
}

public class ResourceProjection
{
    public int CurrentUsers { get; set; }
    public int ProjectedUsers { get; set; }
    public double GrowthFactor { get; set; }
    public long CurrentMemoryUsageMB { get; set; }
    public long ProjectedMemoryUsageMB { get; set; }
    public double CurrentCpuUsagePercent { get; set; }
    public double ProjectedCpuUsagePercent { get; set; }
    public double CurrentResponseTimeMs { get; set; }
    public double ProjectedResponseTimeMs { get; set; }
    public List<string> RecommendedActions { get; set; } = new();
    public ResourceRequirements RecommendedHardware { get; set; } = new();
}

public class ResourceRequirements
{
    public int RecommendedCpuCores { get; set; }
    public long RecommendedMemoryGB { get; set; }
    public long RecommendedStorageGB { get; set; }
    public string RecommendedInstanceType { get; set; } = string.Empty;
    public decimal EstimatedMonthlyCost { get; set; }
}

public class MethodPerformance
{
    public string MethodName { get; set; } = string.Empty;
    public double AverageExecutionTime { get; set; }
    public double MedianExecutionTime { get; set; }
    public double P95ExecutionTime { get; set; }
    public int CallCount { get; set; }
    public double ErrorRate { get; set; }
    public double WarningRate { get; set; }
    public long TotalMemoryImpact { get; set; }
}

public class DailyMetrics
{
    public DateTime Date { get; set; }
    public int RequestCount { get; set; }
    public double AverageResponseTime { get; set; }
    public int ErrorCount { get; set; }
    public int WarningCount { get; set; }
    public int ActiveUsers { get; set; }
    public long AverageMemoryUsage { get; set; }
    public double AverageCpuUsage { get; set; }
}

public class PerformanceTrend
{
    public double ResponseTimeTrend { get; set; } // Positive = getting slower
    public double ErrorRateTrend { get; set; } // Positive = more errors
    public double MemoryUsageTrend { get; set; } // Positive = more memory
    public double UserGrowthTrend { get; set; } // Positive = more users
    public string TrendSummary { get; set; } = string.Empty;
}
```

#### 4. Performance Interceptor for Automatic Tracking
```csharp
// Target: Sivar.Erp.Core/Monitoring/PerformanceInterceptor.cs
public class PerformanceInterceptor : IInterceptor
{
    private readonly IPerformanceTrackingService _performanceTracking;
    private readonly ILogger<PerformanceInterceptor> _logger;

    public void Intercept(IInvocation invocation)
    {
        var stopwatch = Stopwatch.StartNew();
        var methodName = $"{invocation.TargetType.Name}.{invocation.Method.Name}";
        var userId = GetCurrentUserId();
        bool isError = false;

        try
        {
            invocation.Proceed();
        }
        catch (Exception ex)
        {
            isError = true;
            _logger.LogError(ex, "Error in method {MethodName}", methodName);
            throw;
        }
        finally
        {
            stopwatch.Stop();
            
            // Track performance asynchronously to avoid blocking
            _ = Task.Run(async () =>
            {
                try
                {
                    await _performanceTracking.TrackRequestAsync(methodName, stopwatch.Elapsed, userId, isError);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to track performance for {MethodName}", methodName);
                }
            });
        }
    }
}
```

#### 5. Monthly KPI Dashboard Data
```csharp
// Target: Sivar.Erp.Core/Monitoring/KpiDashboardService.cs
public class KpiDashboardService : IKpiDashboardService
{
    public async Task<KpiDashboard> GenerateMonthlyKpiDashboardAsync(DateTime month)
    {
        var performanceReport = await _performanceTracking.GenerateMonthlyReportAsync(month);
        var previousMonth = month.AddMonths(-1);
        var previousReport = await _performanceTracking.GenerateMonthlyReportAsync(previousMonth);

        return new KpiDashboard
        {
            Month = month,
            SystemHealth = new SystemHealthKpis
            {
                ErrorRate = performanceReport.ErrorRate,
                ErrorRateChange = performanceReport.ErrorRate - previousReport.ErrorRate,
                WarningRate = performanceReport.WarningRate,
                WarningRateChange = performanceReport.WarningRate - previousReport.WarningRate,
                Availability = CalculateAvailability(performanceReport),
                AvailabilityChange = CalculateAvailabilityChange(performanceReport, previousReport)
            },
            Performance = new PerformanceKpis
            {
                AverageResponseTime = performanceReport.AverageResponseTime,
                ResponseTimeChange = performanceReport.AverageResponseTime - previousReport.AverageResponseTime,
                P95ResponseTime = performanceReport.P95ResponseTime,
                ThroughputRpm = CalculateThroughput(performanceReport),
                ThroughputChange = CalculateThroughputChange(performanceReport, previousReport)
            },
            Resources = new ResourceKpis
            {
                AverageMemoryUsage = performanceReport.AverageMemoryUsage,
                MemoryUsageChange = performanceReport.AverageMemoryUsage - previousReport.AverageMemoryUsage,
                PeakMemoryUsage = performanceReport.PeakMemoryUsage,
                AverageCpuUsage = performanceReport.AverageCpuUsage,
                CpuUsageChange = performanceReport.AverageCpuUsage - previousReport.AverageCpuUsage,
                PeakCpuUsage = performanceReport.PeakCpuUsage
            },
            Users = new UserKpis
            {
                ActiveUsers = performanceReport.UniqueActiveUsers,
                ActiveUsersChange = performanceReport.UniqueActiveUsers - previousReport.UniqueActiveUsers,
                PeakConcurrentUsers = performanceReport.PeakConcurrentUsers,
                UserGrowthRate = CalculateUserGrowthRate(performanceReport, previousReport)
            },
            Capacity = new CapacityKpis
            {
                CurrentCapacityUtilization = CalculateCapacityUtilization(performanceReport),
                ProjectedCapacityDate = CalculateProjectedCapacityDate(performanceReport),
                RecommendedActions = GenerateCapacityRecommendations(performanceReport)
            }
        };
    }
}
```

### Resource-to-Performance Correlation Formula

#### Mathematical Model
```csharp
// Target: Sivar.Erp.Core/Monitoring/PerformanceFormulas.cs
public static class PerformanceFormulas
{
    /// <summary>
    /// Calculate expected performance based on hardware resources
    /// Formula: Y = (CPU_Cores * CPU_Weight + Memory_GB * Memory_Weight + Storage_IOPS * Storage_Weight) / User_Count
    /// </summary>
    public static PerformanceProjection CalculateExpectedPerformance(HardwareSpecs hardware, int userCount)
    {
        const double CPU_WEIGHT = 0.4;
        const double MEMORY_WEIGHT = 0.4;
        const double STORAGE_WEIGHT = 0.2;
        
        var performanceScore = (hardware.CpuCores * CPU_WEIGHT + 
                               hardware.MemoryGB * MEMORY_WEIGHT + 
                               hardware.StorageIOPS / 1000.0 * STORAGE_WEIGHT) / userCount;
        
        return new PerformanceProjection
        {
            ExpectedResponseTimeMs = CalculateResponseTime(performanceScore),
            ExpectedThroughputRpm = CalculateThroughput(performanceScore),
            ExpectedConcurrentUsers = CalculateConcurrentUsers(performanceScore),
            ConfidenceLevel = CalculateConfidence(hardware, userCount)
        };
    }

    /// <summary>
    /// X Resources → Y Results tracking formula
    /// </summary>
    public static string GenerateResourcePerformanceFormula(List<MonthlyPerformanceReport> historicalData)
    {
        // Linear regression analysis on historical data
        var formula = $"Performance = {CalculateBaselinePerformance(historicalData)} + " +
                     $"(CPU_Cores * {CalculateCpuCoefficient(historicalData)}) + " +
                     $"(Memory_GB * {CalculateMemoryCoefficient(historicalData)}) - " +
                     $"(User_Count * {CalculateUserImpactCoefficient(historicalData)})";
        
        return formula;
    }
}
```

### KPI Tracking Implementation in Services

#### Integration with Core Services
```csharp
// Example: Enhanced AccountingService with Performance Tracking
public class AccountingService : IAccountingService
{
    private readonly IRepository _repository;
    private readonly ILogger<AccountingService> _logger;
    private readonly IPerformanceTrackingService _performanceTracking;

    public async Task<ITransaction> CreateTransactionFromDocumentAsync(IDocument document)
    {
        var stopwatch = Stopwatch.StartNew();
        var userId = GetCurrentUserId();
        
        try
        {
            _logger.LogInformation("Creating transaction from document {DocumentNumber}", document.DocumentNumber);
            
            // Core business logic
            var transaction = await CreateTransactionLogic(document);
            
            stopwatch.Stop();
            await _performanceTracking.TrackRequestAsync(
                "AccountingService.CreateTransactionFromDocumentAsync", 
                stopwatch.Elapsed, 
                userId);
            
            return transaction;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            await _performanceTracking.TrackRequestAsync(
                "AccountingService.CreateTransactionFromDocumentAsync", 
                stopwatch.Elapsed, 
                userId, 
                isError: true);
            
            _logger.LogError(ex, "Failed to create transaction from document {DocumentNumber}", document.DocumentNumber);
            throw;
        }
    }
}
```

### Monitoring Dashboard Setup

#### Performance Dashboard Configuration
```csharp
// Target: Sivar.Erp.Core/Configuration/MonitoringConfiguration.cs
public static class MonitoringConfiguration
{
    public static IServiceCollection AddPerformanceMonitoring(this IServiceCollection services)
    {
        services.AddScoped<IPerformanceTrackingService, PerformanceTrackingService>();
        services.AddScoped<IKpiDashboardService, KpiDashboardService>();
        services.AddScoped<IMemoryMonitor, MemoryMonitor>();
        services.AddScoped<ICpuMonitor, CpuMonitor>();
        
        // Register performance interceptor
        services.AddSingleton<PerformanceInterceptor>();
        
        // Background service for automated KPI generation
        services.AddHostedService<MonthlyKpiGeneratorService>();
        
        return services;
    }
}
```

### Key Performance Indicators (KPIs)

1. **System Health KPIs**
   - Error Rate (target: < 0.1%)
   - Warning Rate (target: < 1%)
   - System Availability (target: > 99.9%)

2. **Performance KPIs**
   - Average Response Time (target: < 200ms)
   - P95 Response Time (target: < 500ms)
   - Throughput (requests per minute)

3. **Resource KPIs**
   - Memory Usage (target: < 80% of available)
   - CPU Usage (target: < 70% average)
   - Storage I/O utilization

4. **User KPIs**
   - Active Users (month-over-month growth)
   - Concurrent Users (peak capacity)
   - User Session Duration

5. **Capacity Planning KPIs**
   - Current Capacity Utilization
   - Projected Capacity Exhaustion Date
   - Resource Efficiency Ratio

This performance tracking system enables precise capacity planning with formulas like:
**"With X CPU cores and Y GB RAM, the system can handle Z concurrent users at W response time"**

## Timeline Summary

- **Week 1**: Foundation Setup (Repository, Logging, DI, Performance Monitoring)
- **Week 2**: Core Services Migration (Accounting, Tax, Import) + KPI Integration
- **Week 3**: Missing Modules Implementation (Payment, Inventory, Security) + Performance Tracking
- **Week 4**: Journal Entry Services Migration + Advanced KPI Dashboard
- **Week 5**: Test Infrastructure Migration + Performance Validation
- **Week 6**: CompleteAccountingWorkflowTest Migration and Performance Baseline

**Total Duration**: 6 weeks
**Final Goal**: Complete Sivar.Erp functionality in Sivar.Erp.Core with Repository pattern, optimized for reading speed, using Microsoft Logging, comprehensive performance tracking, and passing the CompleteAccountingWorkflowTest.
