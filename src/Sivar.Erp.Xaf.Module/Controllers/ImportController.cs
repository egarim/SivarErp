using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.XtraRichEdit.Internal;
using Sivar.Erp.Modules.Accounting.ChartOfAccounts;
using Sivar.Erp.Xaf.Module.BusinessObjects;
using Sivar.Erp.Xaf.Module.Services.ImportExport;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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

                string csvContent="";
                if (FileType.All != importFile.FileType)
                {
                    
                    csvContent = await ExtractCsvContentFromFile(importFile);
                }

                switch (importFile.FileType)
                {
                    case FileType.All:
                        await ImportFromZipFile(importFile);
                        break;
                    case FileType.Accounts:
                        await ImportAccounts(csvContent);
                        break;
                    case FileType.TaxGroups:
                        await ImportTaxGroups(csvContent);
                        break;
                    case FileType.Taxes:
                        await ImportTaxes(csvContent);
                        break;
                    case FileType.TaxRules:
                        await ImportTaxRules(csvContent);
                        break;
                    case FileType.BusinessEntities:
                        await ImportBusinessEntities(csvContent);
                        break;
                    case FileType.DocumentTypes:
                        await ImportDocumentTypes(csvContent);
                        break;
                    case FileType.Items:
                        await ImportItems(csvContent);
                        break;
                    case FileType.GroupMemberships:
                        await ImportGroupMemberships(csvContent);
                        break;
                    case FileType.PaymentMethods:
                        await ImportPaymentMethods(csvContent);
                        break;
                    case FileType.Transactions:
                        await ImportTransactions(csvContent);
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

        private async Task ImportBusinessEntities(string csvContent)
        {
            var importService = new XafBusinessEntityImportExportService(ObjectSpace);
            var (importedItems, errors) = await importService.ImportFromCsvAsync(csvContent, "CurrentUser");

            HandleImportResult(importedItems, errors, "business entities");
        }

        private async Task ImportDocumentTypes(string csvContent)
        {
            var importService = new XafDocumentTypeImportExportService(ObjectSpace);
            var (importedItems, errors) = await importService.ImportFromCsvAsync(csvContent, "CurrentUser");

            HandleImportResult(importedItems, errors, "document types");
        }

        private async Task ImportItems(string csvContent)
        {
            var importService = new XafItemImportExportService(ObjectSpace);
            var (importedItems, errors) = await importService.ImportFromCsvAsync(csvContent, "CurrentUser");

            HandleImportResult(importedItems, errors, "items");
        }

        private async Task ImportGroupMemberships(string csvContent)
        {
            var importService = new XafGroupMembershipImportExportService(ObjectSpace);
            var (importedItems, errors) = await importService.ImportFromCsvAsync(csvContent, "CurrentUser");

            HandleImportResult(importedItems, errors, "group memberships");
        }

        private async Task ImportPaymentMethods(string csvContent)
        {
            var importService = new XafPaymentMethodImportExportService(ObjectSpace);
            var (importedItems, errors) = await importService.ImportFromCsvAsync(csvContent, "CurrentUser");

            HandleImportResult(importedItems, errors, "payment methods");
        }

        private async Task ImportTransactions(string csvContent)
        {
            var importService = new XafTransactionsImportExportService(ObjectSpace);
            var (importedData, errors) = await importService.ImportFromCsvAsync(csvContent);

            HandleImportResult(importedData.Select(x => x.Transaction), errors, "transactions");
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

        /// <summary>
        /// Imports data from a ZIP file containing multiple CSV files.
        /// The ZIP file should contain CSV files named according to FileType enum values (e.g., accounts.csv, taxes.csv).
        /// Not all file types are required in the ZIP - only the ones present will be processed.
        /// Files are imported in priority order (using decimal priorities) to ensure proper dependency resolution:
        /// - Accounts (1.0) are imported first as they are referenced by other entities
        /// - Transactions (10.0) are imported last as they depend on most other entities
        /// - Decimal priorities allow inserting new file types between existing ones if needed
        /// </summary>
        /// <param name="importFile">The import file containing the ZIP archive</param>
        private async Task ImportFromZipFile(ImportFile importFile)
        {
            var allErrors = new List<string>();
            var importResults = new Dictionary<string, int>();

            using (var memoryStream = new MemoryStream())
            {
                importFile.File.SaveToStream(memoryStream);
                memoryStream.Position = 0;

                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read))
                {
                    // Define the mapping between file names, FileType enum values, and import priority
                    var fileTypeMapping = new Dictionary<string, (FileType FileType, decimal Priority)>
                    {
                        { "accounts.csv", (FileType.Accounts, 1.0m) },
                        { "taxgroups.csv", (FileType.TaxGroups, 2.0m) },
                        { "taxes.csv", (FileType.Taxes, 3.0m) },
                        { "taxrules.csv", (FileType.TaxRules, 4.0m) },
                        { "businessentities.csv", (FileType.BusinessEntities, 5.0m) },
                        { "documenttypes.csv", (FileType.DocumentTypes, 6.0m) },
                        { "items.csv", (FileType.Items, 7.0m) },
                        { "groupmemberships.csv", (FileType.GroupMemberships, 8.0m) },
                        { "paymentmethods.csv", (FileType.PaymentMethods, 9.0m) },
                        { "transactions.csv", (FileType.Transactions, 10.0m) }
                    };

                    // Collect all entries with their priorities first
                    var entriesToProcess = new List<(ZipArchiveEntry Entry, FileType FileType, decimal Priority)>();

                    foreach (var entry in archive.Entries)
                    {
                        var fileName = entry.Name.ToLowerInvariant();
                        
                        if (fileTypeMapping.TryGetValue(fileName, out var fileInfo))
                        {
                            entriesToProcess.Add((entry, fileInfo.FileType, fileInfo.Priority));
                        }
                    }

                    // Sort by priority to ensure correct import order
                    entriesToProcess = entriesToProcess.OrderBy(x => x.Priority).ToList();

                    // Process entries in priority order
                    foreach (var (entry, fileType, priority) in entriesToProcess)
                    {
                        try
                        {
                            using (var entryStream = entry.Open())
                            {
                                var csvContent = await ReadCsvFromStream(entryStream);
                                
                                var itemCount = await ProcessSingleFileType(fileType, csvContent, allErrors);
                                if (itemCount > 0)
                                {
                                    var typeName = GetTypeDisplayName(fileType);
                                    importResults[typeName] = itemCount;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            allErrors.Add($"Error processing {entry.Name}: {ex.Message}");
                        }
                    }
                }
            }

            // Display results
            if (allErrors.Any())
            {
                string errorMessage = string.Join("\\n", allErrors);
                throw new UserFriendlyException($"Import completed with errors:\\n{errorMessage}");
            }

            if (importResults.Any())
            {
                string successMessage = "Successfully imported:\\n" +
                    string.Join("\\n", importResults.Select(kvp => $"- {kvp.Value} {kvp.Key}"));
                        
                Application.ShowViewStrategy.ShowMessage(successMessage, InformationType.Success);
                View.ObjectSpace.Refresh();
            }
            else
            {
                Application.ShowViewStrategy.ShowMessage(
                    "No valid CSV files found in the ZIP archive.",
                    InformationType.Warning);
            }
        }

        /// <summary>
        /// Reads CSV content from a stream with proper encoding detection.
        /// Supports UTF-8 (with and without BOM) and Windows-1252 encodings.
        /// Works with both seekable and non-seekable streams (like ZIP entry streams).
        /// </summary>
        /// <param name="stream">The stream to read from</param>
        /// <returns>The CSV content as a string</returns>
        private async Task<string> ReadCsvFromStream(Stream stream)
        {
            // For non-seekable streams (like ZIP entry streams), we need to read everything into memory first
            using (var memoryStream = new MemoryStream())
            {
                await stream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;
                
                // Now we can work with the seekable memory stream
                return await ReadFileWithEncodingDetection(memoryStream);
            }
        }

        /// <summary>
        /// Processes a single file type import and returns the count of imported items.
        /// </summary>
        /// <param name="fileType">The type of file being imported</param>
        /// <param name="csvContent">The CSV content to import</param>
        /// <param name="allErrors">List to collect any errors that occur</param>
        /// <returns>The number of items successfully imported</returns>
        private async Task<int> ProcessSingleFileType(FileType fileType, string csvContent, List<string> allErrors)
        {
            try
            {
                switch (fileType)
                {
                    case FileType.Accounts:
                        var accountValidator = new AccountValidator(AccountValidator.GetElSalvadorAccountTypePrefixes());
                        var accountImportService = new XafAccountImportExportService(ObjectSpace, accountValidator);
                        var (accountItems, accountErrors) = await accountImportService.ImportFromCsvAsync(csvContent, "CurrentUser");
                        
                        allErrors.AddRange(accountErrors);
                        return accountItems.Count();

                    case FileType.TaxGroups:
                        var taxGroupImportService = new XafTaxGroupImportExportService(ObjectSpace);
                        var (taxGroupItems, taxGroupErrors) = await taxGroupImportService.ImportFromCsvAsync(csvContent, "CurrentUser");
                        
                        allErrors.AddRange(taxGroupErrors);
                        return taxGroupItems.Count();

                    case FileType.Taxes:
                        var taxImportService = new XafTaxImportExportService(ObjectSpace);
                        var (taxItems, taxErrors) = await taxImportService.ImportFromCsvAsync(csvContent, "CurrentUser");
                        
                        allErrors.AddRange(taxErrors);
                        return taxItems.Count();

                    case FileType.TaxRules:
                        var taxRuleImportService = new XafTaxRuleImportExportService(ObjectSpace);
                        var (taxRuleItems, taxRuleErrors) = await taxRuleImportService.ImportFromCsvAsync(csvContent, "CurrentUser");
                        
                        allErrors.AddRange(taxRuleErrors);
                        return taxRuleItems.Count();

                    case FileType.BusinessEntities:
                        var businessEntityImportService = new XafBusinessEntityImportExportService(ObjectSpace);
                        var (businessEntityItems, businessEntityErrors) = await businessEntityImportService.ImportFromCsvAsync(csvContent, "CurrentUser");
                        
                        allErrors.AddRange(businessEntityErrors);
                        return businessEntityItems.Count();

                    case FileType.DocumentTypes:
                        var documentTypeImportService = new XafDocumentTypeImportExportService(ObjectSpace);
                        var (documentTypeItems, documentTypeErrors) = await documentTypeImportService.ImportFromCsvAsync(csvContent, "CurrentUser");
                        
                        allErrors.AddRange(documentTypeErrors);
                        return documentTypeItems.Count();

                    case FileType.Items:
                        var itemImportService = new XafItemImportExportService(ObjectSpace);
                        var (itemItems, itemErrors) = await itemImportService.ImportFromCsvAsync(csvContent, "CurrentUser");
                        
                        allErrors.AddRange(itemErrors);
                        return itemItems.Count();

                    case FileType.GroupMemberships:
                        var groupMembershipImportService = new XafGroupMembershipImportExportService(ObjectSpace);
                        var (groupMembershipItems, groupMembershipErrors) = await groupMembershipImportService.ImportFromCsvAsync(csvContent, "CurrentUser");
                        
                        allErrors.AddRange(groupMembershipErrors);
                        return groupMembershipItems.Count();

                    case FileType.PaymentMethods:
                        var paymentMethodImportService = new XafPaymentMethodImportExportService(ObjectSpace);
                        var (paymentMethodItems, paymentMethodErrors) = await paymentMethodImportService.ImportFromCsvAsync(csvContent, "CurrentUser");
                        
                        allErrors.AddRange(paymentMethodErrors);
                        return paymentMethodItems.Count();

                    case FileType.Transactions:
                        var transactionImportService = new XafTransactionsImportExportService(ObjectSpace);
                        var (transactionData, transactionErrors) = await transactionImportService.ImportFromCsvAsync(csvContent);
                        
                        allErrors.AddRange(transactionErrors);
                        return transactionData.Count();

                    default:
                        return 0;
                }
            }
            catch (Exception ex)
            {
                allErrors.Add($"Error importing {fileType}: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Gets the display name for a FileType enum value.
        /// </summary>
        /// <param name="fileType">The FileType enum value</param>
        /// <returns>A user-friendly display name</returns>
        private string GetTypeDisplayName(FileType fileType)
        {
            return fileType switch
            {
                FileType.Accounts => "accounts",
                FileType.TaxGroups => "tax groups",
                FileType.Taxes => "taxes",
                FileType.TaxRules => "tax rules",
                FileType.BusinessEntities => "business entities",
                FileType.DocumentTypes => "document types",
                FileType.Items => "items",
                FileType.GroupMemberships => "group memberships",
                FileType.PaymentMethods => "payment methods",
                FileType.Transactions => "transactions",
                _ => fileType.ToString().ToLowerInvariant()
            };
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
