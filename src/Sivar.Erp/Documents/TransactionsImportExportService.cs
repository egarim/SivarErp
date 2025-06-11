using Sivar.Erp.ChartOfAccounts;
using Sivar.Erp.Documents;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Sivar.Erp.Documents
{
    /// <summary>
    /// Helper to import transactions and ledger entries from tabular text.
    /// </summary>
    public class TransactionsImportExportService
    {
        private readonly List<AccountDto> _accounts;
        private readonly Dictionary<string, Guid> _accountCodeToId;

        public TransactionsImportExportService(List<AccountDto> accounts)
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
        }        /// <summary>
                 /// Exports transactions and their associated ledger entries to CSV format
                 /// </summary>
                 /// <param name="transactionsWithEntries">List of transactions with their associated ledger entries</param>
                 /// <returns>Tuple containing the transactions CSV and ledger entries CSV as strings</returns>
        public (string TransactionsCsv, string LedgerEntriesCsv) ExportTransactions(
            List<(TransactionDto Transaction, List<LedgerEntryDto> Entries)> transactionsWithEntries)
        {
            try
            {
                string transactionsCsv = GenerateTransactionsCsv(transactionsWithEntries.Select(x => x.Transaction).ToList());
                string ledgerEntriesCsv = GenerateLedgerEntriesCsv(transactionsWithEntries);

                return (transactionsCsv, ledgerEntriesCsv);
            }
            catch (Exception ex)
            {
                // Log error or handle exception
                Console.WriteLine($"Export failed: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Exports transactions and their associated ledger entries to CSV format into a single method
        /// </summary>
        /// <param name="transactionsWithEntries">List of transactions with their associated ledger entries</param>
        /// <returns>CSV content as string with both transactions and ledger entries</returns>
        public string ExportTransactionsToCsv(
            List<(TransactionDto Transaction, List<LedgerEntryDto> Entries)> transactionsWithEntries)
        {
            try
            {
                var sb = new StringBuilder();

                // Add transactions section header
                sb.AppendLine("# TRANSACTIONS");
                sb.AppendLine("TransactionId,Date,Description,DocumentId");

                // Write transaction data
                foreach (var (transaction, _) in transactionsWithEntries)
                {
                    string line = $"{transaction.Id}," +
                                  $"{transaction.TransactionDate:yyyy-MM-dd}," +
                                  $"\"{EscapeCsvField(transaction.Description)}\"," +
                                  $"{transaction.DocumentId}";

                    sb.AppendLine(line);
                }

                // Add a separator
                sb.AppendLine();
                sb.AppendLine("# LEDGER ENTRIES");
                sb.AppendLine("EntryId,TransactionId,AccountId,OfficialCode,AccountName,EntryType,Amount");

                // Write ledger entry data
                foreach (var (transaction, entries) in transactionsWithEntries)
                {
                    foreach (var entry in entries)
                    {
                        string line = $"{entry.Id}," +
                                      $"{transaction.Id}," +
                                      $"{entry.AccountId}," +
                                      $"\"{EscapeCsvField(entry.OfficialCode)}\"," +
                                      $"\"{EscapeCsvField(entry.AccountName)}\"," +
                                      $"{entry.EntryType}," +
                                      $"{entry.Amount}";

                        sb.AppendLine(line);
                    }
                }

                return sb.ToString();
            }
            catch (Exception ex)
            {
                // Log error or handle exception
                Console.WriteLine($"Export failed: {ex.Message}");
                throw;
            }
        }
        private string GenerateTransactionsCsv(List<TransactionDto> transactions)
        {
            var sb = new StringBuilder();

            // Write header
            sb.AppendLine("TransactionId,Date,Description,DocumentId");

            // Write transaction data
            foreach (var transaction in transactions)
            {
                string line = $"{transaction.Id}," +
                              $"{transaction.TransactionDate:yyyy-MM-dd}," +
                              $"\"{EscapeCsvField(transaction.Description)}\"," +
                              $"{transaction.DocumentId}";

                sb.AppendLine(line);
            }

            return sb.ToString();
        }
        private string GenerateLedgerEntriesCsv(
            List<(TransactionDto Transaction, List<LedgerEntryDto> Entries)> transactionsWithEntries)
        {
            var sb = new StringBuilder();

            // Write header
            sb.AppendLine("EntryId,TransactionId,AccountId,OfficialCode,AccountName,EntryType,Amount");

            // Write ledger entry data
            foreach (var (transaction, entries) in transactionsWithEntries)
            {
                foreach (var entry in entries)
                {
                    string line = $"{entry.Id}," +
                                  $"{transaction.Id}," +
                                  $"{entry.AccountId}," +
                                  $"\"{EscapeCsvField(entry.OfficialCode)}\"," +
                                  $"\"{EscapeCsvField(entry.AccountName)}\"," +
                                  $"{entry.EntryType}," +
                                  $"{entry.Amount}";

                    sb.AppendLine(line);
                }
            }

            return sb.ToString();
        }

        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return string.Empty;

            // Double quotes need to be escaped with another double quote
            return field.Replace("\"", "\"\"");
        }

        /// <summary>
        /// Imports transactions and their associated ledger entries from a CSV string 
        /// formatted by ExportTransactionsToCsv
        /// </summary>
        /// <param name="csvText">CSV text containing transactions and ledger entries</param>
        /// <returns>List of transactions with their associated ledger entries</returns>
        public List<(TransactionDto Transaction, List<LedgerEntryDto> Entries)> ImportFromCsv(string csvText)
        {
            var result = new List<(TransactionDto Transaction, List<LedgerEntryDto> Entries)>();
            var transactions = new Dictionary<Guid, TransactionDto>();
            var entriesByTransactionId = new Dictionary<Guid, List<LedgerEntryDto>>();

            var lines = csvText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            // Track which section we're in
            bool inTransactionSection = false;
            bool inLedgerEntrySection = false;

            foreach (var line in lines)
            {
                // Skip empty lines
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // Check for section headers
                if (line.StartsWith("# TRANSACTIONS"))
                {
                    inTransactionSection = true;
                    inLedgerEntrySection = false;
                    continue;
                }
                else if (line.StartsWith("# LEDGER ENTRIES"))
                {
                    inTransactionSection = false;
                    inLedgerEntrySection = true;
                    continue;
                }

                // Skip headers
                if (line.StartsWith("TransactionId,") || line.StartsWith("EntryId,"))
                    continue;

                // Parse lines in the corresponding section
                if (inTransactionSection)
                {
                    var transaction = ParseTransactionLine(line);
                    if (transaction != null)
                    {
                        transactions[transaction.Id] = transaction;
                        entriesByTransactionId[transaction.Id] = new List<LedgerEntryDto>();
                    }
                }
                else if (inLedgerEntrySection)
                {
                    var (entry, transactionId) = ParseLedgerEntryLine(line);
                    if (entry != null && transactions.ContainsKey(transactionId))
                    {
                        entriesByTransactionId[transactionId].Add(entry);
                    }
                }
            }

            // Combine transactions and entries
            foreach (var transactionId in transactions.Keys)
            {
                result.Add((transactions[transactionId], entriesByTransactionId[transactionId]));
            }

            Console.WriteLine($"Imported {transactions.Count} transactions with {entriesByTransactionId.Values.Sum(e => e.Count)} total ledger entries");
            return result;
        }

        private TransactionDto? ParseTransactionLine(string line)
        {
            try
            {
                var parts = ParseCsvLine(line);
                if (parts.Length < 4)
                    return null;

                return new TransactionDto
                {
                    Id = Guid.Parse(parts[0]),
                    TransactionDate = DateOnly.Parse(parts[1]),
                    Description = parts[2],
                    DocumentId = Guid.Parse(parts[3])
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to parse transaction: {ex.Message}");
                return null;
            }
        }

        private (LedgerEntryDto? Entry, Guid TransactionId) ParseLedgerEntryLine(string line)
        {
            try
            {
                var parts = ParseCsvLine(line);
                if (parts.Length < 7)
                    return (null, Guid.Empty);

                var transactionId = Guid.Parse(parts[1]);

                var entry = new LedgerEntryDto
                {
                    Id = Guid.Parse(parts[0]),
                    TransactionId = transactionId,
                    AccountId = Guid.Parse(parts[2]),
                    OfficialCode = parts[3],
                    AccountName = parts[4],
                    EntryType = Enum.Parse<EntryType>(parts[5]),
                    Amount = decimal.Parse(parts[6], CultureInfo.InvariantCulture)
                };

                return (entry, transactionId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to parse ledger entry: {ex.Message}");
                return (null, Guid.Empty);
            }
        }

        private string[] ParseCsvLine(string line)
        {
            var result = new List<string>();
            bool inQuotes = false;
            var currentField = new StringBuilder();

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i < line.Length - 1 && line[i + 1] == '"')
                    {
                        // Escaped quote inside quotes - add a single quote and skip the next char
                        currentField.Append('"');
                        i++; // Skip the next quote
                    }
                    else
                    {
                        // Toggle quote mode
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    // End of field
                    result.Add(currentField.ToString());
                    currentField.Clear();
                }
                else
                {
                    currentField.Append(c);
                }
            }

            // Add the last field
            result.Add(currentField.ToString());

            return result.ToArray();
        }
    }
}
