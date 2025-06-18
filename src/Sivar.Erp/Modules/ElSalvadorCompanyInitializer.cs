using Sivar.Erp.Services;
using Sivar.Erp.Services.ImportExport;
using System;
using System.Collections.Generic;
using System.IO;
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
                groupMembershipImportService, taxRuleImportService);
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
    }
}