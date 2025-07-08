using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Sivar.Erp.EfCore.Context;
using Sivar.Erp.EfCore.UnitOfWork;
using Sivar.Erp.EfCore.Repositories;
using Sivar.Erp.EfCore.Adapters;
using Sivar.Erp.Services;

namespace Sivar.Erp.EfCore.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds ERP modules with Entity Framework Core support while maintaining backward compatibility
        /// </summary>
        public static IServiceCollection AddErpModulesWithEfCore(
            this IServiceCollection services,
            Action<DbContextOptionsBuilder> configureDbContext)
        {
            // Add DbContext
            services.AddDbContext<ErpDbContext>(configureDbContext);

            // Add Repository Pattern
            services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            // Add backward compatibility adapter - THIS IS CRITICAL
            services.AddScoped<IObjectDb, DbContextObjectDbAdapter>();

            return services;
        }

        /// <summary>
        /// Adds ERP modules with SQLite for testing (recommended for CompleteAccountingWorkflowTest)
        /// </summary>
        public static IServiceCollection AddErpModulesWithSqlite(
            this IServiceCollection services,
            string connectionString = "Data Source=:memory:")
        {
            return services.AddErpModulesWithEfCore(options =>
            {
                options.UseSqlite(connectionString);
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            });
        }

        /// <summary>
        /// Adds ERP modules with SQL Server
        /// </summary>
        public static IServiceCollection AddErpModulesWithSqlServer(
            this IServiceCollection services,
            string connectionString)
        {
            return services.AddErpModulesWithEfCore(options =>
            {
                options.UseSqlServer(connectionString);
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            });
        }

        /// <summary>
        /// Adds ERP modules with In-Memory database (for unit testing)
        /// </summary>
        public static IServiceCollection AddErpModulesWithInMemoryDatabase(
            this IServiceCollection services,
            string databaseName = "TestErpDatabase")
        {
            return services.AddErpModulesWithEfCore(options =>
            {
                options.UseInMemoryDatabase(databaseName);
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            });
        }
    }
}
