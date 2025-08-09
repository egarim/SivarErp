using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sivar.Erp.Core.Core;
using Sivar.Erp.Core.Demo;
using Sivar.Erp.Core.Infrastructure.Data;
using Sivar.Erp.Core.Modules.Accounting;
using Sivar.Erp.Core.Modules.DataImport;
using Sivar.Erp.Core.Modules.Documents;
using Sivar.Erp.Core.Modules.Taxes;

namespace Sivar.Erp.Core.Core
{
    /// <summary>
    /// Extension methods for configuring ERP services
    /// </summary>
    [Description("Extension methods for configuring ERP services")]
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds core ERP services to the DI container
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="options">Optional configuration options</param>
        /// <returns>The service collection for chaining</returns>
        [Description("Adds core ERP services to the DI container")]
        public static IServiceCollection AddSivarErpCore(
            this IServiceCollection services,
            ErpCoreOptions? options = null)
        {
            // Register core services
            services.AddScoped<IRepository, InMemoryRepository>();
            
            // Register services (will be implemented later)
            services.AddScoped<IAccountingService, AccountingService>();
            services.AddScoped<IDocumentService, DocumentService>();
            services.AddScoped<ITaxService, TaxService>();
            services.AddScoped<ICsvImportService, CsvImportService>();
            
            // Register logging
            services.AddLogging(builder => builder.AddConsole());
            
            return services;
        }
        
        /// <summary>
        /// Adds demo services with embedded test data
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="testDataSet">The test data set to use</param>
        /// <returns>The service collection for chaining</returns>
        [Description("Adds demo services with embedded test data")]
        public static IServiceCollection AddSivarErpDemo(
            this IServiceCollection services,
            string testDataSet = "ElSalvador")
        {
            services.AddSivarErpCore();
            
            services.AddScoped<ISampleDataGenerator, SampleDataGenerator>();
            
            services.AddOptions();
            services.Configure<DemoOptions>(options => 
            {
                options.TestDataSet = testDataSet;
            });
            
            return services;
        }
    }
    
    /// <summary>
    /// Options for configuring the ERP core
    /// </summary>
    public class ErpCoreOptions
    {
        /// <summary>
        /// Gets or sets whether to use in-memory storage
        /// </summary>
        public bool UseInMemoryStorage { get; set; } = true;
        
        /// <summary>
        /// Gets or sets the connection string for database storage
        /// </summary>
        public string? ConnectionString { get; set; }
    }
    
    /// <summary>
    /// Options for configuring demo features
    /// </summary>
    public class DemoOptions
    {
        /// <summary>
        /// Gets or sets the test data set to use
        /// </summary>
        public string TestDataSet { get; set; } = "ElSalvador";
    }
}