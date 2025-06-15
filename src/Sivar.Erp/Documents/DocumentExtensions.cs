using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sivar.Erp.Services.Accounting.Transactions;
using Sivar.Erp.Services.Taxes.TaxAccountingProfiles;
using Sivar.Erp.Services.Taxes.TaxRule;

namespace Sivar.Erp.Documents
{
    /// <summary>
    /// Extension methods for documents and transactions
    /// </summary>
    public static class DocumentExtensions
    {
        /// <summary>
        /// Generates a transaction from a document using a simple transaction generator
        /// </summary>
        /// <param name="document">The document to generate a transaction for</param>
        /// <param name="generator">The transaction generator</param>
        /// <returns>A tuple containing the transaction and ledger entries</returns>
        public static async Task<(TransactionDto Transaction, List<LedgerEntryDto> LedgerEntries)> 
            GenerateSimpleTransactionAsync(this IDocument document, SimpleTransactionGenerator generator)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));
                
            if (generator == null)
                throw new ArgumentNullException(nameof(generator));
            
            return await generator.GenerateTransactionAsync(document);
        }
        
        /// <summary>
        /// Creates a new instance of the DocumentTaxCalculator with tax accounting support
        /// </summary>
        /// <param name="document">The document to calculate taxes for</param>
        /// <param name="documentTypeCode">The document type code to use for tax rule evaluation</param>
        /// <param name="taxRuleEvaluator">The evaluator for determining applicable taxes</param>
        /// <param name="taxAccountingService">Optional service for tax accounting profiles</param>
        /// <returns>A tax calculator with accounting support</returns>
        public static DocumentTaxCalculator CreateTaxCalculatorWithAccounting(
            this DocumentDto document,
            string documentTypeCode,
            TaxRuleEvaluator taxRuleEvaluator,
            ITaxAccountingProfileService taxAccountingService)
        {
            return new DocumentTaxCalculator(
                document, 
                documentTypeCode, 
                taxRuleEvaluator, 
                taxAccountingService);
        }
    }
}