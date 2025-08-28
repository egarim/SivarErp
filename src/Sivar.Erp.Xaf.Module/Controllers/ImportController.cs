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
        private async void Import_Execute(object sender, SimpleActionExecuteEventArgs e)
        {

            try
            {
                // This would typically come from a file upload dialog
                string csvContent = ""; ///GetCsvContentFromUser();
                var importFile = this.View.CurrentObject as ImportFile;
                var MemoryStream = new System.IO.MemoryStream();
                importFile.File.SaveToStream(MemoryStream);
                MemoryStream.Position = 0;
                using (var reader = new System.IO.StreamReader(MemoryStream, Encoding.UTF8))
                {
                    csvContent = reader.ReadToEnd();
                    // Process the CSV content
                }

                var Validator = new AccountValidator(AccountValidator.GetElSalvadorAccountTypePrefixes());
                // Create the service with current ObjectSpace
                var importService = new XafAccountImportExportService(ObjectSpace,Validator);

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
