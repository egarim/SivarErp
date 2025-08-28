# XAF Tax Import/Export Service

## Overview

The `XafTaxImportExportService` is a XAF-specific implementation of the `ITaxImportExportService` interface that provides CSV import and export functionality for tax entities using DevExpress XAF's `IObjectSpace` pattern.

## Key Features

- **XAF Integration**: Uses `IObjectSpace` for data persistence and transaction management
- **Validation**: Integrates with `TaxValidator` for business rule validation
- **Error Handling**: Comprehensive error reporting with line-by-line validation
- **Logging**: Optional logging integration for debugging and monitoring
- **Duplicate Detection**: Prevents importing duplicate taxes based on Code
- **CSV Format**: Supports standard CSV format with quoted fields

## Architecture

```
XafTaxImportExportService
├── Uses: IObjectSpace (XAF data access)
├── Validates: TaxDto using TaxValidator
├── Creates: Sivar.Erp.EfCore.Entities.Tax (XAF entities)
└── Returns: TaxDto (for consistency with interface)
```

## Usage Examples

### Basic Import/Export in XAF Controller

```csharp
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using Microsoft.Extensions.Logging;
using Sivar.Erp.Xaf.Module.Services.ImportExport;

public class TaxImportExportController : ObjectViewController
{
    private SimpleAction _importTaxesAction;
    private SimpleAction _exportTaxesAction;

    public TaxImportExportController()
    {
        _importTaxesAction = new SimpleAction(this, "ImportTaxes", "Edit")
        {
            Caption = "Import Taxes",
            ImageName = "Import"
        };
        _importTaxesAction.Execute += ImportTaxesAction_Execute;

        _exportTaxesAction = new SimpleAction(this, "ExportTaxes", "Edit")
        {
            Caption = "Export Taxes",
            ImageName = "Export"
        };
        _exportTaxesAction.Execute += ExportTaxesAction_Execute;
    }

    private async void ImportTaxesAction_Execute(object sender, SimpleActionExecuteEventArgs e)
    {
        try
        {
            // Get CSV content (from file dialog or other source)
            string csvContent = GetCsvContentFromUser();
            
            if (string.IsNullOrEmpty(csvContent)) return;

            // Create service with current ObjectSpace
            var service = new XafTaxImportExportService(ObjectSpace);
            
            // Import taxes
            var (importedTaxes, errors) = await service.ImportFromCsvAsync(csvContent, SecuritySystem.CurrentUserName);

            if (errors.Any())
            {
                // Show errors to user
                string errorMessage = string.Join("\n", errors);
                throw new UserFriendlyException($"Import completed with errors:\n{errorMessage}");
            }

            // Refresh the view
            ObjectSpace.Refresh();
            View.RefreshDataSource();
            
            Application.ShowViewStrategy.ShowMessage($"Successfully imported {importedTaxes.Count()} taxes.");
        }
        catch (Exception ex)
        {
            Application.ShowViewStrategy.ShowMessage($"Import failed: {ex.Message}");
        }
    }

    private async void ExportTaxesAction_Execute(object sender, SimpleActionExecuteEventArgs e)
    {
        try
        {
            var service = new XafTaxImportExportService(ObjectSpace);
            
            // Export all taxes (passing null exports all)
            string csvContent = await service.ExportToCsvAsync(null);
            
            // Save to file or show to user
            SaveCsvToFile(csvContent);
            
            Application.ShowViewStrategy.ShowMessage("Taxes exported successfully.");
        }
        catch (Exception ex)
        {
            Application.ShowViewStrategy.ShowMessage($"Export failed: {ex.Message}");
        }
    }
}
```

### Advanced Usage with Custom Validation

```csharp
// Using custom validator
var customValidator = new TaxValidator();
var service = new XafTaxImportExportService(objectSpace, customValidator, logger);

// Import with detailed error handling
var (taxes, errors) = await service.ImportFromCsvAsync(csvContent, userName);

if (errors.Any())
{
    foreach (var error in errors)
    {
        logger.LogWarning("Tax import error: {Error}", error);
    }
}
```

### Export Specific Taxes

```csharp
// Get specific taxes to export
var specificTaxes = someListOfTaxDtos;

var service = new XafTaxImportExportService(objectSpace);
string csvContent = await service.ExportToCsvAsync(specificTaxes);
```

## CSV Format

### Required Headers
- `Code`: Unique tax code (e.g., "VAT", "GST")
- `Name`: Display name of the tax
- `TaxType`: Enumeration value (Percentage, FixedAmount, AmountPerUnit)
- `ApplicationLevel`: Enumeration value (Line, Document)

### Optional Headers
- `Percentage`: Tax percentage (for TaxType = Percentage)
- `Amount`: Fixed amount (for TaxType = FixedAmount or AmountPerUnit)
- `IsEnabled`: Boolean indicating if tax is active
- `IsIncludedInPrice`: Boolean indicating if tax is included in price

### Example CSV

```csv
Code,Name,TaxType,ApplicationLevel,Percentage,Amount,IsEnabled,IsIncludedInPrice
VAT,Value Added Tax,Percentage,Line,15.0,,true,false
GST,Goods and Services Tax,Percentage,Line,10.0,,true,false
FEE,Processing Fee,FixedAmount,Document,,5.00,true,false
```

## Key Differences from POCO Implementation

| Aspect | POCO Implementation | XAF Implementation |
|--------|-------------------|-------------------|
| **Data Access** | Direct Entity Framework | XAF IObjectSpace |
| **Transaction Management** | Manual DbContext | Automatic via ObjectSpace |
| **Entity Creation** | Direct EF entity instantiation | ObjectSpace.CreateObject<T>() |
| **Duplicate Checking** | EF LINQ queries | CriteriaOperator.Parse() |
| **Return Type** | Works with TaxDto directly | Converts XAF entities to TaxDto |
| **Persistence** | SaveChanges() | CommitChanges() |

## Validation Rules

The service validates taxes using `TaxValidator`:

- **Code**: Required, maximum 20 characters, alphanumeric
- **Name**: Required, maximum 100 characters
- **Percentage**: 0-100% for TaxType.Percentage
- **Amount**: Non-negative for FixedAmount/AmountPerUnit types
- **Duplicates**: Prevents importing taxes with existing codes

## Error Handling

The service provides detailed error reporting:

- **Line Numbers**: Errors include CSV line numbers for easy debugging
- **Validation Errors**: Specific validation failures per tax
- **Format Errors**: Column count mismatches, header validation
- **Duplicate Errors**: Existing tax code conflicts

## Integration Notes

### Project References Required
- Sivar.Erp (for TaxDto, TaxValidator, ITaxImportExportService)
- Sivar.Erp.EfCore (for Tax entity)
- DevExpress.ExpressApp (for IObjectSpace)

### Logging Configuration
The service accepts an optional `ILogger<XafTaxImportExportService>` for diagnostics:

```csharp
// With logging
var logger = serviceProvider.GetService<ILogger<XafTaxImportExportService>>();
var service = new XafTaxImportExportService(objectSpace, logger);
```

## Thread Safety

- **Not Thread-Safe**: Each instance should be used within a single XAF request
- **ObjectSpace Scope**: ObjectSpace should be properly scoped per operation
- **Stateless**: Service contains no state between method calls

## Performance Considerations

- **Batch Processing**: Imports are processed in batches via single CommitChanges()
- **Memory Usage**: Large CSV files are processed line-by-line
- **Database Queries**: Uses efficient CriteriaOperator for duplicate checking
- **Transaction Scope**: Single transaction for entire import operation
