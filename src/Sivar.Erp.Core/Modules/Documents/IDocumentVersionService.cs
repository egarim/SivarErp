using System.ComponentModel;
using Sivar.Erp.Core.Modules.Domain;
using Sivar.Erp.Core.Core;

namespace Sivar.Erp.Core.Modules.Documents
{
    /// <summary>
    /// Service interface for document version management
    /// </summary>
    [Description("Service interface for document version management")]
    public interface IDocumentVersionService
    {
        /// <summary>
        /// Creates a new version of a document
        /// </summary>
        /// <param name="document">Document to create version for</param>
        /// <param name="versionComment">Comment for the version</param>
        /// <param name="createdBy">User who created the version</param>
        /// <returns>Created document version</returns>
        [Description("Creates a new version of a document")]
        Task<DocumentVersion> CreateVersionAsync(IDocument document, string versionComment, string? createdBy = null);

        /// <summary>
        /// Gets all versions of a document
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <returns>List of document versions</returns>
        [Description("Gets all versions of a document")]
        Task<IEnumerable<DocumentVersion>> GetDocumentVersionsAsync(Guid documentId);

        /// <summary>
        /// Gets a specific version of a document
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <param name="versionNumber">Version number</param>
        /// <returns>Document version</returns>
        [Description("Gets a specific version of a document")]
        Task<DocumentVersion?> GetDocumentVersionAsync(Guid documentId, int versionNumber);

        /// <summary>
        /// Restores a document to a specific version
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <param name="versionNumber">Version number to restore</param>
        /// <param name="restoredBy">User who restored the version</param>
        /// <returns>Restored document</returns>
        [Description("Restores a document to a specific version")]
        Task<IDocument> RestoreVersionAsync(Guid documentId, int versionNumber, string? restoredBy = null);

        /// <summary>
        /// Compares two versions of a document
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <param name="fromVersion">Source version number</param>
        /// <param name="toVersion">Target version number</param>
        /// <returns>Version comparison result</returns>
        [Description("Compares two versions of a document")]
        Task<DocumentVersionComparison> CompareVersionsAsync(Guid documentId, int fromVersion, int toVersion);

        /// <summary>
        /// Deletes old versions based on retention policy
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <param name="versionsToKeep">Number of versions to keep</param>
        /// <returns>Number of versions deleted</returns>
        [Description("Deletes old versions based on retention policy")]
        Task<int> PurgeOldVersionsAsync(Guid documentId, int versionsToKeep);
    }
}
