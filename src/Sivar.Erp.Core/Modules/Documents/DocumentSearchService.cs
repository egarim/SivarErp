using Microsoft.Extensions.Logging;
using Sivar.Erp.Core.Infrastructure.Logging;
using System.ComponentModel;
using Sivar.Erp.Core.Infrastructure.Repository;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Sivar.Erp.Core.Core;
using Sivar.Erp.Core.Modules.BusinessEntities;
using Sivar.Erp.Core.Modules.Domain.Models;

namespace Sivar.Erp.Core.Modules.Documents
{
    /// <summary>
    /// Document search service implementation
    /// </summary>
    [Description("Document search service")]
    public class DocumentSearchService : IDocumentSearchService
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IBusinessEntityRepository _businessEntityRepository;
        private readonly ILogger<DocumentSearchService> _logger;

        public DocumentSearchService(
            IDocumentRepository documentRepository,
            IBusinessEntityRepository businessEntityRepository,
            ILogger<DocumentSearchService> logger)
        {
            _documentRepository = documentRepository;
            _businessEntityRepository = businessEntityRepository;
            _logger = logger;
        }

        public async Task<DocumentSearchResult> SearchAsync(DocumentSearchCriteria criteria, string searchPerformedBy)
        {
            using var activity = LoggingExtensions.StartActivity("DocumentSearch.Search");
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                var query = await _documentRepository.GetAllAsync();
                
                // Apply filters
                query = ApplyFilters(query, criteria);
                
                // Get total count before pagination
                var totalCount = query.Count();
                
                // Apply sorting
                query = ApplySorting(query, criteria);
                
                // Apply pagination
                var skip = (criteria.PageNumber - 1) * criteria.PageSize;
                var pagedDocuments = query.Skip(skip).Take(criteria.PageSize);
                
                // Convert to search items
                var searchItems = new List<DocumentSearchItem>();
                foreach (var doc in pagedDocuments)
                {
                    var businessEntity = await _businessEntityRepository.GetByIdAsync(doc.BusinessEntityId);
                    var searchItem = ConvertToSearchItem(doc, businessEntity, criteria.SearchText);
                    searchItems.Add(searchItem);
                }
                
                stopwatch.Stop();
                
                var result = new DocumentSearchResult
                {
                    Documents = searchItems,
                    TotalCount = totalCount,
                    PageNumber = criteria.PageNumber,
                    PageSize = criteria.PageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / criteria.PageSize),
                    SearchDuration = stopwatch.Elapsed,
                    SearchPerformedAt = DateTime.UtcNow,
                    SearchPerformedBy = searchPerformedBy
                };

                _logger.LogInformation("Search completed with {TotalCount} results in {Duration}ms", 
                    totalCount, stopwatch.ElapsedMilliseconds);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing document search");
                throw;
            }
        }

        public async Task<List<string>> GetSearchSuggestionsAsync(string searchText, int maxSuggestions = 10)
        {
            using var activity = LoggingExtensions.StartActivity("DocumentSearch.GetSuggestions");
            
            try
            {
                if (string.IsNullOrWhiteSpace(searchText) || searchText.Length < 2)
                    return new List<string>();

                var documents = await _documentRepository.GetAllAsync();
                var suggestions = new HashSet<string>();

                // Search in document numbers
                var documentNumbers = documents
                    .Where(d => d.DocumentNumber.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                    .Select(d => d.DocumentNumber)
                    .Take(maxSuggestions / 3);

                suggestions.UnionWith(documentNumbers);

                // Search in notes
                var noteMatches = documents
                    .Where(d => !string.IsNullOrEmpty(d.Notes) && 
                               d.Notes.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                    .SelectMany(d => ExtractWords(d.Notes!))
                    .Where(word => word.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                    .Distinct()
                    .Take(maxSuggestions / 3);

                suggestions.UnionWith(noteMatches);

                // Search in business entity names
                var businessEntities = await _businessEntityRepository.GetAllAsync();
                var entityNames = businessEntities
                    .Where(be => be.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                    .Select(be => be.Name)
                    .Take(maxSuggestions / 3);

                suggestions.UnionWith(entityNames);

                return suggestions.Take(maxSuggestions).OrderBy(s => s).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting search suggestions for '{SearchText}'", searchText);
                return new List<string>();
            }
        }

        public async Task<DocumentSearchResult> QuickSearchAsync(string searchText, string searchPerformedBy, int maxResults = 20)
        {
            using var activity = LoggingExtensions.StartActivity("DocumentSearch.QuickSearch");
            
            try
            {
                var criteria = new DocumentSearchCriteria
                {
                    SearchText = searchText,
                    PageNumber = 1,
                    PageSize = maxResults,
                    SortField = "CreatedAt",
                    SortDescending = true
                };

                return await SearchAsync(criteria, searchPerformedBy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing quick search for '{SearchText}'", searchText);
                throw;
            }
        }

        public async Task<DocumentSearchResult> AdvancedSearchAsync(DocumentSearchCriteria criteria, string searchPerformedBy)
        {
            using var activity = LoggingExtensions.StartActivity("DocumentSearch.AdvancedSearch");
            
            try
            {
                // Advanced search is the same as regular search with more detailed criteria
                return await SearchAsync(criteria, searchPerformedBy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing advanced search");
                throw;
            }
        }

        public async Task<List<DocumentSearchItem>> SearchByBusinessEntityAsync(Guid businessEntityId, int maxResults = 50)
        {
            using var activity = LoggingExtensions.StartActivity("DocumentSearch.SearchByBusinessEntity", businessEntityId);
            
            try
            {
                var criteria = new DocumentSearchCriteria
                {
                    BusinessEntityId = businessEntityId,
                    PageNumber = 1,
                    PageSize = maxResults,
                    SortField = "Date",
                    SortDescending = true
                };

                var result = await SearchAsync(criteria, "System");
                return result.Documents;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching documents by business entity {BusinessEntityId}", businessEntityId);
                throw;
            }
        }

        public async Task<List<DocumentSearchItem>> SearchByDateRangeAsync(DateOnly startDate, DateOnly endDate, int maxResults = 100)
        {
            using var activity = LoggingExtensions.StartActivity("DocumentSearch.SearchByDateRange");
            
            try
            {
                var criteria = new DocumentSearchCriteria
                {
                    DateFrom = startDate,
                    DateTo = endDate,
                    PageNumber = 1,
                    PageSize = maxResults,
                    SortField = "Date",
                    SortDescending = true
                };

                var result = await SearchAsync(criteria, "System");
                return result.Documents;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching documents by date range {StartDate} to {EndDate}", startDate, endDate);
                throw;
            }
        }

        public async Task<Dictionary<string, int>> GetSearchStatisticsAsync()
        {
            using var activity = LoggingExtensions.StartActivity("DocumentSearch.GetStatistics");
            
            try
            {
                var documents = await _documentRepository.GetAllAsync();
                
                var stats = new Dictionary<string, int>
                {
                    ["TotalDocuments"] = documents.Count(),
                    ["DocumentsThisMonth"] = documents.Count(d => 
                        d.CreatedAt.Year == DateTime.Now.Year && 
                        d.CreatedAt.Month == DateTime.Now.Month),
                    ["DocumentsThisYear"] = documents.Count(d => 
                        d.CreatedAt.Year == DateTime.Now.Year),
                    ["DraftDocuments"] = documents.Count(d => d.Status == DocumentStatus.Draft),
                    ["PendingApproval"] = documents.Count(d => d.Status == DocumentStatus.PendingApproval),
                    ["ApprovedDocuments"] = documents.Count(d => d.Status == DocumentStatus.Approved),
                    ["FinalizedDocuments"] = documents.Count(d => d.Status == DocumentStatus.Finalized),
                    ["CancelledDocuments"] = documents.Count(d => d.Status == DocumentStatus.Cancelled)
                };

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting search statistics");
                throw;
            }
        }

        private IEnumerable<Document> ApplyFilters(IEnumerable<Document> query, DocumentSearchCriteria criteria)
        {
            // Filter by document number
            if (!string.IsNullOrEmpty(criteria.DocumentNumber))
            {
                query = query.Where(d => d.DocumentNumber.Contains(criteria.DocumentNumber, StringComparison.OrdinalIgnoreCase));
            }

            // Filter by document type
            if (!string.IsNullOrEmpty(criteria.DocumentTypeCode))
            {
                query = query.Where(d => d.DocumentTypeCode == criteria.DocumentTypeCode);
            }

            // Filter by business entity
            if (criteria.BusinessEntityId.HasValue)
            {
                query = query.Where(d => d.BusinessEntityId == criteria.BusinessEntityId.Value);
            }

            // Filter by status
            if (criteria.Status.HasValue)
            {
                query = query.Where(d => d.Status == criteria.Status.Value);
            }

            // Filter by date range
            if (criteria.DateFrom.HasValue)
            {
                query = query.Where(d => d.Date >= criteria.DateFrom.Value);
            }

            if (criteria.DateTo.HasValue)
            {
                query = query.Where(d => d.Date <= criteria.DateTo.Value);
            }

            // Filter by amount range
            if (criteria.AmountFrom.HasValue)
            {
                query = query.Where(d => d.TotalAmount >= criteria.AmountFrom.Value);
            }

            if (criteria.AmountTo.HasValue)
            {
                query = query.Where(d => d.TotalAmount <= criteria.AmountTo.Value);
            }

            // Filter by search text
            if (!string.IsNullOrEmpty(criteria.SearchText))
            {
                var searchTerms = criteria.SearchText.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                query = query.Where(d => searchTerms.Any(term =>
                    d.DocumentNumber.ToLowerInvariant().Contains(term) ||
                    (d.Notes != null && d.Notes.ToLowerInvariant().Contains(term))));
            }

            // Filter by created by
            if (!string.IsNullOrEmpty(criteria.CreatedBy))
            {
                query = query.Where(d => d.CreatedBy.Contains(criteria.CreatedBy, StringComparison.OrdinalIgnoreCase));
            }

            // Filter by creation date range
            if (criteria.CreatedFrom.HasValue)
            {
                query = query.Where(d => d.CreatedAt >= criteria.CreatedFrom.Value);
            }

            if (criteria.CreatedTo.HasValue)
            {
                query = query.Where(d => d.CreatedAt <= criteria.CreatedTo.Value);
            }

            return query;
        }

        private IEnumerable<Document> ApplySorting(IEnumerable<Document> query, DocumentSearchCriteria criteria)
        {
            if (string.IsNullOrEmpty(criteria.SortField))
                return query.OrderByDescending(d => d.CreatedAt);

            var sortField = criteria.SortField.ToLowerInvariant();
            var ascending = !criteria.SortDescending;

            return sortField switch
            {
                "documentnumber" => ascending ? query.OrderBy(d => d.DocumentNumber) : query.OrderByDescending(d => d.DocumentNumber),
                "date" => ascending ? query.OrderBy(d => d.Date) : query.OrderByDescending(d => d.Date),
                "totalamount" => ascending ? query.OrderBy(d => d.TotalAmount) : query.OrderByDescending(d => d.TotalAmount),
                "status" => ascending ? query.OrderBy(d => d.Status) : query.OrderByDescending(d => d.Status),
                "createdby" => ascending ? query.OrderBy(d => d.CreatedBy) : query.OrderByDescending(d => d.CreatedBy),
                "createdat" => ascending ? query.OrderBy(d => d.CreatedAt) : query.OrderByDescending(d => d.CreatedAt),
                _ => ascending ? query.OrderBy(d => d.CreatedAt) : query.OrderByDescending(d => d.CreatedAt)
            };
        }

        private DocumentSearchItem ConvertToSearchItem(Document document, BusinessEntity? businessEntity, string? searchText)
        {
            var searchItem = new DocumentSearchItem
            {
                Id = document.Id,
                DocumentNumber = document.DocumentNumber,
                DocumentTypeName = GetDocumentTypeName(document.DocumentTypeCode),
                BusinessEntityName = businessEntity?.Name ?? "Unknown",
                Status = document.Status,
                Date = document.Date,
                TotalAmount = document.TotalAmount,
                CreatedBy = document.CreatedBy,
                CreatedAt = document.CreatedAt,
                AttachmentCount = 0, // Would be populated from attachments service
                HasWorkflowHistory = false // Would be populated from workflow service
            };

            // Add search highlights
            if (!string.IsNullOrEmpty(searchText))
            {
                searchItem.MatchHighlights = GetSearchHighlights(document, searchText);
            }

            return searchItem;
        }

        private List<string> GetSearchHighlights(Document document, string searchText)
        {
            var highlights = new List<string>();
            var searchTerms = searchText.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (var term in searchTerms)
            {
                if (document.DocumentNumber.ToLowerInvariant().Contains(term))
                {
                    highlights.Add($"Document Number: {document.DocumentNumber}");
                }

                if (document.Notes != null && document.Notes.ToLowerInvariant().Contains(term))
                {
                    var index = document.Notes.ToLowerInvariant().IndexOf(term);
                    var start = Math.Max(0, index - 20);
                    var length = Math.Min(document.Notes.Length - start, 60);
                    var excerpt = document.Notes.Substring(start, length);
                    highlights.Add($"Notes: ...{excerpt}...");
                }
            }

            return highlights;
        }

        private string GetDocumentTypeName(string documentTypeCode)
        {
            // This would typically come from a document types lookup
            return documentTypeCode switch
            {
                "INV" => "Invoice",
                "QUO" => "Quote",
                "ORD" => "Order",
                "REC" => "Receipt",
                "PAY" => "Payment",
                _ => documentTypeCode
            };
        }

        private List<string> ExtractWords(string text)
        {
            if (string.IsNullOrEmpty(text))
                return new List<string>();

            var words = Regex.Split(text, @"\W+")
                .Where(word => !string.IsNullOrEmpty(word) && word.Length >= 3)
                .Select(word => word.ToLowerInvariant())
                .Distinct()
                .ToList();

            return words;
        }
    }
}
