namespace Sivar.Erp.Core.Domain.Enums;

/// <summary>
/// Roles a user can have within a company
/// </summary>
public enum CompanyRole
{
    /// <summary>
    /// Regular user with basic permissions
    /// </summary>
    User = 0,
    
    /// <summary>
    /// Administrator with elevated permissions
    /// </summary>
    Admin = 1,
    
    /// <summary>
    /// Owner with full control over the company
    /// </summary>
    Owner = 2,
    
    /// <summary>
    /// Manager with permissions over specific areas/branches
    /// </summary>
    Manager = 3,
    
    /// <summary>
    /// Accountant with accounting-specific permissions
    /// </summary>
    Accountant = 4,
    
    /// <summary>
    /// Auditor with read-only access for auditing purposes
    /// </summary>
    Auditor = 5
}

/// <summary>
/// Roles a user can have within a branch
/// </summary>
public enum BranchRole
{
    /// <summary>
    /// Regular employee
    /// </summary>
    Employee = 0,
    
    /// <summary>
    /// Branch manager
    /// </summary>
    Manager = 1,
    
    /// <summary>
    /// Assistant manager
    /// </summary>
    AssistantManager = 2,
    
    /// <summary>
    /// Supervisor
    /// </summary>
    Supervisor = 3
}

/// <summary>
/// Status of user invitations
/// </summary>
public enum InvitationStatus
{
    /// <summary>
    /// Invitation has been sent but not yet responded to
    /// </summary>
    Pending = 0,
    
    /// <summary>
    /// Invitation has been accepted
    /// </summary>
    Accepted = 1,
    
    /// <summary>
    /// Invitation has been declined
    /// </summary>
    Declined = 2,
    
    /// <summary>
    /// Invitation has expired
    /// </summary>
    Expired = 3,
    
    /// <summary>
    /// Invitation has been cancelled
    /// </summary>
    Cancelled = 4
}

/// <summary>
/// Types of permissions within the system
/// </summary>
public enum PermissionType
{
    /// <summary>
    /// Read-only access
    /// </summary>
    Read = 0,
    
    /// <summary>
    /// Write/create access
    /// </summary>
    Write = 1,
    
    /// <summary>
    /// Delete access
    /// </summary>
    Delete = 2,
    
    /// <summary>
    /// Administrative access
    /// </summary>
    Admin = 3,
    
    /// <summary>
    /// Full access (all permissions)
    /// </summary>
    Full = 4
}
