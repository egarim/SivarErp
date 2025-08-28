# XAF Account Import/Export Service Usage Examples

This document provides practical examples of how to use the `XafAccountImportExportService` in XAF applications.

## Setup and Registration

### 1. Service Registration in XAF Module

Add the service registration in your XAF module's `Setup` method or dependency injection configuration:

```csharp
public override void Setup(XafApplication application)
{
    base.Setup(application);
    
    // Register the service in the XAF application's service provider
    // This would typically be done in a dependency injection container
}
```

### 2. Controller Usage Example

Create an XAF controller that uses the service:

```csharp
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using Sivar.Erp.Xaf.Module.Services.ImportExport;

public class AccountImportExportController : ObjectViewController
{
    private SimpleAction importAccountsAction;
    private SimpleAction exportAccountsAction;

    public AccountImportExportController()
    {
        // Initialize import action
        importAccountsAction = new SimpleAction(this, "ImportAccounts", "Tools")
        {
            Caption = "Import Accounts from CSV",
            ToolTip = "Import chart of accounts from CSV file"
        };
        importAccountsAction.Execute += ImportAccountsAction_Execute;

        // Initialize export action
        exportAccountsAction = new SimpleAction(this, "ExportAccounts", "Tools")
        {
            Caption = "Export Accounts to CSV",
            ToolTip = "Export chart of accounts to CSV file"
        };
        exportAccountsAction.Execute += ExportAccountsAction_Execute;
    }

    private async void ImportAccountsAction_Execute(object sender, SimpleActionExecuteEventArgs e)
    {
        try
        {
            // This would typically come from a file upload dialog
            string csvContent = GetCsvContentFromUser();
            
            // Create the service with current ObjectSpace
            var importService = new XafAccountImportExportService(ObjectSpace);
            
            // Import accounts
            var (importedAccounts, errors) = await importService.ImportFromCsvAsync(csvContent, "CurrentUser");
            
            if (errors.Any())
            {
                // Show errors to user
                string errorMessage = string.Join("\\n", errors);
                throw new UserFriendlyException($"Import completed with errors:\\n{errorMessage}");
            }
            else
            {
                // Show success message
                Application.ShowViewStrategy.ShowMessage(
                    $"Successfully imported {importedAccounts.Count()} accounts.", 
                    InformationType.Success);
                    
                // Refresh the view
                View.ObjectSpace.Refresh();
            }
        }
        catch (Exception ex)
        {
            Application.ShowViewStrategy.ShowMessage(
                $"Error during import: {ex.Message}", 
                InformationType.Error);
        }
    }

    private async void ExportAccountsAction_Execute(object sender, SimpleActionExecuteEventArgs e)
    {
        try
        {
            // Create the service with current ObjectSpace
            var exportService = new XafAccountImportExportService(ObjectSpace);
            
            // Export all accounts (pass null to export all)
            string csvContent = await exportService.ExportToCsvAsync(null);
            
            // Save to file or show to user
            SaveCsvContentToFile(csvContent);
            
            Application.ShowViewStrategy.ShowMessage(
                "Accounts exported successfully.", 
                InformationType.Success);
        }
        catch (Exception ex)
        {
            Application.ShowViewStrategy.ShowMessage(
                $"Error during export: {ex.Message}", 
                InformationType.Error);
        }
    }

    private string GetCsvContentFromUser()
    {
        // Implementation would show file upload dialog
        // For example purposes, return sample CSV
        return @"AccountName,OfficialCode,AccountType,ParentOfficialCode,BalanceAndIncomeLineId
Cash,1001,Asset,,
Bank Account,1002,Asset,,
Accounts Receivable,1201,Asset,,
Inventory,1301,Asset,,
Accounts Payable,2001,Liability,,
Retained Earnings,3001,Equity,,
Sales Revenue,4001,Revenue,,
Cost of Goods Sold,5001,Expense,,";
    }

    private void SaveCsvContentToFile(string csvContent)
    {
        // Implementation would show save file dialog
        // For example purposes, just show content length
        Application.ShowViewStrategy.ShowMessage(
            $"CSV content generated ({csvContent.Length} characters)", 
            InformationType.Info);
    }
}
```

## Programmatic Usage Examples

### 1. Basic Import Example

```csharp
public async Task<bool> ImportAccountsFromFile(IObjectSpace objectSpace, string filePath, string userName)
{
    try
    {
        // Read CSV content from file
        string csvContent = await File.ReadAllTextAsync(filePath);
        
        // Create service instance
        var importService = new XafAccountImportExportService(objectSpace);
        
        // Import accounts
        var (importedAccounts, errors) = await importService.ImportFromCsvAsync(csvContent, userName);
        
        // Handle results
        if (errors.Any())
        {
            Console.WriteLine("Import completed with errors:");
            foreach (var error in errors)
            {
                Console.WriteLine($"- {error}");
            }
            return false;
        }
        
        Console.WriteLine($"Successfully imported {importedAccounts.Count()} accounts");
        return true;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Import failed: {ex.Message}");
        return false;
    }
}
```

### 2. Export with Filtering Example

```csharp
public async Task<string> ExportAssetAccounts(IObjectSpace objectSpace)
{
    try
    {
        // Get only Asset accounts
        var assetAccounts = objectSpace.GetObjects<Account>()
            .Where(a => a.AccountType == AccountType.Asset)
            .Cast<IAccount>();
        
        // Create service instance
        var exportService = new XafAccountImportExportService(objectSpace);
        
        // Export filtered accounts
        string csvContent = await exportService.ExportToCsvAsync(assetAccounts);
        
        return csvContent;
    }
    catch (Exception ex)
    {
        throw new InvalidOperationException($"Export failed: {ex.Message}", ex);
    }
}
```

### 3. Batch Import with Validation

```csharp
public async Task<ImportResult> ImportAccountsWithValidation(
    IObjectSpace objectSpace, 
    string csvContent, 
    string userName,
    bool skipDuplicates = true)
{
    var result = new ImportResult();
    
    try
    {
        // Create service with custom validator
        var customValidator = new AccountValidator(AccountValidator.GetElSalvadorAccountTypePrefixes());
        var importService = new XafAccountImportExportService(objectSpace, customValidator);
        
        // Import accounts
        var (importedAccounts, errors) = await importService.ImportFromCsvAsync(csvContent, userName);
        
        result.ImportedCount = importedAccounts.Count();
        result.Errors = errors.ToList();
        result.Success = !errors.Any();
        
        // Log results
        Console.WriteLine($"Import completed:");
        Console.WriteLine($"- Imported: {result.ImportedCount} accounts");
        Console.WriteLine($"- Errors: {result.Errors.Count}");
        
        return result;
    }
    catch (Exception ex)
    {
        result.Success = false;
        result.Errors.Add($"Import failed: {ex.Message}");
        return result;
    }
}

public class ImportResult
{
    public bool Success { get; set; }
    public int ImportedCount { get; set; }
    public List<string> Errors { get; set; } = new List<string>();
}
```

## CSV Format Examples

### Standard Chart of Accounts CSV

```csv
AccountName,OfficialCode,AccountType,ParentOfficialCode,BalanceAndIncomeLineId
"Cash in Hand",1001,Asset,,
"Bank Account - Checking",1002,Asset,,
"Bank Account - Savings",1003,Asset,,
"Accounts Receivable",1201,Asset,,
"Allowance for Doubtful Accounts",1202,Asset,1201,
"Inventory - Raw Materials",1301,Asset,,
"Inventory - Finished Goods",1302,Asset,,
"Prepaid Expenses",1401,Asset,,
"Office Equipment",1501,Asset,,
"Accumulated Depreciation - Equipment",1502,Asset,1501,
"Accounts Payable",2001,Liability,,
"Accrued Expenses",2002,Liability,,
"Short-term Loans",2101,Liability,,
"Long-term Loans",2201,Liability,,
"Common Stock",3001,Equity,,
"Retained Earnings",3002,Equity,,
"Sales Revenue",4001,Revenue,,
"Service Revenue",4002,Revenue,,
"Interest Income",4101,Revenue,,
"Cost of Goods Sold",5001,Expense,,
"Salaries and Wages",5101,Expense,,
"Rent Expense",5201,Expense,,
"Utilities Expense",5202,Expense,,
"Depreciation Expense",5301,Expense,,
"Interest Expense",5401,Expense,,
```

### El Salvador Localized Chart of Accounts

```csv
AccountName,OfficialCode,AccountType,ParentOfficialCode,BalanceAndIncomeLineId
"Caja General",1001,Asset,,
"Banco Corriente",1002,Asset,,
"Cuentas por Cobrar",1201,Asset,,
"Inventario",1301,Asset,,
"Cuentas por Pagar",2001,Liability,,
"Prestamos por Pagar",2101,Liability,,
"Capital Social",3001,Equity,,
"Utilidades Retenidas",3002,Equity,,
"Ventas",4001,Revenue,,
"Ingresos por Servicios",4002,Revenue,,
"Costo de Ventas",5001,Expense,,
"Gastos de Administración",5101,Expense,,
"Gastos de Venta",5201,Expense,,
```

## Error Handling Best Practices

### 1. Comprehensive Error Checking

```csharp
public async Task<bool> SafeImportAccounts(IObjectSpace objectSpace, string csvContent, string userName)
{
    try
    {
        var importService = new XafAccountImportExportService(objectSpace);
        var (importedAccounts, errors) = await importService.ImportFromCsvAsync(csvContent, userName);
        
        if (errors.Any())
        {
            // Categorize errors
            var validationErrors = errors.Where(e => e.Contains("validation failed"));
            var duplicateErrors = errors.Where(e => e.Contains("already exists"));
            var formatErrors = errors.Where(e => e.Contains("Column count mismatch") || e.Contains("missing"));
            
            Console.WriteLine("Import Summary:");
            Console.WriteLine($"Total Errors: {errors.Count()}");
            Console.WriteLine($"Validation Errors: {validationErrors.Count()}");
            Console.WriteLine($"Duplicate Errors: {duplicateErrors.Count()}");
            Console.WriteLine($"Format Errors: {formatErrors.Count()}");
            
            return false;
        }
        
        return true;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Critical error during import: {ex.Message}");
        return false;
    }
}
```

### 2. Transaction Rollback on Errors

```csharp
public async Task<bool> ImportWithRollback(IObjectSpace objectSpace, string csvContent, string userName)
{
    // XAF ObjectSpace automatically handles transactions
    // If CommitChanges() is not called, changes are automatically rolled back
    
    try
    {
        var importService = new XafAccountImportExportService(objectSpace);
        var (importedAccounts, errors) = await importService.ImportFromCsvAsync(csvContent, userName);
        
        if (errors.Any())
        {
            // Don't commit changes if there are errors
            // ObjectSpace will automatically rollback
            Console.WriteLine("Import failed due to errors. Changes rolled back.");
            return false;
        }
        
        // Changes are already committed by the service
        Console.WriteLine("Import successful. Changes committed.");
        return true;
    }
    catch (Exception ex)
    {
        // ObjectSpace will automatically rollback on exception
        Console.WriteLine($"Import failed: {ex.Message}. Changes rolled back.");
        return false;
    }
}
```

## Integration with XAF Security

```csharp
public async Task<bool> SecureImportAccounts(IObjectSpace objectSpace, string csvContent)
{
    try
    {
        // Get current user from XAF security
        var currentUser = objectSpace.GetObjectByKey<ApplicationUser>(SecuritySystem.CurrentUserId);
        string userName = currentUser?.UserName ?? "Unknown";
        
        // Check permissions
        if (!SecuritySystem.IsGranted(new PermissionRequest(typeof(Account), SecurityOperations.Create)))
        {
            throw new UnauthorizedAccessException("User does not have permission to create accounts");
        }
        
        var importService = new XafAccountImportExportService(objectSpace);
        var (importedAccounts, errors) = await importService.ImportFromCsvAsync(csvContent, userName);
        
        return !errors.Any();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Security or import error: {ex.Message}");
        return false;
    }
}
```

These examples demonstrate the main usage patterns for the XAF Account Import/Export Service, covering basic operations, error handling, security integration, and best practices for XAF applications.
