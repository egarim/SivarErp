using Sivar.Erp.Core.Domain.Entities.Identity;

namespace Sivar.Erp.Core.Domain.Interfaces;

/// <summary>
/// Interface for accessing the current company context
/// </summary>
public interface ICompanyContext
{
    /// <summary>
    /// Gets the current company ID
    /// </summary>
    Guid? CompanyId { get; }
    
    /// <summary>
    /// Gets the current company information
    /// </summary>
    Company? Company { get; }
    
    /// <summary>
    /// Sets the current company context
    /// </summary>
    /// <param name="companyId">Company ID</param>
    Task SetCompanyAsync(Guid companyId);
    
    /// <summary>
    /// Sets the current company context with full company information
    /// </summary>
    /// <param name="company">Company entity</param>
    void SetCompany(Company company);
    
    /// <summary>
    /// Clears the current company context
    /// </summary>
    void ClearCompany();
    
    /// <summary>
    /// Checks if the current context has a valid company
    /// </summary>
    bool HasCompany { get; }
    
    /// <summary>
    /// Event raised when the company context changes
    /// </summary>
    event EventHandler<CompanyContextChangedEventArgs>? CompanyChanged;
}

/// <summary>
/// Interface for accessing the current branch context
/// </summary>
public interface IBranchContext
{
    /// <summary>
    /// Gets the current branch ID
    /// </summary>
    Guid? BranchId { get; }
    
    /// <summary>
    /// Gets the current branch information
    /// </summary>
    Branch? Branch { get; }
    
    /// <summary>
    /// Sets the current branch context
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    Task SetBranchAsync(Guid branchId);
    
    /// <summary>
    /// Sets the current branch context with full branch information
    /// </summary>
    /// <param name="branch">Branch entity</param>
    void SetBranch(Branch branch);
    
    /// <summary>
    /// Clears the current branch context
    /// </summary>
    void ClearBranch();
    
    /// <summary>
    /// Checks if the current context has a valid branch
    /// </summary>
    bool HasBranch { get; }
    
    /// <summary>
    /// Event raised when the branch context changes
    /// </summary>
    event EventHandler<BranchContextChangedEventArgs>? BranchChanged;
}

/// <summary>
/// Interface for accessing the current user context
/// </summary>
public interface IUserContext
{
    /// <summary>
    /// Gets the current user ID (Keycloak user ID)
    /// </summary>
    string? UserId { get; }
    
    /// <summary>
    /// Gets the current user information
    /// </summary>
    User? User { get; }
    
    /// <summary>
    /// Gets the current user's email
    /// </summary>
    string? Email { get; }
    
    /// <summary>
    /// Gets the current user's display name
    /// </summary>
    string? DisplayName { get; }
    
    /// <summary>
    /// Gets the current user's preferred language
    /// </summary>
    string? PreferredLanguage { get; }
    
    /// <summary>
    /// Sets the current user context
    /// </summary>
    /// <param name="userId">User ID</param>
    Task SetUserAsync(string userId);
    
    /// <summary>
    /// Sets the current user context with full user information
    /// </summary>
    /// <param name="user">User entity</param>
    void SetUser(User user);
    
    /// <summary>
    /// Clears the current user context
    /// </summary>
    void ClearUser();
    
    /// <summary>
    /// Checks if the current context has a valid user
    /// </summary>
    bool HasUser { get; }
    
    /// <summary>
    /// Gets the user's companies
    /// </summary>
    Task<IEnumerable<UserCompany>> GetUserCompaniesAsync();
    
    /// <summary>
    /// Gets the user's role in the current company
    /// </summary>
    /// <param name="companyId">Company ID</param>
    Task<UserCompany?> GetUserCompanyAsync(Guid companyId);
    
    /// <summary>
    /// Checks if the user has a specific role in the current company
    /// </summary>
    /// <param name="companyId">Company ID</param>
    /// <param name="role">Required role</param>
    Task<bool> HasRoleInCompanyAsync(Guid companyId, Enums.CompanyRole role);
    
    /// <summary>
    /// Event raised when the user context changes
    /// </summary>
    event EventHandler<UserContextChangedEventArgs>? UserChanged;
}

/// <summary>
/// Event arguments for company context changes
/// </summary>
public class CompanyContextChangedEventArgs : EventArgs
{
    public Guid? PreviousCompanyId { get; }
    public Guid? NewCompanyId { get; }
    public Company? PreviousCompany { get; }
    public Company? NewCompany { get; }

    public CompanyContextChangedEventArgs(Guid? previousCompanyId, Guid? newCompanyId, Company? previousCompany = null, Company? newCompany = null)
    {
        PreviousCompanyId = previousCompanyId;
        NewCompanyId = newCompanyId;
        PreviousCompany = previousCompany;
        NewCompany = newCompany;
    }
}

/// <summary>
/// Event arguments for branch context changes
/// </summary>
public class BranchContextChangedEventArgs : EventArgs
{
    public Guid? PreviousBranchId { get; }
    public Guid? NewBranchId { get; }
    public Branch? PreviousBranch { get; }
    public Branch? NewBranch { get; }

    public BranchContextChangedEventArgs(Guid? previousBranchId, Guid? newBranchId, Branch? previousBranch = null, Branch? newBranch = null)
    {
        PreviousBranchId = previousBranchId;
        NewBranchId = newBranchId;
        PreviousBranch = previousBranch;
        NewBranch = newBranch;
    }
}

/// <summary>
/// Event arguments for user context changes
/// </summary>
public class UserContextChangedEventArgs : EventArgs
{
    public string? PreviousUserId { get; }
    public string? NewUserId { get; }
    public User? PreviousUser { get; }
    public User? NewUser { get; }

    public UserContextChangedEventArgs(string? previousUserId, string? newUserId, User? previousUser = null, User? newUser = null)
    {
        PreviousUserId = previousUserId;
        NewUserId = newUserId;
        PreviousUser = previousUser;
        NewUser = newUser;
    }
}
