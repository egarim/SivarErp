using System;
using System.Threading.Tasks;
using Sivar.Erp.Documents;
using Sivar.Erp.Services.Accounting.FiscalPeriods;
using Sivar.Erp.Services.Accounting.Transactions;

namespace Sivar.Erp.Modules.Accounting
{
    /// <summary>
    /// Interface for accounting operations that other modules can use to record financial transactions
    /// and manage fiscal periods
    /// </summary>
    public interface IAccountingModule
    {
        /// <summary>
        /// Posts a transaction after validating fiscal period is open
        /// </summary>
        /// <param name="transaction">Transaction to post</param>
        /// <returns>True if posted successfully</returns>
        Task<bool> PostTransactionAsync(ITransaction transaction);
        
        /// <summary>
        /// Unposts a previously posted transaction if fiscal period is still open
        /// </summary>
        /// <param name="transaction">Transaction to unpost</param>
        /// <returns>True if unposted successfully</returns>
        Task<bool> UnPostTransactionAsync(ITransaction transaction);
        
        /// <summary>
        /// Checks if a date falls within an open fiscal period
        /// </summary>
        /// <param name="date">Date to check</param>
        /// <returns>True if date is in an open fiscal period, false otherwise</returns>
        Task<bool> IsDateInOpenFiscalPeriodAsync(DateOnly date);
        
        /// <summary>
        /// Gets the fiscal period service for advanced operations
        /// </summary>
        /// <returns>The fiscal period service</returns>
        IFiscalPeriodService GetFiscalPeriodService();
    }
}