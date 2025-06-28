using System;
using Microsoft.Extensions.DependencyInjection;
using Sivar.Erp.Modules.Inventory.Reports;

namespace Sivar.Erp.Modules.Inventory.Extensions
{
    /// <summary>
    /// Extensions for registering inventory module services
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds inventory module services to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddInventoryModule(this IServiceCollection services)
        {
            // Register services
            services.AddSingleton<IInventoryService, InventoryService>();
            services.AddSingleton<IInventoryReservationService, InventoryReservationService>();
            services.AddSingleton<IKardexService, KardexService>();
            
            // Register the module
            services.AddSingleton<IInventoryModule, InventoryModule>();

            return services;
        }
    }
}