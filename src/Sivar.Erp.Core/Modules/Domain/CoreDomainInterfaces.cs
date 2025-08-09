using System.ComponentModel;
using Sivar.Erp.Core.Core;

namespace Sivar.Erp.Core.Modules.Domain
{
    /// <summary>
    /// Represents a transaction in the accounting system
    /// </summary>
    [Description("Represents a transaction in the accounting system")]
    public interface ITransaction : IEntity
    {
        /// <summary>
        /// Transaction number for identification
        /// </summary>
        [Description("Transaction number for identification")]
        string TransactionNumber { get; set; }

        /// <summary>
        /// Date when the transaction occurred
        /// </summary>
        [Description("Date when the transaction occurred")]
        DateOnly TransactionDate { get; set; }

        /// <summary>
        /// Description of the transaction
        /// </summary>
        [Description("Description of the transaction")]
        string Description { get; set; }

        /// <summary>
        /// Source document number that generated this transaction
        /// </summary>
        [Description("Source document number that generated this transaction")]
        string? DocumentNumber { get; set; }

        /// <summary>
        /// Collection of ledger entries for this transaction
        /// </summary>
        [Description("Collection of ledger entries for this transaction")]
        ICollection<ILedgerEntry> LedgerEntries { get; set; }

        /// <summary>
        /// Indicates if the transaction has been posted
        /// </summary>
        [Description("Indicates if the transaction has been posted")]
        bool IsPosted { get; set; }

        /// <summary>
        /// Total amount of the transaction (sum of all debit entries)
        /// </summary>
        [Description("Total amount of the transaction")]
        decimal TotalAmount { get; }

        /// <summary>
        /// Validates that the transaction is balanced (debits = credits)
        /// </summary>
        [Description("Validates that the transaction is balanced")]
        bool IsBalanced { get; }
    }

    /// <summary>
    /// Represents a ledger entry within a transaction
    /// </summary>
    [Description("Represents a ledger entry within a transaction")]
    public interface ILedgerEntry : IEntity
    {
        /// <summary>
        /// Transaction number this entry belongs to
        /// </summary>
        [Description("Transaction number this entry belongs to")]
        string TransactionNumber { get; set; }

        /// <summary>
        /// Account code for this entry
        /// </summary>
        [Description("Account code for this entry")]
        string AccountCode { get; set; }

        /// <summary>
        /// Type of entry (Debit or Credit)
        /// </summary>
        [Description("Type of entry (Debit or Credit)")]
        EntryType EntryType { get; set; }

        /// <summary>
        /// Amount for this entry
        /// </summary>
        [Description("Amount for this entry")]
        decimal Amount { get; set; }

        /// <summary>
        /// Description for this specific entry
        /// </summary>
        [Description("Description for this specific entry")]
        string? Description { get; set; }
    }

    /// <summary>
    /// Represents a document in the system
    /// </summary>
    [Description("Represents a document in the system")]
    public interface IDocument : IEntity
    {
        /// <summary>
        /// Document number for identification
        /// </summary>
        [Description("Document number for identification")]
        string DocumentNumber { get; set; }

        /// <summary>
        /// Date of the document
        /// </summary>
        [Description("Date of the document")]
        DateOnly Date { get; set; }

        /// <summary>
        /// Type of document
        /// </summary>
        [Description("Type of document")]
        IDocumentType DocumentType { get; set; }

        /// <summary>
        /// Business entity associated with this document
        /// </summary>
        [Description("Business entity associated with this document")]
        IBusinessEntity BusinessEntity { get; set; }

        /// <summary>
        /// Collection of document totals for accounting
        /// </summary>
        [Description("Collection of document totals for accounting")]
        ICollection<IDocumentTotal> DocumentTotals { get; set; }

        /// <summary>
        /// Total amount of the document
        /// </summary>
        [Description("Total amount of the document")]
        decimal TotalAmount { get; }

        /// <summary>
        /// Status of the document
        /// </summary>
        [Description("Status of the document")]
        DocumentStatus Status { get; set; }
    }

    /// <summary>
    /// Represents a document type definition
    /// </summary>
    [Description("Represents a document type definition")]
    public interface IDocumentType : IEntity
    {
        /// <summary>
        /// Code identifying the document type
        /// </summary>
        [Description("Code identifying the document type")]
        string Code { get; set; }

        /// <summary>
        /// Name of the document type
        /// </summary>
        [Description("Name of the document type")]
        string Name { get; set; }

        /// <summary>
        /// Description of the document type
        /// </summary>
        [Description("Description of the document type")]
        string? Description { get; set; }

        /// <summary>
        /// Indicates if this document type generates accounting transactions
        /// </summary>
        [Description("Indicates if this document type generates accounting transactions")]
        bool GeneratesTransaction { get; set; }
    }

    /// <summary>
    /// Represents a business entity (customer, supplier, etc.)
    /// </summary>
    [Description("Represents a business entity")]
    public interface IBusinessEntity : IEntity
    {
        /// <summary>
        /// Code identifying the business entity
        /// </summary>
        [Description("Code identifying the business entity")]
        string Code { get; set; }

        /// <summary>
        /// Name of the business entity
        /// </summary>
        [Description("Name of the business entity")]
        string Name { get; set; }

        /// <summary>
        /// Email address
        /// </summary>
        [Description("Email address")]
        string? Email { get; set; }

        /// <summary>
        /// Type of business entity
        /// </summary>
        [Description("Type of business entity")]
        BusinessEntityType EntityType { get; set; }
    }

    /// <summary>
    /// Represents a document total for accounting purposes
    /// </summary>
    [Description("Represents a document total for accounting purposes")]
    public interface IDocumentTotal : IEntity
    {
        /// <summary>
        /// Concept or description of the total
        /// </summary>
        [Description("Concept or description of the total")]
        string Concept { get; set; }

        /// <summary>
        /// Total amount
        /// </summary>
        [Description("Total amount")]
        decimal Total { get; set; }

        /// <summary>
        /// Debit account code for this total
        /// </summary>
        [Description("Debit account code for this total")]
        string? DebitAccountCode { get; set; }

        /// <summary>
        /// Credit account code for this total
        /// </summary>
        [Description("Credit account code for this total")]
        string? CreditAccountCode { get; set; }

        /// <summary>
        /// Indicates if this total should be included in transaction generation
        /// </summary>
        [Description("Indicates if this total should be included in transaction generation")]
        bool IncludeInTransaction { get; set; }
    }

    /// <summary>
    /// Represents a tax definition
    /// </summary>
    [Description("Represents a tax definition")]
    public interface ITax : IEntity
    {
        /// <summary>
        /// Tax code for identification
        /// </summary>
        [Description("Tax code for identification")]
        string Code { get; set; }

        /// <summary>
        /// Name of the tax
        /// </summary>
        [Description("Name of the tax")]
        string Name { get; set; }

        /// <summary>
        /// Tax rate as a percentage
        /// </summary>
        [Description("Tax rate as a percentage")]
        decimal Rate { get; set; }

        /// <summary>
        /// Type of tax
        /// </summary>
        [Description("Type of tax")]
        TaxType TaxType { get; set; }

        /// <summary>
        /// Indicates if the tax is currently active
        /// </summary>
        [Description("Indicates if the tax is currently active")]
        bool IsActive { get; set; }
    }

    /// <summary>
    /// Type of ledger entry
    /// </summary>
    [Description("Type of ledger entry")]
    public enum EntryType
    {
        /// <summary>
        /// Debit entry
        /// </summary>
        [Description("Debit entry")]
        Debit = 1,

        /// <summary>
        /// Credit entry
        /// </summary>
        [Description("Credit entry")]
        Credit = 2
    }

    /// <summary>
    /// Status of a document
    /// </summary>
    [Description("Status of a document")]
    public enum DocumentStatus
    {
        /// <summary>
        /// Document is in draft status
        /// </summary>
        [Description("Document is in draft status")]
        Draft = 1,

        /// <summary>
        /// Document is pending approval
        /// </summary>
        [Description("Document is pending approval")]
        Pending = 2,

        /// <summary>
        /// Document is approved
        /// </summary>
        [Description("Document is approved")]
        Approved = 3,

        /// <summary>
        /// Document has been posted
        /// </summary>
        [Description("Document has been posted")]
        Posted = 4,

        /// <summary>
        /// Document has been cancelled
        /// </summary>
        [Description("Document has been cancelled")]
        Cancelled = 5
    }

    /// <summary>
    /// Type of business entity
    /// </summary>
    [Description("Type of business entity")]
    public enum BusinessEntityType
    {
        /// <summary>
        /// Customer entity
        /// </summary>
        [Description("Customer entity")]
        Customer = 1,

        /// <summary>
        /// Supplier entity
        /// </summary>
        [Description("Supplier entity")]
        Supplier = 2,

        /// <summary>
        /// Employee entity
        /// </summary>
        [Description("Employee entity")]
        Employee = 3,

        /// <summary>
        /// Other type of entity
        /// </summary>
        [Description("Other type of entity")]
        Other = 4
    }

    /// <summary>
    /// Type of tax
    /// </summary>
    [Description("Type of tax")]
    public enum TaxType
    {
        /// <summary>
        /// Value Added Tax
        /// </summary>
        [Description("Value Added Tax")]
        VAT = 1,

        /// <summary>
        /// Sales tax
        /// </summary>
        [Description("Sales tax")]
        Sales = 2,

        /// <summary>
        /// Withholding tax
        /// </summary>
        [Description("Withholding tax")]
        Withholding = 3,

        /// <summary>
        /// Income tax
        /// </summary>
        [Description("Income tax")]
        Income = 4
    }

    /// <summary>
    /// Document operation type for tax calculations
    /// </summary>
    [Description("Document operation type for tax calculations")]
    public enum DocumentOperation
    {
        /// <summary>
        /// Sale operation
        /// </summary>
        [Description("Sale operation")]
        Sale = 1,

        /// <summary>
        /// Purchase operation
        /// </summary>
        [Description("Purchase operation")]
        Purchase = 2,

        /// <summary>
        /// Return operation
        /// </summary>
        [Description("Return operation")]
        Return = 3
    }
}