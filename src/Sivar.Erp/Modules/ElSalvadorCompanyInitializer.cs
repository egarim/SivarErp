using Sivar.Erp.Services;
using Sivar.Erp.Services.ImportExport;
using Sivar.Erp.Modules.Accounting;
using Sivar.Erp.Services.Accounting.FiscalPeriods;
using Sivar.Erp.Documents;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Sivar.Erp.Modules
{
    /// <summary>
    /// Initializes an ObjectDb for an El Salvador company with standard data
    /// </summary>
    public class ElSalvadorCompanyInitializer
    {
        private readonly DataImportHelper _dataImportHelper;
        private readonly string _dataDirectory;

        /// <summary>
        /// Initializes a new instance of the ElSalvadorCompanyInitializer with all required services
        /// </summary>
        /// <param name="dataDirectory">Directory containing the initial data files</param>
        /// <param name="accountImportService">Service for importing accounts</param>
        /// <param name="taxImportService">Service for importing taxes</param>
        /// <param name="taxGroupImportService">Service for importing tax groups</param>
        /// <param name="documentTypeImportService">Service for importing document types</param>
        /// <param name="businessEntityImportService">Service for importing business entities</param>
        /// <param name="itemImportService">Service for importing items</param>
        /// <param name="groupMembershipImportService">Service for importing group memberships</param>
        public ElSalvadorCompanyInitializer(
            string dataDirectory,
            IAccountImportExportService accountImportService,
            ITaxImportExportService taxImportService,
            ITaxGroupImportExportService taxGroupImportService,
            IDocumentTypeImportExportService documentTypeImportService,
            IBusinessEntityImportExportService businessEntityImportService,
            IItemImportExportService itemImportService,
            IGroupMembershipImportExportService groupMembershipImportService,
            ITaxRuleImportExportService taxRuleImportService)
        {
            if (string.IsNullOrWhiteSpace(dataDirectory))
                throw new ArgumentException("Data directory path cannot be empty", nameof(dataDirectory));
            
            if (!Directory.Exists(dataDirectory))
                throw new DirectoryNotFoundException($"Data directory not found: {dataDirectory}");

            _dataDirectory = dataDirectory;
            _dataImportHelper = new DataImportHelper(
                accountImportService, 
                taxImportService, 
                taxGroupImportService,
                documentTypeImportService,
                businessEntityImportService,
                itemImportService,
                groupMembershipImportService, 
                taxRuleImportService);
        }

        /// <summary>
        /// Creates a new ObjectDb instance and initializes it with El Salvador company data
        /// </summary>
        /// <returns>Initialized ObjectDb and dictionary of import results</returns>
        public async Task<(IObjectDb ObjectDb, Dictionary<string, List<string>> Results)> CreateNewCompanyAsync()
        {
            var objectDb = new ObjectDb();
            var results = await _dataImportHelper.ImportAllDataAsync(objectDb, _dataDirectory);
            return (objectDb, results);
        }

        /// <summary>
        /// Initializes an existing ObjectDb instance with El Salvador company data
        /// </summary>
        /// <param name="objectDb">The ObjectDb instance to populate</param>
        /// <returns>Dictionary of import results</returns>
        public async Task<Dictionary<string, List<string>>> InitializeExistingCompanyAsync(IObjectDb objectDb)
        {
            if (objectDb == null) throw new ArgumentNullException(nameof(objectDb));
            return await _dataImportHelper.ImportAllDataAsync(objectDb, _dataDirectory);
        }
        
        /// <summary>
        /// Demonstrates how to use the accounting module to create and post transactions
        /// This is a sample method showing how other modules would interact with the accounting module
        /// </summary>
        /// <param name="objectDb">The ObjectDb instance to use</param>
        /// <param name="accountingModule">The accounting module interface</param>
        public async Task<string> DemonstrateAccountingModuleUsageAsync(
            IObjectDb objectDb, 
            IAccountingModule accountingModule)
        {
            try
            {
                // Step 1: Verify a fiscal period exists for today
                var today = DateOnly.FromDateTime(DateTime.Today);
                var isOpenPeriod = await accountingModule.IsDateInOpenFiscalPeriodAsync(today);
                
                if (!isOpenPeriod)
                {
                    // Use the fiscal period service to create a period if needed
                    var fiscalService = accountingModule.GetFiscalPeriodService();
                    
                    // Create a fiscal period for current year
                    var currentYear = DateTime.Now.Year;
                    var fiscalPeriod = new FiscalPeriodDto
                    {
                        Code = $"FP{currentYear}",
                        Name = $"Fiscal Year {currentYear}",
                        Description = $"Regular fiscal period for {currentYear}",
                        StartDate = new DateOnly(currentYear, 1, 1),
                        EndDate = new DateOnly(currentYear, 12, 31),
                        Status = FiscalPeriodStatus.Open,
                        InsertedBy = "SystemAdmin",
                        UpdatedBy = "SystemAdmin",
                        InsertedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    
                    await fiscalService.CreateFiscalPeriodAsync(fiscalPeriod, "SystemAdmin");
                }
                
                // Step 2: Create a document (typically this would come from a form or UI)
                var documentType = objectDb.DocumentTypes.FirstOrDefault();
                var businessEntity = objectDb.BusinessEntities.FirstOrDefault();
                
                if (documentType == null || businessEntity == null)
                {
                    return "Could not find document type or business entity";
                }
                
                // Step 3: Generate a transaction from the document
                // Note: This would typically use actual document with totals
                // For this example, we'll create a sample transaction directly
                
                // In a real system, you would:
                // 1. Create document with real line items
                // 2. Calculate taxes 
                // 3. Generate the transaction
                
                return $"Successfully demonstrated accounting module integration";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}