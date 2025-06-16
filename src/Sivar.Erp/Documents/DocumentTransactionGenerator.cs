using Sivar.Erp.Documents.DocumentToTransactions;
using Sivar.Erp.Services.Accounting.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sivar.Erp.Documents
{
    /// <summary>
    /// Handles the generation of accounting transactions from documents
    /// based on document type
    /// </summary>
    public class DocumentTransactionGenerator
    {
   
        private readonly Dictionary<string, TransactionTemplate> _templates;
        private readonly Dictionary<string, Guid> _accountMappings;

        /// <summary>
        /// Creates a new document transaction generator
        /// </summary>
        /// <param name="transactionService">Service for transaction operations</param>
        /// <param name="accountMappings">Dictionary mapping account keys to account IDs</param>
        public DocumentTransactionGenerator(
        
            Dictionary<string, Guid> accountMappings = null)
        {
           
            _templates = new Dictionary<string, TransactionTemplate>(StringComparer.OrdinalIgnoreCase);
            _accountMappings = accountMappings ?? new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Registers a transaction template for a document type
        /// </summary>
        /// <param name="template">The transaction template to register</param>
        public void RegisterTemplate(TransactionTemplate template)
        {
            if (template == null)
                throw new ArgumentNullException(nameof(template));
                
            _templates[template.DocumentTypeCode] = template;
        }

        /// <summary>
        /// Adds or updates account mappings
        /// </summary>
        /// <param name="accountMappings">Dictionary mapping account keys to account IDs</param>
        public void SetAccountMappings(Dictionary<string, Guid> accountMappings)
        {
            if (accountMappings == null)
                throw new ArgumentNullException(nameof(accountMappings));
                
            foreach (var mapping in accountMappings)
            {
                _accountMappings[mapping.Key] = mapping.Value;
            }
        }

        /// <summary>
        /// Adds an account mapping
        /// </summary>
        /// <param name="key">The account key</param>
        /// <param name="accountId">The account ID</param>
        public void AddAccountMapping(string key, Guid accountId)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));
                
            _accountMappings[key] = accountId;
        }

        /// <summary>
        /// Generates a transaction for a document
        /// </summary>
        /// <param name="document">The document to generate a transaction for</param>
        /// <returns>A tuple containing the transaction and ledger entries</returns>
        public async Task<(TransactionDto Transaction, List<LedgerEntryDto> LedgerEntries)> 
            GenerateTransactionAsync(DocumentDto document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            if (document.DocumentType == null)
                throw new InvalidOperationException("Document must have a document type");

            string documentTypeCode = document.DocumentType.Code;

            // Check if there's a registered template for this document type
            if (!_templates.TryGetValue(documentTypeCode, out var template))
            {
                throw new InvalidOperationException(
                    $"No transaction template registered for document type code: {documentTypeCode}");
            }

            // Create transaction
            var transaction = new TransactionDto
            {
                Oid = Guid.NewGuid(),
                DocumentId = document.Oid,
                TransactionDate = document.Date,
                Description = template.DescriptionGenerator(document)
            };

            // Create and save the transaction
            var createdTransaction = transaction;

            // Generate ledger entries based on the template
            var ledgerEntries = new List<LedgerEntryDto>();
            
            foreach (var entry in template.Entries)
            {
                decimal amount;
                try
                {
                    amount = entry.AmountCalculator(document);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        $"Error calculating amount for account {entry.AccountKey}: {ex.Message}", ex);
                }
                
                // Skip zero or negative amounts
                if (amount <= 0)
                    continue;
                    
                // Find the account ID from mappings
                if (!_accountMappings.TryGetValue(entry.AccountKey, out Guid accountId))
                {
                    throw new InvalidOperationException($"Account mapping not found for key: {entry.AccountKey}");
                }
                
                // Create ledger entry
                var ledgerEntry = new LedgerEntryDto
                {
                    Oid = Guid.NewGuid(),
                    TransactionId = createdTransaction.Oid,
                    AccountId = accountId,
                    EntryType = entry.EntryType,
                    Amount = amount,
                    AccountName = entry.AccountNameOverride ?? entry.AccountKey
                };
                
                ledgerEntries.Add(ledgerEntry);
            }
            
            // Validate transaction balance
            bool isValid = await ValidateTransactionAsync(
                createdTransaction.Oid, ledgerEntries);

            if (!isValid)
            {
                decimal totalDebits = ledgerEntries.Where(e => e.EntryType == EntryType.Debit).Sum(e => e.Amount);
                decimal totalCredits = ledgerEntries.Where(e => e.EntryType == EntryType.Credit).Sum(e => e.Amount);

                throw new InvalidOperationException(
                    $"Generated transaction is not balanced. Debits: {totalDebits}, " + 
                    $"Credits: {totalCredits}, Difference: {totalDebits - totalCredits}");
            }

            return (transaction, ledgerEntries);
        }
        /// <summary>
        /// Validates a transaction for accounting balance
        /// </summary>
        /// <param name="transactionId">Transaction ID</param>
        /// <param name="entries">Ledger entries for the transaction</param>
        /// <returns>True if valid, false otherwise</returns>
        public Task<bool> ValidateTransactionAsync(Guid transactionId, IEnumerable<ILedgerEntry> entries)
        {
            // Validate transaction has entries
            if (entries == null || !entries.Any())
            {
                return Task.FromResult(false);
            }

            // Calculate total debits and credits
            decimal totalDebits = entries
                .Where(e => e.EntryType == EntryType.Debit)
                .Sum(e => e.Amount);

            decimal totalCredits = entries
                .Where(e => e.EntryType == EntryType.Credit)
                .Sum(e => e.Amount);

            // Transaction is valid if debits equal credits
            return Task.FromResult(Math.Abs(totalDebits - totalCredits) < 0.01m);
        }
        /// <summary>
        /// Generates and persists ledger entries for a transaction
        /// </summary>
        /// <param name="document">The document to generate a transaction for</param>
        /// <returns>The generated transaction</returns>
        public async Task<TransactionDto> GenerateAndSaveTransactionAsync(DocumentDto document)
        {
            var (transaction, ledgerEntries) = await GenerateTransactionAsync(document);

            // Here you would save the ledger entries using whatever service you have
            // This is a placeholder for the actual implementation
            // await _ledgerEntryService.CreateLedgerEntriesAsync(transaction.Oid, ledgerEntries);

            return transaction;
        }
    }
}