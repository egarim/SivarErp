using System;
using System.Collections.Generic;

namespace Sivar.Erp.Documents
{
    /// <summary>
    /// Configurable template for generating transactions from documents
    /// </summary>
    public class TransactionTemplate
    {
        /// <summary>
        /// Function to get the transaction description from the document
        /// </summary>
        public Func<DocumentDto, string> DescriptionGenerator { get; set; }
        
        /// <summary>
        /// The list of transaction entries to generate
        /// </summary>
        public List<AccountingTransactionEntry> Entries { get; } = new List<AccountingTransactionEntry>();
        
        /// <summary>
        /// Document type code this template applies to
        /// </summary>
        public string DocumentTypeCode { get; set; }
        
        /// <summary>
        /// Creates a new transaction template
        /// </summary>
        /// <param name="documentTypeCode">The document type code this template applies to</param>
        /// <param name="descriptionGenerator">Function to generate the transaction description</param>
        public TransactionTemplate(
            string documentTypeCode,
            Func<DocumentDto, string> descriptionGenerator = null)
        {
            DocumentTypeCode = documentTypeCode ?? throw new ArgumentNullException(nameof(documentTypeCode));
            DescriptionGenerator = descriptionGenerator ?? DefaultDescriptionGenerator;
        }
        
        /// <summary>
        /// Adds a transaction entry to this template
        /// </summary>
        /// <param name="entry">The entry to add</param>
        /// <returns>This template for fluent chaining</returns>
        public TransactionTemplate WithEntry(AccountingTransactionEntry entry)
        {
            Entries.Add(entry);
            return this;
        }
        
        /// <summary>
        /// Adds multiple transaction entries to this template
        /// </summary>
        /// <param name="entries">The entries to add</param>
        /// <returns>This template for fluent chaining</returns>
        public TransactionTemplate WithEntries(params AccountingTransactionEntry[] entries)
        {
            Entries.AddRange(entries);
            return this;
        }
        
        /// <summary>
        /// Sets the description generator for this template
        /// </summary>
        /// <param name="descriptionGenerator">The description generator function</param>
        /// <returns>This template for fluent chaining</returns>
        public TransactionTemplate WithDescriptionGenerator(Func<DocumentDto, string> descriptionGenerator)
        {
            DescriptionGenerator = descriptionGenerator ?? throw new ArgumentNullException(nameof(descriptionGenerator));
            return this;
        }
        
        /// <summary>
        /// Default description generator
        /// </summary>
        /// <param name="document">The document</param>
        /// <returns>A default description</returns>
        private string DefaultDescriptionGenerator(DocumentDto document)
        {
            string entityName = document.BusinessEntity?.Name ?? "Unknown";
            string documentTypeCode = document.DocumentType?.Code ?? "Unknown";
            
            return $"{documentTypeCode} - {entityName} - {document.Date}";
        }
    }
}