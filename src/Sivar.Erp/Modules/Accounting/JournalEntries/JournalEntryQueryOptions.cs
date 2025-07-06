using Sivar.Erp.Core.Enums;

namespace Sivar.Erp.Modules.Accounting.JournalEntries;

/// <summary>
/// Query options for filtering and sorting journal entries
/// </summary>
public class JournalEntryQueryOptions
{
    /// <summary>
    /// Filter entries from this date onwards
    /// </summary>
    public DateOnly? FromDate { get; set; }

    /// <summary>
    /// Filter entries up to this date
    /// </summary>
    public DateOnly? ToDate { get; set; }

    /// <summary>
    /// Filter entries for a specific account code
    /// </summary>
    public string? AccountCode { get; set; }

    /// <summary>
    /// Filter entries for a specific transaction number
    /// </summary>
    public string? TransactionNumber { get; set; }

    /// <summary>
    /// Filter entries by entry type (Debit or Credit)
    /// </summary>
    public EntryType? EntryType { get; set; }

    /// <summary>
    /// Filter entries by document number
    /// </summary>
    public string? DocumentNumber { get; set; }

    /// <summary>
    /// Filter entries by posted status
    /// </summary>
    public bool? OnlyPosted { get; set; }

    /// <summary>
    /// Number of records to skip for pagination
    /// </summary>
    public int? Skip { get; set; }

    /// <summary>
    /// Number of records to take for pagination
    /// </summary>
    public int? Take { get; set; }

    /// <summary>
    /// Field to sort by (LedgerEntryNumber, TransactionNumber, AccountCode, Amount)
    /// </summary>
    public string? SortBy { get; set; } = "LedgerEntryNumber";

    /// <summary>
    /// Whether to sort in descending order
    /// </summary>
    public bool SortDescending { get; set; } = false;
}
