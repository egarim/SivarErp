using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sivar.Erp.Core.Contracts;

namespace Sivar.Erp.Infrastructure.ImportExport
{
    /// <summary>
    /// Service for importing and exporting tax rules (.NET 9 Infrastructure)
    /// </summary>
    public interface ITaxRuleImportExportService
    {
        /// <summary>
        /// Imports tax rules from CSV content
        /// </summary>
        /// <param name="csvContent">CSV content to import</param>
        /// <param name="userName">User performing the import</param>
        /// <returns>Imported tax rules and any validation errors</returns>
        Task<(IEnumerable<ITaxRule> ImportedTaxRules, IEnumerable<string> Errors)> ImportFromCsvAsync(string csvContent, string userName);
        
        /// <summary>
        /// Exports tax rules to CSV format
        /// </summary>
        /// <param name="taxRules">Tax rules to export</param>
        /// <returns>CSV content</returns>
        Task<string> ExportToCsvAsync(IEnumerable<ITaxRule> taxRules);
    }
}