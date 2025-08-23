using Microsoft.Extensions.Logging;
using Sivar.Erp.Core.Infrastructure.Logging;
using System.ComponentModel;
using Sivar.Erp.Core.Infrastructure.Repository;
using System.Security.Cryptography;

namespace Sivar.Erp.Core.Modules.Documents
{
    /// <summary>
    /// Document attachment service implementation
    /// </summary>
    [Description("Document attachment service")]
    public class DocumentAttachmentService : IDocumentAttachmentService
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly ILogger<DocumentAttachmentService> _logger;
        private readonly string _attachmentsBasePath;

        public DocumentAttachmentService(
            IDocumentRepository documentRepository,
            ILogger<DocumentAttachmentService> logger)
        {
            _documentRepository = documentRepository;
            _logger = logger;
            _attachmentsBasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SivarErp", "Attachments");
            
            // Ensure attachments directory exists
            Directory.CreateDirectory(_attachmentsBasePath);
        }

        public async Task<DocumentAttachment> AttachFileAsync(Guid documentId, Stream fileStream, string fileName, string contentType, string attachedBy, string description = "")
        {
            using var activity = LoggingExtensions.StartActivity("DocumentAttachment.Attach", documentId);
            
            try
            {
                var document = await _documentRepository.GetByIdAsync(documentId);
                if (document == null)
                    throw new InvalidOperationException($"Document {documentId} not found");

                // Read file data
                var fileData = new byte[fileStream.Length];
                await fileStream.ReadAsync(fileData, 0, fileData.Length);

                // Generate unique file name
                var fileExtension = Path.GetExtension(fileName);
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(_attachmentsBasePath, uniqueFileName);

                // Save file to disk
                await File.WriteAllBytesAsync(filePath, fileData);

                // Calculate checksum
                var checksumHash = ComputeChecksum(fileData);

                var attachment = new DocumentAttachment
                {
                    DocumentId = documentId,
                    FileName = uniqueFileName,
                    OriginalFileName = fileName,
                    ContentType = contentType,
                    FileSize = fileData.Length,
                    FilePath = filePath,
                    AttachedBy = attachedBy,
                    AttachedAt = DateTime.UtcNow,
                    Description = description,
                    IsActive = true,
                    ChecksumHash = checksumHash
                };

                // Save attachment record
                await SaveAttachmentAsync(attachment);

                _logger.LogInformation("File {FileName} attached to document {DocumentId} by {AttachedBy}", 
                    fileName, documentId, attachedBy);

                return attachment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error attaching file {FileName} to document {DocumentId}", 
                    fileName, documentId);
                throw;
            }
        }

        public async Task<AttachmentFileData> DownloadAttachmentAsync(Guid attachmentId)
        {
            using var activity = LoggingExtensions.StartActivity("DocumentAttachment.Download", attachmentId);
            
            try
            {
                var attachment = await GetAttachmentAsync(attachmentId);
                if (attachment == null)
                    throw new InvalidOperationException($"Attachment {attachmentId} not found");

                if (!attachment.IsActive)
                    throw new InvalidOperationException($"Attachment {attachmentId} is not active");

                if (!File.Exists(attachment.FilePath))
                    throw new InvalidOperationException($"Attachment file not found at {attachment.FilePath}");

                var fileContent = await File.ReadAllBytesAsync(attachment.FilePath);
                var fileInfo = new FileInfo(attachment.FilePath);

                var fileData = new AttachmentFileData
                {
                    FileName = attachment.OriginalFileName,
                    ContentType = attachment.ContentType,
                    FileContent = fileContent,
                    FileSize = fileContent.Length,
                    LastModified = fileInfo.LastWriteTime
                };

                _logger.LogInformation("Downloaded attachment {AttachmentId} ({FileName})", 
                    attachmentId, attachment.OriginalFileName);

                return fileData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading attachment {AttachmentId}", attachmentId);
                throw;
            }
        }

        public async Task<List<DocumentAttachment>> GetAttachmentsAsync(Guid documentId)
        {
            using var activity = LoggingExtensions.StartActivity("DocumentAttachment.GetAttachments", documentId);
            
            try
            {
                // In a real implementation, this would query attachments table
                // For now, return empty list as placeholder
                return new List<DocumentAttachment>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting attachments for document {DocumentId}", documentId);
                throw;
            }
        }

        public async Task<DocumentAttachment?> GetAttachmentAsync(Guid attachmentId)
        {
            using var activity = LoggingExtensions.StartActivity("DocumentAttachment.GetAttachment", attachmentId);
            
            try
            {
                // In a real implementation, this would query attachment by ID
                // For now, return null as placeholder
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting attachment {AttachmentId}", attachmentId);
                throw;
            }
        }

        public async Task DeleteAttachmentAsync(Guid attachmentId, string deletedBy)
        {
            using var activity = LoggingExtensions.StartActivity("DocumentAttachment.Delete", attachmentId);
            
            try
            {
                var attachment = await GetAttachmentAsync(attachmentId);
                if (attachment == null)
                    throw new InvalidOperationException($"Attachment {attachmentId} not found");

                // Mark as inactive instead of deleting
                attachment.IsActive = false;
                await UpdateAttachmentAsync(attachment);

                // Optionally delete the physical file
                if (File.Exists(attachment.FilePath))
                {
                    File.Delete(attachment.FilePath);
                }

                _logger.LogInformation("Deleted attachment {AttachmentId} ({FileName}) by {DeletedBy}", 
                    attachmentId, attachment.OriginalFileName, deletedBy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting attachment {AttachmentId}", attachmentId);
                throw;
            }
        }

        public async Task<DocumentAttachment> UpdateAttachmentInfoAsync(Guid attachmentId, string description, string modifiedBy)
        {
            using var activity = LoggingExtensions.StartActivity("DocumentAttachment.UpdateInfo", attachmentId);
            
            try
            {
                var attachment = await GetAttachmentAsync(attachmentId);
                if (attachment == null)
                    throw new InvalidOperationException($"Attachment {attachmentId} not found");

                attachment.Description = description;
                await UpdateAttachmentAsync(attachment);

                _logger.LogInformation("Updated attachment {AttachmentId} info by {ModifiedBy}", 
                    attachmentId, modifiedBy);

                return attachment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating attachment {AttachmentId} info", attachmentId);
                throw;
            }
        }

        public async Task<bool> ValidateAttachmentAsync(Guid attachmentId)
        {
            using var activity = LoggingExtensions.StartActivity("DocumentAttachment.Validate", attachmentId);
            
            try
            {
                var attachment = await GetAttachmentAsync(attachmentId);
                if (attachment == null)
                    return false;

                if (!File.Exists(attachment.FilePath))
                    return false;

                // Validate file size
                var fileInfo = new FileInfo(attachment.FilePath);
                if (fileInfo.Length != attachment.FileSize)
                    return false;

                // Validate checksum
                var fileContent = await File.ReadAllBytesAsync(attachment.FilePath);
                var currentChecksum = ComputeChecksum(fileContent);
                
                return currentChecksum == attachment.ChecksumHash;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating attachment {AttachmentId}", attachmentId);
                return false;
            }
        }

        public async Task<Dictionary<string, object>> GetAttachmentStatisticsAsync(Guid? documentId = null)
        {
            using var activity = LoggingExtensions.StartActivity("DocumentAttachment.GetStatistics");
            
            try
            {
                var stats = new Dictionary<string, object>();

                if (documentId.HasValue)
                {
                    var attachments = await GetAttachmentsAsync(documentId.Value);
                    stats["TotalAttachments"] = attachments.Count;
                    stats["TotalSize"] = attachments.Sum(a => a.FileSize);
                    stats["ActiveAttachments"] = attachments.Count(a => a.IsActive);
                }
                else
                {
                    // Global statistics - in real implementation would query all attachments
                    stats["TotalAttachments"] = 0;
                    stats["TotalSize"] = 0L;
                    stats["ActiveAttachments"] = 0;
                }

                stats["LastUpdated"] = DateTime.UtcNow;
                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting attachment statistics");
                throw;
            }
        }

        private async Task SaveAttachmentAsync(DocumentAttachment attachment)
        {
            // In a real implementation, this would save to attachments table
            await Task.CompletedTask;
        }

        private async Task UpdateAttachmentAsync(DocumentAttachment attachment)
        {
            // In a real implementation, this would update the attachment in database
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
