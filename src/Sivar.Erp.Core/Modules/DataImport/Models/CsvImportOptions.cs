using System.ComponentModel;

namespace Sivar.Erp.Core.Modules.DataImport.Models
{
    /// <summary>
    /// Options for CSV import operations
    /// </summary>
    /// <typeparam name="T">The type of entity being imported</typeparam>
    [Description("Options for CSV import operations")]
    public class CsvImportOptions<T>
    {
        /// <summary>
        /// Custom field mapping function (CSV field name to property name)
        /// </summary>
        public Func<string, string>? FieldMapper { get; set; }

        /// <summary>
        /// Custom value converter function (field name, string value to typed value)
        /// </summary>
        public Func<string, string, object?>? ValueConverter { get; set; }

        /// <summary>
        /// Custom validator function
        /// </summary>
        public Func<T, IEnumerable<string>>? Validator { get; set; }

        /// <summary>
        /// Optional header row index (default is 0)
        /// </summary>
        public int HeaderRowIndex { get; set; } = 0;

        /// <summary>
        /// Whether to skip empty rows (default is true)
        /// </summary>
        public bool SkipEmptyRows { get; set; } = true;

        /// <summary>
        /// Whether to continue on error (default is false)
        /// </summary>
        public bool ContinueOnError { get; set; } = false;

        /// <summary>
        /// Character used as field delimiter (default is comma)
        /// </summary>
        public char Delimiter { get; set; } = ',';
    }
}