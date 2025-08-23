using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Sivar.Erp.Core.Core;
using Sivar.Erp.Core.Modules.Accounting;
using Sivar.Erp.Core.Modules.DataImport;
using Sivar.Erp.Core.Modules.Taxes;
using Sivar.Erp.Core.Modules.Domain.Models;

namespace Sivar.Erp.Core.Infrastructure.AI
{
    /// <summary>
    /// Implementation of ERP AI service that exposes core ERP operations to AI agents
    /// </summary>
    [Description("Implementation of ERP AI service")]
    public class ErpAiService : IErpAiService
    {
        private readonly IRepository _repository;
        private readonly IAccountingService _accountingService;
        private readonly ITaxService _taxService;
        private readonly IDataImportService _dataImportService;
        private readonly ILogger<ErpAiService> _logger;
        private readonly Dictionary<string, Func<AiOperationRequest, Task<object>>> _operations;

        /// <summary>
        /// Initializes the ERP AI service with required dependencies
        /// </summary>
        public ErpAiService(
            IRepository repository,
            IAccountingService accountingService,
            ITaxService taxService,
            IDataImportService dataImportService,
            ILogger<ErpAiService> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _accountingService = accountingService ?? throw new ArgumentNullException(nameof(accountingService));
            _taxService = taxService ?? throw new ArgumentNullException(nameof(taxService));
            _dataImportService = dataImportService ?? throw new ArgumentNullException(nameof(dataImportService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _operations = InitializeOperations();
        }

        /// <summary>
        /// Gets all available operations that AI agents can execute
        /// </summary>
        [Description("Gets available operations for AI agents")]
        public async Task<IEnumerable<AiOperation>> GetAvailableOperationsAsync()
        {
            var operations = new List<AiOperation>
            {
                // Data Import Operations
                new AiOperation
                {
                    Name = "ImportTestData",
                    Description = "Imports test data from embedded resources",
                    Category = "DataImport",
                    ReturnType = "DataImportResult",
                    Parameters = new List<AiOperationParameter>
                    {
                        new() { Name = "dataSet", Type = "string", Description = "Test data set name", Required = false, DefaultValue = "ElSalvador" },
                        new() { Name = "userName", Type = "string", Description = "User performing import", Required = false, DefaultValue = "AI Agent" }
                    }
                },

                // Accounting Operations
                new AiOperation
                {
                    Name = "GetAccountBalance",
                    Description = "Gets the current balance for an account",
                    Category = "Accounting",
                    ReturnType = "decimal",
                    Parameters = new List<AiOperationParameter>
                    {
                        new() { Name = "accountCode", Type = "string", Description = "Account code to query", Required = true }
                    }
                },

                new AiOperation
                {
                    Name = "GetChartOfAccounts",
                    Description = "Gets the complete chart of accounts",
                    Category = "Accounting",
                    ReturnType = "List<IAccount>",
                    Parameters = new List<AiOperationParameter>()
                },

                // Tax Operations
                new AiOperation
                {
                    Name = "GetActiveTaxes",
                    Description = "Gets all active tax definitions",
                    Category = "Tax",
                    ReturnType = "List<ITax>",
                    Parameters = new List<AiOperationParameter>()
                },

                // Repository Operations
                new AiOperation
                {
                    Name = "GetEntityCount",
                    Description = "Gets the count of entities for a specific type",
                    Category = "Repository",
                    ReturnType = "int",
                    Parameters = new List<AiOperationParameter>
                    {
                        new() { Name = "entityType", Type = "string", Description = "Entity type name", Required = true }
                    }
                },

                // System Operations
                new AiOperation
                {
                    Name = "GetSystemStatus",
                    Description = "Gets current system status and health",
                    Category = "System",
                    ReturnType = "SystemStatus",
                    Parameters = new List<AiOperationParameter>()
                }
            };

            return await Task.FromResult(operations);
        }

        /// <summary>
        /// Gets operations filtered by category
        /// </summary>
        [Description("Gets operations filtered by category")]
        public async Task<IEnumerable<AiOperation>> GetOperationsByCategoryAsync(string category)
        {
            var allOperations = await GetAvailableOperationsAsync();
            return allOperations.Where(op => string.Equals(op.Category, category, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Executes an operation requested by an AI agent
        /// </summary>
        [Description("Executes an operation requested by an AI agent")]
        public async Task<AiOperationResult> ExecuteOperationAsync(AiOperationRequest request)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                _logger.LogInformation("Executing AI operation {OperationName} for session {SessionId}", 
                    request.OperationName, request.SessionId);

                // Validate operation exists
                if (!_operations.TryGetValue(request.OperationName, out var operation))
                {
                    return new AiOperationResult
                    {
                        Success = false,
                        ErrorMessage = $"Operation '{request.OperationName}' not found",
                        Duration = stopwatch.Elapsed
                    };
                }

                // Execute operation
                var result = await operation(request);

                return new AiOperationResult
                {
                    Success = true,
                    Data = result,
                    Duration = stopwatch.Elapsed,
                    Metadata = new Dictionary<string, object>
                    {
                        ["ExecutedAt"] = DateTime.UtcNow,
                        ["SessionId"] = request.SessionId,
                        ["UserContext"] = request.UserContext
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute AI operation {OperationName}", request.OperationName);
                
                return new AiOperationResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    Duration = stopwatch.Elapsed
                };
            }
        }

        /// <summary>
        /// Validates an operation request before execution
        /// </summary>
        [Description("Validates an operation request before execution")]
        public async Task<AiOperationResult> ValidateOperationAsync(AiOperationRequest request)
        {
            var operations = await GetAvailableOperationsAsync();
            var operation = operations.FirstOrDefault(op => op.Name == request.OperationName);

            if (operation == null)
            {
                return new AiOperationResult
                {
                    Success = false,
                    ErrorMessage = $"Operation '{request.OperationName}' not found"
                };
            }

            // Validate required parameters
            var missingParams = operation.Parameters
                .Where(p => p.Required && !request.Parameters.ContainsKey(p.Name))
                .Select(p => p.Name)
                .ToList();

            if (missingParams.Any())
            {
                return new AiOperationResult
                {
                    Success = false,
                    ErrorMessage = $"Missing required parameters: {string.Join(", ", missingParams)}"
                };
            }

            return new AiOperationResult { Success = true };
        }

        /// <summary>
        /// Gets operation schema for a specific operation
        /// </summary>
        [Description("Gets operation schema for a specific operation")]
        public async Task<AiOperation?> GetOperationSchemaAsync(string operationName)
        {
            var operations = await GetAvailableOperationsAsync();
            return operations.FirstOrDefault(op => op.Name == operationName);
        }

        /// <summary>
        /// Gets AI capabilities and limitations
        /// </summary>
        [Description("Gets AI capabilities and limitations")]
        public async Task<AiCapabilities> GetCapabilitiesAsync()
        {
            return await Task.FromResult(new AiCapabilities
            {
                SupportedCategories = new List<string> { "DataImport", "Accounting", "Tax", "Repository", "System" },
                MaxOperationsPerSession = 100,
                RateLimit = new RateLimitInfo
                {
                    RequestsPerMinute = 60,
                    RequestsPerHour = 1000,
                    BurstLimit = 10
                },
                SecurityFeatures = new List<string> { "RequestValidation", "ParameterSanitization", "SessionTracking" }
            });
        }

        /// <summary>
        /// Initializes the available operations
        /// </summary>
        private Dictionary<string, Func<AiOperationRequest, Task<object>>> InitializeOperations()
        {
            return new Dictionary<string, Func<AiOperationRequest, Task<object>>>
            {
                ["ImportTestData"] = async request =>
                {
                    var dataSet = request.Parameters.GetValueOrDefault("dataSet", "ElSalvador")?.ToString() ?? "ElSalvador";
                    var userName = request.Parameters.GetValueOrDefault("userName", "AI Agent")?.ToString() ?? "AI Agent";
                    return await _dataImportService.ImportTestDataSetAsync(dataSet, userName);
                },

                ["GetAccountBalance"] = async request =>
                {
                    var accountCode = request.Parameters["accountCode"]?.ToString() ?? throw new ArgumentException("accountCode required");
                    // Note: This would need to be implemented in IAccountingService
                    return $"Balance for account {accountCode}: Not implemented yet";
                },

                ["GetChartOfAccounts"] = async request =>
                {
                    // Note: This would need to be implemented in IAccountingService  
                    return "Chart of accounts: Not implemented yet";
                },

                ["GetActiveTaxes"] = async request =>
                {
                    return await _taxService.GetActiveTaxesAsync();
                },

                ["GetEntityCount"] = async request =>
                {
                    var entityType = request.Parameters["entityType"]?.ToString() ?? throw new ArgumentException("entityType required");
                    return GetEntityCountByType(entityType);
                },

                ["GetSystemStatus"] = async request =>
                {
                    return await GetSystemStatusAsync();
                }
            };
        }

        /// <summary>
        /// Gets entity count by type name
        /// </summary>
        private int GetEntityCountByType(string entityType)
        {
            return entityType.ToLowerInvariant() switch
            {
                "account" => _repository.GetObjects<AccountDto>().Count(),
                "tax" => _repository.GetObjects<TaxDto>().Count(),
                "businessentity" => _repository.GetObjects<BusinessEntityDto>().Count(),
                "item" => _repository.GetObjects<ItemDto>().Count(),
                "transaction" => _repository.GetObjects<TransactionDto>().Count(),
                _ => throw new ArgumentException($"Unknown entity type: {entityType}")
            };
        }

        /// <summary>
        /// Gets current system status
        /// </summary>
        private async Task<object> GetSystemStatusAsync()
        {
            return await Task.FromResult(new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                EntityCounts = new
                {
                    Accounts = _repository.GetObjects<AccountDto>().Count(),
                    Taxes = _repository.GetObjects<TaxDto>().Count(),
                    BusinessEntities = _repository.GetObjects<BusinessEntityDto>().Count(),
                    Items = _repository.GetObjects<ItemDto>().Count(),
                    Transactions = _repository.GetObjects<TransactionDto>().Count()
                },
                Memory = GC.GetTotalMemory(false),
                Uptime = Environment.TickCount64
            });
        }
    }
}
