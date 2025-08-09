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
    /// Controller for inventory reservation management
    /// Provides actions for creating, modifying, and managing inventory reservations
    /// </summary>
    public partial class ReservationViewController : ObjectViewController<DetailView, InventoryReservation>
    {
        private SimpleAction extendReservationAction;
        private SimpleAction fulfillReservationAction;
        private SimpleAction cancelReservationAction;
        private SimpleAction createReservationAction;

        public ReservationViewController()
        {
            InitializeActions();
        }

        private void InitializeActions()
        {
            // Extend reservation action
            extendReservationAction = new SimpleAction(this, "ExtendReservation", PredefinedCategory.Edit)
            {
                Caption = "Extend Reservation",
                ToolTip = "Extend the expiration time of this reservation",
                ImageName = "Action_Reload"
            };
            extendReservationAction.Execute += ExtendReservationAction_Execute;

            // Fulfill reservation action
            fulfillReservationAction = new SimpleAction(this, "FulfillReservation", PredefinedCategory.Edit)
            {
                Caption = "Fulfill Reservation",
                ToolTip = "Fulfill this reservation by creating an inventory transaction",
                ImageName = "Action_Grant"
            };
            fulfillReservationAction.Execute += FulfillReservationAction_Execute;

            // Cancel reservation action
            cancelReservationAction = new SimpleAction(this, "CancelReservation", PredefinedCategory.Edit)
            {
                Caption = "Cancel Reservation",
                ToolTip = "Cancel this reservation and release the reserved stock",
                ImageName = "Action_Cancel"
            };
            cancelReservationAction.Execute += CancelReservationAction_Execute;

            // Create reservation action (for item views)
            createReservationAction = new SimpleAction(this, "CreateReservation", PredefinedCategory.Edit)
            {
                Caption = "Create Reservation",
                ToolTip = "Create a new inventory reservation",
                ImageName = "Action_New"
            };
            createReservationAction.Execute += CreateReservationAction_Execute;
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
            var reservation = ViewCurrentObject;
            bool hasReservation = reservation != null;
            bool isActive = hasReservation && reservation.Status == Sivar.Erp.Modules.Inventory.ReservationStatus.Active;
            bool notExpired = hasReservation && !reservation.IsExpired;
            bool canModify = hasReservation && reservation.CanModify;

            extendReservationAction.Enabled["CanModify"] = canModify;
            fulfillReservationAction.Enabled["CanFulfill"] = isActive && notExpired;
            cancelReservationAction.Enabled["CanCancel"] = isActive;
            createReservationAction.Enabled["Always"] = true;
        }

        private void ExtendReservationAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var reservation = ViewCurrentObject;
            if (reservation?.CanModify != true)
                return;

            try
            {
                // Simple prompt for extension hours - using a choice action approach
                var choiceAction = new SingleChoiceAction(this, "ExtensionChoice", PredefinedCategory.Edit);
                choiceAction.Items.Add(new ChoiceActionItem("4Hours", "4 Hours"));
                choiceAction.Items.Add(new ChoiceActionItem("8Hours", "8 Hours"));
                choiceAction.Items.Add(new ChoiceActionItem("24Hours", "24 Hours"));

                // For simplicity, default to 4 hours extension
                int hours = 4;

                reservation.ExpiresAt = reservation.ExpiresAt.AddHours(hours);
                reservation.Notes = string.IsNullOrEmpty(reservation.Notes)
                    ? $"Extended by {hours} hours on {DateTime.UtcNow:yyyy-MM-dd HH:mm}"
                    : $"{reservation.Notes}\nExtended by {hours} hours on {DateTime.UtcNow:yyyy-MM-dd HH:mm}";

                Application.ShowViewStrategy.ShowMessage(
                    $"Reservation extended by {hours} hours. New expiration: {reservation.ExpiresAt:yyyy-MM-dd HH:mm}",
                    InformationType.Success);

                UpdateActionStates();
            }
            catch (Exception ex)
            {
                Application.ShowViewStrategy.ShowMessage(
                    $"Error extending reservation: {ex.Message}",
                    InformationType.Error);
            }
        }

        private void FulfillReservationAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var reservation = ViewCurrentObject;
            if (reservation?.Status != Sivar.Erp.Modules.Inventory.ReservationStatus.Active || reservation.IsExpired)
                return;

            try
            {
                // Create inventory transaction to fulfill the reservation
                var objectSpace = Application.CreateObjectSpace(typeof(InventoryTransaction));
                var transaction = objectSpace.CreateObject<InventoryTransaction>();
                
                var reservationInSpace = objectSpace.GetObject(reservation);
                transaction.Item = reservationInSpace.Item;
                transaction.Quantity = -reservationInSpace.Quantity; // Negative for issue
                transaction.TransactionType = Sivar.Erp.Modules.Inventory.InventoryTransactionType.ReservationFulfillment;
                transaction.SourceWarehouseCode = reservationInSpace.WarehouseCode;
                transaction.ReferenceDocumentNumber = reservationInSpace.SourceDocumentNumber;
                transaction.TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow);
                transaction.Notes = $"Fulfillment of reservation {reservationInSpace.ReservationId}";

                // Update reservation status
                reservationInSpace.Status = Sivar.Erp.Modules.Inventory.ReservationStatus.Fulfilled;
                reservationInSpace.Notes = string.IsNullOrEmpty(reservationInSpace.Notes)
                    ? $"Fulfilled on {DateTime.UtcNow:yyyy-MM-dd HH:mm}"
                    : $"{reservationInSpace.Notes}\nFulfilled on {DateTime.UtcNow:yyyy-MM-dd HH:mm}";

                // Show detailed view for the transaction
                var detailView = Application.CreateDetailView(objectSpace, transaction);
                detailView.Caption = $"Fulfill Reservation - {reservation.Item.Code}";
                
                Application.ShowViewStrategy.ShowView(
                    new ShowViewParameters(detailView)
                    {
                        Context = TemplateContext.PopupWindow,
                        TargetWindow = TargetWindow.NewModalWindow
                    },
                    new ShowViewSource(null, null));

                UpdateActionStates();
            }
            catch (Exception ex)
            {
                Application.ShowViewStrategy.ShowMessage(
                    $"Error fulfilling reservation: {ex.Message}",
                    InformationType.Error);
            }
        }

        private void CancelReservationAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var reservation = ViewCurrentObject;
            if (reservation?.Status != Sivar.Erp.Modules.Inventory.ReservationStatus.Active)
                return;

            try
            {
                // Simple confirmation - in a real implementation you might want a proper dialog
                reservation.Status = Sivar.Erp.Modules.Inventory.ReservationStatus.Cancelled;
                reservation.Notes = string.IsNullOrEmpty(reservation.Notes)
                    ? $"Cancelled on {DateTime.UtcNow:yyyy-MM-dd HH:mm}"
                    : $"{reservation.Notes}\nCancelled on {DateTime.UtcNow:yyyy-MM-dd HH:mm}";

                // Update stock level to release reserved quantity
                var stockLevel = reservation.StockLevel;
                if (stockLevel != null && reservation.Quantity.HasValue)
                {
                    stockLevel.QuantityReserved -= reservation.Quantity.Value;
                }

                Application.ShowViewStrategy.ShowMessage(
                    $"Reservation {reservation.ReservationId} has been cancelled",
                    InformationType.Success);

                UpdateActionStates();
            }
            catch (Exception ex)
            {
                Application.ShowViewStrategy.ShowMessage(
                    $"Error cancelling reservation: {ex.Message}",
                    InformationType.Error);
            }
        }

        private void CreateReservationAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            try
            {
                // Create a new reservation
                var objectSpace = Application.CreateObjectSpace(typeof(InventoryReservation));
                var reservation = objectSpace.CreateObject<InventoryReservation>();
                
                reservation.ExpiresAt = DateTime.UtcNow.AddDays(1); // Default 24 hours
                
                // Show detailed view for the new reservation
                var detailView = Application.CreateDetailView(objectSpace, reservation);
                detailView.Caption = "New Inventory Reservation";
                
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
                    $"Error creating reservation: {ex.Message}",
                    InformationType.Error);
            }
        }

        protected override void OnDeactivated()
        {
            base.OnDeactivated();
        }
    }

    /// <summary>
    /// Controller for managing reservations from inventory item views
    /// </summary>
    public partial class ItemReservationViewController : ObjectViewController<DetailView, InventoryItem>
    {
        private SimpleAction createItemReservationAction;
        private SimpleAction viewItemReservationsAction;

        public ItemReservationViewController()
        {
            InitializeActions();
        }

        private void InitializeActions()
        {
            // Create reservation for item action
            createItemReservationAction = new SimpleAction(this, "CreateItemReservation", PredefinedCategory.Edit)
            {
                Caption = "Reserve Stock",
                ToolTip = "Create a new reservation for this item",
                ImageName = "Action_New"
            };
            createItemReservationAction.Execute += CreateItemReservationAction_Execute;

            // View reservations for item action
            viewItemReservationsAction = new SimpleAction(this, "ViewItemReservations", PredefinedCategory.View)
            {
                Caption = "View Reservations",
                ToolTip = "View all reservations for this item",
                ImageName = "Action_Report"
            };
            viewItemReservationsAction.Execute += ViewItemReservationsAction_Execute;
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            UpdateActionStates();
        }

        private void UpdateActionStates()
        {
            var inventoryItem = ViewCurrentObject;
            bool hasItem = inventoryItem != null;
            bool isStockable = hasItem && inventoryItem.IsStockable;
            bool isActive = hasItem && inventoryItem.IsActive;

            createItemReservationAction.Enabled["HasItem"] = isStockable && isActive;
            viewItemReservationsAction.Enabled["HasItem"] = hasItem;
        }

        private void CreateItemReservationAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var inventoryItem = ViewCurrentObject;
            if (inventoryItem?.IsStockable != true || !inventoryItem.IsActive)
                return;

            try
            {
                // Create a new reservation for this item
                var objectSpace = Application.CreateObjectSpace(typeof(InventoryReservation));
                var reservation = objectSpace.CreateObject<InventoryReservation>();
                
                reservation.Item = objectSpace.GetObject(inventoryItem);
                reservation.ExpiresAt = DateTime.UtcNow.AddDays(1); // Default 24 hours
                
                // Show detailed view for the new reservation
                var detailView = Application.CreateDetailView(objectSpace, reservation);
                detailView.Caption = $"Reserve Stock - {inventoryItem.Code}";
                
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
                    $"Error creating reservation: {ex.Message}",
                    InformationType.Error);
            }
        }

        private void ViewItemReservationsAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var inventoryItem = ViewCurrentObject;
            if (inventoryItem == null)
                return;

            try
            {
                // Create a detail view showing the item with its reservations tab
                var objectSpace = Application.CreateObjectSpace(typeof(InventoryItem));
                var item = objectSpace.GetObject(inventoryItem);
                
                var detailView = Application.CreateDetailView(objectSpace, item);
                detailView.Caption = $"Reservations - {inventoryItem.Code} ({inventoryItem.Description})";

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
                    $"Error viewing reservations: {ex.Message}",
                    InformationType.Error);
            }
        }
    }
}