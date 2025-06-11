using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Sivar.Erp.Documents;
using Sivar.Erp.ChartOfAccounts;

namespace Sivar.Erp.Documents
{
    /// <summary>
    /// Helper to import transactions and ledger entries from tabular text.
    /// </summary>
    public class TransactionImportHelper
    {
        private readonly List<AccountDto> _accounts;
        private readonly Dictionary<string, Guid> _accountCodeToId;

        public TransactionImportHelper(List<AccountDto> accounts)
        {
            _accounts = accounts;
            _accountCodeToId = accounts.ToDictionary(a => a.OfficialCode, a => a.Id);
        }

        public List<(TransactionDto Transaction, List<LedgerEntryDto> Entries)> Import(string text)
        {
            var result = new List<(TransactionDto, List<LedgerEntryDto>)>();
            var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            string? currentPartida = null;
            TransactionDto? currentTransaction = null;
            List<LedgerEntryDto> currentEntries = new();
            Guid currentTransactionId = Guid.Empty;

            foreach (var line in lines.Skip(1)) // skip header
            {
                var cols = line.Split('\t');
                if (cols.Length < 5) continue;
                var partida = cols[0].Trim();
                var tipoLinea = cols[1].Trim();

                if (tipoLinea == "h") // Header row for transaction
                {
                    // If we have a current transaction, add it to the results before starting a new one
                    if (currentTransaction != null)
                    {
                        result.Add((currentTransaction, currentEntries));
                        currentEntries = new();
                    }

                    currentTransactionId = Guid.NewGuid();
                    currentTransaction = new TransactionDto
                    {
                        Id = currentTransactionId,
                        DocumentId = Guid.Empty, // Set as needed
                        TransactionDate = DateOnly.Parse(cols[2], CultureInfo.InvariantCulture),
                        Description = cols[3].Trim()
                    };
                    currentPartida = partida;
                    Console.WriteLine($"Created transaction {currentTransactionId} with date {currentTransaction.TransactionDate} and description {currentTransaction.Description}");
                }
                else if (currentTransaction != null && partida == currentPartida) // Ledger entry row
                {
                    // Ledger entry row - account is in column 2 (index 1)
                    var accountCode = cols[2].Trim();
                    if (string.IsNullOrEmpty(accountCode)) continue;

                    if (!_accountCodeToId.TryGetValue(accountCode, out var accountId))
                    {
                        Console.WriteLine($"Account code '{accountCode}' not found in provided accounts.");
                        continue; // Skip this entry, but keep processing others
                    }

                    decimal debitAmount = 0;
                    decimal creditAmount = 0;

                    // Parse debit amount (column 3)
                    if (!string.IsNullOrWhiteSpace(cols[3]))
                    {
                        if (!decimal.TryParse(cols[3], NumberStyles.Any, CultureInfo.InvariantCulture, out debitAmount))
                        {
                            Console.WriteLine($"Failed to parse debit amount: {cols[3]}");
                        }
                    }

                    // Parse credit amount (column 4)
                    if (!string.IsNullOrWhiteSpace(cols[4]))
                    {
                        if (!decimal.TryParse(cols[4], NumberStyles.Any, CultureInfo.InvariantCulture, out creditAmount))
                        {
                            Console.WriteLine($"Failed to parse credit amount: {cols[4]}");
                        }
                    }                    // Only skip if both debit and credit are non-zero (which is invalid)
                    if (debitAmount != 0 && creditAmount != 0)
                    {
                        Console.WriteLine($"Invalid entry: both debit and credit have values - debit={debitAmount}, credit={creditAmount}");
                        continue;
                    }                    // Allow entries where both debit and credit are 0
                    // Find the account to get its name and official code
                    var account = _accounts.FirstOrDefault(a => a.Id == accountId);

                    var entry = new LedgerEntryDto
                    {
                        Id = Guid.NewGuid(),
                        TransactionId = currentTransactionId,
                        AccountId = accountId,
                        // If both are zero, default to Debit with zero amount                        // When both are zero, default to Debit with zero amount
                        EntryType = debitAmount != 0 ? EntryType.Debit : EntryType.Credit,
                        Amount = debitAmount != 0 ? debitAmount : creditAmount,
                        // Set the new properties
                        AccountName = account?.AccountName ?? string.Empty,
                        OfficialCode = account?.OfficialCode ?? accountCode
                    };

                    currentEntries.Add(entry);
                    Console.WriteLine($"Added ledger entry: account {accountCode}, {entry.EntryType}, amount {entry.Amount}");
                }
            }

            // Don't forget the last transaction if there is one
            if (currentTransaction != null)
            {
                result.Add((currentTransaction, currentEntries));
            }

            Console.WriteLine($"Imported {result.Count} transactions with {result.Sum(t => t.Item2.Count)} total ledger entries");
            return result;
        }
    }
}
