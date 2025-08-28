using DevExpress.ExpressApp;
using DevExpress.Data.Filtering;
using Microsoft.Extensions.Logging;
using Sivar.Erp.EfCore.Entities;
using Sivar.Erp.Modules.Inventory.Core.Interfaces;
using Sivar.Erp.Infrastructure.ImportExport;
using System.Text;

#nullable enable

namespace Sivar.Erp.Xaf.Module.Services.ImportExport
{
    /// <summary>
    /// XAF implementation of item import/export service using IObjectSpace
    /// </summary>
    public class XafItemImportExportService : IItemImportExportService
    {
        private readonly IObjectSpace _objectSpace;
        private readonly ILogger<XafItemImportExportService>? _logger;

        /// <summary>
        /// Initializes a new instance of the XafItemImportExportService class
        /// </summary>
        /// <param name="objectSpace">XAF ObjectSpace for data operations</param>
        /// <param name="logger">Optional logger for diagnostic information</param>
        public XafItemImportExportService(IObjectSpace objectSpace, ILogger<XafItemImportExportService>? logger = null)
        {
            _objectSpace = objectSpace ?? throw new ArgumentNullException(nameof(objectSpace));
            _logger = logger;
        }

        /// <summary>
        /// Imports items from a CSV file using XAF ObjectSpace
        /// </summary>
        /// <param name="csvContent">Content of the CSV file as a string</param>
        /// <param name="userName">User performing the operation</param>
        /// <returns>Collection of imported items and any validation errors</returns>
        public Task<(IEnumerable<IItem> ImportedItems, IEnumerable<string> Errors)> ImportFromCsvAsync(string csvContent, string userName)
        {
            var importedItems = new List<IItem>();
            var errors = new List<string>();

            if (string.IsNullOrEmpty(csvContent))
            {
                errors.Add("CSV content is empty");
                return Task.FromResult<(IEnumerable<IItem>, IEnumerable<string>)>((importedItems, errors));
            }

            try
            {
                _logger?.LogInformation("Starting item import from CSV for user: {UserName}", userName);

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

                    try
                    {
                        var itemDto = CreateItemFromCsvFields(headers, fields);

                        // Validate item
                        if (!ValidateItem(itemDto, errors, i + 1))
                        {
                            continue;
                        }

                        // Check if item already exists
                        var existingItem = _objectSpace.FindObject<Item>(
                            CriteriaOperator.Parse("Code = ?", itemDto.Code));

                        Item item;
                        if (existingItem != null)
                        {
                            _logger?.LogInformation("Updating existing item with code: {Code}", itemDto.Code);
                            item = existingItem;
                        }
                        else
                        {
                            _logger?.LogInformation("Creating new item with code: {Code}", itemDto.Code);
                            item = _objectSpace.CreateObject<Item>();
                        }

                        // Update properties
                        UpdateItemFromDto(item, itemDto, userName);

                        importedItems.Add(item);
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Line {i + 1}: Error processing item - {ex.Message}");
                        _logger?.LogError(ex, "Error processing line {LineNumber} during item import", i + 1);
                    }
                }

                // Save changes to ObjectSpace
                if (importedItems.Any() && !errors.Any())
                {
                    try
                    {
                        _objectSpace.CommitChanges();
                        _logger?.LogInformation("Successfully imported {Count} items", importedItems.Count);
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Error saving changes to database: {ex.Message}");
                        _logger?.LogError(ex, "Error saving items to database");
                    }
                }

                return Task.FromResult<(IEnumerable<IItem>, IEnumerable<string>)>((importedItems, errors));
            }
            catch (Exception ex)
            {
                errors.Add($"Error importing CSV: {ex.Message}");
                _logger?.LogError(ex, "Unexpected error during item import");
                return Task.FromResult<(IEnumerable<IItem>, IEnumerable<string>)>((importedItems, errors));
            }
        }

        /// <summary>
        /// Exports items to a CSV format using XAF ObjectSpace
        /// </summary>
        /// <param name="items">Items to export</param>
        /// <returns>CSV content as a string</returns>
        public Task<string> ExportToCsvAsync(IEnumerable<IItem> items)
        {
            try
            {
                _logger?.LogInformation("Starting item export to CSV");

                if (items == null || !items.Any())
                {
                    _logger?.LogInformation("No items provided for export, returning header only");
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

                _logger?.LogInformation("Successfully exported {Count} items to CSV", items.Count());
                return Task.FromResult(csvBuilder.ToString());
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error during item export");
                throw;
            }
        }

        /// <summary>
        /// Parses a CSV line into fields, handling quoted values
        /// </summary>
        /// <param name="line">CSV line to parse</param>
        /// <returns>Array of fields</returns>
        private string[] ParseCsvLine(string line)
        {
            var fields = new List<string>();
            bool inQuotes = false;
            var field = new StringBuilder();

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        // Double quote - add single quote to field
                        field.Append('"');
                        i++; // Skip next quote
                    }
                    else
                    {
                        // Toggle quote state
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    // Field separator outside quotes
                    fields.Add(field.ToString());
                    field.Clear();
                }
                else
                {
                    field.Append(c);
                }
            }

            // Add the last field
            fields.Add(field.ToString());

            return fields.ToArray();
        }

        /// <summary>
        /// Validates CSV headers
        /// </summary>
        /// <param name="headers">Header fields</param>
        /// <param name="errors">Error collection</param>
        /// <returns>True if headers are valid</returns>
        private bool ValidateHeaders(string[] headers, List<string> errors)
        {
            var requiredHeaders = new[] { "Code", "Type", "Description", "BasePrice" };
            var missingHeaders = requiredHeaders.Where(rh => !headers.Any(h => string.Equals(h, rh, StringComparison.OrdinalIgnoreCase))).ToList();

            if (missingHeaders.Any())
            {
                errors.Add($"Missing required headers: {string.Join(", ", missingHeaders)}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Creates an ItemData object from CSV fields
        /// </summary>
        /// <param name="headers">CSV headers</param>
        /// <param name="fields">CSV field values</param>
        /// <returns>ItemData object</returns>
        private ItemData CreateItemFromCsvFields(string[] headers, string[] fields)
        {
            var item = new ItemData();

            for (int i = 0; i < headers.Length; i++)
            {
                string value = fields[i];
                if (string.IsNullOrWhiteSpace(value)) continue;

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
                        if (decimal.TryParse(value, out decimal price))
                        {
                            item.BasePrice = price;
                        }
                        break;
                }
            }

            return item;
        }

        /// <summary>
        /// Validates an item
        /// </summary>
        /// <param name="item">Item to validate</param>
        /// <param name="errors">Error collection</param>
        /// <param name="lineNumber">Line number for error reporting</param>
        /// <returns>True if item is valid</returns>
        private bool ValidateItem(ItemData item, List<string> errors, int lineNumber)
        {
            if (string.IsNullOrWhiteSpace(item.Code))
            {
                errors.Add($"Line {lineNumber}: Item code is required");
                return false;
            }

            if (string.IsNullOrWhiteSpace(item.Type))
            {
                errors.Add($"Line {lineNumber}: Item type is required");
                return false;
            }

            if (string.IsNullOrWhiteSpace(item.Description))
            {
                errors.Add($"Line {lineNumber}: Item description is required");
                return false;
            }

            if (item.BasePrice < 0)
            {
                errors.Add($"Line {lineNumber}: Base price cannot be negative");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Updates an Item entity from ItemData
        /// </summary>
        /// <param name="item">Item entity to update</param>
        /// <param name="dto">DTO with updated data</param>
        /// <param name="userName">User performing the update</param>
        private void UpdateItemFromDto(Item item, ItemData dto, string userName)
        {
            var currentTime = DateTime.UtcNow;
            
            item.Code = dto.Code;
            item.Type = dto.Type;
            item.Description = dto.Description;
            item.BasePrice = dto.BasePrice;

            // Set audit fields - XAF automatically handles the ID/Oid
            item.InsertedBy = userName;
            item.InsertedAt = currentTime;
            item.UpdatedBy = userName;
            item.UpdatedAt = currentTime;
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
            return $"\"{item.Code}\",\"{item.Type}\",\"{item.Description}\",{item.BasePrice}";
        }

        /// <summary>
        /// Internal data structure for processing item data during import
        /// </summary>
        private class ItemData
        {
            public string Code { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public decimal BasePrice { get; set; }
        }
    }
}
