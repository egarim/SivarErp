using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace Sivar.Erp.Core.Tests.Integration;

/// <summary>
/// Base class for API integration tests
/// </summary>
public class ApiTestBase : IDisposable
{
    protected readonly WebApplicationFactory<Program> Factory;
    protected readonly HttpClient Client;
    protected readonly JsonSerializerOptions JsonOptions;
    protected readonly IServiceProvider ServiceProvider;

    public ApiTestBase()
    {
        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureServices(services =>
                {
                    // Override services for testing if needed
                    services.AddLogging(logging => logging.AddConsole());
                });
            });

        Client = Factory.CreateClient();
        ServiceProvider = Factory.Services;
        
        JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        // Set default company header for all requests
        Client.DefaultRequestHeaders.Add("CompanyId", TestConstants.DefaultCompanyId);
    }

    protected async Task<T?> GetAsync<T>(string endpoint)
    {
        var response = await Client.GetAsync(endpoint);
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json, JsonOptions);
    }

    protected async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest request)
    {
        var json = JsonSerializer.Serialize(request, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await Client.PostAsync(endpoint, content);
        response.EnsureSuccessStatusCode();
        
        var responseJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<TResponse>(responseJson, JsonOptions);
    }

    protected async Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest request)
    {
        var json = JsonSerializer.Serialize(request, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await Client.PutAsync(endpoint, content);
        response.EnsureSuccessStatusCode();
        
        var responseJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<TResponse>(responseJson, JsonOptions);
    }

    protected async Task DeleteAsync(string endpoint)
    {
        var response = await Client.DeleteAsync(endpoint);
        response.EnsureSuccessStatusCode();
    }

    public void Dispose()
    {
        Client?.Dispose();
        Factory?.Dispose();
    }
}

/// <summary>
/// Test constants
/// </summary>
public static class TestConstants
{
    public const string DefaultCompanyId = "12345678-1234-1234-1234-123456789012";
    public const string DefaultUserId = "87654321-4321-4321-4321-210987654321";
}
