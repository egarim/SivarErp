using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Sivar.Erp.Core.Client.Services.Abstractions;
using Sivar.Erp.Core.Client.Services.Http;
using Sivar.Erp.Core.Client.Services.Sales;
using Sivar.Erp.Core.Client.Services.Inventory;
using Sivar.Erp.Core.Client.Services.Purchasing;

namespace Sivar.Erp.Core.Client.Services.Extensions;

/// <summary>
/// Extension methods for configuring client services in DI container
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all API client services to the service collection
    /// </summary>
    public static IServiceCollection AddSivarErpClientServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Get API base URL from configuration
        var apiBaseUrl = configuration["SivarErp:ApiBaseUrl"] ?? "http://localhost:5000";

        // Add HTTP clients for each service
        services.AddHttpClient<ICompanyApiService, CompanyApiService>(client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        services.AddHttpClient<IAccountingApiService, AccountingApiService>(client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        // Sales module services
        services.AddHttpClient<ISalesOrderApiService, SalesOrderApiService>(client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        services.AddHttpClient<IInvoiceApiService, InvoiceApiService>(client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        // Inventory module services
        services.AddHttpClient<IInventoryApiService, InventoryApiService>(client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        // Purchasing module services
        services.AddHttpClient<IPurchaseOrderApiService, PurchaseOrderApiService>(client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        return services;
    }

    /// <summary>
    /// Adds API client services with authentication support
    /// </summary>
    public static IServiceCollection AddSivarErpClientServicesWithAuth(
        this IServiceCollection services,
        IConfiguration configuration,
        Func<IServiceProvider, Task<string?>> tokenProvider)
    {
        var apiBaseUrl = configuration["SivarErp:ApiBaseUrl"] ?? "http://localhost:5000";

        // Add HTTP clients with authentication
        services.AddHttpClient<ICompanyApiService, CompanyApiService>(client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        services.AddHttpClient<IAccountingApiService, AccountingApiService>(client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        // TODO: Add authentication handlers when implementing Keycloak integration

        return services;
    }

    /// <summary>
    /// Adds API client services for testing scenarios (with mock base URL)
    /// </summary>
    public static IServiceCollection AddSivarErpClientServicesForTesting(
        this IServiceCollection services,
        string testApiBaseUrl = "http://localhost:5000")
    {
        services.AddHttpClient<ICompanyApiService, CompanyApiService>(client =>
        {
            client.BaseAddress = new Uri(testApiBaseUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        services.AddHttpClient<IAccountingApiService, AccountingApiService>(client =>
        {
            client.BaseAddress = new Uri(testApiBaseUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        return services;
    }
}
