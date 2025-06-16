using Sivar.Erp.Services.Accounting.Transactions;
using System;
using System.Linq;

namespace Sivar.Erp.Documents
{
    public static class ITransactionExtensions
    {
        /// <summary>
        /// Validates a transaction for accounting balance
        /// </summary>
        /// <param name="entries">Ledger entries for the transaction</param>
        /// <returns>True if valid, false otherwise</returns>
        public static Task<bool> ValidateTransactionAsync(this ITransaction transaction)
        {
            // Validate transaction has entries
            if (transaction.LedgerEntries == null || !transaction.LedgerEntries.Any())
            {
                return Task.FromResult(false);
            }

            // Calculate total debits and credits
            decimal totalDebits = transaction.LedgerEntries
                .Where(e => e.EntryType == EntryType.Debit)
                .Sum(e => e.Amount);

            decimal totalCredits = transaction.LedgerEntries
                .Where(e => e.EntryType == EntryType.Credit)
                .Sum(e => e.Amount);

            // Transaction is valid if debits equal credits
            return Task.FromResult(Math.Abs(totalDebits - totalCredits) < 0.01m);
        }

    }
}