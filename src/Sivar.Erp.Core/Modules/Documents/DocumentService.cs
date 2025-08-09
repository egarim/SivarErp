using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using Sivar.Erp.Core.Core;

namespace Sivar.Erp.Core.Modules.Documents
{
    /// <summary>
    /// Implementation of the document service
    /// </summary>
    public class DocumentService : IDocumentService
    {
        private readonly IRepository _repository;
        private readonly ILogger<DocumentService> _logger;
        
        /// <summary>
        /// Initializes a new instance of the DocumentService class
        /// </summary>
        /// <param name="repository">The repository for data access</param>
        /// <param name="logger">The logger for diagnostic information</param>
        public DocumentService(IRepository repository, ILogger<DocumentService> logger)
        {
            _repository = repository;
            _logger = logger;
        }
        
        /// <inheritdoc/>
        public Task<IDocument> CreateDocumentAsync(IDocumentType documentType, IBusinessEntity businessEntity)
        {
            // To be implemented
            throw new NotImplementedException();
        }
        
        /// <inheritdoc/>
        public Task CalculateDocumentTaxesAsync(IDocument document, string documentOperation)
        {
            // To be implemented
            throw new NotImplementedException();
        }
        
        /// <inheritdoc/>
        public Task<ValidationResult> ValidateDocumentAsync(IDocument document)
        {
            // To be implemented
            throw new NotImplementedException();
        }
    }
}