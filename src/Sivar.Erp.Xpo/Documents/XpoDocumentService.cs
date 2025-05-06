using DevExpress.Xpo;
using Sivar.Erp.Documents;
using Sivar.Erp.Xpo.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sivar.Erp.Xpo.Documents
{
    /// <summary>
    /// Implementation of document service using XPO
    /// </summary>
    public class XpoDocumentService : IDocumentService
    {
        private readonly IAuditService _auditService;

        /// <summary>
        /// Initializes a new instance of the document service
        /// </summary>
        /// <param name="auditService">Audit service</param>
        public XpoDocumentService(IAuditService auditService)
        {
            _auditService = auditService;
        }

        /// <summary>
        /// Creates a new document
        /// </summary>
        /// <param name="document">Document to create</param>
        /// <param name="userName">User performing the operation</param>
        /// <returns>Created document with ID</returns>
        public async Task<IDocument> CreateDocumentAsync(IDocument document, string userName)
        {
            using var uow = XpoDataAccessService.GetUnitOfWork();

            XpoDocument xpoDocument;

            if (document is XpoDocument existingXpoDoc)
            {
                // If it's already an XPO object, we need to recreate it in this UnitOfWork
                xpoDocument = new XpoDocument(uow);
                // Copy properties
                xpoDocument.DocumentDate = existingXpoDoc.DocumentDate;
                xpoDocument.DocumentNo = existingXpoDoc.DocumentNo;
                xpoDocument.Description = existingXpoDoc.Description;
                xpoDocument.DocumentComments = existingXpoDoc.DocumentComments;
                xpoDocument.InternalComments = existingXpoDoc.InternalComments;
                xpoDocument.DocumentType = existingXpoDoc.DocumentType;
                xpoDocument.ExtendedDocumentTypeId = existingXpoDoc.ExtendedDocumentTypeId;
                xpoDocument.ExternalId = existingXpoDoc.ExternalId;
            }
            else
            {
                // Create new XPO document from the interface
                xpoDocument = new XpoDocument(uow)
                {
                    DocumentDate = document.DocumentDate,
                    DocumentNo = document.DocumentNo,
                    Description = document.Description,
                    DocumentComments = document.DocumentComments,
                    InternalComments = document.InternalComments,
                    DocumentType = document.DocumentType,
                    ExtendedDocumentTypeId = document.ExtendedDocumentTypeId,
                    ExternalId = document.ExternalId
                };
            }

            // Generate new ID if not provided
            if (xpoDocument.Id == Guid.Empty)
            {
                xpoDocument.Id = Guid.NewGuid();
            }

            // Set audit information
            _auditService.SetCreationAudit(xpoDocument, userName);

            // Save changes
            await uow.CommitChangesAsync();

            return xpoDocument;
        }

        /// <summary>
        /// Updates an existing document
        /// </summary>
        /// <param name="document">Document with updated values</param>
        /// <param name="userName">User performing the operation</param>
        /// <returns>Updated document</returns>
        public async Task<IDocument> UpdateDocumentAsync(IDocument document, string userName)
        {
            if (document.Id == Guid.Empty)
            {
                throw new ArgumentException("Document ID must be provided for update", nameof(document));
            }

            using var uow = XpoDataAccessService.GetUnitOfWork();

            // Find existing document
            var xpoDocument = await uow.GetObjectByKeyAsync<XpoDocument>(document.Id);

            if (xpoDocument == null)
            {
                throw new Exception($"Document with ID {document.Id} not found");
            }

            // Update properties
            xpoDocument.DocumentDate = document.DocumentDate;
            xpoDocument.DocumentNo = document.DocumentNo;
            xpoDocument.Description = document.Description;
            xpoDocument.DocumentComments = document.DocumentComments;
            xpoDocument.InternalComments = document.InternalComments;
            xpoDocument.DocumentType = document.DocumentType;
            xpoDocument.ExtendedDocumentTypeId = document.ExtendedDocumentTypeId;
            xpoDocument.ExternalId = document.ExternalId;

            // Set audit information for update
            _auditService.SetUpdateAudit(xpoDocument, userName);

            // Save changes
            await uow.CommitChangesAsync();

            return xpoDocument;
        }

        /// <summary>
        /// Retrieves a document by ID
        /// </summary>
        /// <param name="id">Document ID</param>
        /// <returns>Document if found, null otherwise</returns>
        public async Task<IDocument?> GetDocumentByIdAsync(Guid id)
        {
            using var uow = XpoDataAccessService.GetUnitOfWork();
            return await uow.GetObjectByKeyAsync<XpoDocument>(id);
        }

        /// <summary>
        /// Retrieves documents within date range
        /// </summary>
        /// <param name="startDate">Start date (inclusive)</param>
        /// <param name="endDate">End date (inclusive)</param>
        /// <returns>Collection of documents</returns>
        public async Task<IEnumerable<IDocument>> GetDocumentsByDateRangeAsync(DateOnly startDate, DateOnly endDate)
        {
            using var uow = XpoDataAccessService.GetUnitOfWork();

            var documents = await Task.Run(() =>
                uow.Query<XpoDocument>()
                    .Where(d => d.DocumentDate >= startDate && d.DocumentDate <= endDate)
                    .ToList());

            return documents;
        }

        /// <summary>
        /// Retrieves documents by type
        /// </summary>
        /// <param name="documentType">Document type</param>
        /// <returns>Collection of documents</returns>
        public async Task<IEnumerable<IDocument>> GetDocumentsByTypeAsync(DocumentType documentType)
        {
            using var uow = XpoDataAccessService.GetUnitOfWork();

            var documents = await Task.Run(() =>
                uow.Query<XpoDocument>()
                    .Where(d => d.DocumentType == documentType)
                    .ToList());

            return documents;
        }
    }
}