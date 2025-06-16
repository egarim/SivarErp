using Sivar.Erp.Documents;
using Sivar.Erp.Documents.DocumentToTransactions;
using Sivar.Erp.ErpSystem.ActivityStream;
using Sivar.Erp.ErpSystem.Options;
using Sivar.Erp.ErpSystem.Sequencers;
using Sivar.Erp.ErpSystem.Services;
using Sivar.Erp.ErpSystem.TimeService;
using Sivar.Erp.Services.Accounting.BalanceCalculators;
using Sivar.Erp.Services.Accounting.FiscalPeriods;
using Sivar.Erp.Services.Accounting.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sivar.Erp.Services.Accounting
{
    public class AccountingModule : ErpModuleBase
    {
        protected IFiscalPeriodService FiscalPeriodService;
        protected IAccountBalanceCalculator AccountBalanceCalculator;
     
   

        // Constants for transaction number sequence codes
        private const string TRANSACTION_SEQUENCE_CODE = "TRANS";
        private const string BATCH_SEQUENCE_CODE = "BATCH";

        public AccountingModule(
            IOptionService optionService, 
            IActivityStreamService activityStreamService, 
            IDateTimeZoneService dateTimeZoneService,
            IFiscalPeriodService fiscalPeriodService, 
            IAccountBalanceCalculator accountBalanceCalculator, 
          
            ISequencerService sequencerService) 
            : base(optionService, activityStreamService, dateTimeZoneService, sequencerService)
        {
            FiscalPeriodService = fiscalPeriodService;
            AccountBalanceCalculator = accountBalanceCalculator;
           
           
        }

        /// <summary>
        /// Posts a transaction after validating fiscal period is open
        /// </summary>
        /// <param name="transaction">Transaction to post</param>
        /// <returns>True if posted successfully, false otherwise</returns>
        /// <exception cref="InvalidOperationException">Thrown when fiscal period is closed</exception>
        public async Task<bool> PostTransactionAsync(ITransaction transaction)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));

            if (transaction.IsPosted)
                return true; // Already posted

            // Validate that transaction is in an open fiscal period
            var fiscalPeriod = await FiscalPeriodService.GetFiscalPeriodForDateAsync(transaction.TransactionDate);
            
            if (fiscalPeriod == null)
                throw new InvalidOperationException($"No fiscal period found for date {transaction.TransactionDate}");
                
            if (fiscalPeriod.Status == FiscalPeriodStatus.Closed)
                throw new InvalidOperationException($"Cannot post transaction: Fiscal period '{fiscalPeriod.Name}' is closed");

            // Validate transaction balance (debits = credits)
            bool isValid =await transaction.ValidateTransactionAsync();

            if (!isValid)
                throw new InvalidOperationException("Transaction has unbalanced debits and credits");

            // Generate transaction number if not already set
            if (string.IsNullOrEmpty(transaction.TransactionNumber))
            {
                transaction.TransactionNumber = await sequencerService.GetNextNumberAsync(TRANSACTION_SEQUENCE_CODE);
            }

            // Post the transaction
            transaction.Post();
            
            // Log the activity
            var systemActor = CreateSystemStreamObject();
            var transactionTarget = CreateStreamObject(
                "Transaction", 
                transaction.Oid.ToString(), 
                $"Transaction {transaction.TransactionNumber} on {transaction.TransactionDate}");
                
            await RecordActivityAsync(
                systemActor,
                "Posted",
                transactionTarget);
                
            return true;
        }

        /// <summary>
        /// Unposts a transaction if possible
        /// </summary>
        /// <param name="transaction">Transaction to unpost</param>
        /// <returns>True if unposted successfully, false otherwise</returns>
        /// <exception cref="InvalidOperationException">Thrown when fiscal period is closed</exception>
        public async Task<bool> UnPostTransactionAsync(ITransaction transaction)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));

            if (!transaction.IsPosted)
                return true; // Already unposted

            // Validate that transaction is in an open fiscal period
            var fiscalPeriod = await FiscalPeriodService.GetFiscalPeriodForDateAsync(transaction.TransactionDate);
            
            if (fiscalPeriod == null)
                throw new InvalidOperationException($"No fiscal period found for date {transaction.TransactionDate}");
                
            if (fiscalPeriod.Status == FiscalPeriodStatus.Closed)
                throw new InvalidOperationException($"Cannot unpost transaction: Fiscal period '{fiscalPeriod.Name}' is closed");

            // Unpost the transaction
            transaction.UnPost();
            
            // Log the activity
            var systemActor = CreateSystemStreamObject();
            var transactionTarget = CreateStreamObject(
                "Transaction", 
                transaction.Oid.ToString(), 
                $"Transaction {transaction.TransactionNumber} on {transaction.TransactionDate}");
                
            await RecordActivityAsync(
                systemActor,
                "Unposted",
                transactionTarget);
                
            return true;
        }

        /// <summary>
        /// Posts a transaction batch and all its transactions
        /// </summary>
        /// <param name="batch">Transaction batch to post</param>
        /// <returns>True if posted successfully, false otherwise</returns>
        /// <exception cref="InvalidOperationException">Thrown when fiscal period is closed</exception>
        public async Task<bool> PostTransactionBatchAsync(ITransactionBatch batch)
        {
            if (batch == null)
                throw new ArgumentNullException(nameof(batch));

            if (batch.IsPosted)
                return true; // Already posted

            // Validate that batch is in an open fiscal period
            var fiscalPeriod = await FiscalPeriodService.GetFiscalPeriodForDateAsync(batch.BatchDate);
            
            if (fiscalPeriod == null)
                throw new InvalidOperationException($"No fiscal period found for date {batch.BatchDate}");
                
            if (fiscalPeriod.Status == FiscalPeriodStatus.Closed)
                throw new InvalidOperationException($"Cannot post batch: Fiscal period '{fiscalPeriod.Name}' is closed");

            // Generate batch transaction number if not already set
            if (string.IsNullOrEmpty(batch.TransactionNumber))
            {
                batch.TransactionNumber = await sequencerService.GetNextNumberAsync(BATCH_SEQUENCE_CODE);
            }

            // Get all transactions in this batch
            // Assuming we can access transactions through a cast to TransactionBatchDto
            // In a real implementation, we would use a repository or other data access method
            var batchDto = batch as TransactionBatchDto;
            
            if (batchDto?.Transactions != null)
            {
                // Post each transaction in the batch
                foreach (var transaction in batchDto.Transactions)
                {
                    await PostTransactionAsync(transaction);
                }
            }

            // Post the batch itself
            batch.Post();
            
            // Update batch status
            batch.Status = BatchStatus.Processed;
            
            // Log the activity
            var systemActor = CreateSystemStreamObject();
            var batchTarget = CreateStreamObject(
                "TransactionBatch", 
                batch.Oid.ToString(), 
                $"Transaction batch {batch.TransactionNumber} '{batch.ReferenceCode}'");
                
            await RecordActivityAsync(
                systemActor,
                "Posted",
                batchTarget);
                
            return true;
        }

        /// <summary>
        /// Unposts a transaction batch and all its transactions
        /// </summary>
        /// <param name="batch">Transaction batch to unpost</param>
        /// <returns>True if unposted successfully, false otherwise</returns>
        /// <exception cref="InvalidOperationException">Thrown when fiscal period is closed</exception>
        public async Task<bool> UnPostTransactionBatchAsync(ITransactionBatch batch)
        {
            if (batch == null)
                throw new ArgumentNullException(nameof(batch));

            if (!batch.IsPosted)
                return true; // Already unposted

            // Validate that batch is in an open fiscal period
            var fiscalPeriod = await FiscalPeriodService.GetFiscalPeriodForDateAsync(batch.BatchDate);
            
            if (fiscalPeriod == null)
                throw new InvalidOperationException($"No fiscal period found for date {batch.BatchDate}");
                
            if (fiscalPeriod.Status == FiscalPeriodStatus.Closed)
                throw new InvalidOperationException($"Cannot unpost batch: Fiscal period '{fiscalPeriod.Name}' is closed");

            // Get all transactions in this batch
            // Assuming we can access transactions through a cast to TransactionBatchDto
            // In a real implementation, we would use a repository or other data access method
            var batchDto = batch as TransactionBatchDto;
            
            if (batchDto?.Transactions != null)
            {
                // Unpost each transaction in the batch
                foreach (var transaction in batchDto.Transactions)
                {
                    await UnPostTransactionAsync(transaction);
                }
            }

            // Unpost the batch itself
            batch.UnPost();
            
            // Update batch status
            batch.Status = BatchStatus.Approved;
            
            // Log the activity
            var systemActor = CreateSystemStreamObject();
            var batchTarget = CreateStreamObject(
                "TransactionBatch", 
                batch.Oid.ToString(), 
                $"Transaction batch {batch.TransactionNumber} '{batch.ReferenceCode}'");
                
            await RecordActivityAsync(
                systemActor,
                "Unposted",
                batchTarget);
                
            return true;
        }

        /// <summary>
        /// Checks if a date falls within an open fiscal period
        /// </summary>
        /// <param name="date">Date to check</param>
        /// <returns>True if date is within an open fiscal period, false otherwise</returns>
        public async Task<bool> IsDateInOpenFiscalPeriodAsync(DateOnly date)
        {
            var fiscalPeriod = await FiscalPeriodService.GetFiscalPeriodForDateAsync(date);
            return fiscalPeriod != null && fiscalPeriod.Status == FiscalPeriodStatus.Open;
        }

        public override void RegisterSequence(IEnumerable<SequenceDto> sequenceDtos)
        {
            SequenceDto sequence = new SequenceDto();
            sequence.Code = TRANSACTION_SEQUENCE_CODE;
            sequence.CurrentNumber = 1;
            sequence.Name = "Transactions";
            sequence.Prefix = "T";
            sequence.Suffix = "S";


            SequenceDto BatchSequence = new SequenceDto();
            BatchSequence.Code = BATCH_SEQUENCE_CODE;
            BatchSequence.CurrentNumber = 1;
            BatchSequence.Name = "Batch";
            BatchSequence.Prefix = "B";
            BatchSequence.Suffix = "S";
            
          

            this.sequencerService.CreateSequenceAsync(sequence);
            this.sequencerService.CreateSequenceAsync(BatchSequence);
        }
    }
}