using System.ComponentModel;
using Sivar.Erp.Core.Core;
using Sivar.Erp.Core.Modules.DataImport.Models;

namespace Sivar.Erp.Core.Modules.DataImport
{
    /// <summary>
    /// Service for managing data import operations including CSV and other formats
    /// </summary>
    [Description("Service for managing data import operations")]
    public interface IDataImportService
    {
        /// <summary>
        /// Imports data from multiple sources in a batch operation
        /// </summary>
        /// <param name="importRequests">Collection of import requests</param>
        /// <param name="userName">User performing the import</param>
        /// <returns>Batch import result</returns>
        [Description("Imports data from multiple sources in a batch operation")]
        Task<BatchImportResult> ImportBatchAsync(IEnumerable<ImportRequest> importRequests, string userName);

        /// <summary>
        /// Imports data from embedded test data resources
        /// </summary>
        /// <param name="dataSet">Name of the data set to import (e.g., 'ElSalvador')</param>
        /// <param name="userName">User performing the import</param>
        /// <returns>Import result summary</returns>
        [Description("Imports data from embedded test data resources")]
        Task<DataSetImportResult> ImportTestDataSetAsync(string dataSet, string userName);

        /// <summary>
        /// Validates import data before actual import
        /// </summary>
        /// <param name="importRequest">Import request to validate</param>
        /// <returns>Validation result</returns>
        [Description("Validates import data before actual import")]
        Task<ImportValidationResult> ValidateImportAsync(ImportRequest importRequest);

        /// <summary>
        /// Gets import history for auditing purposes
        /// </summary>
        /// <param name="fromDate">Start date for history</param>
        /// <param name="toDate">End date for history</param>
        /// <param name="userName">Optional user filter</param>
        /// <returns>Collection of import history records</returns>
        [Description("Gets import history for auditing purposes")]
        Task<IEnumerable<ImportHistory>> GetImportHistoryAsync(DateTime fromDate, DateTime toDate, string? userName = null);

        /// <summary>
        /// Creates an import template for a specific entity type
        /// </summary>
        /// <typeparam name="T">Entity type to create template for</typeparam>
        /// <returns>CSV template with headers and sample data</returns>
        [Description("Creates an import template for a specific entity type")]
        Task<string> CreateImportTemplateAsync<T>() where T : class;

        /// <summary>
        /// Exports data to CSV format
        /// </summary>
        /// <typeparam name="T">Entity type to export</typeparam>
        /// <param name="filter">Optional filter criteria</param>
        /// <returns>CSV content</returns>
        [Description("Exports data to CSV format")]
        Task<string> ExportToCsvAsync<T>(Func<T, bool>? filter = null) where T : class;
    }

    /// <summary>
    /// Represents an import request
    /// </summary>
    [Description("Represents an import request")]
    public class ImportRequest
    {
        /// <summary>
        /// Unique identifier for the import request
        /// </summary>
        [Description("Unique identifier for the import request")]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Type of entity being imported
        /// </summary>
        [Description("Type of entity being imported")]
        public Type EntityType { get; set; } = typeof(object);

        /// <summary>
        /// Source of the data (CSV content, file path, etc.)
        /// </summary>
        [Description("Source of the data")]
        public string DataSource { get; set; } = string.Empty;

        /// <summary>
        /// Import options specific to the entity type
        /// </summary>
        [Description("Import options specific to the entity type")]
        public Dictionary<string, object> Options { get; set; } = new();

        /// <summary>
        /// Indicates if the import should validate only (no actual import)
        /// </summary>
        [Description("Indicates if the import should validate only")]
        public bool ValidateOnly { get; set; }

        /// <summary>
        /// Priority of the import request
        /// </summary>
        [Description("Priority of the import request")]
        public ImportPriority Priority { get; set; } = ImportPriority.Normal;
    }

    /// <summary>
    /// Result of a batch import operation
    /// </summary>
    [Description("Result of a batch import operation")]
    public class BatchImportResult
    {
        /// <summary>
        /// Overall success status
        /// </summary>
        [Description("Overall success status")]
        public bool Success { get; set; }

        /// <summary>
        /// Collection of individual import results
        /// </summary>
        [Description("Collection of individual import results")]
        public IList<ImportResult> ImportResults { get; set; } = new List<ImportResult>();

        /// <summary>
        /// Total entities processed across all imports
        /// </summary>
        [Description("Total entities processed across all imports")]
        public int TotalProcessed { get; set; }

        /// <summary>
        /// Total entities successfully imported
        /// </summary>
        [Description("Total entities successfully imported")]
        public int TotalImported { get; set; }

        /// <summary>
        /// Total time taken for the batch import
        /// </summary>
        [Description("Total time taken for the batch import")]
        public TimeSpan TotalDuration { get; set; }

        /// <summary>
        /// Collection of errors that occurred during the batch import
        /// </summary>
        [Description("Collection of errors that occurred")]
        public IList<string> Errors { get; set; } = new List<string>();
    }

    /// <summary>
    /// Result of importing a test data set
    /// </summary>
    [Description("Result of importing a test data set")]
    public class DataSetImportResult
    {
        /// <summary>
        /// Name of the data set imported
        /// </summary>
        [Description("Name of the data set imported")]
        public string DataSetName { get; set; } = string.Empty;

        /// <summary>
        /// Overall success status
        /// </summary>
        [Description("Overall success status")]
        public bool Success { get; set; }

        /// <summary>
        /// Collection of import results by entity type
        /// </summary>
        [Description("Collection of import results by entity type")]
        public Dictionary<string, ImportResult> Results { get; set; } = new();

        /// <summary>
        /// Total time taken for the data set import
        /// </summary>
        [Description("Total time taken for the data set import")]
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Summary of what was imported
        /// </summary>
        [Description("Summary of what was imported")]
        public string Summary { get; set; } = string.Empty;
    }

    /// <summary>
    /// Generic import result
    /// </summary>
    [Description("Generic import result")]
    public class ImportResult
    {
        /// <summary>
        /// Success status of the import
        /// </summary>
        [Description("Success status of the import")]
        public bool Success { get; set; }

        /// <summary>
        /// Type of entity that was imported
        /// </summary>
        [Description("Type of entity that was imported")]
        public string EntityType { get; set; } = string.Empty;

        /// <summary>
        /// Number of entities processed
        /// </summary>
        [Description("Number of entities processed")]
        public int TotalProcessed { get; set; }

        /// <summary>
        /// Number of entities successfully imported
        /// </summary>
        [Description("Number of entities successfully imported")]
        public int TotalImported { get; set; }

        /// <summary>
        /// Collection of errors that occurred
        /// </summary>
        [Description("Collection of errors that occurred")]
        public IList<string> Errors { get; set; } = new List<string>();

        /// <summary>
        /// Collection of warnings
        /// </summary>
        [Description("Collection of warnings")]
        public IList<string> Warnings { get; set; } = new List<string>();

        /// <summary>
        /// Time taken for the import
        /// </summary>
        [Description("Time taken for the import")]
        public TimeSpan Duration { get; set; }
    }

    /// <summary>
    /// Result of import validation
    /// </summary>
    [Description("Result of import validation")]
    public class ImportValidationResult
    {
        /// <summary>
        /// Indicates if the data is valid for import
        /// </summary>
        [Description("Indicates if the data is valid for import")]
        public bool IsValid { get; set; }

        /// <summary>
        /// Collection of validation errors
        /// </summary>
        [Description("Collection of validation errors")]
        public IList<string> Errors { get; set; } = new List<string>();

        /// <summary>
        /// Collection of validation warnings
        /// </summary>
        [Description("Collection of validation warnings")]
        public IList<string> Warnings { get; set; } = new List<string>();

        /// <summary>
        /// Number of records that would be processed
        /// </summary>
        [Description("Number of records that would be processed")]
        public int RecordCount { get; set; }

        /// <summary>
        /// Preview of records that would be imported
        /// </summary>
        [Description("Preview of records that would be imported")]
        public IList<object> Preview { get; set; } = new List<object>();
    }

    /// <summary>
    /// Import history record for auditing
    /// </summary>
    [Description("Import history record for auditing")]
    public class ImportHistory : IEntity
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        [Description("Unique identifier")]
        public Guid Id { get; set; }

        /// <summary>
        /// Date when created
        /// </summary>
        [Description("Date when created")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Date when last updated
        /// </summary>
        [Description("Date when last updated")]
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// User who performed the import
        /// </summary>
        [Description("User who performed the import")]
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Type of entity imported
        /// </summary>
        [Description("Type of entity imported")]
        public string EntityType { get; set; } = string.Empty;

        /// <summary>
        /// Source of the import (file name, data set name, etc.)
        /// </summary>
        [Description("Source of the import")]
        public string ImportSource { get; set; } = string.Empty;

        /// <summary>
        /// Number of records processed
        /// </summary>
        [Description("Number of records processed")]
        public int RecordsProcessed { get; set; }

        /// <summary>
        /// Number of records successfully imported
        /// </summary>
        [Description("Number of records successfully imported")]
        public int RecordsImported { get; set; }

        /// <summary>
        /// Success status
        /// </summary>
        [Description("Success status")]
        public bool Success { get; set; }

        /// <summary>
        /// Error details if any
        /// </summary>
        [Description("Error details if any")]
        public string? ErrorDetails { get; set; }

        /// <summary>
        /// Duration of the import operation
        /// </summary>
        [Description("Duration of the import operation")]
        public TimeSpan Duration { get; set; }
    }

    /// <summary>
    /// Priority levels for import operations
    /// </summary>
    [Description("Priority levels for import operations")]
    public enum ImportPriority
    {
        /// <summary>
        /// Low priority import
        /// </summary>
        [Description("Low priority import")]
        Low = 1,

        /// <summary>
        /// Normal priority import
        /// </summary>
        [Description("Normal priority import")]
        Normal = 2,

        /// <summary>
        /// High priority import
        /// </summary>
        [Description("High priority import")]
        High = 3,

        /// <summary>
        /// Critical priority import
        /// </summary>
        [Description("Critical priority import")]
        Critical = 4
    }
}