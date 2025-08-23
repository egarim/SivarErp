using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Sivar.Erp.Core.Core;
using Sivar.Erp.Core.Infrastructure.Logging;
using Sivar.Erp.Core.Modules.Domain;

namespace Sivar.Erp.Core.Modules.Accounting
{
    /// <summary>
    /// Advanced accounting analytics and reporting service
    /// </summary>
    [Description("Advanced accounting analytics and reporting service")]
    public interface IAccountingAnalyticsService
    {
        /// <summary>
        /// Generates financial statements for a specific period
        /// </summary>
        Task<FinancialStatements> GenerateFinancialStatementsAsync(DateOnly fromDate, DateOnly toDate);

        /// <summary>
        /// Generates trial balance report with detailed account information
        /// </summary>
        Task<TrialBalanceReport> GenerateTrialBalanceReportAsync(DateOnly asOfDate);

        /// <summary>
        /// Generates cash flow statement
        /// </summary>
        Task<CashFlowStatement> GenerateCashFlowStatementAsync(DateOnly fromDate, DateOnly toDate);

        /// <summary>
        /// Gets account aging analysis
        /// </summary>
        Task<AccountAgingAnalysis> GetAccountAgingAnalysisAsync(string accountCode, DateOnly asOfDate);

        /// <summary>
        /// Gets transaction volume analytics
        /// </summary>
        Task<TransactionVolumeAnalytics> GetTransactionVolumeAnalyticsAsync(DateOnly fromDate, DateOnly toDate);

        /// <summary>
        /// Performs account reconciliation
        /// </summary>
        Task<AccountReconciliation> PerformAccountReconciliationAsync(string accountCode, DateOnly fromDate, DateOnly toDate);
    }

    /// <summary>
    /// Implementation of accounting analytics service
    /// </summary>
    [Description("Accounting analytics service implementation")]
    public class AccountingAnalyticsService : IAccountingAnalyticsService
    {
        private readonly IRepository _repository;
        private readonly IAccountingService _accountingService;
        private readonly IErpLoggingService _erpLogger;
        private readonly ILogger<AccountingAnalyticsService> _logger;

        public AccountingAnalyticsService(
            IRepository repository,
            IAccountingService accountingService,
            IErpLoggingService erpLogger,
            ILogger<AccountingAnalyticsService> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _accountingService = accountingService ?? throw new ArgumentNullException(nameof(accountingService));
            _erpLogger = erpLogger ?? throw new ArgumentNullException(nameof(erpLogger));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Generates comprehensive financial statements
        /// </summary>
        /// <param name="fromDate">Start date of the period</param>
        /// <param name="toDate">End date of the period</param>
        /// <returns>Complete financial statements</returns>
        [Description("Generates financial statements")]
        public async Task<FinancialStatements> GenerateFinancialStatementsAsync(DateOnly fromDate, DateOnly toDate)
        {
            using var timer = _erpLogger.StartOperationTimer("GenerateFinancialStatements");
            
            _logger.LogInformation("Generating financial statements for period {FromDate} to {ToDate}", fromDate, toDate);

            var accounts = _repository.GetObjects<IAccount>().ToList();
            var statements = new FinancialStatements
            {
                PeriodStart = fromDate,
                PeriodEnd = toDate,
                GeneratedAt = DateTime.UtcNow
            };

            // Balance Sheet
            statements.BalanceSheet = await GenerateBalanceSheetAsync(accounts, toDate);
            
            // Income Statement
            statements.IncomeStatement = await GenerateIncomeStatementAsync(accounts, fromDate, toDate);
            
            // Cash Flow Statement
            statements.CashFlowStatement = await GenerateCashFlowStatementAsync(fromDate, toDate);

            _erpLogger.LogPerformanceMetrics("FinancialStatementsGeneration", DateTime.UtcNow - statements.GeneratedAt.Value, accounts.Count);
            
            return statements;
        }

        /// <summary>
        /// Generates detailed trial balance report
        /// </summary>
        /// <param name="asOfDate">Date for the trial balance</param>
        /// <returns>Trial balance report with account details</returns>
        [Description("Generates trial balance report")]
        public async Task<TrialBalanceReport> GenerateTrialBalanceReportAsync(DateOnly asOfDate)
        {
            using var timer = _erpLogger.StartOperationTimer("GenerateTrialBalanceReport");

            _logger.LogInformation("Generating trial balance report as of {AsOfDate}", asOfDate);

            var accounts = _repository.GetObjects<IAccount>().OrderBy(a => a.OfficialCode).ToList();
            var report = new TrialBalanceReport
            {
                AsOfDate = asOfDate,
                GeneratedAt = DateTime.UtcNow,
                AccountBalances = new List<DetailedAccountBalance>()
            };

            decimal totalDebits = 0;
            decimal totalCredits = 0;

            foreach (var account in accounts)
            {
                var balance = await _accountingService.CalculateAccountBalanceAsync(account.OfficialCode, asOfDate);
                var entries = await _accountingService.GetAccountLedgerEntriesAsync(account.OfficialCode, 
                    DateOnly.FromDateTime(DateTime.Today.AddYears(-1)), asOfDate);

                var accountBalance = new DetailedAccountBalance
                {
                    AccountCode = account.OfficialCode,
                    AccountName = account.Description,
                    AccountType = account.AccountType.ToString(),
                    Balance = balance,
                    DebitBalance = balance >= 0 ? balance : 0,
                    CreditBalance = balance < 0 ? Math.Abs(balance) : 0,
                    TransactionCount = entries.Count(),
                    LastTransactionDate = entries.Any() ? entries.Max(e => e.TransactionDate) : null
                };

                report.AccountBalances.Add(accountBalance);
                
                if (accountBalance.DebitBalance > 0)
                    totalDebits += accountBalance.DebitBalance;
                if (accountBalance.CreditBalance > 0)
                    totalCredits += accountBalance.CreditBalance;
            }

            report.TotalDebits = totalDebits;
            report.TotalCredits = totalCredits;
            report.IsBalanced = Math.Abs(totalDebits - totalCredits) < 0.01m;

            _erpLogger.LogPerformanceMetrics("TrialBalanceGeneration", DateTime.UtcNow - report.GeneratedAt.Value, accounts.Count);

            return report;
        }

        /// <summary>
        /// Generates cash flow statement using direct method
        /// </summary>
        /// <param name="fromDate">Start date</param>
        /// <param name="toDate">End date</param>
        /// <returns>Cash flow statement</returns>
        [Description("Generates cash flow statement")]
        public async Task<CashFlowStatement> GenerateCashFlowStatementAsync(DateOnly fromDate, DateOnly toDate)
        {
            using var timer = _erpLogger.StartOperationTimer("GenerateCashFlowStatement");

            _logger.LogInformation("Generating cash flow statement for period {FromDate} to {ToDate}", fromDate, toDate);

            var cashAccounts = _repository.GetObjects<IAccount>()
                .Where(a => a.AccountType == AccountType.Asset && a.Description.ToLower().Contains("cash"))
                .ToList();

            var statement = new CashFlowStatement
            {
                PeriodStart = fromDate,
                PeriodEnd = toDate,
                GeneratedAt = DateTime.UtcNow
            };

            // Operating Activities
            statement.OperatingActivities = await CalculateOperatingCashFlowAsync(fromDate, toDate);
            
            // Investing Activities
            statement.InvestingActivities = await CalculateInvestingCashFlowAsync(fromDate, toDate);
            
            // Financing Activities
            statement.FinancingActivities = await CalculateFinancingCashFlowAsync(fromDate, toDate);

            // Net Cash Flow
            statement.NetCashFlow = statement.OperatingActivities.Sum(a => a.Amount) +
                                   statement.InvestingActivities.Sum(a => a.Amount) +
                                   statement.FinancingActivities.Sum(a => a.Amount);

            // Cash at beginning and end
            statement.CashAtBeginning = 0; // Would calculate from previous period
            statement.CashAtEnd = statement.CashAtBeginning + statement.NetCashFlow;

            await Task.CompletedTask;
            return statement;
        }

        /// <summary>
        /// Gets account aging analysis for accounts receivable/payable
        /// </summary>
        /// <param name="accountCode">Account to analyze</param>
        /// <param name="asOfDate">Analysis date</param>
        /// <returns>Aging analysis</returns>
        [Description("Gets account aging analysis")]
        public async Task<AccountAgingAnalysis> GetAccountAgingAnalysisAsync(string accountCode, DateOnly asOfDate)
        {
            using var timer = _erpLogger.StartOperationTimer("GetAccountAgingAnalysis");

            var entries = await _accountingService.GetAccountLedgerEntriesAsync(accountCode, 
                DateOnly.FromDateTime(DateTime.Today.AddYears(-2)), asOfDate);

            var analysis = new AccountAgingAnalysis
            {
                AccountCode = accountCode,
                AsOfDate = asOfDate,
                Current = 0,
                Days30 = 0,
                Days60 = 0,
                Days90 = 0,
                Days120Plus = 0
            };

            foreach (var entry in entries.Where(e => e.Amount != 0))
            {
                var daysDiff = (asOfDate.ToDateTime(TimeOnly.MinValue) - entry.TransactionDate.ToDateTime(TimeOnly.MinValue)).Days;
                
                if (daysDiff <= 30)
                    analysis.Current += entry.Amount;
                else if (daysDiff <= 60)
                    analysis.Days30 += entry.Amount;
                else if (daysDiff <= 90)
                    analysis.Days60 += entry.Amount;
                else if (daysDiff <= 120)
                    analysis.Days90 += entry.Amount;
                else
                    analysis.Days120Plus += entry.Amount;
            }

            analysis.TotalBalance = analysis.Current + analysis.Days30 + analysis.Days60 + analysis.Days90 + analysis.Days120Plus;

            return analysis;
        }

        /// <summary>
        /// Gets transaction volume analytics
        /// </summary>
        /// <param name="fromDate">Start date</param>
        /// <param name="toDate">End date</param>
        /// <returns>Transaction analytics</returns>
        [Description("Gets transaction volume analytics")]
        public async Task<TransactionVolumeAnalytics> GetTransactionVolumeAnalyticsAsync(DateOnly fromDate, DateOnly toDate)
        {
            using var timer = _erpLogger.StartOperationTimer("GetTransactionVolumeAnalytics");

            var transactions = _repository.GetObjects<ITransaction>()
                .Where(t => t.TransactionDate >= fromDate && t.TransactionDate <= toDate)
                .ToList();

            var analytics = new TransactionVolumeAnalytics
            {
                PeriodStart = fromDate,
                PeriodEnd = toDate,
                TotalTransactions = transactions.Count,
                TotalAmount = transactions.Sum(t => Math.Abs(t.TotalAmount)),
                AverageTransactionSize = transactions.Any() ? transactions.Average(t => Math.Abs(t.TotalAmount)) : 0,
                DailyVolumes = new List<DailyTransactionVolume>()
            };

            // Daily breakdown
            var dailyGroups = transactions
                .GroupBy(t => t.TransactionDate)
                .OrderBy(g => g.Key);

            foreach (var dailyGroup in dailyGroups)
            {
                analytics.DailyVolumes.Add(new DailyTransactionVolume
                {
                    Date = dailyGroup.Key,
                    TransactionCount = dailyGroup.Count(),
                    TotalAmount = dailyGroup.Sum(t => Math.Abs(t.TotalAmount)),
                    AverageAmount = dailyGroup.Average(t => Math.Abs(t.TotalAmount))
                });
            }

            await Task.CompletedTask;
            return analytics;
        }

        /// <summary>
        /// Performs account reconciliation
        /// </summary>
        /// <param name="accountCode">Account to reconcile</param>
        /// <param name="fromDate">Start date</param>
        /// <param name="toDate">End date</param>
        /// <returns>Reconciliation results</returns>
        [Description("Performs account reconciliation")]
        public async Task<AccountReconciliation> PerformAccountReconciliationAsync(string accountCode, DateOnly fromDate, DateOnly toDate)
        {
            using var timer = _erpLogger.StartOperationTimer("PerformAccountReconciliation");

            var entries = await _accountingService.GetAccountLedgerEntriesAsync(accountCode, fromDate, toDate);
            var reconciliation = new AccountReconciliation
            {
                AccountCode = accountCode,
                PeriodStart = fromDate,
                PeriodEnd = toDate,
                PerformedAt = DateTime.UtcNow
            };

            // Calculate beginning balance
            reconciliation.BeginningBalance = await _accountingService.CalculateAccountBalanceAsync(accountCode, fromDate.AddDays(-1));
            
            // Calculate ending balance
            reconciliation.EndingBalance = await _accountingService.CalculateAccountBalanceAsync(accountCode, toDate);
            
            // Calculate activity
            reconciliation.TotalDebits = entries.Where(e => e.Amount > 0).Sum(e => e.Amount);
            reconciliation.TotalCredits = entries.Where(e => e.Amount < 0).Sum(e => Math.Abs(e.Amount));
            reconciliation.NetActivity = reconciliation.TotalDebits - reconciliation.TotalCredits;
            
            // Verify reconciliation
            var calculatedEndingBalance = reconciliation.BeginningBalance + reconciliation.NetActivity;
            reconciliation.ReconciliationDifference = reconciliation.EndingBalance - calculatedEndingBalance;
            reconciliation.IsReconciled = Math.Abs(reconciliation.ReconciliationDifference) < 0.01m;

            return reconciliation;
        }

        #region Private Helper Methods

        private async Task<BalanceSheet> GenerateBalanceSheetAsync(List<IAccount> accounts, DateOnly asOfDate)
        {
            var balanceSheet = new BalanceSheet
            {
                AsOfDate = asOfDate,
                Assets = new List<AccountBalance>(),
                Liabilities = new List<AccountBalance>(),
                Equity = new List<AccountBalance>()
            };

            foreach (var account in accounts)
            {
                var balance = await _accountingService.CalculateAccountBalanceAsync(account.OfficialCode, asOfDate);
                var accountBalance = new AccountBalance
                {
                    AccountCode = account.OfficialCode,
                    AccountName = account.Description,
                    Balance = balance
                };

                switch (account.AccountType)
                {
                    case AccountType.Asset:
                        balanceSheet.Assets.Add(accountBalance);
                        break;
                    case AccountType.Liability:
                        balanceSheet.Liabilities.Add(accountBalance);
                        break;
                    case AccountType.Equity:
                        balanceSheet.Equity.Add(accountBalance);
                        break;
                }
            }

            balanceSheet.TotalAssets = balanceSheet.Assets.Sum(a => a.Balance);
            balanceSheet.TotalLiabilities = balanceSheet.Liabilities.Sum(l => l.Balance);
            balanceSheet.TotalEquity = balanceSheet.Equity.Sum(e => e.Balance);

            return balanceSheet;
        }

        private async Task<IncomeStatement> GenerateIncomeStatementAsync(List<IAccount> accounts, DateOnly fromDate, DateOnly toDate)
        {
            var incomeStatement = new IncomeStatement
            {
                PeriodStart = fromDate,
                PeriodEnd = toDate,
                Revenues = new List<AccountBalance>(),
                Expenses = new List<AccountBalance>()
            };

            foreach (var account in accounts.Where(a => a.AccountType == AccountType.Revenue || a.AccountType == AccountType.Expense))
            {
                var entries = await _accountingService.GetAccountLedgerEntriesAsync(account.OfficialCode, fromDate, toDate);
                var periodBalance = entries.Sum(e => e.Amount);

                var accountBalance = new AccountBalance
                {
                    AccountCode = account.OfficialCode,
                    AccountName = account.Description,
                    Balance = periodBalance
                };

                if (account.AccountType == AccountType.Revenue)
                    incomeStatement.Revenues.Add(accountBalance);
                else
                    incomeStatement.Expenses.Add(accountBalance);
            }

            incomeStatement.TotalRevenues = incomeStatement.Revenues.Sum(r => r.Balance);
            incomeStatement.TotalExpenses = incomeStatement.Expenses.Sum(e => e.Balance);
            incomeStatement.NetIncome = incomeStatement.TotalRevenues - incomeStatement.TotalExpenses;

            return incomeStatement;
        }

        private async Task<List<CashFlowActivity>> CalculateOperatingCashFlowAsync(DateOnly fromDate, DateOnly toDate)
        {
            // Simplified implementation - would need more sophisticated logic in production
            await Task.CompletedTask;
            return new List<CashFlowActivity>
            {
                new() { Description = "Cash receipts from customers", Amount = 0 },
                new() { Description = "Cash payments to suppliers", Amount = 0 },
                new() { Description = "Cash payments for operating expenses", Amount = 0 }
            };
        }

        private async Task<List<CashFlowActivity>> CalculateInvestingCashFlowAsync(DateOnly fromDate, DateOnly toDate)
        {
            await Task.CompletedTask;
            return new List<CashFlowActivity>
            {
                new() { Description = "Purchase of equipment", Amount = 0 },
                new() { Description = "Sale of investments", Amount = 0 }
            };
        }

        private async Task<List<CashFlowActivity>> CalculateFinancingCashFlowAsync(DateOnly fromDate, DateOnly toDate)
        {
            await Task.CompletedTask;
            return new List<CashFlowActivity>
            {
                new() { Description = "Proceeds from loans", Amount = 0 },
                new() { Description = "Dividend payments", Amount = 0 }
            };
        }

        #endregion
    }
}
