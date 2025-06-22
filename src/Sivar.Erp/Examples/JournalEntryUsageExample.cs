using Microsoft.Extensions.DependencyInjection;
using Sivar.Erp.Modules.Accounting;
using Sivar.Erp.Modules.Accounting.JournalEntries;
using Sivar.Erp.Modules.Accounting.Reports;
using Sivar.Erp.Services;
using Sivar.Erp.Services.Accounting.Transactions;

namespace Sivar.Erp.Examples;

/// <summary>
/// Example demonstrating how to use the journal entry functionality
/// </summary>
public class JournalEntryUsageExample
{
    private readonly IAccountingModule _accountingModule;
    private readonly IJournalEntryService _journalEntryService;
    private readonly IJournalEntryReportService _reportService;

    public JournalEntryUsageExample(
        IAccountingModule accountingModule,
        IJournalEntryService journalEntryService,
        IJournalEntryReportService reportService)
    {
        _accountingModule = accountingModule;
        _journalEntryService = journalEntryService;
        _reportService = reportService;
    }

    /// <summary>
    /// Example: View journal entries for a specific transaction
    /// </summary>
    public async Task ViewTransactionJournalEntriesExample(string transactionNumber)
    {
        Console.WriteLine($"Journal Entries for Transaction: {transactionNumber}");
        Console.WriteLine(new string('-', 50));

        var journalEntries = await _accountingModule.GetTransactionJournalEntriesAsync(transactionNumber);

        foreach (var entry in journalEntries)
        {
            Console.WriteLine($"Entry: {entry.LedgerEntryNumber}");
            Console.WriteLine($"Account: {entry.OfficialCode} - {entry.AccountName}");
            Console.WriteLine($"Type: {entry.EntryType}");
            Console.WriteLine($"Amount: {entry.Amount:C}");
            Console.WriteLine();
        }

        // Check if transaction is balanced
        var isBalanced = await _accountingModule.ValidateTransactionBalanceAsync(transactionNumber);
        Console.WriteLine($"Transaction is balanced: {isBalanced}");
    }

    /// <summary>
    /// Example: Generate journal entries report for a date range
    /// </summary>
    public async Task GenerateJournalEntriesReportExample(DateOnly fromDate, DateOnly toDate)
    {
        var options = new JournalEntryQueryOptions
        {
            FromDate = fromDate,
            ToDate = toDate,
            OnlyPosted = true,
            Take = 100 // Limit results
        };

        var report = await _accountingModule.GenerateJournalReportAsync(options);

        Console.WriteLine(report.ReportTitle);
        Console.WriteLine($"Period: {report.FromDate} to {report.ToDate}");
        Console.WriteLine($"Total Entries: {report.TotalEntries}");
        Console.WriteLine($"Total Debits: {report.TotalDebits:C}");
        Console.WriteLine($"Total Credits: {report.TotalCredits:C}");
        Console.WriteLine($"Is Balanced: {report.IsBalanced}");
        Console.WriteLine();

        Console.WriteLine("Journal Entries:");
        Console.WriteLine(new string('-', 80));
        foreach (var entry in report.Entries.Take(10)) // Show first 10 entries
        {
            Console.WriteLine($"{entry.LedgerEntryNumber} | {entry.TransactionNumber} | {entry.OfficialCode} | {entry.EntryType} | {entry.Amount:C}");
        }
    }

    /// <summary>
    /// Example: Generate transaction audit trail
    /// </summary>
    public async Task GenerateTransactionAuditTrailExample(string transactionNumber)
    {
        var auditTrail = await _accountingModule.GenerateTransactionAuditTrailAsync(transactionNumber);

        Console.WriteLine("Transaction Audit Trail");
        Console.WriteLine(new string('=', 50));
        Console.WriteLine($"Transaction: {auditTrail.TransactionNumber}");
        Console.WriteLine($"Document: {auditTrail.DocumentNumber}");
        Console.WriteLine($"Date: {auditTrail.TransactionDate}");
        Console.WriteLine($"Description: {auditTrail.Description}");
        Console.WriteLine($"Posted: {auditTrail.IsPosted}");
        Console.WriteLine($"Is Balanced: {auditTrail.IsBalanced}");
        Console.WriteLine();

        Console.WriteLine("Journal Entries:");
        Console.WriteLine(new string('-', 80));
        foreach (var entry in auditTrail.JournalEntries)
        {
            Console.WriteLine($"{entry.LedgerEntryNumber} | {entry.OfficialCode} - {entry.AccountName} | {entry.EntryType} | {entry.Amount:C}");
        }

        Console.WriteLine();
        Console.WriteLine($"Total Debits: {auditTrail.TotalDebits:C}");
        Console.WriteLine($"Total Credits: {auditTrail.TotalCredits:C}");
        Console.WriteLine($"Affected Accounts: {string.Join(", ", auditTrail.AffectedAccounts)}");
    }

    /// <summary>
    /// Example: Search journal entries by account
    /// </summary>
    public async Task SearchJournalEntriesByAccountExample(string accountCode, DateOnly fromDate, DateOnly toDate)
    {
        var options = new JournalEntryQueryOptions
        {
            AccountCode = accountCode,
            FromDate = fromDate,
            ToDate = toDate,
            OnlyPosted = true,
            SortBy = "TransactionNumber",
            Take = 50
        };

        var journalEntries = await _accountingModule.GetJournalEntriesAsync(options);

        Console.WriteLine($"Journal Entries for Account: {accountCode}");
        Console.WriteLine($"Period: {fromDate} to {toDate}");
        Console.WriteLine(new string('-', 80));

        decimal totalDebits = 0;
        decimal totalCredits = 0;

        foreach (var entry in journalEntries)
        {
            Console.WriteLine($"{entry.TransactionNumber} | {entry.LedgerEntryNumber} | {entry.EntryType} | {entry.Amount:C}");

            if (entry.EntryType == EntryType.Debit)
                totalDebits += entry.Amount;
            else
                totalCredits += entry.Amount;
        }

        Console.WriteLine(new string('-', 80));
        Console.WriteLine($"Total Debits: {totalDebits:C}");
        Console.WriteLine($"Total Credits: {totalCredits:C}");
        Console.WriteLine($"Net Activity: {(totalDebits - totalCredits):C}");
    }

    /// <summary>
    /// Example: Generate account activity report
    /// </summary>
    public async Task GenerateAccountActivityReportExample(string accountCode, DateOnly fromDate, DateOnly toDate)
    {
        var report = await _reportService.GenerateAccountActivityReportAsync(accountCode, fromDate, toDate);

        Console.WriteLine($"Account Activity Report");
        Console.WriteLine($"Account: {report.AccountCode} - {report.AccountName}");
        Console.WriteLine($"Period: {report.FromDate} to {report.ToDate}");
        Console.WriteLine(new string('=', 80));
        Console.WriteLine($"Opening Balance: {report.OpeningBalance:C}");
        Console.WriteLine($"Total Debits: {report.TotalDebits:C}");
        Console.WriteLine($"Total Credits: {report.TotalCredits:C}");
        Console.WriteLine($"Closing Balance: {report.ClosingBalance:C}");
        Console.WriteLine($"Total Transactions: {report.TotalTransactions}");
        Console.WriteLine();

        Console.WriteLine("Detailed Entries:");
        Console.WriteLine(new string('-', 80));
        foreach (var entry in report.Entries.Take(20)) // Show first 20 entries
        {
            Console.WriteLine($"{entry.TransactionNumber} | {entry.LedgerEntryNumber} | {entry.EntryType} | {entry.Amount:C}");
        }
    }

    /// <summary>
    /// Example: Generate trial balance from journal entries
    /// </summary>
    public async Task GenerateTrialBalanceExample(DateOnly asOfDate)
    {
        var trialBalance = await _reportService.GenerateTrialBalanceFromJournalEntriesAsync(asOfDate, onlyPosted: true);

        Console.WriteLine($"Trial Balance as of {asOfDate}");
        Console.WriteLine(new string('=', 80));
        Console.WriteLine($"Generated: {trialBalance.GeneratedAt}");
        Console.WriteLine($"Only Posted Transactions: {trialBalance.OnlyPostedTransactions}");
        Console.WriteLine($"Is Balanced: {trialBalance.IsBalanced}");
        Console.WriteLine();

        Console.WriteLine("Account                     | Debit Balance | Credit Balance | Net Balance");
        Console.WriteLine(new string('-', 80));

        foreach (var account in trialBalance.Accounts)
        {
            Console.WriteLine($"{account.AccountCode,-15} {account.AccountName,-12} | {account.DebitBalance,12:C} | {account.CreditBalance,13:C} | {account.NetBalance,10:C}");
        }

        Console.WriteLine(new string('-', 80));
        Console.WriteLine($"{"TOTALS",-28} | {trialBalance.TotalDebits,12:C} | {trialBalance.TotalCredits,13:C} | {(trialBalance.TotalDebits - trialBalance.TotalCredits),10:C}");
    }
}

/// <summary>
/// Extension methods for dependency injection setup
/// </summary>
public static class JournalEntryServiceExtensions
{
    /// <summary>
    /// Registers journal entry services in the DI container
    /// </summary>
    public static IServiceCollection AddJournalEntryServices(this IServiceCollection services)
    {
        services.AddScoped<IJournalEntryService, JournalEntryService>();
        services.AddScoped<IJournalEntryReportService, JournalEntryReportService>();

        return services;
    }
}
