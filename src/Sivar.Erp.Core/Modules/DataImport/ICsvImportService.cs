using System.ComponentModel;
using Sivar.Erp.Core.Modules.DataImport.Models;

namespace Sivar.Erp.Core.Modules.DataImport
{
    /// <summary>
    /// Generic service for CSV data import operations
    /// </summary>
    [Description("Generic service for CSV data import operations")]
    public interface ICsvImportService
    {
        /// <summary>
        /// Imports entities from CSV content with validation
        /// </summary>
        /// <typeparam name="T">The type of entity to import</typeparam>
        /// <param name="csvContent">The CSV content as a string</param>
        /// <param name="userName">The user performing the import</param>
        /// <param name="options">Optional import options</param>
        /// <returns>Result containing imported entities and any errors</returns>
        [Description("Imports entities from CSV content with validation")]
        Task<CsvImportResult<T>> ImportFromCsvAsync<T>(
            string csvContent,
            string userName,
            CsvImportOptions<T>? options = null) where T : class;
    }
}