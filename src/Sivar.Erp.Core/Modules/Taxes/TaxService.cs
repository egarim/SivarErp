using Microsoft.Extensions.Logging;
using Sivar.Erp.Core.Core;
using Sivar.Erp.Core.Modules.Documents;
using Sivar.Erp.Core.Modules.Taxes.Models;

namespace Sivar.Erp.Core.Modules.Taxes
{
    /// <summary>
    /// Implementation of the tax service
    /// </summary>
    public class TaxService : ITaxService
    {
        private readonly IRepository _repository;
        private readonly ILogger<TaxService> _logger;
        
        /// <summary>
        /// Initializes a new instance of the TaxService class
        /// </summary>
        /// <param name="repository">The repository for data access</param>
        /// <param name="logger">The logger for diagnostic information</param>
        public TaxService(IRepository repository, ILogger<TaxService> logger)
        {
            _repository = repository;
            _logger = logger;
        }
        
        /// <inheritdoc/>
        public Task<IEnumerable<ITax>> GetApplicableTaxesAsync(IDocument document, DocumentOperation operation)
        {
            // To be implemented
            _logger.LogInformation("Getting applicable taxes for document {DocumentNumber}", document.DocumentNumber);
            return Task.FromResult<IEnumerable<ITax>>(new List<ITax>());
        }
        
        /// <inheritdoc/>
        public Task ApplyTaxRulesToDocumentAsync(IDocument document)
        {
            // To be implemented
            _logger.LogInformation("Applying tax rules to document {DocumentNumber}", document.DocumentNumber);
            return Task.CompletedTask;
        }
    }
}