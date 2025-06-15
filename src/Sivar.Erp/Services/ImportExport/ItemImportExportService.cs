using Sivar.Erp.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sivar.Erp.Services.ImportExport
{
    /// <summary>
    /// Implementation of item import/export service
    /// </summary>
    public class ItemImportExportService : IItemImportExportService
    {
        private readonly ItemValidator _itemValidator;

        /// <summary>
        /// Initializes a new instance of the ItemImportExportService class
        /// </summary>
        public ItemImportExportService()
        {
            _itemValidator = new ItemValidator();
        }

        /// <summary>
        /// Initializes a new instance of the ItemImportExportService class with a custom validator
        /// </summary>
        /// <param name="itemValidator">Custom item validator</param>
        public ItemImportExportService(ItemValidator itemValidator)
        {
            _itemValidator = itemValidator ?? new ItemValidator();
        }

        /// <summary>
        /// Imports items from a CSV file
        /// </summary>
        /// <param name="csvContent">Content of the CSV file as a string</param>
        /// <param name="userName">User performing the operation</param>
        /// <returns>Collection of imported items and any validation errors</returns>
        public Task<(IEnumerable<IItem> ImportedItems, IEnumerable<string> Errors)> ImportFromCsvAsync(string csvContent, string userName)
        {
            List<ItemDto> importedItems = new List<ItemDto>();
            List<string> errors = new List<string>();

            if (string.IsNullOrEmpty(csvContent))
            {
                errors.Add("CSV content is empty");
                return Task.FromResult<(IEnumerable<IItem>, IEnumerable<string>)>((importedItems, errors));
            }

            try
            {
                // Split the CSV into lines (preserve all lines)
                string[] lines = csvContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                if (lines.Length <= 1)
                {
                    errors.Add("CSV file contains no data rows");
                    return Task.FromResult<(IEnumerable<IItem>, IEnumerable<string>)>((importedItems, errors));
                }

                // Assume first line is header
                string[] headers = ParseCsvLine(lines[0]);

                // Validate headers
                if (!ValidateHeaders(headers, errors))
                {
                    return Task.FromResult<(IEnumerable<IItem>, IEnumerable<string>)>((importedItems, errors));
                }

                // Process data rows
                for (int i = 1; i < lines.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(lines[i])) continue; // Skip empty lines
                    string[] fields = ParseCsvLine(lines[i]);

                    if (fields.Length != headers.Length)
                    {
                        errors.Add($"Line {i + 1}: Column count mismatch. Expected {headers.Length}, got {fields.Length}");
                        continue;
                    }

                    var item = CreateItemFromCsvFields(headers, fields);

                    // Validate item
                    if (!_itemValidator.ValidateItem(item))
                    {
                        errors.Add($"Line {i + 1}: Item validation failed for item {item.Code}");
                        continue;
                    }

                    importedItems.Add(item);
                }

                return Task.FromResult<(IEnumerable<IItem>, IEnumerable<string>)>((importedItems, errors));
            }
            catch (Exception ex)
            {
                errors.Add($"Error importing CSV: {ex.Message}");
                return Task.FromResult<(IEnumerable<IItem>, IEnumerable<string>)>((importedItems, errors));
            }
        }

        /// <summary>
        /// Exports items to a CSV format
        /// </summary>
        /// <param name="items">Items to export</param>
        /// <returns>CSV content as a string</returns>
        public Task<string> ExportToCsvAsync(IEnumerable<IItem> items)
        {
            if (items == null || !items.Any())
            {
                return Task.FromResult(GetCsvHeader());
            }

            StringBuilder csvBuilder = new StringBuilder();

            // Add header
            csvBuilder.AppendLine(GetCsvHeader());

            // Add data rows
            foreach (var item in items)
            {
                csvBuilder.AppendLine(GetCsvRow(item));
            }

            return Task.FromResult(csvBuilder.ToString());
        }

        /// <summary>
        /// Parses a CSV line into fields, handling quoted values
        /// </summary>
        /// <param name="line">CSV line to parse</param>
        /// <returns>Array of fields</returns>
        private string[] ParseCsvLine(string line)
        {
            List<string> fields = new List<string>();
            bool inQuotes = false;
            int startIndex = 0;

            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (line[i] == ',' && !inQuotes)
                {
                    fields.Add(line.Substring(startIndex, i - startIndex).Trim().TrimStart('"').TrimEnd('"'));
                    startIndex = i + 1;
                }
            }

            // Add the last field
            fields.Add(line.Substring(startIndex).Trim().TrimStart('"').TrimEnd('"'));

            return fields.ToArray();
        }

        /// <summary>
        /// Validates CSV headers for required fields
        /// </summary>
        /// <param name="headers">Array of header names</param>
        /// <param name="errors">Collection to add any validation errors to</param>
        /// <returns>True if headers are valid, false otherwise</returns>
        private bool ValidateHeaders(string[] headers, List<string> errors)
        {
            // Define required headers
            string[] requiredHeaders = { "Code", "Type", "Description", "BasePrice" };

            foreach (var requiredHeader in requiredHeaders)
            {
                if (!headers.Contains(requiredHeader, StringComparer.OrdinalIgnoreCase))
                {
                    errors.Add($"Required header '{requiredHeader}' is missing");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Creates an item from CSV fields
        /// </summary>
        /// <param name="headers">CSV header fields</param>
        /// <param name="fields">CSV data fields</param>
        /// <returns>New item with populated properties</returns>
        private ItemDto CreateItemFromCsvFields(string[] headers, string[] fields)
        {
            var item = new ItemDto
            {
                Oid = Guid.NewGuid()
            };

            for (int i = 0; i < headers.Length; i++)
            {
                string value = fields[i];

                switch (headers[i].ToLowerInvariant())
                {
                    case "code":
                        item.Code = value;
                        break;
                    case "type":
                        item.Type = value;
                        break;
                    case "description":
                        item.Description = value;
                        break;
                    case "baseprice":
                        if (decimal.TryParse(value, out var basePrice))
                        {
                            item.BasePrice = basePrice;
                        }
                        else
                        {
                            // Default to 0 if invalid
                            item.BasePrice = 0;
                        }
                        break;
                }
            }

            return item;
        }

        /// <summary>
        /// Gets the CSV header row
        /// </summary>
        /// <returns>CSV header as a string</returns>
        private string GetCsvHeader()
        {
            return "Code,Type,Description,BasePrice";
        }

        /// <summary>
        /// Gets a CSV row for an item
        /// </summary>
        /// <param name="item">Item to convert to CSV</param>
        /// <returns>CSV row as a string</returns>
        private string GetCsvRow(IItem item)
        {
            // Add quotes around fields that might contain commas
            return $"\"{item.Code}\",\"{item.Type}\",\"{item.Description}\",{item.BasePrice}";
        }
    }
}