using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.Persistent.Base;
using DevExpress.Data.Filtering;
using Microsoft.Extensions.Logging;
using Sivar.Erp.Services.Taxes;
using Sivar.Erp.Xaf.Module.BusinessObjects.Documents;
using Sivar.Erp.Xaf.Module.BusinessObjects.Taxes;
using System;
using System.Linq;

namespace Sivar.Erp.Xaf.Module.Controllers.Documents
{
    /// <summary>
    /// Controller for real-time document totals calculation and tax management
    /// </summary>
    [Appearance("DocumentTotalsController_ReadOnlyCalculatedFields", 
        TargetItems = "Subtotal;Total", 
        Enabled = false, 
        Context = "DetailView")]
    public class DocumentTotalsController : ViewController<DetailView>, IModelExtender
    {
        private readonly ILogger<DocumentTotalsController> _logger;
        private bool _isCalculating = false;

        public DocumentTotalsController()
        {
            TargetObjectType = typeof(Document);
            TargetViewType = ViewType.DetailView;

            // Try to get logger from DI container if available
            try
            {
                _logger = Application?.ServiceProvider?.GetService(typeof(ILogger<DocumentTotalsController>)) as ILogger<DocumentTotalsController>;
            }
            catch
            {
                // Logger not available, will handle gracefully
            }
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            
            // Subscribe to object space events for real-time calculations
            ObjectSpace.ObjectChanged += ObjectSpace_ObjectChanged;
            ObjectSpace.ObjectDeleted += ObjectSpace_ObjectDeleted;
            
            // Subscribe to nested list view events for line management
            if (View is DetailView detailView)
            {
                foreach (var item in detailView.Items.OfType<ListPropertyEditor>())
                {
                    if (item.MemberInfo.Name == nameof(Document.Lines))
                    {
                        item.ControlCreated += LinesListPropertyEditor_ControlCreated;
                    }
                }
            }
        }

        protected override void OnDeactivated()
        {
            ObjectSpace.ObjectChanged -= ObjectSpace_ObjectChanged;
            ObjectSpace.ObjectDeleted -= ObjectSpace_ObjectDeleted;
            base.OnDeactivated();
        }

        private void LinesListPropertyEditor_ControlCreated(object sender, EventArgs e)
        {
            if (sender is ListPropertyEditor listPropertyEditor && listPropertyEditor.ListView != null)
            {
                listPropertyEditor.ListView.ObjectSpace.ObjectChanged += NestedObjectSpace_ObjectChanged;
                listPropertyEditor.ListView.ObjectSpace.ObjectDeleted += NestedObjectSpace_ObjectDeleted;
            }
        }

        private void ObjectSpace_ObjectChanged(object sender, ObjectChangedEventArgs e)
        {
            HandleObjectChanged(e.Object, e.PropertyName, e.OldValue, e.NewValue);
        }

        private void NestedObjectSpace_ObjectChanged(object sender, ObjectChangedEventArgs e)
        {
            HandleObjectChanged(e.Object, e.PropertyName, e.OldValue, e.NewValue);
        }

        private void ObjectSpace_ObjectDeleted(object sender, ObjectsManipulatingEventArgs e)
        {
            foreach (var obj in e.Objects)
            {
                if (obj is DocumentLine line && line.Document != null)
                {
                    RecalculateDocumentTotals(line.Document);
                }
                else if (obj is Total total && total.Document != null)
                {
                    RecalculateDocumentTotals(total.Document);
                }
            }
        }

        private void NestedObjectSpace_ObjectDeleted(object sender, ObjectsManipulatingEventArgs e)
        {
            ObjectSpace_ObjectDeleted(sender, e);
        }

        private void HandleObjectChanged(object obj, string propertyName, object oldValue, object newValue)
        {
            if (_isCalculating) return; // Prevent recursive calculations

            try
            {
                switch (obj)
                {
                    case Document document:
                        HandleDocumentChanged(document, propertyName);
                        break;
                    
                    case DocumentLine line:
                        HandleDocumentLineChanged(line, propertyName, oldValue, newValue);
                        break;
                    
                    case Total total:
                        HandleTotalChanged(total, propertyName);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error in document totals calculation for property {PropertyName}", propertyName);
            }
        }

        private void HandleDocumentChanged(Document document, string propertyName)
        {
            // Handle document-level changes that might affect calculations
            if (propertyName == nameof(Document.DocumentType))
            {
                // Document type changed - might affect tax calculations
                RecalculateAllTaxes(document);
            }
        }

        private void HandleDocumentLineChanged(DocumentLine line, string propertyName, object oldValue, object newValue)
        {
            if (line.Document == null) return;

            switch (propertyName)
            {
                case nameof(DocumentLine.Quantity):
                case nameof(DocumentLine.UnitPrice):
                    // Quantity or unit price changed - recalculate line amount
                    RecalculateLineAmount(line);
                    RecalculateLineTaxes(line);
                    RecalculateDocumentTotals(line.Document);
                    break;

                case nameof(DocumentLine.Item):
                    // Item changed - update unit price and recalculate
                    HandleItemChanged(line, oldValue as Item, newValue as Item);
                    break;

                case nameof(DocumentLine.Taxes):
                    // Tax assignments changed - recalculate taxes
                    RecalculateLineTaxes(line);
                    RecalculateDocumentTotals(line.Document);
                    break;
            }
        }

        private void HandleTotalChanged(Total total, string propertyName)
        {
            if (total.Document == null) return;

            if (propertyName == nameof(Total.Amount))
            {
                // Manual total amount change - recalculate document totals
                RecalculateDocumentTotals(total.Document);
            }
        }

        private void RecalculateLineAmount(DocumentLine line)
        {
            if (_isCalculating) return;

            _isCalculating = true;
            try
            {
                var newAmount = line.Quantity * line.UnitPrice;
                if (line.Amount != newAmount)
                {
                    line.Amount = newAmount;
                    _logger?.LogDebug("Recalculated line {LineNumber} amount: {Amount}", 
                        line.LineNumber, newAmount);
                }
            }
            finally
            {
                _isCalculating = false;
            }
        }

        private void HandleItemChanged(DocumentLine line, Item oldItem, Item newItem)
        {
            if (_isCalculating) return;

            _isCalculating = true;
            try
            {
                if (newItem != null)
                {
                    // Update description and unit price from item
                    if (string.IsNullOrWhiteSpace(line.Description))
                    {
                        line.Description = newItem.Description;
                    }

                    if (line.UnitPrice == 0 && newItem.BasePrice > 0)
                    {
                        line.UnitPrice = newItem.BasePrice;
                    }

                    // Auto-assign item's default taxes if line has no taxes
                    if (line.Taxes.Count == 0)
                    {
                        AssignDefaultTaxesToLine(line, newItem);
                    }

                    _logger?.LogDebug("Updated line {LineNumber} from item {ItemCode}", 
                        line.LineNumber, newItem.Code);
                }

                // Recalculate amount regardless
                RecalculateLineAmount(line);
            }
            finally
            {
                _isCalculating = false;
            }
        }

        private void AssignDefaultTaxesToLine(DocumentLine line, Item item)
        {
            // This would typically come from item's tax configuration
            // For now, we'll use a simple logic to assign VAT if item is sellable
            if (item.IsSellable)
            {
                var vatTax = line.Session.FindObject<Tax>(
                    CriteriaOperator.Parse("Code = ? AND TaxType = ?", "VAT", TaxType.Percentage));
                
                if (vatTax != null && !line.Taxes.Contains(vatTax))
                {
                    line.Taxes.Add(vatTax);
                    _logger?.LogDebug("Auto-assigned VAT tax to line {LineNumber}", line.LineNumber);
                }
            }
        }

        private void RecalculateLineTaxes(DocumentLine line)
        {
            if (_isCalculating) return;

            _isCalculating = true;
            try
            {
                // Remove existing tax totals for this line
                var existingTaxTotals = line.LineTotals.Where(t => t.Concept.StartsWith("Tax")).ToList();
                foreach (var taxTotal in existingTaxTotals)
                {
                    line.Session.Delete(taxTotal);
                }

                // Calculate new tax totals
                foreach (var tax in line.Taxes)
                {
                    if (tax.ApplicationLevel != TaxApplicationLevel.Line)
                        continue;

                    var taxAmount = CalculateTaxAmount(line.Amount, tax);
                    if (taxAmount > 0)
                    {
                        var taxTotal = new Total(line.Session);
                        taxTotal.Concept = $"Tax - {tax.Name}";
                        taxTotal.Amount = taxAmount;
                        taxTotal.DocumentLine = line;
                        taxTotal.IncludeInTransaction = true;

                        _logger?.LogDebug("Calculated {TaxName} for line {LineNumber}: {Amount}", 
                            tax.Name, line.LineNumber, taxAmount);
                    }
                }
            }
            finally
            {
                _isCalculating = false;
            }
        }

        private void RecalculateAllTaxes(Document document)
        {
            foreach (var line in document.Lines)
            {
                RecalculateLineTaxes(line);
            }
            RecalculateDocumentTotals(document);
        }

        private void RecalculateDocumentTotals(Document document)
        {
            if (_isCalculating) return;

            try
            {
                // Force refresh of calculated properties
                document.Session.Reload(document);
                
                // Trigger view refresh to update calculated fields
                if (View?.CurrentObject == document)
                {
                    View.RefreshDataSource();
                }

                _logger?.LogDebug("Recalculated totals for document {DocumentNumber}: Subtotal={Subtotal}, Total={Total}", 
                    document.DocumentNumber, document.Subtotal, document.Total);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error recalculating document totals for {DocumentNumber}", 
                    document.DocumentNumber);
            }
        }

        private decimal CalculateTaxAmount(decimal baseAmount, Tax tax)
        {
            switch (tax.TaxType)
            {
                case TaxType.Percentage:
                    return baseAmount * (tax.Percentage / 100m);
                
                case TaxType.FixedAmount:
                    return tax.Amount;
                
                case TaxType.AmountPerUnit:
                    // This would require quantity information
                    return 0; // Simplified for now
                
                default:
                    return 0;
            }
        }

        public void ExtendModelInterfaces(ModelInterfaceExtenders extenders)
        {
            // Extend model for conditional appearance if needed
        }
    }
}