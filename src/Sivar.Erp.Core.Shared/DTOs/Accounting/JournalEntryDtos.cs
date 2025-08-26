using System.ComponentModel.DataAnnotations;
using Sivar.Erp.Core.Domain.Enums;

namespace Sivar.Erp.Core.Shared.DTOs.Accounting;

/// <summary>
/// DTO for creating a new journal entry
/// </summary>
public class CreateJournalEntryDto
{
    /// <summary>
    /// Reference number for the journal entry
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string ReferenceNumber { get; set; } = string.Empty;

    /// <summary>
    /// Description of the journal entry
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Transaction date
    /// </summary>
    [Required]
    public DateTime TransactionDate { get; set; }

    /// <summary>
    /// Journal entry lines (debits and credits)
    /// </summary>
    [Required]
    [MinLength(2, ErrorMessage = "A journal entry must have at least 2 lines")]
    public List<CreateJournalEntryLineDto> Lines { get; set; } = new();
}

/// <summary>
/// DTO for creating a journal entry line
/// </summary>
public class CreateJournalEntryLineDto
{
    /// <summary>
    /// Account ID for this line
    /// </summary>
    [Required]
    public Guid AccountId { get; set; }

    /// <summary>
    /// Description for this line
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Debit amount (0 if credit)
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Debit amount must be positive")]
    public decimal DebitAmount { get; set; }

    /// <summary>
    /// Credit amount (0 if debit)
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Credit amount must be positive")]
    public decimal CreditAmount { get; set; }
}

/// <summary>
/// DTO for journal entry response
/// </summary>
public class JournalEntryDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public JournalEntryStatus Status { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? PostedDate { get; set; }
    public string? PostedBy { get; set; }
    public List<JournalEntryLineDto> Lines { get; set; } = new();
}

/// <summary>
/// DTO for journal entry line response
/// </summary>
public class JournalEntryLineDto
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
}

/// <summary>
/// DTO for posting/unposting journal entries
/// </summary>
public class PostJournalEntryDto
{
    /// <summary>
    /// Journal entry IDs to post
    /// </summary>
    [Required]
    [MinLength(1, ErrorMessage = "At least one journal entry ID is required")]
    public List<Guid> JournalEntryIds { get; set; } = new();

    /// <summary>
    /// Optional posting comment
    /// </summary>
    [MaxLength(500)]
    public string? Comment { get; set; }
}

/// <summary>
/// DTO for trial balance
/// </summary>
public class TrialBalanceDto
{
    public DateTime AsOfDate { get; set; }
    public List<TrialBalanceLineDto> Lines { get; set; } = new();
    public decimal TotalDebits { get; set; }
    public decimal TotalCredits { get; set; }
    public bool IsBalanced => TotalDebits == TotalCredits;
}

/// <summary>
/// DTO for trial balance line
/// </summary>
public class TrialBalanceLineDto
{
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public AccountType AccountType { get; set; }
    public decimal DebitBalance { get; set; }
    public decimal CreditBalance { get; set; }
}
