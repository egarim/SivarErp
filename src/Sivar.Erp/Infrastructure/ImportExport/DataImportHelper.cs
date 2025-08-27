using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sivar.Erp.Core.Contracts;

namespace Sivar.Erp.Infrastructure.ImportExport
{
    /// <summary>
    /// Helper class for importing data from various sources into the ERP system
    /// </summary>
    public class DataImportHelper
    {
        private readonly ILogger<DataImportHelper> _logger;
        private readonly IAccountImportExportService _accountImportService;
        private readonly ITaxImportExportService _taxImportService;
        private readonly ITaxGroupImportExportService _taxGroupImportService;
        private readonly IDocumentTypeImportExportService _documentTypeImportService;
        private readonly IBusinessEntityImportExportService _businessEntityImportService;
        private readonly IItemImportExportService _itemImportService;
        private readonly IGroupMembershipImportExportService _groupMembershipImportService;
        private readonly ITaxRuleImportExportService _taxRuleImportService;

        /// <summary>
        /// Initializes a new instance of DataImportHelper
        /// </summary>
        public DataImportHelper(
            ILogger<DataImportHelper> logger,
            IAccountImportExportService accountImportService,
            ITaxImportExportService taxImportService,
            ITaxGroupImportExportService taxGroupImportService,
            IDocumentTypeImportExportService documentTypeImportService,
            IBusinessEntityImportExportService businessEntityImportService,
            IItemImportExportService itemImportService,
            IGroupMembershipImportExportService groupMembershipImportService,
            ITaxRuleImportExportService taxRuleImportService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _accountImportService = accountImportService ?? throw new ArgumentNullException(nameof(accountImportService));
            _taxImportService = taxImportService ?? throw new ArgumentNullException(nameof(taxImportService));
            _taxGroupImportService = taxGroupImportService ?? throw new ArgumentNullException(nameof(taxGroupImportService));
            _documentTypeImportService = documentTypeImportService ?? throw new ArgumentNullException(nameof(documentTypeImportService));
            _businessEntityImportService = businessEntityImportService ?? throw new ArgumentNullException(nameof(businessEntityImportService));
            _itemImportService = itemImportService ?? throw new ArgumentNullException(nameof(itemImportService));
            _groupMembershipImportService = groupMembershipImportService ?? throw new ArgumentNullException(nameof(groupMembershipImportService));
            _taxRuleImportService = taxRuleImportService ?? throw new ArgumentNullException(nameof(taxRuleImportService));
        }

        /// <summary>
        /// Imports all data from CSV files in the specified directory
        /// </summary>
        /// <param name="objectDb">Object database to populate</param>
        /// <param name="dataDirectory">Directory containing CSV files</param>
        /// <returns>Dictionary of filename to import results</returns>
        public async Task<Dictionary<string, List<string>>> ImportAllDataAsync(IObjectDb objectDb, string dataDirectory)
        {
            var results = new Dictionary<string, List<string>>();

            try
            {
                // Import in dependency order
                await ImportAccountsAsync(objectDb, dataDirectory, results);
                await ImportTaxGroupsAsync(objectDb, dataDirectory, results);
                await ImportTaxesAsync(objectDb, dataDirectory, results);
                await ImportDocumentTypesAsync(objectDb, dataDirectory, results);
                await ImportBusinessEntitiesAsync(objectDb, dataDirectory, results);
                await ImportItemsAsync(objectDb, dataDirectory, results);
                await ImportGroupMembershipsAsync(objectDb, dataDirectory, results);
                await ImportTaxRulesAsync(objectDb, dataDirectory, results);

                _logger.LogInformation("Data import completed successfully. Imported from {FileCount} files.", results.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during data import process");
                results.Add("ERROR", new List<string> { $"Import process failed: {ex.Message}" });
            }

            return results;
        }

        private async Task ImportAccountsAsync(IObjectDb objectDb, string dataDirectory, Dictionary<string, List<string>> results)
        {
            var filename = "Accounts.csv";
            var filePath = Path.Combine(dataDirectory, filename);
            
            if (!File.Exists(filePath))
            {
                results[filename] = new List<string> { "File not found" };
                return;
            }

            try
            {
                var csvContent = await File.ReadAllTextAsync(filePath);
                var (importedAccounts, errors) = await _accountImportService.ImportFromCsvAsync(csvContent, "DataImportHelper");
                
                results[filename] = new List<string>();
                
                foreach (var account in importedAccounts)
                {
                    objectDb.Accounts.Add(account);
                    results[filename].Add($"? Imported account: {account.OfficialCode} - {account.AccountName}");
                }
                
                if (errors.Any())
                {
                    results[filename].AddRange(errors.Select(e => $"? Error: {e}"));
                }
                
                _logger.LogInformation("Imported {Count} accounts from {Filename}", importedAccounts.Count(), filename);
            }
            catch (Exception ex)
            {
                results[filename] = new List<string> { $"? Error importing {filename}: {ex.Message}" };
                _logger.LogError(ex, "Error importing accounts from {Filename}", filename);
            }
        }

        private async Task ImportTaxesAsync(IObjectDb objectDb, string dataDirectory, Dictionary<string, List<string>> results)
        {
            var filename = "Taxes.csv";
            var filePath = Path.Combine(dataDirectory, filename);
            
            if (!File.Exists(filePath))
            {
                results[filename] = new List<string> { "File not found" };
                return;
            }

            try
            {
                var csvContent = await File.ReadAllTextAsync(filePath);
                var (importedTaxes, errors) = await _taxImportService.ImportFromCsvAsync(csvContent, "DataImportHelper");
                
                results[filename] = new List<string>();
                
                foreach (var tax in importedTaxes)
                {
                    objectDb.Taxes.Add(tax);
                    results[filename].Add($"? Imported tax: {tax.Code} - {tax.Name}");
                }
                
                if (errors.Any())
                {
                    results[filename].AddRange(errors.Select(e => $"? Error: {e}"));
                }
                
                _logger.LogInformation("Imported {Count} taxes from {Filename}", importedTaxes.Count(), filename);
            }
            catch (Exception ex)
            {
                results[filename] = new List<string> { $"? Error importing {filename}: {ex.Message}" };
                _logger.LogError(ex, "Error importing taxes from {Filename}", filename);
            }
        }

        private async Task ImportTaxGroupsAsync(IObjectDb objectDb, string dataDirectory, Dictionary<string, List<string>> results)
        {
            var filename = "TaxGroups.csv";
            var filePath = Path.Combine(dataDirectory, filename);
            
            if (!File.Exists(filePath))
            {
                results[filename] = new List<string> { "File not found" };
                return;
            }

            try
            {
                var csvContent = await File.ReadAllTextAsync(filePath);
                var (importedTaxGroups, errors) = await _taxGroupImportService.ImportFromCsvAsync(csvContent, "DataImportHelper");
                
                results[filename] = new List<string>();
                
                foreach (var taxGroup in importedTaxGroups)
                {
                    objectDb.TaxGroups.Add(taxGroup);
                    results[filename].Add($"? Imported tax group: {taxGroup.Code} - {taxGroup.Name}");
                }
                
                if (errors.Any())
                {
                    results[filename].AddRange(errors.Select(e => $"? Error: {e}"));
                }
                
                _logger.LogInformation("Imported {Count} tax groups from {Filename}", importedTaxGroups.Count(), filename);
            }
            catch (Exception ex)
            {
                results[filename] = new List<string> { $"? Error importing {filename}: {ex.Message}" };
                _logger.LogError(ex, "Error importing tax groups from {Filename}", filename);
            }
        }

        private async Task ImportDocumentTypesAsync(IObjectDb objectDb, string dataDirectory, Dictionary<string, List<string>> results)
        {
            var filename = "DocumentTypes.csv";
            var filePath = Path.Combine(dataDirectory, filename);
            
            if (!File.Exists(filePath))
            {
                results[filename] = new List<string> { "File not found" };
                return;
            }

            try
            {
                var csvContent = await File.ReadAllTextAsync(filePath);
                var (importedDocumentTypes, errors) = await _documentTypeImportService.ImportFromCsvAsync(csvContent, "DataImportHelper");
                
                results[filename] = new List<string>();
                
                foreach (var documentType in importedDocumentTypes)
                {
                    objectDb.DocumentTypes.Add(documentType);
                    results[filename].Add($"? Imported document type: {documentType.Code} - {documentType.Name}");
                }
                
                if (errors.Any())
                {
                    results[filename].AddRange(errors.Select(e => $"? Error: {e}"));
                }
                
                _logger.LogInformation("Imported {Count} document types from {Filename}", importedDocumentTypes.Count(), filename);
            }
            catch (Exception ex)
            {
                results[filename] = new List<string> { $"? Error importing {filename}: {ex.Message}" };
                _logger.LogError(ex, "Error importing document types from {Filename}", filename);
            }
        }

        private async Task ImportBusinessEntitiesAsync(IObjectDb objectDb, string dataDirectory, Dictionary<string, List<string>> results)
        {
            var filename = "BusinessEntities.csv";
            var filePath = Path.Combine(dataDirectory, filename);
            
            if (!File.Exists(filePath))
            {
                results[filename] = new List<string> { "File not found" };
                return;
            }

            try
            {
                var csvContent = await File.ReadAllTextAsync(filePath);
                var (importedBusinessEntities, errors) = await _businessEntityImportService.ImportFromCsvAsync(csvContent, "DataImportHelper");
                
                results[filename] = new List<string>();
                
                foreach (var businessEntity in importedBusinessEntities)
                {
                    objectDb.BusinessEntities.Add(businessEntity);
                    results[filename].Add($"? Imported business entity: {businessEntity.Code} - {businessEntity.Name}");
                }
                
                if (errors.Any())
                {
                    results[filename].AddRange(errors.Select(e => $"? Error: {e}"));
                }
                
                _logger.LogInformation("Imported {Count} business entities from {Filename}", importedBusinessEntities.Count(), filename);
            }
            catch (Exception ex)
            {
                results[filename] = new List<string> { $"? Error importing {filename}: {ex.Message}" };
                _logger.LogError(ex, "Error importing business entities from {Filename}", filename);
            }
        }

        private async Task ImportItemsAsync(IObjectDb objectDb, string dataDirectory, Dictionary<string, List<string>> results)
        {
            var filename = "Items.csv";
            var filePath = Path.Combine(dataDirectory, filename);
            
            if (!File.Exists(filePath))
            {
                results[filename] = new List<string> { "File not found" };
                return;
            }

            try
            {
                var csvContent = await File.ReadAllTextAsync(filePath);
                var (importedItems, errors) = await _itemImportService.ImportFromCsvAsync(csvContent, "DataImportHelper");
                
                results[filename] = new List<string>();
                
                foreach (var item in importedItems)
                {
                    objectDb.Items.Add(item);
                    results[filename].Add($"? Imported item: {item.Code} - {item.Description}");
                }
                
                if (errors.Any())
                {
                    results[filename].AddRange(errors.Select(e => $"? Error: {e}"));
                }
                
                _logger.LogInformation("Imported {Count} items from {Filename}", importedItems.Count(), filename);
            }
            catch (Exception ex)
            {
                results[filename] = new List<string> { $"? Error importing {filename}: {ex.Message}" };
                _logger.LogError(ex, "Error importing items from {Filename}", filename);
            }
        }

        private async Task ImportGroupMembershipsAsync(IObjectDb objectDb, string dataDirectory, Dictionary<string, List<string>> results)
        {
            var filename = "GroupMemberships.csv";
            var filePath = Path.Combine(dataDirectory, filename);
            
            if (!File.Exists(filePath))
            {
                results[filename] = new List<string> { "File not found" };
                return;
            }

            try
            {
                var csvContent = await File.ReadAllTextAsync(filePath);
                var (importedGroupMemberships, errors) = await _groupMembershipImportService.ImportFromCsvAsync(csvContent, "DataImportHelper");
                
                results[filename] = new List<string>();
                
                foreach (var groupMembership in importedGroupMemberships)
                {
                    objectDb.GroupMemberships.Add(groupMembership);
                    results[filename].Add($"? Imported group membership");
                }
                
                if (errors.Any())
                {
                    results[filename].AddRange(errors.Select(e => $"? Error: {e}"));
                }
                
                _logger.LogInformation("Imported {Count} group memberships from {Filename}", importedGroupMemberships.Count(), filename);
            }
            catch (Exception ex)
            {
                results[filename] = new List<string> { $"? Error importing {filename}: {ex.Message}" };
                _logger.LogError(ex, "Error importing group memberships from {Filename}", filename);
            }
        }

        private async Task ImportTaxRulesAsync(IObjectDb objectDb, string dataDirectory, Dictionary<string, List<string>> results)
        {
            var filename = "TaxRules.csv";
            var filePath = Path.Combine(dataDirectory, filename);
            
            if (!File.Exists(filePath))
            {
                results[filename] = new List<string> { "File not found" };
                return;
            }

            try
            {
                var csvContent = await File.ReadAllTextAsync(filePath);
                var (importedTaxRules, errors) = await _taxRuleImportService.ImportFromCsvAsync(csvContent, "DataImportHelper");
                
                results[filename] = new List<string>();
                
                foreach (var taxRule in importedTaxRules)
                {
                    objectDb.TaxRules.Add(taxRule);
                    results[filename].Add($"? Imported tax rule: {taxRule.Code} - {taxRule.Description}");
                }
                
                if (errors.Any())
                {
                    results[filename].AddRange(errors.Select(e => $"? Error: {e}"));
                }
                
                _logger.LogInformation("Imported {Count} tax rules from {Filename}", importedTaxRules.Count(), filename);
            }
            catch (Exception ex)
            {
                results[filename] = new List<string> { $"? Error importing {filename}: {ex.Message}" };
                _logger.LogError(ex, "Error importing tax rules from {Filename}", filename);
            }
        }
    }
}