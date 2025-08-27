using Microsoft.Extensions.DependencyInjection;
using Sivar.Erp.Core.Application.Services.Documents;
using Sivar.Erp.Core.Infrastructure.Services.Documents;

namespace Sivar.Erp.Core.Infrastructure.DependencyInjection
{
    /// <summary>
    /// Extension methods for configuring document services
    /// </summary>
    public static class DocumentServiceExtensions
    {
        /// <summary>
        /// Adds document management services to the DI container
        /// </summary>
        public static IServiceCollection AddDocumentServices(this IServiceCollection services)
        {
            // Document services
            services.AddScoped<IDocumentService, DocumentService>();
            
            // Add validators if using FluentValidation
            // services.AddScoped<IValidator<Document>, DocumentValidator>();
            // services.AddScoped<IValidator<DocumentLine>, DocumentLineValidator>();
            
            // Add AutoMapper profiles for document DTOs
            // services.AddAutoMapper(typeof(DocumentMappingProfile));
            
            return services;
        }

        /// <summary>
        /// Adds document management services with advanced options
        /// </summary>
        public static IServiceCollection AddDocumentServices(
            this IServiceCollection services,
            Action<DocumentServiceOptions>? configureOptions = null)
        {
            // Configure options
            if (configureOptions != null)
            {
                services.Configure(configureOptions);
            }

            return services.AddDocumentServices();
        }
    }

    /// <summary>
    /// Configuration options for document services
    /// </summary>
    public class DocumentServiceOptions
    {
        /// <summary>
        /// Maximum number of lines allowed per document
        /// </summary>
        public int MaxLinesPerDocument { get; set; } = 1000;

        /// <summary>
        /// Default page size for document queries
        /// </summary>
        public int DefaultPageSize { get; set; } = 50;

        /// <summary>
        /// Maximum page size for document queries
        /// </summary>
        public int MaxPageSize { get; set; } = 500;

        /// <summary>
        /// Enable automatic document number generation
        /// </summary>
        public bool EnableAutoNumberGeneration { get; set; } = true;

        /// <summary>
        /// Default currency code for new documents
        /// </summary>
        public string DefaultCurrencyCode { get; set; } = "USD";

        /// <summary>
        /// Enable document validation before save
        /// </summary>
        public bool EnableValidation { get; set; } = true;

        /// <summary>
        /// Enable audit logging for document operations
        /// </summary>
        public bool EnableAuditLogging { get; set; } = true;

        /// <summary>
        /// Cache duration for document statistics (in minutes)
        /// </summary>
        public int StatisticsCacheDurationMinutes { get; set; } = 15;

        /// <summary>
        /// Maximum concurrent document operations
        /// </summary>
        public int MaxConcurrentOperations { get; set; } = 10;
    }
}
