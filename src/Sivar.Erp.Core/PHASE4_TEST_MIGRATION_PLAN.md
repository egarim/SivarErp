# Phase 4: CompleteAccountingWorkflowTest Migration Plan

## 🎯 **Objective**: Clean rewrite of CompleteAccountingWorkflowTest using Sivar.Erp.Core infrastructure

### **Current Status Assessment:**

#### ✅ **What We Have in Core:**
- Modern Repository pattern (`IRepository`, `InMemoryRepository`)
- Core Services (`AccountingService`, `TaxService`, `DataImportService`)
- Dependency Injection configuration (`ServiceCollectionExtensions`)
- Microsoft Logging integration
- Performance monitoring and telemetry
- Phase 3 advanced features (Localization, AI, Performance)

#### ❌ **What We Need to Build:**
1. **Core Test Infrastructure**
2. **Modern Service Factory**
3. **Data Import/Export Services for Core**
4. **Clean Test Implementation**

---

## 🛠 **Implementation Plan**

### **Day 1: Core Test Infrastructure**

#### **1.1 Core Test Service Factory**
```csharp
// Sivar.Erp.Core/Tests/Infrastructure/CoreTestServiceFactory.cs
public static class CoreTestServiceFactory
{
    public static IServiceProvider CreateServiceProvider(Action<IServiceCollection>? configure = null)
    {
        var services = new ServiceCollection();
        
        // Add Core ERP services
        services.AddSivarErpCore(options =>
        {
            options.UseInMemoryRepository = true;
            options.EnableTelemetry = false; // Disable for tests
            options.EnableStructuredLogging = true;
            options.MinimumLogLevel = LogLevel.Debug;
        });
        
        // Add Phase 3 features for tests
        services.AddSivarErpPhase3Features();
        
        // Configure test-specific services
        configure?.Invoke(services);
        
        return services.BuildServiceProvider();
    }
}
```

#### **1.2 Core Test Data Import Service**
```csharp
// Sivar.Erp.Core/Tests/Infrastructure/CoreTestDataImporter.cs
public class CoreTestDataImporter
{
    private readonly IRepository _repository;
    private readonly IDataImportService _dataImportService;
    private readonly ILogger<CoreTestDataImporter> _logger;
    
    public async Task<DataImportResult> ImportTestDataAsync(string dataDirectory)
    {
        // Use Core DataImportService to import CSV test data
        return await _dataImportService.ImportFromDirectoryAsync(dataDirectory, "TestUser");
    }
}
```

### **Day 2: Modern Test Implementation**

#### **2.1 New Test Class Structure**
```csharp
// Sivar.Erp.Core.Tests/Integration/CompleteAccountingWorkflowCoreTest.cs
[TestFixture]
public class CompleteAccountingWorkflowCoreTest
{
    private IServiceProvider _serviceProvider = null!;
    private IRepository _repository = null!;
    private IAccountingService _accountingService = null!;
    private ITaxService _taxService = null!;
    private IDataImportService _dataImportService = null!;
    private ILogger<CompleteAccountingWorkflowCoreTest> _logger = null!;

    [SetUp]
    public void Setup()
    {
        _serviceProvider = CoreTestServiceFactory.CreateServiceProvider(services =>
        {
            // Add test-specific configurations
            services.AddSingleton<TestUserContext>();
        });
        
        _repository = _serviceProvider.GetRequiredService<IRepository>();
        _accountingService = _serviceProvider.GetRequiredService<IAccountingService>();
        _taxService = _serviceProvider.GetRequiredService<ITaxService>();
        _dataImportService = _serviceProvider.GetRequiredService<IDataImportService>();
        _logger = _serviceProvider.GetRequiredService<ILogger<CompleteAccountingWorkflowCoreTest>>();
    }

    [Test]
    public async Task ExecuteCompleteWorkflowTest()
    {
        // 1. Import test data using Core services
        await ImportTestData();
        
        // 2. Create purchase transaction
        var purchaseDocument = CreatePurchaseDocument();
        var purchaseTransaction = await _accountingService.CreateTransactionAsync(purchaseDocument);
        
        // 3. Create sales transaction
        var salesDocument = CreateSalesDocument();
        var salesTransaction = await _accountingService.CreateTransactionAsync(salesDocument);
        
        // 4. Verify accounting integrity
        await ValidateAccountingIntegrity();
        
        Assert.Pass("Core accounting workflow executed successfully!");
    }
}
```

### **Day 3: Data Services Migration**

#### **3.1 Missing Services Implementation**
- Document creation and validation
- Tax calculation services
- Account balance verification
- Transaction integrity checks

#### **3.2 Modern CSV Import/Export**
- Leverage existing Core `DataImportService`
- Add Core-specific test data models
- Modern async/await patterns throughout

### **Day 4-5: Integration and Testing**

#### **4.1 Build and Test Verification**
- Ensure all Core components compile
- Run new test and verify it passes
- Performance benchmarking against legacy test

#### **4.2 Documentation**
- Update migration plan with completion status
- Document new test patterns for future use

---

## 🎯 **Key Differences from Legacy Test:**

### **Modern Patterns:**
- ✅ Repository pattern instead of `IObjectDb`
- ✅ Modern dependency injection
- ✅ Async/await throughout
- ✅ Structured logging
- ✅ Clean service boundaries

### **Eliminated Complexity:**
- ❌ No backward compatibility
- ❌ No legacy service dependencies  
- ❌ No mixed patterns
- ❌ No adapter/bridge classes

---

## 📊 **Implementation Effort:**

| Component | Effort | Status |
|-----------|--------|--------|
| Core Test Factory | 4 hours | ⏳ TODO |
| Test Data Import | 6 hours | ⏳ TODO |
| Modern Test Class | 8 hours | ⏳ TODO |
| Missing Services | 12 hours | ⏳ TODO |
| Integration & Testing | 4 hours | ⏳ TODO |
| **Total** | **34 hours** | **~4-5 days** |

---

## 🚀 **Next Steps:**

1. **Start with Core Test Factory** - Foundation for everything else
2. **Build Modern Test Data Import** - Leverage existing Core services
3. **Implement Clean Test Class** - No legacy dependencies
4. **Add Missing Core Services** - Fill gaps as needed
5. **Validate and Document** - Ensure quality and maintainability

This approach gives us a **clean, modern, maintainable test** that showcases the power of the new Core infrastructure without any legacy baggage! 🎉
