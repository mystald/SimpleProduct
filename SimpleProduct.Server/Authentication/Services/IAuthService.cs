using SimpleProduct.Server.Authentication.Dtos;

namespace SimpleProduct.Server.Authentication.Services
{
    public interface IAuthService
    {
        /// <summary>
        /// Registers a new user, assigns default 'User' role, and returns authentication tokens.
        /// </summary>
        Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);

        /// <summary>
        /// Authenticates user credentials and returns new authentication tokens.
        /// </summary>
        Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);

        /// <summary>
        /// Validates refresh token, rotates it, and issues new token pair.
        /// </summary>
        Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken ct = default);

        /// <summary>
        /// Revokes all active refresh tokens for the given user ID.
        /// </summary>
        Task<bool> RevokeRefreshTokenAsync(string userId, CancellationToken ct = default);

        /// <summary>
        /// Retrieves the current user's profile and roles.
        /// </summary>
        Task<UserInfoResponse?> GetCurrentUserAsync(string userId, CancellationToken ct = default);
    }
}
