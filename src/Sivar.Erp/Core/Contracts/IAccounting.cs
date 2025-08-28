using System;
using System.Collections.Generic;
using Sivar.Erp.Core.Enums;
using Sivar.Erp.Modules.Accounting.FiscalPeriods;
using Sivar.Erp.Modules.Accounting.Transactions;

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
        FiscalPeriodStatus Status { get; set; } 
        DateTime CreatedDate { get; set; }
        string CreatedBy { get; set; }
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
