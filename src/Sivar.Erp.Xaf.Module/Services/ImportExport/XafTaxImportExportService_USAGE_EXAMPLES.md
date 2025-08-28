# XAF Tax Import/Export Service - Usage Examples

## Complete Controller Implementation

### Tax Management Controller

```csharp
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Utils;
using Microsoft.Extensions.Logging;
using Sivar.Erp.EfCore.Entities;
using Sivar.Erp.Modules.Taxes;
using Sivar.Erp.Xaf.Module.Services.ImportExport;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Sivar.Erp.Xaf.Module.Controllers
{
    public class TaxManagementController : ObjectViewController<ListView, Tax>
    {
        private SimpleAction _importTaxesAction;
        private SimpleAction _exportTaxesAction;
        private SimpleAction _exportSelectedTaxesAction;
        private readonly ILogger<TaxManagementController>? _logger;

        public TaxManagementController()
        {
            InitializeActions();
        }

        public TaxManagementController(ILogger<TaxManagementController> logger)
        {
            _logger = logger;
            InitializeActions();
        }

        private void InitializeActions()
        {
            // Import action
            _importTaxesAction = new SimpleAction(this, "ImportTaxes", "Import/Export")
            {
                Caption = "Import Taxes from CSV",
                ImageName = "Import",
                ToolTip = "Import taxes from a CSV file"
            };
            _importTaxesAction.Execute += ImportTaxesAction_Execute;

            // Export all action
            _exportTaxesAction = new SimpleAction(this, "ExportAllTaxes", "Import/Export")
            {
                Caption = "Export All Taxes",
                ImageName = "Export",
                ToolTip = "Export all taxes to CSV file"
            };
            _exportTaxesAction.Execute += ExportAllTaxesAction_Execute;

            // Export selected action
            _exportSelectedTaxesAction = new SimpleAction(this, "ExportSelectedTaxes", "Import/Export")
            {
                Caption = "Export Selected",
                ImageName = "ExportSelected",
                ToolTip = "Export selected taxes to CSV file",
                SelectionDependencyType = SelectionDependencyType.RequireMultipleObjects
            };
            _exportSelectedTaxesAction.Execute += ExportSelectedTaxesAction_Execute;
        }

        private async void ImportTaxesAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            try
            {
                // Open file dialog to select CSV file
                using var openFileDialog = new OpenFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                    Title = "Select Tax CSV File",
                    Multiselect = false
                };

                if (openFileDialog.ShowDialog() != DialogResult.OK)
                    return;

                // Read CSV content
                string csvContent = await File.ReadAllTextAsync(openFileDialog.FileName, Encoding.UTF8);

                if (string.IsNullOrWhiteSpace(csvContent))
                {
                    Application.ShowViewStrategy.ShowMessage("The selected file is empty.");
                    return;
                }

                // Create service and import
                var service = new XafTaxImportExportService(ObjectSpace, _logger);
                var (importedTaxes, errors) = await service.ImportFromCsvAsync(csvContent, SecuritySystem.CurrentUserName);

                // Handle results
                if (errors.Any())
                {
                    string errorMessage = string.Join("\n", errors.Take(10)); // Show first 10 errors
                    if (errors.Count() > 10)
                        errorMessage += $"\n... and {errors.Count() - 10} more errors.";

                    Application.ShowViewStrategy.ShowMessage(
                        $"Import completed with {errors.Count()} error(s):\n\n{errorMessage}",
                        "Import Errors",
                        MessageType.Warning);
                }

                if (importedTaxes.Any())
                {
                    // Refresh the view to show new taxes
                    ObjectSpace.Refresh();
                    View.RefreshDataSource();

                    string successMessage = $"Successfully imported {importedTaxes.Count()} tax(es).";
                    if (errors.Any())
                        successMessage += $" {errors.Count()} error(s) occurred.";

                    Application.ShowViewStrategy.ShowMessage(successMessage, "Import Complete", MessageType.Information);
                }
                else if (!errors.Any())
                {
                    Application.ShowViewStrategy.ShowMessage("No new taxes were imported.", "Import Complete", MessageType.Information);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error during tax import");
                Application.ShowViewStrategy.ShowMessage($"Import failed: {ex.Message}", "Import Error", MessageType.Error);
            }
        }

        private async void ExportAllTaxesAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            try
            {
                var service = new XafTaxImportExportService(ObjectSpace, _logger);
                string csvContent = await service.ExportToCsvAsync(null); // null = export all

                await SaveCsvToFile(csvContent, "All_Taxes");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error during tax export");
                Application.ShowViewStrategy.ShowMessage($"Export failed: {ex.Message}", "Export Error", MessageType.Error);
            }
        }

        private async void ExportSelectedTaxesAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            try
            {
                // Get selected taxes
                var selectedTaxes = View.SelectedObjects.Cast<Tax>().ToList();
                
                if (!selectedTaxes.Any())
                {
                    Application.ShowViewStrategy.ShowMessage("No taxes selected for export.", "Export", MessageType.Warning);
                    return;
                }

                // Convert XAF entities to DTOs for export
                var service = new XafTaxImportExportService(ObjectSpace, _logger);
                var taxDtos = selectedTaxes.Select(ConvertTaxToDto).ToList();

                string csvContent = await service.ExportToCsvAsync(taxDtos);
                await SaveCsvToFile(csvContent, "Selected_Taxes");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error during selected taxes export");
                Application.ShowViewStrategy.ShowMessage($"Export failed: {ex.Message}", "Export Error", MessageType.Error);
            }
        }

        private async Task SaveCsvToFile(string csvContent, string defaultFileName)
        {
            using var saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                Title = "Save Tax CSV File",
                FileName = $"{defaultFileName}_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                await File.WriteAllTextAsync(saveFileDialog.FileName, csvContent, Encoding.UTF8);
                Application.ShowViewStrategy.ShowMessage($"Taxes exported successfully to:\n{saveFileDialog.FileName}", "Export Complete", MessageType.Information);
            }
        }

        private TaxDto ConvertTaxToDto(Tax tax)
        {
            return new TaxDto
            {
                ID = tax.Oid,
                Name = tax.Name,
                Code = tax.Code,
                TaxType = tax.TaxType,
                ApplicationLevel = tax.ApplicationLevel,
                Percentage = tax.Percentage,
                Amount = tax.Amount,
                IsEnabled = tax.IsEnabled,
                IsIncludedInPrice = tax.IsIncludedInPrice
            };
        }
    }
}
```

## Web API Controller for Tax Import/Export

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DevExpress.ExpressApp;
using Sivar.Erp.Xaf.Module.Services.ImportExport;
using Sivar.Erp.Modules.Taxes;

namespace Sivar.Erp.Xaf.Blazor.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaxImportExportController : ControllerBase
    {
        private readonly IObjectSpaceProvider _objectSpaceProvider;
        private readonly ILogger<TaxImportExportController> _logger;

        public TaxImportExportController(IObjectSpaceProvider objectSpaceProvider, ILogger<TaxImportExportController> logger)
        {
            _objectSpaceProvider = objectSpaceProvider;
            _logger = logger;
        }

        [HttpPost("import")]
        public async Task<IActionResult> ImportTaxes([FromBody] ImportRequest request)
        {
            using var objectSpace = _objectSpaceProvider.CreateObjectSpace();
            
            try
            {
                var service = new XafTaxImportExportService(objectSpace, _logger);
                var (importedTaxes, errors) = await service.ImportFromCsvAsync(request.CsvContent, request.UserName);

                return Ok(new ImportResponse
                {
                    ImportedCount = importedTaxes.Count(),
                    Errors = errors.ToList(),
                    Success = !errors.Any()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tax import failed");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportTaxes([FromQuery] bool selectedOnly = false, [FromQuery] string[] taxCodes = null)
        {
            using var objectSpace = _objectSpaceProvider.CreateObjectSpace();
            
            try
            {
                var service = new XafTaxImportExportService(objectSpace, _logger);
                string csvContent;

                if (selectedOnly && taxCodes?.Length > 0)
                {
                    // Export specific taxes by codes
                    var taxes = objectSpace.GetObjects<Tax>()
                        .Where(t => taxCodes.Contains(t.Code))
                        .Select(ConvertTaxToDto)
                        .ToList();
                    
                    csvContent = await service.ExportToCsvAsync(taxes);
                }
                else
                {
                    // Export all taxes
                    csvContent = await service.ExportToCsvAsync(null);
                }

                var bytes = System.Text.Encoding.UTF8.GetBytes(csvContent);
                return File(bytes, "text/csv", $"taxes_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tax export failed");
                return BadRequest(new { error = ex.Message });
            }
        }

        private TaxDto ConvertTaxToDto(Tax tax)
        {
            return new TaxDto
            {
                ID = tax.Oid,
                Name = tax.Name,
                Code = tax.Code,
                TaxType = tax.TaxType,
                ApplicationLevel = tax.ApplicationLevel,
                Percentage = tax.Percentage,
                Amount = tax.Amount,
                IsEnabled = tax.IsEnabled,
                IsIncludedInPrice = tax.IsIncludedInPrice
            };
        }
    }

    public class ImportRequest
    {
        public string CsvContent { get; set; }
        public string UserName { get; set; }
    }

    public class ImportResponse
    {
        public int ImportedCount { get; set; }
        public List<string> Errors { get; set; }
        public bool Success { get; set; }
    }
}
```

## Scheduled Background Import

```csharp
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using DevExpress.ExpressApp;
using Sivar.Erp.Xaf.Module.Services.ImportExport;

public class TaxImportBackgroundService : BackgroundService
{
    private readonly IObjectSpaceProvider _objectSpaceProvider;
    private readonly ILogger<TaxImportBackgroundService> _logger;
    private readonly string _importFolderPath;

    public TaxImportBackgroundService(
        IObjectSpaceProvider objectSpaceProvider, 
        ILogger<TaxImportBackgroundService> logger,
        IConfiguration configuration)
    {
        _objectSpaceProvider = objectSpaceProvider;
        _logger = logger;
        _importFolderPath = configuration["TaxImport:FolderPath"] ?? @"C:\TaxImports";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessImportFiles();
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Check every 5 minutes
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in tax import background service");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Wait before retry
            }
        }
    }

    private async Task ProcessImportFiles()
    {
        if (!Directory.Exists(_importFolderPath))
            return;

        var csvFiles = Directory.GetFiles(_importFolderPath, "*.csv");
        
        foreach (var filePath in csvFiles)
        {
            try
            {
                await ProcessSingleFile(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing file {FilePath}", filePath);
                // Move to error folder
                MoveFileToErrorFolder(filePath);
            }
        }
    }

    private async Task ProcessSingleFile(string filePath)
    {
        using var objectSpace = _objectSpaceProvider.CreateObjectSpace();
        
        var csvContent = await File.ReadAllTextAsync(filePath);
        var service = new XafTaxImportExportService(objectSpace, _logger);
        
        var (importedTaxes, errors) = await service.ImportFromCsvAsync(csvContent, "System");

        if (errors.Any())
        {
            _logger.LogWarning("Import file {FilePath} had {ErrorCount} errors", filePath, errors.Count());
            await WriteErrorLog(filePath, errors);
        }

        _logger.LogInformation("Processed file {FilePath}: {ImportedCount} taxes imported, {ErrorCount} errors", 
            filePath, importedTaxes.Count(), errors.Count());

        // Move to processed folder
        MoveFileToProcessedFolder(filePath);
    }

    private async Task WriteErrorLog(string filePath, IEnumerable<string> errors)
    {
        var errorLogPath = Path.ChangeExtension(filePath, ".errors.txt");
        await File.WriteAllLinesAsync(errorLogPath, errors);
    }

    private void MoveFileToProcessedFolder(string filePath)
    {
        var processedFolder = Path.Combine(Path.GetDirectoryName(filePath), "Processed");
        Directory.CreateDirectory(processedFolder);
        
        var fileName = Path.GetFileName(filePath);
        var destinationPath = Path.Combine(processedFolder, $"{DateTime.Now:yyyyMMdd_HHmmss}_{fileName}");
        
        File.Move(filePath, destinationPath);
    }

    private void MoveFileToErrorFolder(string filePath)
    {
        var errorFolder = Path.Combine(Path.GetDirectoryName(filePath), "Errors");
        Directory.CreateDirectory(errorFolder);
        
        var fileName = Path.GetFileName(filePath);
        var destinationPath = Path.Combine(errorFolder, $"{DateTime.Now:yyyyMMdd_HHmmss}_{fileName}");
        
        File.Move(filePath, destinationPath);
    }
}
```

## Testing Examples

```csharp
using Xunit;
using DevExpress.ExpressApp;
using Sivar.Erp.Xaf.Module.Services.ImportExport;
using Sivar.Erp.EfCore.Entities;

public class XafTaxImportExportServiceTests
{
    private readonly IObjectSpace _objectSpace;
    private readonly XafTaxImportExportService _service;

    public XafTaxImportExportServiceTests()
    {
        // Setup test ObjectSpace (implementation depends on your test setup)
        _objectSpace = CreateTestObjectSpace();
        _service = new XafTaxImportExportService(_objectSpace);
    }

    [Fact]
    public async Task ImportFromCsvAsync_ValidData_ShouldImportSuccessfully()
    {
        // Arrange
        var csvContent = @"Code,Name,TaxType,ApplicationLevel,Percentage,Amount,IsEnabled,IsIncludedInPrice
VAT,Value Added Tax,Percentage,Line,15.0,,true,false
GST,Goods and Services Tax,Percentage,Line,10.0,,true,false";

        // Act
        var (importedTaxes, errors) = await _service.ImportFromCsvAsync(csvContent, "TestUser");

        // Assert
        Assert.Equal(2, importedTaxes.Count());
        Assert.Empty(errors);
        
        var vatTax = importedTaxes.First(t => t.Code == "VAT");
        Assert.Equal("Value Added Tax", vatTax.Name);
        Assert.Equal(15.0m, vatTax.Percentage);
    }

    [Fact]
    public async Task ImportFromCsvAsync_DuplicateCode_ShouldReturnError()
    {
        // Arrange
        // First, create a tax in the database
        var existingTax = _objectSpace.CreateObject<Tax>();
        existingTax.Code = "VAT";
        existingTax.Name = "Existing VAT";
        _objectSpace.CommitChanges();

        var csvContent = @"Code,Name,TaxType,ApplicationLevel,Percentage,Amount,IsEnabled,IsIncludedInPrice
VAT,New VAT,Percentage,Line,15.0,,true,false";

        // Act
        var (importedTaxes, errors) = await _service.ImportFromCsvAsync(csvContent, "TestUser");

        // Assert
        Assert.Empty(importedTaxes);
        Assert.Single(errors);
        Assert.Contains("already exists", errors.First());
    }

    [Fact]
    public async Task ExportToCsvAsync_WithData_ShouldExportCorrectly()
    {
        // Arrange
        var tax1 = _objectSpace.CreateObject<Tax>();
        tax1.Code = "VAT";
        tax1.Name = "Value Added Tax";
        tax1.TaxType = TaxType.Percentage;
        tax1.Percentage = 15.0m;
        
        var tax2 = _objectSpace.CreateObject<Tax>();
        tax2.Code = "GST";
        tax2.Name = "Goods and Services Tax";
        tax2.TaxType = TaxType.Percentage;
        tax2.Percentage = 10.0m;
        
        _objectSpace.CommitChanges();

        // Act
        var csvContent = await _service.ExportToCsvAsync(null);

        // Assert
        Assert.Contains("VAT", csvContent);
        Assert.Contains("GST", csvContent);
        Assert.Contains("Value Added Tax", csvContent);
        Assert.Contains("Goods and Services Tax", csvContent);
    }

    private IObjectSpace CreateTestObjectSpace()
    {
        // Implementation depends on your test setup
        // This might involve creating an in-memory database or test XAF application
        throw new NotImplementedException("Implement based on your test infrastructure");
    }
}
```

These examples show comprehensive usage patterns for the XAF Tax Import/Export Service in various scenarios including desktop controllers, web APIs, background services, and unit tests.
