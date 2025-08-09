using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.Persistent.Base;
using Sivar.Erp.Xaf.Module.BusinessObjects.Inventory;
using System;
using System.Linq;

namespace Sivar.Erp.Xaf.Module.Controllers.Inventory
{
    /// <summary>
    /// Controller for inventory management operations
    /// Provides actions for stock adjustments, transfers, and item management
    /// </summary>
    public partial class InventoryViewController : ObjectViewController<DetailView, InventoryItem>
    {
        private SimpleAction adjustStockAction;
        private SimpleAction transferStockAction;
        private SimpleAction calculateReorderAction;
        private SimpleAction viewKardexAction;

        public InventoryViewController()
        {
            InitializeActions();
        }

        private void InitializeActions()
        {
            // Stock adjustment action
            adjustStockAction = new SimpleAction(this, "AdjustStock", PredefinedCategory.Edit)
            {
                Caption = "Adjust Stock",
                ToolTip = "Adjust stock levels for this item",
                ImageName = "Action_Edit"
            };
            adjustStockAction.Execute += AdjustStockAction_Execute;

            // Stock transfer action
            transferStockAction = new SimpleAction(this, "TransferStock", PredefinedCategory.Edit)
            {
                Caption = "Transfer Stock",
                ToolTip = "Transfer stock between warehouses",
                ImageName = "Action_Export"
            };
            transferStockAction.Execute += TransferStockAction_Execute;

            // Calculate reorder action
            calculateReorderAction = new SimpleAction(this, "CalculateReorder", PredefinedCategory.Tools)
            {
                Caption = "Calculate Reorder",
                ToolTip = "Calculate optimal reorder points and quantities",
                ImageName = "Action_Refresh"
            };
            calculateReorderAction.Execute += CalculateReorderAction_Execute;

            // View kardex action
            viewKardexAction = new SimpleAction(this, "ViewKardex", PredefinedCategory.View)
            {
                Caption = "View Kardex",
                ToolTip = "View inventory movement history (kardex)",
                ImageName = "Action_Report"
            };
            viewKardexAction.Execute += ViewKardexAction_Execute;
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            UpdateActionStates();
        }

        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();
            UpdateActionStates();
        }

        private void UpdateActionStates()
        {
            var inventoryItem = ViewCurrentObject;
            bool hasItem = inventoryItem != null;
            bool isStockable = hasItem && inventoryItem.IsStockable;
            bool isActive = hasItem && inventoryItem.IsActive;

            adjustStockAction.Enabled["HasItem"] = isStockable && isActive;
            transferStockAction.Enabled["HasItem"] = isStockable && isActive;
            calculateReorderAction.Enabled["HasItem"] = hasItem;
            viewKardexAction.Enabled["HasItem"] = hasItem;
        }

        private void AdjustStockAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var inventoryItem = ViewCurrentObject;
            if (inventoryItem?.IsStockable != true || !inventoryItem.IsActive)
                return;

            try
            {
                // Create a new inventory transaction for adjustment
                var objectSpace = Application.CreateObjectSpace(typeof(InventoryTransaction));
                var transaction = objectSpace.CreateObject<InventoryTransaction>();
                
                transaction.Item = objectSpace.GetObject(inventoryItem);
                transaction.TransactionType = Sivar.Erp.Modules.Inventory.InventoryTransactionType.Adjustment;
                transaction.TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow);
                transaction.ReferenceDocumentNumber = "MANUAL-ADJ";
                
                // Show detailed view for the adjustment
                var detailView = Application.CreateDetailView(objectSpace, transaction);
                detailView.Caption = $"Stock Adjustment - {inventoryItem.Code}";
                
                Application.ShowViewStrategy.ShowView(
                    new ShowViewParameters(detailView)
                    {
                        Context = TemplateContext.PopupWindow,
                        TargetWindow = TargetWindow.NewModalWindow
                    },
                    new ShowViewSource(null, null));
            }
            catch (Exception ex)
            {
                Application.ShowViewStrategy.ShowMessage(
                    $"Error creating stock adjustment: {ex.Message}",
                    InformationType.Error);
            }
        }

        private void TransferStockAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var inventoryItem = ViewCurrentObject;
            if (inventoryItem?.IsStockable != true || !inventoryItem.IsActive)
                return;

            try
            {
                // Create a new inventory transaction for transfer
                var objectSpace = Application.CreateObjectSpace(typeof(InventoryTransaction));
                var transaction = objectSpace.CreateObject<InventoryTransaction>();
                
                transaction.Item = objectSpace.GetObject(inventoryItem);
                transaction.TransactionType = Sivar.Erp.Modules.Inventory.InventoryTransactionType.Transfer;
                transaction.TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow);
                transaction.ReferenceDocumentNumber = "MANUAL-TRF";
                
                // Show detailed view for the transfer
                var detailView = Application.CreateDetailView(objectSpace, transaction);
                detailView.Caption = $"Stock Transfer - {inventoryItem.Code}";
                
                Application.ShowViewStrategy.ShowView(
                    new ShowViewParameters(detailView)
                    {
                        Context = TemplateContext.PopupWindow,
                        TargetWindow = TargetWindow.NewModalWindow
                    },
                    new ShowViewSource(null, null));
            }
            catch (Exception ex)
            {
                Application.ShowViewStrategy.ShowMessage(
                    $"Error creating stock transfer: {ex.Message}",
                    InformationType.Error);
            }
        }

        private void CalculateReorderAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var inventoryItem = ViewCurrentObject;
            if (inventoryItem == null)
                return;

            try
            {
                // Calculate optimal reorder points based on historical consumption
                var stockLevels = inventoryItem.StockLevels.ToList();
                var transactions = inventoryItem.Transactions
                    .Where(t => t.TransactionDate >= DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-6)))
                    .ToList();

                if (transactions.Any())
                {
                    // Calculate average monthly consumption
                    var issues = transactions.Where(t => t.Quantity.HasValue && t.Quantity.Value < 0).ToList();
                    decimal totalIssued = Math.Abs(issues.Sum(t => t.Quantity.Value));
                    decimal monthsInPeriod = 6;
                    decimal averageMonthlyConsumption = totalIssued / monthsInPeriod;

                    // Calculate suggested reorder point (30 days of consumption)
                    decimal suggestedReorderPoint = averageMonthlyConsumption;
                    decimal suggestedReorderQuantity = averageMonthlyConsumption * 2; // 2 months supply

                    inventoryItem.ReorderPoint = suggestedReorderPoint;
                    inventoryItem.ReorderQuantity = suggestedReorderQuantity;

                    // Update minimum levels for stock levels
                    foreach (var stockLevel in stockLevels)
                    {
                        stockLevel.MinimumLevel = suggestedReorderPoint;
                    }

                    Application.ShowViewStrategy.ShowMessage(
                        $"Reorder calculations updated:\n" +
                        $"Reorder Point: {suggestedReorderPoint:n2}\n" +
                        $"Reorder Quantity: {suggestedReorderQuantity:n2}\n" +
                        $"Based on {totalIssued:n2} units consumed over 6 months",
                        InformationType.Success);
                }
                else
                {
                    Application.ShowViewStrategy.ShowMessage(
                        "No transaction history found for reorder calculation",
                        InformationType.Warning);
                }
            }
            catch (Exception ex)
            {
                Application.ShowViewStrategy.ShowMessage(
                    $"Error calculating reorder levels: {ex.Message}",
                    InformationType.Error);
            }
        }

        private void ViewKardexAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var inventoryItem = ViewCurrentObject;
            if (inventoryItem == null)
                return;

            try
            {
                // Create a detail view showing the item with its transactions tab
                var objectSpace = Application.CreateObjectSpace(typeof(InventoryItem));
                var item = objectSpace.GetObject(inventoryItem);
                
                var detailView = Application.CreateDetailView(objectSpace, item);
                detailView.Caption = $"Kardex - {inventoryItem.Code} ({inventoryItem.Description})";

                Application.ShowViewStrategy.ShowView(
                    new ShowViewParameters(detailView)
                    {
                        Context = TemplateContext.PopupWindow,
                        TargetWindow = TargetWindow.NewModalWindow
                    },
                    new ShowViewSource(null, null));
            }
            catch (Exception ex)
            {
                Application.ShowViewStrategy.ShowMessage(
                    $"Error viewing kardex: {ex.Message}",
                    InformationType.Error);
            }
        }

        protected override void OnDeactivated()
        {
            base.OnDeactivated();
        }
    }
}