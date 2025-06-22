using System.Security.Claims;
using Sivar.Erp.ErpSystem.Modules.Security.Core;

namespace Sivar.Erp.ErpSystem.Modules.Security.Platform
{
    public class GenericSecurityContext : ISecurityContext
    {
        private ClaimsPrincipal? _currentUser;
        private readonly List<string> _roles = new();
        private readonly List<string> _permissions = new();

        public event EventHandler<SecurityContextChangedEventArgs>? SecurityContextChanged;

        public ClaimsPrincipal? CurrentUser => _currentUser;
        public string? UserId { get; private set; }
        public string? UserName { get; private set; }
        public bool IsAuthenticated { get; private set; }
        public IReadOnlyList<string> Roles => _roles.AsReadOnly();
        public IReadOnlyList<string> Permissions => _permissions.AsReadOnly();
        public DateTime? LoginTime { get; private set; }
        public DateTime? LastActivity { get; private set; }

        public virtual void SetCurrentUser(IUser user)
        {
            var previousUserId = UserId;

            UserId = user.Id;
            UserName = user.Username;
            IsAuthenticated = true;
            LoginTime = DateTime.UtcNow;
            LastActivity = DateTime.UtcNow;

            _roles.Clear();
            _roles.AddRange(user.Roles);

            _permissions.Clear();
            _permissions.AddRange(user.DirectPermissions);

            // Create claims principal
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Name, user.Username)
            };

            // Add user properties if it's a concrete User
            if (user is User concreteUser)
            {
                if (!string.IsNullOrEmpty(concreteUser.Email))
                    claims.Add(new Claim(ClaimTypes.Email, concreteUser.Email));
                if (!string.IsNullOrEmpty(concreteUser.FirstName))
                    claims.Add(new Claim(ClaimTypes.GivenName, concreteUser.FirstName));
                if (!string.IsNullOrEmpty(concreteUser.LastName))
                    claims.Add(new Claim(ClaimTypes.Surname, concreteUser.LastName));
            }

            claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));
            claims.AddRange(user.DirectPermissions.Select(perm => new Claim("permission", perm)));

            var identity = new ClaimsIdentity(claims, "Generic");
            _currentUser = new ClaimsPrincipal(identity);

            OnSecurityContextChanged(new SecurityContextChangedEventArgs
            {
                PreviousUserId = previousUserId,
                NewUserId = UserId,
                ChangeType = SecurityContextChangeType.Login
            });
        }

        public virtual void Logout()
        {
            var previousUserId = UserId;

            _currentUser = null;
            UserId = null;
            UserName = null;
            IsAuthenticated = false;
            LoginTime = null;
            LastActivity = null;
            _roles.Clear();
            _permissions.Clear();

            OnSecurityContextChanged(new SecurityContextChangedEventArgs
            {
                PreviousUserId = previousUserId,
                NewUserId = null,
                ChangeType = SecurityContextChangeType.Logout
            });
        }

        public void UpdateLastActivity()
        {
            LastActivity = DateTime.UtcNow;
        }

        protected virtual void OnSecurityContextChanged(SecurityContextChangedEventArgs e)
        {
            SecurityContextChanged?.Invoke(this, e);
        }
    }
}
