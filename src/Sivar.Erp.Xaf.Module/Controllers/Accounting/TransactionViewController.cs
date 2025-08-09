using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.Persistent.Base;
using DevExpress.Data.Filtering;
using Microsoft.Extensions.Logging;
using Sivar.Erp.Services.Accounting.Transactions;
using Sivar.Erp.Services.Accounting.FiscalPeriods;
using Sivar.Erp.Modules.Accounting;
using Sivar.Erp.Xaf.Module.BusinessObjects.Accounting;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Sivar.Erp.Xaf.Module.Controllers.Accounting
{
    /// <summary>
    /// Controller for transaction processing and management operations
    /// </summary>
    [DefaultClassOptions]
    public class TransactionViewController : ViewController<DetailView>, IModelExtender
    {
        private readonly IAccountingModule _accountingModule;
        private readonly IFiscalPeriodService _fiscalPeriodService;
        private readonly ILogger<TransactionViewController> _logger;

        // Actions for transaction operations
        private SimpleAction _postTransactionAction;
        private SimpleAction _unpostTransactionAction;
        private SimpleAction _validateTransactionAction;
        private SimpleAction _copyTransactionAction;
        private SimpleAction _reverseTransactionAction;
        private SimpleAction _viewLedgerEntriesAction;
        private SimpleAction _generateTransactionAuditTrailAction;

        public TransactionViewController()
        {
            TargetObjectType = typeof(Transaction);
            TargetViewType = ViewType.DetailView;

            // Initialize actions
            InitializeActions();

            // Try to get services from DI container if available
            try
            {
                _accountingModule = Application?.ServiceProvider?.GetService(typeof(IAccountingModule)) as IAccountingModule;
                _fiscalPeriodService = Application?.ServiceProvider?.GetService(typeof(IFiscalPeriodService)) as IFiscalPeriodService;
                _logger = Application?.ServiceProvider?.GetService(typeof(ILogger<TransactionViewController>)) as ILogger<TransactionViewController>;
            }
            catch
            {
                // Services not available, will handle gracefully
            }
        }

        private void InitializeActions()
        {
            // Post Transaction Action
            _postTransactionAction = new SimpleAction(this, "PostTransaction", PredefinedCategory.Edit)
            {
                Caption = "Post",
                ToolTip = "Post this transaction to the ledger",
                ImageName = "CheckMark",
                SelectionDependencyType = SelectionDependencyType.RequireSingleObject
            };
            _postTransactionAction.Execute += PostTransactionAction_Execute;

            // Unpost Transaction Action
            _unpostTransactionAction = new SimpleAction(this, "UnpostTransaction", PredefinedCategory.Edit)
            {
                Caption = "Unpost",
                ToolTip = "Unpost this transaction from the ledger",
                ImageName = "Cancel",
                SelectionDependencyType = SelectionDependencyType.RequireSingleObject
            };
            _unpostTransactionAction.Execute += UnpostTransactionAction_Execute;

            // Validate Transaction Action
            _validateTransactionAction = new SimpleAction(this, "ValidateTransaction", PredefinedCategory.Tools)
            {
                Caption = "Validate",
                ToolTip = "Validate transaction balance and posting requirements",
                ImageName = "CheckSpelling",
                SelectionDependencyType = SelectionDependencyType.RequireSingleObject
            };
            _validateTransactionAction.Execute += ValidateTransactionAction_Execute;

            // Copy Transaction Action
            _copyTransactionAction = new SimpleAction(this, "CopyTransaction", PredefinedCategory.Edit)
            {
                Caption = "Copy",
                ToolTip = "Create a copy of this transaction",
                ImageName = "Copy",
                SelectionDependencyType = SelectionDependencyType.RequireSingleObject
            };
            _copyTransactionAction.Execute += CopyTransactionAction_Execute;

            // Reverse Transaction Action
            _reverseTransactionAction = new SimpleAction(this, "ReverseTransaction", PredefinedCategory.Edit)
            {
                Caption = "Reverse",
                ToolTip = "Create a reversal transaction",
                ImageName = "Undo",
                SelectionDependencyType = SelectionDependencyType.RequireSingleObject
            };
            _reverseTransactionAction.Execute += ReverseTransactionAction_Execute;

            // View Ledger Entries Action
            _viewLedgerEntriesAction = new SimpleAction(this, "ViewLedgerEntries", PredefinedCategory.View)
            {
                Caption = "View Entries",
                ToolTip = "View all ledger entries for this transaction",
                ImageName = "Action_Report_Object_Inplaces_Transaction",
                SelectionDependencyType = SelectionDependencyType.RequireSingleObject
            };
            _viewLedgerEntriesAction.Execute += ViewLedgerEntriesAction_Execute;

            // Generate Transaction Audit Trail Action
            _generateTransactionAuditTrailAction = new SimpleAction(this, "GenerateTransactionAuditTrail", PredefinedCategory.Reports)
            {
                Caption = "Audit Trail",
                ToolTip = "Generate audit trail report for this transaction",
                ImageName = "Report",
                SelectionDependencyType = SelectionDependencyType.RequireSingleObject
            };
            _generateTransactionAuditTrailAction.Execute += GenerateTransactionAuditTrailAction_Execute;
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
            if (e.Object is Transaction)
            {
                UpdateActionStates();
            }
        }

        private void UpdateActionStates()
        {
            var transaction = View.CurrentObject as Transaction;
            if (transaction == null)
            {
                SetAllActionsEnabled(false);
                return;
            }

            // Update action availability based on transaction status
            _postTransactionAction.Enabled["TransactionStatus"] = !transaction.IsPosted && IsTransactionComplete(transaction);
            _unpostTransactionAction.Enabled["TransactionStatus"] = transaction.IsPosted;
            _validateTransactionAction.Enabled["General"] = true;
            _copyTransactionAction.Enabled["General"] = true;
            _reverseTransactionAction.Enabled["TransactionStatus"] = transaction.IsPosted;
            _viewLedgerEntriesAction.Enabled["General"] = transaction.LedgerEntries.Count > 0;
            _generateTransactionAuditTrailAction.Enabled["General"] = transaction.LedgerEntries.Count > 0;

            // Additional validation - check fiscal period
            if (_fiscalPeriodService != null)
            {
                CheckFiscalPeriodAsync(transaction);
            }
        }

        private async void CheckFiscalPeriodAsync(Transaction transaction)
        {
            try
            {
                var fiscalPeriod = await _fiscalPeriodService.GetFiscalPeriodForDateAsync(transaction.TransactionDate);
                var isInOpenPeriod = fiscalPeriod?.Status == FiscalPeriodStatus.Open;

                _postTransactionAction.Enabled["FiscalPeriod"] = isInOpenPeriod;
                _unpostTransactionAction.Enabled["FiscalPeriod"] = isInOpenPeriod;
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Could not check fiscal period for transaction {TransactionNumber}", transaction.TransactionNumber);
            }
        }

        private bool IsTransactionComplete(Transaction transaction)
        {
            return !string.IsNullOrWhiteSpace(transaction.Description) &&
                   transaction.LedgerEntries.Count > 0 &&
                   transaction.IsBalanced;
        }

        private void SetAllActionsEnabled(bool enabled)
        {
            _postTransactionAction.Enabled.SetItemValue("General", enabled);
            _unpostTransactionAction.Enabled.SetItemValue("General", enabled);
            _validateTransactionAction.Enabled.SetItemValue("General", enabled);
            _copyTransactionAction.Enabled.SetItemValue("General", enabled);
            _reverseTransactionAction.Enabled.SetItemValue("General", enabled);
            _viewLedgerEntriesAction.Enabled.SetItemValue("General", enabled);
            _generateTransactionAuditTrailAction.Enabled.SetItemValue("General", enabled);
        }

        private async void PostTransactionAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var transaction = e.CurrentObject as Transaction;
            if (transaction == null || _accountingModule == null) return;

            try
            {
                // Validate transaction before posting
                var isValid = await _accountingModule.ValidateTransactionAsync(transaction);
                if (!isValid)
                {
                    Application.ShowViewStrategy.ShowMessage(
                        "Transaction validation failed. Please check that debits equal credits.",
                        InformationType.Warning);
                    return;
                }

                // Check fiscal period
                var isInOpenPeriod = await _accountingModule.IsDateInOpenFiscalPeriodAsync(transaction.TransactionDate);
                if (!isInOpenPeriod)
                {
                    Application.ShowViewStrategy.ShowMessage(
                        "Cannot post transaction: Date is not in an open fiscal period.",
                        InformationType.Warning);
                    return;
                }

                // Post the transaction
                var success = await _accountingModule.PostTransactionAsync(transaction);
                
                if (success)
                {
                    ObjectSpace.CommitChanges();
                    Application.ShowViewStrategy.ShowMessage(
                        $"Transaction {transaction.TransactionNumber} posted successfully.",
                        InformationType.Success);

                    _logger?.LogInformation("Transaction {TransactionNumber} posted by {User}", 
                        transaction.TransactionNumber, SecuritySystem.CurrentUserName);

                    UpdateActionStates();
                }
                else
                {
                    Application.ShowViewStrategy.ShowMessage(
                        "Failed to post transaction. Please check the transaction details.",
                        InformationType.Warning);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error posting transaction {TransactionNumber}", transaction.TransactionNumber);
                Application.ShowViewStrategy.ShowMessage(
                    $"Error posting transaction: {ex.Message}",
                    InformationType.Error);
            }
        }

        private async void UnpostTransactionAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var transaction = e.CurrentObject as Transaction;
            if (transaction == null || _accountingModule == null) return;

            try
            {
                // Check fiscal period
                var isInOpenPeriod = await _accountingModule.IsDateInOpenFiscalPeriodAsync(transaction.TransactionDate);
                if (!isInOpenPeriod)
                {
                    Application.ShowViewStrategy.ShowMessage(
                        "Cannot unpost transaction: Date is not in an open fiscal period.",
                        InformationType.Warning);
                    return;
                }

                // Unpost the transaction
                var success = await _accountingModule.UnPostTransactionAsync(transaction);
                
                if (success)
                {
                    ObjectSpace.CommitChanges();
                    Application.ShowViewStrategy.ShowMessage(
                        $"Transaction {transaction.TransactionNumber} unposted successfully.",
                        InformationType.Success);

                    _logger?.LogInformation("Transaction {TransactionNumber} unposted by {User}", 
                        transaction.TransactionNumber, SecuritySystem.CurrentUserName);

                    UpdateActionStates();
                }
                else
                {
                    Application.ShowViewStrategy.ShowMessage(
                        "Failed to unpost transaction.",
                        InformationType.Warning);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error unposting transaction {TransactionNumber}", transaction.TransactionNumber);
                Application.ShowViewStrategy.ShowMessage(
                    $"Error unposting transaction: {ex.Message}",
                    InformationType.Error);
            }
        }

        private async void ValidateTransactionAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var transaction = e.CurrentObject as Transaction;
            if (transaction == null) return;

            try
            {
                var validationMessages = new List<string>();

                // Basic validation
                if (string.IsNullOrWhiteSpace(transaction.Description))
                    validationMessages.Add("• Description is required");

                if (transaction.LedgerEntries.Count == 0)
                    validationMessages.Add("• At least one ledger entry is required");

                if (transaction.LedgerEntries.Count == 1)
                    validationMessages.Add("• At least two ledger entries are required for a valid transaction");

                // Balance validation
                if (!transaction.IsBalanced)
                {
                    validationMessages.Add($"• Transaction is not balanced:");
                    validationMessages.Add($"  - Total Debits: {transaction.TotalDebits:C}");
                    validationMessages.Add($"  - Total Credits: {transaction.TotalCredits:C}");
                    validationMessages.Add($"  - Difference: {transaction.OutOfBalance:C}");
                }

                // Account validation
                foreach (var entry in transaction.LedgerEntries)
                {
                    if (string.IsNullOrWhiteSpace(entry.OfficialCode))
                        validationMessages.Add($"• Ledger entry #{entry.LedgerEntryNumber}: Account code is required");

                    if (entry.Amount <= 0)
                        validationMessages.Add($"• Ledger entry #{entry.LedgerEntryNumber}: Amount must be greater than zero");
                }

                // Fiscal period validation
                if (_accountingModule != null)
                {
                    var isInOpenPeriod = await _accountingModule.IsDateInOpenFiscalPeriodAsync(transaction.TransactionDate);
                    if (!isInOpenPeriod)
                        validationMessages.Add("• Transaction date is not in an open fiscal period");
                }

                // Show validation results
                if (validationMessages.Any())
                {
                    var errorMessage = "Transaction Validation Issues:\n\n" + string.Join("\n", validationMessages);
                    Application.ShowViewStrategy.ShowMessage(errorMessage, InformationType.Warning);
                }
                else
                {
                    Application.ShowViewStrategy.ShowMessage(
                        "Transaction validation passed successfully. Ready for posting.",
                        InformationType.Success);
                }

                _logger?.LogInformation("Validated transaction {TransactionNumber}. Found {ErrorCount} issues", 
                    transaction.TransactionNumber, validationMessages.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error validating transaction {TransactionNumber}", transaction.TransactionNumber);
                Application.ShowViewStrategy.ShowMessage(
                    $"Error validating transaction: {ex.Message}",
                    InformationType.Error);
            }
        }

        private void CopyTransactionAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var originalTransaction = e.CurrentObject as Transaction;
            if (originalTransaction == null) return;

            try
            {
                // Create a new transaction with copied values
                var newTransaction = ObjectSpace.CreateObject<Transaction>();
                newTransaction.TransactionDate = originalTransaction.TransactionDate;
                newTransaction.Description = $"Copy of {originalTransaction.Description}";
                newTransaction.DocumentNumber = originalTransaction.DocumentNumber;

                // Copy ledger entries
                foreach (var originalEntry in originalTransaction.LedgerEntries)
                {
                    var newEntry = ObjectSpace.CreateObject<LedgerEntry>();
                    newEntry.OfficialCode = originalEntry.OfficialCode;
                    newEntry.AccountName = originalEntry.AccountName;
                    newEntry.EntryType = originalEntry.EntryType;
                    newEntry.Amount = originalEntry.Amount;
                    newEntry.Transaction = newTransaction;
                    
                    newTransaction.LedgerEntries.Add(newEntry);
                }

                // Show the new transaction
                var detailView = Application.CreateDetailView(ObjectSpace, newTransaction);
                var showViewParameters = new ShowViewParameters(detailView)
                {
                    Context = TemplateContext.View,
                    TargetWindow = TargetWindow.Current
                };
                
                Application.ShowViewStrategy.ShowView(showViewParameters, new ShowViewSource(null, null));

                _logger?.LogInformation("Copied transaction {OriginalTransactionNumber} to new transaction", 
                    originalTransaction.TransactionNumber);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error copying transaction {TransactionNumber}", originalTransaction.TransactionNumber);
                Application.ShowViewStrategy.ShowMessage(
                    $"Error copying transaction: {ex.Message}",
                    InformationType.Error);
            }
        }

        private void ReverseTransactionAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var originalTransaction = e.CurrentObject as Transaction;
            if (originalTransaction == null) return;

            try
            {
                // Create a reversal transaction
                var reversalTransaction = ObjectSpace.CreateObject<Transaction>();
                reversalTransaction.TransactionDate = DateOnly.FromDateTime(DateTime.Today);
                reversalTransaction.Description = $"Reversal of {originalTransaction.Description}";
                reversalTransaction.DocumentNumber = originalTransaction.DocumentNumber;

                // Create reversal entries (flip debit/credit)
                foreach (var originalEntry in originalTransaction.LedgerEntries)
                {
                    var reversalEntry = ObjectSpace.CreateObject<LedgerEntry>();
                    reversalEntry.OfficialCode = originalEntry.OfficialCode;
                    reversalEntry.AccountName = originalEntry.AccountName;
                    reversalEntry.EntryType = originalEntry.EntryType == EntryType.Debit ? EntryType.Credit : EntryType.Debit;
                    reversalEntry.Amount = originalEntry.Amount;
                    reversalEntry.Transaction = reversalTransaction;
                    
                    reversalTransaction.LedgerEntries.Add(reversalEntry);
                }

                // Show the reversal transaction
                var detailView = Application.CreateDetailView(ObjectSpace, reversalTransaction);
                var showViewParameters = new ShowViewParameters(detailView)
                {
                    Context = TemplateContext.View,
                    TargetWindow = TargetWindow.Current
                };
                
                Application.ShowViewStrategy.ShowView(showViewParameters, new ShowViewSource(null, null));

                _logger?.LogInformation("Created reversal transaction for {OriginalTransactionNumber}", 
                    originalTransaction.TransactionNumber);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating reversal transaction for {TransactionNumber}", originalTransaction.TransactionNumber);
                Application.ShowViewStrategy.ShowMessage(
                    $"Error creating reversal transaction: {ex.Message}",
                    InformationType.Error);
            }
        }

        private void ViewLedgerEntriesAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var transaction = e.CurrentObject as Transaction;
            if (transaction == null) return;

            try
            {
                var listView = Application.CreateListView(typeof(LedgerEntry), true);
                listView.CollectionSource.Criteria["Filter"] = CriteriaOperator.Parse("[Transaction.Oid] = ?", transaction.Oid);
                
                var showViewParameters = new ShowViewParameters(listView)
                {
                    Context = TemplateContext.PopupWindow,
                    TargetWindow = TargetWindow.NewModalWindow
                };
                
                Application.ShowViewStrategy.ShowView(showViewParameters, new ShowViewSource(null, null));

                _logger?.LogInformation("Viewed ledger entries for transaction {TransactionNumber}", transaction.TransactionNumber);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error viewing ledger entries for transaction {TransactionNumber}", transaction.TransactionNumber);
                Application.ShowViewStrategy.ShowMessage(
                    $"Error viewing ledger entries: {ex.Message}",
                    InformationType.Error);
            }
        }

        private async void GenerateTransactionAuditTrailAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var transaction = e.CurrentObject as Transaction;
            if (transaction == null || _accountingModule == null) return;

            try
            {
                // Generate audit trail for this transaction
                var auditTrail = await _accountingModule.GenerateTransactionAuditTrailAsync(transaction.TransactionNumber);

                var reportMessage = $"Transaction Audit Trail Report\n\n" +
                                  $"Transaction Number: {auditTrail.TransactionNumber}\n" +
                                  $"Date: {auditTrail.TransactionDate:yyyy-MM-dd}\n" +
                                  $"Description: {auditTrail.Description}\n" +
                                  $"Status: {(auditTrail.IsPosted ? "Posted" : "Draft")}\n" +
                                  $"Total Debits: {auditTrail.TotalDebits:C}\n" +
                                  $"Total Credits: {auditTrail.TotalCredits:C}\n" +
                                  $"Balance: {(auditTrail.IsBalanced ? "Balanced" : "Unbalanced")}\n\n" +
                                  $"Ledger Entries: {transaction.LedgerEntries.Count}";

                Application.ShowViewStrategy.ShowMessage(reportMessage, InformationType.Info);

                _logger?.LogInformation("Generated transaction audit trail for {TransactionNumber}", transaction.TransactionNumber);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating transaction audit trail for {TransactionNumber}", transaction.TransactionNumber);
                Application.ShowViewStrategy.ShowMessage(
                    $"Error generating transaction audit trail: {ex.Message}",
                    InformationType.Error);
            }
        }

        public void ExtendModelInterfaces(ModelInterfaceExtenders extenders)
        {
            // Extend model if needed for custom properties
        }
    }
}