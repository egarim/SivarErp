using Sivar.Erp.Services;
using Sivar.Erp.Services.Accounting.ChartOfAccounts;
using Sivar.Erp.Services.ImportExport;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Sivar.Erp.Modules
{
    /// <summary>
    /// Example of how to use the ElSalvadorCompanyInitializer
    /// </summary>
    public static class ElSalvadorCompanyInitializerUsage
    {
        /// <summary>
        /// Example of initializing a new El Salvador company
        /// </summary>
        public static async Task InitializeElSalvadorCompanyExample()
        {
            try
            {
                // Path to the data files
                string dataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "ElSalvador");

                // Create all required import services - in a real application, these would be injected
                var accountValidator = new AccountValidator(AccountValidator.GetElSalvadorAccountTypePrefixes());
                
                var accountImportService = new AccountImportExportService(accountValidator);
                var taxImportService = new TaxImportExportService();
                var taxGroupImportService = new TaxGroupImportExportService();
                var businessEntityImportService = new BusinessEntityImportExportService();
                var itemImportService = new ItemImportExportService();
                var documentTypeImportService = new DocumentTypeImportExportService();
                var groupMembershipImportService = new GroupMembershipImportExportService();
                var taxRuleImportService = new TaxRuleImportExportService();
                // Create the initializer
                var initializer = new ElSalvadorCompanyInitializer(
                    dataDirectory,
                    accountImportService,
                    taxImportService,
                    taxGroupImportService,
                    documentTypeImportService,
                    businessEntityImportService,
                    itemImportService,
                    groupMembershipImportService,
                    taxRuleImportService);

                // Option 1: Create a new company with all data
                var (objectDb, results) = await initializer.CreateNewCompanyAsync();

                // Option 2: Initialize an existing ObjectDb
                var existingDb = new ObjectDb();
                var resultsForExistingDb = await initializer.InitializeExistingCompanyAsync(existingDb);

                // Check the results
                foreach (var file in results.Keys)
                {
                    Console.WriteLine($"File: {file}");
                    foreach (var message in results[file])
                    {
                        Console.WriteLine($"  {message}");
                    }
                }

                // At this point, objectDb contains all the data for the El Salvador company
                Console.WriteLine($"Accounts: {objectDb.Accounts.Count}");
                Console.WriteLine($"Business Entities: {objectDb.BusinessEntities.Count}");
                Console.WriteLine($"Document Types: {objectDb.DocumentTypes.Count}");
                Console.WriteLine($"Tax Groups: {objectDb.TaxGroups.Count}");
                Console.WriteLine($"Taxes: {objectDb.Taxes.Count}");
                Console.WriteLine($"Items: {objectDb.Items.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing company: {ex.Message}");
            }
        }
    }
}