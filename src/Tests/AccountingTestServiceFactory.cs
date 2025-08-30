using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;


using Sivar.Erp.Modules.Documents.Core.Interfaces;
using Sivar.Erp.Modules.ImportExport;
using Sivar.Erp.Modules.Inventory.Application.Validators;

using Sivar.Erp.ErpSystem.Options;
using Sivar.Erp.ErpSystem.Sequencers;
using Sivar.Erp.ErpSystem.TimeService;
using Sivar.Erp.Infrastructure.ImportExport.Documents;

using Sivar.Erp.Modules;
using Sivar.Erp.Modules.Accounting.ChartOfAccounts;
using Sivar.Erp.Modules.Accounting.JournalEntries;
using Sivar.Erp.Modules.Accounting.Reports;
using Sivar.Erp.Modules.Documents.Application.Services;
using Sivar.Erp.Modules.Payments.Services;
using Sivar.Erp.Modules.Taxes.TaxAccountingProfiles;
using Sivar.Erp.Modules.Taxes.TaxGroup;
using Sivar.Erp.Modules.Taxes.TaxRule;
using System;

namespace Tests
{
    /// <summary>
    /// Factory for creating and configuring services needed for accounting tests
    /// </summary>
    public static class AccountingTestServiceFactory
    {
        /// <summary>
        /// Configures all services needed for accounting tests
        /// </summary>
        /// <returns>Configured ServiceCollection</returns>
        public static ServiceCollection ConfigureServices()
        {
            var services = new ServiceCollection();

            // Register logging services
            RegisterLoggingServices(services);

            // Register validators with specific configurations
            RegisterValidators(services);

            // Register import/export services
            RegisterImportExportServices(services);

            // Register core ERP services
            RegisterCoreServices(services);

            // Register accounting-specific services
            RegisterAccountingServices(services);

            // Register helper and utility services
            RegisterHelperServices(services);

            return services;
        }

        /// <summary>
        /// Creates a configured service provider ready for use in tests
        /// </summary>
        /// <returns>Configured IServiceProvider</returns>
        public static IServiceProvider CreateServiceProvider()
        {
            return ConfigureServices().BuildServiceProvider();
        }

        /// <summary>
        /// Creates a service provider with custom configuration options
        /// </summary>
        /// <param name="configureServices">Action to customize service registration</param>
        /// <returns>Configured IServiceProvider</returns>
        public static IServiceProvider CreateServiceProvider(Action<ServiceCollection> configureServices)
        {
            var services = ConfigureServices();
            configureServices?.Invoke(services);
            return services.BuildServiceProvider();
        }

        private static void RegisterLoggingServices(ServiceCollection services)
        {
            // Add logging services
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Debug);
            });

            // Add generic ILogger<T> resolution
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
        }

        private static void RegisterValidators(ServiceCollection services)
        {
            // Register El Salvador specific account validator
            services.AddSingleton<AccountValidator>(provider =>
                new AccountValidator(AccountValidator.GetElSalvadorAccountTypePrefixes()));

            // Register other validators
            services.AddSingleton<TaxRuleValidator>();
            services.AddSingleton<ItemValidator>();
            services.AddSingleton<GroupMembershipValidator>();
        }

        private static void RegisterImportExportServices(ServiceCollection services)
        {
            // Import/Export services for data loading
            services.AddTransient<IAccountImportExportService>(provider =>
                new AccountImportExportService(provider.GetRequiredService<AccountValidator>()));

            services.AddTransient<ITaxImportExportService, TaxImportExportService>();
            services.AddTransient<ITaxGroupImportExportService, TaxGroupImportExportService>();
            services.AddTransient<IDocumentTypeImportExportService, DocumentTypeImportExportService>();
            services.AddTransient<IBusinessEntityImportExportService, BusinessEntityImportExportService>();

            // Register Core.Contracts interface directly - NO MORE ADAPTERS!
            services.AddTransient<IItemImportExportService, ItemImportExportService>();

            services.AddTransient<IGroupMembershipImportExportService>(provider =>
                new GroupMembershipImportExportService(provider.GetRequiredService<GroupMembershipValidator>())); 
                
            // DocumentTotalsService is currently commented out, so skip registration
            // services.AddTransient<IDocumentTotalsService>(sp =>
            // {
            //     var objectDb = sp.GetRequiredService<IObjectDb>();
            //     var dateTimeService = sp.GetRequiredService<IDateTimeZoneService>();
            //     var logger = sp.GetRequiredService<ILogger<DocumentTotalsService>>();
            //     return new DocumentTotalsService(objectDb, dateTimeService, logger);
            // });

            // Register the document accounting profile services
            services.AddTransient<IDocumentAccountingProfileService>(sp =>
            {
                var objectDb = sp.GetRequiredService<IObjectDb>();
                var logger = sp.GetRequiredService<ILogger<DocumentAccountingProfileService>>();
                return new DocumentAccountingProfileService(objectDb, logger);
            });

            // Register Core.Contracts interface directly - NO MORE ADAPTERS!
            services.AddTransient<IDocumentAccountingProfileImportExportService, DocumentAccountingProfileImportExportService>();

            services.AddTransient<ITaxRuleImportExportService>(provider =>
                new TaxRuleImportExportService(provider.GetRequiredService<TaxRuleValidator>()));

            services.AddTransient<ITestAccountMappingImportExportService, TestAccountMappingImportExportService>();
        }

        private static void RegisterCoreServices(ServiceCollection services)
        {
            // Core ERP system services
            services.AddSingleton<IDateTimeZoneService, DateTimeZoneService>();
            services.AddSingleton<IOptionService, OptionService>();

            // Note: SequencerService and ActivityStreamService require ObjectDb, 
            // so they'll be created manually in the test with the specific ObjectDb instance
        }
        private static void RegisterAccountingServices(ServiceCollection services)
        {
            // Tax and accounting services
            services.AddTransient<ITaxAccountingProfileService, TaxAccountingProfileService>();
            services.AddTransient<ITaxAccountingProfileImportExportService, TaxAccountingProfileImportExportService>();

            // Payment services
            services.AddTransient<IPaymentService>(provider =>
            {
                var objectDb = provider.GetRequiredService<IObjectDb>();
                var logger = provider.GetRequiredService<ILogger<PaymentService>>();
                // Account mappings will be injected later in the test after import
                var accountMappings = new Dictionary<string, string>();
                return new PaymentService(objectDb, logger, accountMappings);
            });

            services.AddTransient<IPaymentMethodService>(provider =>
            {
                var objectDb = provider.GetRequiredService<IObjectDb>();
                var logger = provider.GetRequiredService<ILogger<PaymentMethodService>>();
                return new PaymentMethodService(objectDb, logger);
            });

            // Journal Entry services
            services.AddTransient<IJournalEntryService>(provider =>
            {
                var objectDb = provider.GetRequiredService<IObjectDb>();
                var logger = provider.GetRequiredService<ILogger<JournalEntryService>>();
                return new JournalEntryService(logger, objectDb);
            }); services.AddTransient<IJournalEntryReportService>(provider =>
            {
                var objectDb = provider.GetRequiredService<IObjectDb>();
                var journalEntryService = provider.GetRequiredService<IJournalEntryService>();
                var logger = provider.GetRequiredService<ILogger<JournalEntryReportService>>();
                return new JournalEntryReportService(logger, objectDb, journalEntryService);
            });

            // Transaction services will be created manually as they need specific configurations
        }

        private static void RegisterHelperServices(ServiceCollection services)
        {
            // Data import helper - will be resolved with all its dependencies
            services.AddTransient<DataImportHelper>(provider =>
                new DataImportHelper(
                    provider.GetRequiredService<IAccountImportExportService>(),
                    provider.GetRequiredService<ITaxImportExportService>(),
                    provider.GetRequiredService<ITaxGroupImportExportService>(),
                    provider.GetRequiredService<IDocumentTypeImportExportService>(),
                    provider.GetRequiredService<IBusinessEntityImportExportService>(),
                    provider.GetRequiredService<IItemImportExportService>(), // Use consolidated interface
                    provider.GetRequiredService<IGroupMembershipImportExportService>(),
                    provider.GetRequiredService<ITaxRuleImportExportService>(),
                    "TestUser"));
        }
    }
}