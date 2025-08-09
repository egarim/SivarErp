using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.Persistent.Base;
using DevExpress.Data.Filtering;
using Microsoft.Extensions.Logging;
using Sivar.Erp.Services.Accounting.ChartOfAccounts;
using Sivar.Erp.Services.Accounting.BalanceCalculators;
using Sivar.Erp.Xaf.Module.BusinessObjects.Accounting;
using System;
using System.Linq;

namespace Sivar.Erp.Xaf.Module.Controllers.Accounting
{
    /// <summary>
    /// Controller for account management operations and chart of accounts functionality
    /// </summary>
    [DefaultClassOptions]
    public class AccountingViewController : ViewController, IModelExtender
    {
        private readonly IAccountBalanceCalculator _accountBalanceCalculator;
        private readonly ILogger<AccountingViewController> _logger;

        // Actions for account operations
        private SimpleAction _calculateBalanceAction;
        private SimpleAction _viewChildAccountsAction;
        private SimpleAction _viewTransactionsAction;
        private SimpleAction _validateAccountStructureAction;
        private SimpleAction _refreshBalancesAction;
        private SingleChoiceAction _moveAccountAction;

        public AccountingViewController()
        {
            TargetObjectType = typeof(Account);
            TargetViewType = ViewType.Any;

            // Initialize actions
            InitializeActions();

            // Try to get services from DI container if available
            try
            {
                _accountBalanceCalculator = Application?.ServiceProvider?.GetService(typeof(IAccountBalanceCalculator)) as IAccountBalanceCalculator;
                _logger = Application?.ServiceProvider?.GetService(typeof(ILogger<AccountingViewController>)) as ILogger<AccountingViewController>;
            }
            catch
            {
                // Services not available, will handle gracefully
            }
        }

        private void InitializeActions()
        {
            // Calculate Balance Action
            _calculateBalanceAction = new SimpleAction(this, "CalculateAccountBalance", PredefinedCategory.View)
            {
                Caption = "Calculate Balance",
                ToolTip = "Calculate current balance for selected account",
                ImageName = "CalculateNow",
                SelectionDependencyType = SelectionDependencyType.RequireSingleObject
            };
            _calculateBalanceAction.Execute += CalculateBalanceAction_Execute;

            // View Child Accounts Action
            _viewChildAccountsAction = new SimpleAction(this, "ViewChildAccounts", PredefinedCategory.View)
            {
                Caption = "View Child Accounts",
                ToolTip = "Show all child accounts for this account",
                ImageName = "TreeView",
                SelectionDependencyType = SelectionDependencyType.RequireSingleObject
            };
            _viewChildAccountsAction.Execute += ViewChildAccountsAction_Execute;

            // View Transactions Action
            _viewTransactionsAction = new SimpleAction(this, "ViewAccountTransactions", PredefinedCategory.View)
            {
                Caption = "View Transactions",
                ToolTip = "Show all transactions for this account",
                ImageName = "Action_Report_Object_Inplaces_Transaction",
                SelectionDependencyType = SelectionDependencyType.RequireSingleObject
            };
            _viewTransactionsAction.Execute += ViewTransactionsAction_Execute;

            // Validate Account Structure Action
            _validateAccountStructureAction = new SimpleAction(this, "ValidateAccountStructure", PredefinedCategory.Tools)
            {
                Caption = "Validate Structure",
                ToolTip = "Validate the chart of accounts structure",
                ImageName = "CheckSpelling",
                SelectionDependencyType = SelectionDependencyType.RequireMultipleObjects
            };
            _validateAccountStructureAction.Execute += ValidateAccountStructureAction_Execute;

            // Refresh Balances Action
            _refreshBalancesAction = new SimpleAction(this, "RefreshAccountBalances", PredefinedCategory.Tools)
            {
                Caption = "Refresh Balances",
                ToolTip = "Refresh balance calculations for all accounts",
                ImageName = "Refresh",
                SelectionDependencyType = SelectionDependencyType.RequireMultipleObjects
            };
            _refreshBalancesAction.Execute += RefreshBalancesAction_Execute;

            // Move Account Action
            _moveAccountAction = new SingleChoiceAction(this, "MoveAccount", PredefinedCategory.Edit)
            {
                Caption = "Move to Parent",
                ToolTip = "Move account to a different parent account",
                ImageName = "MoveItem",
                SelectionDependencyType = SelectionDependencyType.RequireSingleObject,
                ItemType = SingleChoiceActionItemType.ItemIsOperation
            };
            _moveAccountAction.Execute += MoveAccountAction_Execute;
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            UpdateActionStates();
            
            // Subscribe to object space events
            ObjectSpace.ObjectChanged += ObjectSpace_ObjectChanged;
        }

        protected override void OnDeactivated()
        {
            ObjectSpace.ObjectChanged -= ObjectSpace_ObjectChanged;
            base.OnDeactivated();
        }

        private void ObjectSpace_ObjectChanged(object sender, ObjectChangedEventArgs e)
        {
            if (e.Object is Account)
            {
                UpdateActionStates();
            }
        }

        private void UpdateActionStates()
        {
            var account = View.CurrentObject as Account;
            
            if (View is ListView)
            {
                // List view - enable actions that don't require selection
                _validateAccountStructureAction.Enabled["General"] = true;
                _refreshBalancesAction.Enabled["General"] = true;
                
                // Disable single-object actions
                _calculateBalanceAction.Enabled["General"] = false;
                _viewChildAccountsAction.Enabled["General"] = false;
                _viewTransactionsAction.Enabled["General"] = false;
                _moveAccountAction.Enabled["General"] = false;
            }
            else if (account != null)
            {
                // Detail view with account selected
                _calculateBalanceAction.Enabled["General"] = true;
                _viewChildAccountsAction.Enabled["General"] = true;
                _viewTransactionsAction.Enabled["General"] = true;
                _moveAccountAction.Enabled["General"] = !string.IsNullOrEmpty(account.ParentOfficialCode);
                _validateAccountStructureAction.Enabled["General"] = true;
                _refreshBalancesAction.Enabled["General"] = true;

                // Setup move account lookup
                SetupMoveAccountLookup();
            }
            else
            {
                SetAllActionsEnabled(false);
            }
        }

        private void SetAllActionsEnabled(bool enabled)
        {
            _calculateBalanceAction.Enabled.SetItemValue("General", enabled);
            _viewChildAccountsAction.Enabled.SetItemValue("General", enabled);
            _viewTransactionsAction.Enabled.SetItemValue("General", enabled);
            _validateAccountStructureAction.Enabled.SetItemValue("General", enabled);
            _refreshBalancesAction.Enabled.SetItemValue("General", enabled);
            _moveAccountAction.Enabled.SetItemValue("General", enabled);
        }

        private void SetupMoveAccountLookup()
        {
            var account = View.CurrentObject as Account;
            if (account == null) return;

            try
            {
                // Clear existing items
                _moveAccountAction.Items.Clear();

                // Add "Move to Root" option
                var rootItem = new ChoiceActionItem
                {
                    Id = "Root",
                    Caption = "Move to Root Level",
                    Data = null, // null means root level
                    ToolTip = "Move to the root level (no parent)"
                };
                _moveAccountAction.Items.Add(rootItem);

                // Get all accounts except current account and its descendants
                var allAccounts = ObjectSpace.GetObjectsQuery<Account>()
                    .Where(a => a.OfficialCode != account.OfficialCode)
                    .ToList();

                // Filter out descendants to prevent circular references
                var availableParents = allAccounts.Where(a => !IsDescendantOf(a, account)).ToList();

                // Add available parent accounts as choice items
                foreach (var parentAccount in availableParents.OrderBy(a => a.OfficialCode))
                {
                    var choiceItem = new ChoiceActionItem
                    {
                        Id = parentAccount.OfficialCode,
                        Caption = $"{parentAccount.OfficialCode} - {parentAccount.AccountName}",
                        Data = parentAccount.OfficialCode,
                        ToolTip = $"Move to parent: {parentAccount.AccountName} ({parentAccount.AccountType})"
                    };
                    _moveAccountAction.Items.Add(choiceItem);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Could not setup move account lookup for {AccountCode}", account.OfficialCode);
            }
        }

        private void CalculateBalanceAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var account = e.CurrentObject as Account;
            if (account == null || _accountBalanceCalculator == null) return;

            try
            {
                var currentDate = DateOnly.FromDateTime(DateTime.Today);
                var balance = _accountBalanceCalculator.CalculateAccountBalance(account.OfficialCode, currentDate);

                Application.ShowViewStrategy.ShowMessage(
                    $"Account {account.OfficialCode} - {account.AccountName}\n" +
                    $"Current Balance: {balance:C}\n" +
                    $"As of: {currentDate:yyyy-MM-dd}",
                    InformationType.Info);

                _logger?.LogInformation("Calculated balance for account {AccountCode}: {Balance}", 
                    account.OfficialCode, balance);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error calculating balance for account {AccountCode}", account.OfficialCode);
                Application.ShowViewStrategy.ShowMessage(
                    $"Error calculating balance: {ex.Message}",
                    InformationType.Error);
            }
        }

        private void ViewChildAccountsAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var account = e.CurrentObject as Account;
            if (account == null) return;

            try
            {
                var childAccounts = ObjectSpace.GetObjectsQuery<Account>()
                    .Where(a => a.ParentOfficialCode == account.OfficialCode)
                    .ToList();

                if (childAccounts.Any())
                {
                    var listView = Application.CreateListView(typeof(Account), true);
                    listView.CollectionSource.Criteria["Filter"] = CriteriaOperator.Parse("[ParentOfficialCode] = ?", account.OfficialCode);
                    
                    var showViewParameters = new ShowViewParameters(listView)
                    {
                        Context = TemplateContext.PopupWindow,
                        TargetWindow = TargetWindow.NewModalWindow
                    };
                    
                    Application.ShowViewStrategy.ShowView(showViewParameters, new ShowViewSource(null, null));
                }
                else
                {
                    Application.ShowViewStrategy.ShowMessage(
                        $"Account '{account.AccountName}' has no child accounts.",
                        InformationType.Info);
                }

                _logger?.LogInformation("Viewed child accounts for account {AccountCode}", account.OfficialCode);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error viewing child accounts for account {AccountCode}", account.OfficialCode);
                Application.ShowViewStrategy.ShowMessage(
                    $"Error viewing child accounts: {ex.Message}",
                    InformationType.Error);
            }
        }

        private void ViewTransactionsAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var account = e.CurrentObject as Account;
            if (account == null) return;

            try
            {
                var listView = Application.CreateListView(typeof(LedgerEntry), true);
                listView.CollectionSource.Criteria["Filter"] = CriteriaOperator.Parse("[OfficialCode] = ?", account.OfficialCode);
                
                var showViewParameters = new ShowViewParameters(listView)
                {
                    Context = TemplateContext.PopupWindow,
                    TargetWindow = TargetWindow.NewModalWindow
                };
                
                Application.ShowViewStrategy.ShowView(showViewParameters, new ShowViewSource(null, null));

                _logger?.LogInformation("Viewed transactions for account {AccountCode}", account.OfficialCode);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error viewing transactions for account {AccountCode}", account.OfficialCode);
                Application.ShowViewStrategy.ShowMessage(
                    $"Error viewing transactions: {ex.Message}",
                    InformationType.Error);
            }
        }

        private void ValidateAccountStructureAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            try
            {
                var allAccounts = ObjectSpace.GetObjectsQuery<Account>().ToList();
                var validationErrors = new List<string>();

                // Validate account structure
                foreach (var account in allAccounts)
                {
                    // Check for circular references
                    if (HasCircularReference(account, allAccounts))
                    {
                        validationErrors.Add($"Circular reference detected for account {account.OfficialCode}");
                    }

                    // Check parent exists if specified
                    if (!string.IsNullOrWhiteSpace(account.ParentOfficialCode))
                    {
                        var parent = allAccounts.FirstOrDefault(a => a.OfficialCode == account.ParentOfficialCode);
                        if (parent == null)
                        {
                            validationErrors.Add($"Parent account not found for account {account.OfficialCode}");
                        }
                    }

                    // Check for duplicate codes
                    if (allAccounts.Count(a => a.OfficialCode == account.OfficialCode) > 1)
                    {
                        validationErrors.Add($"Duplicate account code: {account.OfficialCode}");
                    }
                }

                if (validationErrors.Any())
                {
                    var errorMessage = "Chart of Accounts Validation Errors:\n\n" + string.Join("\n", validationErrors);
                    Application.ShowViewStrategy.ShowMessage(errorMessage, InformationType.Warning);
                }
                else
                {
                    Application.ShowViewStrategy.ShowMessage(
                        "Chart of Accounts structure validation passed successfully.",
                        InformationType.Success);
                }

                _logger?.LogInformation("Validated account structure. Found {ErrorCount} errors", validationErrors.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error validating account structure");
                Application.ShowViewStrategy.ShowMessage(
                    $"Error validating account structure: {ex.Message}",
                    InformationType.Error);
            }
        }

        private void RefreshBalancesAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            if (_accountBalanceCalculator == null)
            {
                Application.ShowViewStrategy.ShowMessage(
                    "Balance calculator service not available.",
                    InformationType.Warning);
                return;
            }

            try
            {
                var currentDate = DateOnly.FromDateTime(DateTime.Today);
                var allAccounts = ObjectSpace.GetObjectsQuery<Account>().ToList();
                var balanceCount = 0;

                foreach (var account in allAccounts)
                {
                    var balance = _accountBalanceCalculator.CalculateAccountBalance(account.OfficialCode, currentDate);
                    balanceCount++;
                }

                Application.ShowViewStrategy.ShowMessage(
                    $"Refreshed balances for {balanceCount} accounts as of {currentDate:yyyy-MM-dd}.",
                    InformationType.Success);

                _logger?.LogInformation("Refreshed balances for {AccountCount} accounts", balanceCount);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error refreshing account balances");
                Application.ShowViewStrategy.ShowMessage(
                    $"Error refreshing balances: {ex.Message}",
                    InformationType.Error);
            }
        }

        private void MoveAccountAction_Execute(object sender, SingleChoiceActionExecuteEventArgs e)
        {
            var account = View.CurrentObject as Account;
            var selectedParentCode = e.SelectedChoiceActionItem?.Data as string;
            
            if (account == null) return;

            try
            {
                if (selectedParentCode == null)
                {
                    // Move to root level
                    account.ParentOfficialCode = string.Empty;
                }
                else
                {
                    // Find the new parent account
                    var newParent = ObjectSpace.GetObjectsQuery<Account>()
                        .FirstOrDefault(a => a.OfficialCode == selectedParentCode);
                    
                    if (newParent == null)
                    {
                        Application.ShowViewStrategy.ShowMessage(
                            $"Parent account with code '{selectedParentCode}' not found.",
                            InformationType.Warning);
                        return;
                    }

                    // Validate that new parent is not a descendant
                    if (IsDescendantOf(newParent, account))
                    {
                        Application.ShowViewStrategy.ShowMessage(
                            "Cannot move account to one of its descendants.",
                            InformationType.Warning);
                        return;
                    }

                    account.ParentOfficialCode = newParent.OfficialCode;
                }

                ObjectSpace.CommitChanges();

                var parentName = string.IsNullOrEmpty(selectedParentCode) ? "Root Level" : 
                    ObjectSpace.GetObjectsQuery<Account>()
                        .FirstOrDefault(a => a.OfficialCode == selectedParentCode)?.AccountName ?? selectedParentCode;

                Application.ShowViewStrategy.ShowMessage(
                    $"Account '{account.AccountName}' moved to '{parentName}' successfully.",
                    InformationType.Success);

                _logger?.LogInformation("Moved account {AccountCode} to parent {ParentCode}", 
                    account.OfficialCode, selectedParentCode ?? "Root");

                // Refresh the action items since the hierarchy changed
                UpdateActionStates();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error moving account {AccountCode}", account.OfficialCode);
                Application.ShowViewStrategy.ShowMessage(
                    $"Error moving account: {ex.Message}",
                    InformationType.Error);
            }
        }

        private bool HasCircularReference(Account account, List<Account> allAccounts)
        {
            var visited = new HashSet<string>();
            var current = account;

            while (current != null && !string.IsNullOrWhiteSpace(current.ParentOfficialCode))
            {
                if (visited.Contains(current.OfficialCode))
                    return true;

                visited.Add(current.OfficialCode);
                current = allAccounts.FirstOrDefault(a => a.OfficialCode == current.ParentOfficialCode);
            }

            return false;
        }

        private bool IsDescendantOf(Account potentialDescendant, Account ancestor)
        {
            var current = potentialDescendant;
            
            while (current != null && !string.IsNullOrWhiteSpace(current.ParentOfficialCode))
            {
                if (current.ParentOfficialCode == ancestor.OfficialCode)
                    return true;

                current = ObjectSpace.GetObjectsQuery<Account>()
                    .FirstOrDefault(a => a.OfficialCode == current.ParentOfficialCode);
            }

            return false;
        }

        public void ExtendModelInterfaces(ModelInterfaceExtenders extenders)
        {
            // Extend model if needed for custom properties
        }
    }
}