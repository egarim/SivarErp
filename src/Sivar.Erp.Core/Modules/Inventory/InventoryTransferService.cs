using Microsoft.Extensions.Logging;
using Sivar.Erp.Core.Infrastructure.Logging;
using Sivar.Erp.Modules.Inventory.Reports;
using System.ComponentModel;
using Sivar.Erp.Core.Infrastructure.Repository;
using Sivar.Erp.Core.Modules.Inventory.Models;
using System.Diagnostics;

namespace Sivar.Erp.Core.Modules.Inventory
{
    /// <summary>
    /// Multi-location inventory transfer service implementation
    /// </summary>
    [Description("Multi-location inventory transfer service")]
    public class InventoryTransferService : IInventoryTransferService
    {
        private readonly IRepository _repository;
        private readonly IBusinessEntityRepository _businessEntityRepository;
        private readonly ILogger<InventoryTransferService> _logger;

        public InventoryTransferService(
            IRepository repository,
            IBusinessEntityRepository businessEntityRepository,
            ILogger<InventoryTransferService> logger)
        {
            _repository = repository;
            _businessEntityRepository = businessEntityRepository;
            _logger = logger;
        }

        public async Task<InventoryTransferRequest> CreateTransferRequestAsync(TransferRequestData request)
        {
            using var activity = LoggingExtensions.StartActivity("InventoryTransfer.CreateRequest");

            try
            {
                _logger.LogInformation("Creating transfer request for item {ItemCode} from {FromWarehouse} to {ToWarehouse}, quantity {Quantity}", 
                    request.ItemCode, request.FromWarehouse, request.ToWarehouse, request.Quantity);

                // Validate transfer request
                await ValidateTransferRequestAsync(request);

                // Create transfer request
                var transferRequest = new InventoryTransferRequest
                {
                    TransferId = Guid.NewGuid().ToString(),
                    RequestData = request,
                    Status = TransferStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = request.RequestedBy
                };

                // Check if auto-approval is enabled for this type of transfer
                var autoApproval = await CheckAutoApprovalRulesAsync(request);
                if (autoApproval.IsAutoApproved)
                {
                    transferRequest.Status = TransferStatus.Approved;
                    transferRequest.ApprovedAt = DateTime.UtcNow;
                    transferRequest.ApprovedBy = "SYSTEM";
                    transferRequest.ApprovalNotes = autoApproval.ApprovalReason;

                    // Add to approval history
                    transferRequest.ApprovalHistory.Add(new TransferApprovalHistory
                    {
                        ActionDate = DateTime.UtcNow,
                        ActionBy = "SYSTEM",
                        Action = "Auto-Approved",
                        Notes = autoApproval.ApprovalReason,
                        PreviousStatus = TransferStatus.Pending,
                        NewStatus = TransferStatus.Approved
                    });
                }

                // Save transfer request
                await SaveTransferRequestAsync(transferRequest);

                _logger.LogInformation("Created transfer request {TransferId} with status {Status}", 
                    transferRequest.TransferId, transferRequest.Status);

                return transferRequest;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating transfer request");
                throw;
            }
        }

        public async Task<bool> ApproveTransferRequestAsync(string transferId, string approvedBy, string notes = null)
        {
            using var activity = LoggingExtensions.StartActivity("InventoryTransfer.ApproveRequest");

            try
            {
                _logger.LogInformation("Approving transfer request {TransferId} by {ApprovedBy}", transferId, approvedBy);

                var transferRequest = await GetTransferRequestByIdAsync(transferId);
                if (transferRequest == null)
                {
                    _logger.LogWarning("Transfer request {TransferId} not found", transferId);
                    return false;
                }

                if (transferRequest.Status != TransferStatus.Pending)
                {
                    _logger.LogWarning("Transfer request {TransferId} is not in pending status. Current status: {Status}", 
                        transferId, transferRequest.Status);
                    return false;
                }

                // Validate that stock is still available
                var stockValidation = await ValidateStockAvailabilityAsync(transferRequest.RequestData);
                if (!stockValidation.IsValid)
                {
                    _logger.LogWarning("Stock validation failed for transfer {TransferId}: {ValidationMessage}", 
                        transferId, stockValidation.Message);
                    return false;
                }

                // Update transfer request
                transferRequest.Status = TransferStatus.Approved;
                transferRequest.ApprovedAt = DateTime.UtcNow;
                transferRequest.ApprovedBy = approvedBy;
                transferRequest.ApprovalNotes = notes;

                // Add to approval history
                transferRequest.ApprovalHistory.Add(new TransferApprovalHistory
                {
                    ActionDate = DateTime.UtcNow,
                    ActionBy = approvedBy,
                    Action = "Approved",
                    Notes = notes ?? "",
                    PreviousStatus = TransferStatus.Pending,
                    NewStatus = TransferStatus.Approved
                });

                // Save updated request
                await SaveTransferRequestAsync(transferRequest);

                _logger.LogInformation("Transfer request {TransferId} approved successfully", transferId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving transfer request {TransferId}", transferId);
                throw;
            }
        }

        public async Task<InventoryTransferExecution> ExecuteTransferAsync(string transferId, TransferExecutionData execution)
        {
            using var activity = LoggingExtensions.StartActivity("InventoryTransfer.ExecuteTransfer");
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("Executing transfer {TransferId} with actual quantity {ActualQuantity}", 
                    transferId, execution.ActualQuantity);

                var transferRequest = await GetTransferRequestByIdAsync(transferId);
                if (transferRequest == null)
                    throw new NotFoundException($"Transfer request {transferId} not found");

                if (transferRequest.Status != TransferStatus.Approved)
                    throw new InvalidOperationException($"Transfer {transferId} is not approved. Current status: {transferRequest.Status}");

                // Create execution record
                var transferExecution = new InventoryTransferExecution
                {
                    TransferId = transferId,
                    ExecutionId = Guid.NewGuid().ToString(),
                    ExecutionData = execution,
                    Status = TransferExecutionStatus.InProgress
                };

                // Execute transfer steps
                await ExecuteTransferStepsAsync(transferExecution, transferRequest);

                stopwatch.Stop();
                transferExecution.CompletedAt = DateTime.UtcNow;

                _logger.LogInformation("Transfer {TransferId} executed successfully in {Duration}ms", 
                    transferId, stopwatch.ElapsedMilliseconds);

                return transferExecution;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing transfer {TransferId}", transferId);
                throw;
            }
        }

        public async Task<bool> CancelTransferRequestAsync(string transferId, string cancelledBy, string reason)
        {
            using var activity = LoggingExtensions.StartActivity("InventoryTransfer.CancelRequest");

            try
            {
                _logger.LogInformation("Cancelling transfer request {TransferId} by {CancelledBy}", transferId, cancelledBy);

                var transferRequest = await GetTransferRequestByIdAsync(transferId);
                if (transferRequest == null)
                    return false;

                if (transferRequest.Status == TransferStatus.Completed || transferRequest.Status == TransferStatus.InTransit)
                {
                    _logger.LogWarning("Cannot cancel transfer {TransferId} in status {Status}", transferId, transferRequest.Status);
                    return false;
                }

                // Update status to cancelled
                var previousStatus = transferRequest.Status;
                transferRequest.Status = TransferStatus.Cancelled;

                // Add to approval history
                transferRequest.ApprovalHistory.Add(new TransferApprovalHistory
                {
                    ActionDate = DateTime.UtcNow,
                    ActionBy = cancelledBy,
                    Action = "Cancelled",
                    Notes = reason,
                    PreviousStatus = previousStatus,
                    NewStatus = TransferStatus.Cancelled
                });

                // Save updated request
                await SaveTransferRequestAsync(transferRequest);

                _logger.LogInformation("Transfer request {TransferId} cancelled successfully", transferId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling transfer request {TransferId}", transferId);
                throw;
            }
        }

        public async Task<InventoryTransferStatus> GetTransferStatusAsync(string transferId)
        {
            try
            {
                var transferRequest = await GetTransferRequestByIdAsync(transferId);
                if (transferRequest == null)
                    throw new NotFoundException($"Transfer {transferId} not found");

                var status = new InventoryTransferStatus
                {
                    TransferId = transferId,
                    Status = transferRequest.Status,
                    PercentComplete = CalculateTransferPercentComplete(transferRequest),
                    CurrentStep = GetCurrentTransferStep(transferRequest),
                    EstimatedCompletion = CalculateEstimatedCompletion(transferRequest)
                };

                // Get status history
                status.StatusHistory = transferRequest.ApprovalHistory.Select(h => new TransferStatusUpdate
                {
                    UpdateTime = h.ActionDate,
                    Status = h.NewStatus,
                    UpdatedBy = h.ActionBy,
                    Notes = h.Notes
                }).ToList();

                return status;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transfer status for {TransferId}", transferId);
                throw;
            }
        }

        public async Task<List<InventoryTransferHistory>> GetTransferHistoryAsync(TransferHistoryQuery query)
        {
            using var activity = LoggingExtensions.StartActivity("InventoryTransfer.GetHistory");

            try
            {
                _logger.LogInformation("Retrieving transfer history with query parameters");

                var transfers = await QueryTransferHistoryAsync(query);

                var history = transfers.Select(t => new InventoryTransferHistory
                {
                    TransferId = t.TransferId,
                    ItemCode = t.RequestData.ItemCode,
                    ItemName = GetItemName(t.RequestData.ItemCode),
                    FromWarehouse = t.RequestData.FromWarehouse,
                    ToWarehouse = t.RequestData.ToWarehouse,
                    Quantity = t.RequestData.Quantity,
                    RequestDate = t.CreatedAt,
                    CompletionDate = GetCompletionDate(t),
                    Status = t.Status,
                    RequestedBy = t.CreatedBy,
                    ExecutedBy = GetExecutedBy(t),
                    Reason = t.RequestData.Reason
                }).ToList();

                return history;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transfer history");
                throw;
            }
        }

        public async Task<List<PendingTransfer>> GetPendingTransfersAsync(PendingTransfersQuery query)
        {
            using var activity = LoggingExtensions.StartActivity("InventoryTransfer.GetPendingTransfers");

            try
            {
                var pendingTransfers = await QueryPendingTransfersAsync(query);

                var result = pendingTransfers.Select(t => new PendingTransfer
                {
                    TransferId = t.TransferId,
                    ItemCode = t.RequestData.ItemCode,
                    ItemName = GetItemName(t.RequestData.ItemCode),
                    FromWarehouse = t.RequestData.FromWarehouse,
                    ToWarehouse = t.RequestData.ToWarehouse,
                    Quantity = t.RequestData.Quantity,
                    RequestedDate = t.CreatedAt,
                    RequiredDate = t.RequestData.RequiredDate,
                    Priority = t.RequestData.Priority,
                    RequestedBy = t.CreatedBy,
                    IsOverdue = t.RequestData.RequiredDate < DateTime.Now,
                    DaysOutstanding = (DateTime.Now - t.CreatedAt).Days
                }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending transfers");
                throw;
            }
        }

        public async Task<BulkTransferResult> ProcessBulkTransferAsync(BulkTransferRequest request)
        {
            using var activity = LoggingExtensions.StartActivity("InventoryTransfer.ProcessBulkTransfer");

            try
            {
                _logger.LogInformation("Processing bulk transfer with {RequestCount} requests by {RequestedBy}", 
                    request.Transfers.Count, request.RequestedBy);

                var result = new BulkTransferResult
                {
                    BatchId = Guid.NewGuid().ToString(),
                    TotalRequests = request.Transfers.Count,
                    ProcessedAt = DateTime.UtcNow,
                    ProcessedBy = request.RequestedBy
                };

                // Validate all transfers if requested
                if (request.ValidateInventory)
                {
                    await ValidateBulkTransferRequestsAsync(request.Transfers, result);
                }

                // Process each transfer request
                for (int i = 0; i < request.Transfers.Count; i++)
                {
                    try
                    {
                        var transferRequest = await CreateTransferRequestAsync(request.Transfers[i]);
                        result.CreatedTransferIds.Add(transferRequest.TransferId);
                        result.SuccessfulRequests++;

                        // If immediate processing is requested and transfer is auto-approved
                        if (request.ProcessImmediately && transferRequest.Status == TransferStatus.Approved)
                        {
                            var execution = new TransferExecutionData
                            {
                                ActualQuantity = request.Transfers[i].Quantity,
                                ExecutedBy = request.RequestedBy,
                                ExecutionDate = DateTime.UtcNow,
                                TransportMethod = "Bulk Transfer"
                            };

                            await ExecuteTransferAsync(transferRequest.TransferId, execution);
                        }
                    }
                    catch (Exception ex)
                    {
                        result.FailedRequests++;
                        result.Errors.Add(new BulkTransferError
                        {
                            RequestIndex = i,
                            ItemCode = request.Transfers[i].ItemCode,
                            ErrorCode = "TRANSFER_CREATION_FAILED",
                            ErrorMessage = ex.Message
                        });

                        _logger.LogError(ex, "Failed to create transfer for item {ItemCode} at index {Index}", 
                            request.Transfers[i].ItemCode, i);
                    }
                }

                _logger.LogInformation("Bulk transfer processed: {SuccessfulRequests} successful, {FailedRequests} failed", 
                    result.SuccessfulRequests, result.FailedRequests);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing bulk transfer");
                throw;
            }
        }

        // Private helper methods
        private async Task ValidateTransferRequestAsync(TransferRequestData request)
        {
            // Validate item exists
            var item = await GetItemByCodeAsync(request.ItemCode);
            if (item == null)
                throw new ValidationException($"Item {request.ItemCode} not found");

            // Validate warehouses exist
            if (!await WarehouseExistsAsync(request.FromWarehouse))
                throw new ValidationException($"Source warehouse {request.FromWarehouse} not found");

            if (!await WarehouseExistsAsync(request.ToWarehouse))
                throw new ValidationException($"Destination warehouse {request.ToWarehouse} not found");

            // Validate quantity
            if (request.Quantity <= 0)
                throw new ValidationException("Transfer quantity must be positive");

            // Validate stock availability
            var stockValidation = await ValidateStockAvailabilityAsync(request);
            if (!stockValidation.IsValid)
                throw new ValidationException(stockValidation.Message);
        }

        private async Task<AutoApprovalResult> CheckAutoApprovalRulesAsync(TransferRequestData request)
        {
            // Check various auto-approval rules
            await Task.CompletedTask;

            // Example rules:
            // - Same company transfers under certain value
            // - Emergency transfers
            // - Regular stock balancing transfers
            
            return new AutoApprovalResult
            {
                IsAutoApproved = false,
                ApprovalReason = "Manual approval required"
            };
        }

        private async Task<StockValidationResult> ValidateStockAvailabilityAsync(TransferRequestData request)
        {
            // Check available stock in source warehouse
            await Task.CompletedTask;
            
            return new StockValidationResult
            {
                IsValid = true,
                Message = "Stock available"
            };
        }

        private async Task ExecuteTransferStepsAsync(InventoryTransferExecution execution, InventoryTransferRequest request)
        {
            // Step 1: Reserve inventory in source warehouse
            var reserveStep = new TransferExecutionStep
            {
                StepName = "Reserve Inventory",
                StartTime = DateTime.UtcNow,
                Status = "In Progress"
            };
            execution.ExecutionSteps.Add(reserveStep);

            try
            {
                // Reserve inventory logic here
                await ReserveInventoryForTransferAsync(request.RequestData, execution.ExecutionData.ActualQuantity);
                
                reserveStep.EndTime = DateTime.UtcNow;
                reserveStep.Status = "Completed";
                reserveStep.Notes = $"Reserved {execution.ExecutionData.ActualQuantity} units";
            }
            catch (Exception ex)
            {
                reserveStep.EndTime = DateTime.UtcNow;
                reserveStep.Status = "Failed";
                reserveStep.Notes = ex.Message;
                throw;
            }

            // Step 2: Issue from source warehouse
            var issueStep = new TransferExecutionStep
            {
                StepName = "Issue from Source",
                StartTime = DateTime.UtcNow,
                Status = "In Progress"
            };
            execution.ExecutionSteps.Add(issueStep);

            try
            {
                var issueTransactionId = await IssueInventoryForTransferAsync(request.RequestData, execution.ExecutionData.ActualQuantity);
                execution.SourceTransactionIds.Add(issueTransactionId);
                
                issueStep.EndTime = DateTime.UtcNow;
                issueStep.Status = "Completed";
                issueStep.StepData["TransactionId"] = issueTransactionId;
            }
            catch (Exception ex)
            {
                issueStep.EndTime = DateTime.UtcNow;
                issueStep.Status = "Failed";
                issueStep.Notes = ex.Message;
                throw;
            }

            // Step 3: Receive at destination warehouse
            var receiveStep = new TransferExecutionStep
            {
                StepName = "Receive at Destination",
                StartTime = DateTime.UtcNow,
                Status = "In Progress"
            };
            execution.ExecutionSteps.Add(receiveStep);

            try
            {
                var receiveTransactionId = await ReceiveInventoryForTransferAsync(request.RequestData, execution.ExecutionData.ActualQuantity);
                execution.DestinationTransactionIds.Add(receiveTransactionId);
                
                receiveStep.EndTime = DateTime.UtcNow;
                receiveStep.Status = "Completed";
                receiveStep.StepData["TransactionId"] = receiveTransactionId;
                
                execution.Status = TransferExecutionStatus.Completed;
            }
            catch (Exception ex)
            {
                receiveStep.EndTime = DateTime.UtcNow;
                receiveStep.Status = "Failed";
                receiveStep.Notes = ex.Message;
                execution.Status = TransferExecutionStatus.Failed;
                throw;
            }

            // Update transfer request status
            await UpdateTransferRequestStatusAsync(request.TransferId, TransferStatus.Completed);
        }

        private async Task SaveTransferRequestAsync(InventoryTransferRequest request)
        {
            // Save to repository
            await Task.CompletedTask;
        }

        private async Task<InventoryTransferRequest?> GetTransferRequestByIdAsync(string transferId)
        {
            // Get from repository
            await Task.CompletedTask;
            return null;
        }

        private async Task<List<InventoryTransferRequest>> QueryTransferHistoryAsync(TransferHistoryQuery query)
        {
            // Query transfer history from repository
            await Task.CompletedTask;
            return new List<InventoryTransferRequest>();
        }

        private async Task<List<InventoryTransferRequest>> QueryPendingTransfersAsync(PendingTransfersQuery query)
        {
            // Query pending transfers from repository
            await Task.CompletedTask;
            return new List<InventoryTransferRequest>();
        }

        private async Task ValidateBulkTransferRequestsAsync(List<TransferRequestData> transfers, BulkTransferResult result)
        {
            // Validate all transfer requests
            await Task.CompletedTask;
        }

        private async Task<object?> GetItemByCodeAsync(string itemCode)
        {
            // Get item from repository
            await Task.CompletedTask;
            return null;
        }

        private async Task<bool> WarehouseExistsAsync(string warehouseCode)
        {
            // Check if warehouse exists
            await Task.CompletedTask;
            return true;
        }

        private async Task ReserveInventoryForTransferAsync(TransferRequestData request, decimal quantity)
        {
            // Reserve inventory logic
            await Task.CompletedTask;
        }

        private async Task<string> IssueInventoryForTransferAsync(TransferRequestData request, decimal quantity)
        {
            // Issue inventory and return transaction ID
            await Task.CompletedTask;
            return Guid.NewGuid().ToString();
        }

        private async Task<string> ReceiveInventoryForTransferAsync(TransferRequestData request, decimal quantity)
        {
            // Receive inventory and return transaction ID
            await Task.CompletedTask;
            return Guid.NewGuid().ToString();
        }

        private async Task UpdateTransferRequestStatusAsync(string transferId, TransferStatus status)
        {
            // Update transfer request status
            await Task.CompletedTask;
        }

        private decimal CalculateTransferPercentComplete(InventoryTransferRequest request)
        {
            return request.Status switch
            {
                TransferStatus.Pending => 0m,
                TransferStatus.Approved => 25m,
                TransferStatus.InTransit => 75m,
                TransferStatus.Completed => 100m,
                TransferStatus.Cancelled => 0m,
                _ => 0m
            };
        }

        private string GetCurrentTransferStep(InventoryTransferRequest request)
        {
            return request.Status switch
            {
                TransferStatus.Pending => "Awaiting Approval",
                TransferStatus.Approved => "Ready for Execution",
                TransferStatus.InTransit => "In Transit",
                TransferStatus.Completed => "Completed",
                TransferStatus.Cancelled => "Cancelled",
                _ => "Unknown"
            };
        }

        private DateTime? CalculateEstimatedCompletion(InventoryTransferRequest request)
        {
            if (request.Status == TransferStatus.Completed || request.Status == TransferStatus.Cancelled)
                return null;

            // Calculate based on transfer type and distance
            return DateTime.UtcNow.AddDays(1); // Default 1 day
        }

        private string GetItemName(string itemCode)
        {
            // Get item name from cache or repository
            return $"Item {itemCode}";
        }

        private DateTime? GetCompletionDate(InventoryTransferRequest request)
        {
            return request.Status == TransferStatus.Completed 
                ? request.ApprovalHistory.LastOrDefault()?.ActionDate 
                : null;
        }

        private string? GetExecutedBy(InventoryTransferRequest request)
        {
            return request.ApprovalHistory
                .Where(h => h.Action.Contains("Execute") || h.NewStatus == TransferStatus.Completed)
                .LastOrDefault()?.ActionBy;
        }
    }

    // Supporting classes
    internal class AutoApprovalResult
    {
        public bool IsAutoApproved { get; set; }
        public string ApprovalReason { get; set; } = string.Empty;
    }

    internal class StockValidationResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
