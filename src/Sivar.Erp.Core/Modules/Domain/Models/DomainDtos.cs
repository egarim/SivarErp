using System.ComponentModel;
using Sivar.Erp.Core.Core;
using Sivar.Erp.Core.Modules.Domain;
using Sivar.Erp.Core.Modules.Accounting;

namespace Sivar.Erp.Core.Modules.Domain.Models
{
    /// <summary>
    /// Data transfer object implementation of ITransaction
    /// </summary>
    [Description("Data transfer object implementation of ITransaction")]
    public class TransactionDto : ITransaction
    {
        /// <summary>
        /// Unique identifier for the entity
        /// </summary>
        [Description("Unique identifier for the entity")]
        public Guid Id { get; set; }

        /// <summary>
        /// Date when the entity was created
        /// </summary>
        [Description("Date when the entity was created")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Date when the entity was last updated
        /// </summary>
        [Description("Date when the entity was last updated")]
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Transaction number for identification
        /// </summary>
        [Description("Transaction number for identification")]
        public string TransactionNumber { get; set; } = string.Empty;

        /// <summary>
        /// Date when the transaction occurred
        /// </summary>
        [Description("Date when the transaction occurred")]
        public DateOnly TransactionDate { get; set; }

        /// <summary>
        /// Description of the transaction
        /// </summary>
        [Description("Description of the transaction")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Source document number that generated this transaction
        /// </summary>
        [Description("Source document number that generated this transaction")]
        public string? DocumentNumber { get; set; }

        /// <summary>
        /// Collection of ledger entries for this transaction
        /// </summary>
        [Description("Collection of ledger entries for this transaction")]
        public ICollection<ILedgerEntry> LedgerEntries { get; set; } = new List<ILedgerEntry>();

        /// <summary>
        /// Indicates if the transaction has been posted
        /// </summary>
        [Description("Indicates if the transaction has been posted")]
        public bool IsPosted { get; set; }

        /// <summary>
        /// Total amount of the transaction (sum of all debit entries)
        /// </summary>
        [Description("Total amount of the transaction")]
        public decimal TotalAmount => LedgerEntries.Where(le => le.EntryType == EntryType.Debit).Sum(le => le.Amount);

        /// <summary>
        /// Validates that the transaction is balanced (debits = credits)
        /// </summary>
        [Description("Validates that the transaction is balanced")]
        public bool IsBalanced
        {
            get
            {
                var totalDebits = LedgerEntries.Where(le => le.EntryType == EntryType.Debit).Sum(le => le.Amount);
                var totalCredits = LedgerEntries.Where(le => le.EntryType == EntryType.Credit).Sum(le => le.Amount);
                return Math.Abs(totalDebits - totalCredits) < 0.01m; // Allow for small rounding differences
            }
        }
    }

    /// <summary>
    /// Data transfer object implementation of ILedgerEntry
    /// </summary>
    [Description("Data transfer object implementation of ILedgerEntry")]
    public class LedgerEntryDto : ILedgerEntry
    {
        /// <summary>
        /// Unique identifier for the entity
        /// </summary>
        [Description("Unique identifier for the entity")]
        public Guid Id { get; set; }

        /// <summary>
        /// Date when the entity was created
        /// </summary>
        [Description("Date when the entity was created")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Date when the entity was last updated
        /// </summary>
        [Description("Date when the entity was last updated")]
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Transaction number this entry belongs to
        /// </summary>
        [Description("Transaction number this entry belongs to")]
        public string TransactionNumber { get; set; } = string.Empty;

        /// <summary>
        /// Account code for this entry
        /// </summary>
        [Description("Account code for this entry")]
        public string AccountCode { get; set; } = string.Empty;

        /// <summary>
        /// Type of entry (Debit or Credit)
        /// </summary>
        [Description("Type of entry (Debit or Credit)")]
        public EntryType EntryType { get; set; }

        /// <summary>
        /// Amount for this entry
        /// </summary>
        [Description("Amount for this entry")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Description for this specific entry
        /// </summary>
        [Description("Description for this specific entry")]
        public string? Description { get; set; }
    }

    /// <summary>
    /// Data transfer object implementation of IDocument
    /// </summary>
    [Description("Data transfer object implementation of IDocument")]
    public class DocumentDto : IDocument
    {
        /// <summary>
        /// Unique identifier for the entity
        /// </summary>
        [Description("Unique identifier for the entity")]
        public Guid Id { get; set; }

        /// <summary>
        /// Date when the entity was created
        /// </summary>
        [Description("Date when the entity was created")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Date when the entity was last updated
        /// </summary>
        [Description("Date when the entity was last updated")]
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Document number for identification
        /// </summary>
        [Description("Document number for identification")]
        public string DocumentNumber { get; set; } = string.Empty;

        /// <summary>
        /// Date of the document
        /// </summary>
        [Description("Date of the document")]
        public DateOnly Date { get; set; }

        /// <summary>
        /// Type of document
        /// </summary>
        [Description("Type of document")]
        public IDocumentType DocumentType { get; set; } = new DocumentTypeDto();

        /// <summary>
        /// Business entity associated with this document
        /// </summary>
        [Description("Business entity associated with this document")]
        public IBusinessEntity BusinessEntity { get; set; } = new BusinessEntityDto();

        /// <summary>
        /// Collection of document totals for accounting
        /// </summary>
        [Description("Collection of document totals for accounting")]
        public ICollection<IDocumentTotal> DocumentTotals { get; set; } = new List<IDocumentTotal>();

        /// <summary>
        /// Total amount of the document
        /// </summary>
        [Description("Total amount of the document")]
        public decimal TotalAmount => DocumentTotals.Sum(dt => dt.Total);

        /// <summary>
        /// Status of the document
        /// </summary>
        [Description("Status of the document")]
        public DocumentStatus Status { get; set; }
    }

    /// <summary>
    /// Data transfer object implementation of IDocumentType
    /// </summary>
    [Description("Data transfer object implementation of IDocumentType")]
    public class DocumentTypeDto : IDocumentType
    {
        /// <summary>
        /// Unique identifier for the entity
        /// </summary>
        [Description("Unique identifier for the entity")]
        public Guid Id { get; set; }

        /// <summary>
        /// Date when the entity was created
        /// </summary>
        [Description("Date when the entity was created")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Date when the entity was last updated
        /// </summary>
        [Description("Date when the entity was last updated")]
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Code identifying the document type
        /// </summary>
        [Description("Code identifying the document type")]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Name of the document type
        /// </summary>
        [Description("Name of the document type")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Description of the document type
        /// </summary>
        [Description("Description of the document type")]
        public string? Description { get; set; }

        /// <summary>
        /// Indicates if this document type generates accounting transactions
        /// </summary>
        [Description("Indicates if this document type generates accounting transactions")]
        public bool GeneratesTransaction { get; set; }
    }

    /// <summary>
    /// Data transfer object implementation of IBusinessEntity
    /// </summary>
    [Description("Data transfer object implementation of IBusinessEntity")]
    public class BusinessEntityDto : IBusinessEntity
    {
        /// <summary>
        /// Unique identifier for the entity
        /// </summary>
        [Description("Unique identifier for the entity")]
        public Guid Id { get; set; }

        /// <summary>
        /// Date when the entity was created
        /// </summary>
        [Description("Date when the entity was created")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Date when the entity was last updated
        /// </summary>
        [Description("Date when the entity was last updated")]
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Code identifying the business entity
        /// </summary>
        [Description("Code identifying the business entity")]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Name of the business entity
        /// </summary>
        [Description("Name of the business entity")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Email address
        /// </summary>
        [Description("Email address")]
        public string? Email { get; set; }

        /// <summary>
        /// Type of business entity
        /// </summary>
        [Description("Type of business entity")]
        public BusinessEntityType EntityType { get; set; }
    }

    /// <summary>
    /// Data transfer object implementation of IDocumentTotal
    /// </summary>
    [Description("Data transfer object implementation of IDocumentTotal")]
    public class DocumentTotalDto : IDocumentTotal
    {
        /// <summary>
        /// Unique identifier for the entity
        /// </summary>
        [Description("Unique identifier for the entity")]
        public Guid Id { get; set; }

        /// <summary>
        /// Date when the entity was created
        /// </summary>
        [Description("Date when the entity was created")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Date when the entity was last updated
        /// </summary>
        [Description("Date when the entity was last updated")]
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Concept or description of the total
        /// </summary>
        [Description("Concept or description of the total")]
        public string Concept { get; set; } = string.Empty;

        /// <summary>
        /// Total amount
        /// </summary>
        [Description("Total amount")]
        public decimal Total { get; set; }

        /// <summary>
        /// Debit account code for this total
        /// </summary>
        [Description("Debit account code for this total")]
        public string? DebitAccountCode { get; set; }

        /// <summary>
        /// Credit account code for this total
        /// </summary>
        [Description("Credit account code for this total")]
        public string? CreditAccountCode { get; set; }

        /// <summary>
        /// Indicates if this total should be included in transaction generation
        /// </summary>
        [Description("Indicates if this total should be included in transaction generation")]
        public bool IncludeInTransaction { get; set; }
    }

    /// <summary>
    /// Data transfer object implementation of ITax
    /// </summary>
    [Description("Data transfer object implementation of ITax")]
    public class TaxDto : ITax
    {
        /// <summary>
        /// Unique identifier for the entity
        /// </summary>
        [Description("Unique identifier for the entity")]
        public Guid Id { get; set; }

        /// <summary>
        /// Date when the entity was created
        /// </summary>
        [Description("Date when the entity was created")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Date when the entity was last updated
        /// </summary>
        [Description("Date when the entity was last updated")]
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Tax code for identification
        /// </summary>
        [Description("Tax code for identification")]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Name of the tax
        /// </summary>
        [Description("Name of the tax")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Tax rate as a percentage
        /// </summary>
        [Description("Tax rate as a percentage")]
        public decimal Rate { get; set; }

        /// <summary>
        /// Type of tax
        /// </summary>
        [Description("Type of tax")]
        public TaxType TaxType { get; set; }

        /// <summary>
        /// Indicates if the tax is currently active
        /// </summary>
        [Description("Indicates if the tax is currently active")]
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Data transfer object implementation of IItem
    /// </summary>
    [Description("Data transfer object implementation of IItem")]
    public class ItemDto : IEntity
    {
        /// <summary>
        /// Unique identifier for the entity
        /// </summary>
        [Description("Unique identifier for the entity")]
        public Guid Id { get; set; }

        /// <summary>
        /// Date when the entity was created
        /// </summary>
        [Description("Date when the entity was created")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Date when the entity was last updated
        /// </summary>
        [Description("Date when the entity was last updated")]
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Item code for identification
        /// </summary>
        [Description("Item code for identification")]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Name of the item
        /// </summary>
        [Description("Name of the item")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Description of the item
        /// </summary>
        [Description("Description of the item")]
        public string? Description { get; set; }

        /// <summary>
        /// Unit price of the item
        /// </summary>
        [Description("Unit price of the item")]
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Unit of measure for the item
        /// </summary>
        [Description("Unit of measure for the item")]
        public string UnitOfMeasure { get; set; } = "UNIT";

        /// <summary>
        /// Indicates if the item is currently active
        /// </summary>
        [Description("Indicates if the item is currently active")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Item category or type
        /// </summary>
        [Description("Item category or type")]
        public string? Category { get; set; }
    }

    /// <summary>
    /// Data transfer object implementation for Account
    /// </summary>
    [Description("Data transfer object implementation for Account")]
    public class AccountDto : IEntity, IAccount
    {
        /// <summary>
        /// Unique identifier for the entity
        /// </summary>
        [Description("Unique identifier for the entity")]
        public Guid Id { get; set; }

        /// <summary>
        /// Date when the entity was created
        /// </summary>
        [Description("Date when the entity was created")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Date when the entity was last updated
        /// </summary>
        [Description("Date when the entity was last updated")]
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Official account code
        /// </summary>
        [Description("Official account code")]
        public string OfficialCode { get; set; } = string.Empty;

        /// <summary>
        /// Account name
        /// </summary>
        [Description("Account name")]
        public string AccountName { get; set; } = string.Empty;

        /// <summary>
        /// Account description
        /// </summary>
        [Description("Account description")]
        public string? Description { get; set; }

        /// <summary>
        /// Type of account
        /// </summary>
        [Description("Type of account")]
        public Sivar.Erp.Core.Modules.Accounting.AccountType AccountType { get; set; }

        /// <summary>
        /// Parent account code for hierarchical structure
        /// </summary>
        [Description("Parent account code for hierarchical structure")]
        public string? ParentAccountCode { get; set; }

        /// <summary>
        /// Indicates if the account is currently active
        /// </summary>
        [Description("Indicates if the account is currently active")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Current balance of the account
        /// </summary>
        [Description("Current balance of the account")]
        public decimal Balance { get; set; }
    }

    /// <summary>
    /// Types of accounts in the chart of accounts
    /// </summary>
    [Description("Types of accounts in the chart of accounts")]
    public enum AccountType
    {
        /// <summary>
        /// Asset account
        /// </summary>
        [Description("Asset account")]
        Asset = 1,

        /// <summary>
        /// Liability account
        /// </summary>
        [Description("Liability account")]
        Liability = 2,

        /// <summary>
        /// Equity account
        /// </summary>
        [Description("Equity account")]
        Equity = 3,

        /// <summary>
        /// Revenue account
        /// </summary>
        [Description("Revenue account")]
        Revenue = 4,

        /// <summary>
        /// Expense account
        /// </summary>
        [Description("Expense account")]
        Expense = 5
    }

    /// <summary>
    /// Represents account balance information for trial balance and reports
    /// </summary>
    [Description("Represents account balance information")]
    public class AccountBalance
    {
        /// <summary>
        /// Account code
        /// </summary>
        [Description("Account code")]
        public string AccountCode { get; set; } = string.Empty;

        /// <summary>
        /// Account name
        /// </summary>
        [Description("Account name")]
        public string AccountName { get; set; } = string.Empty;

        /// <summary>
        /// Total debit balance
        /// </summary>
        [Description("Total debit balance")]
        public decimal DebitBalance { get; set; }

        /// <summary>
        /// Total credit balance
        /// </summary>
        [Description("Total credit balance")]
        public decimal CreditBalance { get; set; }

        /// <summary>
        /// Net balance (debits minus credits)
        /// </summary>
        [Description("Net balance")]
        public decimal NetBalance => DebitBalance - CreditBalance;

        /// <summary>
        /// Date the balance was calculated as of
        /// </summary>
        [Description("Date the balance was calculated as of")]
        public DateOnly AsOfDate { get; set; }
    }
}