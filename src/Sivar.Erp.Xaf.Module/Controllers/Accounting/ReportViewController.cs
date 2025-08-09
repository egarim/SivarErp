using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.Persistent.Base;
using DevExpress.Data.Filtering;
using Microsoft.Extensions.Logging;
using Sivar.Erp.Services.Accounting.FiscalPeriods;
using Sivar.Erp.Modules.Accounting;
using Sivar.Erp.Modules.Accounting.JournalEntries;
using Sivar.Erp.Modules.Accounting.Reports;
using Sivar.Erp.Xaf.Module.BusinessObjects.Accounting;
using Sivar.Erp.Services.Accounting.ChartOfAccounts;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Sivar.Erp.Xaf.Module.Controllers.Accounting
{
    /// <summary>
    /// Controller for financial reporting and accounting analysis operations
    /// </summary>
    [DefaultClassOptions]
    public class ReportViewController : ViewController, IModelExtender
    {
        private readonly IAccountingModule _accountingModule;
        private readonly IJournalEntryReportService _reportService;
        private readonly IFiscalPeriodService _fiscalPeriodService;
        private readonly ILogger<ReportViewController> _logger;

        // Actions for report operations
        private ParametrizedAction _generateTrialBalanceAction;
        private SingleChoiceAction _generateJournalReportAction;
        private ParametrizedAction _generateAccountBalanceAction;
        private SimpleAction _generateGeneralLedgerAction;
        private SingleChoiceAction _generateFiscalPeriodReportAction;
        private SimpleAction _exportAccountingDataAction;
        private SimpleAction _validateAccountingEquationAction;

        public ReportViewController()
        {
            TargetObjectType = typeof(Account); // Primary target, but can work with multiple types
            TargetViewType = ViewType.Any;

            // Initialize actions
            InitializeActions();

            // Try to get services from DI container if available
            try
            {
                _accountingModule = Application?.ServiceProvider?.GetService(typeof(IAccountingModule)) as IAccountingModule;
                _reportService = Application?.ServiceProvider?.GetService(typeof(IJournalEntryReportService)) as IJournalEntryReportService;
                _fiscalPeriodService = Application?.ServiceProvider?.GetService(typeof(IFiscalPeriodService)) as IFiscalPeriodService;
                _logger = Application?.ServiceProvider?.GetService(typeof(ILogger<ReportViewController>)) as ILogger<ReportViewController>;
            }
            catch
            {
                // Services not available, will handle gracefully
            }
        }

        private void InitializeActions()
        {
            // Generate Trial Balance Action - Use DateTime instead of DateOnly
            _generateTrialBalanceAction = new ParametrizedAction(this, "GenerateTrialBalance", PredefinedCategory.Reports, typeof(DateTime))
            {
                Caption = "Trial Balance",
                ToolTip = "Generate trial balance report as of a specific date",
                ImageName = "Report",
                SelectionDependencyType = SelectionDependencyType.RequireSingleObject
            };
            _generateTrialBalanceAction.Execute += GenerateTrialBalanceAction_Execute;

            // Generate Journal Report Action - Use SingleChoiceAction for better UX
            _generateJournalReportAction = new SingleChoiceAction(this, "GenerateJournalReport", PredefinedCategory.Reports)
            {
                Caption = "Journal Report",
                ToolTip = "Generate journal entries report for a date range",
                ImageName = "Report_JournalEntries",
                SelectionDependencyType = SelectionDependencyType.RequireSingleObject,
                ItemType = SingleChoiceActionItemType.ItemIsOperation
            };
            _generateJournalReportAction.Execute += GenerateJournalReportAction_Execute;

            // Generate Account Balance Action
            _generateAccountBalanceAction = new ParametrizedAction(this, "GenerateAccountBalance", PredefinedCategory.Reports, typeof(string))
            {
                Caption = "Account Balance",
                ToolTip = "Generate balance report for specific account",
                ImageName = "Report_AccountBalance",
                SelectionDependencyType = SelectionDependencyType.RequireSingleObject
            };
            _generateAccountBalanceAction.Execute += GenerateAccountBalanceAction_Execute;

            // Generate General Ledger Action
            _generateGeneralLedgerAction = new SimpleAction(this, "GenerateGeneralLedger", PredefinedCategory.Reports)
            {
                Caption = "General Ledger",
                ToolTip = "Generate complete general ledger report",
                ImageName = "Report_GeneralLedger",
                SelectionDependencyType = SelectionDependencyType.RequireSingleObject
            };
            _generateGeneralLedgerAction.Execute += GenerateGeneralLedgerAction_Execute;

            // Generate Fiscal Period Report Action
            _generateFiscalPeriodReportAction = new SingleChoiceAction(this, "GenerateFiscalPeriodReport", PredefinedCategory.Reports)
            {
                Caption = "Period Report",
                ToolTip = "Generate financial report for a specific fiscal period",
                ImageName = "Report_FiscalPeriod",
                SelectionDependencyType = SelectionDependencyType.RequireSingleObject,
                ItemType = SingleChoiceActionItemType.ItemIsOperation
            };
            _generateFiscalPeriodReportAction.Execute += GenerateFiscalPeriodReportAction_Execute;

            // Export Accounting Data Action
            _exportAccountingDataAction = new SimpleAction(this, "ExportAccountingData", PredefinedCategory.Tools)
            {
                Caption = "Export Data",
                ToolTip = "Export accounting data for external analysis",
                ImageName = "Export",
                SelectionDependencyType = SelectionDependencyType.RequireSingleObject
            };
            _exportAccountingDataAction.Execute += ExportAccountingDataAction_Execute;

            // Validate Accounting Equation Action
            _validateAccountingEquationAction = new SimpleAction(this, "ValidateAccountingEquation", PredefinedCategory.Tools)
            {
                Caption = "Validate Equation",
                ToolTip = "Validate that Assets + Expenses = Liabilities + Equity + Revenue",
                ImageName = "CheckSpelling",
                SelectionDependencyType = SelectionDependencyType.RequireSingleObject
            };
            _validateAccountingEquationAction.Execute += ValidateAccountingEquationAction_Execute;
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            UpdateActionStates();
            SetupParametrizedActions();
        }

        private void UpdateActionStates()
        {
            bool servicesAvailable = _accountingModule != null && _reportService != null;
            
            _generateTrialBalanceAction.Enabled["Services"] = servicesAvailable;
            _generateJournalReportAction.Enabled["Services"] = servicesAvailable;
            _generateAccountBalanceAction.Enabled["Services"] = servicesAvailable;
            _generateGeneralLedgerAction.Enabled["Services"] = servicesAvailable;
            _generateFiscalPeriodReportAction.Enabled["Services"] = servicesAvailable;
            _exportAccountingDataAction.Enabled["Services"] = servicesAvailable;
            _validateAccountingEquationAction.Enabled["Services"] = servicesAvailable;

            // Account-specific actions
            var account = View.CurrentObject as Account;
            _generateAccountBalanceAction.Enabled["AccountSelected"] = account != null;
        }

        private void SetupParametrizedActions()
        {
            // Setup trial balance with current date (DateTime instead of DateOnly)
            _generateTrialBalanceAction.Value = DateTime.Today;

            // Setup journal report choices
            SetupJournalReportChoices();

            // Setup fiscal period report with available periods
            if (_fiscalPeriodService != null)
            {
                SetupFiscalPeriodLookupAsync();
            }
        }

        private void SetupJournalReportChoices()
        {
            _generateJournalReportAction.Items.Clear();
            
            _generateJournalReportAction.Items.Add(new ChoiceActionItem
            {
                Id = "current_month",
                Caption = "Current Month",
                Data = "current_month",
                ToolTip = "Show journal entries for the current month"
            });
            
            _generateJournalReportAction.Items.Add(new ChoiceActionItem
            {
                Id = "last_month",
                Caption = "Last Month",
                Data = "last_month",
                ToolTip = "Show journal entries for the previous month"
            });
            
            _generateJournalReportAction.Items.Add(new ChoiceActionItem
            {
                Id = "current_year",
                Caption = "Current Year",
                Data = "current_year",
                ToolTip = "Show journal entries for the current year"
            });
        }

        private async void SetupFiscalPeriodLookupAsync()
        {
            try
            {
                var openPeriods = await _fiscalPeriodService.GetFiscalPeriodsByStatusAsync(FiscalPeriodStatus.Open);
                var closedPeriods = await _fiscalPeriodService.GetFiscalPeriodsByStatusAsync(FiscalPeriodStatus.Closed);
                
                var allPeriods = openPeriods.Concat(closedPeriods).ToList();
                
                if (allPeriods.Any())
                {
                    // Clear existing items
                    _generateFiscalPeriodReportAction.Items.Clear();
                    
                    // Add fiscal periods as choice items
                    foreach (var period in allPeriods.OrderBy(p => p.StartDate))
                    {
                        var choiceItem = new ChoiceActionItem
                        {
                            Id = period.Code,
                            Caption = $"{period.Name} ({period.Code})",
                            Data = period.Code,
                            ToolTip = $"Period: {period.StartDate:yyyy-MM-dd} to {period.EndDate:yyyy-MM-dd} - Status: {period.Status}"
                        };
                        _generateFiscalPeriodReportAction.Items.Add(choiceItem);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Could not setup fiscal period lookup");
            }
        }

        private async void GenerateTrialBalanceAction_Execute(object sender, ParametrizedActionExecuteEventArgs e)
        {
            if (_accountingModule == null) return;

            // Convert DateTime to DateOnly
            var dateTimeValue = (DateTime)(e.ParameterCurrentValue ?? DateTime.Today);
            var asOfDate = DateOnly.FromDateTime(dateTimeValue);

            try
            {
                var allAccounts = ObjectSpace.GetObjectsQuery<Account>().ToList();
                var trialBalanceData = new List<(string AccountCode, string AccountName, decimal DebitBalance, decimal CreditBalance)>();

                foreach (var account in allAccounts)
                {
                    var balance = await _accountingModule.GetAccountBalanceAsync(account.OfficialCode, asOfDate);
                    
                    if (balance != 0)
                    {
                        if (balance > 0)
                        {
                            // Debit balance
                            trialBalanceData.Add((account.OfficialCode, account.AccountName, balance, 0));
                        }
                        else
                        {
                            // Credit balance
                            trialBalanceData.Add((account.OfficialCode, account.AccountName, 0, Math.Abs(balance)));
                        }
                    }
                }

                var totalDebits = trialBalanceData.Sum(x => x.DebitBalance);
                var totalCredits = trialBalanceData.Sum(x => x.CreditBalance);
                var isBalanced = Math.Abs(totalDebits - totalCredits) < 0.01m;

                var reportText = $"TRIAL BALANCE\nAs of {asOfDate:yyyy-MM-dd}\n\n" +
                               $"Account Code\t\tAccount Name\t\tDebit\t\tCredit\n" +
                               new string('-', 80) + "\n";

                foreach (var item in trialBalanceData.OrderBy(x => x.AccountCode))
                {
                    reportText += $"{item.AccountCode,-15}\t{item.AccountName,-25}\t{item.DebitBalance:C}\t\t{item.CreditBalance:C}\n";
                }

                reportText += new string('-', 80) + "\n" +
                             $"TOTALS:\t\t\t\t\t{totalDebits:C}\t\t{totalCredits:C}\n" +
                             $"Status: {(isBalanced ? "BALANCED" : "OUT OF BALANCE")}\n";

                Application.ShowViewStrategy.ShowMessage(reportText, InformationType.Info);

                _logger?.LogInformation("Generated trial balance report as of {AsOfDate}", asOfDate);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating trial balance report");
                Application.ShowViewStrategy.ShowMessage(
                    $"Error generating trial balance: {ex.Message}",
                    InformationType.Error);
            }
        }

        private async void GenerateJournalReportAction_Execute(object sender, SingleChoiceActionExecuteEventArgs e)
        {
            if (_reportService == null) return;

            var dateRange = e.SelectedChoiceActionItem?.Data as string ?? "current_month";

            try
            {
                var (startDate, endDate) = ParseDateRange(dateRange);
                
                var queryOptions = new JournalEntryQueryOptions
                {
                    FromDate = startDate,
                    ToDate = endDate,
                    OnlyPosted = true
                };

                var journalReport = await _reportService.GenerateJournalEntryReportAsync(queryOptions);

                var reportText = $"JOURNAL ENTRIES REPORT\n" +
                               $"From {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}\n\n" +
                               $"Total Entries: {journalReport.TotalEntries}\n" +
                               $"Total Debits: {journalReport.TotalDebits:C}\n" +
                               $"Total Credits: {journalReport.TotalCredits:C}\n" +
                               $"Period Balance: {(journalReport.IsBalanced ? "Balanced" : "Unbalanced")}\n\n";

                if (journalReport.Entries.Any())
                {
                    reportText += "JOURNAL ENTRIES (First 10):\n";
                    foreach (var entry in journalReport.Entries.Take(10)) // Limit for display
                    {
                        reportText += $"  {entry.LedgerEntryNumber}: {entry.OfficialCode} - {entry.EntryType} - {entry.Amount:C}\n";
                    }
                }

                Application.ShowViewStrategy.ShowMessage(reportText, InformationType.Info);

                _logger?.LogInformation("Generated journal report for period {StartDate} to {EndDate}", startDate, endDate);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating journal report");
                Application.ShowViewStrategy.ShowMessage(
                    $"Error generating journal report: {ex.Message}",
                    InformationType.Error);
            }
        }

        private async void GenerateAccountBalanceAction_Execute(object sender, ParametrizedActionExecuteEventArgs e)
        {
            var account = View.CurrentObject as Account;
            if (account == null || _accountingModule == null) return;

            var asOfDate = DateOnly.FromDateTime(DateTime.Today);

            try
            {
                var balance = await _accountingModule.GetAccountBalanceAsync(account.OfficialCode, asOfDate);

                // Get recent entries for this account
                var accountEntries = ObjectSpace.GetObjectsQuery<LedgerEntry>()
                    .Where(le => le.OfficialCode == account.OfficialCode)
                    .OrderByDescending(le => le.CreatedOn)
                    .Take(10)
                    .ToList();

                var reportText = $"ACCOUNT BALANCE REPORT\n" +
                               $"Account: {account.OfficialCode} - {account.AccountName}\n" +
                               $"Type: {account.AccountType}\n" +
                               $"Current Balance: {balance:C} as of {asOfDate:yyyy-MM-dd}\n\n";

                if (accountEntries.Any())
                {
                    reportText += "RECENT TRANSACTIONS:\n";
                    foreach (var entry in accountEntries)
                    {
                        reportText += $"  {entry.CreatedOn:yyyy-MM-dd} - {entry.EntryType} - {entry.Amount:C}\n";
                    }
                }
                else
                {
                    reportText += "No transactions found for this account.\n";
                }

                Application.ShowViewStrategy.ShowMessage(reportText, InformationType.Info);

                _logger?.LogInformation("Generated account balance report for {AccountCode}", account.OfficialCode);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating account balance report for {AccountCode}", account.OfficialCode);
                Application.ShowViewStrategy.ShowMessage(
                    $"Error generating account balance report: {ex.Message}",
                    InformationType.Error);
            }
        }

        private async void GenerateGeneralLedgerAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            if (_accountingModule == null) return;

            try
            {
                var currentDate = DateOnly.FromDateTime(DateTime.Today);
                var allAccounts = ObjectSpace.GetObjectsQuery<Account>().OrderBy(a => a.OfficialCode).ToList();
                var activeAccounts = new List<(Account Account, decimal Balance)>();

                // Get balances for all accounts
                foreach (var account in allAccounts)
                {
                    var balance = await _accountingModule.GetAccountBalanceAsync(account.OfficialCode, currentDate);
                    if (balance != 0)
                    {
                        activeAccounts.Add((account, balance));
                    }
                }

                var reportText = $"GENERAL LEDGER SUMMARY\n" +
                               $"As of {currentDate:yyyy-MM-dd}\n\n" +
                               $"Active Accounts: {activeAccounts.Count}\n" +
                               $"Total Accounts: {allAccounts.Count}\n\n";

                reportText += "ACCOUNT BALANCES:\n";
                reportText += "Code\t\tName\t\t\t\tType\t\tBalance\n";
                reportText += new string('-', 80) + "\n";

                var totalAssets = 0m;
                var totalLiabilities = 0m;
                var totalEquity = 0m;
                var totalRevenue = 0m;
                var totalExpenses = 0m;

                foreach (var (account, balance) in activeAccounts)
                {
                    reportText += $"{account.OfficialCode,-12}\t{account.AccountName,-25}\t{account.AccountType,-15}\t{balance:C}\n";
                    
                    // Categorize by account type
                    switch (account.AccountType)
                    {
                        case AccountType.Asset:
                            totalAssets += balance;
                            break;
                        case AccountType.Liability:
                            totalLiabilities += Math.Abs(balance);
                            break;
                        case AccountType.Equity:
                            totalEquity += Math.Abs(balance);
                            break;
                        case AccountType.Revenue:
                            totalRevenue += Math.Abs(balance);
                            break;
                        case AccountType.Expense:
                            totalExpenses += balance;
                            break;
                    }
                }

                reportText += new string('-', 80) + "\n";
                reportText += "SUMMARY BY TYPE:\n";
                reportText += $"Assets: {totalAssets:C}\n";
                reportText += $"Liabilities: {totalLiabilities:C}\n";
                reportText += $"Equity: {totalEquity:C}\n";
                reportText += $"Revenue: {totalRevenue:C}\n";
                reportText += $"Expenses: {totalExpenses:C}\n\n";
                
                var netWorth = totalAssets - totalLiabilities;
                reportText += $"Net Worth: {netWorth:C}\n";

                Application.ShowViewStrategy.ShowMessage(reportText, InformationType.Info);

                _logger?.LogInformation("Generated general ledger report with {AccountCount} active accounts", activeAccounts.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating general ledger report");
                Application.ShowViewStrategy.ShowMessage(
                    $"Error generating general ledger report: {ex.Message}",
                    InformationType.Error);
            }
        }

        private async void GenerateFiscalPeriodReportAction_Execute(object sender, SingleChoiceActionExecuteEventArgs e)
        {
            var periodCode = e.SelectedChoiceActionItem?.Data as string;
            if (string.IsNullOrWhiteSpace(periodCode) || _reportService == null) return;

            try
            {
                // Find the fiscal period by code
                var fiscalPeriod = ObjectSpace.GetObjectsQuery<FiscalPeriod>()
                    .FirstOrDefault(fp => fp.Code == periodCode);
                
                if (fiscalPeriod == null)
                {
                    Application.ShowViewStrategy.ShowMessage(
                        $"Fiscal period with code '{periodCode}' not found.",
                        InformationType.Warning);
                    return;
                }

                var queryOptions = new JournalEntryQueryOptions
                {
                    FromDate = fiscalPeriod.StartDate,
                    ToDate = fiscalPeriod.EndDate,
                    OnlyPosted = true
                };

                var journalReport = await _reportService.GenerateJournalEntryReportAsync(queryOptions);

                var reportText = $"FISCAL PERIOD REPORT\n" +
                               $"Period: {fiscalPeriod.Name} ({fiscalPeriod.Code})\n" +
                               $"From {fiscalPeriod.StartDate:yyyy-MM-dd} to {fiscalPeriod.EndDate:yyyy-MM-dd}\n" +
                               $"Status: {fiscalPeriod.Status}\n\n" +
                               $"TRANSACTION SUMMARY:\n" +
                               $"Total Journal Entries: {journalReport.TotalEntries}\n" +
                               $"Total Debits: {journalReport.TotalDebits:C}\n" +
                               $"Total Credits: {journalReport.TotalCredits:C}\n" +
                               $"Period Balance: {(journalReport.IsBalanced ? "Balanced" : "Unbalanced")}\n\n";

                if (journalReport.Entries.Any())
                {
                    reportText += "SAMPLE ENTRIES:\n";
                    foreach (var entry in journalReport.Entries.Take(5))
                    {
                        reportText += $"  {entry.LedgerEntryNumber}: {entry.OfficialCode} - {entry.EntryType} - {entry.Amount:C}\n";
                    }
                }

                Application.ShowViewStrategy.ShowMessage(reportText, InformationType.Info);

                _logger?.LogInformation("Generated fiscal period report for {PeriodCode}", periodCode);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating fiscal period report for {PeriodCode}", periodCode);
                Application.ShowViewStrategy.ShowMessage(
                    $"Error generating fiscal period report: {ex.Message}",
                    InformationType.Error);
            }
        }

        private void ExportAccountingDataAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            try
            {
                var allAccounts = ObjectSpace.GetObjectsQuery<Account>().Count();
                var allTransactions = ObjectSpace.GetObjectsQuery<Transaction>().Count();
                var allLedgerEntries = ObjectSpace.GetObjectsQuery<LedgerEntry>().Count();
                var allFiscalPeriods = ObjectSpace.GetObjectsQuery<FiscalPeriod>().Count();

                var exportSummary = $"ACCOUNTING DATA EXPORT SUMMARY\n\n" +
                                  $"Chart of Accounts: {allAccounts} accounts\n" +
                                  $"Transactions: {allTransactions} transactions\n" +
                                  $"Ledger Entries: {allLedgerEntries} entries\n" +
                                  $"Fiscal Periods: {allFiscalPeriods} periods\n\n" +
                                  $"Export completed at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n" +
                                  $"Note: This is a summary. Use external tools for detailed data export.";

                Application.ShowViewStrategy.ShowMessage(exportSummary, InformationType.Success);

                _logger?.LogInformation("Exported accounting data summary");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error exporting accounting data");
                Application.ShowViewStrategy.ShowMessage(
                    $"Error exporting accounting data: {ex.Message}",
                    InformationType.Error);
            }
        }

        private async void ValidateAccountingEquationAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            if (_accountingModule == null) return;

            try
            {
                var currentDate = DateOnly.FromDateTime(DateTime.Today);
                var allAccounts = ObjectSpace.GetObjectsQuery<Account>().ToList();

                var totalAssets = 0m;
                var totalLiabilities = 0m;
                var totalEquity = 0m;
                var totalRevenue = 0m;
                var totalExpenses = 0m;

                foreach (var account in allAccounts)
                {
                    var balance = await _accountingModule.GetAccountBalanceAsync(account.OfficialCode, currentDate);
                    
                    switch (account.AccountType)
                    {
                        case AccountType.Asset:
                            totalAssets += balance;
                            break;
                        case AccountType.Liability:
                            totalLiabilities += Math.Abs(balance); // Convert to positive
                            break;
                        case AccountType.Equity:
                            totalEquity += Math.Abs(balance); // Convert to positive
                            break;
                        case AccountType.Revenue:
                            totalRevenue += Math.Abs(balance); // Convert to positive
                            break;
                        case AccountType.Expense:
                            totalExpenses += balance;
                            break;
                    }
                }

                // Basic accounting equation: Assets = Liabilities + Equity
                var leftSide = totalAssets + totalExpenses;
                var rightSide = totalLiabilities + totalEquity + totalRevenue;
                var difference = leftSide - rightSide;
                var isBalanced = Math.Abs(difference) < 0.01m;

                var validationReport = $"ACCOUNTING EQUATION VALIDATION\n" +
                                     $"As of {currentDate:yyyy-MM-dd}\n\n" +
                                     $"LEFT SIDE (Assets + Expenses):\n" +
                                     $"  Assets: {totalAssets:C}\n" +
                                     $"  Expenses: {totalExpenses:C}\n" +
                                     $"  Total: {leftSide:C}\n\n" +
                                     $"RIGHT SIDE (Liabilities + Equity + Revenue):\n" +
                                     $"  Liabilities: {totalLiabilities:C}\n" +
                                     $"  Equity: {totalEquity:C}\n" +
                                     $"  Revenue: {totalRevenue:C}\n" +
                                     $"  Total: {rightSide:C}\n\n" +
                                     $"DIFFERENCE: {difference:C}\n" +
                                     $"STATUS: {(isBalanced ? "BALANCED ✓" : "OUT OF BALANCE ✗")}\n";

                var messageType = isBalanced ? InformationType.Success : InformationType.Warning;
                Application.ShowViewStrategy.ShowMessage(validationReport, messageType);

                _logger?.LogInformation("Validated accounting equation. Balanced: {IsBalanced}, Difference: {Difference}", 
                    isBalanced, difference);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error validating accounting equation");
                Application.ShowViewStrategy.ShowMessage(
                    $"Error validating accounting equation: {ex.Message}",
                    InformationType.Error);
            }
        }

        private (DateOnly StartDate, DateOnly EndDate) ParseDateRange(string dateRange)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            
            switch (dateRange.ToLower())
            {
                case "current_month":
                    return (new DateOnly(today.Year, today.Month, 1), today);
                case "current_year":
                    return (new DateOnly(today.Year, 1, 1), today);
                case "last_month":
                    var lastMonth = today.AddMonths(-1);
                    var start = new DateOnly(lastMonth.Year, lastMonth.Month, 1);
                    var end = start.AddMonths(1).AddDays(-1);
                    return (start, end);
                default:
                    return (today.AddMonths(-1), today);
            }
        }

        public void ExtendModelInterfaces(ModelInterfaceExtenders extenders)
        {
            // Extend model if needed for custom properties
        }
    }
}