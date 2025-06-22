using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sivar.Erp.ErpSystem.Modules.Security;
using Sivar.Erp.ErpSystem.Modules.Security.Core;
using Sivar.Erp.ErpSystem.Modules.Security.Platform;
using Sivar.Erp.ErpSystem.Modules.Security.Extensions;
using Sivar.Erp.Services;

namespace Sivar.Erp.Examples
{
    /// <summary>
    /// Example showing how to use the Security Module in different scenarios
    /// </summary>
    public class SecurityModuleUsageExample
    {        /// <summary>
             /// Example of setting up and using the security module in a test environment
             /// </summary>
        public static async Task<bool> ExampleUsage()
        {
            // Note: In a real application, you would configure this through your DI container
            // This is simplified for demonstration purposes

            Console.WriteLine("Security Module Usage Example");
            Console.WriteLine("=============================");

            // This example shows the concepts - in practice you'd use your DI container
            Console.WriteLine("✓ Security module setup would be done through DI container");
            Console.WriteLine("✓ Example operations: authenticate, authorize, audit");
            Console.WriteLine("✓ See SecurityModule.cs and ServiceCollectionExtensions.cs for actual implementation");

            return await Task.FromResult(true);
        }

        /// <summary>
        /// Example of checking permissions before executing business operations
        /// </summary>
        public static async Task<SecurityResult> SecureBusinessOperationExample(
            ISecurityModule securityModule,
            string operation,
            Func<Task> businessOperation)
        {
            // Check if user has permission
            var authResult = await securityModule.CheckBusinessOperationAsync(operation);

            if (!authResult.IsAuthorized)
            {
                Console.WriteLine($"Access denied for operation '{operation}': {authResult.DenialReason}");
                return authResult;
            }

            try
            {
                // Execute the business operation
                await businessOperation();
                Console.WriteLine($"Business operation '{operation}' completed successfully");
                return SecurityResult.Success();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing business operation '{operation}': {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Example of customizing the security module for a specific platform
        /// </summary>
        public static class CustomSecurityContextExample
        {
            public class CustomSecurityContext : GenericSecurityContext
            {
                public string? CompanyId { get; private set; }
                public string? Department { get; private set; }

                public override void SetCurrentUser(IUser user)
                {
                    base.SetCurrentUser(user);

                    // Add custom properties if the user is our concrete implementation
                    if (user is User concreteUser)
                    {
                        CompanyId = concreteUser.Properties.TryGetValue("CompanyId", out var companyId)
                            ? companyId?.ToString() : null;
                        Department = concreteUser.Properties.TryGetValue("Department", out var dept)
                            ? dept?.ToString() : null;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Extension methods for custom security module configuration
    /// </summary>
    public static class CustomSecurityExtensions
    {
        public static IServiceCollection AddCustomSecurityModule(this IServiceCollection services)
        {
            return services.AddErpSecurityModule<SecurityModule, SecurityModuleUsageExample.CustomSecurityContextExample.CustomSecurityContext>();
        }
    }
}
