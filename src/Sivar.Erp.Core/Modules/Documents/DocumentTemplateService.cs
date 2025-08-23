using Microsoft.Extensions.Logging;
using Sivar.Erp.Core.Infrastructure.Logging;
using System.ComponentModel;
using Sivar.Erp.Core.Infrastructure.Repository;
using System.Text.Json;

namespace Sivar.Erp.Core.Modules.Documents
{
    /// <summary>
    /// Document template service implementation
    /// </summary>
    [Description("Document template service")]
    public class DocumentTemplateService : IDocumentTemplateService
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly ILogger<DocumentTemplateService> _logger;

        public DocumentTemplateService(
            IDocumentRepository documentRepository,
            ILogger<DocumentTemplateService> logger)
        {
            _documentRepository = documentRepository;
            _logger = logger;
        }

        public async Task<DocumentTemplate> CreateTemplateAsync(DocumentTemplate template, string createdBy)
        {
            using var activity = LoggingExtensions.StartActivity("DocumentTemplate.Create");
            
            try
            {
                template.Id = Guid.NewGuid();
                template.CreatedBy = createdBy;
                template.CreatedAt = DateTime.UtcNow;
                template.IsActive = true;
                template.UsageCount = 0;

                // Validate template
                await ValidateTemplateAsync(template);

                // Save template
                await SaveTemplateAsync(template);

                _logger.LogInformation("Created template {TemplateName} ({TemplateId}) by {CreatedBy}", 
                    template.Name, template.Id, createdBy);

                return template;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating template {TemplateName}", template.Name);
                throw;
            }
        }

        public async Task<DocumentTemplate?> GetTemplateAsync(Guid templateId)
        {
            using var activity = LoggingExtensions.StartActivity("DocumentTemplate.Get", templateId);
            
            try
            {
                // In a real implementation, this would query templates table
                // For now, return null as placeholder
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting template {TemplateId}", templateId);
                throw;
            }
        }

        public async Task<List<DocumentTemplate>> GetTemplatesByTypeAsync(string documentTypeCode)
        {
            using var activity = LoggingExtensions.StartActivity("DocumentTemplate.GetByType");
            
            try
            {
                // In a real implementation, this would query templates by document type
                // For now, return empty list as placeholder
                return new List<DocumentTemplate>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting templates for document type {DocumentTypeCode}", documentTypeCode);
                throw;
            }
        }

        public async Task<List<DocumentTemplate>> GetActiveTemplatesAsync()
        {
            using var activity = LoggingExtensions.StartActivity("DocumentTemplate.GetActive");
            
            try
            {
                // In a real implementation, this would query active templates
                // For now, return empty list as placeholder
                return new List<DocumentTemplate>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active templates");
                throw;
            }
        }

        public async Task<Document> CreateDocumentFromTemplateAsync(Guid templateId, Dictionary<string, object> fieldValues, string createdBy)
        {
            using var activity = LoggingExtensions.StartActivity("DocumentTemplate.CreateDocument", templateId);
            
            try
            {
                var template = await GetTemplateAsync(templateId);
                if (template == null)
                    throw new InvalidOperationException($"Template {templateId} not found");

                if (!template.IsActive)
                    throw new InvalidOperationException($"Template {templateId} is not active");

                // Deserialize template data
                var templateData = JsonSerializer.Deserialize<Document>(template.TemplateData);
                if (templateData == null)
                    throw new InvalidOperationException("Failed to deserialize template data");

                // Apply field values
                var document = ApplyFieldValues(templateData, template.Fields, fieldValues);
                
                // Set document metadata
                document.Id = Guid.NewGuid();
                document.CreatedBy = createdBy;
                document.CreatedAt = DateTime.UtcNow;
                document.Status = DocumentStatus.Draft;

                // Generate document number if not provided
                if (string.IsNullOrEmpty(document.DocumentNumber))
                {
                    document.DocumentNumber = await GenerateDocumentNumberAsync(document.DocumentTypeCode);
                }

                // Save document
                await _documentRepository.CreateAsync(document);

                // Update template usage count
                template.UsageCount++;
                await UpdateTemplateAsync(template);

                _logger.LogInformation("Created document {DocumentNumber} from template {TemplateName} by {CreatedBy}", 
                    document.DocumentNumber, template.Name, createdBy);

                return document;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating document from template {TemplateId}", templateId);
                throw;
            }
        }

        public async Task<DocumentTemplate> UpdateTemplateAsync(DocumentTemplate template, string modifiedBy)
        {
            using var activity = LoggingExtensions.StartActivity("DocumentTemplate.Update", template.Id);
            
            try
            {
                template.ModifiedBy = modifiedBy;
                template.ModifiedAt = DateTime.UtcNow;

                // Validate template
                await ValidateTemplateAsync(template);

                // Update template
                await UpdateTemplateAsync(template);

                _logger.LogInformation("Updated template {TemplateName} ({TemplateId}) by {ModifiedBy}", 
                    template.Name, template.Id, modifiedBy);

                return template;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating template {TemplateId}", template.Id);
                throw;
            }
        }

        public async Task DeleteTemplateAsync(Guid templateId, string deletedBy)
        {
            using var activity = LoggingExtensions.StartActivity("DocumentTemplate.Delete", templateId);
            
            try
            {
                var template = await GetTemplateAsync(templateId);
                if (template == null)
                    throw new InvalidOperationException($"Template {templateId} not found");

                // Mark as inactive instead of deleting
                template.IsActive = false;
                template.ModifiedBy = deletedBy;
                template.ModifiedAt = DateTime.UtcNow;

                await UpdateTemplateAsync(template);

                _logger.LogInformation("Deleted template {TemplateName} ({TemplateId}) by {DeletedBy}", 
                    template.Name, templateId, deletedBy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting template {TemplateId}", templateId);
                throw;
            }
        }

        public async Task<DocumentTemplate> CloneTemplateAsync(Guid templateId, string newName, string clonedBy)
        {
            using var activity = LoggingExtensions.StartActivity("DocumentTemplate.Clone", templateId);
            
            try
            {
                var originalTemplate = await GetTemplateAsync(templateId);
                if (originalTemplate == null)
                    throw new InvalidOperationException($"Template {templateId} not found");

                var clonedTemplate = new DocumentTemplate
                {
                    Id = Guid.NewGuid(),
                    Name = newName,
                    Description = $"Cloned from {originalTemplate.Name}",
                    DocumentTypeCode = originalTemplate.DocumentTypeCode,
                    TemplateData = originalTemplate.TemplateData,
                    Fields = originalTemplate.Fields.Select(f => new TemplateField
                    {
                        Name = f.Name,
                        DisplayName = f.DisplayName,
                        FieldType = f.FieldType,
                        IsRequired = f.IsRequired,
                        DefaultValue = f.DefaultValue,
                        ValidValues = new List<string>(f.ValidValues),
                        ValidationExpression = f.ValidationExpression,
                        Description = f.Description,
                        SortOrder = f.SortOrder
                    }).ToList(),
                    IsActive = true,
                    CreatedBy = clonedBy,
                    CreatedAt = DateTime.UtcNow,
                    UsageCount = 0,
                    Category = originalTemplate.Category,
                    Tags = new List<string>(originalTemplate.Tags)
                };

                await SaveTemplateAsync(clonedTemplate);

                _logger.LogInformation("Cloned template {OriginalName} to {NewName} ({NewTemplateId}) by {ClonedBy}", 
                    originalTemplate.Name, newName, clonedTemplate.Id, clonedBy);

                return clonedTemplate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cloning template {TemplateId}", templateId);
                throw;
            }
        }

        public async Task<bool> ValidateTemplateAsync(DocumentTemplate template)
        {
            using var activity = LoggingExtensions.StartActivity("DocumentTemplate.Validate", template.Id);
            
            try
            {
                var validationErrors = new List<string>();

                // Validate basic properties
                if (string.IsNullOrWhiteSpace(template.Name))
                    validationErrors.Add("Template name is required");

                if (string.IsNullOrWhiteSpace(template.DocumentTypeCode))
                    validationErrors.Add("Document type code is required");

                if (string.IsNullOrWhiteSpace(template.TemplateData))
                    validationErrors.Add("Template data is required");

                // Validate template data is valid JSON
                try
                {
                    JsonSerializer.Deserialize<Document>(template.TemplateData);
                }
                catch (JsonException)
                {
                    validationErrors.Add("Template data is not valid JSON");
                }

                // Validate fields
                foreach (var field in template.Fields)
                {
                    if (string.IsNullOrWhiteSpace(field.Name))
                        validationErrors.Add($"Field name is required for field at position {field.SortOrder}");

                    if (string.IsNullOrWhiteSpace(field.FieldType))
                        validationErrors.Add($"Field type is required for field {field.Name}");
                }

                if (validationErrors.Any())
                {
                    var errorMessage = string.Join("; ", validationErrors);
                    throw new InvalidOperationException($"Template validation failed: {errorMessage}");
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating template {TemplateId}", template.Id);
                throw;
            }
        }

        public async Task<Dictionary<string, object>> GetTemplateStatisticsAsync()
        {
            using var activity = LoggingExtensions.StartActivity("DocumentTemplate.GetStatistics");
            
            try
            {
                var templates = await GetActiveTemplatesAsync();
                
                var stats = new Dictionary<string, object>
                {
                    ["TotalTemplates"] = templates.Count,
                    ["ActiveTemplates"] = templates.Count(t => t.IsActive),
                    ["TotalUsage"] = templates.Sum(t => t.UsageCount),
                    ["AverageUsage"] = templates.Count > 0 ? templates.Average(t => t.UsageCount) : 0,
                    ["MostUsedTemplate"] = templates.OrderByDescending(t => t.UsageCount).FirstOrDefault()?.Name ?? "None",
                    ["TemplatesByType"] = templates.GroupBy(t => t.DocumentTypeCode).ToDictionary(g => g.Key, g => g.Count()),
                    ["LastUpdated"] = DateTime.UtcNow
                };

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting template statistics");
                throw;
            }
        }

        private Document ApplyFieldValues(Document templateDocument, List<TemplateField> fields, Dictionary<string, object> fieldValues)
        {
            // Apply field values to template document based on field definitions
            foreach (var field in fields)
            {
                if (fieldValues.TryGetValue(field.Name, out var value))
                {
                    // Apply value based on field type and name
                    switch (field.Name.ToLowerInvariant())
                    {
                        case "documentnumber":
                            templateDocument.DocumentNumber = value?.ToString() ?? "";
                            break;
                        case "date":
                            if (value is DateOnly dateValue)
                                templateDocument.Date = dateValue;
                            else if (DateTime.TryParse(value?.ToString(), out var parsedDate))
                                templateDocument.Date = DateOnly.FromDateTime(parsedDate);
                            break;
                        case "businessentityid":
                            if (value is Guid guidValue)
                                templateDocument.BusinessEntityId = guidValue;
                            else if (Guid.TryParse(value?.ToString(), out var parsedGuid))
                                templateDocument.BusinessEntityId = parsedGuid;
                            break;
                        case "notes":
                            templateDocument.Notes = value?.ToString();
                            break;
                        case "totalamount":
                            if (value is decimal decimalValue)
                                templateDocument.TotalAmount = decimalValue;
                            else if (decimal.TryParse(value?.ToString(), out var parsedDecimal))
                                templateDocument.TotalAmount = parsedDecimal;
                            break;
                    }
                }
                else if (field.IsRequired && string.IsNullOrEmpty(field.DefaultValue))
                {
                    throw new InvalidOperationException($"Required field '{field.DisplayName}' is missing");
                }
                else if (!string.IsNullOrEmpty(field.DefaultValue))
                {
                    // Apply default value
                    fieldValues[field.Name] = field.DefaultValue;
                }
            }

            return templateDocument;
        }

        private async Task<string> GenerateDocumentNumberAsync(string documentTypeCode)
        {
            // Generate a unique document number based on type and current date
            var prefix = documentTypeCode.ToUpperInvariant();
            var datePrefix = DateTime.Now.ToString("yyyyMM");
            var sequence = await GetNextSequenceNumberAsync(documentTypeCode, datePrefix);
            
            return $"{prefix}-{datePrefix}-{sequence:D4}";
        }

        private async Task<int> GetNextSequenceNumberAsync(string documentTypeCode, string datePrefix)
        {
            // In a real implementation, this would query the database for the next sequence number
            // For now, return a random number as placeholder
            await Task.CompletedTask;
            return new Random().Next(1, 9999);
        }

        private async Task SaveTemplateAsync(DocumentTemplate template)
        {
            // In a real implementation, this would save to templates table
            await Task.CompletedTask;
        }

        private async Task UpdateTemplateAsync(DocumentTemplate template)
        {
            // In a real implementation, this would update template in database
            await Task.CompletedTask;
        }
    }
}
