namespace Sivar.Erp.ErpSystem.Modules.Security.Core
{
    public interface IUserService
    {
        Task<AuthenticationResult> AuthenticateAsync(string username, string password);
        Task<IUser?> GetUserByIdAsync(string userId);
        Task<IUser?> GetUserByUsernameAsync(string username);
        Task<IEnumerable<IUser>> GetAllUsersAsync();
        Task<IUser> CreateUserAsync(CreateUserRequest request);
        Task<IUser> UpdateUserAsync(string userId, UpdateUserRequest request);
        Task DeleteUserAsync(string userId);
        Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
        Task<bool> IsUserActiveAsync(string userId);
        Task SetUserActiveStatusAsync(string userId, bool isActive);
    }

    public class CreateUserRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public List<string> DirectPermissions { get; set; } = new();
        public bool IsActive { get; set; } = true;
    }

    public class UpdateUserRequest
    {
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public List<string>? Roles { get; set; }
        public List<string>? DirectPermissions { get; set; }
        public bool? IsActive { get; set; }
    }

    public class AuthenticationResult
    {
        public bool IsSuccessful { get; set; }
        public IUser? User { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime? LastLoginDate { get; set; }

        public static AuthenticationResult Success(IUser user) =>
            new() { IsSuccessful = true, User = user };

        public static AuthenticationResult Failed(string errorMessage) =>
            new() { IsSuccessful = false, ErrorMessage = errorMessage };
    }
}
