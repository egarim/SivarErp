using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.XtraRichEdit.Internal;
using Sivar.Erp.Modules.Accounting.ChartOfAccounts;
using Sivar.Erp.Xaf.Module.BusinessObjects;
using Sivar.Erp.Xaf.Module.Services.ImportExport;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sivar.Erp.Xaf.Module.Controllers
{
    public class ImportController : ViewController
    {
        SimpleAction Import;
        public ImportController() : base()
        {
            // Target required Views (use the TargetXXX properties) and create their Actions.

            TargetObjectType = typeof(ImportFile);

            Import = new SimpleAction(this, "Import File", "View");
            Import.Execute += Import_Execute;

        }

        private async void Import_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            try
            {
                var importFile = this.View.CurrentObject as ImportFile;
                string csvContent = await ExtractCsvContentFromFile(importFile);

                switch (importFile.FileType)
                {
                    case FIleType.Accounts:
                        await ImportAccounts(csvContent);
                        break;
                    case FIleType.TaxGroups:
                        await ImportTaxGroups(csvContent);
                        break;
                    case FIleType.Taxes:
                        await ImportTaxes(csvContent);
                        break;
                    case FIleType.TaxRules:
                        await ImportTaxRules(csvContent);
                        break;
                    default:
                        throw new UserFriendlyException($"Unsupported file type: {importFile.FileType}");
                }
            }
            catch (Exception ex)
            {
                Application.ShowViewStrategy.ShowMessage(
                    $"Error during import: {ex.Message}",
                    InformationType.Error);
            }
        }

        private async Task<string> ExtractCsvContentFromFile(ImportFile importFile)
        {
            var memoryStream = new System.IO.MemoryStream();
            importFile.File.SaveToStream(memoryStream);
            memoryStream.Position = 0;
            
            // Read the file with proper encoding detection
            return await ReadFileWithEncodingDetection(memoryStream);
        }

        private async Task<string> ReadFileWithEncodingDetection(MemoryStream stream)
        {
            stream.Position = 0;
            var buffer = new byte[Math.Min(1024, stream.Length)];
            await stream.ReadAsync(buffer, 0, buffer.Length);
            stream.Position = 0;

            // Check for UTF-8 BOM
            if (buffer.Length >= 3 && buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF)
            {
                // File has UTF-8 BOM, read as UTF-8
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    return await reader.ReadToEndAsync();
                }
            }
            
            // Try UTF-8 first (without BOM)
            try
            {
                stream.Position = 0;
                using (var reader = new StreamReader(stream, new UTF8Encoding(false, true)))
                {
                    var content = await reader.ReadToEndAsync();
                    // If UTF-8 parsing succeeded without throwing, return the content
                    return content;
                }
            }
            catch (DecoderFallbackException)
            {
                // UTF-8 failed, try Windows-1252 (Latin-1)
                stream.Position = 0;
                using (var reader = new StreamReader(stream, Encoding.GetEncoding(1252)))
                {
                    return await reader.ReadToEndAsync();
                }
            }
        }

        private async Task ImportAccounts(string csvContent)
        {
            var validator = new AccountValidator(AccountValidator.GetElSalvadorAccountTypePrefixes());
            var importService = new XafAccountImportExportService(ObjectSpace, validator);
            var (importedItems, errors) = await importService.ImportFromCsvAsync(csvContent, "CurrentUser");

            HandleImportResult(importedItems, errors, "accounts");
        }

        private async Task ImportTaxes(string csvContent)
        {
            var importService = new XafTaxImportExportService(ObjectSpace);
            var (importedItems, errors) = await importService.ImportFromCsvAsync(csvContent, "CurrentUser");

            HandleImportResult(importedItems, errors, "taxes");
        }

        private async Task ImportTaxGroups(string csvContent)
        {
            var importService = new XafTaxGroupImportExportService(ObjectSpace);
            var (importedItems, errors) = await importService.ImportFromCsvAsync(csvContent, "CurrentUser");

            HandleImportResult(importedItems, errors, "tax groups");
        }

        private async Task ImportTaxRules(string csvContent)
        {
            var importService = new XafTaxRuleImportExportService(ObjectSpace);
            var (importedItems, errors) = await importService.ImportFromCsvAsync(csvContent, "CurrentUser");

            HandleImportResult(importedItems, errors, "tax rules");
        }

        private void HandleImportResult<T>(IEnumerable<T> importedItems, IEnumerable<string> errors, string itemTypeName)
        {
            if (errors.Any())
            {
                string errorMessage = string.Join("\\n", errors);
                throw new UserFriendlyException($"Import completed with errors:\\n{errorMessage}");
            }
            
            Application.ShowViewStrategy.ShowMessage(
                $"Successfully imported {importedItems.Count()} {itemTypeName}.",
                InformationType.Success);
            
            View.ObjectSpace.Refresh();
        }
        protected override void OnActivated()
        {
            base.OnActivated();
            // Perform various tasks depending on the target View.
        }
        protected override void OnDeactivated()
        {
            // Unsubscribe from previously subscribed events and release other references and resources.
            base.OnDeactivated();
        }
        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();
            // Access and customize the target View control.
        }
    }
}
