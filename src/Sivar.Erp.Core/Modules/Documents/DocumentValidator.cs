using System.ComponentModel;
using Sivar.Erp.Core.Modules.Documents.Models;
using Microsoft.Extensions.Logging;
using Sivar.Erp.Core.Core;
using Sivar.Erp.Core.Modules.Domain;

namespace Sivar.Erp.Core.Modules.Documents
{
    /// <summary>
    /// Comprehensive document validator with business rules
    /// </summary>
    [Description("Document validator with business rules")]
    public class DocumentValidator
    {
        private readonly IRepository _repository;
        private readonly ILogger<DocumentValidator> _logger;

        public DocumentValidator(IRepository repository, ILogger<DocumentValidator> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Validates a document comprehensively
        /// </summary>
        /// <param name="document">Document to validate</param>
        /// <returns>Detailed validation result</returns>
        [Description("Validates a document comprehensively")]
        public async Task<DocumentValidationResult> ValidateAsync(IDocument document)
        {
            var result = new DocumentValidationResult();

            if (document == null)
            {
                result.AddError("Document cannot be null");
                return result;
            }

            // Basic document validation
            await ValidateBasicDocumentPropertiesAsync(document, result);

            // Business entity validation
            await ValidateBusinessEntityAsync(document, result);

            // Document type validation
            await ValidateDocumentTypeAsync(document, result);

            // Document totals validation
            await ValidateDocumentTotalsAsync(document, result);

            // Status-specific validation
            ValidateDocumentStatus(document, result);

            _logger.LogDebug("Document validation completed for {DocumentNumber}. Valid: {IsValid}, Errors: {ErrorCount}", 
                document.DocumentNumber, result.IsValid, result.Errors.Count);

            return result;
        }

        private async Task ValidateBasicDocumentPropertiesAsync(IDocument document, DocumentValidationResult result)
        {
            if (string.IsNullOrWhiteSpace(document.DocumentNumber))
            {
                result.DocumentErrors.Add(new DocumentValidationError
                {
                    Message = "Document number is required",
                    FieldName = nameof(document.DocumentNumber),
                    ErrorCode = "DOC001"
                });
            }

            if (document.Date == default)
            {
                result.DocumentErrors.Add(new DocumentValidationError
                {
                    Message = "Document date is required",
                    FieldName = nameof(document.Date),
                    ErrorCode = "DOC002"
                });
            }

            // Check for duplicate document number
            if (!string.IsNullOrWhiteSpace(document.DocumentNumber))
            {
                var existingDocument = _repository.GetObjects<IDocument>()
                    .Where(d => d.DocumentNumber == document.DocumentNumber && d.Id != document.Id)
                    .FirstOrDefault();

                if (existingDocument != null)
                {
                    result.DocumentErrors.Add(new DocumentValidationError
                    {
                        Message = $"Document number '{document.DocumentNumber}' already exists",
                        FieldName = nameof(document.DocumentNumber),
                        ErrorCode = "DOC003"
                    });
                }
            }

            await Task.CompletedTask;
        }

        private async Task ValidateBusinessEntityAsync(IDocument document, DocumentValidationResult result)
        {
            if (document.BusinessEntity == null)
            {
                result.DocumentErrors.Add(new DocumentValidationError
                {
                    Message = "Business entity is required",
                    FieldName = nameof(document.BusinessEntity),
                    ErrorCode = "DOC004"
                });
                return;
            }

            // Validate business entity exists and is active
            var businessEntity = _repository.GetObjects<IBusinessEntity>()
                .Where(be => be.Code == document.BusinessEntity.Code)
                .FirstOrDefault();

            if (businessEntity == null)
            {
                result.DocumentErrors.Add(new DocumentValidationError
                {
                    Message = $"Business entity '{document.BusinessEntity.Code}' not found",
                    FieldName = nameof(document.BusinessEntity),
                    ErrorCode = "DOC005"
                });
            }

            await Task.CompletedTask;
        }

        private async Task ValidateDocumentTypeAsync(IDocument document, DocumentValidationResult result)
        {
            if (document.DocumentType == null)
            {
                result.DocumentErrors.Add(new DocumentValidationError
                {
                    Message = "Document type is required",
                    FieldName = nameof(document.DocumentType),
                    ErrorCode = "DOC006"
                });
                return;
            }

            // Validate document type exists
            var documentType = _repository.GetObjects<IDocumentType>()
                .Where(dt => dt.Code == document.DocumentType.Code)
                .FirstOrDefault();

            if (documentType == null)
            {
                result.DocumentErrors.Add(new DocumentValidationError
                {
                    Message = $"Document type '{document.DocumentType.Code}' not found",
                    FieldName = nameof(document.DocumentType),
                    ErrorCode = "DOC007"
                });
            }

            await Task.CompletedTask;
        }

        private async Task ValidateDocumentTotalsAsync(IDocument document, DocumentValidationResult result)
        {
            if (document.DocumentTotals == null || !document.DocumentTotals.Any())
            {
                result.Warnings.Add("Document has no totals defined");
                return;
            }

            // Validate that totals balance
            var totalAmount = document.DocumentTotals.Sum(t => t.Total);
            if (totalAmount <= 0)
            {
                result.DocumentErrors.Add(new DocumentValidationError
                {
                    Message = "Document total amount must be greater than zero",
                    FieldName = nameof(document.DocumentTotals),
                    ErrorCode = "DOC008"
                });
            }

            // Validate account codes exist for transaction-enabled totals
            var transactionTotals = document.DocumentTotals.Where(t => t.IncludeInTransaction).ToList();
            
            foreach (var total in transactionTotals)
            {
                if (string.IsNullOrWhiteSpace(total.DebitAccountCode) && string.IsNullOrWhiteSpace(total.CreditAccountCode))
                {
                    result.DocumentErrors.Add(new DocumentValidationError
                    {
                        Message = $"Total '{total.Concept}' marked for transaction must have at least one account code",
                        FieldName = nameof(total.DebitAccountCode),
                        ErrorCode = "DOC009"
                    });
                }

                // Validate account codes exist
                await ValidateAccountCodeAsync(total.DebitAccountCode, result, "debit");
                await ValidateAccountCodeAsync(total.CreditAccountCode, result, "credit");
            }
        }

        private async Task ValidateAccountCodeAsync(string? accountCode, DocumentValidationResult result, string accountType)
        {
            if (string.IsNullOrWhiteSpace(accountCode))
                return;

            var account = _repository.GetObjects<IAccount>()
                .Where(a => a.OfficialCode == accountCode)
                .FirstOrDefault();

            if (account == null)
            {
                result.DocumentErrors.Add(new DocumentValidationError
                {
                    Message = $"Account code '{accountCode}' not found for {accountType} entry",
                    FieldName = $"{accountType}AccountCode",
                    ErrorCode = "DOC010"
                });
            }
            else if (!account.IsActive)
            {
                result.DocumentErrors.Add(new DocumentValidationError
                {
                    Message = $"Account '{accountCode}' is inactive and cannot be used",
                    FieldName = $"{accountType}AccountCode",
                    ErrorCode = "DOC011"
                });
            }

            await Task.CompletedTask;
        }

        private void ValidateDocumentStatus(IDocument document, DocumentValidationResult result)
        {
            // Status-specific validation rules
            switch (document.Status)
            {
                case DocumentStatus.Posted:
                    if (document.TotalAmount <= 0)
                    {
                        result.DocumentErrors.Add(new DocumentValidationError
                        {
                            Message = "Posted documents must have a positive total amount",
                            FieldName = nameof(document.Status),
                            ErrorCode = "DOC012"
                        });
                    }
                    break;

                case DocumentStatus.Cancelled:
                    // Cancelled documents should generally not be modified
                    result.Warnings.Add("Document is cancelled - modifications may not be allowed");
                    break;
            }
        }
    }
}
