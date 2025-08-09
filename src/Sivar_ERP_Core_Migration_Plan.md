# Sivar.ERP ? Sivar.ERP.Core Migration Plan

## ?? **Migration Overview**

This document outlines the complete migration from the current `Sivar.ERP` to a redesigned `Sivar.ERP.Core` that follows modern architectural principles while maintaining the minimum viable product (MVP) functionality demonstrated in `CompleteAccountingWorkflowTest.cs`.

## ?? **Current State Analysis**

### **Issues with Current Architecture**
- **Hard Dependencies**: Direct instantiation of services and tight coupling
- **No Dependency Injection**: Manual service creation throughout the codebase
- **IObjectDb Anti-Pattern**: 23+ entity-specific collections violate SRP
- **Missing Interfaces**: Many services lack interface abstractions
- **Poor Testability**: Difficult to mock and unit test components
- **Performance Issues**: No optimization for memory usage or data loading
- **No Localization**: Hardcoded strings and no internationalization support

### **MVP Requirements (From CompleteAccountingWorkflowTest)**
The test demonstrates the essential ERP workflow that **MUST** continue to work:

1. **Data Import System**: CSV import for accounts, taxes, business entities, items
2. **Tax Calculation Engine**: Complex tax rules and accounting profiles
3. **Document Processing**: Sales/Purchase invoice creation and processing
4. **Transaction Generation**: Converting documents to accounting entries
5. **Journal Entry System**: Transaction posting and reporting
6. **Payment Processing**: Payment methods and transaction generation
7. **Basic Security**: User authentication and authorization
8. **Inventory Tracking**: Basic inventory management and kardex reports
9. **Performance Monitoring**: Built-in diagnostics and logging

### **Critical CSV Import System**
The current system has a sophisticated CSV import infrastructure that's essential for test data setup:

#### **Import Services Currently Available:**
- `IAccountImportExportService` - Chart of accounts import
- `ITaxImportExportService` - Tax definitions import
- `ITaxGroupImportExportService` - Tax group classifications
- `IBusinessEntityImportExportService` - Customer/Supplier import
- `IItemImportExportService` - Product/Service items import
- `IDocumentTypeImportExportService` - Document type definitions
- `ITaxRuleImportExportService` - Complex tax calculation rules
- `IGroupMembershipImportExportService` - Entity group assignments
- `ITestAccountMappingImportExportService` - Account mapping for tests

#### **CSV Files Used in Testing:**
- `ComercialChartOfAccounts.txt` - Complete chart of accounts
- `ElSalvadorTaxGroups.txt` - Tax group definitions
- `ElSalvadorTaxes.txt` - Tax rate and type definitions
- `ElSalvadorTaxRules.txt` - Complex tax calculation rules
- `BusinesEntities.txt` - Customer and supplier data
- `Items.txt` - Product and service definitions
- `TestAccountMappings.csv` - Logical to actual account mappings
- `TaxAccountingProfiles.csv` - Tax accounting configurations
- `DocumentAccountingProfiles.csv` - Document accounting rules

## ??? **New Architecture: Sivar.ERP.Core**

### **Core Architectural Principles**

#### **1. Dependency Injection First**
```csharp
// All services must be interface-based and DI-friendly
public interface IAccountingService
{
    Task<ITransaction> CreateTransactionAsync(IDocument document);
    Task<bool> PostTransactionAsync(ITransaction transaction);
}

public class AccountingService : IAccountingService
{
    private readonly IRepository _repository;
    private readonly ILogger<AccountingService> _logger;
    
    public AccountingService(IRepository repository, ILogger<AccountingService> logger)
    {
        _repository = repository;
        _logger = logger;
    }
}
```

#### **2. Generic Repository Pattern (XAF-Inspired)**
```csharp
public interface IRepository : IDisposable
{
    T CreateObject<T>() where T : class, new();
    T? GetObjectByKey<T>(object key) where T : class;
    IQueryable<T> GetObjects<T>() where T : class;
    T? FindObject<T>(Expression<Func<T, bool>> criteria) where T : class;
    Task CommitChanges();
    void Rollback();
    bool IsModified { get; }
}
```

#### **3. SOLID Design Principles**
- **S**ingle Responsibility: Each service has one clear purpose
- **O**pen/Closed: Services extensible via interfaces and DI
- **L**iskov Substitution: All implementations fully interchangeable
- **I**nterface Segregation: Small, focused interfaces
- **D**ependency Inversion: Depend on abstractions, not concretions

#### **4. Modular Architecture**
```
Sivar.ERP.Core/
??? Core/                          # Core abstractions and interfaces
?   ??? IRepository.cs
?   ??? IEntity.cs
?   ??? ServiceCollectionExtensions.cs
??? Modules/
?   ??? Accounting/               # Accounting module
?   ?   ??? IAccountingService.cs
?   ?   ??? AccountingService.cs
?   ?   ??? AccountingModule.cs
?   ??? DataImport/               # **ENHANCED** CSV Import System
?   ?   ??? IDataImportService.cs
?   ?   ??? ICsvImportService.cs
?   ?   ??? DataImportService.cs
?   ?   ??? Importers/
?   ?       ??? AccountImporter.cs
?   ?       ??? TaxImporter.cs
?   ?       ??? BusinessEntityImporter.cs
?   ??? Inventory/                # Inventory module
?   ??? Payments/                 # Payments module
?   ??? Security/                 # Security module
?   ??? Documents/                # Document processing
??? Infrastructure/               # Infrastructure concerns
?   ??? Data/
?   ?   ??? InMemoryRepository.cs
?   ?   ??? EntityFrameworkRepository.cs
?   ?   ??? XpoRepository.cs
?   ??? Localization/
?   ??? Performance/
??? Demo/                         # In-memory demo implementation
    ??? DemoServiceCollection.cs
    ??? SampleDataGenerator.cs
    ??? TestData/                 # **NEW** Embedded test CSV files
        ??? ElSalvador/
        ?   ??? ComercialChartOfAccounts.txt
        ?   ??? ElSalvadorTaxes.txt
        ?   ??? BusinesEntities.txt
        ??? TestAccountMappings.csv
```

## ?? **Migration Phases**

### **Phase 1: Core Infrastructure (Week 1)**

#### **Day 1-2: Project Structure and Core Abstractions**

1. **Create New Project Structure**
   ```
   ? Create Sivar.ERP.Core project (.NET 9)
   ? Create Sivar.ERP.Core.Tests project
   ? Setup solution structure with proper dependencies
   ? Embed existing CSV test data files as resources
   ```

2. **Core Interfaces and Abstractions**
   ```csharp
   // Core/IEntity.cs
   [Description("Base interface for all business entities")]
   public interface IEntity
   {
       [Description("Unique identifier for the entity")]
       Guid Id { get; set; }
       
       [Description("Date when the entity was created")]
       DateTime CreatedAt { get; set; }
       
       [Description("Date when the entity was last updated")]
       DateTime UpdatedAt { get; set; }
   }
   
   // Core/IRepository.cs - Generic repository pattern
   [Description("Generic repository interface for data access operations")]
   public interface IRepository : IDisposable
   {
       [Description("Creates a new object of the specified type")]
       T CreateObject<T>() where T : class, new();
       
       [Description("Retrieves an object by its primary key")]
       T? GetObjectByKey<T>([Description("Primary key value")] object key) where T : class;
       
       [Description("Gets all objects of the specified type")]
       IQueryable<T> GetObjects<T>() where T : class;
       
       [Description("Commits all pending changes")]
       Task CommitChanges();
   }
   ```

3. **Dependency Injection Extensions**
   ```csharp
   // Core/ServiceCollectionExtensions.cs
   [Description("Extension methods for configuring ERP services")]
   public static class ServiceCollectionExtensions
   {
       [Description("Adds core ERP services to the DI container")]
       public static IServiceCollection AddSivarErpCore(
           [Description("Service collection to configure")] this IServiceCollection services,
           [Description("Configuration options")] ErpCoreOptions? options = null)
       {
           // Register core services
           services.AddScoped<IRepository, InMemoryRepository>();
           services.AddScoped<IAccountingService, AccountingService>();
           services.AddScoped<IDocumentService, DocumentService>();
           services.AddScoped<IDataImportService, DataImportService>(); // **NEW**
           // ... etc
           
           return services;
       }
   }
   ```

#### **Day 3-4: Repository Pattern Implementation**

1. **In-Memory Repository (MVP)**
   ```csharp
   // Infrastructure/Data/InMemoryRepository.cs
   [Description("In-memory implementation of the repository pattern for demos and testing")]
   public class InMemoryRepository : IRepository
   {
       private readonly ConcurrentDictionary<Type, IList> _collections = new();
       private readonly HashSet<object> _newObjects = new();
       private readonly HashSet<object> _modifiedObjects = new();
       
       [Description("Creates a new object and adds it to the repository")]
       public T CreateObject<T>() where T : class, new()
       {
           var obj = new T();
           _newObjects.Add(obj);
           GetCollection<T>().Add(obj);
           return obj;
       }
       
       // ... rest of implementation
   }
   ```

2. **Entity Framework Repository (Future)**
   ```csharp
   // Infrastructure/Data/EntityFrameworkRepository.cs
   [Description("Entity Framework implementation of the repository pattern")]
   public class EntityFrameworkRepository : IRepository
   {
       // Future implementation for production databases
   }
   ```

#### **Day 5: Enhanced CSV Import System**

1. **Generic CSV Import Infrastructure**
   ```csharp
   // Modules/DataImport/ICsvImportService.cs
   [Description("Generic service for CSV data import operations")]
   public interface ICsvImportService
   {
       [Description("Imports entities from CSV content with validation")]
       Task<CsvImportResult<T>> ImportFromCsvAsync<T>(
           [Description("CSV content to import")] string csvContent,
           [Description("User performing the import")] string userName,
           [Description("Import options and validation rules")] CsvImportOptions<T>? options = null) 
           where T : class, new();
           
       [Description("Validates CSV structure before import")]
       Task<CsvValidationResult> ValidateCsvStructureAsync<T>(
           [Description("CSV content to validate")] string csvContent) 
           where T : class;
   }
   
   // Modules/DataImport/IDataImportService.cs  
   [Description("High-level service for coordinated data import operations")]
   public interface IDataImportService
   {
       [Description("Imports all data from embedded test resources")]
       Task<DataImportResult> ImportTestDataAsync(
           [Description("Target repository")] IRepository repository,
           [Description("Test data set to import")] string testDataSet = "ElSalvador");
           
       [Description("Imports all data from specified directory")]
       Task<DataImportResult> ImportFromDirectoryAsync(
           [Description("Target repository")] IRepository repository,
           [Description("Directory containing CSV files")] string dataDirectory,
           [Description("User performing import")] string userName = "System");
           
       [Description("Imports specific entity type from CSV")]
       Task<EntityImportResult<T>> ImportEntitiesAsync<T>(
           [Description("Target repository")] IRepository repository,
           [Description("CSV content")] string csvContent,
           [Description("User performing import")] string userName) 
           where T : class, new();
   }
   ```

2. **Specialized Entity Importers**
   ```csharp
   // Modules/DataImport/Importers/IEntityImporter.cs
   [Description("Base interface for entity-specific importers")]
   public interface IEntityImporter<T> where T : class
   {
       [Description("Imports entities with business logic validation")]
       Task<EntityImportResult<T>> ImportAsync(
           [Description("Repository for data persistence")] IRepository repository,
           [Description("CSV content to import")] string csvContent,
           [Description("User performing import")] string userName);
   }
   
   // Modules/DataImport/Importers/AccountImporter.cs
   [Description("Specialized importer for chart of accounts")]
   public class AccountImporter : IEntityImporter<IAccount>
   {
       private readonly ILogger<AccountImporter> _logger;
       private readonly ICsvImportService _csvImporter;
       private readonly IAccountValidator _validator;
       
       public async Task<EntityImportResult<IAccount>> ImportAsync(
           IRepository repository, string csvContent, string userName)
       {
           // Enhanced validation for El Salvador account structure
           // Proper error handling and logging
           // Repository-based persistence
       }
   }
   ```

### **Phase 2: Service Migration (Week 2)**

#### **Day 1-2: Accounting Module Migration**

1. **Migrate AccountingModule to AccountingService**
   ```csharp
   // Modules/Accounting/AccountingService.cs
   [Description("Core accounting service implementing business logic")]
   public class AccountingService : IAccountingService
   {
       private readonly IRepository _repository;
       private readonly ILogger<AccountingService> _logger;
       private readonly IFiscalPeriodService _fiscalPeriodService;
       
       [Description("Initializes the accounting service with required dependencies")]
       public AccountingService(
           [Description("Repository for data access")] IRepository repository,
           [Description("Logger for diagnostics")] ILogger<AccountingService> logger,
           [Description("Service for fiscal period management")] IFiscalPeriodService fiscalPeriodService)
       {
           _repository = repository ?? throw new ArgumentNullException(nameof(repository));
           _logger = logger ?? throw new ArgumentNullException(nameof(logger));
           _fiscalPeriodService = fiscalPeriodService ?? throw new ArgumentNullException(nameof(fiscalPeriodService));
       }
       
       [Description("Creates a transaction from a document with proper validation")]
       public async Task<ITransaction> CreateTransactionAsync(IDocument document, string? description = null)
       {
           _logger.LogInformation("Creating transaction from document {DocumentNumber}", document.DocumentNumber);
           
           // Implementation using repository pattern
           var transaction = _repository.CreateObject<TransactionDto>();
           // ... business logic
           
           return transaction;
       }
   }
   ```

#### **Day 3-4: Document and Tax Services**

1. **Document Service Migration**
   ```csharp
   // Modules/Documents/IDocumentService.cs
   [Description("Service for document processing and management")]
   public interface IDocumentService
   {
       [Description("Creates a new document")]
       Task<IDocument> CreateDocumentAsync(
           [Description("Document type")] IDocumentType documentType,
           [Description("Business entity")] IBusinessEntity businessEntity);
           
       [Description("Calculates taxes for a document")]
       Task CalculateDocumentTaxesAsync(
           [Description("Document to calculate taxes for")] IDocument document,
           [Description("Document operation type")] string documentOperation);
   }
   ```

2. **Tax Service Migration**
   ```csharp
   // Modules/Taxes/ITaxService.cs
   [Description("Service for tax calculations and management")]
   public interface ITaxService
   {
       [Description("Calculates applicable taxes for a document")]
       Task<IEnumerable<ITax>> GetApplicableTaxesAsync(
           [Description("Document to calculate taxes for")] IDocument document,
           [Description("Document operation type")] DocumentOperation operation);
   }
   ```

#### **Day 5: Data Import Migration**

1. **Enhanced DataImportService Implementation**
   ```csharp
   // Modules/DataImport/DataImportService.cs
   [Description("Comprehensive data import service with repository pattern")]
   public class DataImportService : IDataImportService
   {
       private readonly ICsvImportService _csvImporter;
       private readonly ILogger<DataImportService> _logger;
       private readonly Dictionary<Type, IEntityImporter> _importers;
       
       [Description("Imports all test data from embedded resources")]
       public async Task<DataImportResult> ImportTestDataAsync(
           IRepository repository, string testDataSet = "ElSalvador")
       {
           _logger.LogInformation("Starting test data import for {TestDataSet}", testDataSet);
           
           var result = new DataImportResult();
           
           try
           {
               // Import in dependency order
               await ImportAccountsFromResource(repository, testDataSet, result);
               await ImportTaxGroupsFromResource(repository, testDataSet, result);
               await ImportTaxesFromResource(repository, testDataSet, result);
               await ImportTaxRulesFromResource(repository, testDataSet, result);
               await ImportBusinessEntitiesFromResource(repository, testDataSet, result);
               await ImportItemsFromResource(repository, testDataSet, result);
               await ImportAccountMappingsFromResource(repository, testDataSet, result);
               
               // Commit all changes atomically
               await repository.CommitChanges();
               
               _logger.LogInformation("Test data import completed successfully");
               result.IsSuccessful = true;
           }
           catch (Exception ex)
           {
               _logger.LogError(ex, "Test data import failed");
               repository.Rollback();
               result.IsSuccessful = false;
               result.Errors.Add($"Import failed: {ex.Message}");
           }
           
           return result;
       }
       
       [Description("Imports data from external directory")]
       public async Task<DataImportResult> ImportFromDirectoryAsync(
           IRepository repository, string dataDirectory, string userName = "System")
       {
           // Implementation for external directory import
           // Maintains compatibility with existing test structure
       }
   }
   ```

2. **Embedded Test Data Resources**
   ```csharp
   // Demo/TestData/TestDataResourceManager.cs
   [Description("Manages embedded test data resources")]
   public static class TestDataResourceManager
   {
       [Description("Gets embedded CSV content for entity type")]
       public static async Task<string> GetTestDataCsvAsync(
           [Description("Test data set name")] string testDataSet,
           [Description("Entity type name")] string entityType)
       {
           var resourceName = $"Sivar.ERP.Core.Demo.TestData.{testDataSet}.{entityType}.txt";
           // Load from embedded resources
       }
       
       [Description("Lists available test data sets")]
       public static IEnumerable<string> GetAvailableTestDataSets()
       {
           return new[] { "ElSalvador", "USA", "Mexico" }; // Future expansion
       }
   }
   ```

### **Phase 3: Advanced Features (Week 3)**

#### **Day 1-2: Localization and AI Integration**

1. **Localization Support**
   ```csharp
   // Infrastructure/Localization/ILocalizationService.cs
   [Description("Service for managing multi-language support")]
   public interface ILocalizationService
   {
       [Description("Gets localized string for the specified key")]
       string GetString(
           [Description("Localization key")] string key,
           [Description("Optional culture code")] string? culture = null);
           
       [Description("Sets the current culture")]
       void SetCulture([Description("Culture code")] string culture);
   }
   ```

2. **AI Integration Support**
   ```csharp
   // Infrastructure/AI/IErpAiService.cs
   [Description("Service for exposing ERP functionality to AI agents")]
   public interface IErpAiService
   {
       [Description("Gets available operations for AI agents")]
       Task<IEnumerable<AiOperation>> GetAvailableOperationsAsync();
       
       [Description("Executes an operation requested by an AI agent")]
       Task<AiOperationResult> ExecuteOperationAsync(
           [Description("Operation to execute")] AiOperationRequest request);
   }
   ```

#### **Day 3-4: Performance Optimization**

1. **Performance Monitoring**
   ```csharp
   // Infrastructure/Performance/IPerformanceMonitor.cs
   [Description("Service for monitoring system performance")]
   public interface IPerformanceMonitor
   {
       [Description("Starts tracking performance for an operation")]
       IDisposable StartTracking([Description("Operation name")] string operationName);
       
       [Description("Gets performance metrics")]
       Task<PerformanceMetrics> GetMetricsAsync(
           [Description("Time range for metrics")] TimeRange timeRange);
   }
   ```

2. **Memory Optimization**
   ```csharp
   // Infrastructure/Performance/MemoryOptimizer.cs
   [Description("Utility for optimizing memory usage")]
   public class MemoryOptimizer
   {
       [Description("Configures services for low memory usage")]
       public static void ConfigureForLowMemory(
           [Description("Service collection to configure")] IServiceCollection services)
       {
           // Configure object pooling, caching strategies, etc.
       }
   }
   ```

#### **Day 5: Demo Implementation**

1. **Enhanced Demo Setup with CSV Data**
   ```csharp
   // Demo/DemoServiceCollection.cs
   [Description("Configures services for demonstration purposes")]
   public static class DemoServiceCollection
   {
       [Description("Adds demo ERP services with embedded test data")]
       public static IServiceCollection AddSivarErpDemo(
           [Description("Service collection")] this IServiceCollection services,
           [Description("Test data set to use")] string testDataSet = "ElSalvador")
       {
           services.AddSivarErpCore();
           services.AddScoped<IDataImportService, DataImportService>();
           services.AddScoped<ISampleDataGenerator>(provider => 
               new SampleDataGenerator(provider, testDataSet));
           return services;
       }
   }
   
   // Demo/SampleDataGenerator.cs
   [Description("Generates sample data from embedded CSV resources")]
   public class SampleDataGenerator : ISampleDataGenerator
   {
       [Description("Populates repository with test data")]
       public async Task GenerateSampleDataAsync(IRepository repository)
       {
           var dataImportService = _serviceProvider.GetRequiredService<IDataImportService>();
           await dataImportService.ImportTestDataAsync(repository, _testDataSet);
       }
   }
   ```

### **Phase 4: Test Migration and Validation (Week 4)**

#### **Day 1-3: CompleteAccountingWorkflowTest Migration**

1. **Test Migration with Enhanced CSV Support**
   ```csharp
   // Tests/CompleteAccountingWorkflowTest.cs (NEW VERSION)
   [TestFixture]
   [Description("Comprehensive test of the complete accounting workflow using new architecture")]
   public class CompleteAccountingWorkflowTest
   {
       private IServiceProvider _serviceProvider = null!;
       private IRepository _repository = null!;
       private IAccountingService _accountingService = null!;
       private IDataImportService _dataImportService = null!;
       
       [SetUp]
       [Description("Sets up test environment with dependency injection")]
       public void Setup()
       {
           var services = new ServiceCollection();
           services.AddSivarErpCore();
           services.AddSivarErpDemo("ElSalvador"); // Use El Salvador test data
           _serviceProvider = services.BuildServiceProvider();
           
           _repository = _serviceProvider.GetRequiredService<IRepository>();
           _accountingService = _serviceProvider.GetRequiredService<IAccountingService>();
           _dataImportService = _serviceProvider.GetRequiredService<IDataImportService>();
       }
       
       [Test]
       [Description("Executes the complete accounting workflow to verify MVP functionality")]
       public async Task ExecuteCompleteWorkflowTest()
       {
           // Step 1: Import test data using new service
           var importResult = await _dataImportService.ImportTestDataAsync(_repository, "ElSalvador");
           Assert.That(importResult.IsSuccessful, Is.True, "Test data import should succeed");
           
           // Verify imported data counts match expectations
           Assert.That(_repository.GetObjects<IAccount>().Count(), Is.GreaterThan(10));
           Assert.That(_repository.GetObjects<ITax>().Count(), Is.GreaterThan(2));
           Assert.That(_repository.GetObjects<IBusinessEntity>().Count(), Is.GreaterThan(2));
           
           // Continue with existing workflow tests...
           // All existing test logic should work with repository pattern
       }
   }
   ```

#### **Day 4-5: Integration Testing**

1. **CSV Import Integration Tests**
   ```csharp
   // Tests/Integration/CsvImportIntegrationTests.cs
   [TestFixture]
   [Description("Tests CSV import functionality with repository pattern")]
   public class CsvImportIntegrationTests
   {
       [Test]
       [Description("Tests complete data import cycle")]
       public async Task ImportTestData_ShouldPopulateRepository()
       {
           // Setup
           var services = new ServiceCollection();
           services.AddSivarErpCore();
           services.AddSivarErpDemo();
           var serviceProvider = services.BuildServiceProvider();
           
           var repository = serviceProvider.GetRequiredService<IRepository>();
           var dataImportService = serviceProvider.GetRequiredService<IDataImportService>();
           
           // Act
           var result = await dataImportService.ImportTestDataAsync(repository, "ElSalvador");
           
           // Assert
           Assert.That(result.IsSuccessful, Is.True);
           Assert.That(result.Errors, Is.Empty);
           
           // Verify all expected entities were imported
           Assert.That(repository.GetObjects<IAccount>().Count(), Is.GreaterThan(50));
           Assert.That(repository.GetObjects<ITaxGroup>().Count(), Is.EqualTo(10));
           Assert.That(repository.GetObjects<ITax>().Count(), Is.GreaterThan(5));
           
           // Verify relationships are properly established
           var taxableItems = repository.FindObject<ITaxGroup>(tg => tg.Code == "TAXABLE_ITEMS");
           Assert.That(taxableItems, Is.Not.Null);
       }
   }
   ```

## ?? **Migration Deliverables**

### **Core Components**

#### **1. Enhanced Project Structure**
```
Sivar.ERP.Core/
??? Sivar.ERP.Core.csproj              # Main library
??? Core/
?   ??? IEntity.cs                     # Base entity interface
?   ??? IRepository.cs                 # Repository pattern
?   ??? ServiceCollectionExtensions.cs # DI configuration
??? Modules/
?   ??? Accounting/
?   ?   ??? IAccountingService.cs
?   ?   ??? AccountingService.cs
?   ?   ??? Models/
?   ??? DataImport/                    # **ENHANCED** CSV Import System
?   ?   ??? IDataImportService.cs
?   ?   ??? ICsvImportService.cs
?   ?   ??? DataImportService.cs
?   ?   ??? CsvImportService.cs
?   ?   ??? Importers/
?   ?       ??? IEntityImporter.cs
?   ?       ??? AccountImporter.cs
?   ?       ??? TaxImporter.cs
?   ?       ??? BusinessEntityImporter.cs
?   ?       ??? ItemImporter.cs
?   ??? Documents/
?   ??? Taxes/
?   ??? Payments/
?   ??? Security/
?   ??? Inventory/
??? Infrastructure/
?   ??? Data/
?   ??? Localization/
?   ??? Performance/
?   ??? AI/
??? Demo/
    ??? DemoServiceCollection.cs
    ??? SampleDataGenerator.cs
    ??? TestDataResourceManager.cs
    ??? TestData/                      # **NEW** Embedded CSV resources
        ??? ElSalvador/
            ??? ComercialChartOfAccounts.txt
            ??? ElSalvadorTaxGroups.txt
            ??? ElSalvadorTaxes.txt
            ??? ElSalvadorTaxRules.txt
            ??? BusinesEntities.txt
            ??? Items.txt
            ??? TestAccountMappings.csv
            ??? TaxAccountingProfiles.csv
            ??? DocumentAccountingProfiles.csv

Sivar.ERP.Core.Tests/
??? Sivar.ERP.Core.Tests.csproj
??? Unit/                              # Unit tests
??? Integration/                       # Integration tests
?   ??? CsvImportIntegrationTests.cs  # **NEW**
?   ??? DataImportIntegrationTests.cs # **NEW**
??? CompleteAccountingWorkflowTest.cs  # MVP validation test
```

#### **2. Key Interfaces**

**Enhanced Repository Pattern:**
```csharp
public interface IRepository : IDisposable
{
    T CreateObject<T>() where T : class, new();
    T? GetObjectByKey<T>(object key) where T : class;
    IQueryable<T> GetObjects<T>() where T : class;
    T? FindObject<T>(Expression<Func<T, bool>> criteria) where T : class;
    Task CommitChanges();
    void Rollback();
    bool IsModified { get; }
}
```

**CSV Import System:**
```csharp
public interface IDataImportService
public interface ICsvImportService  
public interface IEntityImporter<T>
// + Specialized importers for each entity type
```

**Service Interfaces:**
```csharp
public interface IAccountingService
public interface IDocumentService  
public interface ITaxService
public interface IPaymentService
public interface IInventoryService
public interface ISecurityService
```

#### **3. Enhanced Demo Implementation**
```csharp
// Complete in-memory demo with embedded CSV test data
services.AddSivarErpDemo("ElSalvador");

// Auto-imports comprehensive test data
var demo = serviceProvider.GetRequiredService<ISampleDataGenerator>();
await demo.GenerateSampleDataAsync(repository);

// Supports multiple test data sets for different countries/scenarios
services.AddSivarErpDemo("USA");      // Future expansion
services.AddSivarErpDemo("Mexico");   // Future expansion
```

## ?? **Enhanced Success Criteria**

### **Functional Requirements**
1. ? **CompleteAccountingWorkflowTest passes 100%** - Non-negotiable MVP validation
2. ? **All existing CSV import functionality preserved** - Critical for test data setup
3. ? **Enhanced data import with validation** - Better error handling and logging
4. ? **Embedded test resources** - No external file dependencies for demos
5. ? **Performance improved** - Lower memory usage, faster data loading
6. ? **Dependency injection throughout** - No hard dependencies or direct instantiation

### **CSV Import System Requirements**
1. ? **All current CSV formats supported** - Backward compatibility maintained
2. ? **Enhanced validation and error reporting** - Better debugging capabilities
3. ? **Repository pattern integration** - No ObjectDb dependencies
4. ? **Embedded test data resources** - Self-contained demo capability
5. ? **Extensible import architecture** - Easy to add new entity types
6. ? **Atomic import transactions** - All-or-nothing import reliability

### **Architectural Requirements**
1. ? **SOLID principles implemented** - Clear separation of concerns
2. ? **Interface-based design** - All services behind interfaces
3. ? **Repository pattern working** - ORM-independent data access
4. ? **Modular architecture** - Independent, testable modules
5. ? **Enhanced CSV import system** - Modern, extensible, well-tested

### **Quality Requirements**
1. ? **100% test coverage** for core services and CSV import
2. ? **Comprehensive documentation** - System.ComponentModel.Description attributes
3. ? **Localization ready** - Multi-language support infrastructure
4. ? **AI integration ready** - Exposed operations for AI agents
5. ? **CSV import testing** - Comprehensive integration tests

### **Performance Requirements**
1. ? **Memory usage optimized** - Configurable for low-memory scenarios
2. ? **Fast data loading** - Efficient repository operations with bulk import
3. ? **Performance monitoring** - Built-in metrics and diagnostics
4. ? **Efficient CSV processing** - Streaming import for large files

## ?? **Risk Mitigation**

### **MVP Validation Risk**
- **Risk**: CompleteAccountingWorkflowTest fails after migration
- **Mitigation**: Test-driven migration - test must pass at each phase
- **Fallback**: Incremental migration with backward compatibility

### **CSV Import Compatibility Risk**
- **Risk**: Existing CSV files incompatible with new import system
- **Mitigation**: Maintain exact compatibility with current file formats
- **Validation**: Comprehensive import tests for all CSV file types

### **Performance Risk**
- **Risk**: New architecture slower than current implementation
- **Mitigation**: Performance benchmarking at each phase
- **Target**: Equal or better performance than current system

### **Test Data Dependency Risk**
- **Risk**: Loss of external CSV test files breaks testing
- **Mitigation**: Embed all test CSV files as resources in the assembly
- **Benefit**: Self-contained demo that works anywhere

## ?? **Implementation Checklist**

### **Phase 1: Core Infrastructure**
- [ ] Create Sivar.ERP.Core project structure
- [ ] Implement core interfaces (IEntity, IRepository)
- [ ] Create dependency injection extensions
- [ ] Implement in-memory repository
- [ ] Embed existing CSV test files as resources
- [ ] Create enhanced CSV import service interfaces
- [ ] Unit tests for core components

### **Phase 2: Service Migration**
- [ ] Migrate AccountingService with DI
- [ ] Migrate DocumentService with DI
- [ ] Migrate TaxService with DI
- [ ] Migrate PaymentService with DI
- [ ] **Implement enhanced DataImportService with repository pattern**
- [ ] **Create specialized entity importers**
- [ ] **Create CSV import integration tests**
- [ ] Integration tests for services

### **Phase 3: Advanced Features**
- [ ] Implement localization infrastructure
- [ ] Add AI integration support
- [ ] Performance monitoring implementation
- [ ] Memory optimization utilities
- [ ] **Enhanced demo service collection with embedded test data**
- [ ] **Multi-country test data support infrastructure**

### **Phase 4: Validation**
- [ ] Migrate CompleteAccountingWorkflowTest
- [ ] **Verify all CSV import functionality works**
- [ ] **Test embedded resource loading**
- [ ] Verify test passes 100%
- [ ] Performance benchmarking
- [ ] Documentation completion
- [ ] Final integration testing

## ?? **Migration Timeline**

**Week 1**: Core Infrastructure, Repository Pattern, and CSV Import Foundation  
**Week 2**: Service Migration and Enhanced Data Import Implementation  
**Week 3**: Advanced Features, Embedded Resources, and Multi-Data Set Support  
**Week 4**: Test Migration, CSV Import Validation, and Final Testing  

**Total Duration**: 4 weeks  
**Validation Gate**: CompleteAccountingWorkflowTest must pass 100% with all CSV import functionality preserved

## ?? **CSV Import Enhancement Benefits**

### **Immediate Benefits**
- **Self-Contained Testing**: No external file dependencies
- **Better Error Handling**: Comprehensive validation and logging
- **Repository Integration**: Modern data persistence patterns
- **Atomic Operations**: All-or-nothing import reliability

### **Long-Term Benefits**
- **Multi-Country Support**: Easy to add new localized test data sets
- **Extensible Architecture**: Simple to add new entity importers
- **Production Ready**: Robust import system suitable for real deployments
- **AI Integration**: CSV import operations can be exposed to AI agents

This enhanced migration plan ensures that the critical CSV import functionality that enables your comprehensive test suite is not only preserved but significantly improved with modern architectural patterns, better error handling, and enhanced testability.