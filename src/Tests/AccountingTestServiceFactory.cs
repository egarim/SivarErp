using Microsoft.Extensions.DependencyInjection;
using Sivar.Erp.Documents;
using Sivar.Erp.ErpSystem.ActivityStream;
using Sivar.Erp.ErpSystem.Options;
using Sivar.Erp.ErpSystem.Sequencers;
using Sivar.Erp.ErpSystem.TimeService;
using Sivar.Erp.Modules;
using Sivar.Erp.Services;
using Sivar.Erp.Services.Accounting.ChartOfAccounts;
using Sivar.Erp.Services.ImportExport;
using Sivar.Erp.Services.Taxes.TaxAccountingProfiles;
using Sivar.Erp.Services.Taxes.TaxGroup;
using Sivar.Erp.Services.Taxes.TaxRule;
using System;

namespace Sivar.Erp.Tests.Infrastructure
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

            services.AddTransient<IItemImportExportService>(provider =>
                new ItemImportExportService(provider.GetRequiredService<ItemValidator>()));

            services.AddTransient<IGroupMembershipImportExportService>(provider =>
                new GroupMembershipImportExportService(provider.GetRequiredService<GroupMembershipValidator>()));

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
                    provider.GetRequiredService<IItemImportExportService>(),
                    provider.GetRequiredService<IGroupMembershipImportExportService>(),
                    provider.GetRequiredService<ITaxRuleImportExportService>(),
                    "TestUser"));
        }
    }
}