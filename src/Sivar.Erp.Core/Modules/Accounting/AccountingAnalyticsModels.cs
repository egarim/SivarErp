using System.ComponentModel;

namespace Sivar.Erp.Core.Modules.Accounting
{
    /// <summary>
    /// Complete financial statements for a period
    /// </summary>
    [Description("Complete financial statements")]
    public class FinancialStatements
    {
        public DateOnly PeriodStart { get; set; }
        public DateOnly PeriodEnd { get; set; }
        public DateTime? GeneratedAt { get; set; }
        public BalanceSheet BalanceSheet { get; set; } = new();
        public IncomeStatement IncomeStatement { get; set; } = new();
        public CashFlowStatement CashFlowStatement { get; set; } = new();
    }

    /// <summary>
    /// Balance sheet financial statement
    /// </summary>
    [Description("Balance sheet")]
    public class BalanceSheet
    {
        public DateOnly AsOfDate { get; set; }
        public List<AccountBalance> Assets { get; set; } = new();
        public List<AccountBalance> Liabilities { get; set; } = new();
        public List<AccountBalance> Equity { get; set; } = new();
        public decimal TotalAssets { get; set; }
        public decimal TotalLiabilities { get; set; }
        public decimal TotalEquity { get; set; }
        public bool IsBalanced => Math.Abs(TotalAssets - (TotalLiabilities + TotalEquity)) < 0.01m;
    }

    /// <summary>
    /// Income statement
    /// </summary>
    [Description("Income statement")]
    public class IncomeStatement
    {
        public DateOnly PeriodStart { get; set; }
        public DateOnly PeriodEnd { get; set; }
        public List<AccountBalance> Revenues { get; set; } = new();
        public List<AccountBalance> Expenses { get; set; } = new();
        public decimal TotalRevenues { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal NetIncome { get; set; }
        public decimal ProfitMargin => TotalRevenues != 0 ? (NetIncome / TotalRevenues) * 100 : 0;
    }

    /// <summary>
    /// Cash flow statement
    /// </summary>
    [Description("Cash flow statement")]
    public class CashFlowStatement
    {
        public DateOnly PeriodStart { get; set; }
        public DateOnly PeriodEnd { get; set; }
        public DateTime? GeneratedAt { get; set; }
        public List<CashFlowActivity> OperatingActivities { get; set; } = new();
        public List<CashFlowActivity> InvestingActivities { get; set; } = new();
        public List<CashFlowActivity> FinancingActivities { get; set; } = new();
        public decimal NetCashFlow { get; set; }
        public decimal CashAtBeginning { get; set; }
        public decimal CashAtEnd { get; set; }
    }

    /// <summary>
    /// Cash flow activity item
    /// </summary>
    [Description("Cash flow activity")]
    public class CashFlowActivity
    {
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Category { get; set; } = string.Empty;
    }

    /// <summary>
    /// Account balance with details
    /// </summary>
    [Description("Account balance")]
    public class AccountBalance
    {
        public string AccountCode { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public string AccountCategory { get; set; } = string.Empty;
    }

    /// <summary>
    /// Detailed trial balance report
    /// </summary>
    [Description("Trial balance report")]
    public class TrialBalanceReport
    {
        public DateOnly AsOfDate { get; set; }
        public DateTime? GeneratedAt { get; set; }
        public List<DetailedAccountBalance> AccountBalances { get; set; } = new();
        public decimal TotalDebits { get; set; }
        public decimal TotalCredits { get; set; }
        public bool IsBalanced { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    /// <summary>
    /// Detailed account balance for trial balance
    /// </summary>
    [Description("Detailed account balance")]
    public class DetailedAccountBalance
    {
        public string AccountCode { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public decimal DebitBalance { get; set; }
        public decimal CreditBalance { get; set; }
        public int TransactionCount { get; set; }
        public DateOnly? LastTransactionDate { get; set; }
        public bool IsActive => TransactionCount > 0;
    }

    /// <summary>
    /// Account aging analysis
    /// </summary>
    [Description("Account aging analysis")]
    public class AccountAgingAnalysis
    {
        public string AccountCode { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public DateOnly AsOfDate { get; set; }
        public decimal Current { get; set; }
        public decimal Days30 { get; set; }
        public decimal Days60 { get; set; }
        public decimal Days90 { get; set; }
        public decimal Days120Plus { get; set; }
        public decimal TotalBalance { get; set; }
        public decimal PercentCurrent => TotalBalance != 0 ? (Current / TotalBalance) * 100 : 0;
        public decimal PercentOverdue => TotalBalance != 0 ? ((Days30 + Days60 + Days90 + Days120Plus) / TotalBalance) * 100 : 0;
    }

    /// <summary>
    /// Transaction volume analytics
    /// </summary>
    [Description("Transaction volume analytics")]
    public class TransactionVolumeAnalytics
    {
        public DateOnly PeriodStart { get; set; }
        public DateOnly PeriodEnd { get; set; }
        public int TotalTransactions { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AverageTransactionSize { get; set; }
        public decimal PeakDailyVolume { get; set; }
        public List<DailyTransactionVolume> DailyVolumes { get; set; } = new();
        public List<TransactionTypeVolume> VolumesByType { get; set; } = new();
    }

    /// <summary>
    /// Daily transaction volume
    /// </summary>
    [Description("Daily transaction volume")]
    public class DailyTransactionVolume
    {
        public DateOnly Date { get; set; }
        public int TransactionCount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AverageAmount { get; set; }
    }

    /// <summary>
    /// Transaction volume by type
    /// </summary>
    [Description("Transaction volume by type")]
    public class TransactionTypeVolume
    {
        public string TransactionType { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AverageAmount { get; set; }
        public decimal Percentage { get; set; }
    }

    /// <summary>
    /// Account reconciliation results
    /// </summary>
    [Description("Account reconciliation")]
    public class AccountReconciliation
    {
        public string AccountCode { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public DateOnly PeriodStart { get; set; }
        public DateOnly PeriodEnd { get; set; }
        public DateTime? PerformedAt { get; set; }
        public string PerformedBy { get; set; } = string.Empty;
        public decimal BeginningBalance { get; set; }
        public decimal EndingBalance { get; set; }
        public decimal TotalDebits { get; set; }
        public decimal TotalCredits { get; set; }
        public decimal NetActivity { get; set; }
        public decimal ReconciliationDifference { get; set; }
        public bool IsReconciled { get; set; }
        public List<ReconciliationItem> UnreconciledItems { get; set; } = new();
        public string Notes { get; set; } = string.Empty;
    }

    /// <summary>
    /// Unreconciled item in account reconciliation
    /// </summary>
    [Description("Reconciliation item")]
    public class ReconciliationItem
    {
        public DateOnly TransactionDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Reference { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Comments { get; set; } = string.Empty;
    }

    /// <summary>
    /// Financial ratio analysis
    /// </summary>
    [Description("Financial ratio analysis")]
    public class FinancialRatioAnalysis
    {
        public DateOnly AnalysisDate { get; set; }
        public LiquidityRatios Liquidity { get; set; } = new();
        public ProfitabilityRatios Profitability { get; set; } = new();
        public EfficiencyRatios Efficiency { get; set; } = new();
        public LeverageRatios Leverage { get; set; } = new();
    }

    /// <summary>
    /// Liquidity ratios
    /// </summary>
    [Description("Liquidity ratios")]
    public class LiquidityRatios
    {
        public decimal CurrentRatio { get; set; }
        public decimal QuickRatio { get; set; }
        public decimal CashRatio { get; set; }
        public decimal WorkingCapital { get; set; }
    }

    /// <summary>
    /// Profitability ratios
    /// </summary>
    [Description("Profitability ratios")]
    public class ProfitabilityRatios
    {
        public decimal GrossProfitMargin { get; set; }
        public decimal NetProfitMargin { get; set; }
        public decimal ReturnOnAssets { get; set; }
        public decimal ReturnOnEquity { get; set; }
    }

    /// <summary>
    /// Efficiency ratios
    /// </summary>
    [Description("Efficiency ratios")]
    public class EfficiencyRatios
    {
        public decimal AssetTurnover { get; set; }
        public decimal InventoryTurnover { get; set; }
        public decimal ReceivablesTurnover { get; set; }
        public decimal PayablesTurnover { get; set; }
    }

    /// <summary>
    /// Leverage ratios
    /// </summary>
    [Description("Leverage ratios")]
    public class LeverageRatios
    {
        public decimal DebtToEquity { get; set; }
        public decimal DebtToAssets { get; set; }
        public decimal EquityMultiplier { get; set; }
        public decimal InterestCoverage { get; set; }
    }
}
