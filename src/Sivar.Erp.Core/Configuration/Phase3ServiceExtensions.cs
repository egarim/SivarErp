using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Sivar.Erp.Core.Infrastructure.Localization;
using Sivar.Erp.Core.Infrastructure.AI;
using Sivar.Erp.Core.Infrastructure.Performance;

namespace Sivar.Erp.Core.Configuration
{
    /// <summary>
    /// Extension methods for configuring Phase 3 advanced ERP features
    /// </summary>
    [Description("Extension methods for Phase 3 advanced features")]
    public static class Phase3ServiceExtensions
    {
        /// <summary>
        /// Adds Phase 3 advanced features: Localization, AI Integration, and Performance Monitoring
        /// </summary>
        /// <param name="services">Service collection to configure</param>
        /// <returns>Configured service collection</returns>
        [Description("Adds Phase 3 advanced features")]
        public static IServiceCollection AddSivarErpPhase3Features(this IServiceCollection services)
        {
            // Register Localization services
            services.AddScoped<ILocalizationService, InMemoryLocalizationService>();

            // Register AI Integration services
            services.AddScoped<IErpAiService, ErpAiService>();

            // Register Performance Monitoring services
            services.AddScoped<IPerformanceMonitor, InMemoryPerformanceMonitor>();
            services.AddSingleton<IStringInterningService, StringInterningService>();

            // Configure memory optimization for performance
            services.ConfigureForHighPerformance();

            return services;
        }

        /// <summary>
        /// Adds Phase 3 features optimized for low memory usage
        /// </summary>
        /// <param name="services">Service collection to configure</param>
        /// <returns>Configured service collection</returns>
        [Description("Adds Phase 3 features optimized for low memory")]
        public static IServiceCollection AddSivarErpPhase3FeaturesLowMemory(this IServiceCollection services)
        {
            // Register core Phase 3 services
            services.AddSivarErpPhase3Features();

            // Configure for low memory usage
            services.ConfigureForLowMemory();

            return services;
        }

        /// <summary>
        /// Adds Phase 3 features with AI capabilities enabled
        /// </summary>
        /// <param name="services">Service collection to configure</param>
        /// <param name="enableAdvancedAI">Whether to enable advanced AI features</param>
        /// <returns>Configured service collection</returns>
        [Description("Adds Phase 3 features with AI capabilities")]
        public static IServiceCollection AddSivarErpWithAI(
            this IServiceCollection services,
            bool enableAdvancedAI = false)
        {
            services.AddSivarErpPhase3Features();

            if (enableAdvancedAI)
            {
                // Additional AI configuration would go here
                // For now, just the standard AI service
            }

            return services;
        }
    }
}
