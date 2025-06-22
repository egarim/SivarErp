using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sivar.Erp.ErpSystem.Modules.Security.Core;
using Sivar.Erp.ErpSystem.Modules.Security.Platform;
using Sivar.Erp.ErpSystem.Modules.Security.Services;

namespace Sivar.Erp.ErpSystem.Modules.Security.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the ERP Security Module with default generic platform implementation
        /// </summary>
        public static IServiceCollection AddErpSecurityModule(this IServiceCollection services)
        {
            return services.AddErpSecurityModule<SecurityModule, GenericSecurityContext>();
        }

        /// <summary>
        /// Adds the ERP Security Module with custom security module and security context implementations
        /// </summary>
        public static IServiceCollection AddErpSecurityModule<TSecurityModule, TSecurityContext>(this IServiceCollection services)
            where TSecurityModule : class, ISecurityModule
            where TSecurityContext : class, ISecurityContext
        {
            // Core security services
            services.AddSingleton<TSecurityContext>();
            services.AddSingleton<ISecurityContext>(provider => provider.GetRequiredService<TSecurityContext>());

            services.AddScoped<IAuthorizationService, AuthorizationService>();
            services.AddScoped<IUserService, ObjectDbUserService>();
            services.AddScoped<IRoleService, ObjectDbRoleService>();
            services.AddScoped<IPermissionService, PermissionService>();
            services.AddScoped<IAuditService, ObjectDbAuditService>();

            // Security module
            services.AddScoped<TSecurityModule>();
            services.AddScoped<ISecurityModule>(provider => provider.GetRequiredService<TSecurityModule>());

            return services;
        }

        /// <summary>
        /// Adds the ERP Security Module with custom security module and default generic security context
        /// </summary>
        public static IServiceCollection AddErpSecurityModule<TSecurityModule>(this IServiceCollection services)
            where TSecurityModule : class, ISecurityModule
        {
            return services.AddErpSecurityModule<TSecurityModule, GenericSecurityContext>();
        }

        /// <summary>
        /// Adds custom implementations for specific security services while keeping others as default
        /// </summary>
        public static IServiceCollection AddCustomSecurityService<TInterface, TImplementation>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped)
            where TInterface : class
            where TImplementation : class, TInterface
        {
            services.Add(new ServiceDescriptor(typeof(TInterface), typeof(TImplementation), lifetime));
            return services;
        }
    }
}
