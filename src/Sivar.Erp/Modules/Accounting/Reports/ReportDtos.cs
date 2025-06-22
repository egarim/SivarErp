using Sivar.Erp.Services.Accounting.Transactions;

namespace Sivar.Erp.Modules.Accounting.Reports;

/// <summary>
/// Report data for journal entries
/// </summary>
public class JournalEntryReportDto
{
    public string ReportTitle { get; set; } = "Journal Entry Report";
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public string? AccountCodeFilter { get; set; }
    public string? TransactionNumberFilter { get; set; }
    public IEnumerable<ILedgerEntry> Entries { get; set; } = new List<ILedgerEntry>();
    public decimal TotalDebits { get; set; }
    public decimal TotalCredits { get; set; }
    public int TotalEntries { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public bool IsBalanced => Math.Abs(TotalDebits - TotalCredits) < 0.01m;
}

/// <summary>
/// Audit trail data for a specific transaction
/// </summary>
public class TransactionAuditTrailDto
{
    public string TransactionNumber { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public DateOnly TransactionDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsPosted { get; set; }
    public IEnumerable<ILedgerEntry> JournalEntries { get; set; } = new List<ILedgerEntry>();
    public decimal TotalDebits { get; set; }
    public decimal TotalCredits { get; set; }
    public bool IsBalanced => Math.Abs(TotalDebits - TotalCredits) < 0.01m;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public IEnumerable<string> AffectedAccounts { get; set; } = new List<string>();
}

/// <summary>
/// Account activity report data
/// </summary>
public class AccountActivityReportDto
{
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public DateOnly FromDate { get; set; }
    public DateOnly ToDate { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal ClosingBalance { get; set; }
    public IEnumerable<ILedgerEntry> Entries { get; set; } = new List<ILedgerEntry>();
    public decimal TotalDebits { get; set; }
    public decimal TotalCredits { get; set; }
    public int TotalTransactions { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Trial balance report data from journal entries
/// </summary>
public class TrialBalanceReportDto
{
    public DateOnly AsOfDate { get; set; }
    public bool OnlyPostedTransactions { get; set; }
    public IEnumerable<TrialBalanceAccountDto> Accounts { get; set; } = new List<TrialBalanceAccountDto>();
    public decimal TotalDebits { get; set; }
    public decimal TotalCredits { get; set; }
    public bool IsBalanced => Math.Abs(TotalDebits - TotalCredits) < 0.01m;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Individual account data for trial balance
/// </summary>
public class TrialBalanceAccountDto
{
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal DebitBalance { get; set; }
    public decimal CreditBalance { get; set; }
    public decimal NetBalance => DebitBalance - CreditBalance;
}
