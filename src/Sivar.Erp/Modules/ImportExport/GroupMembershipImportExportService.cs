using Sivar.Erp.Services.Taxes.TaxGroup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sivar.Erp.Services.ImportExport
{
    /// <summary>
    /// Implementation of group membership import/export service
    /// </summary>
    public class GroupMembershipImportExportService : IGroupMembershipImportExportService
    {
        private readonly GroupMembershipValidator _membershipValidator;

        /// <summary>
        /// Initializes a new instance of the GroupMembershipImportExportService class
        /// </summary>
        public GroupMembershipImportExportService()
        {
            _membershipValidator = new GroupMembershipValidator();
        }

        /// <summary>
        /// Initializes a new instance of the GroupMembershipImportExportService class with a custom validator
        /// </summary>
        /// <param name="membershipValidator">Custom membership validator</param>
        public GroupMembershipImportExportService(GroupMembershipValidator membershipValidator)
        {
            _membershipValidator = membershipValidator ?? new GroupMembershipValidator();
        }

        /// <summary>
        /// Imports group memberships from a CSV file
        /// </summary>
        /// <param name="csvContent">Content of the CSV file as a string</param>
        /// <param name="userName">User performing the operation</param>
        /// <returns>Collection of imported memberships and any validation errors</returns>
        public Task<(IEnumerable<GroupMembershipDto> ImportedMemberships, IEnumerable<string> Errors)> ImportFromCsvAsync(string csvContent, string userName)
        {
            List<GroupMembershipDto> importedMemberships = new List<GroupMembershipDto>();
            List<string> errors = new List<string>();

            if (string.IsNullOrEmpty(csvContent))
            {
                errors.Add("CSV content is empty");
                return Task.FromResult<(IEnumerable<GroupMembershipDto>, IEnumerable<string>)>((importedMemberships, errors));
            }

            try
            {
                // Split the CSV into lines (preserve all lines)
                string[] lines = csvContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                if (lines.Length <= 1)
                {
                    errors.Add("CSV file contains no data rows");
                    return Task.FromResult<(IEnumerable<GroupMembershipDto>, IEnumerable<string>)>((importedMemberships, errors));
                }

                // Assume first line is header
                string[] headers = ParseCsvLine(lines[0]);

                // Validate headers
                if (!ValidateHeaders(headers, errors))
                {
                    return Task.FromResult<(IEnumerable<GroupMembershipDto>, IEnumerable<string>)>((importedMemberships, errors));
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

                    var membership = CreateMembershipFromCsvFields(headers, fields);

                    // Validate membership
                    if (!_membershipValidator.ValidateMembership(membership))
                    {
                        errors.Add($"Line {i + 1}: Membership validation failed for group {membership.GroupId} and entity {membership.EntityId}");
                        continue;
                    }

                    importedMemberships.Add(membership);
                }

                return Task.FromResult<(IEnumerable<GroupMembershipDto>, IEnumerable<string>)>((importedMemberships, errors));
            }
            catch (Exception ex)
            {
                errors.Add($"Error importing CSV: {ex.Message}");
                return Task.FromResult<(IEnumerable<GroupMembershipDto>, IEnumerable<string>)>((importedMemberships, errors));
            }
        }

        /// <summary>
        /// Exports memberships to a CSV format
        /// </summary>
        /// <param name="memberships">Memberships to export</param>
        /// <returns>CSV content as a string</returns>
        public Task<string> ExportToCsvAsync(IEnumerable<GroupMembershipDto> memberships)
        {
            if (memberships == null || !memberships.Any())
            {
                return Task.FromResult(GetCsvHeader());
            }

            StringBuilder csvBuilder = new StringBuilder();

            // Add header
            csvBuilder.AppendLine(GetCsvHeader());

            // Add data rows
            foreach (var membership in memberships)
            {
                csvBuilder.AppendLine(GetCsvRow(membership));
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
            string[] requiredHeaders = { "GroupId", "EntityId", "GroupType" };

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
        /// Creates a membership from CSV fields
        /// </summary>
        /// <param name="headers">CSV header fields</param>
        /// <param name="fields">CSV data fields</param>
        /// <returns>New membership with populated properties</returns>
        private GroupMembershipDto CreateMembershipFromCsvFields(string[] headers, string[] fields)
        {
            var membership = new GroupMembershipDto
            {
                Oid = Guid.NewGuid() // Generate a new ID for imported memberships
            };

            for (int i = 0; i < headers.Length; i++)
            {
                string value = fields[i];

                switch (headers[i].ToLowerInvariant())
                {
                    case "oid":
                        if (Guid.TryParse(value, out var oid))
                        {
                            membership.Oid = oid;
                        }
                        break;
                    case "groupid":
                        membership.GroupId = value;
                        break;
                    case "entityid":
                        membership.EntityId = value;
                        break;
                    case "grouptype":
                        if (Enum.TryParse<GroupType>(value, true, out var groupType))
                        {
                            membership.GroupType = groupType;
                        }
                        break;
                }
            }

            return membership;
        }

        /// <summary>
        /// Gets the CSV header row
        /// </summary>
        /// <returns>CSV header as a string</returns>
        private string GetCsvHeader()
        {
            return "Oid,GroupId,EntityId,GroupType";
        }

        /// <summary>
        /// Gets a CSV row for a membership
        /// </summary>
        /// <param name="membership">Membership to convert to CSV</param>
        /// <returns>CSV row as a string</returns>
        private string GetCsvRow(GroupMembershipDto membership)
        {
            return $"{membership.Oid},\"{membership.GroupId}\",\"{membership.EntityId}\",{membership.GroupType}";
        }
    }
}