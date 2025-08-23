using Microsoft.Extensions.Logging;
using Sivar.Erp.Core.Infrastructure.Logging;
using Sivar.Erp.Core.Infrastructure.Repositories;
using Sivar.Erp.Core.Modules.Domain;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Sivar.Erp.Core.Modules.Documents
{
    /// <summary>
    /// Document version service implementation
    /// </summary>
    [Description("Document version service")]
    public class DocumentVersionService : IDocumentVersionService
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly ILogger<DocumentVersionService> _logger;

        public DocumentVersionService(
            IDocumentRepository documentRepository,
            ILogger<DocumentVersionService> logger)
        {
            _documentRepository = documentRepository;
            _logger = logger;
        }

        public async Task<DocumentVersion> CreateVersionAsync(Guid documentId, string versionComments, string createdBy)
        {
            using var activity = LoggingExtensions.StartActivity("DocumentVersion.Create", documentId);
            
            try
            {
                var document = await _documentRepository.GetByIdAsync(documentId);
                if (document == null)
                    throw new InvalidOperationException($"Document {documentId} not found");

                // Get latest version number
                var latestVersion = await GetLatestVersionAsync(documentId);
                var newVersionNumber = latestVersion?.VersionNumber + 1 ?? 1;

                // Serialize document state
                var documentSnapshot = JsonSerializer.Serialize(document, new JsonSerializerOptions 
                { 
                    WriteIndented = false,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var documentBytes = Encoding.UTF8.GetBytes(documentSnapshot);
                var checksumHash = ComputeChecksum(documentBytes);

                var version = new DocumentVersion
                {
                    DocumentId = documentId,
                    VersionNumber = newVersionNumber,
                    VersionComments = versionComments,
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    DocumentSnapshot = documentSnapshot,
                    FileSize = documentBytes.Length,
                    ChecksumHash = checksumHash
                };

                // Deactivate previous versions
                if (latestVersion != null)
                {
                    latestVersion.IsActive = false;
                    await UpdateVersionAsync(latestVersion);
                }

                // Save new version
                await SaveVersionAsync(version);

                _logger.LogInformation("Created version {VersionNumber} for document {DocumentId}", 
                    newVersionNumber, documentId);

                return version;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating version for document {DocumentId}", documentId);
                throw;
            }
        }

        public async Task<DocumentVersion?> GetVersionAsync(Guid documentId, int versionNumber)
        {
            using var activity = LoggingExtensions.StartActivity("DocumentVersion.GetVersion", documentId);
            
            try
            {
                // In a real implementation, this would query a versions table
                // For now, return null as placeholder
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting version {VersionNumber} for document {DocumentId}", 
                    versionNumber, documentId);
                throw;
            }
        }

        public async Task<DocumentVersion?> GetLatestVersionAsync(Guid documentId)
        {
            using var activity = LoggingExtensions.StartActivity("DocumentVersion.GetLatest", documentId);
            
            try
            {
                // In a real implementation, this would query the latest version from versions table
                // For now, return null as placeholder
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting latest version for document {DocumentId}", documentId);
                throw;
            }
        }

        public async Task<List<DocumentVersion>> GetVersionHistoryAsync(Guid documentId)
        {
            using var activity = LoggingExtensions.StartActivity("DocumentVersion.GetHistory", documentId);
            
            try
            {
                // In a real implementation, this would query all versions for the document
                // For now, return empty list as placeholder
                return new List<DocumentVersion>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting version history for document {DocumentId}", documentId);
                throw;
            }
        }

        public async Task<DocumentVersionComparison> CompareVersionsAsync(Guid documentId, int fromVersion, int toVersion)
        {
            using var activity = LoggingExtensions.StartActivity("DocumentVersion.Compare", documentId);
            
            try
            {
                var fromVersionData = await GetVersionAsync(documentId, fromVersion);
                var toVersionData = await GetVersionAsync(documentId, toVersion);

                if (fromVersionData == null || toVersionData == null)
                    throw new InvalidOperationException("One or both versions not found");

                var comparison = new DocumentVersionComparison
                {
                    DocumentId = documentId,
                    FromVersion = fromVersion,
                    ToVersion = toVersion,
                    ComparedAt = DateTime.UtcNow,
                    ComparedBy = "System" // In real implementation, get from context
                };

                // Deserialize document snapshots
                var fromDoc = JsonSerializer.Deserialize<Document>(fromVersionData.DocumentSnapshot);
                var toDoc = JsonSerializer.Deserialize<Document>(toVersionData.DocumentSnapshot);

                if (fromDoc != null && toDoc != null)
                {
                    comparison.FieldChanges = CompareDocumentFields(fromDoc, toDoc);
                    comparison.LineChanges = CompareDocumentLines(fromDoc, toDoc);
                }

                _logger.LogInformation("Compared versions {FromVersion} and {ToVersion} for document {DocumentId}", 
                    fromVersion, toVersion, documentId);

                return comparison;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error comparing versions {FromVersion} and {ToVersion} for document {DocumentId}", 
                    fromVersion, toVersion, documentId);
                throw;
            }
        }

        public async Task<Document> RestoreVersionAsync(Guid documentId, int versionNumber, string restoredBy)
        {
            using var activity = LoggingExtensions.StartActivity("DocumentVersion.Restore", documentId);
            
            try
            {
                var version = await GetVersionAsync(documentId, versionNumber);
                if (version == null)
                    throw new InvalidOperationException($"Version {versionNumber} not found for document {documentId}");

                // Deserialize document from snapshot
                var restoredDocument = JsonSerializer.Deserialize<Document>(version.DocumentSnapshot);
                if (restoredDocument == null)
                    throw new InvalidOperationException("Failed to deserialize document from version snapshot");

                // Update timestamps and user info
                restoredDocument.ModifiedAt = DateTime.UtcNow;
                restoredDocument.ModifiedBy = restoredBy;

                // Save restored document
                await _documentRepository.UpdateAsync(restoredDocument);

                // Create new version for the restoration
                await CreateVersionAsync(documentId, $"Restored from version {versionNumber}", restoredBy);

                _logger.LogInformation("Restored document {DocumentId} to version {VersionNumber} by {RestoredBy}", 
                    documentId, versionNumber, restoredBy);

                return restoredDocument;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring document {DocumentId} to version {VersionNumber}", 
                    documentId, versionNumber);
                throw;
            }
        }

        public async Task DeleteVersionAsync(Guid documentId, int versionNumber)
        {
            using var activity = LoggingExtensions.StartActivity("DocumentVersion.Delete", documentId);
            
            try
            {
                var version = await GetVersionAsync(documentId, versionNumber);
                if (version == null)
                    throw new InvalidOperationException($"Version {versionNumber} not found for document {documentId}");

                if (version.IsActive)
                    throw new InvalidOperationException("Cannot delete the active version");

                // In a real implementation, this would delete from versions table
                await Task.CompletedTask;

                _logger.LogInformation("Deleted version {VersionNumber} for document {DocumentId}", 
                    versionNumber, documentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting version {VersionNumber} for document {DocumentId}", 
                    versionNumber, documentId);
                throw;
            }
        }

        public async Task<Dictionary<string, int>> GetVersionStatisticsAsync(Guid documentId)
        {
            using var activity = LoggingExtensions.StartActivity("DocumentVersion.GetStatistics", documentId);
            
            try
            {
                var versions = await GetVersionHistoryAsync(documentId);
                
                var stats = new Dictionary<string, int>
                {
                    ["TotalVersions"] = versions.Count,
                    ["ActiveVersion"] = versions.FirstOrDefault(v => v.IsActive)?.VersionNumber ?? 0,
                    ["TotalSize"] = (int)versions.Sum(v => v.FileSize)
                };

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting version statistics for document {DocumentId}", documentId);
                throw;
            }
        }

        private List<DocumentFieldChange> CompareDocumentFields(Document fromDoc, Document toDoc)
        {
            var changes = new List<DocumentFieldChange>();

            // Compare basic fields
            if (fromDoc.DocumentNumber != toDoc.DocumentNumber)
                changes.Add(new DocumentFieldChange
                {
                    FieldName = nameof(Document.DocumentNumber),
                    FieldDisplayName = "Document Number",
                    OldValue = fromDoc.DocumentNumber,
                    NewValue = toDoc.DocumentNumber,
                    ChangeType = "Modified"
                });

            if (fromDoc.TotalAmount != toDoc.TotalAmount)
                changes.Add(new DocumentFieldChange
                {
                    FieldName = nameof(Document.TotalAmount),
                    FieldDisplayName = "Total Amount",
                    OldValue = fromDoc.TotalAmount.ToString("C"),
                    NewValue = toDoc.TotalAmount.ToString("C"),
                    ChangeType = "Modified"
                });

            if (fromDoc.Status != toDoc.Status)
                changes.Add(new DocumentFieldChange
                {
                    FieldName = nameof(Document.Status),
                    FieldDisplayName = "Status",
                    OldValue = fromDoc.Status.ToString(),
                    NewValue = toDoc.Status.ToString(),
                    ChangeType = "Modified"
                });

            if (fromDoc.Notes != toDoc.Notes)
                changes.Add(new DocumentFieldChange
                {
                    FieldName = nameof(Document.Notes),
                    FieldDisplayName = "Notes",
                    OldValue = fromDoc.Notes,
                    NewValue = toDoc.Notes,
                    ChangeType = "Modified"
                });

            return changes;
        }

        private List<DocumentLineChange> CompareDocumentLines(Document fromDoc, Document toDoc)
        {
            var changes = new List<DocumentLineChange>();

            // Compare document lines
            var maxLines = Math.Max(fromDoc.Lines.Count, toDoc.Lines.Count);
            
            for (int i = 0; i < maxLines; i++)
            {
                var fromLine = i < fromDoc.Lines.Count ? fromDoc.Lines[i] : null;
                var toLine = i < toDoc.Lines.Count ? toDoc.Lines[i] : null;

                if (fromLine == null && toLine != null)
                {
                    changes.Add(new DocumentLineChange
                    {
                        LineNumber = i + 1,
                        ChangeType = "Added",
                        NewLineData = JsonSerializer.Serialize(toLine),
                        FieldChanges = new List<DocumentFieldChange>()
                    });
                }
                else if (fromLine != null && toLine == null)
                {
                    changes.Add(new DocumentLineChange
                    {
                        LineNumber = i + 1,
                        ChangeType = "Removed",
                        OldLineData = JsonSerializer.Serialize(fromLine),
                        FieldChanges = new List<DocumentFieldChange>()
                    });
                }
                else if (fromLine != null && toLine != null)
                {
                    var lineFieldChanges = CompareDocumentLineFields(fromLine, toLine);
                    if (lineFieldChanges.Any())
                    {
                        changes.Add(new DocumentLineChange
                        {
                            LineNumber = i + 1,
                            ChangeType = "Modified",
                            OldLineData = JsonSerializer.Serialize(fromLine),
                            NewLineData = JsonSerializer.Serialize(toLine),
                            FieldChanges = lineFieldChanges
                        });
                    }
                }
            }

            return changes;
        }

        private List<DocumentFieldChange> CompareDocumentLineFields(DocumentLine fromLine, DocumentLine toLine)
        {
            var changes = new List<DocumentFieldChange>();

            if (fromLine.ProductCode != toLine.ProductCode)
                changes.Add(new DocumentFieldChange
                {
                    FieldName = nameof(DocumentLine.ProductCode),
                    FieldDisplayName = "Product Code",
                    OldValue = fromLine.ProductCode,
                    NewValue = toLine.ProductCode,
                    ChangeType = "Modified"
                });

            if (fromLine.Quantity != toLine.Quantity)
                changes.Add(new DocumentFieldChange
                {
                    FieldName = nameof(DocumentLine.Quantity),
                    FieldDisplayName = "Quantity",
                    OldValue = fromLine.Quantity.ToString(),
                    NewValue = toLine.Quantity.ToString(),
                    ChangeType = "Modified"
                });

            if (fromLine.UnitPrice != toLine.UnitPrice)
                changes.Add(new DocumentFieldChange
                {
                    FieldName = nameof(DocumentLine.UnitPrice),
                    FieldDisplayName = "Unit Price",
                    OldValue = fromLine.UnitPrice.ToString("C"),
                    NewValue = toLine.UnitPrice.ToString("C"),
                    ChangeType = "Modified"
                });

            return changes;
        }

        private async Task SaveVersionAsync(DocumentVersion version)
        {
            // In a real implementation, this would save to a document versions table
            await Task.CompletedTask;
        }

        private async Task UpdateVersionAsync(DocumentVersion version)
        {
            // In a real implementation, this would update the version in the database
            await Task.CompletedTask;
        }

        private static string ComputeChecksum(byte[] data)
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(data);
            return Convert.ToBase64String(hash);
        }
    }
}
