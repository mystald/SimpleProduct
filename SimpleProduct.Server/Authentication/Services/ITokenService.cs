using System.Security.Claims;
using SimpleProduct.Server.Authentication.Models;

namespace SimpleProduct.Server.Authentication.Services
{
    public interface ITokenService
    {
        /// <summary>
        /// Generates a JWT access token containing claims for user id, email, name, and roles.
        /// </summary>
        string GenerateAccessToken(ApplicationUser user, IList<string> roles);

        /// <summary>
        /// Generates a cryptographically secure random refresh token.
        /// </summary>
        RefreshToken GenerateRefreshToken(string userId);

        /// <summary>
        /// Extracts the ClaimsPrincipal from an expired or active token without validating its lifetime.
        /// </summary>
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }
}
