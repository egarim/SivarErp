# Journal Entry Functionality

This document describes the journal entry viewing functionality added to the SivarErp accounting system.

## Overview

The journal entry functionality allows you to:

- View journal entries (ledger entries) for posted transactions
- Query journal entries with flexible filtering options
- Generate comprehensive reports and audit trails
- Validate transaction balances
- Track account activity over time

## Architecture

The functionality is built around your existing accounting infrastructure:

### Core Components

1. **IJournalEntryService** - Service for querying journal entries
2. **IJournalEntryReportService** - Service for generating reports
3. **JournalEntryQueryOptions** - Flexible query options
4. **Report DTOs** - Structured report data

### Integration Points

- **AccountingModule** - Extended to expose journal entry operations
- **ILedgerEntry** - Uses your existing ledger entry model
- **ITransaction** - Works with your transaction system
- **IObjectDb** - Leverages your data access layer

## Services

### IJournalEntryService

Core service for querying journal entries:

```csharp
// Get journal entries with filtering
var options = new JournalEntryQueryOptions
{
    FromDate = new DateOnly(2025, 1, 1),
    ToDate = new DateOnly(2025, 6, 30),
    OnlyPosted = true,
    Take = 100
};
var entries = await journalEntryService.GetJournalEntriesAsync(options);

// Get entries for a specific transaction
var transactionEntries = await journalEntryService.GetJournalEntriesByTransactionAsync("TRANS-001");

// Validate transaction balance
var isBalanced = await journalEntryService.IsTransactionBalancedAsync("TRANS-001");
```

### IJournalEntryReportService

Service for generating comprehensive reports:

```csharp
// Generate journal entry report
var report = await reportService.GenerateJournalEntryReportAsync(options);

// Generate transaction audit trail
var auditTrail = await reportService.GenerateTransactionAuditTrailAsync("TRANS-001");

// Generate account activity report
var activityReport = await reportService.GenerateAccountActivityReportAsync("1100", fromDate, toDate);

// Generate trial balance from journal entries
var trialBalance = await reportService.GenerateTrialBalanceFromJournalEntriesAsync(asOfDate);
```

## Query Options

The `JournalEntryQueryOptions` class provides flexible filtering:

```csharp
public class JournalEntryQueryOptions
{
    public DateOnly? FromDate { get; set; }           // Filter by date range
    public DateOnly? ToDate { get; set; }
    public string? AccountCode { get; set; }          // Filter by account
    public string? TransactionNumber { get; set; }    // Filter by transaction
    public EntryType? EntryType { get; set; }         // Filter by debit/credit
    public string? DocumentNumber { get; set; }       // Filter by document
    public bool? OnlyPosted { get; set; }            // Only posted transactions
    public int? Skip { get; set; }                   // Pagination
    public int? Take { get; set; }
    public string? SortBy { get; set; }              // Sorting
    public bool SortDescending { get; set; }
}
```

## AccountingModule Integration

The `AccountingModule` has been extended with journal entry methods:

```csharp
// Through AccountingModule
var accountingModule = serviceProvider.GetService<IAccountingModule>();

// Get transaction journal entries
var journalEntries = await accountingModule.GetTransactionJournalEntriesAsync("TRANS-001");

// Validate transaction balance
var isBalanced = await accountingModule.ValidateTransactionBalanceAsync("TRANS-001");

// Query journal entries
var entries = await accountingModule.GetJournalEntriesAsync(queryOptions);

// Generate reports
var report = await accountingModule.GenerateJournalReportAsync(queryOptions);
var auditTrail = await accountingModule.GenerateTransactionAuditTrailAsync("TRANS-001");
```

## Report Types

### 1. Journal Entry Report

Comprehensive report of journal entries with totals and validation:

```csharp
public class JournalEntryReportDto
{
    public string ReportTitle { get; set; }
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public IEnumerable<ILedgerEntry> Entries { get; set; }
    public decimal TotalDebits { get; set; }
    public decimal TotalCredits { get; set; }
    public int TotalEntries { get; set; }
    public bool IsBalanced { get; set; }
    public DateTime GeneratedAt { get; set; }
}
```

### 2. Transaction Audit Trail

Detailed audit trail for a specific transaction:

```csharp
public class TransactionAuditTrailDto
{
    public string TransactionNumber { get; set; }
    public string DocumentNumber { get; set; }
    public DateOnly TransactionDate { get; set; }
    public string Description { get; set; }
    public bool IsPosted { get; set; }
    public IEnumerable<ILedgerEntry> JournalEntries { get; set; }
    public decimal TotalDebits { get; set; }
    public decimal TotalCredits { get; set; }
    public bool IsBalanced { get; set; }
    public IEnumerable<string> AffectedAccounts { get; set; }
}
```

### 3. Account Activity Report

Activity report for a specific account over a date range:

```csharp
public class AccountActivityReportDto
{
    public string AccountCode { get; set; }
    public string AccountName { get; set; }
    public DateOnly FromDate { get; set; }
    public DateOnly ToDate { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal ClosingBalance { get; set; }
    public IEnumerable<ILedgerEntry> Entries { get; set; }
    public decimal TotalDebits { get; set; }
    public decimal TotalCredits { get; set; }
    public int TotalTransactions { get; set; }
}
```

### 4. Trial Balance Report

Trial balance generated from journal entries:

```csharp
public class TrialBalanceReportDto
{
    public DateOnly AsOfDate { get; set; }
    public bool OnlyPostedTransactions { get; set; }
    public IEnumerable<TrialBalanceAccountDto> Accounts { get; set; }
    public decimal TotalDebits { get; set; }
    public decimal TotalCredits { get; set; }
    public bool IsBalanced { get; set; }
}
```

## Usage Examples

### Basic Journal Entry Viewing

```csharp
// View all posted journal entries for the last month
var options = new JournalEntryQueryOptions
{
    FromDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-30)),
    ToDate = DateOnly.FromDateTime(DateTime.Today),
    OnlyPosted = true
};

var entries = await accountingModule.GetJournalEntriesAsync(options);

foreach (var entry in entries)
{
    Console.WriteLine($"{entry.TransactionNumber} | {entry.OfficialCode} | {entry.EntryType} | {entry.Amount:C}");
}
```

### Transaction Analysis

```csharp
// Analyze a specific transaction
var transactionNumber = "TRANS-001";

// Get all journal entries for the transaction
var journalEntries = await accountingModule.GetTransactionJournalEntriesAsync(transactionNumber);

// Check if transaction is balanced
var isBalanced = await accountingModule.ValidateTransactionBalanceAsync(transactionNumber);

// Generate detailed audit trail
var auditTrail = await accountingModule.GenerateTransactionAuditTrailAsync(transactionNumber);

Console.WriteLine($"Transaction {transactionNumber}:");
Console.WriteLine($"Posted: {auditTrail.IsPosted}");
Console.WriteLine($"Balanced: {auditTrail.IsBalanced}");
Console.WriteLine($"Total Debits: {auditTrail.TotalDebits:C}");
Console.WriteLine($"Total Credits: {auditTrail.TotalCredits:C}");
Console.WriteLine($"Affected Accounts: {string.Join(", ", auditTrail.AffectedAccounts)}");
```

### Account Activity Analysis

```csharp
// Analyze account activity
var accountCode = "1100"; // Cash account
var fromDate = new DateOnly(2025, 1, 1);
var toDate = new DateOnly(2025, 6, 30);

var activityReport = await reportService.GenerateAccountActivityReportAsync(accountCode, fromDate, toDate);

Console.WriteLine($"Account {activityReport.AccountCode} - {activityReport.AccountName}");
Console.WriteLine($"Opening Balance: {activityReport.OpeningBalance:C}");
Console.WriteLine($"Period Activity:");
Console.WriteLine($"  Debits: {activityReport.TotalDebits:C}");
Console.WriteLine($"  Credits: {activityReport.TotalCredits:C}");
Console.WriteLine($"Closing Balance: {activityReport.ClosingBalance:C}");
Console.WriteLine($"Total Transactions: {activityReport.TotalTransactions}");
```

## Dependency Injection Setup

Register the services in your DI container:

```csharp
// In your service registration
services.AddScoped<IJournalEntryService, JournalEntryService>();
services.AddScoped<IJournalEntryReportService, JournalEntryReportService>();

// Update AccountingModule registration to include new dependencies
services.AddScoped<IAccountingModule, AccountingModule>();
```

Or use the extension method:

```csharp
services.AddJournalEntryServices();
```

## Performance Considerations

1. **Pagination** - Use `Skip` and `Take` for large result sets
2. **Date Filtering** - Always include date ranges for better performance
3. **Posted Only** - Set `OnlyPosted = true` to exclude draft transactions
4. **Specific Queries** - Use specific filters (account, transaction) when possible

## Integration with Existing Code

The journal entry functionality integrates seamlessly with your existing codebase:

- Uses your existing `ILedgerEntry` and `ITransaction` interfaces
- Works with your `IObjectDb` data access layer
- Extends your `AccountingModule` without breaking changes
- Follows your established patterns and conventions

## Future Enhancements

Potential future enhancements could include:

1. **Real-time Updates** - Live updates when transactions are posted
2. **Export Functionality** - Export reports to Excel, PDF, etc.
3. **Advanced Filtering** - More complex query builders
4. **Caching** - Cache frequently accessed journal entry data
5. **Batch Operations** - Bulk operations on journal entries

## Testing

The implementation includes comprehensive examples showing how to use all functionality. You can use the `JournalEntryUsageExample` class as a reference for implementing your own use cases.

## Support

This functionality is built as a pure business layer and can be consumed by any presentation layer (Web API, desktop app, etc.). The services are designed to be stateless and thread-safe for use in multi-user environments.
