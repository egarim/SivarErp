using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Sivar.Erp.Core.Core;
using Sivar.Erp.Core.Modules.Domain;

namespace Sivar.Erp.Core.Modules.Documents
{
    /// <summary>
    /// Advanced document analytics and reporting service
    /// </summary>
    [Description("Advanced document analytics and reporting service")]
    public class DocumentAnalytics
    {
        private readonly IRepository _repository;
        private readonly ILogger<DocumentAnalytics> _logger;

        public DocumentAnalytics(IRepository repository, ILogger<DocumentAnalytics> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets document statistics for a date range
        /// </summary>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>Document statistics</returns>
        [Description("Gets document statistics for a date range")]
        public async Task<DocumentStatistics> GetDocumentStatisticsAsync(DateTime startDate, DateTime endDate)
        {
            _logger.LogDebug("Generating document statistics for period {StartDate} to {EndDate}", startDate, endDate);

            var documents = _repository.GetObjects<IDocument>()
                .Where(d => d.Date.ToDateTime(TimeOnly.MinValue) >= startDate && d.Date.ToDateTime(TimeOnly.MinValue) <= endDate)
                .ToList();

            var statistics = new DocumentStatistics
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalDocuments = documents.Count,
                TotalAmount = documents.Sum(d => d.TotalAmount),
                DocumentsByStatus = documents.GroupBy(d => d.Status)
                    .ToDictionary(g => g.Key.ToString(), g => g.Count()),
                DocumentsByType = documents.GroupBy(d => d.DocumentType?.Code ?? "Unknown")
                    .ToDictionary(g => g.Key, g => g.Count()),
                AverageDocumentAmount = documents.Any() ? (double)documents.Average(d => d.TotalAmount) : 0,
                LargestDocumentAmount = documents.Any() ? documents.Max(d => d.TotalAmount) : 0,
                SmallestDocumentAmount = documents.Any() ? documents.Min(d => d.TotalAmount) : 0
            };

            // Calculate daily statistics
            statistics.DocumentsByDay = documents
                .GroupBy(d => d.Date.ToDateTime(TimeOnly.MinValue).Date)
                .ToDictionary(g => g.Key, g => new DailyDocumentStats
                {
                    Date = g.Key,
                    DocumentCount = g.Count(),
                    TotalAmount = g.Sum(d => d.TotalAmount),
                    AverageAmount = (double)g.Average(d => d.TotalAmount)
                });

            // Business entity analysis
            statistics.TopBusinessEntities = documents
                .Where(d => d.BusinessEntity != null)
                .GroupBy(d => d.BusinessEntity!.Code)
                .OrderByDescending(g => g.Sum(d => d.TotalAmount))
                .Take(10)
                .ToDictionary(g => g.Key, g => new BusinessEntityDocumentStats
                {
                    BusinessEntityCode = g.Key,
                    DocumentCount = g.Count(),
                    TotalAmount = g.Sum(d => d.TotalAmount),
                    AverageAmount = (double)g.Average(d => d.TotalAmount)
                });

            _logger.LogDebug("Document statistics generated: {TotalDocuments} documents, {TotalAmount:C} total amount", 
                statistics.TotalDocuments, statistics.TotalAmount);

            await Task.CompletedTask;
            return statistics;
        }

        /// <summary>
        /// Gets document trends analysis
        /// </summary>
        /// <param name="months">Number of months to analyze</param>
        /// <returns>Document trends</returns>
        [Description("Gets document trends analysis")]
        public async Task<DocumentTrends> GetDocumentTrendsAsync(int months = 12)
        {
            var endDate = DateTime.Today;
            var startDate = endDate.AddMonths(-months);

            _logger.LogDebug("Analyzing document trends for {Months} months", months);

            var documents = _repository.GetObjects<IDocument>()
                .Where(d => d.Date.ToDateTime(TimeOnly.MinValue) >= startDate && d.Date.ToDateTime(TimeOnly.MinValue) <= endDate)
                .ToList();

            var trends = new DocumentTrends
            {
                AnalysisPeriod = months,
                StartDate = startDate,
                EndDate = endDate
            };

            // Monthly analysis
            trends.MonthlyStats = documents
                .GroupBy(d => new { d.Date.ToDateTime(TimeOnly.MinValue).Year, d.Date.ToDateTime(TimeOnly.MinValue).Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new MonthlyDocumentStats
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    DocumentCount = g.Count(),
                    TotalAmount = g.Sum(d => d.TotalAmount),
                    AverageAmount = (double)g.Average(d => d.TotalAmount),
                    PostedDocuments = g.Count(d => d.Status == DocumentStatus.Posted),
                    CancelledDocuments = g.Count(d => d.Status == DocumentStatus.Cancelled)
                })
                .ToList();

            // Growth calculations
            if (trends.MonthlyStats.Count >= 2)
            {
                var lastMonth = trends.MonthlyStats.Last();
                var previousMonth = trends.MonthlyStats[trends.MonthlyStats.Count - 2];

                trends.DocumentCountGrowth = CalculateGrowthPercentage(previousMonth.DocumentCount, lastMonth.DocumentCount);
                trends.AmountGrowth = CalculateGrowthPercentage((double)previousMonth.TotalAmount, (double)lastMonth.TotalAmount);
            }

            await Task.CompletedTask;
            return trends;
        }

        /// <summary>
        /// Gets document performance metrics
        /// </summary>
        /// <returns>Document performance metrics</returns>
        [Description("Gets document performance metrics")]
        public async Task<DocumentPerformanceMetrics> GetPerformanceMetricsAsync()
        {
            _logger.LogDebug("Calculating document performance metrics");

            var last30Days = DateTime.Today.AddDays(-30);
            var documents = _repository.GetObjects<IDocument>()
                .Where(d => d.CreatedAt >= last30Days)
                .ToList();

            var metrics = new DocumentPerformanceMetrics
            {
                AnalysisPeriod = 30,
                TotalDocuments = documents.Count
            };

            if (documents.Any())
            {
                // Processing time analysis (assuming CreatedAt vs UpdatedAt represents processing time)
                var processedDocuments = documents.Where(d => d.UpdatedAt > d.CreatedAt).ToList();
                if (processedDocuments.Any())
                {
                    var processingTimes = processedDocuments
                        .Select(d => (d.UpdatedAt - d.CreatedAt).TotalHours)
                        .Where(h => h >= 0)
                        .ToList();

                    if (processingTimes.Any())
                    {
                        metrics.AverageProcessingTimeHours = processingTimes.Average();
                        metrics.MaxProcessingTimeHours = processingTimes.Max();
                        metrics.MinProcessingTimeHours = processingTimes.Min();
                    }
                }

                // Status distribution
                metrics.PostedPercentage = (double)documents.Count(d => d.Status == DocumentStatus.Posted) / documents.Count * 100;
                metrics.CancelledPercentage = (double)documents.Count(d => d.Status == DocumentStatus.Cancelled) / documents.Count * 100;
                metrics.DraftPercentage = (double)documents.Count(d => d.Status == DocumentStatus.Draft) / documents.Count * 100;

                // Error rate (assuming cancelled documents represent errors)
                metrics.ErrorRate = metrics.CancelledPercentage;
            }

            await Task.CompletedTask;
            return metrics;
        }

        private static double CalculateGrowthPercentage(double previousValue, double currentValue)
        {
            if (previousValue == 0)
                return currentValue > 0 ? 100 : 0;

            return ((currentValue - previousValue) / previousValue) * 100;
        }
    }

    /// <summary>
    /// Document statistics data structure
    /// </summary>
    public class DocumentStatistics
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalDocuments { get; set; }
        public decimal TotalAmount { get; set; }
        public double AverageDocumentAmount { get; set; }
        public decimal LargestDocumentAmount { get; set; }
        public decimal SmallestDocumentAmount { get; set; }
        public Dictionary<string, int> DocumentsByStatus { get; set; } = new();
        public Dictionary<string, int> DocumentsByType { get; set; } = new();
        public Dictionary<DateTime, DailyDocumentStats> DocumentsByDay { get; set; } = new();
        public Dictionary<string, BusinessEntityDocumentStats> TopBusinessEntities { get; set; } = new();
    }

    /// <summary>
    /// Daily document statistics
    /// </summary>
    public class DailyDocumentStats
    {
        public DateTime Date { get; set; }
        public int DocumentCount { get; set; }
        public decimal TotalAmount { get; set; }
        public double AverageAmount { get; set; }
    }

    /// <summary>
    /// Business entity document statistics
    /// </summary>
    public class BusinessEntityDocumentStats
    {
        public string BusinessEntityCode { get; set; } = string.Empty;
        public int DocumentCount { get; set; }
        public decimal TotalAmount { get; set; }
        public double AverageAmount { get; set; }
    }

    /// <summary>
    /// Document trends analysis
    /// </summary>
    public class DocumentTrends
    {
        public int AnalysisPeriod { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<MonthlyDocumentStats> MonthlyStats { get; set; } = new();
        public double DocumentCountGrowth { get; set; }
        public double AmountGrowth { get; set; }
    }

    /// <summary>
    /// Monthly document statistics
    /// </summary>
    public class MonthlyDocumentStats
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int DocumentCount { get; set; }
        public decimal TotalAmount { get; set; }
        public double AverageAmount { get; set; }
        public int PostedDocuments { get; set; }
        public int CancelledDocuments { get; set; }
    }

    /// <summary>
    /// Document performance metrics
    /// </summary>
    public class DocumentPerformanceMetrics
    {
        public int AnalysisPeriod { get; set; }
        public int TotalDocuments { get; set; }
        public double AverageProcessingTimeHours { get; set; }
        public double MaxProcessingTimeHours { get; set; }
        public double MinProcessingTimeHours { get; set; }
        public double PostedPercentage { get; set; }
        public double CancelledPercentage { get; set; }
        public double DraftPercentage { get; set; }
        public double ErrorRate { get; set; }
    }
}
