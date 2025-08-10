using System.ComponentModel;
using System.Text;
using Microsoft.Extensions.Logging;
using Sivar.Erp.Core.Modules.DataImport.Models;

namespace Sivar.Erp.Core.Modules.DataImport
{
    /// <summary>
    /// Service for validating and preprocessing CSV data before import
    /// </summary>
    [Description("Service for validating and preprocessing CSV data before import")]
    public class CsvValidationService
    {
        private readonly ILogger<CsvValidationService>? _logger;

        /// <summary>
        /// Initializes a new instance of the CsvValidationService class
        /// </summary>
        /// <param name="logger">Optional logger for diagnostic information</param>
        public CsvValidationService(ILogger<CsvValidationService>? logger = null)
        {
            _logger = logger;
        }

        /// <summary>
        /// Validates CSV content structure and format
        /// </summary>
        /// <param name="csvContent">CSV content to validate</param>
        /// <param name="requiredHeaders">Required header columns</param>
        /// <param name="options">Validation options</param>
        /// <returns>Validation result with errors and warnings</returns>
        [Description("Validates CSV content structure and format")]
        public async Task<CsvValidationResult> ValidateAsync(
            string csvContent, 
            string[] requiredHeaders, 
            CsvValidationOptions? options = null)
        {
            var result = new CsvValidationResult();
            options ??= new CsvValidationOptions();

            try
            {
                if (string.IsNullOrWhiteSpace(csvContent))
                {
                    result.Errors.Add("CSV content is empty");
                    return result;
                }

                // Split into lines
                var lines = csvContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                result.TotalLines = lines.Length;

                if (lines.Length == 0)
                {
                    result.Errors.Add("CSV file contains no lines");
                    return result;
                }

                // Validate header
                await ValidateHeader(lines[0], requiredHeaders, options, result);
                
                if (result.Errors.Any())
                {
                    return result;
                }

                // Validate data lines
                await ValidateDataLines(lines.Skip(1), options, result);

                result.IsValid = !result.Errors.Any();
                result.DataRowCount = lines.Length - 1;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error validating CSV content");
                result.Errors.Add($"Validation error: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Preprocesses CSV content to clean and standardize format
        /// </summary>
        /// <param name="csvContent">Raw CSV content</param>
        /// <param name="options">Preprocessing options</param>
        /// <returns>Cleaned and standardized CSV content</returns>
        [Description("Preprocesses CSV content to clean and standardize format")]
        public async Task<string> PreprocessAsync(string csvContent, CsvPreprocessingOptions? options = null)
        {
            options ??= new CsvPreprocessingOptions();
            
            if (string.IsNullOrWhiteSpace(csvContent))
            {
                return string.Empty;
            }

            var lines = csvContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var processedLines = new List<string>();

            foreach (var line in lines)
            {
                var processedLine = await ProcessLine(line, options);
                
                if (!string.IsNullOrWhiteSpace(processedLine) || !options.RemoveEmptyLines)
                {
                    processedLines.Add(processedLine);
                }
            }

            return string.Join("\n", processedLines);
        }

        /// <summary>
        /// Detects the delimiter used in CSV content
        /// </summary>
        /// <param name="csvContent">CSV content to analyze</param>
        /// <returns>Most likely delimiter character</returns>
        [Description("Detects the delimiter used in CSV content")]
        public char DetectDelimiter(string csvContent)
        {
            if (string.IsNullOrWhiteSpace(csvContent))
            {
                return ',';
            }

            var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length == 0)
            {
                return ',';
            }

            // Common delimiters to test
            var delimiters = new[] { ',', ';', '\t', '|' };
            var delimiterCounts = new Dictionary<char, int>();

            // Use first few lines to detect delimiter
            var testLines = lines.Take(Math.Min(5, lines.Length));

            foreach (var delimiter in delimiters)
            {
                var totalCount = 0;
                var consistentCount = true;
                var expectedFieldCount = -1;

                foreach (var line in testLines)
                {
                    var fieldCount = CountFields(line, delimiter);
                    
                    if (expectedFieldCount == -1)
                    {
                        expectedFieldCount = fieldCount;
                    }
                    else if (fieldCount != expectedFieldCount)
                    {
                        consistentCount = false;
                        break;
                    }
                    
                    totalCount += fieldCount;
                }

                if (consistentCount && expectedFieldCount > 1)
                {
                    delimiterCounts[delimiter] = totalCount;
                }
            }

            // Return delimiter with highest consistent field count
            return delimiterCounts.Any() 
                ? delimiterCounts.OrderByDescending(kvp => kvp.Value).First().Key 
                : ',';
        }

        /// <summary>
        /// Generates a detailed report of CSV content structure
        /// </summary>
        /// <param name="csvContent">CSV content to analyze</param>
        /// <returns>Detailed analysis report</returns>
        [Description("Generates a detailed report of CSV content structure")]
        public async Task<CsvAnalysisReport> AnalyzeAsync(string csvContent)
        {
            var report = new CsvAnalysisReport();

            if (string.IsNullOrWhiteSpace(csvContent))
            {
                report.Summary = "CSV content is empty";
                return report;
            }

            try
            {
                var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                report.TotalLines = lines.Length;
                report.DataLines = lines.Length - 1;

                if (lines.Length > 0)
                {
                    report.DetectedDelimiter = DetectDelimiter(csvContent);
                    
                    // Analyze header
                    var headerFields = ParseCsvLine(lines[0], report.DetectedDelimiter);
                    report.ColumnCount = headerFields.Length;
                    report.Headers = headerFields;

                    // Analyze data consistency
                    var fieldCounts = new List<int>();
                    for (int i = 1; i < lines.Length; i++)
                    {
                        var fields = ParseCsvLine(lines[i], report.DetectedDelimiter);
                        fieldCounts.Add(fields.Length);
                    }

                    report.HasConsistentFieldCount = fieldCounts.All(c => c == report.ColumnCount);
                    report.MinFieldCount = fieldCounts.Any() ? fieldCounts.Min() : 0;
                    report.MaxFieldCount = fieldCounts.Any() ? fieldCounts.Max() : 0;

                    // Generate summary
                    var summary = new StringBuilder();
                    summary.AppendLine($"CSV Analysis Summary:");
                    summary.AppendLine($"- Total lines: {report.TotalLines}");
                    summary.AppendLine($"- Data lines: {report.DataLines}");
                    summary.AppendLine($"- Columns: {report.ColumnCount}");
                    summary.AppendLine($"- Delimiter: '{report.DetectedDelimiter}'");
                    summary.AppendLine($"- Consistent field count: {report.HasConsistentFieldCount}");
                    
                    if (!report.HasConsistentFieldCount)
                    {
                        summary.AppendLine($"- Field count range: {report.MinFieldCount}-{report.MaxFieldCount}");
                    }

                    report.Summary = summary.ToString();
                }
            }
            catch (Exception ex)
            {
                report.Summary = $"Analysis failed: {ex.Message}";
                _logger?.LogError(ex, "Error analyzing CSV content");
            }

            return report;
        }

        private async Task ValidateHeader(string headerLine, string[] requiredHeaders, CsvValidationOptions options, CsvValidationResult result)
        {
            var headers = ParseCsvLine(headerLine, options.Delimiter);
            result.DetectedHeaders = headers;

            if (headers.Length == 0)
            {
                result.Errors.Add("Header line is empty");
                return;
            }

            // Check for required headers
            var missingHeaders = requiredHeaders.Where(rh => 
                !headers.Any(h => string.Equals(h, rh, StringComparison.OrdinalIgnoreCase))).ToList();

            if (missingHeaders.Any())
            {
                result.Errors.Add($"Missing required headers: {string.Join(", ", missingHeaders)}");
            }

            // Check for duplicate headers
            var duplicateHeaders = headers.GroupBy(h => h.ToLowerInvariant())
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateHeaders.Any())
            {
                result.Warnings.Add($"Duplicate headers found: {string.Join(", ", duplicateHeaders)}");
            }

            await Task.CompletedTask;
        }

        private async Task ValidateDataLines(IEnumerable<string> dataLines, CsvValidationOptions options, CsvValidationResult result)
        {
            var expectedFieldCount = result.DetectedHeaders?.Length ?? 0;
            var lineNumber = 2; // Start at 2 (after header)

            foreach (var line in dataLines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    if (!options.AllowEmptyLines)
                    {
                        result.Warnings.Add($"Line {lineNumber}: Empty line found");
                    }
                    lineNumber++;
                    continue;
                }

                var fields = ParseCsvLine(line, options.Delimiter);
                
                if (fields.Length != expectedFieldCount)
                {
                    result.Errors.Add($"Line {lineNumber}: Field count mismatch. Expected {expectedFieldCount}, got {fields.Length}");
                }

                lineNumber++;
            }

            await Task.CompletedTask;
        }

        private async Task<string> ProcessLine(string line, CsvPreprocessingOptions options)
        {
            var processedLine = line;

            if (options.TrimFields)
            {
                var fields = ParseCsvLine(line, options.Delimiter);
                var trimmedFields = fields.Select(f => f.Trim()).ToArray();
                processedLine = string.Join(options.Delimiter.ToString(), trimmedFields);
            }

            if (options.RemoveComments && processedLine.TrimStart().StartsWith(options.CommentPrefix))
            {
                return string.Empty;
            }

            return await Task.FromResult(processedLine);
        }

        private string[] ParseCsvLine(string line, char delimiter)
        {
            var result = new List<string>();
            var inQuotes = false;
            var field = new StringBuilder();
            
            foreach (var c in line)
            {
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == delimiter && !inQuotes)
                {
                    result.Add(field.ToString());
                    field.Clear();
                }
                else
                {
                    field.Append(c);
                }
            }
            
            result.Add(field.ToString());
            return result.ToArray();
        }

        private int CountFields(string line, char delimiter)
        {
            return ParseCsvLine(line, delimiter).Length;
        }
    }

    /// <summary>
    /// Options for CSV validation
    /// </summary>
    [Description("Options for CSV validation")]
    public class CsvValidationOptions
    {
        /// <summary>
        /// Delimiter character to use
        /// </summary>
        [Description("Delimiter character to use")]
        public char Delimiter { get; set; } = ',';

        /// <summary>
        /// Allow empty lines in CSV
        /// </summary>
        [Description("Allow empty lines in CSV")]
        public bool AllowEmptyLines { get; set; } = true;

        /// <summary>
        /// Maximum allowed field count variance
        /// </summary>
        [Description("Maximum allowed field count variance")]
        public int MaxFieldCountVariance { get; set; } = 0;
    }

    /// <summary>
    /// Options for CSV preprocessing
    /// </summary>
    [Description("Options for CSV preprocessing")]
    public class CsvPreprocessingOptions
    {
        /// <summary>
        /// Delimiter character to use
        /// </summary>
        [Description("Delimiter character to use")]
        public char Delimiter { get; set; } = ',';

        /// <summary>
        /// Remove empty lines
        /// </summary>
        [Description("Remove empty lines")]
        public bool RemoveEmptyLines { get; set; } = true;

        /// <summary>
        /// Trim whitespace from fields
        /// </summary>
        [Description("Trim whitespace from fields")]
        public bool TrimFields { get; set; } = true;

        /// <summary>
        /// Remove comment lines
        /// </summary>
        [Description("Remove comment lines")]
        public bool RemoveComments { get; set; } = true;

        /// <summary>
        /// Comment line prefix
        /// </summary>
        [Description("Comment line prefix")]
        public string CommentPrefix { get; set; } = "#";
    }

    /// <summary>
    /// Result of CSV validation
    /// </summary>
    [Description("Result of CSV validation")]
    public class CsvValidationResult
    {
        /// <summary>
        /// Indicates if the CSV is valid
        /// </summary>
        [Description("Indicates if the CSV is valid")]
        public bool IsValid { get; set; }

        /// <summary>
        /// List of validation errors
        /// </summary>
        [Description("List of validation errors")]
        public List<string> Errors { get; set; } = new();

        /// <summary>
        /// List of validation warnings
        /// </summary>
        [Description("List of validation warnings")]
        public List<string> Warnings { get; set; } = new();

        /// <summary>
        /// Total number of lines in CSV
        /// </summary>
        [Description("Total number of lines in CSV")]
        public int TotalLines { get; set; }

        /// <summary>
        /// Number of data rows (excluding header)
        /// </summary>
        [Description("Number of data rows")]
        public int DataRowCount { get; set; }

        /// <summary>
        /// Detected headers from first line
        /// </summary>
        [Description("Detected headers from first line")]
        public string[]? DetectedHeaders { get; set; }
    }

    /// <summary>
    /// Detailed analysis report of CSV structure
    /// </summary>
    [Description("Detailed analysis report of CSV structure")]
    public class CsvAnalysisReport
    {
        /// <summary>
        /// Summary of the analysis
        /// </summary>
        [Description("Summary of the analysis")]
        public string Summary { get; set; } = string.Empty;

        /// <summary>
        /// Total number of lines
        /// </summary>
        [Description("Total number of lines")]
        public int TotalLines { get; set; }

        /// <summary>
        /// Number of data lines
        /// </summary>
        [Description("Number of data lines")]
        public int DataLines { get; set; }

        /// <summary>
        /// Number of columns
        /// </summary>
        [Description("Number of columns")]
        public int ColumnCount { get; set; }

        /// <summary>
        /// Detected delimiter character
        /// </summary>
        [Description("Detected delimiter character")]
        public char DetectedDelimiter { get; set; }

        /// <summary>
        /// Header column names
        /// </summary>
        [Description("Header column names")]
        public string[] Headers { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Indicates if all lines have consistent field count
        /// </summary>
        [Description("Indicates if all lines have consistent field count")]
        public bool HasConsistentFieldCount { get; set; }

        /// <summary>
        /// Minimum field count found
        /// </summary>
        [Description("Minimum field count found")]
        public int MinFieldCount { get; set; }

        /// <summary>
        /// Maximum field count found
        /// </summary>
        [Description("Maximum field count found")]
        public int MaxFieldCount { get; set; }
    }
}