using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Sivar.Erp.Core.Infrastructure.Data;
using Sivar.Erp.Core.Infrastructure.Repositories;
using Sivar.Erp.Core.Infrastructure.Services;
using Sivar.Erp.Core.Shared.Interfaces;

namespace Sivar.Erp.Core.Infrastructure.Extensions;

/// <summary>
/// Extension methods for configuring ERP Core services
/// </summary>
public static class ErpCoreServiceExtensions
{
    /// <summary>
    /// Registers ERP Core infrastructure services
    /// </summary>
    public static IServiceCollection AddErpCoreInfrastructure(this IServiceCollection services, string connectionString)
    {
        // Register Entity Framework DbContext
        services.AddDbContext<ErpDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Register Repository Pattern
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register Application Services
        services.AddScoped<IInventoryService, BasicInventoryService>();
        services.AddScoped<ISalesOrderService, BasicSalesOrderService>();

        return services;
    }

    /// <summary>
    /// Registers ERP Core services for testing with in-memory database
    /// </summary>
    public static IServiceCollection AddErpCoreInfrastructureInMemory(this IServiceCollection services)
    {
        // Register Entity Framework DbContext with in-memory database
        services.AddDbContext<ErpDbContext>(options =>
            options.UseInMemoryDatabase("ErpTestDb"));

        // Register Repository Pattern
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register Application Services
        services.AddScoped<IInventoryService, BasicInventoryService>();
        services.AddScoped<ISalesOrderService, BasicSalesOrderService>();

        return services;
    }
}
