# XAF Import/Export Services

This directory contains XAF-specific implementations of import/export services that use DevExpress XAF's `IObjectSpace` for data persistence.

## XafAccountImportExportService

### Overview
The `XafAccountImportExportService` is an XAF implementation of the `IAccountImportExportService` interface that provides CSV import/export functionality for chart of accounts using XAF's ObjectSpace pattern.

### Key Features
- **XAF Integration**: Uses `IObjectSpace` for data operations instead of in-memory collections
- **Entity Framework Entities**: Works with `Sivar.Erp.EfCore.Entities.Account` entities
- **Validation**: Reuses existing `AccountValidator` logic for business rule validation
- **Audit Trail**: Automatically sets audit fields (`InsertedBy`, `InsertedAt`, `UpdatedBy`, `UpdatedAt`)
- **Duplicate Detection**: Checks for existing accounts by `OfficialCode` before import
- **Transaction Management**: Uses XAF's built-in transaction scope via `ObjectSpace.CommitChanges()`

### Usage

#### Constructor Injection
```csharp
// Basic usage with ObjectSpace
var service = new XafAccountImportExportService(objectSpace);

// With custom validator and logging
var service = new XafAccountImportExportService(objectSpace, customValidator, logger);
```

#### Import Accounts from CSV
```csharp
string csvContent = "AccountName,OfficialCode,AccountType,ParentOfficialCode,BalanceAndIncomeLineId\n" +
                   "Cash,1001,Asset,,\n" +
                   "Accounts Receivable,1201,Asset,,";

var (importedAccounts, errors) = await service.ImportFromCsvAsync(csvContent, "admin");

if (errors.Any())
{
    foreach (var error in errors)
    {
        Console.WriteLine($"Import Error: {error}");
    }
}
```

#### Export Accounts to CSV
```csharp
// Export all accounts
string csvContent = await service.ExportToCsvAsync(null);

// Export specific accounts
var accounts = objectSpace.GetObjects<Account>().Where(a => a.AccountType == AccountType.Asset);
string csvContent = await service.ExportToCsvAsync(accounts);
```

### CSV Format

The service expects/generates CSV files with the following columns:
- `AccountName` (required): Name of the account
- `OfficialCode` (required): Unique identifier for the account
- `AccountType` (required): Type of account (Asset, Liability, Equity, Revenue, Expense, Settlement)
- `ParentOfficialCode` (optional): Official code of parent account for hierarchical accounts
- `BalanceAndIncomeLineId` (optional): GUID reference to financial statement line

### Differences from POCO Implementation

1. **Data Access**: Uses XAF `IObjectSpace` instead of in-memory collections
2. **Entity Type**: Creates `Sivar.Erp.EfCore.Entities.Account` from validated `AccountDto`
3. **Persistence**: Uses `objectSpace.CommitChanges()` for database persistence
4. **Duplicate Detection**: Queries database for existing accounts by `OfficialCode` before creating entities
5. **Audit Fields**: Automatically sets XAF audit trail fields
6. **Validation Flow**: 
   - Creates `AccountDto` from CSV fields (same as POCO)
   - Validates the DTO using existing `AccountValidator`
   - Checks for duplicates in database
   - Only then creates XAF entity from validated DTO

### Import Process Flow

The service follows this logical flow to ensure data integrity:

1. **Parse CSV**: Extract fields from CSV content
2. **Create DTO**: Create `AccountDto` from CSV fields (validation-ready format)
3. **Validate**: Use `AccountValidator` to check business rules
4. **Check Duplicates**: Query database for existing accounts with same `OfficialCode`
5. **Create Entity**: Only if validation passes and no duplicates exist, create XAF entity
6. **Commit**: Save all valid entities to database in a single transaction

### Error Handling

The service provides comprehensive error handling:
- CSV format validation (empty content, missing headers, column count mismatches)
- Business rule validation using `AccountValidator`
- Duplicate account detection
- Exception handling with detailed error messages
- Logging support for diagnostic information

### Logging

The service supports optional logging through `ILogger<XafAccountImportExportService>`:
- Import/export operation start/completion
- Success metrics (number of records processed)
- Error details and warnings
- Performance information

### Dependencies

- `DevExpress.ExpressApp` (for IObjectSpace)
- `Sivar.Erp.EfCore.Entities` (for Account entity)
- `Sivar.Erp.Modules.Accounting.ChartOfAccounts` (for interfaces and validator)
- `Sivar.Erp.Modules.ImportExport` (for service interface)
- `Microsoft.Extensions.Logging` (optional, for diagnostics)

### Integration with XAF Applications

Register the service in your XAF application's dependency injection container:

```csharp
// In your XAF module or startup configuration
services.AddScoped<IAccountImportExportService>(provider =>
{
    var objectSpace = provider.GetRequiredService<IObjectSpace>();
    var logger = provider.GetService<ILogger<XafAccountImportExportService>>();
    return new XafAccountImportExportService(objectSpace, logger);
});
```

### Best Practices

1. **ObjectSpace Lifecycle**: Ensure the ObjectSpace is properly managed by XAF's dependency injection
2. **Transaction Scope**: Let XAF handle transaction boundaries through ObjectSpace
3. **Error Handling**: Always check the errors collection from import operations
4. **Validation**: Rely on the existing `AccountValidator` for business rule validation
5. **Logging**: Use structured logging for better diagnostic capabilities
6. **Performance**: For large imports, consider batch processing strategies
