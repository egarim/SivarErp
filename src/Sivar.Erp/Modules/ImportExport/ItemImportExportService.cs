using Sivar.Erp.Modules.Inventory.Core.Interfaces;
using System.Text;

namespace Sivar.Erp.Modules.ImportExport
{
    /// <summary>
    /// Implementation of item import/export service
    /// </summary>
    public class ItemImportExportService : IItemImportExportService
    {
        /// <summary>
        /// Initializes a new instance of the ItemImportExportService class
        /// </summary>
        public ItemImportExportService()
        {
        }

        /// <summary>
        /// Imports items from a CSV file
        /// </summary>
        /// <param name="csvContent">Content of the CSV file as a string</param>
        /// <param name="userName">User performing the operation</param>
        /// <returns>Collection of imported items and any validation errors</returns>
        public async Task<(IEnumerable<IItem> ImportedItems, IEnumerable<string> Errors)> ImportFromCsvAsync(string csvContent, string userName)
        {
            var importedItems = new List<IItem>();
            var errors = new List<string>();

            try
            {
                // Validate CSV content
                if (string.IsNullOrWhiteSpace(csvContent))
                {
                    errors.Add("CSV content is empty");
                    return (importedItems, errors);
                }

                var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length < 2)
                {
                    errors.Add("CSV must contain at least a header row and one data row");
                    return (importedItems, errors);
                }

                // Parse header
                var header = lines[0].Split(',').Select(h => h.Trim('"', ' ')).ToArray();
                var expectedColumns = new[] { "Name", "Code", "Type", "Description", "BasePrice", "IsActive", "ItemCategoryName", "UnitOfMeasureName" };
                
                // Validate required columns
                foreach (var column in expectedColumns)
                {
                    if (!header.Contains(column, StringComparer.OrdinalIgnoreCase))
                    {
                        errors.Add($"Required column '{column}' not found in CSV header");
                    }
                }

                if (errors.Any())
                {
                    return (importedItems, errors);
                }

                // Get column indices
                var nameIndex = Array.FindIndex(header, h => h.Equals("Name", StringComparison.OrdinalIgnoreCase));
                var codeIndex = Array.FindIndex(header, h => h.Equals("Code", StringComparison.OrdinalIgnoreCase));
                var typeIndex = Array.FindIndex(header, h => h.Equals("Type", StringComparison.OrdinalIgnoreCase));
                var descriptionIndex = Array.FindIndex(header, h => h.Equals("Description", StringComparison.OrdinalIgnoreCase));
                var basePriceIndex = Array.FindIndex(header, h => h.Equals("BasePrice", StringComparison.OrdinalIgnoreCase));
                var isActiveIndex = Array.FindIndex(header, h => h.Equals("IsActive", StringComparison.OrdinalIgnoreCase));
                var itemCategoryNameIndex = Array.FindIndex(header, h => h.Equals("ItemCategoryName", StringComparison.OrdinalIgnoreCase));
                var unitOfMeasureNameIndex = Array.FindIndex(header, h => h.Equals("UnitOfMeasureName", StringComparison.OrdinalIgnoreCase));

                // Process data rows
                for (int i = 1; i < lines.Length; i++)
                {
                    var line = lines[i].Trim();
                    if (string.IsNullOrEmpty(line)) continue;

                    var values = line.Split(',').Select(v => v.Trim('"', ' ')).ToArray();

                    if (values.Length != header.Length)
                    {
                        errors.Add($"Row {i + 1}: Expected {header.Length} columns, found {values.Length}");
                        continue;
                    }

                    try
                    {
                        var item = new Item
                        {
                            Name = values[nameIndex],
                            Code = values[codeIndex],
                            Type = values[typeIndex],
                            Description = values[descriptionIndex],
                            BasePrice = decimal.TryParse(values[basePriceIndex], out var price) ? price : 0,
                            UnitPrice = decimal.TryParse(values[basePriceIndex], out var unitPrice) ? unitPrice : 0,
                            IsActive = bool.TryParse(values[isActiveIndex], out var active) ? active : true,
                            ItemCategoryName = values[itemCategoryNameIndex],
                            UnitOfMeasureName = values[unitOfMeasureNameIndex],
                            CreatedBy = userName,
                            CreatedAt = DateTime.UtcNow
                        };

                        // Basic validation
                        if (string.IsNullOrWhiteSpace(item.Name))
                        {
                            errors.Add($"Row {i + 1}: Item name is required");
                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(item.Code))
                        {
                            errors.Add($"Row {i + 1}: Item code is required");
                            continue;
                        }

                        importedItems.Add(item);
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Row {i + 1}: Error processing item - {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                errors.Add($"General error: {ex.Message}");
            }

            return (importedItems, errors);
        }

        /// <summary>
        /// Exports items to a CSV format
        /// </summary>
        /// <param name="items">Items to export</param>
        /// <returns>CSV content as a string</returns>
        public Task<string> ExportToCsvAsync(IEnumerable<IItem> items)
        {
            var csv = new StringBuilder();

            // Header - only include IItem interface properties
            csv.AppendLine("Code,Type,Description,BasePrice");

            // Data rows - only use IItem interface properties
            foreach (var item in items)
            {
                csv.AppendLine($"\"{item.Code}\",\"{item.Type}\",\"{item.Description}\",{item.BasePrice}");
            }

            return Task.FromResult(csv.ToString());
        }
    }

    /// <summary>
    /// Simple implementation of IItem for import operations
    /// </summary>
    internal class Item : IItem
    {
        public string Code { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        
        // Additional properties for import operations
        public string Name { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public bool IsActive { get; set; } = true;
        public string? ItemCategoryName { get; set; }
        public string? UnitOfMeasureName { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}
