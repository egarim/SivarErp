using System;
using System.Collections.Generic;
using Sivar.Erp.Core.Enums;

namespace Sivar.Erp.Core.Contracts
{
    /// <summary>
    /// Interface for fiscal periods
    /// </summary>
    public interface IFiscalPeriod
    {
        string Code { get; set; }
        string Name { get; set; }
        DateOnly StartDate { get; set; }
        DateOnly EndDate { get; set; }
        bool IsActive { get; set; }
        FiscalPeriodStatus Status { get; set; } // Add Status property
        DateTime CreatedDate { get; set; }
        string CreatedBy { get; set; }
    }

    /// <summary>
    /// Interface for transactions
    /// </summary>
    public interface ITransaction
    {
        string TransactionNumber { get; set; }
        DateOnly TransactionDate { get; set; }
        string? DocumentNumber { get; set; }
        string? Description { get; set; }
        bool IsPosted { get; set; }
        IList<ILedgerEntry>? LedgerEntries { get; set; }
        DateTime CreatedDate { get; set; }
        string CreatedBy { get; set; }
    }

    /// <summary>
    /// Interface for ledger entries
    /// </summary>
    public interface ILedgerEntry
    {
        string LedgerEntryNumber { get; set; }
        string TransactionNumber { get; set; }
        string OfficialCode { get; set; }
        EntryType EntryType { get; set; }
        decimal Amount { get; set; }
        string? Description { get; set; }
        DateTime CreatedDate { get; set; }
        string CreatedBy { get; set; }
    }

    /// <summary>
    /// Interface for transaction batches
    /// </summary>
    public interface ITransactionBatch
    {
        string BatchNumber { get; set; }
        string BatchName { get; set; }
        IList<ITransaction> Transactions { get; set; }
        DateTime CreatedDate { get; set; }
        string CreatedBy { get; set; }
    }

    /// <summary>
    /// Interface for document accounting profiles
    /// </summary>
    public interface IDocumentAccountingProfile
    {
        string DocumentOperation { get; set; }
        string? SalesAccountCode { get; set; }
        string? AccountsReceivableCode { get; set; }
        string? CostOfGoodsSoldAccountCode { get; set; }
        string? InventoryAccountCode { get; set; }
        decimal? CostRatio { get; set; }
    }

    /// <summary>
    /// Interface for account balance calculator
    /// </summary>
    public interface IAccountBalanceCalculator
    {
        Task<decimal> GetAccountBalanceAsync(string accountCode);
        Task<decimal> GetAccountBalanceAsync(string accountCode, DateOnly asOfDate);
    }

    /// <summary>
    /// Interface for fiscal period service
    /// </summary>
    public interface IFiscalPeriodService
    {
        Task<IFiscalPeriod> CreateFiscalPeriodAsync(IFiscalPeriod fiscalPeriod, string userId);
        Task<IFiscalPeriod?> GetFiscalPeriodByIdAsync(string code);
    }
}