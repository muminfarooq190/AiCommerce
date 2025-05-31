using EcommerceApi.Models;
using System.Security.Claims;

namespace EcommerceApi.Providers
{
    /// <summary>
    /// Provides strongly typed access to user-specific claims for the current HTTP request.
    /// </summary>
    public sealed class UserProvider : IUserProvider
    {
        private readonly Guid? _tenantId;
        private readonly Guid? _userId;
        private readonly string? _email;
        private readonly string? _companyName;
        private readonly bool _isAuthenticated;

        /// <summary>
        /// Initializes a new <see cref="UserProvider"/> for the current request.
        /// </summary>
        /// <param name="httpContextAccessor">Accessor used to obtain the <see cref="ClaimsPrincipal"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="httpContextAccessor"/> is <c>null</c>.</exception>
        public UserProvider(IHttpContextAccessor httpContextAccessor)
        {
            if (httpContextAccessor is null)
                throw new ArgumentNullException(nameof(httpContextAccessor));

            ClaimsPrincipal? claims = httpContextAccessor.HttpContext?.User;
            _isAuthenticated = claims?.Identity?.IsAuthenticated ?? false;

            // Extract and validate required claims.
            _tenantId = TryGetGuidClaim(claims, AppClaims.TenantId, ref _isAuthenticated);
            _userId = TryGetGuidClaim(claims, AppClaims.UserId, ref _isAuthenticated);
            _email = TryGetStringClaim(claims, AppClaims.Email, ref _isAuthenticated);
            _companyName = TryGetStringClaim(claims, AppClaims.CompanyName, ref _isAuthenticated);
        }

        /// <inheritdoc/>
        public Guid TenantId =>
            _tenantId ?? throw new InvalidOperationException("TenantId claim is missing or invalid.");

        /// <inheritdoc/>
        public Guid UserId =>
            _userId ?? throw new InvalidOperationException("UserId claim is missing or invalid.");

        /// <inheritdoc/>
        public string Email =>
            _email ?? throw new InvalidOperationException("Email claim is missing.");
        
        public string CompanyName =>
            _companyName ?? throw new InvalidOperationException("CompanyName claim is missing.");

        /// <inheritdoc/>
        public bool IsAuthenticated => _isAuthenticated;


        #region Helper methods

        private static Guid? TryGetGuidClaim(ClaimsPrincipal? principal, string claimType, ref bool authFlag)
        {
            string? value = principal?.FindFirst(claimType)?.Value;
            if (value is null || !Guid.TryParse(value, out Guid result))
            {
                authFlag = false;
                return null;
            }
            return result;
        }

        private static string? TryGetStringClaim(ClaimsPrincipal? principal, string claimType, ref bool authFlag)
        {
            string? value = principal?.FindFirst(claimType)?.Value;
            if (string.IsNullOrWhiteSpace(value))
            {
                authFlag = false;
                return null;
            }
            return value;
        }

        #endregion
    }
}
