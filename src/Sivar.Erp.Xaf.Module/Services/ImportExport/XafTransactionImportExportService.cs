using DevExpress.ExpressApp;
using DevExpress.Data.Filtering;
using Microsoft.Extensions.Logging;
using Sivar.Erp.Core.Enums;
using Sivar.Erp.EfCore.Entities;
using Sivar.Erp.Modules.Accounting.Transactions;
using Sivar.Erp.Modules.ImportExport;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace Sivar.Erp.Xaf.Module.Services.ImportExport
{
    /// <summary>
    /// XAF implementation of transaction import/export service using IObjectSpace
    /// </summary>
    public class XafTransactionImportExportService : ITransactionImportExportService
    {
        private readonly IObjectSpace _objectSpace;
        private readonly ILogger<XafTransactionImportExportService>? _logger;

        /// <summary>
        /// Initializes a new instance of the XafTransactionImportExportService class
        /// </summary>
        /// <param name="objectSpace">XAF ObjectSpace for data operations</param>
        /// <param name="logger">Optional logger for diagnostic information</param>
        public XafTransactionImportExportService(
            IObjectSpace objectSpace,
            ILogger<XafTransactionImportExportService>? logger = null)
        {
            _objectSpace = objectSpace ?? throw new ArgumentNullException(nameof(objectSpace));
            _logger = logger;
        }

        /// <summary>
        /// Exports transactions and their associated ledger entries to CSV format
        /// </summary>
        /// <param name="transactionsWithEntries">List of transactions with their associated ledger entries</param>
        /// <returns>Tuple containing the transactions CSV and ledger entries CSV as strings</returns>
        public async Task<(string TransactionsCsv, string LedgerEntriesCsv)> ExportTransactionsAsync(
            List<(TransactionDto Transaction, List<LedgerEntryDto> Entries)> transactionsWithEntries)
        {
            try
            {
                _logger?.LogDebug("Exporting {Count} transactions to separate CSV files", transactionsWithEntries.Count);

                await Task.CompletedTask; // Make method async

                string transactionsCsv = GenerateTransactionsCsv(transactionsWithEntries.Select(x => x.Transaction).ToList());
                string ledgerEntriesCsv = GenerateLedgerEntriesCsv(transactionsWithEntries);

                _logger?.LogDebug("Successfully exported transactions to CSV files");

                return (transactionsCsv, ledgerEntriesCsv);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error exporting transactions to CSV");
                throw;
            }
        }

        /// <summary>
        /// Exports transactions and their associated ledger entries to a single CSV format
        /// </summary>
        /// <param name="transactionsWithEntries">List of transactions with their associated ledger entries</param>
        /// <returns>CSV content as string with both transactions and ledger entries</returns>
        public async Task<string> ExportTransactionsToCsvAsync(
            List<(ITransaction Transaction, IEnumerable<ILedgerEntry> Entries)> transactionsWithEntries)
        {
            try
            {
                _logger?.LogDebug("Exporting {Count} transactions to single CSV format", transactionsWithEntries.Count);

                await Task.CompletedTask; // Make method async

                var sb = new StringBuilder();

                // Add transactions section header
                sb.AppendLine("# TRANSACTIONS");
                sb.AppendLine("TransactionId,Date,Description,DocumentId");

                // Write transaction data
                foreach (var (transaction, _) in transactionsWithEntries)
                {
                    string line = $"{transaction.TransactionNumber}," +
                                  $"{transaction.TransactionDate:yyyy-MM-dd}," +
                                  $"\"{EscapeCsvField(transaction.Description ?? string.Empty)}\"," +
                                  $"{transaction.DocumentNumber ?? string.Empty}";

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
                        string line = $"{entry.LedgerEntryNumber ?? string.Empty}," +
                                      $"{transaction.TransactionNumber}," +
                                      $"\"{EscapeCsvField(entry.OfficialCode ?? string.Empty)}\"," +
                                      $"\"{EscapeCsvField(entry.AccountName ?? string.Empty)}\"," +
                                      $"{entry.EntryType}," +
                                      $"{entry.Amount}";

                        sb.AppendLine(line);
                    }
                }

                _logger?.LogDebug("Successfully exported transactions to single CSV format");

                return sb.ToString();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error exporting transactions to single CSV format");
                throw;
            }
        }

        /// <summary>
        /// Imports transactions and their associated ledger entries from a CSV string 
        /// formatted by ExportTransactionsToCsv
        /// </summary>
        /// <param name="csvText">CSV text containing transactions and ledger entries</param>
        /// <returns>List of transactions with their associated ledger entries and any import errors</returns>
        public async Task<(List<(ITransaction Transaction, IEnumerable<ILedgerEntry> Entries)> ImportedData, IEnumerable<string> Errors)> ImportFromCsvAsync(string csvText)
        {
            var result = new List<(ITransaction Transaction, IEnumerable<ILedgerEntry> Entries)>();
            var errors = new List<string>();

            try
            {
                _logger?.LogDebug("Starting transaction import from CSV");

                await Task.CompletedTask; // Make method async

                if (string.IsNullOrWhiteSpace(csvText))
                {
                    errors.Add("CSV content is empty");
                    return (result, errors);
                }

                var transactions = new Dictionary<string, ITransaction>();
                var entriesByTransactionId = new Dictionary<string, List<ILedgerEntry>>();

                var lines = csvText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                // Track which section we're in
                bool inTransactionSection = false;
                bool inLedgerEntrySection = false;
                int lineNumber = 0;

                foreach (var line in lines)
                {
                    lineNumber++;

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
                        var (transaction, parseError) = ParseTransactionLine(line, lineNumber);
                        if (transaction != null)
                        {
                            transactions[transaction.TransactionNumber] = transaction;
                            entriesByTransactionId[transaction.TransactionNumber] = new List<ILedgerEntry>();
                        }
                        else if (!string.IsNullOrEmpty(parseError))
                        {
                            errors.Add(parseError);
                        }
                    }
                    else if (inLedgerEntrySection)
                    {
                        var (entry, transactionId, parseError) = ParseLedgerEntryLine(line, lineNumber);
                        if (entry != null && !string.IsNullOrEmpty(transactionId))
                        {
                            if (transactions.ContainsKey(transactionId))
                            {
                                entriesByTransactionId[transactionId].Add(entry);
                            }
                            else
                            {
                                errors.Add($"Line {lineNumber}: Transaction {transactionId} not found for ledger entry");
                            }
                        }
                        else if (!string.IsNullOrEmpty(parseError))
                        {
                            errors.Add(parseError);
                        }
                    }
                }

                // Combine transactions and entries
                foreach (var transactionId in transactions.Keys)
                {
                    ITransaction transaction = transactions[transactionId];
                    IEnumerable<ILedgerEntry> ledgerEntryDtos = entriesByTransactionId[transactionId];
                    result.Add((transaction, ledgerEntryDtos));
                }

                _logger?.LogDebug("Imported {TransactionCount} transactions with {EntryCount} total ledger entries, {ErrorCount} errors",
                    transactions.Count, entriesByTransactionId.Values.Sum(e => e.Count), errors.Count);

                return (result, errors);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error importing transactions from CSV");
                errors.Add($"Import failed: {ex.Message}");
                return (result, errors);
            }
        }

        /// <summary>
        /// Generates CSV content for transactions
        /// </summary>
        /// <param name="transactions">List of transactions</param>
        /// <returns>CSV content</returns>
        private string GenerateTransactionsCsv(List<TransactionDto> transactions)
        {
            var sb = new StringBuilder();

            // Write header
            sb.AppendLine("TransactionId,Date,Description,DocumentId");

            // Write transaction data
            foreach (var transaction in transactions)
            {
                string line = $"{transaction.TransactionNumber}," +
                              $"{transaction.TransactionDate:yyyy-MM-dd}," +
                              $"\"{EscapeCsvField(transaction.Description ?? string.Empty)}\"," +
                              $"{transaction.DocumentNumber ?? string.Empty}";

                sb.AppendLine(line);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Generates CSV content for ledger entries
        /// </summary>
        /// <param name="transactionsWithEntries">Transactions with their entries</param>
        /// <returns>CSV content</returns>
        private string GenerateLedgerEntriesCsv(List<(TransactionDto Transaction, List<LedgerEntryDto> Entries)> transactionsWithEntries)
        {
            var sb = new StringBuilder();

            // Write header
            sb.AppendLine("EntryId,TransactionId,OfficialCode,AccountName,EntryType,Amount");

            // Write ledger entry data
            foreach (var (transaction, entries) in transactionsWithEntries)
            {
                foreach (var entry in entries)
                {
                    string line = $"{entry.LedgerEntryNumber ?? string.Empty}," +
                                  $"{transaction.TransactionNumber}," +
                                  $"\"{EscapeCsvField(entry.OfficialCode ?? string.Empty)}\"," +
                                  $"\"{EscapeCsvField(entry.AccountName ?? string.Empty)}\"," +
                                  $"{entry.EntryType}," +
                                  $"{entry.Amount}";

                    sb.AppendLine(line);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Escapes CSV field content
        /// </summary>
        /// <param name="field">Field to escape</param>
        /// <returns>Escaped field</returns>
        private static string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return string.Empty;

            // Double quotes need to be escaped with another double quote
            return field.Replace("\"", "\"\"");
        }

        /// <summary>
        /// Parses a transaction line from CSV
        /// </summary>
        /// <param name="line">CSV line</param>
        /// <param name="lineNumber">Line number for error reporting</param>
        /// <returns>Parsed transaction and any error message</returns>
        private (TransactionDto? Transaction, string? Error) ParseTransactionLine(string line, int lineNumber)
        {
            try
            {
                var parts = ParseCsvLine(line);
                if (parts.Length < 4)
                    return (null, $"Line {lineNumber}: Transaction line does not have enough fields (expected 4, got {parts.Length})");

                if (!DateOnly.TryParse(parts[1], out var transactionDate))
                    return (null, $"Line {lineNumber}: Invalid transaction date '{parts[1]}'");

                return (new TransactionDto
                {
                    TransactionNumber = parts[0],
                    TransactionDate = transactionDate,
                    Description = parts[2],
                    DocumentNumber = parts[3]
                }, null);
            }
            catch (Exception ex)
            {
                return (null, $"Line {lineNumber}: Failed to parse transaction - {ex.Message}");
            }
        }

        /// <summary>
        /// Parses a ledger entry line from CSV
        /// </summary>
        /// <param name="line">CSV line</param>
        /// <param name="lineNumber">Line number for error reporting</param>
        /// <returns>Parsed ledger entry, transaction ID, and any error message</returns>
        private (LedgerEntryDto? Entry, string TransactionId, string? Error) ParseLedgerEntryLine(string line, int lineNumber)
        {
            try
            {
                var parts = ParseCsvLine(line);
                if (parts.Length < 7)
                    return (null, string.Empty, $"Line {lineNumber}: Ledger entry line does not have enough fields (expected 7, got {parts.Length})");

                var transactionId = parts[1];
                var officialCode = parts[2];

                // Validate account exists using XAF ObjectSpace
                var accountCriteria = new BinaryOperator("OfficialCode", officialCode);
                var account = _objectSpace.FindObject<Account>(accountCriteria);
                if (account == null)
                {
                    _logger?.LogWarning("Account code '{OfficialCode}' not found in chart of accounts", officialCode);
                }

                if (!Enum.TryParse<EntryType>(parts[4], out var entryType))
                    return (null, string.Empty, $"Line {lineNumber}: Invalid entry type '{parts[4]}'");

                if (!decimal.TryParse(parts[5], NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
                    return (null, string.Empty, $"Line {lineNumber}: Invalid amount '{parts[5]}'");

                var entry = new LedgerEntryDto
                {
                    LedgerEntryNumber = parts[0],
                    TransactionNumber = transactionId,
                    OfficialCode = officialCode,
                    AccountName = parts[3],
                    EntryType = entryType,
                    Amount = amount
                };

                return (entry, transactionId, null);
            }
            catch (Exception ex)
            {
                return (null, string.Empty, $"Line {lineNumber}: Failed to parse ledger entry - {ex.Message}");
            }
        }

        /// <summary>
        /// Parses a CSV line handling quoted fields
        /// </summary>
        /// <param name="line">CSV line to parse</param>
        /// <returns>Array of parsed fields</returns>
        private static string[] ParseCsvLine(string line)
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
