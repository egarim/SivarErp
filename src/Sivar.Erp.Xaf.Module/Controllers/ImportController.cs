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
                    case FIleType.Taxes:
                        await ImportTaxes(csvContent);
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
            
            using (var reader = new System.IO.StreamReader(memoryStream, Encoding.UTF8))
            {
                return await reader.ReadToEndAsync();
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
