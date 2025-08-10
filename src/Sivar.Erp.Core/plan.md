# Sivar ERP Core - Step-by-Step Implementation Plan

## 📋 Overview
This implementation plan breaks down the refactor from the legacy system `Sivar.ERP` and its test `Test` to the new `Sivar.ERP.Core` and test `Sivar.Erp.Core.Tests` architecture into manageable daily tasks. The plan prioritizes **CSV import functionality preservation** as it's critical for test data setup and the `CompleteAccountingWorkflowTest`.

## 🎯 Success Metrics
- ✅ **CompleteAccountingWorkflowTest passes 100%** 
- ✅ **All CSV import functionality preserved**
- ✅ **Enhanced error handling and validation**
- ✅ **Embedded test data resources (no external file dependencies)**
- ✅ **Modern architecture with dependency injection**


## 🎯 REMEMBER
- ✅ **DOUBLE CHECK IF A CLASS, ENUM , INTERFACE OR SERVICE ALREADY EXIST BEFORE YOU CREATE IT** 
- ✅ **ONCE IS TIME TO IMPLEMENT THE LOGIC OF THE SERVICE YOU CAN TAKE A LOOK TO THE Sivar.Erp project or the test project** 
---

## 🚀 Phase 1: Core Infrastructure (Week 1)

### **Day 1: Project Setup & Core Abstractions**

#### Morning Tasks (2-3 hours)
1. **Create project structure**
   ```bash
   # Create new solution and projects
   dotnet new sln -n Sivar.ERP
   dotnet new classlib -n Sivar.ERP.Core -f net9.0
   dotnet new xunit -n Sivar.ERP.Core.Tests -f net9.0
   dotnet sln add Sivar.ERP.Core Sivar.ERP.Core.Tests
   ```

2. **Setup project references and packages**
   ```xml
   <!-- Sivar.ERP.Core.csproj -->
   <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="9.0.0" />
   <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="9.0.0" />
   <PackageReference Include="System.ComponentModel.Annotations" Version="5.0.0" />
   ```

3. **Create folder structure**
   ```
   Sivar.ERP.Core/
   ├── Core/
   ├── Modules/
   │   ├── Accounting/
   │   ├── DataImport/
   │   ├── Documents/
   │   ├── Taxes/
   │   ├── Payments/
   │   └── Inventory/
   ├── Infrastructure/Data/
   └── Demo/TestData/ElSalvador/
   ```

#### Afternoon Tasks (3-4 hours)
4. **Implement core interfaces**
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
   
   // Core/IRepository.cs
   [Description("Generic repository interface for data access operations")]
   public interface IRepository : IDisposable
   {
       [Description("Creates a new object of the specified type")]
       T CreateObject<T>() where T : class, new();
       
       [Description("Retrieves an object by its primary key")]
       T? GetObjectByKey<T>(object key) where T : class;
       
       [Description("Gets all objects of the specified type")]
       IQueryable<T> GetObjects<T>() where T : class;
       
       [Description("Finds first object matching criteria")]
       T? FindObject<T>(Expression<Func<T, bool>> criteria) where T : class;
       
       [Description("Commits all pending changes")]
       Task CommitChanges();
       
       [Description("Rollback all pending changes")]
       void Rollback();
       
       [Description("Check if repository has unsaved changes")]
       bool IsModified { get; }
   }
   ```

**✅ Day 1 Deliverables:**
- New project structure created
- Core interfaces defined
- Basic folder structure in place

---

### **Day 2: Embed CSV Test Data & Resource Management**

#### Morning Tasks (3-4 hours)
1. **Embed CSV files as resources**
   - Copy all CSV files from your documents to `Demo/TestData/ElSalvador/`
   - Update `.csproj` to include as embedded resources:
   ```xml
   <ItemGroup>
     <EmbeddedResource Include="Demo\TestData\ElSalvador\*.csv" />
     <EmbeddedResource Include="Demo\TestData\ElSalvador\*.txt" />
   </ItemGroup>
   ```

2. **Create TestDataResourceManager**
   ```csharp
   // Demo/TestDataResourceManager.cs
   [Description("Manages embedded test data resources")]
   public class TestDataResourceManager
   {
       [Description("Loads CSV content from embedded resources")]
       public static async Task<string> LoadCsvAsync(string fileName)
       {
           var assembly = Assembly.GetExecutingAssembly();
           var resourceName = $"Sivar.ERP.Core.Demo.TestData.ElSalvador.{fileName}";
           
           using var stream = assembly.GetManifestResourceStream(resourceName);
           if (stream == null)
               throw new FileNotFoundException($"Embedded resource not found: {resourceName}");
               
           using var reader = new StreamReader(stream);
           return await reader.ReadToEndAsync();
       }
       
       [Description("Lists all available CSV files")]
       public static IEnumerable<string> GetAvailableCsvFiles()
       {
           var assembly = Assembly.GetExecutingAssembly();
           return assembly.GetManifestResourceNames()
               .Where(name => name.Contains("TestData.ElSalvador"))
               .Select(name => Path.GetFileName(name));
       }
   }
   ```

#### Afternoon Tasks (2-3 hours)
3. **Create basic CSV import infrastructure**
   ```csharp
   // Modules/DataImport/ICsvImportService.cs
   [Description("Generic service for CSV data import operations")]
   public interface ICsvImportService
   {
       [Description("Imports entities from CSV content with validation")]
       Task<CsvImportResult<T>> ImportFromCsvAsync<T>(
           string csvContent,
           string userName,
           CsvImportOptions<T>? options = null) where T : class;
   }
   
   // Modules/DataImport/Models/CsvImportResult.cs
   public class CsvImportResult<T>
   {
       public bool Success { get; set; }
       public List<T> ImportedEntities { get; set; } = new();
       public List<string> Errors { get; set; } = new();
       public int TotalProcessed { get; set; }
       public TimeSpan Duration { get; set; }
   }
   ```

**✅ Day 2 Deliverables:**
- All CSV test data embedded as resources
- TestDataResourceManager implemented
- Basic CSV import interfaces defined

---

### **Day 3: In-Memory Repository Implementation**

#### Full Day Task (6-8 hours)
1. **Implement InMemoryRepository**
   ```csharp
   // Infrastructure/Data/InMemoryRepository.cs
   [Description("In-memory implementation of the repository pattern for demos and testing")]
   public class InMemoryRepository : IRepository
   {
       private readonly ConcurrentDictionary<Type, IList> _collections = new();
       private readonly HashSet<object> _newObjects = new();
       private readonly HashSet<object> _modifiedObjects = new();
       private readonly object _lock = new();
       
       public T CreateObject<T>() where T : class, new()
       {
           lock (_lock)
           {
               var obj = new T();
               _newObjects.Add(obj);
               GetCollection<T>().Add(obj);
               return obj;
           }
       }
       
       public T? GetObjectByKey<T>(object key) where T : class
       {
           // Implementation for key-based retrieval
           // Assume entities have an Id property
       }
       
       public IQueryable<T> GetObjects<T>() where T : class
       {
           return GetCollection<T>().AsQueryable();
       }
       
       public T? FindObject<T>(Expression<Func<T, bool>> criteria) where T : class
       {
           return GetObjects<T>().FirstOrDefault(criteria);
       }
       
       public async Task CommitChanges()
       {
           // Simulate async commit
           await Task.Delay(1);
           _newObjects.Clear();
           _modifiedObjects.Clear();
       }
       
       public void Rollback()
       {
           // Remove new objects, restore modified ones
           foreach (var newObj in _newObjects)
           {
               // Remove from collections
           }
           _newObjects.Clear();
           _modifiedObjects.Clear();
       }
       
       public bool IsModified => _newObjects.Count > 0 || _modifiedObjects.Count > 0;
       
       private IList<T> GetCollection<T>() where T : class
       {
           return (IList<T>)_collections.GetOrAdd(typeof(T), _ => new List<T>());
       }
       
       public void Dispose()
       {
           _collections.Clear();
           _newObjects.Clear();
           _modifiedObjects.Clear();
       }
   }
   ```

2. **Create unit tests for repository**
   ```csharp
   // Tests/Unit/InMemoryRepositoryTests.cs
   public class InMemoryRepositoryTests
   {
       [Fact]
       public void CreateObject_ShouldCreateAndTrackNewObject()
       {
           // Test implementation
       }
       
       [Fact]
       public async Task CommitChanges_ShouldClearTrackedObjects()
       {
           // Test implementation
       }
       
       // More comprehensive tests...
   }
   ```

**✅ Day 3 Deliverables:**
- Fully functional InMemoryRepository
- Comprehensive unit tests
- Thread-safe implementation

---

### **Day 4: Service Interfaces & DI Configuration**

#### Morning Tasks (3-4 hours)
1. **Define core service interfaces**
   ```csharp
   // Modules/Accounting/IAccountingService.cs
   [Description("Service for managing accounting operations")]
   public interface IAccountingService
   {
       [Description("Creates a transaction from a document")]
       Task<ITransaction> CreateTransactionAsync(IDocument document, string? description = null);
       
       [Description("Posts a transaction to the ledger")]
       Task PostTransactionAsync(ITransaction transaction);
       
       [Description("Calculates account balances")]
       Task<decimal> CalculateAccountBalanceAsync(string accountCode, DateTime? asOfDate = null);
   }
   
   // Modules/Documents/IDocumentService.cs
   [Description("Service for document processing and management")]
   public interface IDocumentService
   {
       [Description("Creates a new document")]
       Task<IDocument> CreateDocumentAsync(IDocumentType documentType, IBusinessEntity businessEntity);
       
       [Description("Calculates taxes for a document")]
       Task CalculateDocumentTaxesAsync(IDocument document, string documentOperation);
       
       [Description("Validates document before processing")]
       Task<ValidationResult> ValidateDocumentAsync(IDocument document);
   }
   
   // Modules/Taxes/ITaxService.cs
   [Description("Service for tax calculations and management")]
   public interface ITaxService
   {
       [Description("Calculates applicable taxes for a document")]
       Task<IEnumerable<ITax>> GetApplicableTaxesAsync(IDocument document, DocumentOperation operation);
       
       [Description("Applies tax rules to document lines")]
       Task ApplyTaxRulesToDocumentAsync(IDocument document);
   }
   ```

#### Afternoon Tasks (3-4 hours)
2. **Create DI configuration**
   ```csharp
   // Core/ServiceCollectionExtensions.cs
   [Description("Extension methods for configuring ERP services")]
   public static class ServiceCollectionExtensions
   {
       [Description("Adds core ERP services to the DI container")]
       public static IServiceCollection AddSivarErpCore(
           this IServiceCollection services,
           ErpCoreOptions? options = null)
       {
           // Register core services
           services.AddScoped<IRepository, InMemoryRepository>();
           services.AddScoped<IAccountingService, AccountingService>();
           services.AddScoped<IDocumentService, DocumentService>();
           services.AddScoped<ITaxService, TaxService>();
           services.AddScoped<IDataImportService, DataImportService>();
           services.AddScoped<ICsvImportService, CsvImportService>();
           
           // Register logging
           services.AddLogging();
           
           return services;
       }
       
       [Description("Adds demo services with embedded test data")]
       public static IServiceCollection AddSivarErpDemo(
           this IServiceCollection services,
           string testDataSet = "ElSalvador")
       {
           services.AddSivarErpCore();
           services.AddScoped<ISampleDataGenerator, SampleDataGenerator>();
           services.Configure<DemoOptions>(options => 
           {
               options.TestDataSet = testDataSet;
           });
           
           return services;
       }
   }
   ```

**✅ Day 4 Deliverables:**
- All core service interfaces defined
- DI configuration implemented
- Demo service registration ready

---

### **Day 5: Enhanced CSV Import System**

#### Morning Tasks (4-5 hours)
1. **Implement CsvImportService**
   ```csharp
   // Modules/DataImport/CsvImportService.cs
   [Description("Implementation of CSV import functionality")]
   public class CsvImportService : ICsvImportService
   {
       private readonly ILogger<CsvImportService> _logger;
       private readonly IRepository _repository;
       
       public async Task<CsvImportResult<T>> ImportFromCsvAsync<T>(
           string csvContent,
           string userName,
           CsvImportOptions<T>? options = null) where T : class
       {
           var stopwatch = Stopwatch.StartNew();
           var result = new CsvImportResult<T>();
           
           try
           {
               // Parse CSV content
               var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
               var headers = ParseHeaders(lines[0]);
               
               // Process each data line
               for (int i = 1; i < lines.Length; i++)
               {
                   try
                   {
                       var entity = await ProcessLineAsync<T>(lines[i], headers, options);
                       if (entity != null)
                       {
                           result.ImportedEntities.Add(entity);
                       }
                   }
                   catch (Exception ex)
                   {
                       result.Errors.Add($"Line {i + 1}: {ex.Message}");
                   }
               }
               
               result.TotalProcessed = lines.Length - 1;
               result.Success = result.Errors.Count == 0;
               
               if (result.Success)
               {
                   await _repository.CommitChanges();
               }
               else
               {
                   _repository.Rollback();
               }
           }
           catch (Exception ex)
           {
               _logger.LogError(ex, "Failed to import CSV data");
               result.Errors.Add($"Import failed: {ex.Message}");
               result.Success = false;
           }
           finally
           {
               result.Duration = stopwatch.Elapsed;
           }
           
           return result;
       }
   }
   ```

#### Afternoon Tasks (2-3 hours)
2. **Create specialized entity importers**
   ```csharp
   // Modules/DataImport/Importers/IEntityImporter.cs
   [Description("Base interface for entity-specific importers")]
   public interface IEntityImporter<T> where T : class
   {
       [Description("Imports entities with business logic validation")]
       Task<EntityImportResult<T>> ImportAsync(
           IRepository repository,
           string csvContent,
           string userName);
   }
   
   // Modules/DataImport/Importers/AccountImporter.cs
   [Description("Specialized importer for chart of accounts")]
   public class AccountImporter : IEntityImporter<IAccount>
   {
       public async Task<EntityImportResult<IAccount>> ImportAsync(
           IRepository repository,
           string csvContent,
           string userName)
       {
           // Implement account-specific import logic
           // Validate account codes, hierarchies, etc.
       }
   }
   ```

**✅ Day 5 Deliverables:**
- Functional CSV import service
- Specialized entity importers framework
- Error handling and validation

---

## 🛠️ Phase 2: Service Migration (Week 2)

### **Day 6-7: Accounting Service Implementation**

#### Tasks
1. **Implement AccountingService**
   ```csharp
   // Modules/Accounting/AccountingService.cs
   [Description("Core accounting service implementing business logic")]
   public class AccountingService : IAccountingService
   {
       private readonly IRepository _repository;
       private readonly ILogger<AccountingService> _logger;
       
       public async Task<ITransaction> CreateTransactionAsync(IDocument document, string? description = null)
       {
           _logger.LogInformation("Creating transaction from document {DocumentNumber}", document.DocumentNumber);
           
           var transaction = _repository.CreateObject<TransactionDto>();
           transaction.DocumentNumber = document.DocumentNumber;
           transaction.Description = description ?? $"Transaction for {document.DocumentNumber}";
           
           // Apply accounting profiles logic
           await ApplyAccountingProfiles(transaction, document);
           
           return transaction;
       }
       
       private async Task ApplyAccountingProfiles(ITransaction transaction, IDocument document)
       {
           // Implementation of accounting profile logic
           // This preserves the existing accounting logic
       }
   }
   ```

2. **Create comprehensive unit tests**
3. **Implement model DTOs that match existing interfaces**

**✅ Day 6-7 Deliverables:**
- Fully functional AccountingService
- Unit tests with high coverage
- Integration with repository pattern

---

### **Day 8-9: Document & Tax Services**

#### Tasks
1. **Implement DocumentService**
2. **Implement TaxService**
3. **Create integration tests**
4. **Ensure CSV import compatibility**

**✅ Day 8-9 Deliverables:**
- DocumentService and TaxService implementations
- Tax calculation logic preserved
- Document processing workflows functional

---

### **Day 10: Data Import Service Integration**

#### Tasks
1. **Implement enhanced DataImportService**
2. **Test embedded resource loading**
3. **Create CSV import integration tests**
4. **Validate against existing CSV files**

**✅ Day 10 Deliverables:**
- Enhanced DataImportService
- All CSV imports working
- Integration tests passing

---

## 🎨 Phase 3: Advanced Features (Week 3)

### **Day 11-12: Demo Service Collection**

#### Tasks
1. **Implement SampleDataGenerator**
   ```csharp
   // Demo/SampleDataGenerator.cs
   [Description("Generates comprehensive sample data for demos")]
   public class SampleDataGenerator : ISampleDataGenerator
   {
       public async Task GenerateSampleDataAsync(IRepository repository)
       {
           // Load and import all embedded CSV files
           await ImportChartOfAccountsAsync(repository);
           await ImportTaxDataAsync(repository);
           await ImportBusinessEntitiesAsync(repository);
           await ImportItemsAsync(repository);
           // ... etc
       }
   }
   ```

2. **Test multi-data set support**
3. **Performance optimization**

---

### **Day 13-15: Infrastructure Features**

#### Tasks
1. **Localization infrastructure**
2. **Performance monitoring**
3. **AI integration support**
4. **Memory optimization**

---

## ✅ Phase 4: Validation (Week 4)

### **Day 16-17: Test Migration**

#### Tasks
1. **Migrate CompleteAccountingWorkflowTest**
2. **Verify CSV import functionality**
3. **Test embedded resource loading**

---

### **Day 18-20: Final Validation**

#### Tasks
1. **Performance benchmarking**
2. **Documentation completion**
3. **Final integration testing**
4. **Ensure 100% test pass rate**

---

## 📊 Progress Tracking

### **Week 1 Checklist**
- [ ] Day 1: Project setup & core abstractions
- [ ] Day 2: Embed CSV data & resource management
- [ ] Day 3: In-memory repository implementation
- [ ] Day 4: Service interfaces & DI configuration
- [ ] Day 5: Enhanced CSV import system

### **Week 2 Checklist**
- [ ] Day 6-7: Accounting service implementation
- [ ] Day 8-9: Document & tax services
- [ ] Day 10: Data import service integration

### **Week 3 Checklist**
- [ ] Day 11-12: Demo service collection
- [ ] Day 13-15: Infrastructure features

### **Week 4 Checklist**
- [ ] Day 16-17: Test migration
- [ ] Day 18-20: Final validation

---

## 🚨 Critical Success Factors

1. **Test-Driven Migration**: Every phase must maintain the `CompleteAccountingWorkflowTest` passing
2. **CSV Import Priority**: Preserve all CSV import functionality from day 1
3. **Embedded Resources**: Ensure self-contained demo capability
4. **Incremental Progress**: Each day should produce working, tested code
5. **Performance Monitoring**: Track performance at each phase

---

## 🎯 Next Steps

**Start with Day 1 immediately:**
1. Create the project structure
2. Implement core interfaces
3. Set up the foundation for CSV import system

This plan ensures a systematic, low-risk migration that preserves all critical functionality while building a modern, maintainable architecture.

Remember to:


1. Performance and Resource Management
Optimize for low memory usage and fast data loading across all components.

Use a data warehouse architecture as the foundation for ERP data organization.


2. Development Standards
Use PascalCase for naming conventions.

Apply System.ComponentModel.Description attributes to classes, methods, and method parameters.

Provide comprehensive documentation for all classes and public members.

3. Testing and Demonstration
Create a simple in-memory demo implementation for testing and demonstration purposes.

Ensure each module has integration tests to validate functionality in isolation and in combination.

