namespace Sivar.Erp.Core.Domain.Enums;

/// <summary>
/// Account types based on accounting equation
/// </summary>
public enum AccountType
{
    Asset = 1,
    Liability = 2,
    Equity = 3,
    Revenue = 4,
    Expense = 5
}

/// <summary>
/// Status of a journal entry
/// </summary>
public enum JournalEntryStatus
{
    /// <summary>
    /// Entry is in draft state and can be edited
    /// </summary>
    Draft = 0,
    
    /// <summary>
    /// Entry is pending approval
    /// </summary>
    PendingApproval = 1,
    
    /// <summary>
    /// Entry has been posted and affects account balances
    /// </summary>
    Posted = 2,
    
    /// <summary>
    /// Entry has been reversed
    /// </summary>
    Reversed = 3,
    
    /// <summary>
    /// Entry has been cancelled
    /// </summary>
    Cancelled = 4
}

/// <summary>
/// Approval status for journal entries
/// </summary>
public enum ApprovalStatus
{
    /// <summary>
    /// No approval required or approval is pending
    /// </summary>
    Pending = 0,
    
    /// <summary>
    /// Entry has been approved
    /// </summary>
    Approved = 1,
    
    /// <summary>
    /// Entry has been rejected
    /// </summary>
    Rejected = 2
}

/// <summary>
/// Status of an accounting period
/// </summary>
public enum PeriodStatus
{
    /// <summary>
    /// Period is open for transactions
    /// </summary>
    Open = 0,
    
    /// <summary>
    /// Period is temporarily closed
    /// </summary>
    Closed = 1,
    
    /// <summary>
    /// Period is permanently locked
    /// </summary>
    Locked = 2
}

/// <summary>
/// Status of a fiscal year
/// </summary>
public enum FiscalYearStatus
{
    /// <summary>
    /// Fiscal year is currently active
    /// </summary>
    Active = 0,
    
    /// <summary>
    /// Fiscal year is closed
    /// </summary>
    Closed = 1,
    
    /// <summary>
    /// Fiscal year is archived
    /// </summary>
    Archived = 2
}
