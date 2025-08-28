using DevExpress.ExpressApp;
using DevExpress.Data.Filtering;
using Microsoft.Extensions.Logging;
using Sivar.Erp.EfCore.Entities;
using Sivar.Erp.Modules.Taxes.TaxGroup;
using Sivar.Erp.Infrastructure.ImportExport;
using System.Text;

#nullable enable

namespace Sivar.Erp.Xaf.Module.Services.ImportExport
{
    /// <summary>
    /// XAF implementation of group membership import/export service using IObjectSpace
    /// </summary>
    public class XafGroupMembershipImportExportService : IGroupMembershipImportExportService
    {
        private readonly IObjectSpace _objectSpace;
        private readonly ILogger<XafGroupMembershipImportExportService>? _logger;

        /// <summary>
        /// Initializes a new instance of the XafGroupMembershipImportExportService class
        /// </summary>
        /// <param name="objectSpace">XAF ObjectSpace for data operations</param>
        /// <param name="logger">Optional logger for diagnostic information</param>
        public XafGroupMembershipImportExportService(IObjectSpace objectSpace, ILogger<XafGroupMembershipImportExportService>? logger = null)
        {
            _objectSpace = objectSpace ?? throw new ArgumentNullException(nameof(objectSpace));
            _logger = logger;
        }

        /// <summary>
        /// Imports group memberships from a CSV file using XAF ObjectSpace
        /// </summary>
        /// <param name="csvContent">Content of the CSV file as a string</param>
        /// <param name="userName">User performing the operation</param>
        /// <returns>Collection of imported group memberships and any validation errors</returns>
        public Task<(IEnumerable<IGroupMembership> ImportedGroupMemberships, IEnumerable<string> Errors)> ImportFromCsvAsync(string csvContent, string userName)
        {
            var importedMemberships = new List<IGroupMembership>();
            var errors = new List<string>();

            if (string.IsNullOrEmpty(csvContent))
            {
                errors.Add("CSV content is empty");
                return Task.FromResult<(IEnumerable<IGroupMembership>, IEnumerable<string>)>((importedMemberships, errors));
            }

            try
            {
                _logger?.LogInformation("Starting group membership import from CSV for user: {UserName}", userName);

                // Split the CSV into lines (preserve all lines)
                string[] lines = csvContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                if (lines.Length <= 1)
                {
                    errors.Add("CSV file contains no data rows");
                    return Task.FromResult<(IEnumerable<IGroupMembership>, IEnumerable<string>)>((importedMemberships, errors));
                }

                // Assume first line is header
                string[] headers = ParseCsvLine(lines[0]);

                // Validate headers
                if (!ValidateHeaders(headers, errors))
                {
                    return Task.FromResult<(IEnumerable<IGroupMembership>, IEnumerable<string>)>((importedMemberships, errors));
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
                        var membershipDto = CreateMembershipFromCsvFields(headers, fields);

                        // Validate membership
                        if (!ValidateMembership(membershipDto, errors, i + 1))
                        {
                            continue;
                        }

                        // Check if membership already exists
                        var existingMembership = _objectSpace.FindObject<GroupMembership>(
                            CriteriaOperator.Parse("GroupCode = ? AND EntityId = ? AND GroupType = ?", 
                                membershipDto.GroupCode, membershipDto.EntityId, membershipDto.GroupType));

                        GroupMembership membership;
                        if (existingMembership != null)
                        {
                            _logger?.LogInformation("Updating existing membership for GroupCode: {GroupCode}, EntityId: {EntityId}", 
                                membershipDto.GroupCode, membershipDto.EntityId);
                            membership = existingMembership;
                        }
                        else
                        {
                            _logger?.LogInformation("Creating new membership for GroupCode: {GroupCode}, EntityId: {EntityId}", 
                                membershipDto.GroupCode, membershipDto.EntityId);
                            membership = _objectSpace.CreateObject<GroupMembership>();
                        }

                        // Update properties
                        UpdateMembershipFromDto(membership, membershipDto, userName);

                        importedMemberships.Add(membership);
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Line {i + 1}: Error processing group membership - {ex.Message}");
                        _logger?.LogError(ex, "Error processing line {LineNumber} during group membership import", i + 1);
                    }
                }

                // Save changes to ObjectSpace
                if (importedMemberships.Any() && !errors.Any())
                {
                    try
                    {
                        _objectSpace.CommitChanges();
                        _logger?.LogInformation("Successfully imported {Count} group memberships", importedMemberships.Count);
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Error saving changes to database: {ex.Message}");
                        _logger?.LogError(ex, "Error saving group memberships to database");
                    }
                }

                return Task.FromResult<(IEnumerable<IGroupMembership>, IEnumerable<string>)>((importedMemberships, errors));
            }
            catch (Exception ex)
            {
                errors.Add($"Error importing CSV: {ex.Message}");
                _logger?.LogError(ex, "Unexpected error during group membership import");
                return Task.FromResult<(IEnumerable<IGroupMembership>, IEnumerable<string>)>((importedMemberships, errors));
            }
        }

        /// <summary>
        /// Exports group memberships to a CSV format using XAF ObjectSpace
        /// </summary>
        /// <param name="groupMemberships">Group memberships to export</param>
        /// <returns>CSV content as a string</returns>
        public Task<string> ExportToCsvAsync(IEnumerable<IGroupMembership> groupMemberships)
        {
            try
            {
                _logger?.LogInformation("Starting group membership export to CSV");

                if (groupMemberships == null || !groupMemberships.Any())
                {
                    _logger?.LogInformation("No group memberships provided for export, returning header only");
                    return Task.FromResult(GetCsvHeader());
                }

                StringBuilder csvBuilder = new StringBuilder();

                // Add header
                csvBuilder.AppendLine(GetCsvHeader());

                // Add data rows
                foreach (var membership in groupMemberships)
                {
                    csvBuilder.AppendLine(GetCsvRow(membership));
                }

                _logger?.LogInformation("Successfully exported {Count} group memberships to CSV", groupMemberships.Count());
                return Task.FromResult(csvBuilder.ToString());
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error during group membership export");
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
            var requiredHeaders = new[] { "GroupCode", "EntityId", "GroupType" };
            var missingHeaders = requiredHeaders.Where(rh => !headers.Any(h => string.Equals(h, rh, StringComparison.OrdinalIgnoreCase))).ToList();

            if (missingHeaders.Any())
            {
                errors.Add($"Missing required headers: {string.Join(", ", missingHeaders)}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Creates a GroupMembershipData object from CSV fields
        /// </summary>
        /// <param name="headers">CSV headers</param>
        /// <param name="fields">CSV field values</param>
        /// <returns>GroupMembershipData object</returns>
        private GroupMembershipData CreateMembershipFromCsvFields(string[] headers, string[] fields)
        {
            var membership = new GroupMembershipData();

            for (int i = 0; i < headers.Length; i++)
            {
                string value = fields[i];
                if (string.IsNullOrWhiteSpace(value)) continue;

                switch (headers[i].ToLowerInvariant())
                {
                    case "groupcode":
                        membership.GroupCode = value;
                        break;
                    case "entityid":
                        membership.EntityId = value;
                        break;
                    case "grouptype":
                        if (Enum.TryParse<GroupType>(value, true, out GroupType groupType))
                        {
                            membership.GroupType = groupType;
                        }
                        break;
                }
            }

            return membership;
        }

        /// <summary>
        /// Validates a group membership
        /// </summary>
        /// <param name="membership">Membership to validate</param>
        /// <param name="errors">Error collection</param>
        /// <param name="lineNumber">Line number for error reporting</param>
        /// <returns>True if membership is valid</returns>
        private bool ValidateMembership(GroupMembershipData membership, List<string> errors, int lineNumber)
        {
            if (string.IsNullOrWhiteSpace(membership.GroupCode))
            {
                errors.Add($"Line {lineNumber}: Group code is required");
                return false;
            }

            if (string.IsNullOrWhiteSpace(membership.EntityId))
            {
                errors.Add($"Line {lineNumber}: Entity ID is required");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Updates a GroupMembership entity from GroupMembershipData
        /// </summary>
        /// <param name="membership">GroupMembership entity to update</param>
        /// <param name="dto">DTO with updated data</param>
        /// <param name="userName">User performing the update</param>
        private void UpdateMembershipFromDto(GroupMembership membership, GroupMembershipData dto, string userName)
        {
            var currentTime = DateTime.UtcNow;
            
            membership.GroupCode = dto.GroupCode;
            membership.EntityId = dto.EntityId;
            membership.GroupType = dto.GroupType;

            // Set audit fields - XAF automatically handles the ID/Oid
            membership.InsertedBy = userName;
            membership.InsertedAt = currentTime;
            membership.UpdatedBy = userName;
            membership.UpdatedAt = currentTime;
        }

        /// <summary>
        /// Gets the CSV header row
        /// </summary>
        /// <returns>CSV header as a string</returns>
        private string GetCsvHeader()
        {
            return "GroupCode,EntityId,GroupType";
        }

        /// <summary>
        /// Gets a CSV row for a group membership
        /// </summary>
        /// <param name="membership">Group membership to convert to CSV</param>
        /// <returns>CSV row as a string</returns>
        private string GetCsvRow(IGroupMembership membership)
        {
            return $"\"{membership.GroupCode}\",\"{membership.EntityId}\",\"{membership.GroupType}\"";
        }

        /// <summary>
        /// Internal data structure for processing group membership data during import
        /// </summary>
        private class GroupMembershipData
        {
            public string GroupCode { get; set; } = string.Empty;
            public string EntityId { get; set; } = string.Empty;
            public GroupType GroupType { get; set; }
        }
    }
}
