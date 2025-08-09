using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.Persistent.Base;
using DevExpress.Data.Filtering;
using Microsoft.Extensions.Logging;
using Sivar.Erp.Documents;
using Sivar.Erp.Services.Documents;
using Sivar.Erp.Xaf.Module.BusinessObjects.Documents;
using System;

namespace Sivar.Erp.Xaf.Module.Controllers.Documents
{
    /// <summary>
    /// Controller for basic document operations and workflow management
    /// </summary>
    public class DocumentViewController : ViewController<DetailView>, IModelExtender
    {
        private readonly IDocumentTotalsService _documentTotalsService;
        private readonly ILogger<DocumentViewController> _logger;

        // Actions for document workflow
        private SimpleAction _approveDocumentAction;
        private SimpleAction _postDocumentAction;
        private SimpleAction _cancelDocumentAction;
        private SimpleAction _voidDocumentAction;
        private SimpleAction _generateAccountingTotalsAction;

        public DocumentViewController()
        {
            TargetObjectType = typeof(Document);
            TargetViewType = ViewType.DetailView;

            // Initialize actions
            InitializeActions();

            // Try to get services from DI container if available
            try
            {
                _documentTotalsService = Application?.ServiceProvider?.GetService(typeof(IDocumentTotalsService)) as IDocumentTotalsService;
                _logger = Application?.ServiceProvider?.GetService(typeof(ILogger<DocumentViewController>)) as ILogger<DocumentViewController>;
            }
            catch
            {
                // Services not available, will handle gracefully
            }
        }

        private void InitializeActions()
        {
            // Approve Document Action
            _approveDocumentAction = new SimpleAction(this, "ApproveDocument", PredefinedCategory.Edit)
            {
                Caption = "Approve",
                ToolTip = "Approve this document",
                ImageName = "CheckCircled",
                SelectionDependencyType = SelectionDependencyType.RequireSingleObject
            };
            _approveDocumentAction.Execute += ApproveDocumentAction_Execute;

            // Post Document Action
            _postDocumentAction = new SimpleAction(this, "PostDocument", PredefinedCategory.Edit)
            {
                Caption = "Post",
                ToolTip = "Post this document",
                ImageName = "CheckMark",
                SelectionDependencyType = SelectionDependencyType.RequireSingleObject
            };
            _postDocumentAction.Execute += PostDocumentAction_Execute;

            // Cancel Document Action
            _cancelDocumentAction = new SimpleAction(this, "CancelDocument", PredefinedCategory.Edit)
            {
                Caption = "Cancel",
                ToolTip = "Cancel this document",
                ImageName = "Cancel",
                SelectionDependencyType = SelectionDependencyType.RequireSingleObject
            };
            _cancelDocumentAction.Execute += CancelDocumentAction_Execute;

            // Void Document Action
            _voidDocumentAction = new SimpleAction(this, "VoidDocument", PredefinedCategory.Edit)
            {
                Caption = "Void",
                ToolTip = "Void this document",
                ImageName = "Delete",
                SelectionDependencyType = SelectionDependencyType.RequireSingleObject
            };
            _voidDocumentAction.Execute += VoidDocumentAction_Execute;

            // Generate Accounting Totals Action
            _generateAccountingTotalsAction = new SimpleAction(this, "GenerateAccountingTotals", PredefinedCategory.Tools)
            {
                Caption = "Generate Accounting Totals",
                ToolTip = "Generate accounting totals for this document",
                ImageName = "CalculateNow",
                SelectionDependencyType = SelectionDependencyType.RequireSingleObject
            };
            _generateAccountingTotalsAction.Execute += GenerateAccountingTotalsAction_Execute;
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
            if (e.Object is Document)
            {
                UpdateActionStates();
            }
        }

        private void UpdateActionStates()
        {
            var document = View.CurrentObject as Document;
            if (document == null)
            {
                SetAllActionsEnabled(false);
                return;
            }

            // Update action availability based on document status
            _approveDocumentAction.Enabled["DocumentStatus"] = document.Status == DocumentStatus.Draft;
            _postDocumentAction.Enabled["DocumentStatus"] = document.Status == DocumentStatus.Approved;
            _cancelDocumentAction.Enabled["DocumentStatus"] = 
                document.Status == DocumentStatus.Draft || 
                document.Status == DocumentStatus.PendingApproval ||
                document.Status == DocumentStatus.Approved;
            _voidDocumentAction.Enabled["DocumentStatus"] = document.Status == DocumentStatus.Posted;
            _generateAccountingTotalsAction.Enabled["DocumentStatus"] = 
                document.Status == DocumentStatus.Approved || 
                document.Status == DocumentStatus.Posted;

            // Additional validation - ensure document has lines
            var hasLines = document.Lines.Count > 0;
            _approveDocumentAction.Enabled["HasLines"] = hasLines;
            _postDocumentAction.Enabled["HasLines"] = hasLines;
            _generateAccountingTotalsAction.Enabled["HasLines"] = hasLines;
        }

        private void SetAllActionsEnabled(bool enabled)
        {
            _approveDocumentAction.Enabled.SetItemValue("General", enabled);
            _postDocumentAction.Enabled.SetItemValue("General", enabled);
            _cancelDocumentAction.Enabled.SetItemValue("General", enabled);
            _voidDocumentAction.Enabled.SetItemValue("General", enabled);
            _generateAccountingTotalsAction.Enabled.SetItemValue("General", enabled);
        }

        private void ApproveDocumentAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var document = e.CurrentObject as Document;
            if (document == null) return;

            try
            {
                // Validate document before approval
                if (!ValidateDocumentForApproval(document))
                    return;

                document.Status = DocumentStatus.Approved;
                ObjectSpace.CommitChanges();

                Application.ShowViewStrategy.ShowMessage(
                    $"Document {document.DocumentNumber} has been approved successfully.",
                    InformationType.Success);

                _logger?.LogInformation("Document {DocumentNumber} approved by {User}", 
                    document.DocumentNumber, SecuritySystem.CurrentUserName);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error approving document {DocumentNumber}", document.DocumentNumber);
                Application.ShowViewStrategy.ShowMessage(
                    $"Error approving document: {ex.Message}",
                    InformationType.Error);
            }
        }

        private void PostDocumentAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var document = e.CurrentObject as Document;
            if (document == null) return;

            try
            {
                // Validate document before posting
                if (!ValidateDocumentForPosting(document))
                    return;

                document.Status = DocumentStatus.Posted;
                ObjectSpace.CommitChanges();

                Application.ShowViewStrategy.ShowMessage(
                    $"Document {document.DocumentNumber} has been posted successfully.",
                    InformationType.Success);

                _logger?.LogInformation("Document {DocumentNumber} posted by {User}", 
                    document.DocumentNumber, SecuritySystem.CurrentUserName);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error posting document {DocumentNumber}", document.DocumentNumber);
                Application.ShowViewStrategy.ShowMessage(
                    $"Error posting document: {ex.Message}",
                    InformationType.Error);
            }
        }

        private void CancelDocumentAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var document = e.CurrentObject as Document;
            if (document == null) return;

            // Simple implementation - in real scenario, use proper dialog
            try
            {
                document.Status = DocumentStatus.Cancelled;
                ObjectSpace.CommitChanges();

                Application.ShowViewStrategy.ShowMessage(
                    $"Document {document.DocumentNumber} has been cancelled.",
                    InformationType.Success);

                _logger?.LogInformation("Document {DocumentNumber} cancelled by {User}", 
                    document.DocumentNumber, SecuritySystem.CurrentUserName);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error cancelling document {DocumentNumber}", document.DocumentNumber);
                Application.ShowViewStrategy.ShowMessage(
                    $"Error cancelling document: {ex.Message}",
                    InformationType.Error);
            }
        }

        private void VoidDocumentAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var document = e.CurrentObject as Document;
            if (document == null) return;

            // Simple implementation - in real scenario, use proper dialog
            try
            {
                document.Status = DocumentStatus.Voided;
                ObjectSpace.CommitChanges();

                Application.ShowViewStrategy.ShowMessage(
                    $"Document {document.DocumentNumber} has been voided.",
                    InformationType.Success);

                _logger?.LogInformation("Document {DocumentNumber} voided by {User}", 
                    document.DocumentNumber, SecuritySystem.CurrentUserName);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error voiding document {DocumentNumber}", document.DocumentNumber);
                Application.ShowViewStrategy.ShowMessage(
                    $"Error voiding document: {ex.Message}",
                    InformationType.Error);
            }
        }

        private void GenerateAccountingTotalsAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var document = e.CurrentObject as Document;
            if (document == null || _documentTotalsService == null) return;

            try
            {
                var documentOperation = document.DocumentType?.DocumentOperation.ToString();
                if (string.IsNullOrWhiteSpace(documentOperation))
                {
                    Application.ShowViewStrategy.ShowMessage(
                        "Document type does not specify a document operation.",
                        InformationType.Warning);
                    return;
                }

                var success = _documentTotalsService.AddDocumentAccountingTotals(document, documentOperation);
                
                if (success)
                {
                    ObjectSpace.CommitChanges();
                    Application.ShowViewStrategy.ShowMessage(
                        "Accounting totals generated successfully.",
                        InformationType.Success);

                    _logger?.LogInformation("Accounting totals generated for document {DocumentNumber}", 
                        document.DocumentNumber);
                }
                else
                {
                    Application.ShowViewStrategy.ShowMessage(
                        "Failed to generate accounting totals. Check accounting profile configuration.",
                        InformationType.Warning);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating accounting totals for document {DocumentNumber}", 
                    document.DocumentNumber);
                Application.ShowViewStrategy.ShowMessage(
                    $"Error generating accounting totals: {ex.Message}",
                    InformationType.Error);
            }
        }

        private bool ValidateDocumentForApproval(Document document)
        {
            if (document.Lines.Count == 0)
            {
                Application.ShowViewStrategy.ShowMessage(
                    "Document must have at least one line to be approved.",
                    InformationType.Warning);
                return false;
            }

            if (document.BusinessEntity == null)
            {
                Application.ShowViewStrategy.ShowMessage(
                    "Document must have a business entity assigned.",
                    InformationType.Warning);
                return false;
            }

            if (document.DocumentType == null)
            {
                Application.ShowViewStrategy.ShowMessage(
                    "Document must have a document type assigned.",
                    InformationType.Warning);
                return false;
            }

            return true;
        }

        private bool ValidateDocumentForPosting(Document document)
        {
            if (!ValidateDocumentForApproval(document))
                return false;

            // Additional validation for posting
            foreach (var line in document.Lines)
            {
                if (line.Quantity <= 0)
                {
                    Application.ShowViewStrategy.ShowMessage(
                        $"Line {line.LineNumber}: Quantity must be greater than zero.",
                        InformationType.Warning);
                    return false;
                }

                if (line.UnitPrice < 0)
                {
                    Application.ShowViewStrategy.ShowMessage(
                        $"Line {line.LineNumber}: Unit price cannot be negative.",
                        InformationType.Warning);
                    return false;
                }
            }

            return true;
        }

        public void ExtendModelInterfaces(ModelInterfaceExtenders extenders)
        {
            // Extend model if needed for custom properties
        }
    }
}