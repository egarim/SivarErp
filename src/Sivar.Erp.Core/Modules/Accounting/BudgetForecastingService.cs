using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Sivar.Erp.Core.Core;
using Sivar.Erp.Core.Infrastructure.Logging;
using Sivar.Erp.Core.Modules.Domain;

namespace Sivar.Erp.Core.Modules.Accounting
{
    /// <summary>
    /// Budget and forecasting service for financial planning
    /// </summary>
    [Description("Budget and forecasting service")]
    public interface IBudgetForecastingService
    {
        /// <summary>
        /// Creates a new budget for a fiscal period
        /// </summary>
        Task<Budget> CreateBudgetAsync(BudgetRequest request, string userId);

        /// <summary>
        /// Updates an existing budget
        /// </summary>
        Task<Budget> UpdateBudgetAsync(string budgetId, BudgetUpdateRequest request, string userId);

        /// <summary>
        /// Gets budget vs actual analysis
        /// </summary>
        Task<BudgetAnalysis> GetBudgetAnalysisAsync(string budgetId, DateOnly fromDate, DateOnly toDate);

        /// <summary>
        /// Creates financial forecast based on historical data
        /// </summary>
        Task<FinancialForecast> CreateForecastAsync(ForecastRequest request, string userId);

        /// <summary>
        /// Updates forecast with actual data
        /// </summary>
        Task<FinancialForecast> UpdateForecastAsync(string forecastId, ForecastUpdateRequest request, string userId);

        /// <summary>
        /// Gets budget variance report
        /// </summary>
        Task<BudgetVarianceReport> GetBudgetVarianceReportAsync(string budgetId, DateOnly asOfDate);

        /// <summary>
        /// Approves or rejects a budget
        /// </summary>
        Task<BudgetApprovalResult> ProcessBudgetApprovalAsync(string budgetId, BudgetApprovalRequest request, string userId);

        /// <summary>
        /// Creates rolling forecast
        /// </summary>
        Task<RollingForecast> CreateRollingForecastAsync(RollingForecastRequest request, string userId);
    }

    /// <summary>
    /// Implementation of budget and forecasting service
    /// </summary>
    [Description("Budget and forecasting service implementation")]
    public class BudgetForecastingService : IBudgetForecastingService
    {
        private readonly IRepository _repository;
        private readonly IAccountingService _accountingService;
        private readonly IAccountingAnalyticsService _analyticsService;
        private readonly IErpLoggingService _erpLogger;
        private readonly ILogger<BudgetForecastingService> _logger;

        public BudgetForecastingService(
            IRepository repository,
            IAccountingService accountingService,
            IAccountingAnalyticsService analyticsService,
            IErpLoggingService erpLogger,
            ILogger<BudgetForecastingService> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _accountingService = accountingService ?? throw new ArgumentNullException(nameof(accountingService));
            _analyticsService = analyticsService ?? throw new ArgumentNullException(nameof(analyticsService));
            _erpLogger = erpLogger ?? throw new ArgumentNullException(nameof(erpLogger));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Creates a comprehensive budget for financial planning
        /// </summary>
        /// <param name="request">Budget creation request</param>
        /// <param name="userId">User creating the budget</param>
        /// <returns>Created budget</returns>
        [Description("Creates a new budget")]
        public async Task<Budget> CreateBudgetAsync(BudgetRequest request, string userId)
        {
            using var timer = _erpLogger.StartOperationTimer("CreateBudget");

            _logger.LogInformation("Creating budget '{BudgetName}' for period {StartDate} to {EndDate} by user {UserId}", 
                request.BudgetName, request.PeriodStart, request.PeriodEnd, userId);

            var budget = new Budget
            {
                Id = Guid.NewGuid().ToString(),
                BudgetName = request.BudgetName,
                Description = request.Description,
                PeriodStart = request.PeriodStart,
                PeriodEnd = request.PeriodEnd,
                BudgetType = request.BudgetType,
                Status = BudgetStatus.Draft,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                BudgetLines = new List<BudgetLine>(),
                Version = 1
            };

            try
            {
                // Create budget lines based on request
                foreach (var lineRequest in request.BudgetLines)
                {
                    var budgetLine = new BudgetLine
                    {
                        Id = Guid.NewGuid().ToString(),
                        AccountCode = lineRequest.AccountCode,
                        BudgetAmount = lineRequest.BudgetAmount,
                        Notes = lineRequest.Notes,
                        MonthlyBreakdown = await CreateMonthlyBreakdownAsync(lineRequest, request.PeriodStart, request.PeriodEnd)
                    };

                    budget.BudgetLines.Add(budgetLine);
                }

                // Calculate totals
                budget.TotalBudgetAmount = budget.BudgetLines.Sum(bl => bl.BudgetAmount);

                // Save budget
                await SaveBudgetAsync(budget);

                _erpLogger.LogAuditEvent("BudgetCreated", userId, $"Created budget '{budget.BudgetName}' with {budget.BudgetLines.Count} lines");

                return budget;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create budget '{BudgetName}'", request.BudgetName);
                throw;
            }
        }

        /// <summary>
        /// Updates an existing budget with new data
        /// </summary>
        /// <param name="budgetId">Budget identifier</param>
        /// <param name="request">Update request</param>
        /// <param name="userId">User updating the budget</param>
        /// <returns>Updated budget</returns>
        [Description("Updates an existing budget")]
        public async Task<Budget> UpdateBudgetAsync(string budgetId, BudgetUpdateRequest request, string userId)
        {
            using var timer = _erpLogger.StartOperationTimer("UpdateBudget");

            var budget = await GetBudgetByIdAsync(budgetId);
            if (budget == null)
                throw new InvalidOperationException($"Budget with ID {budgetId} not found");

            if (budget.Status == BudgetStatus.Approved && !request.CreateNewVersion)
                throw new InvalidOperationException("Cannot modify approved budget without creating new version");

            try
            {
                if (request.CreateNewVersion)
                {
                    budget = await CreateBudgetVersionAsync(budget, userId);
                }

                // Update budget properties
                if (!string.IsNullOrEmpty(request.BudgetName))
                    budget.BudgetName = request.BudgetName;
                
                if (!string.IsNullOrEmpty(request.Description))
                    budget.Description = request.Description;

                budget.ModifiedBy = userId;
                budget.ModifiedAt = DateTime.UtcNow;

                // Update budget lines
                foreach (var lineUpdate in request.LineUpdates)
                {
                    var existingLine = budget.BudgetLines.FirstOrDefault(bl => bl.Id == lineUpdate.LineId);
                    if (existingLine != null)
                    {
                        existingLine.BudgetAmount = lineUpdate.NewBudgetAmount;
                        existingLine.Notes = lineUpdate.Notes;
                        existingLine.MonthlyBreakdown = await RecalculateMonthlyBreakdownAsync(existingLine, budget.PeriodStart, budget.PeriodEnd);
                    }
                }

                // Recalculate totals
                budget.TotalBudgetAmount = budget.BudgetLines.Sum(bl => bl.BudgetAmount);

                await SaveBudgetAsync(budget);

                _erpLogger.LogAuditEvent("BudgetUpdated", userId, $"Updated budget '{budget.BudgetName}' version {budget.Version}");

                return budget;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update budget {BudgetId}", budgetId);
                throw;
            }
        }

        /// <summary>
        /// Analyzes budget performance against actual results
        /// </summary>
        /// <param name="budgetId">Budget identifier</param>
        /// <param name="fromDate">Analysis start date</param>
        /// <param name="toDate">Analysis end date</param>
        /// <returns>Budget analysis results</returns>
        [Description("Gets budget vs actual analysis")]
        public async Task<BudgetAnalysis> GetBudgetAnalysisAsync(string budgetId, DateOnly fromDate, DateOnly toDate)
        {
            using var timer = _erpLogger.StartOperationTimer("GetBudgetAnalysis");

            var budget = await GetBudgetByIdAsync(budgetId);
            if (budget == null)
                throw new InvalidOperationException($"Budget with ID {budgetId} not found");

            var analysis = new BudgetAnalysis
            {
                BudgetId = budgetId,
                BudgetName = budget.BudgetName,
                AnalysisPeriodStart = fromDate,
                AnalysisPeriodEnd = toDate,
                GeneratedAt = DateTime.UtcNow,
                AccountAnalyses = new List<AccountBudgetAnalysis>()
            };

            try
            {
                foreach (var budgetLine in budget.BudgetLines)
                {
                    // Get actual amounts for the period
                    var actualEntries = await _accountingService.GetAccountLedgerEntriesAsync(
                        budgetLine.AccountCode, fromDate, toDate);
                    
                    var actualAmount = actualEntries.Sum(e => e.Amount);
                    
                    // Calculate budget amount for the period
                    var budgetAmount = CalculateBudgetAmountForPeriod(budgetLine, fromDate, toDate);
                    
                    var accountAnalysis = new AccountBudgetAnalysis
                    {
                        AccountCode = budgetLine.AccountCode,
                        BudgetAmount = budgetAmount,
                        ActualAmount = actualAmount,
                        Variance = actualAmount - budgetAmount,
                        VariancePercentage = budgetAmount != 0 ? ((actualAmount - budgetAmount) / budgetAmount) * 100 : 0,
                        IsFavorable = DetermineIfVarianceIsFavorable(budgetLine.AccountCode, actualAmount - budgetAmount)
                    };

                    analysis.AccountAnalyses.Add(accountAnalysis);
                }

                // Calculate summary metrics
                analysis.TotalBudgetAmount = analysis.AccountAnalyses.Sum(a => a.BudgetAmount);
                analysis.TotalActualAmount = analysis.AccountAnalyses.Sum(a => a.ActualAmount);
                analysis.TotalVariance = analysis.TotalActualAmount - analysis.TotalBudgetAmount;
                analysis.TotalVariancePercentage = analysis.TotalBudgetAmount != 0 ? 
                    (analysis.TotalVariance / analysis.TotalBudgetAmount) * 100 : 0;

                return analysis;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate budget analysis for budget {BudgetId}", budgetId);
                throw;
            }
        }

        /// <summary>
        /// Creates financial forecast using historical trends and statistical models
        /// </summary>
        /// <param name="request">Forecast creation request</param>
        /// <param name="userId">User creating the forecast</param>
        /// <returns>Financial forecast</returns>
        [Description("Creates financial forecast")]
        public async Task<FinancialForecast> CreateForecastAsync(ForecastRequest request, string userId)
        {
            using var timer = _erpLogger.StartOperationTimer("CreateForecast");

            _logger.LogInformation("Creating forecast '{ForecastName}' for period {StartDate} to {EndDate}",
                request.ForecastName, request.ForecastPeriodStart, request.ForecastPeriodEnd);

            var forecast = new FinancialForecast
            {
                Id = Guid.NewGuid().ToString(),
                ForecastName = request.ForecastName,
                Description = request.Description,
                ForecastPeriodStart = request.ForecastPeriodStart,
                ForecastPeriodEnd = request.ForecastPeriodEnd,
                BasePeriodStart = request.BasePeriodStart,
                BasePeriodEnd = request.BasePeriodEnd,
                ForecastMethod = request.ForecastMethod,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                ForecastLines = new List<ForecastLine>()
            };

            try
            {
                var accounts = _repository.GetObjects<IAccount>().ToList();

                foreach (var account in accounts.Where(a => request.AccountCodes.Contains(a.OfficialCode)))
                {
                    var forecastLine = await CreateForecastLineAsync(account, request, forecast);
                    forecast.ForecastLines.Add(forecastLine);
                }

                await SaveForecastAsync(forecast);

                _erpLogger.LogAuditEvent("ForecastCreated", userId, $"Created forecast '{forecast.ForecastName}' with {forecast.ForecastLines.Count} lines");

                return forecast;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create forecast '{ForecastName}'", request.ForecastName);
                throw;
            }
        }

        /// <summary>
        /// Updates forecast with new actual data and revised projections
        /// </summary>
        /// <param name="forecastId">Forecast identifier</param>
        /// <param name="request">Update request</param>
        /// <param name="userId">User updating the forecast</param>
        /// <returns>Updated forecast</returns>
        [Description("Updates forecast")]
        public async Task<FinancialForecast> UpdateForecastAsync(string forecastId, ForecastUpdateRequest request, string userId)
        {
            using var timer = _erpLogger.StartOperationTimer("UpdateForecast");

            var forecast = await GetForecastByIdAsync(forecastId);
            if (forecast == null)
                throw new InvalidOperationException($"Forecast with ID {forecastId} not found");

            try
            {
                forecast.ModifiedBy = userId;
                forecast.ModifiedAt = DateTime.UtcNow;

                // Update forecast lines
                foreach (var lineUpdate in request.LineUpdates)
                {
                    var existingLine = forecast.ForecastLines.FirstOrDefault(fl => fl.Id == lineUpdate.LineId);
                    if (existingLine != null)
                    {
                        existingLine.ForecastAmount = lineUpdate.NewForecastAmount;
                        existingLine.Confidence = lineUpdate.Confidence;
                        existingLine.Notes = lineUpdate.Notes;
                    }
                }

                // Refresh forecast with latest actual data
                await RefreshForecastWithActualsAsync(forecast);

                await SaveForecastAsync(forecast);

                _erpLogger.LogAuditEvent("ForecastUpdated", userId, $"Updated forecast '{forecast.ForecastName}'");

                return forecast;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update forecast {ForecastId}", forecastId);
                throw;
            }
        }

        /// <summary>
        /// Generates comprehensive budget variance report
        /// </summary>
        /// <param name="budgetId">Budget identifier</param>
        /// <param name="asOfDate">Report date</param>
        /// <returns>Budget variance report</returns>
        [Description("Gets budget variance report")]
        public async Task<BudgetVarianceReport> GetBudgetVarianceReportAsync(string budgetId, DateOnly asOfDate)
        {
            using var timer = _erpLogger.StartOperationTimer("GetBudgetVarianceReport");

            var budget = await GetBudgetByIdAsync(budgetId);
            if (budget == null)
                throw new InvalidOperationException($"Budget with ID {budgetId} not found");

            var report = new BudgetVarianceReport
            {
                BudgetId = budgetId,
                BudgetName = budget.BudgetName,
                ReportDate = asOfDate,
                GeneratedAt = DateTime.UtcNow,
                VarianceAnalyses = new List<VarianceAnalysis>()
            };

            try
            {
                foreach (var budgetLine in budget.BudgetLines)
                {
                    var periodStart = new DateOnly(asOfDate.Year, asOfDate.Month, 1);
                    var actualEntries = await _accountingService.GetAccountLedgerEntriesAsync(
                        budgetLine.AccountCode, periodStart, asOfDate);

                    var actualAmount = actualEntries.Sum(e => e.Amount);
                    var budgetAmount = GetMonthlyBudgetAmount(budgetLine, asOfDate);

                    var variance = new VarianceAnalysis
                    {
                        AccountCode = budgetLine.AccountCode,
                        PeriodBudget = budgetAmount,
                        PeriodActual = actualAmount,
                        PeriodVariance = actualAmount - budgetAmount,
                        YearToDateBudget = CalculateYearToDateBudget(budgetLine, budget.PeriodStart, asOfDate),
                        YearToDateActual = await CalculateYearToDateActual(budgetLine.AccountCode, budget.PeriodStart, asOfDate),
                        VarianceReason = await AnalyzeVarianceReason(budgetLine.AccountCode, actualAmount, budgetAmount)
                    };

                    variance.YearToDateVariance = variance.YearToDateActual - variance.YearToDateBudget;
                    report.VarianceAnalyses.Add(variance);
                }

                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate budget variance report for budget {BudgetId}", budgetId);
                throw;
            }
        }

        /// <summary>
        /// Processes budget approval workflow
        /// </summary>
        /// <param name="budgetId">Budget identifier</param>
        /// <param name="request">Approval request</param>
        /// <param name="userId">User processing approval</param>
        /// <returns>Approval result</returns>
        [Description("Processes budget approval")]
        public async Task<BudgetApprovalResult> ProcessBudgetApprovalAsync(string budgetId, BudgetApprovalRequest request, string userId)
        {
            using var timer = _erpLogger.StartOperationTimer("ProcessBudgetApproval");

            var budget = await GetBudgetByIdAsync(budgetId);
            if (budget == null)
                throw new InvalidOperationException($"Budget with ID {budgetId} not found");

            var result = new BudgetApprovalResult
            {
                BudgetId = budgetId,
                RequestedAction = request.Action,
                ProcessedBy = userId,
                ProcessedAt = DateTime.UtcNow,
                IsSuccessful = true
            };

            try
            {
                switch (request.Action.ToLower())
                {
                    case "approve":
                        budget.Status = BudgetStatus.Approved;
                        budget.ApprovedBy = userId;
                        budget.ApprovedAt = DateTime.UtcNow;
                        result.Message = "Budget approved successfully";
                        break;

                    case "reject":
                        budget.Status = BudgetStatus.Rejected;
                        budget.RejectedBy = userId;
                        budget.RejectedAt = DateTime.UtcNow;
                        budget.RejectionReason = request.Comments;
                        result.Message = "Budget rejected";
                        break;

                    case "send_back":
                        budget.Status = BudgetStatus.Draft;
                        result.Message = "Budget sent back for revision";
                        break;

                    default:
                        throw new InvalidOperationException($"Unknown approval action: {request.Action}");
                }

                budget.ApprovalComments = request.Comments;
                await SaveBudgetAsync(budget);

                _erpLogger.LogAuditEvent("BudgetApprovalProcessed", userId, 
                    $"Budget '{budget.BudgetName}' {request.Action.ToLower()}ed by {userId}");

                return result;
            }
            catch (Exception ex)
            {
                result.IsSuccessful = false;
                result.ErrorMessage = ex.Message;
                _logger.LogError(ex, "Failed to process budget approval for budget {BudgetId}", budgetId);
                return result;
            }
        }

        /// <summary>
        /// Creates rolling forecast with continuous updates
        /// </summary>
        /// <param name="request">Rolling forecast request</param>
        /// <param name="userId">User creating the forecast</param>
        /// <returns>Rolling forecast</returns>
        [Description("Creates rolling forecast")]
        public async Task<RollingForecast> CreateRollingForecastAsync(RollingForecastRequest request, string userId)
        {
            using var timer = _erpLogger.StartOperationTimer("CreateRollingForecast");

            var rollingForecast = new RollingForecast
            {
                Id = Guid.NewGuid().ToString(),
                ForecastName = request.ForecastName,
                Description = request.Description,
                ForecastPeriods = request.ForecastPeriods,
                UpdateFrequency = request.UpdateFrequency,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                Periods = new List<ForecastPeriod>()
            };

            try
            {
                // Create forecast periods
                var currentDate = DateOnly.FromDateTime(DateTime.Today);
                for (int i = 0; i < request.ForecastPeriods; i++)
                {
                    var periodStart = currentDate.AddMonths(i);
                    var periodEnd = periodStart.AddMonths(1).AddDays(-1);

                    var period = new ForecastPeriod
                    {
                        PeriodNumber = i + 1,
                        PeriodStart = periodStart,
                        PeriodEnd = periodEnd,
                        ForecastLines = await CreatePeriodForecastLinesAsync(request.AccountCodes, periodStart, periodEnd)
                    };

                    rollingForecast.Periods.Add(period);
                }

                await SaveRollingForecastAsync(rollingForecast);

                _erpLogger.LogAuditEvent("RollingForecastCreated", userId, 
                    $"Created rolling forecast '{rollingForecast.ForecastName}' with {rollingForecast.ForecastPeriods} periods");

                return rollingForecast;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create rolling forecast '{ForecastName}'", request.ForecastName);
                throw;
            }
        }

        #region Private Helper Methods

        private async Task<List<MonthlyBudgetAmount>> CreateMonthlyBreakdownAsync(
            BudgetLineRequest lineRequest, DateOnly periodStart, DateOnly periodEnd)
        {
            var monthlyBreakdown = new List<MonthlyBudgetAmount>();
            var currentDate = periodStart;

            while (currentDate <= periodEnd)
            {
                var monthlyAmount = lineRequest.BudgetAmount / 
                    ((periodEnd.Year - periodStart.Year) * 12 + (periodEnd.Month - periodStart.Month) + 1);

                monthlyBreakdown.Add(new MonthlyBudgetAmount
                {
                    Year = currentDate.Year,
                    Month = currentDate.Month,
                    Amount = monthlyAmount
                });

                currentDate = currentDate.AddMonths(1);
            }

            await Task.CompletedTask;
            return monthlyBreakdown;
        }

        private async Task<Budget?> GetBudgetByIdAsync(string budgetId)
        {
            // Implementation would retrieve from repository
            await Task.CompletedTask;
            return null;
        }

        private async Task SaveBudgetAsync(Budget budget)
        {
            // Implementation would save to repository
            await Task.CompletedTask;
        }

        private async Task<Budget> CreateBudgetVersionAsync(Budget originalBudget, string userId)
        {
            // Implementation would create new version
            await Task.CompletedTask;
            return originalBudget;
        }

        private async Task<List<MonthlyBudgetAmount>> RecalculateMonthlyBreakdownAsync(
            BudgetLine budgetLine, DateOnly periodStart, DateOnly periodEnd)
        {
            // Implementation would recalculate monthly breakdown
            await Task.CompletedTask;
            return budgetLine.MonthlyBreakdown;
        }

        private decimal CalculateBudgetAmountForPeriod(BudgetLine budgetLine, DateOnly fromDate, DateOnly toDate)
        {
            // Implementation would calculate budget amount for specific period
            return budgetLine.BudgetAmount;
        }

        private bool DetermineIfVarianceIsFavorable(string accountCode, decimal variance)
        {
            // Implementation would determine if variance is favorable based on account type
            return variance >= 0;
        }

        private async Task<ForecastLine> CreateForecastLineAsync(IAccount account, ForecastRequest request, FinancialForecast forecast)
        {
            // Implementation would create forecast line using statistical models
            await Task.CompletedTask;
            return new ForecastLine { AccountCode = account.OfficialCode };
        }

        private async Task SaveForecastAsync(FinancialForecast forecast)
        {
            // Implementation would save forecast to repository
            await Task.CompletedTask;
        }

        private async Task<FinancialForecast?> GetForecastByIdAsync(string forecastId)
        {
            // Implementation would retrieve forecast from repository
            await Task.CompletedTask;
            return null;
        }

        private async Task RefreshForecastWithActualsAsync(FinancialForecast forecast)
        {
            // Implementation would update forecast with latest actual data
            await Task.CompletedTask;
        }

        private decimal GetMonthlyBudgetAmount(BudgetLine budgetLine, DateOnly asOfDate)
        {
            // Implementation would get budget amount for specific month
            return budgetLine.BudgetAmount / 12;
        }

        private decimal CalculateYearToDateBudget(BudgetLine budgetLine, DateOnly periodStart, DateOnly asOfDate)
        {
            // Implementation would calculate YTD budget
            return budgetLine.BudgetAmount;
        }

        private async Task<decimal> CalculateYearToDateActual(string accountCode, DateOnly periodStart, DateOnly asOfDate)
        {
            // Implementation would calculate YTD actual
            await Task.CompletedTask;
            return 0;
        }

        private async Task<string> AnalyzeVarianceReason(string accountCode, decimal actualAmount, decimal budgetAmount)
        {
            // Implementation would analyze variance reason
            await Task.CompletedTask;
            return "No significant variance";
        }

        private async Task SaveRollingForecastAsync(RollingForecast rollingForecast)
        {
            // Implementation would save rolling forecast
            await Task.CompletedTask;
        }

        private async Task<List<ForecastLine>> CreatePeriodForecastLinesAsync(List<string> accountCodes, DateOnly periodStart, DateOnly periodEnd)
        {
            // Implementation would create forecast lines for period
            await Task.CompletedTask;
            return new List<ForecastLine>();
        }

        #endregion
    }
}
