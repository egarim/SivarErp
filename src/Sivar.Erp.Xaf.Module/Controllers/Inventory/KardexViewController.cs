using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.Persistent.Base;
using Sivar.Erp.Xaf.Module.BusinessObjects.Inventory;
using Sivar.Erp.Modules.Inventory.Reports;
using System;
using System.Linq;

namespace Sivar.Erp.Xaf.Module.Controllers.Inventory
{
    /// <summary>
    /// Controller for inventory kardex and analysis operations
    /// Provides actions for generating reports and analyzing inventory data
    /// </summary>
    public partial class KardexViewController : ObjectViewController<DetailView, InventoryItem>
    {
        private SimpleAction generateKardexAction;
        private SimpleAction inventoryValuationAction;
        private SimpleAction reorderAnalysisAction;
        private SimpleAction abcAnalysisAction;

        public KardexViewController()
        {
            InitializeActions();
        }

        private void InitializeActions()
        {
            // Generate kardex report action
            generateKardexAction = new SimpleAction(this, "GenerateKardex", PredefinedCategory.Reports)
            {
                Caption = "Generate Kardex",
                ToolTip = "Generate inventory movement report (kardex) for this item",
                ImageName = "Action_Report"
            };
            generateKardexAction.Execute += GenerateKardexAction_Execute;

            // Inventory valuation action
            inventoryValuationAction = new SimpleAction(this, "InventoryValuation", PredefinedCategory.Reports)
            {
                Caption = "Inventory Valuation",
                ToolTip = "View current inventory valuation for this item",
                ImageName = "Action_Analytics_Chart"
            };
            inventoryValuationAction.Execute += InventoryValuationAction_Execute;

            // Reorder analysis action
            reorderAnalysisAction = new SimpleAction(this, "ReorderAnalysis", PredefinedCategory.Tools)
            {
                Caption = "Reorder Analysis",
                ToolTip = "Analyze reorder requirements and consumption patterns",
                ImageName = "Action_Analytics_Pie"
            };
            reorderAnalysisAction.Execute += ReorderAnalysisAction_Execute;

            // ABC analysis action
            abcAnalysisAction = new SimpleAction(this, "ABCAnalysis", PredefinedCategory.Tools)
            {
                Caption = "ABC Analysis",
                ToolTip = "Perform ABC analysis on inventory items",
                ImageName = "Action_FullExpand"
            };
            abcAnalysisAction.Execute += ABCAnalysisAction_Execute;
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

            generateKardexAction.Enabled["HasItem"] = hasItem;
            inventoryValuationAction.Enabled["HasItem"] = isStockable && isActive;
            reorderAnalysisAction.Enabled["HasItem"] = isStockable && isActive;
            abcAnalysisAction.Enabled["Always"] = true; // ABC analysis can work on all items
        }

        private void GenerateKardexAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var inventoryItem = ViewCurrentObject;
            if (inventoryItem == null)
                return;

            try
            {
                // Create a parameter object for the kardex report
                var objectSpace = Application.CreateObjectSpace(typeof(KardexReportParameter));
                var parameter = objectSpace.CreateObject<KardexReportParameter>();
                
                parameter.ItemCode = inventoryItem.Code;
                parameter.StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-3)); // Default to last 3 months
                parameter.EndDate = DateOnly.FromDateTime(DateTime.UtcNow);
                
                // Show detailed view for the parameter
                var detailView = Application.CreateDetailView(objectSpace, parameter);
                detailView.Caption = $"Kardex Report Parameters - {inventoryItem.Code}";
                
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
                    $"Error preparing kardex report: {ex.Message}",
                    InformationType.Error);
            }
        }

        private void InventoryValuationAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var inventoryItem = ViewCurrentObject;
            if (inventoryItem?.IsStockable != true || !inventoryItem.IsActive)
                return;

            try
            {
                // Calculate current valuation
                var stockLevels = inventoryItem.StockLevels.ToList();
                decimal totalQuantity = 0;
                decimal totalValue = 0;

                foreach (var stockLevel in stockLevels)
                {
                    var quantity = stockLevel.QuantityOnHand;
                    var value = quantity * inventoryItem.AverageCost;
                    totalQuantity += quantity;
                    totalValue += value;
                }

                var message = $"Current Inventory Valuation for {inventoryItem.Code}:\n\n" +
                             $"Total Quantity: {totalQuantity:n2} {inventoryItem.UnitOfMeasure}\n" +
                             $"Average Cost: {inventoryItem.AverageCost:c}\n" +
                             $"Total Value: {totalValue:c}\n\n" +
                             $"Warehouses: {stockLevels.Count}\n" +
                             $"Available Quantity: {stockLevels.Sum(sl => sl.AvailableQuantity):n2}\n" +
                             $"Reserved Quantity: {stockLevels.Sum(sl => sl.QuantityReserved):n2}";

                Application.ShowViewStrategy.ShowMessage(message, InformationType.Info);
            }
            catch (Exception ex)
            {
                Application.ShowViewStrategy.ShowMessage(
                    $"Error calculating inventory valuation: {ex.Message}",
                    InformationType.Error);
            }
        }

        private void ReorderAnalysisAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var inventoryItem = ViewCurrentObject;
            if (inventoryItem?.IsStockable != true || !inventoryItem.IsActive)
                return;

            try
            {
                // Analyze consumption patterns and reorder requirements
                var stockLevels = inventoryItem.StockLevels.ToList();
                var transactions = inventoryItem.Transactions
                    .Where(t => t.TransactionDate >= DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-6)))
                    .OrderBy(t => t.TransactionDate)
                    .ToList();

                if (!transactions.Any())
                {
                    Application.ShowViewStrategy.ShowMessage(
                        "No transaction history found for the last 6 months",
                        InformationType.Warning);
                    return;
                }

                // Calculate consumption statistics
                var issues = transactions.Where(t => t.Quantity.HasValue && t.Quantity.Value < 0).ToList();
                var receipts = transactions.Where(t => t.Quantity.HasValue && t.Quantity.Value > 0).ToList();

                decimal totalIssued = Math.Abs(issues.Sum(t => t.Quantity.Value));
                decimal totalReceived = receipts.Sum(t => t.Quantity.Value);
                decimal monthsInPeriod = 6;
                decimal averageMonthlyConsumption = totalIssued / monthsInPeriod;

                // Current stock status
                decimal totalOnHand = stockLevels.Sum(sl => sl.QuantityOnHand);
                decimal totalAvailable = stockLevels.Sum(sl => sl.AvailableQuantity);

                // Calculate days of stock remaining
                decimal daysOfStock = averageMonthlyConsumption > 0 ? (totalAvailable / averageMonthlyConsumption) * 30 : 0;

                // Identify items below reorder point
                var itemsBelowReorder = stockLevels.Where(sl => sl.QuantityOnHand <= inventoryItem.ReorderPoint).ToList();

                var analysisMessage = $"Reorder Analysis for {inventoryItem.Code} (Last 6 Months):\n\n" +
                                    $"Consumption Analysis:\n" +
                                    $"• Total Issued: {totalIssued:n2} {inventoryItem.UnitOfMeasure}\n" +
                                    $"• Total Received: {totalReceived:n2} {inventoryItem.UnitOfMeasure}\n" +
                                    $"• Average Monthly Consumption: {averageMonthlyConsumption:n2} {inventoryItem.UnitOfMeasure}\n" +
                                    $"• Number of Issue Transactions: {issues.Count}\n" +
                                    $"• Number of Receipt Transactions: {receipts.Count}\n\n" +
                                    $"Current Stock Status:\n" +
                                    $"• Total On Hand: {totalOnHand:n2} {inventoryItem.UnitOfMeasure}\n" +
                                    $"• Total Available: {totalAvailable:n2} {inventoryItem.UnitOfMeasure}\n" +
                                    $"• Estimated Days of Stock: {daysOfStock:n0} days\n" +
                                    $"• Current Reorder Point: {inventoryItem.ReorderPoint:n2} {inventoryItem.UnitOfMeasure}\n" +
                                    $"• Current Reorder Quantity: {inventoryItem.ReorderQuantity:n2} {inventoryItem.UnitOfMeasure}\n\n";

                if (itemsBelowReorder.Any())
                {
                    analysisMessage += $"?? WARNING: {itemsBelowReorder.Count} warehouse(s) are below reorder point!\n";
                    foreach (var warehouse in itemsBelowReorder)
                    {
                        analysisMessage += $"• {warehouse.WarehouseCode}: {warehouse.QuantityOnHand:n2} (Reorder at {inventoryItem.ReorderPoint:n2})\n";
                    }
                }
                else
                {
                    analysisMessage += "? All warehouses are above reorder point\n";
                }

                Application.ShowViewStrategy.ShowMessage(analysisMessage, InformationType.Info);
            }
            catch (Exception ex)
            {
                Application.ShowViewStrategy.ShowMessage(
                    $"Error performing reorder analysis: {ex.Message}",
                    InformationType.Error);
            }
        }

        private void ABCAnalysisAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            try
            {
                // ABC Analysis works on all inventory items, not just the current one
                var objectSpace = Application.CreateObjectSpace(typeof(InventoryItem));
                
                // Get all active, stockable inventory items
                var allItems = objectSpace.GetObjects<InventoryItem>()
                    .Where(i => i.IsActive && i.IsStockable)
                    .ToList();

                if (!allItems.Any())
                {
                    Application.ShowViewStrategy.ShowMessage(
                        "No active, stockable inventory items found for ABC analysis",
                        InformationType.Warning);
                    return;
                }

                // Calculate annual usage value for each item
                var itemAnalysis = new List<(InventoryItem Item, decimal AnnualValue, decimal AnnualQuantity)>();

                foreach (var item in allItems)
                {
                    // Get last 12 months of issue transactions
                    var issues = item.Transactions
                        .Where(t => t.TransactionDate >= DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)) &&
                                   t.Quantity.HasValue && t.Quantity.Value < 0)
                        .ToList();

                    decimal annualQuantity = Math.Abs(issues.Sum(t => t.Quantity.Value));
                    decimal annualValue = annualQuantity * item.AverageCost;

                    itemAnalysis.Add((item, annualValue, annualQuantity));
                }

                // Sort by annual value descending
                var sortedItems = itemAnalysis.OrderByDescending(x => x.AnnualValue).ToList();
                decimal totalValue = sortedItems.Sum(x => x.AnnualValue);

                // Classify items into ABC categories
                var results = new List<string>();
                decimal cumulativeValue = 0;
                var categoryA = new List<string>();
                var categoryB = new List<string>();
                var categoryC = new List<string>();

                for (int i = 0; i < sortedItems.Count; i++)
                {
                    var item = sortedItems[i];
                    cumulativeValue += item.AnnualValue;
                    decimal cumulativePercentage = totalValue > 0 ? (cumulativeValue / totalValue) * 100 : 0;

                    string category;
                    if (cumulativePercentage <= 80)
                    {
                        category = "A";
                        categoryA.Add($"{item.Item.Code}: {item.AnnualValue:c} ({(item.AnnualValue / totalValue * 100):n1}%)");
                    }
                    else if (cumulativePercentage <= 95)
                    {
                        category = "B";
                        categoryB.Add($"{item.Item.Code}: {item.AnnualValue:c} ({(item.AnnualValue / totalValue * 100):n1}%)");
                    }
                    else
                    {
                        category = "C";
                        categoryC.Add($"{item.Item.Code}: {item.AnnualValue:c} ({(item.AnnualValue / totalValue * 100):n1}%)");
                    }
                }

                var analysisResults = $"ABC Analysis Results (Based on Annual Usage Value):\n\n" +
                                    $"Total Items Analyzed: {sortedItems.Count}\n" +
                                    $"Total Annual Value: {totalValue:c}\n\n" +
                                    $"Category A Items (Top 80% of value): {categoryA.Count}\n" +
                                    string.Join("\n", categoryA.Take(10)) + 
                                    (categoryA.Count > 10 ? $"\n... and {categoryA.Count - 10} more" : "") + "\n\n" +
                                    $"Category B Items (Next 15% of value): {categoryB.Count}\n" +
                                    string.Join("\n", categoryB.Take(5)) + 
                                    (categoryB.Count > 5 ? $"\n... and {categoryB.Count - 5} more" : "") + "\n\n" +
                                    $"Category C Items (Remaining 5% of value): {categoryC.Count}\n" +
                                    string.Join("\n", categoryC.Take(5)) + 
                                    (categoryC.Count > 5 ? $"\n... and {categoryC.Count - 5} more" : "");

                Application.ShowViewStrategy.ShowMessage(analysisResults, InformationType.Info);
            }
            catch (Exception ex)
            {
                Application.ShowViewStrategy.ShowMessage(
                    $"Error performing ABC analysis: {ex.Message}",
                    InformationType.Error);
            }
        }

        protected override void OnDeactivated()
        {
            base.OnDeactivated();
        }
    }

    /// <summary>
    /// Simple parameter class for kardex report generation
    /// This would typically be a proper business object in a full implementation
    /// </summary>
    public class KardexReportParameter
    {
        public string ItemCode { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public string WarehouseCode { get; set; }
    }
}