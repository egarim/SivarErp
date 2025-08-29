using DevExpress.ExpressApp;
using DevExpress.Data.Filtering;
using Microsoft.Extensions.Logging;
using Sivar.Erp.EfCore.Entities;
using Sivar.Erp.Modules.Accounting.Transactions;
using Sivar.Erp.Modules.ImportExport;
using System.Text;
using System.Globalization;

#nullable enable

namespace Sivar.Erp.Xaf.Module.Services.ImportExport
{
    /// <summary>
    /// XAF implementation of transaction import/export service using IObjectSpace, following the same pattern as XafAccountImportExportService
    /// </summary>
    public class XafTransactionsImportExportService : ITransactionImportExportService
    {
        private readonly IObjectSpace _objectSpace;
        private readonly ILogger<XafTransactionsImportExportService>? _logger;

        /// <summary>
        /// Initializes a new instance of the XafTransactionsImportExportService class
        /// </summary>
        /// <param name="objectSpace">XAF ObjectSpace for data operations</param>
        /// <param name="logger">Optional logger for diagnostic information</param>
        public XafTransactionsImportExportService(IObjectSpace objectSpace, ILogger<XafTransactionsImportExportService>? logger = null)
        {
            _objectSpace = objectSpace ?? throw new ArgumentNullException(nameof(objectSpace));
            _logger = logger;
        }

        /// <summary>
        /// Exports transactions and their associated ledger entries to CSV format
        /// </summary>
        /// <param name="transactionsWithEntries">List of transactions with their associated ledger entries</param>
        /// <returns>Tuple containing the transactions CSV and ledger entries CSV as strings</returns>
        public Task<(string TransactionsCsv, string LedgerEntriesCsv)> ExportTransactionsAsync(
            List<(TransactionDto Transaction, List<LedgerEntryDto> Entries)> transactionsWithEntries)
        {
            try
            {
                _logger?.LogInformation("Starting transaction export to separate CSV files");

                string transactionsCsv = GenerateTransactionsCsv(transactionsWithEntries.Select(x => x.Transaction).ToList());
                string ledgerEntriesCsv = GenerateLedgerEntriesCsv(transactionsWithEntries);

                _logger?.LogInformation("Successfully exported {Count} transactions to separate CSV files", transactionsWithEntries.Count);
                return Task.FromResult((transactionsCsv, ledgerEntriesCsv));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error exporting transactions to separate CSV files");
                throw;
            }
        }

        /// <summary>
        /// Exports transactions and their associated ledger entries to a single CSV format
        /// </summary>
        /// <param name="transactionsWithEntries">List of transactions with their associated ledger entries</param>
        /// <returns>CSV content as string with both transactions and ledger entries</returns>
        public Task<string> ExportTransactionsToCsvAsync(
            List<(ITransaction Transaction, IEnumerable<ILedgerEntry> Entries)> transactionsWithEntries)
        {
            try
            {
                _logger?.LogInformation("Starting transaction export to single CSV format");

                // If no transactions provided, get all transactions from ObjectSpace
                var transactionsToExport = transactionsWithEntries?.ToList() ?? 
                    GetAllTransactionsFromObjectSpace();

                if (!transactionsToExport.Any())
                {
                    _logger?.LogInformation("No transactions found for export");
                    return Task.FromResult(GetCombinedCsvHeader());
                }

                StringBuilder csvBuilder = new StringBuilder();

                // Add transactions section header
                csvBuilder.AppendLine("# TRANSACTIONS");
                csvBuilder.AppendLine("TransactionId,Date,Description,DocumentId");

                // Add transaction data
                foreach (var (transaction, _) in transactionsToExport)
                {
                    csvBuilder.AppendLine(GetTransactionCsvRow(transaction));
                }

                // Add separator
                csvBuilder.AppendLine();
                csvBuilder.AppendLine("# LEDGER ENTRIES");
                csvBuilder.AppendLine("EntryId,TransactionId,AccountId,OfficialCode,AccountName,EntryType,Amount");

                // Add ledger entry data
                foreach (var (transaction, entries) in transactionsToExport)
                {
                    foreach (var entry in entries)
                    {
                        csvBuilder.AppendLine(GetLedgerEntryCsvRow(entry, transaction.TransactionNumber));
                    }
                }

                _logger?.LogInformation("Successfully exported {Count} transactions to single CSV format", transactionsToExport.Count);
                return Task.FromResult(csvBuilder.ToString());
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error exporting transactions to single CSV format");
                throw;
            }
        }

        /// <summary>
        /// Imports transactions and their associated ledger entries from a CSV string using XAF ObjectSpace
        /// </summary>
        /// <param name="csvText">CSV text containing transactions and ledger entries</param>
        /// <returns>List of transactions with their associated ledger entries and any import errors</returns>
        public Task<(List<(ITransaction Transaction, IEnumerable<ILedgerEntry> Entries)> ImportedData, IEnumerable<string> Errors)> ImportFromCsvAsync(string csvText)
        {
            List<(ITransaction Transaction, IEnumerable<ILedgerEntry> Entries)> importedData = new List<(ITransaction, IEnumerable<ILedgerEntry>)>();
            List<string> errors = new List<string>();

            if (string.IsNullOrEmpty(csvText))
            {
                errors.Add("CSV content is empty");
                return Task.FromResult<(List<(ITransaction, IEnumerable<ILedgerEntry>)>, IEnumerable<string>)>((importedData, errors));
            }

            try
            {
                _logger?.LogInformation("Starting transaction import from CSV");

                // Split the CSV into lines (preserve all lines)
                string[] lines = csvText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                if (lines.Length <= 1)
                {
                    errors.Add("CSV file contains no data rows");
                    return Task.FromResult<(List<(ITransaction, IEnumerable<ILedgerEntry>)>, IEnumerable<string>)>((importedData, errors));
                }

                var transactionDtos = new Dictionary<string, TransactionDto>();
                var ledgerEntryDtos = new Dictionary<string, List<LedgerEntryDto>>();

                // Parse the CSV content
                ParseCsvContent(lines, transactionDtos, ledgerEntryDtos, errors);

                if (errors.Count > 0)
                {
                    _logger?.LogWarning("Parsing completed with {ErrorCount} errors", errors.Count);
                    return Task.FromResult<(List<(ITransaction, IEnumerable<ILedgerEntry>)>, IEnumerable<string>)>((importedData, errors));
                }

                // Validate and create XAF entities
                foreach (var transactionDto in transactionDtos.Values)
                {
                    var ledgerEntries = ledgerEntryDtos.GetValueOrDefault(transactionDto.TransactionNumber, new List<LedgerEntryDto>());

                    // Validate transaction balance
                    if (!ValidateTransactionBalance(transactionDto, ledgerEntries, errors))
                    {
                        continue;
                    }

                    // Check for duplicate transactions by TransactionNumber in the database
                    var existingTransaction = _objectSpace.FindObject<Transaction>(CriteriaOperator.Parse("TransactionNumber = ?", transactionDto.TransactionNumber));
                    if (existingTransaction != null)
                    {
                        errors.Add($"Transaction with number '{transactionDto.TransactionNumber}' already exists");
                        continue;
                    }

                    // Create XAF entities
                    var xafTransaction = CreateXafTransactionFromDto(transactionDto);
                    var xafLedgerEntries = CreateXafLedgerEntriesFromDtos(ledgerEntries, xafTransaction);

                    importedData.Add((xafTransaction, xafLedgerEntries));
                }

                // If there are no errors, commit the changes
                if (errors.Count == 0 && importedData.Count > 0)
                {
                    _objectSpace.CommitChanges();
                    _logger?.LogInformation("Successfully imported {Count} transactions", importedData.Count);
                }
                else if (errors.Count > 0)
                {
                    _logger?.LogWarning("Import completed with {ErrorCount} errors", errors.Count);
                }

                return Task.FromResult<(List<(ITransaction, IEnumerable<ILedgerEntry>)>, IEnumerable<string>)>((importedData, errors));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error importing CSV content");
                errors.Add($"Error importing CSV: {ex.Message}");
                return Task.FromResult<(List<(ITransaction, IEnumerable<ILedgerEntry>)>, IEnumerable<string>)>((importedData, errors));
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Gets all transactions from ObjectSpace for export
        /// </summary>
        /// <returns>List of transactions with their ledger entries</returns>
        private List<(ITransaction Transaction, IEnumerable<ILedgerEntry> Entries)> GetAllTransactionsFromObjectSpace()
        {
            var transactions = _objectSpace.GetObjects<Transaction>().ToList();
            return transactions.Select(t => ((ITransaction)t, (IEnumerable<ILedgerEntry>)t.LedgerEntries)).ToList();
        }

        /// <summary>
        /// Parses CSV content into transaction and ledger entry DTOs
        /// </summary>
        /// <param name="lines">CSV lines</param>
        /// <param name="transactionDtos">Dictionary to store parsed transactions</param>
        /// <param name="ledgerEntryDtos">Dictionary to store parsed ledger entries</param>
        /// <param name="errors">List to collect parsing errors</param>
        private void ParseCsvContent(string[] lines, Dictionary<string, TransactionDto> transactionDtos, 
            Dictionary<string, List<LedgerEntryDto>> ledgerEntryDtos, List<string> errors)
        {
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
                    var transaction = ParseTransactionLine(line, lineNumber, errors);
                    if (transaction != null)
                    {
                        transactionDtos[transaction.TransactionNumber] = transaction;
                        ledgerEntryDtos[transaction.TransactionNumber] = new List<LedgerEntryDto>();
                    }
                }
                else if (inLedgerEntrySection)
                {
                    var (entry, transactionId) = ParseLedgerEntryLine(line, lineNumber, errors);
                    if (entry != null && !string.IsNullOrEmpty(transactionId))
                    {
                        if (ledgerEntryDtos.ContainsKey(transactionId))
                        {
                            ledgerEntryDtos[transactionId].Add(entry);
                        }
                        else
                        {
                            errors.Add($"Line {lineNumber}: Transaction '{transactionId}' not found for ledger entry");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creates a TransactionDto from CSV fields
        /// </summary>
        /// <param name="line">CSV line to parse</param>
        /// <param name="lineNumber">Line number for error reporting</param>
        /// <param name="errors">List to collect errors</param>
        /// <returns>TransactionDto or null if parsing failed</returns>
        private TransactionDto? ParseTransactionLine(string line, int lineNumber, List<string> errors)
        {
            try
            {
                var parts = ParseCsvLine(line);
                if (parts.Length < 4)
                {
                    errors.Add($"Line {lineNumber}: Transaction line has insufficient columns (expected 4, got {parts.Length})");
                    return null;
                }

                return new TransactionDto
                {
                    TransactionNumber = parts[0],
                    TransactionDate = DateOnly.Parse(parts[1]),
                    Description = parts[2],
                    DocumentNumber = parts[3]
                };
            }
            catch (Exception ex)
            {
                errors.Add($"Line {lineNumber}: Failed to parse transaction - {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Creates a LedgerEntryDto from CSV fields
        /// </summary>
        /// <param name="line">CSV line to parse</param>
        /// <param name="lineNumber">Line number for error reporting</param>
        /// <param name="errors">List to collect errors</param>
        /// <returns>Tuple of LedgerEntryDto, TransactionId, or nulls if parsing failed</returns>
        private (LedgerEntryDto? Entry, string TransactionId) ParseLedgerEntryLine(string line, int lineNumber, List<string> errors)
        {
            try
            {
                var parts = ParseCsvLine(line);
                if (parts.Length < 7)
                {
                    errors.Add($"Line {lineNumber}: Ledger entry line has insufficient columns (expected 7, got {parts.Length})");
                    return (null, string.Empty);
                }

                var transactionId = parts[1];
                
                // Validate account exists
                var account = _objectSpace.FindObject<Account>(CriteriaOperator.Parse("OfficialCode = ?", parts[3]));
                if (account == null)
                {
                    errors.Add($"Line {lineNumber}: Account with official code '{parts[3]}' not found");
                    return (null, transactionId);
                }

                var entry = new LedgerEntryDto
                {
                    LedgerEntryNumber = parts[0],
                    TransactionNumber = transactionId,
                    OfficialCode = parts[3],
                    AccountName = parts[4],
                    EntryType = Enum.Parse<EntryType>(parts[5]),
                    Amount = decimal.Parse(parts[6], CultureInfo.InvariantCulture)
                };

                return (entry, transactionId);
            }
            catch (Exception ex)
            {
                errors.Add($"Line {lineNumber}: Failed to parse ledger entry - {ex.Message}");
                return (null, string.Empty);
            }
        }

        /// <summary>
        /// Validates that a transaction's debits equal credits
        /// </summary>
        /// <param name="transaction">Transaction to validate</param>
        /// <param name="ledgerEntries">Associated ledger entries</param>
        /// <param name="errors">List to collect errors</param>
        /// <returns>True if valid, false otherwise</returns>
        private bool ValidateTransactionBalance(TransactionDto transaction, List<LedgerEntryDto> ledgerEntries, List<string> errors)
        {
            if (!ledgerEntries.Any())
            {
                errors.Add($"Transaction '{transaction.TransactionNumber}' has no ledger entries");
                return false;
            }

            decimal totalDebits = ledgerEntries.Where(e => e.EntryType == EntryType.Debit).Sum(e => e.Amount);
            decimal totalCredits = ledgerEntries.Where(e => e.EntryType == EntryType.Credit).Sum(e => e.Amount);

            if (Math.Abs(totalDebits - totalCredits) > 0.01m)
            {
                errors.Add($"Transaction '{transaction.TransactionNumber}' is not balanced. Debits: {totalDebits:F2}, Credits: {totalCredits:F2}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Creates a XAF Transaction entity from a TransactionDto
        /// </summary>
        /// <param name="transactionDto">Transaction DTO</param>
        /// <returns>New XAF Transaction with populated properties</returns>
        private Transaction CreateXafTransactionFromDto(TransactionDto transactionDto)
        {
            var transaction = _objectSpace.CreateObject<Transaction>();
            var currentTime = DateTime.UtcNow;

            // Copy data from DTO to XAF entity
            transaction.TransactionNumber = transactionDto.TransactionNumber;
            transaction.TransactionDate = transactionDto.TransactionDate;
            transaction.Description = transactionDto.Description;
            transaction.DocumentNumber = transactionDto.DocumentNumber;
            transaction.IsPosted = false;

            // Set audit fields
            transaction.InsertedBy = "ImportUser"; // TODO: Get actual user
            transaction.InsertedAt = currentTime;
            transaction.UpdatedBy = "ImportUser"; // TODO: Get actual user
            transaction.UpdatedAt = currentTime;

            return transaction;
        }

        /// <summary>
        /// Creates XAF LedgerEntry entities from LedgerEntryDtos
        /// </summary>
        /// <param name="ledgerEntryDtos">Ledger entry DTOs</param>
        /// <param name="xafTransaction">Parent XAF transaction</param>
        /// <returns>List of XAF LedgerEntry entities</returns>
        private List<ILedgerEntry> CreateXafLedgerEntriesFromDtos(List<LedgerEntryDto> ledgerEntryDtos, Transaction xafTransaction)
        {
            var ledgerEntries = new List<ILedgerEntry>();
            var currentTime = DateTime.UtcNow;

            foreach (var dto in ledgerEntryDtos)
            {
                var ledgerEntry = _objectSpace.CreateObject<LedgerEntry>();

                // Copy data from DTO to XAF entity
                ledgerEntry.LedgerEntryNumber = dto.LedgerEntryNumber;
                ledgerEntry.TransactionNumber = xafTransaction.TransactionNumber;
                ledgerEntry.Transaction = xafTransaction; // Set navigation property, XAF handles foreign key
                ledgerEntry.EntryType = dto.EntryType;
                ledgerEntry.Amount = dto.Amount;
                ledgerEntry.AccountName = dto.AccountName;
                ledgerEntry.OfficialCode = dto.OfficialCode;

                // Find and link the account
                var account = _objectSpace.FindObject<Account>(CriteriaOperator.Parse("OfficialCode = ?", dto.OfficialCode));
                if (account != null)
                {
                    ledgerEntry.Account = account; // Set navigation property, XAF handles foreign key
                }

                // Set audit fields
                ledgerEntry.InsertedBy = "ImportUser"; // TODO: Get actual user
                ledgerEntry.InsertedAt = currentTime;
                ledgerEntry.UpdatedBy = "ImportUser"; // TODO: Get actual user
                ledgerEntry.UpdatedAt = currentTime;

                // Add to transaction's collection
                xafTransaction.LedgerEntries.Add(ledgerEntry);
                ledgerEntries.Add(ledgerEntry);
            }

            return ledgerEntries;
        }

        /// <summary>
        /// Parses a CSV line into fields, handling quoted values
        /// </summary>
        /// <param name="line">CSV line to parse</param>
        /// <returns>Array of fields</returns>
        private string[] ParseCsvLine(string line)
        {
            List<string> fields = new List<string>();
            bool inQuotes = false;
            int startIndex = 0;

            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (line[i] == ',' && !inQuotes)
                {
                    string field = line.Substring(startIndex, i - startIndex);
                    field = field.Trim('"'); // Remove surrounding quotes
                    fields.Add(field);
                    startIndex = i + 1;
                }
            }

            // Add the last field
            string lastField = line.Substring(startIndex);
            lastField = lastField.Trim('"'); // Remove surrounding quotes
            fields.Add(lastField);

            return fields.ToArray();
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
                sb.AppendLine(GetTransactionCsvRow(transaction));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Generates CSV content for ledger entries
        /// </summary>
        /// <param name="transactionsWithEntries">Transactions with their ledger entries</param>
        /// <returns>CSV content</returns>
        private string GenerateLedgerEntriesCsv(List<(TransactionDto Transaction, List<LedgerEntryDto> Entries)> transactionsWithEntries)
        {
            var sb = new StringBuilder();

            // Write header
            sb.AppendLine("LedgerEntryNumber,TransactionNumber,AccountId,OfficialCode,AccountName,EntryType,Amount");

            // Write ledger entry data
            foreach (var (transaction, entries) in transactionsWithEntries)
            {
                foreach (var entry in entries)
                {
                    sb.AppendLine(GetLedgerEntryCsvRow(entry, transaction.TransactionNumber));
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets the CSV header for combined format
        /// </summary>
        /// <returns>CSV header as a string</returns>
        private string GetCombinedCsvHeader()
        {
            return "# TRANSACTIONS\nTransactionId,Date,Description,DocumentId\n\n# LEDGER ENTRIES\nEntryId,TransactionId,AccountId,OfficialCode,AccountName,EntryType,Amount";
        }

        /// <summary>
        /// Gets a CSV row for a transaction
        /// </summary>
        /// <param name="transaction">Transaction to convert to CSV</param>
        /// <returns>CSV row as a string</returns>
        private string GetTransactionCsvRow(ITransaction transaction)
        {
            return $"{transaction.TransactionNumber}," +
                   $"{transaction.TransactionDate:yyyy-MM-dd}," +
                   $"\"{EscapeCsvField(transaction.Description ?? string.Empty)}\"," +
                   $"{transaction.DocumentNumber ?? string.Empty}";
        }

        /// <summary>
        /// Gets a CSV row for a ledger entry
        /// </summary>
        /// <param name="entry">Ledger entry to convert to CSV</param>
        /// <param name="transactionNumber">Transaction number</param>
        /// <returns>CSV row as a string</returns>
        private string GetLedgerEntryCsvRow(ILedgerEntry entry, string transactionNumber)
        {
            return $"{entry.LedgerEntryNumber}," +
                   $"{transactionNumber}," +
                   $"\"{EscapeCsvField(entry.OfficialCode)}\"," +
                   $"\"{EscapeCsvField(entry.AccountName)}\"," +
                   $"{entry.EntryType}," +
                   $"{entry.Amount}";
        }

        /// <summary>
        /// Escapes CSV field content
        /// </summary>
        /// <param name="field">Field to escape</param>
        /// <returns>Escaped field</returns>
        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return string.Empty;

            // Double quotes need to be escaped with another double quote
            return field.Replace("\"", "\"\"");
        }

        #endregion
    }
}
