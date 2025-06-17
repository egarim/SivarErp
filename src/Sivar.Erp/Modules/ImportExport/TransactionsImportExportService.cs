using Sivar.Erp.Services.Accounting.ChartOfAccounts;
using Sivar.Erp.Services.Accounting.Transactions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Sivar.Erp.Services.ImportExport
{
    /// <summary>
    /// Helper to import transactions and ledger entries from tabular text.
    /// </summary>
    public class TransactionsImportExportService
    {
        private readonly List<AccountDto> _accounts;

        public TransactionsImportExportService(List<AccountDto> accounts)
        {
            _accounts = accounts;

        }

        /// <summary>
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
                    string line = $"{transaction.Oid}," +
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
                        string line = $"{entry.Oid}," +
                                      $"{transaction.Oid}," +
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
                string line = $"{transaction.Oid}," +
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
                    string line = $"{entry.Oid}," +
                                  $"{transaction.Oid}," +
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
                        transactions[transaction.Oid] = transaction;
                        entriesByTransactionId[transaction.Oid] = new List<LedgerEntryDto>();
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
                    Oid = Guid.Parse(parts[0]),
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
                var officialCode = parts[3];

                // Look up AccountId using OfficialCode
                //if (!_accountCodeToId.TryGetValue(officialCode, out var accountId))
                //{
                //    Console.WriteLine($"Account code '{officialCode}' not found in provided accounts.");
                //    accountId = Guid.Parse(parts[2]); // Fallback to the AccountId from CSV
                //}

                var accountId= _accounts.FirstOrDefault(a => a.OfficialCode == officialCode)?.OfficialCode ?? string.Empty;
                Console.WriteLine($"Account code '{officialCode}' not found in provided accounts.");

                var entry = new LedgerEntryDto
                {
                    Oid = Guid.Parse(parts[0]),
                    TransactionId = transactionId,
                    OfficialCode = officialCode,
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
