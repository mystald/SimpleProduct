using SimpleProduct.Server.Authentication.Dtos;

namespace SimpleProduct.Server.Authentication.Services
{
    public interface IAuthService
    {
        /// <summary>
        /// Registers a new user, assigns default 'User' role, and returns authentication tokens.
        /// </summary>
        Task<AuthResponse> RegisterAsync(RegisterRequest request);

        /// <summary>
        /// Authenticates user credentials and returns new authentication tokens.
        /// </summary>
        Task<AuthResponse> LoginAsync(LoginRequest request);

        /// <summary>
        /// Validates refresh token, rotates it, and issues new token pair.
        /// </summary>
        Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);

        /// <summary>
        /// Revokes all active refresh tokens for the given user ID.
        /// </summary>
        Task<bool> RevokeRefreshTokenAsync(string userId);

        /// <summary>
        /// Retrieves the current user's profile and roles.
        /// </summary>
        Task<UserInfoResponse?> GetCurrentUserAsync(string userId);
    }
}
