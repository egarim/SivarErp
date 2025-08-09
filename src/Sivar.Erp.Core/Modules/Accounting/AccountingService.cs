using Microsoft.Extensions.Logging;
using Sivar.Erp.Core.Core;
using Sivar.Erp.Core.Modules.Documents;

namespace Sivar.Erp.Core.Modules.Accounting
{
    /// <summary>
    /// Implementation of the accounting service
    /// </summary>
    public class AccountingService : IAccountingService
    {
        private readonly IRepository _repository;
        private readonly ILogger<AccountingService> _logger;
        
        /// <summary>
        /// Initializes a new instance of the AccountingService class
        /// </summary>
        /// <param name="repository">The repository for data access</param>
        /// <param name="logger">The logger for diagnostic information</param>
        public AccountingService(IRepository repository, ILogger<AccountingService> logger)
        {
            _repository = repository;
            _logger = logger;
        }
        
        /// <inheritdoc/>
        public Task<ITransaction> CreateTransactionAsync(IDocument document, string? description = null)
        {
            // To be implemented
            throw new NotImplementedException();
        }
        
        /// <inheritdoc/>
        public Task PostTransactionAsync(ITransaction transaction)
        {
            // To be implemented
            throw new NotImplementedException();
        }
        
        /// <inheritdoc/>
        public Task<decimal> CalculateAccountBalanceAsync(string accountCode, DateTime? asOfDate = null)
        {
            // To be implemented
            throw new NotImplementedException();
        }
    }
}