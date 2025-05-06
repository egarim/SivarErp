using DevExpress.Xpo;
using Sivar.Erp.Documents;
using Sivar.Erp.Xpo.ChartOfAccounts;
using Sivar.Erp.Xpo.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sivar.Erp.Xpo.Documents
{
    /// <summary>
    /// Implementation of transaction service using XPO
    /// </summary>
    public class XpoTransactionService : ITransactionService
    {
        /// <summary>
        /// Creates a new transaction
        /// </summary>
        /// <param name="transaction">Transaction to create</param>
        /// <returns>Created transaction with ID</returns>
        public async Task<ITransaction> CreateTransactionAsync(ITransaction transaction)
        {
            using var uow = XpoDataAccessService.GetUnitOfWork();

            XpoTransaction xpoTransaction;

            if (transaction is XpoTransaction existingXpoTrans)
            {
                // If it's already an XPO object, we need to recreate it in this UnitOfWork
                xpoTransaction = new XpoTransaction(uow);
                // Copy properties
                xpoTransaction.DocumentId = existingXpoTrans.DocumentId;
                xpoTransaction.TransactionDate = existingXpoTrans.TransactionDate;
                xpoTransaction.Description = existingXpoTrans.Description;
            }
            else
            {
                // Find the associated document
                var document = await uow.GetObjectByKeyAsync<XpoDocument>(transaction.DocumentId);

                if (document == null)
                {
                    throw new Exception($"Document with ID {transaction.DocumentId} not found");
                }

                // Create new XPO transaction from the interface
                xpoTransaction = new XpoTransaction(uow)
                {
                    Document = document,
                    TransactionDate = transaction.TransactionDate,
                    Description = transaction.Description
                };
            }

            // Generate new ID if not provided
            if (xpoTransaction.Id == Guid.Empty)
            {
                xpoTransaction.Id = Guid.NewGuid();
            }

            // Save changes
            await uow.CommitChangesAsync();

            return xpoTransaction;
        }

        /// <summary>
        /// Validates a transaction for accounting balance
        /// </summary>
        /// <param name="transactionId">Transaction ID</param>
        /// <param name="entries">Ledger entries for the transaction</param>
        /// <returns>True if valid, false otherwise</returns>
        public async Task<bool> ValidateTransactionAsync(Guid transactionId, IEnumerable<ILedgerEntry> entries)
        {
            // Validate transaction has entries
            if (entries == null || !entries.Any())
            {
                return false;
            }

            // Calculate total debits and credits
            decimal totalDebits = entries
                .Where(e => e.EntryType == EntryType.Debit)
                .Sum(e => e.Amount);

            decimal totalCredits = entries
                .Where(e => e.EntryType == EntryType.Credit)
                .Sum(e => e.Amount);

            // Transaction is valid if debits equal credits
            return Math.Abs(totalDebits - totalCredits) < 0.01m;
        }

        /// <summary>
        /// Retrieves all transactions for a document
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <returns>Collection of transactions</returns>
        public async Task<IEnumerable<ITransaction>> GetTransactionsByDocumentIdAsync(Guid documentId)
        {
            using var uow = XpoDataAccessService.GetUnitOfWork();

            var transactions = await Task.Run(() =>
                uow.Query<XpoTransaction>()
                    .Where(t => t.DocumentId == documentId)
                    .ToList());

            return transactions;
        }

        /// <summary>
        /// Retrieves all ledger entries for a transaction
        /// </summary>
        /// <param name="transactionId">Transaction ID</param>
        /// <returns>Collection of ledger entries</returns>
        public async Task<IEnumerable<ILedgerEntry>> GetLedgerEntriesByTransactionIdAsync(Guid transactionId)
        {
            using var uow = XpoDataAccessService.GetUnitOfWork();

            var entries = await Task.Run(() =>
                uow.Query<XpoLedgerEntry>()
                    .Where(e => e.TransactionId == transactionId)
                    .ToList());

            return entries;
        }

        /// <summary>
        /// Creates a new ledger entry
        /// </summary>
        /// <param name="entry">Ledger entry to create</param>
        /// <returns>Created ledger entry</returns>
        public async Task<ILedgerEntry> CreateLedgerEntryAsync(ILedgerEntry entry)
        {
            using var uow = XpoDataAccessService.GetUnitOfWork();

            // Find the associated transaction
            var transaction = await uow.GetObjectByKeyAsync<XpoTransaction>(entry.TransactionId);

            if (transaction == null)
            {
                throw new Exception($"Transaction with ID {entry.TransactionId} not found");
            }

            // Find the associated account
            var account = await uow.GetObjectByKeyAsync<XpoAccount>(entry.AccountId);

            if (account == null)
            {
                throw new Exception($"Account with ID {entry.AccountId} not found");
            }

            // Create new XPO ledger entry
            var xpoEntry = new XpoLedgerEntry(uow)
            {
                Id = entry.Id == Guid.Empty ? Guid.NewGuid() : entry.Id,
                Transaction = transaction,
                Account = account,
                EntryType = entry.EntryType,
                Amount = entry.Amount,
                PersonId = entry.PersonId,
                CostCentreId = entry.CostCentreId
            };

            // Save changes
            await uow.CommitChangesAsync();

            return xpoEntry;
        }

        /// <summary>
        /// Creates multiple ledger entries in a single transaction
        /// </summary>
        /// <param name="transactionId">Transaction ID</param>
        /// <param name="entries">Ledger entries to create</param>
        /// <returns>Created ledger entries</returns>
        public async Task<IEnumerable<ILedgerEntry>> CreateLedgerEntriesAsync(
            Guid transactionId, IEnumerable<ILedgerEntry> entries)
        {
            using var uow = XpoDataAccessService.GetUnitOfWork();

            // Find the associated transaction
            var transaction = await uow.GetObjectByKeyAsync<XpoTransaction>(transactionId);

            if (transaction == null)
            {
                throw new Exception($"Transaction with ID {transactionId} not found");
            }

            var createdEntries = new List<XpoLedgerEntry>();

            foreach (var entry in entries)
            {
                // Find the associated account
                var account = await uow.GetObjectByKeyAsync<XpoAccount>(entry.AccountId);

                if (account == null)
                {
                    throw new Exception($"Account with ID {entry.AccountId} not found");
                }

                // Create new XPO ledger entry
                var xpoEntry = new XpoLedgerEntry(uow)
                {
                    Id = entry.Id == Guid.Empty ? Guid.NewGuid() : entry.Id,
                    Transaction = transaction,
                    Account = account,
                    EntryType = entry.EntryType,
                    Amount = entry.Amount,
                    PersonId = entry.PersonId,
                    CostCentreId = entry.CostCentreId
                };

                createdEntries.Add(xpoEntry);
            }

            // Save all changes in a single commit
            await uow.CommitChangesAsync();

            return createdEntries;
        }
    }
}