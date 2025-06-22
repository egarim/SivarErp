using Sivar.Erp.ErpSystem.Modules.Security.Core;
using Sivar.Erp.Services;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace Sivar.Erp.ErpSystem.Modules.Security.Services
{
    public class ObjectDbUserService : IUserService
    {
        private readonly IObjectDb _objectDb;
        private readonly ILogger<ObjectDbUserService> _logger;

        public ObjectDbUserService(IObjectDb objectDb, ILogger<ObjectDbUserService> logger)
        {
            _objectDb = objectDb;
            _logger = logger;
        }

        public async Task<AuthenticationResult> AuthenticateAsync(string username, string password)
        {
            try
            {
                var user = await GetUserByUsernameAsync(username);
                if (user == null)
                {
                    _logger.LogWarning("Authentication failed: User not found - {Username}", username);
                    return AuthenticationResult.Failed("Invalid username or password");
                }

                if (!user.IsActive)
                {
                    _logger.LogWarning("Authentication failed: User inactive - {Username}", username);
                    return AuthenticationResult.Failed("User account is inactive");
                }

                // Verify password (assuming User is the concrete implementation)
                if (user is User concreteUser)
                {
                    var passwordHash = HashPassword(password);
                    if (concreteUser.PasswordHash != passwordHash)
                    {
                        _logger.LogWarning("Authentication failed: Invalid password - {Username}", username);
                        return AuthenticationResult.Failed("Invalid username or password");
                    }

                    // Update last login
                    concreteUser.LastLoginDate = DateTime.UtcNow;
                    // Note: In a real implementation, you'd save this to the database
                }

                _logger.LogInformation("Authentication successful - {Username}", username);
                return AuthenticationResult.Success(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during authentication for user {Username}", username);
                return AuthenticationResult.Failed("Authentication error");
            }
        }

        public async Task<IUser?> GetUserByIdAsync(string userId)
        {
            return await Task.FromResult(_objectDb.Users.FirstOrDefault(u => u.Id == userId));
        }

        public async Task<IUser?> GetUserByUsernameAsync(string username)
        {
            return await Task.FromResult(_objectDb.Users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)));
        }

        public async Task<IEnumerable<IUser>> GetAllUsersAsync()
        {
            return await Task.FromResult(_objectDb.Users.Cast<IUser>());
        }

        public async Task<IUser> CreateUserAsync(CreateUserRequest request)
        {
            // Validate username is unique
            var existingUser = await GetUserByUsernameAsync(request.Username);
            if (existingUser != null)
            {
                throw new InvalidOperationException($"Username '{request.Username}' already exists");
            }

            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                Username = request.Username,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                IsActive = request.IsActive,
                CreatedDate = DateTime.UtcNow,
                PasswordHash = HashPassword(request.Password),
                Roles = new List<string>(request.Roles),
                DirectPermissions = new List<string>(request.DirectPermissions)
            };

            _objectDb.Users.Add(user);
            _logger.LogInformation("Created user: {Username} with ID: {UserId}", user.Username, user.Id);

            return await Task.FromResult(user);
        }

        public async Task<IUser> UpdateUserAsync(string userId, UpdateUserRequest request)
        {
            var user = _objectDb.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                throw new InvalidOperationException($"User with ID '{userId}' not found");
            }

            if (request.Email != null) user.Email = request.Email;
            if (request.FirstName != null) user.FirstName = request.FirstName;
            if (request.LastName != null) user.LastName = request.LastName;
            if (request.IsActive.HasValue) user.IsActive = request.IsActive.Value;

            if (request.Roles != null)
            {
                user.Roles.Clear();
                user.Roles.AddRange(request.Roles);
            }

            if (request.DirectPermissions != null)
            {
                user.DirectPermissions.Clear();
                user.DirectPermissions.AddRange(request.DirectPermissions);
            }

            _logger.LogInformation("Updated user: {Username} with ID: {UserId}", user.Username, user.Id);
            return await Task.FromResult(user);
        }

        public async Task DeleteUserAsync(string userId)
        {
            var user = _objectDb.Users.FirstOrDefault(u => u.Id == userId);
            if (user != null)
            {
                _objectDb.Users.Remove(user);
                _logger.LogInformation("Deleted user: {Username} with ID: {UserId}", user.Username, user.Id);
            }

            await Task.CompletedTask;
        }

        public async Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            var user = _objectDb.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null) return false;

            var currentPasswordHash = HashPassword(currentPassword);
            if (user.PasswordHash != currentPasswordHash)
            {
                _logger.LogWarning("Password change failed: Invalid current password for user {UserId}", userId);
                return false;
            }

            user.PasswordHash = HashPassword(newPassword);
            _logger.LogInformation("Password changed successfully for user {UserId}", userId);
            return await Task.FromResult(true);
        }

        public async Task<bool> IsUserActiveAsync(string userId)
        {
            var user = await GetUserByIdAsync(userId);
            return user?.IsActive ?? false;
        }

        public async Task SetUserActiveStatusAsync(string userId, bool isActive)
        {
            var user = _objectDb.Users.FirstOrDefault(u => u.Id == userId);
            if (user != null)
            {
                user.IsActive = isActive;
                _logger.LogInformation("Set user {UserId} active status to {IsActive}", userId, isActive);
            }

            await Task.CompletedTask;
        }

        private string HashPassword(string password)
        {
            // Simple hash for demo - in production use a proper password hashing library like BCrypt
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "SivarErpSalt"));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}
