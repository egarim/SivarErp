using Sivar.Erp.Documents.DocumentToTransactions;
using Sivar.Erp.Services.Accounting.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sivar.Erp.Documents
{
    /// <summary>
    /// Generates transactions directly from document totals with accounting information
    /// </summary>
    public class TransactionGeneratorService
    {
    
        private readonly Dictionary<string, Guid> _accountMappings;

        /// <summary>
        /// Creates a new simple transaction generator
        /// </summary>
        /// <param name="transactionService">Service for transaction operations</param>
        /// <param name="accountMappings">Dictionary mapping account codes to account IDs</param>
        public TransactionGeneratorService(
          
            Dictionary<string, Guid> accountMappings = null)
        {
           
            _accountMappings = accountMappings ?? new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Generate a transaction from a document using totals with accounting information
        /// </summary>
        /// <param name="document">The document to generate a transaction for</param>
        /// <returns>A tuple containing the transaction and ledger entries</returns>
        public async Task<(TransactionDto Transaction, List<LedgerEntryDto> LedgerEntries)> 
            GenerateTransactionAsync(IDocument document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            // Create the transaction
            var transaction = new TransactionDto
            {
                Oid = Guid.NewGuid(),
                DocumentId = document.Oid,
                TransactionDate = document.Date,
                Description = GenerateTransactionDescription(document)
            };

            var entries = new List<LedgerEntryDto>();

            // Process all totals that have accounting information
            foreach (var total in document.DocumentTotals.Where(t => t is TotalDto dto && 
                (dto.IncludeInTransaction || 
                (!string.IsNullOrEmpty(dto.DebitAccountCode) || !string.IsNullOrEmpty(dto.CreditAccountCode)))))
            {
                var totalDto = total as TotalDto;
                
                // Create debit entry if account code is specified
                if (!string.IsNullOrEmpty(totalDto.DebitAccountCode))
                {
                    if (_accountMappings.TryGetValue(totalDto.DebitAccountCode, out var accountId))
                    {
                        var entry = new LedgerEntryDto
                        {
                            Oid = Guid.NewGuid(),
                            TransactionId = transaction.Oid,
                            AccountId = accountId,
                            EntryType = EntryType.Debit,
                            Amount = totalDto.Total,
                            AccountName = totalDto.Concept
                        };
                        
                        entries.Add(entry);
                    }
                }
                
                // Create credit entry if account code is specified
                if (!string.IsNullOrEmpty(totalDto.CreditAccountCode))
                {
                    if (_accountMappings.TryGetValue(totalDto.CreditAccountCode, out var accountId))
                    {
                        var entry = new LedgerEntryDto
                        {
                            Oid = Guid.NewGuid(),
                            TransactionId = transaction.Oid,
                            AccountId = accountId,
                            EntryType = EntryType.Credit,
                            Amount = totalDto.Total,
                            AccountName = totalDto.Concept
                        };
                        
                        entries.Add(entry);
                    }
                }
            }
            
           
            
            return (transaction, entries);
        }
        
        /// <summary>
        /// Generate a description for the transaction
        /// </summary>
        private string GenerateTransactionDescription(IDocument document)
        {
            string businessEntityName = document.BusinessEntity?.Name ?? "Unknown";
            string documentTypeCode = document.DocumentType?.Code ?? "Unknown";
            
            return $"{documentTypeCode} - {businessEntityName} - {document.Date}";
        }
        
        /// <summary>
        /// Add an account mapping
        /// </summary>
        /// <param name="accountCode">The account code</param>
        /// <param name="accountId">The account ID</param>
        public void AddAccountMapping(string accountCode, Guid accountId)
        {
            if (string.IsNullOrEmpty(accountCode))
                throw new ArgumentNullException(nameof(accountCode));
                
            _accountMappings[accountCode] = accountId;
        }
        
        /// <summary>
        /// Add account mappings
        /// </summary>
        /// <param name="mappings">Dictionary mapping account codes to account IDs</param>
        public void AddAccountMappings(Dictionary<string, Guid> mappings)
        {
            if (mappings == null)
                throw new ArgumentNullException(nameof(mappings));
                
            foreach (var mapping in mappings)
            {
                _accountMappings[mapping.Key] = mapping.Value;
            }
        }
       
    }
}