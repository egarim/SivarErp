using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;
using Sivar.Erp.Core.Shared.Responses;

namespace Sivar.Erp.Core.Client.Services.Http;

/// <summary>
/// Base class for API client services with common HTTP operations
/// </summary>
public abstract class ApiClientBase
{
    protected readonly HttpClient HttpClient;
    protected readonly ILogger Logger;
    private readonly JsonSerializerOptions _jsonOptions;

    protected ApiClientBase(HttpClient httpClient, ILogger logger)
    {
        HttpClient = httpClient;
        Logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    /// <summary>
    /// Sends a GET request and returns the response
    /// </summary>
    protected async Task<ApiResponse<T>> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("Sending GET request to {Endpoint}", endpoint);
            
            var response = await HttpClient.GetAsync(endpoint, cancellationToken);
            return await ProcessResponse<T>(response, cancellationToken);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during GET request to {Endpoint}", endpoint);
            return ApiResponse<T>.Failure($"Request failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Sends a POST request with JSON body and returns the response
    /// </summary>
    protected async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object? body = null, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("Sending POST request to {Endpoint}", endpoint);
            
            HttpResponseMessage response;
            if (body != null)
            {
                response = await HttpClient.PostAsJsonAsync(endpoint, body, _jsonOptions, cancellationToken);
            }
            else
            {
                response = await HttpClient.PostAsync(endpoint, null, cancellationToken);
            }
            
            return await ProcessResponse<T>(response, cancellationToken);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during POST request to {Endpoint}", endpoint);
            return ApiResponse<T>.Failure($"Request failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Sends a PUT request with JSON body and returns the response
    /// </summary>
    protected async Task<ApiResponse<T>> PutAsync<T>(string endpoint, object body, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("Sending PUT request to {Endpoint}", endpoint);
            
            var response = await HttpClient.PutAsJsonAsync(endpoint, body, _jsonOptions, cancellationToken);
            return await ProcessResponse<T>(response, cancellationToken);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during PUT request to {Endpoint}", endpoint);
            return ApiResponse<T>.Failure($"Request failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Sends a DELETE request and returns the response
    /// </summary>
    protected async Task<ApiResponse<T>> DeleteAsync<T>(string endpoint, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("Sending DELETE request to {Endpoint}", endpoint);
            
            var response = await HttpClient.DeleteAsync(endpoint, cancellationToken);
            return await ProcessResponse<T>(response, cancellationToken);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during DELETE request to {Endpoint}", endpoint);
            return ApiResponse<T>.Failure($"Request failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Processes HTTP response and deserializes to ApiResponse
    /// </summary>
    private async Task<ApiResponse<T>> ProcessResponse<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        
        if (response.IsSuccessStatusCode)
        {
            try
            {
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<T>>(content, _jsonOptions);
                return apiResponse ?? ApiResponse<T>.Failure("Failed to deserialize response");
            }
            catch (JsonException ex)
            {
                Logger.LogError(ex, "Failed to deserialize successful response: {Content}", content);
                return ApiResponse<T>.Failure($"Deserialization failed: {ex.Message}");
            }
        }
        else
        {
            Logger.LogWarning("HTTP request failed with status {StatusCode}: {Content}", 
                response.StatusCode, content);
            
            try
            {
                // Try to deserialize error response
                var errorResponse = JsonSerializer.Deserialize<ApiResponse<T>>(content, _jsonOptions);
                return errorResponse ?? ApiResponse<T>.Failure($"HTTP {response.StatusCode}: {response.ReasonPhrase}");
            }
            catch
            {
                // If deserialization fails, return generic error
                return ApiResponse<T>.Failure($"HTTP {response.StatusCode}: {response.ReasonPhrase}");
            }
        }
    }

    /// <summary>
    /// Sets the company ID header for requests
    /// </summary>
    protected void SetCompanyId(Guid companyId)
    {
        HttpClient.DefaultRequestHeaders.Remove("companyId");
        HttpClient.DefaultRequestHeaders.Add("companyId", companyId.ToString());
    }

    /// <summary>
    /// Sets the branch ID header for requests
    /// </summary>
    protected void SetBranchId(Guid branchId)
    {
        HttpClient.DefaultRequestHeaders.Remove("branchId");
        HttpClient.DefaultRequestHeaders.Add("branchId", branchId.ToString());
    }
}
