using Microsoft.Extensions.DependencyInjection;
using Sivar.Erp.Core.Application.Services.Sales;
using Sivar.Erp.Core.Application.Services.Inventory;
using Sivar.Erp.Core.Application.Services.Purchasing;
using Sivar.Erp.Core.Shared.Interfaces;

namespace Sivar.Erp.Core.Infrastructure.Extensions;

/// <summary>
/// Extension methods for registering application services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all ERP application services
    /// </summary>
    public static IServiceCollection AddErpApplicationServices(this IServiceCollection services)
    {
        // Sales Services
        services.AddScoped<ISalesOrderService, SalesOrderService>();
        services.AddScoped<IInvoiceService, InvoiceService>();

        // Inventory Services
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IStockLevelService, StockLevelService>();
        services.AddScoped<IInventoryTransactionService, InventoryTransactionService>();

        // Purchasing Services
        services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
        services.AddScoped<ISupplierService, SupplierService>();
        // Note: IPurchaseInvoiceService would be implemented similar to InvoiceService but for purchases

        return services;
    }

    /// <summary>
    /// Registers sales-specific services
    /// </summary>
    public static IServiceCollection AddSalesServices(this IServiceCollection services)
    {
        services.AddScoped<ISalesOrderService, SalesOrderService>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        return services;
    }

    /// <summary>
    /// Registers inventory-specific services
    /// </summary>
    public static IServiceCollection AddInventoryServices(this IServiceCollection services)
    {
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IStockLevelService, StockLevelService>();
        services.AddScoped<IInventoryTransactionService, InventoryTransactionService>();
        return services;
    }

    /// <summary>
    /// Registers purchasing-specific services
    /// </summary>
    public static IServiceCollection AddPurchasingServices(this IServiceCollection services)
    {
        services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
        services.AddScoped<ISupplierService, SupplierService>();
        return services;
    }
}
