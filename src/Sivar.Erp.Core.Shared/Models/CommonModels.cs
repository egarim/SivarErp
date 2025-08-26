namespace Sivar.Erp.Core.Shared.Models;

/// <summary>
/// Generic API response wrapper
/// </summary>
/// <typeparam name="T">Data type</typeparam>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public string? ErrorCode { get; set; }
    public List<string>? Errors { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse<T> SuccessResult(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message
        };
    }

    public static ApiResponse<T> ErrorResult(string message, string? errorCode = null, List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            ErrorCode = errorCode,
            Errors = errors
        };
    }
}

/// <summary>
/// Non-generic API response
/// </summary>
public class ApiResponse : ApiResponse<object?>
{
    public static ApiResponse Ok(string? message = null)
    {
        return (ApiResponse)SuccessResult(null, message);
    }

    public static new ApiResponse ErrorResult(string message, string? errorCode = null, List<string>? errors = null)
    {
        return new ApiResponse
        {
            Success = false,
            Message = message,
            ErrorCode = errorCode,
            Errors = errors
        };
    }
}

/// <summary>
/// Paginated result wrapper
/// </summary>
/// <typeparam name="T">Item type</typeparam>
public class PaginatedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public static PaginatedResult<T> Create(List<T> items, int totalCount, int pageNumber, int pageSize)
    {
        return new PaginatedResult<T>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}

/// <summary>
/// Company information model
/// </summary>
public class CompanyInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string Currency { get; set; } = "USD";
    public string DefaultLanguage { get; set; } = "en-US";
    public string TimeZone { get; set; } = "UTC";
    public bool IsActive { get; set; } = true;
    public string UserRole { get; set; } = string.Empty;
    public bool IsOwner { get; set; }
}

/// <summary>
/// Branch information model
/// </summary>
public class BranchInfo
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public bool IsHeadquarters { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// User information model
/// </summary>
public class UserInfo
{
    public Guid Id { get; set; }
    public string KeycloakUserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string PreferredLanguage { get; set; } = "en-US";
    public bool IsActive { get; set; } = true;
    public List<CompanyInfo> Companies { get; set; } = new();
}

/// <summary>
/// Authentication result model
/// </summary>
public class AuthenticationResult
{
    public bool Success { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public UserInfo? User { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorCode { get; set; }
}

/// <summary>
/// User invitation request model
/// </summary>
public class InvitationRequest
{
    public string Email { get; set; } = string.Empty;
    public Guid CompanyId { get; set; }
    public string Role { get; set; } = string.Empty;
    public string? Message { get; set; }
    public List<Guid>? BranchIds { get; set; }
    public Dictionary<string, object>? CustomPermissions { get; set; }
}
