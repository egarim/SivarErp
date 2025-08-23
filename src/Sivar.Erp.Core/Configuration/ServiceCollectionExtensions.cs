using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sivar.Erp.Core.Core;
using Sivar.Erp.Core.Infrastructure.Data;
using Sivar.Erp.Core.Modules.Accounting;
using Sivar.Erp.Core.Modules.Documents;
using Sivar.Erp.Core.Modules.Taxes;
using Sivar.Erp.Core.Modules.DataImport;
using Sivar.Erp.Core.Modules.DataImport.Importers;
using Sivar.Erp.Core.Modules.Domain;
using Sivar.Erp.Core.Modules.Domain.Models;
using Sivar.Erp.Core.Demo;

namespace Sivar.Erp.Core.Configuration
{
    /// <summary>
    /// Extension methods for configuring ERP services in the dependency injection container
    /// </summary>
    [Description("Extension methods for configuring ERP services")]
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds core ERP services to the DI container with standard configuration
        /// </summary>
        /// <param name="services">The service collection to add services to</param>
        /// <param name="options">Optional configuration options</param>
        /// <returns>The service collection for method chaining</returns>
        [Description("Adds core ERP services to the DI container")]
        public static IServiceCollection AddSivarErpCore(
            this IServiceCollection services,
            ErpCoreOptions? options = null)
        {
            // Configure options
            if (options != null)
            {
                services.Configure<ErpCoreOptions>(opt =>
                {
                    opt.DatabaseProvider = options.DatabaseProvider;
                    opt.DefaultTimeZone = options.DefaultTimeZone;
                    opt.EnablePerformanceLogging = options.EnablePerformanceLogging;
                    opt.EnableAuditLogging = options.EnableAuditLogging;
                    opt.MaxTransactionBatchSize = options.MaxTransactionBatchSize;
                });
            }
            else
            {
                services.Configure<ErpCoreOptions>(opt =>
                {
                    opt.DatabaseProvider = DatabaseProvider.InMemory;
                    opt.DefaultTimeZone = "UTC";
                    opt.EnablePerformanceLogging = true;
                    opt.EnableAuditLogging = true;
                    opt.MaxTransactionBatchSize = 1000;
                });
            }

            // Register core infrastructure
            services.AddScoped<IRepository, InMemoryRepository>();

            // Register business services
            services.AddScoped<IAccountingService, AccountingService>();
            services.AddScoped<IDocumentService, DocumentService>();
            services.AddScoped<ITaxService, TaxService>();
            services.AddScoped<IDataImportService, DataImportService>();
            services.AddScoped<ICsvImportService, CsvImportService>();
            services.AddScoped<CsvValidationService>();

            // Register specialized entity importers
            services.AddScoped<IEntityImporter<IAccount>, AccountImporter>();
            services.AddScoped<IEntityImporter<IBusinessEntity>, BusinessEntityImporter>();
            services.AddScoped<IEntityImporter<ITax>, TaxImporter>();
            services.AddScoped<IEntityImporter<ItemDto>, ItemImporter>();
            services.AddScoped<IEntityImporter<IDocumentType>, DocumentTypeImporter>();

            // Phase 3 services will be registered in a separate extension method
            // TODO: Add Phase 3 service registration after namespace issues are resolved

            // Register logging services
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });

            return services;
        }

        /// <summary>
        /// Adds demo services with embedded test data capabilities
        /// </summary>
        /// <param name="services">The service collection to add services to</param>
        /// <param name="testDataSet">Name of the test data set to use (default: ElSalvador)</param>
        /// <returns>The service collection for method chaining</returns>
        [Description("Adds demo services with embedded test data")]
        public static IServiceCollection AddSivarErpDemo(
            this IServiceCollection services,
            string testDataSet = "ElSalvador")
        {
            // Add core services first
            services.AddSivarErpCore(new ErpCoreOptions
            {
                DatabaseProvider = DatabaseProvider.InMemory,
                EnablePerformanceLogging = true,
                EnableAuditLogging = true
            });

            // Register demo-specific services
            services.AddScoped<ISampleDataGenerator, SampleDataGenerator>();
            services.AddScoped<IAdvancedDemoScenarioService, AdvancedDemoScenarioService>();

            // Configure demo options
            services.Configure<DemoOptions>(options =>
            {
                options.TestDataSet = testDataSet;
                options.AutoGenerateData = true;
                options.MaxSampleRecords = 1000;
                options.IncludeTestTransactions = true;
                options.IncludeTestDocuments = true;
            });

            return services;
        }

        /// <summary>
        /// Adds enhanced demo services with advanced scenario support
        /// </summary>
        /// <param name="services">The service collection to add services to</param>
        /// <param name="demoOptions">Enhanced demo configuration options</param>
        /// <returns>The service collection for method chaining</returns>
        [Description("Adds enhanced demo services with advanced scenario support")]
        public static IServiceCollection AddSivarErpEnhancedDemo(
            this IServiceCollection services,
            EnhancedDemoOptions? demoOptions = null)
        {
            // Add core services
            services.AddSivarErpCore(new ErpCoreOptions
            {
                DatabaseProvider = DatabaseProvider.InMemory,
                EnablePerformanceLogging = true,
                EnableAuditLogging = true,
                MaxTransactionBatchSize = demoOptions?.MaxTransactionBatchSize ?? 2000
            });

            // Register all demo services
            services.AddScoped<ISampleDataGenerator, SampleDataGenerator>();
            services.AddScoped<IAdvancedDemoScenarioService, AdvancedDemoScenarioService>();

            // Configure enhanced demo options
            if (demoOptions != null)
            {
                services.Configure<DemoOptions>(options =>
                {
                    options.TestDataSet = demoOptions.TestDataSet;
                    options.AutoGenerateData = demoOptions.AutoGenerateData;
                    options.MaxSampleRecords = demoOptions.MaxSampleRecords;
                    options.IncludeTestTransactions = demoOptions.IncludeTestTransactions;
                    options.IncludeTestDocuments = demoOptions.IncludeTestDocuments;
                });

                services.Configure<EnhancedDemoOptions>(options =>
                {
                    options.TestDataSet = demoOptions.TestDataSet;
                    options.AutoGenerateData = demoOptions.AutoGenerateData;
                    options.MaxSampleRecords = demoOptions.MaxSampleRecords;
                    options.IncludeTestTransactions = demoOptions.IncludeTestTransactions;
                    options.IncludeTestDocuments = demoOptions.IncludeTestDocuments;
                    options.DefaultRegion = demoOptions.DefaultRegion;
                    options.DefaultIndustry = demoOptions.DefaultIndustry;
                    options.DefaultComplexity = demoOptions.DefaultComplexity;
                    options.EnableAdvancedScenarios = demoOptions.EnableAdvancedScenarios;
                    options.EnablePerformanceTesting = demoOptions.EnablePerformanceTesting;
                    options.MaxTransactionBatchSize = demoOptions.MaxTransactionBatchSize;
                });
            }
            else
            {
                services.Configure<DemoOptions>(options =>
                {
                    options.TestDataSet = "ElSalvador";
                    options.AutoGenerateData = true;
                    options.MaxSampleRecords = 1000;
                    options.IncludeTestTransactions = true;
                    options.IncludeTestDocuments = true;
                });

                services.Configure<EnhancedDemoOptions>(options =>
                {
                    options.TestDataSet = "ElSalvador";
                    options.AutoGenerateData = true;
                    options.MaxSampleRecords = 1000;
                    options.IncludeTestTransactions = true;
                    options.IncludeTestDocuments = true;
                    options.DefaultRegion = "ElSalvador";
                    options.DefaultIndustry = "Generic";
                    options.DefaultComplexity = ScenarioComplexity.Medium;
                    options.EnableAdvancedScenarios = true;
                    options.EnablePerformanceTesting = false;
                    options.MaxTransactionBatchSize = 2000;
                });
            }

            return services;
        }

        /// <summary>
        /// Adds advanced ERP services with additional features
        /// </summary>
        /// <param name="services">The service collection to add services to</param>
        /// <param name="options">Advanced configuration options</param>
        /// <returns>The service collection for method chaining</returns>
        [Description("Adds advanced ERP services with additional features")]
        public static IServiceCollection AddSivarErpAdvanced(
            this IServiceCollection services,
            AdvancedErpOptions? options = null)
        {
            // Add core services
            services.AddSivarErpCore(options?.CoreOptions);

            // Add advanced services
            if (options?.EnableWorkflowEngine == true)
            {
                // Future: Add workflow engine services
                // services.AddScoped<IWorkflowEngine, WorkflowEngine>();
            }

            if (options?.EnableReporting == true)
            {
                // Future: Add reporting services
                // services.AddScoped<IReportingService, ReportingService>();
            }

            if (options?.EnableNotifications == true)
            {
                // Future: Add notification services
                // services.AddScoped<INotificationService, NotificationService>();
            }

            return services;
        }

        /// <summary>
        /// Adds ERP services for production use with optimized configuration
        /// </summary>
        /// <param name="services">The service collection to add services to</param>
        /// <param name="connectionString">Database connection string</param>
        /// <param name="options">Production configuration options</param>
        /// <returns>The service collection for method chaining</returns>
        [Description("Adds ERP services for production use")]
        public static IServiceCollection AddSivarErpProduction(
            this IServiceCollection services,
            string connectionString,
            ProductionErpOptions? options = null)
        {
            var coreOptions = new ErpCoreOptions
            {
                DatabaseProvider = options?.DatabaseProvider ?? DatabaseProvider.SqlServer,
                DefaultTimeZone = options?.DefaultTimeZone ?? "UTC",
                EnablePerformanceLogging = options?.EnablePerformanceLogging ?? true,
                EnableAuditLogging = options?.EnableAuditLogging ?? true,
                MaxTransactionBatchSize = options?.MaxTransactionBatchSize ?? 5000
            };

            services.AddSivarErpCore(coreOptions);

            // Configure for production
            services.Configure<ErpCoreOptions>(opt =>
            {
                opt.ConnectionString = connectionString;
            });

            // Future: Replace in-memory repository with actual database repository
            // services.AddScoped<IRepository, SqlServerRepository>();

            return services;
        }
    }

    /// <summary>
    /// Core configuration options for the ERP system
    /// </summary>
    [Description("Core configuration options for the ERP system")]
    public class ErpCoreOptions
    {
        /// <summary>
        /// Database provider to use
        /// </summary>
        [Description("Database provider to use")]
        public DatabaseProvider DatabaseProvider { get; set; } = DatabaseProvider.InMemory;

        /// <summary>
        /// Database connection string (for non-InMemory providers)
        /// </summary>
        [Description("Database connection string")]
        public string? ConnectionString { get; set; }

        /// <summary>
        /// Default time zone for the system
        /// </summary>
        [Description("Default time zone for the system")]
        public string DefaultTimeZone { get; set; } = "UTC";

        /// <summary>
        /// Enable performance logging
        /// </summary>
        [Description("Enable performance logging")]
        public bool EnablePerformanceLogging { get; set; } = true;

        /// <summary>
        /// Enable audit logging
        /// </summary>
        [Description("Enable audit logging")]
        public bool EnableAuditLogging { get; set; } = true;

        /// <summary>
        /// Maximum number of transactions to process in a single batch
        /// </summary>
        [Description("Maximum number of transactions to process in a single batch")]
        public int MaxTransactionBatchSize { get; set; } = 1000;

        /// <summary>
        /// Enable automatic tax calculations
        /// </summary>
        [Description("Enable automatic tax calculations")]
        public bool EnableAutomaticTaxCalculation { get; set; } = true;

        /// <summary>
        /// Default currency code
        /// </summary>
        [Description("Default currency code")]
        public string DefaultCurrencyCode { get; set; } = "USD";
    }

    /// <summary>
    /// Demo-specific configuration options
    /// </summary>
    [Description("Demo-specific configuration options")]
    public class DemoOptions
    {
        /// <summary>
        /// Name of the test data set to use
        /// </summary>
        [Description("Name of the test data set to use")]
        public string TestDataSet { get; set; } = "ElSalvador";

        /// <summary>
        /// Automatically generate sample data on startup
        /// </summary>
        [Description("Automatically generate sample data on startup")]
        public bool AutoGenerateData { get; set; } = true;

        /// <summary>
        /// Maximum number of sample records to generate
        /// </summary>
        [Description("Maximum number of sample records to generate")]
        public int MaxSampleRecords { get; set; } = 1000;

        /// <summary>
        /// Include test transactions in sample data
        /// </summary>
        [Description("Include test transactions in sample data")]
        public bool IncludeTestTransactions { get; set; } = true;

        /// <summary>
        /// Include test documents in sample data
        /// </summary>
        [Description("Include test documents in sample data")]
        public bool IncludeTestDocuments { get; set; } = true;
    }

    /// <summary>
    /// Enhanced demo configuration options with advanced scenario support
    /// </summary>
    [Description("Enhanced demo configuration options with advanced scenario support")]
    public class EnhancedDemoOptions : DemoOptions
    {
        /// <summary>
        /// Default region for demo scenarios
        /// </summary>
        [Description("Default region for demo scenarios")]
        public string DefaultRegion { get; set; } = "ElSalvador";

        /// <summary>
        /// Default industry type for demo scenarios
        /// </summary>
        [Description("Default industry type for demo scenarios")]
        public string DefaultIndustry { get; set; } = "Generic";

        /// <summary>
        /// Default complexity level for demo scenarios
        /// </summary>
        [Description("Default complexity level for demo scenarios")]
        public ScenarioComplexity DefaultComplexity { get; set; } = ScenarioComplexity.Medium;

        /// <summary>
        /// Enable advanced business workflow scenarios
        /// </summary>
        [Description("Enable advanced business workflow scenarios")]
        public bool EnableAdvancedScenarios { get; set; } = true;

        /// <summary>
        /// Enable performance testing scenarios
        /// </summary>
        [Description("Enable performance testing scenarios")]
        public bool EnablePerformanceTesting { get; set; } = false;

        /// <summary>
        /// Maximum transaction batch size for demo scenarios
        /// </summary>
        [Description("Maximum transaction batch size for demo scenarios")]
        public int MaxTransactionBatchSize { get; set; } = 2000;

        /// <summary>
        /// Enable multi-data set support
        /// </summary>
        [Description("Enable multi-data set support")]
        public bool EnableMultiDataSetSupport { get; set; } = true;

        /// <summary>
        /// Enable regional customization
        /// </summary>
        [Description("Enable regional customization")]
        public bool EnableRegionalCustomization { get; set; } = true;

        /// <summary>
        /// Enable industry-specific scenarios
        /// </summary>
        [Description("Enable industry-specific scenarios")]
        public bool EnableIndustryScenarios { get; set; } = true;

        /// <summary>
        /// Cache generated demo data for better performance
        /// </summary>
        [Description("Cache generated demo data for better performance")]
        public bool EnableDataCaching { get; set; } = true;

        /// <summary>
        /// Generate realistic demo data with proper relationships
        /// </summary>
        [Description("Generate realistic demo data with proper relationships")]
        public bool EnableRealisticDataGeneration { get; set; } = true;
    }

    /// <summary>
    /// Scenario complexity levels for demo configuration
    /// </summary>
    [Description("Scenario complexity levels for demo configuration")]
    public enum ScenarioComplexity
    {
        /// <summary>
        /// Simple scenarios with minimal data
        /// </summary>
        [Description("Simple scenarios with minimal data")]
        Simple = 1,

        /// <summary>
        /// Medium complexity scenarios with moderate data volume
        /// </summary>
        [Description("Medium complexity scenarios with moderate data volume")]
        Medium = 2,

        /// <summary>
        /// Complex scenarios with large data volume and advanced features
        /// </summary>
        [Description("Complex scenarios with large data volume and advanced features")]
        Complex = 3
    }

    /// <summary>
    /// Advanced feature configuration options
    /// </summary>
    [Description("Advanced feature configuration options")]
    public class AdvancedErpOptions
    {
        /// <summary>
        /// Core ERP options
        /// </summary>
        [Description("Core ERP options")]
        public ErpCoreOptions? CoreOptions { get; set; }

        /// <summary>
        /// Enable workflow engine
        /// </summary>
        [Description("Enable workflow engine")]
        public bool EnableWorkflowEngine { get; set; }

        /// <summary>
        /// Enable reporting services
        /// </summary>
        [Description("Enable reporting services")]
        public bool EnableReporting { get; set; }

        /// <summary>
        /// Enable notification services
        /// </summary>
        [Description("Enable notification services")]
        public bool EnableNotifications { get; set; }

        /// <summary>
        /// Enable integration services
        /// </summary>
        [Description("Enable integration services")]
        public bool EnableIntegrations { get; set; }
    }

    /// <summary>
    /// Production-specific configuration options
    /// </summary>
    [Description("Production-specific configuration options")]
    public class ProductionErpOptions
    {
        /// <summary>
        /// Database provider for production use
        /// </summary>
        [Description("Database provider for production use")]
        public DatabaseProvider DatabaseProvider { get; set; } = DatabaseProvider.SqlServer;

        /// <summary>
        /// Default time zone for production
        /// </summary>
        [Description("Default time zone for production")]
        public string DefaultTimeZone { get; set; } = "UTC";

        /// <summary>
        /// Enable performance logging in production
        /// </summary>
        [Description("Enable performance logging in production")]
        public bool EnablePerformanceLogging { get; set; } = true;

        /// <summary>
        /// Enable audit logging in production
        /// </summary>
        [Description("Enable audit logging in production")]
        public bool EnableAuditLogging { get; set; } = true;

        /// <summary>
        /// Maximum transaction batch size for production
        /// </summary>
        [Description("Maximum transaction batch size for production")]
        public int MaxTransactionBatchSize { get; set; } = 5000;

        /// <summary>
        /// Enable database connection pooling
        /// </summary>
        [Description("Enable database connection pooling")]
        public bool EnableConnectionPooling { get; set; } = true;

        /// <summary>
        /// Maximum number of database connections in pool
        /// </summary>
        [Description("Maximum number of database connections in pool")]
        public int MaxConnectionPoolSize { get; set; } = 100;
    }

    /// <summary>
    /// Supported database providers
    /// </summary>
    [Description("Supported database providers")]
    public enum DatabaseProvider
    {
        /// <summary>
        /// In-memory database for demos and testing
        /// </summary>
        [Description("In-memory database for demos and testing")]
        InMemory = 1,

        /// <summary>
        /// SQL Server database
        /// </summary>
        [Description("SQL Server database")]
        SqlServer = 2,

        /// <summary>
        /// PostgreSQL database
        /// </summary>
        [Description("PostgreSQL database")]
        PostgreSQL = 3,

        /// <summary>
        /// MySQL database
        /// </summary>
        [Description("MySQL database")]
        MySQL = 4,

        /// <summary>
        /// SQLite database
        /// </summary>
        [Description("SQLite database")]
        SQLite = 5
    }
}