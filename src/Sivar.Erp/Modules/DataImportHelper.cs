using Sivar.Erp.Services;
using Sivar.Erp.Services.ImportExport;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Sivar.Erp.Modules
{
    /// <summary>
    /// Helper class for importing initial data into an ObjectDb instance
    /// </summary>
    public class DataImportHelper
    {
        private readonly IAccountImportExportService _accountImportService;
        private readonly ITaxImportExportService _taxImportService;
        private readonly ITaxGroupImportExportService _taxGroupImportService;
        private readonly IDocumentTypeImportExportService _documentTypeImportService;
        private readonly IBusinessEntityImportExportService _businessEntityImportService;
        private readonly IItemImportExportService _itemImportService;
        private readonly IGroupMembershipImportExportService _groupMembershipImportService;
        private readonly ITaxRuleImportExportService _taxRuleImportService;
        private readonly string _username;

        /// <summary>
        /// Initializes a new instance of the DataImportHelper class with all required import services
        /// </summary>
        /// <param name="accountImportService">Service for importing accounts</param>
        /// <param name="taxImportService">Service for importing taxes</param>
        /// <param name="taxGroupImportService">Service for importing tax groups</param>
        /// <param name="documentTypeImportService">Service for importing document types</param>
        /// <param name="businessEntityImportService">Service for importing business entities</param>
        /// <param name="itemImportService">Service for importing items</param>
        /// <param name="groupMembershipImportService">Service for importing group memberships</param>
        /// <param name="username">Username to use for import operations</param>
        public DataImportHelper(
           IAccountImportExportService accountImportService,
           ITaxImportExportService taxImportService,
           ITaxGroupImportExportService taxGroupImportService,
           IDocumentTypeImportExportService documentTypeImportService,
           IBusinessEntityImportExportService businessEntityImportService,
           IItemImportExportService itemImportService,
           IGroupMembershipImportExportService groupMembershipImportService,
           ITaxRuleImportExportService taxRuleImportService, // ✅ ADD THIS
           string username = "SystemInit")
        {
            _accountImportService = accountImportService ?? throw new ArgumentNullException(nameof(accountImportService));
            _taxImportService = taxImportService ?? throw new ArgumentNullException(nameof(taxImportService));
            _taxGroupImportService = taxGroupImportService ?? throw new ArgumentNullException(nameof(taxGroupImportService));
            _documentTypeImportService = documentTypeImportService ?? throw new ArgumentNullException(nameof(documentTypeImportService));
            _businessEntityImportService = businessEntityImportService ?? throw new ArgumentNullException(nameof(businessEntityImportService));
            _itemImportService = itemImportService ?? throw new ArgumentNullException(nameof(itemImportService));
            _groupMembershipImportService = groupMembershipImportService ?? throw new ArgumentNullException(nameof(groupMembershipImportService));
            _taxRuleImportService = taxRuleImportService ?? throw new ArgumentNullException(nameof(taxRuleImportService)); // ✅ ADD THIS
            _username = username;
        }

        /// <summary>
        /// Loads all data from the specified data directory into the ObjectDb
        /// </summary>
        /// <param name="objectDb">The ObjectDb instance to populate</param>
        /// <param name="dataDirectory">Directory containing the data files</param>
        /// <returns>A dictionary of import results with any errors</returns>
        public async Task<Dictionary<string, List<string>>> ImportAllDataAsync(IObjectDb objectDb, string dataDirectory)
        {
            if (objectDb == null) throw new ArgumentNullException(nameof(objectDb));
            if (string.IsNullOrWhiteSpace(dataDirectory)) throw new ArgumentException("Data directory path cannot be empty", nameof(dataDirectory));
            if (!Directory.Exists(dataDirectory)) throw new DirectoryNotFoundException($"Data directory not found: {dataDirectory}");

            var results = new Dictionary<string, List<string>>();

            // Define the order of imports to handle dependencies correctly
            await ImportAccounts(objectDb, dataDirectory, results);
            await ImportTaxGroups(objectDb, dataDirectory, results);
            await ImportTaxes(objectDb, dataDirectory, results);
            await ImportTaxRules(objectDb, dataDirectory, results); // ✅ ADD THIS
            await ImportBusinessEntities(objectDb, dataDirectory, results);
            await ImportItems(objectDb, dataDirectory, results);
            await ImportGroupMemberships(objectDb, dataDirectory, results);
            await ImportDocumentTypes(objectDb, dataDirectory, results);

            return results;
        }
        /// <summary>
        /// ✅ NEW METHOD: Import tax rules from ElSalvadorTaxRules.txt
        /// </summary>
        private async Task ImportTaxRules(IObjectDb objectDb, string dataDirectory, Dictionary<string, List<string>> results)
        {
            var fileName = "ElSalvadorTaxRules.txt";
            var filePath = Path.Combine(dataDirectory, fileName);

            if (!File.Exists(filePath))
            {
                AddResult(results, fileName, $"File not found: {filePath}");
                return;
            }

            try
            {
                var csvContent = await File.ReadAllTextAsync(filePath);
                var (importedTaxRules, errors) = await _taxRuleImportService.ImportFromCsvAsync(csvContent, _username);

                if (errors.Count() > 0)
                {
                    foreach (var error in errors)
                    {
                        AddResult(results, fileName, error);
                    }
                }
                else
                {
                    // Add tax rules to ObjectDb
                    foreach (var taxRule in importedTaxRules)
                    {
                        objectDb.TaxRules.Add(taxRule); // ✅ Assuming ObjectDb has a TaxRules collection
                    }

                    AddResult(results, fileName, $"Successfully imported {importedTaxRules.Count()} tax rules");
                }
            }
            catch (Exception ex)
            {
                AddResult(results, fileName, $"Error importing tax rules: {ex.Message}");
            }
        }
        /// <summary>
        /// Import chart of accounts from ComercialChartOfAccounts.txt
        /// </summary>
        private async Task ImportAccounts(IObjectDb objectDb, string dataDirectory, Dictionary<string, List<string>> results)
        {
            var fileName = "ComercialChartOfAccounts.txt";
            var filePath = Path.Combine(dataDirectory, fileName);

            if (!File.Exists(filePath))
            {
                AddResult(results, fileName, $"File not found: {filePath}");
                return;
            }

            try
            {
                var csvContent = await File.ReadAllTextAsync(filePath);
                var (importedAccounts, errors) = await _accountImportService.ImportFromCsvAsync(csvContent, _username);

                if (errors.Count() > 0)
                {
                    foreach (var error in errors)
                    {
                        AddResult(results, fileName, error);
                    }
                }
                else
                {
                    // Add accounts to ObjectDb
                    foreach (var account in importedAccounts)
                    {
                        objectDb.Accounts.Add(account);
                    }

                    AddResult(results, fileName, $"Successfully imported {importedAccounts.Count()} accounts");
                }
            }
            catch (Exception ex)
            {
                AddResult(results, fileName, $"Error importing accounts: {ex.Message}");
            }
        }

        /// <summary>
        /// Import tax groups from ElSalvadorTaxGroups.txt
        /// </summary>
        private async Task ImportTaxGroups(IObjectDb objectDb, string dataDirectory, Dictionary<string, List<string>> results)
        {
            var fileName = "ElSalvadorTaxGroups.txt";
            var filePath = Path.Combine(dataDirectory, fileName);

            if (!File.Exists(filePath))
            {
                AddResult(results, fileName, $"File not found: {filePath}");
                return;
            }

            try
            {
                var csvContent = await File.ReadAllTextAsync(filePath);
                var (importedTaxGroups, errors) = await _taxGroupImportService.ImportFromCsvAsync(csvContent, _username);

                if (errors.Count() > 0)
                {
                    foreach (var error in errors)
                    {
                        AddResult(results, fileName, error);
                    }
                }
                else
                {
                    // Add tax groups to ObjectDb
                    foreach (var taxGroup in importedTaxGroups)
                    {
                        objectDb.TaxGroups.Add(taxGroup);
                    }

                    AddResult(results, fileName, $"Successfully imported {importedTaxGroups.Count()} tax groups");
                }
            }
            catch (Exception ex)
            {
                AddResult(results, fileName, $"Error importing tax groups: {ex.Message}");
            }
        }

        /// <summary>
        /// Import taxes from ElSalvadorTaxes.txt
        /// </summary>
        private async Task ImportTaxes(IObjectDb objectDb, string dataDirectory, Dictionary<string, List<string>> results)
        {
            var fileName = "ElSalvadorTaxes.txt";
            var filePath = Path.Combine(dataDirectory, fileName);

            if (!File.Exists(filePath))
            {
                AddResult(results, fileName, $"File not found: {filePath}");
                return;
            }

            try
            {
                var csvContent = await File.ReadAllTextAsync(filePath);
                var (importedTaxes, errors) = await _taxImportService.ImportFromCsvAsync(csvContent, _username);

                if (errors.Count() > 0)
                {
                    foreach (var error in errors)
                    {
                        AddResult(results, fileName, error);
                    }
                }
                else
                {
                    // Add taxes to ObjectDb
                    foreach (var tax in importedTaxes)
                    {
                        objectDb.Taxes.Add(tax);
                    }

                    AddResult(results, fileName, $"Successfully imported {importedTaxes.Count()} taxes");
                }
            }
            catch (Exception ex)
            {
                AddResult(results, fileName, $"Error importing taxes: {ex.Message}");
            }
        }

        /// <summary>
        /// Import business entities from BusinesEntities.txt
        /// </summary>
        private async Task ImportBusinessEntities(IObjectDb objectDb, string dataDirectory, Dictionary<string, List<string>> results)
        {
            var fileName = "BusinesEntities.txt";
            var filePath = Path.Combine(dataDirectory, fileName);

            if (!File.Exists(filePath))
            {
                AddResult(results, fileName, $"File not found: {filePath}");
                return;
            }

            try
            {
                var csvContent = await File.ReadAllTextAsync(filePath);
                var (importedEntities, errors) = await _businessEntityImportService.ImportFromCsvAsync(csvContent, _username);

                if (errors.Count() > 0)
                {
                    foreach (var error in errors)
                    {
                        AddResult(results, fileName, error);
                    }
                }
                else
                {
                    // Add business entities to ObjectDb
                    foreach (var entity in importedEntities)
                    {
                        objectDb.BusinessEntities.Add(entity);
                    }

                    AddResult(results, fileName, $"Successfully imported {importedEntities.Count()} business entities");
                }
            }
            catch (Exception ex)
            {
                AddResult(results, fileName, $"Error importing business entities: {ex.Message}");
            }
        }

        /// <summary>
        /// Import items from Items.txt
        /// </summary>
        private async Task ImportItems(IObjectDb objectDb, string dataDirectory, Dictionary<string, List<string>> results)
        {
            var fileName = "Items.txt";
            var filePath = Path.Combine(dataDirectory, fileName);

            if (!File.Exists(filePath))
            {
                AddResult(results, fileName, $"File not found: {filePath}");
                return;
            }

            try
            {
                var csvContent = await File.ReadAllTextAsync(filePath);
                var (importedItems, errors) = await _itemImportService.ImportFromCsvAsync(csvContent, _username);

                if (errors.Count() > 0)
                {
                    foreach (var error in errors)
                    {
                        AddResult(results, fileName, error);
                    }
                }
                else
                {
                    // Add items to ObjectDb
                    foreach (var item in importedItems)
                    {
                        objectDb.Items.Add(item);
                    }

                    AddResult(results, fileName, $"Successfully imported {importedItems.Count()} items");
                }
            }
            catch (Exception ex)
            {
                AddResult(results, fileName, $"Error importing items: {ex.Message}");
            }
        }

        /// <summary>
        /// Import group memberships from custom CSV
        /// </summary>
        private async Task ImportGroupMemberships(IObjectDb objectDb, string dataDirectory, Dictionary<string, List<string>> results)
        {
            // For group memberships, we create a CSV from our assignments
            var csvContent = CreateGroupMembershipCsv();
            var fileName = "GroupMemberships.csv";

            try
            {
                var (importedMemberships, errors) = await _groupMembershipImportService.ImportFromCsvAsync(csvContent, _username);

                if (errors.Count() > 0)
                {
                    foreach (var error in errors)
                    {
                        AddResult(results, fileName, error);
                    }
                }
                else
                {
                    // Add group memberships to ObjectDb
                    foreach (var membership in importedMemberships)
                    {
                        objectDb.GroupMemberships.Add(membership);
                    }

                    AddResult(results, fileName, $"Successfully imported {importedMemberships.Count()} group memberships");
                    
                    // Save the generated group memberships to a file for reference
                    var outputPath = Path.Combine(dataDirectory, fileName);
                    await File.WriteAllTextAsync(outputPath, csvContent);
                }
            }
            catch (Exception ex)
            {
                AddResult(results, fileName, $"Error importing group memberships: {ex.Message}");
            }
        }

        /// <summary>
        /// Import document types
        /// </summary>
        private async Task ImportDocumentTypes(IObjectDb objectDb, string dataDirectory, Dictionary<string, List<string>> results)
        {
            // Create standard document types for El Salvador
            var csvContent = CreateDocumentTypeCsv();
            var fileName = "DocumentTypes.csv";

            try
            {
                var (importedDocumentTypes, errors) = await _documentTypeImportService.ImportFromCsvAsync(csvContent, _username);

                if (errors.Count() > 0)
                {
                    foreach (var error in errors)
                    {
                        AddResult(results, fileName, error);
                    }
                }
                else
                {
                    // Add document types to ObjectDb
                    foreach (var documentType in importedDocumentTypes)
                    {
                        objectDb.DocumentTypes.Add(documentType);
                    }

                    AddResult(results, fileName, $"Successfully imported {importedDocumentTypes.Count()} document types");
                    
                    // Save the generated document types to a file for reference
                    var outputPath = Path.Combine(dataDirectory, fileName);
                    await File.WriteAllTextAsync(outputPath, csvContent);
                }
            }
            catch (Exception ex)
            {
                AddResult(results, fileName, $"Error importing document types: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a CSV for group memberships based on the requirements
        /// </summary>
        private string CreateGroupMembershipCsv()
        {
            // Create mappings between business entities and tax groups
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Oid,GroupId,EntityId,GroupType");
            
            // National clients as registered taxpayers
            csv.AppendLine($"{Guid.NewGuid()},\"REGISTERED_TAXPAYERS\",\"CL001\",BusinessEntity");
            csv.AppendLine($"{Guid.NewGuid()},\"REGISTERED_TAXPAYERS\",\"CL002\",BusinessEntity");
            csv.AppendLine($"{Guid.NewGuid()},\"REGISTERED_TAXPAYERS\",\"CL003\",BusinessEntity");
            
            // International clients as final consumers
            csv.AppendLine($"{Guid.NewGuid()},\"FINAL_CONSUMERS\",\"CL004\",BusinessEntity");
            csv.AppendLine($"{Guid.NewGuid()},\"FINAL_CONSUMERS\",\"CL005\",BusinessEntity");
            
            // All products as taxable items
            csv.AppendLine($"{Guid.NewGuid()},\"TAXABLE_ITEMS\",\"PR001\",Item");
            csv.AppendLine($"{Guid.NewGuid()},\"TAXABLE_ITEMS\",\"PR002\",Item");
            csv.AppendLine($"{Guid.NewGuid()},\"TAXABLE_ITEMS\",\"PR003\",Item");
            csv.AppendLine($"{Guid.NewGuid()},\"TAXABLE_ITEMS\",\"PR004\",Item");
            csv.AppendLine($"{Guid.NewGuid()},\"TAXABLE_ITEMS\",\"PR005\",Item");

            return csv.ToString();
        }

        /// <summary>
        /// Creates a CSV for document types based on El Salvador's requirements
        /// </summary>
        private string CreateDocumentTypeCsv()
        {
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Oid,Code,Name,IsEnabled,DocumentOperation");
            
            // Common document types for El Salvador
            csv.AppendLine($"{Guid.NewGuid()},\"FCF\",\"Factura de Consumidor Final\",true,SalesInvoice");
            csv.AppendLine($"{Guid.NewGuid()},\"CCF\",\"Comprobante de Crédito Fiscal\",true,SalesInvoice");
            csv.AppendLine($"{Guid.NewGuid()},\"NC\",\"Nota de Crédito\",true,SalesCreditNote");
            csv.AppendLine($"{Guid.NewGuid()},\"ND\",\"Nota de Débito\",true,SalesDebitNote");
            csv.AppendLine($"{Guid.NewGuid()},\"FEX\",\"Factura de Exportación\",true,SalesInvoice");
            csv.AppendLine($"{Guid.NewGuid()},\"COM\",\"Compra\",true,PurchaseInvoice");
            csv.AppendLine($"{Guid.NewGuid()},\"NCC\",\"Nota de Crédito de Compra\",true,PurchaseCreditNote");
            csv.AppendLine($"{Guid.NewGuid()},\"NDC\",\"Nota de Débito de Compra\",true,PurchaseDebitNote");

            return csv.ToString();
        }

        /// <summary>
        /// Helper to add a result to the results dictionary
        /// </summary>
        private void AddResult(Dictionary<string, List<string>> results, string fileName, string message)
        {
            if (!results.ContainsKey(fileName))
            {
                results[fileName] = new List<string>();
            }
            
            results[fileName].Add(message);
        }
    }
}