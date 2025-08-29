using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using Microsoft.Extensions.Logging;
using Sivar.Erp.EfCore.Entities;
using Sivar.Erp.ErpSystem.ActivityStream;
using Sivar.Erp.ErpSystem.Options;
using Sivar.Erp.ErpSystem.Sequencers;
using Sivar.Erp.ErpSystem.TimeService;
using Sivar.Erp.Modules.Accounting.Transactions;
using Sivar.Erp.Modules;
using Sivar.Erp.Xaf.Module.Services.Accounting;
using Sivar.Erp.Xaf.Module.Services.Sequencers;
using Sivar.Erp.Xaf.Module.Services.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sivar.Erp.Xaf.Module.Controllers
{
    public class TransactionController : ViewController
    {
        SimpleAction PostTransaction;
        public TransactionController() : base()
        {
            // Target required Views (use the TargetXXX properties) and create their Actions.
            this.TargetObjectType = typeof(Transaction);

            PostTransaction = new SimpleAction(this, "PostTransaction", "View");
            PostTransaction.Caption = "Post Transaction";
            PostTransaction.Execute += PostTransaction_Execute;
            
        }
        private async void PostTransaction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var currentTransaction = this.View.CurrentObject as Transaction;
            
            if (currentTransaction == null)
            {
                throw new InvalidOperationException("No transaction selected for posting");
            }

            try
            {
                // Create the XAF Accounting Module with required services
                var accountingModule = CreateXafAccountingModule();
                
                // Convert XAF Transaction entity to ITransaction DTO
                var transactionDto = ConvertToTransactionDto(currentTransaction);
                
                // Post the transaction using the accounting module
                bool posted = await accountingModule.PostTransactionAsync(transactionDto);
                
                if (posted)
                {
                    // Update the XAF entity to reflect the posted status
                    currentTransaction.IsPosted = true;
                    this.ObjectSpace.CommitChanges();
                    
                    // Show success message
                    Application.ShowViewStrategy.ShowMessage(
                        $"Transaction {currentTransaction.TransactionNumber} posted successfully!", 
                        InformationType.Success);
                }
                else
                {
                    Application.ShowViewStrategy.ShowMessage(
                        "Failed to post transaction. Please check the transaction details.", 
                        InformationType.Error);
                }
            }
            catch (Exception ex)
            {
                // Show error message
                Application.ShowViewStrategy.ShowMessage(
                    $"Error posting transaction: {ex.Message}", 
                    InformationType.Error);
            }
        }
        
        /// <summary>
        /// Creates a configured XafAccountingModule with all required services
        /// </summary>
        /// <returns>Configured XafAccountingModule instance</returns>
        private XafAccountingModule CreateXafAccountingModule()
        {
            // Create required services using XAF implementations
            var optionService = new OptionService();
            var dateTimeZoneService = new Sivar.Erp.ErpSystem.TimeService.DateTimeZoneService();
            var activityStreamService = new XafActivityStreamService(this.ObjectSpace);
            var sequencerService = new XafSequencerService(this.ObjectSpace);
            
            // Create and return the XAF accounting module
            return new XafAccountingModule(
                this.ObjectSpace,
                optionService,
                activityStreamService,
                dateTimeZoneService,
                sequencerService);
        }
        
        /// <summary>
        /// Converts XAF Transaction entity to ITransaction DTO
        /// </summary>
        /// <param name="xafTransaction">XAF Transaction entity</param>
        /// <returns>ITransaction DTO</returns>
        private ITransaction ConvertToTransactionDto(Transaction xafTransaction)
        {
            var transactionDto = new TransactionDto
            {
                TransactionNumber = xafTransaction.TransactionNumber,
                TransactionDate = xafTransaction.TransactionDate,
                Description = xafTransaction.Description ?? string.Empty,
                DocumentNumber = xafTransaction.DocumentNumber ?? string.Empty,
                IsPosted = xafTransaction.IsPosted,
                LedgerEntries = new List<ILedgerEntry>()
            };
            
            // Convert associated ledger entries
            if (xafTransaction.LedgerEntries != null)
            {
                var ledgerEntries = new List<ILedgerEntry>();
                
                foreach (var xafLedgerEntry in xafTransaction.LedgerEntries)
                {
                    var ledgerEntryDto = new LedgerEntryDto
                    {
                        LedgerEntryNumber = xafLedgerEntry.LedgerEntryNumber,
                        TransactionNumber = xafLedgerEntry.TransactionNumber,
                        OfficialCode = xafLedgerEntry.OfficialCode ?? string.Empty,
                        AccountName = xafLedgerEntry.AccountName ?? string.Empty,
                        EntryType = xafLedgerEntry.EntryType,
                        Amount = xafLedgerEntry.Amount
                    };
                    
                    ledgerEntries.Add(ledgerEntryDto);
                }
                
                transactionDto.LedgerEntries = ledgerEntries;
            }
            
            return transactionDto;
        }
        protected override void OnActivated()
        {
            base.OnActivated();
            // Perform various tasks depending on the target View.
        }
        protected override void OnDeactivated()
        {
            // Unsubscribe from previously subscribed events and release other references and resources.
            base.OnDeactivated();
        }
        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();
            // Access and customize the target View control.
        }
    }
}
