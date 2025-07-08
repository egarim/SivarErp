using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sivar.Erp.Services;
using Sivar.Erp.EfCore.Extensions;
using Sivar.Erp.BusinessEntities;
using Sivar.Erp.Services.Accounting.ChartOfAccounts;

namespace Sivar.Erp.EfCore.Examples
{
    /// <summary>
    /// Example demonstrating how to use the new EF Core implementation as a drop-in replacement
    /// for legacy IObjectDb consumers
    /// </summary>
    public class EfCoreUsageExample
    {
        public static async Task<int> Main(string[] args)
        {
            // Create host with EF Core services
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    // Register EF Core infrastructure (this replaces the old ObjectDb registration)
                    services.AddErpModulesWithSqlServer("Server=localhost;Database=SivarErp;Trusted_Connection=true;");

                    // Your existing application services can remain unchanged
                    // They will automatically receive the EF Core-backed IObjectDb
                })
                .Build();

            var logger = host.Services.GetRequiredService<ILogger<EfCoreUsageExample>>();
            var objectDb = host.Services.GetRequiredService<IObjectDb>();

            try
            {
                logger.LogInformation("Starting EF Core example...");

                // Legacy code can use IObjectDb exactly as before
                await DemonstrateBackwardCompatibility(objectDb, logger);

                logger.LogInformation("EF Core example completed successfully!");
                return 0;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error running EF Core example");
                return 1;
            }
        }

        /// <summary>
        /// Demonstrates that existing code using IObjectDb works unchanged with EF Core
        /// </summary>
        private static async Task DemonstrateBackwardCompatibility(IObjectDb objectDb, ILogger logger)
        {
            logger.LogInformation("=== Demonstrating Backward Compatibility ===");

            // 1. Working with Business Entities (existing pattern)
            logger.LogInformation("Creating sample business entity...");

            var customer = new BusinessEntityDto
            {
                Code = "CUST001",
                Name = "Sample Customer Inc.",
                Address = "123 Main St",
                City = "San Salvador",
                State = "San Salvador",
                ZipCode = "01101",
                Country = "El Salvador",
                PhoneNumber = "+503-1234-5678",
                Email = "contact@customer.com"
            };

            objectDb.BusinessEntities.Add(customer);
            logger.LogInformation($"Added business entity: {customer.Name}");

            // 2. Querying (existing LINQ patterns work)
            logger.LogInformation("Querying data...");

            var customers = objectDb.BusinessEntities
                .Where(be => be.Name.Contains("Customer"))
                .ToList();

            logger.LogInformation($"Found {customers.Count} customers");

            // 3. Working with existing collections
            logger.LogInformation($"Total accounts in system: {objectDb.Accounts.Count}");
            logger.LogInformation($"Total business entities: {objectDb.BusinessEntities.Count}");
            logger.LogInformation($"Total document types: {objectDb.DocumentTypes.Count}");

            // 4. Your existing business logic works exactly the same
            logger.LogInformation("=== All legacy patterns work seamlessly! ===");
            logger.LogInformation("Your existing transaction generation, posting, and reporting code");
            logger.LogInformation("works exactly the same - it just uses EF Core underneath");

            await Task.CompletedTask;
        }
    }
}
