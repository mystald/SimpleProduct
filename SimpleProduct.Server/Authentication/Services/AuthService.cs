using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SimpleProduct.Server.Authentication.Dtos;
using SimpleProduct.Server.Authentication.Models;
using SimpleProduct.Server.Data;

namespace SimpleProduct.Server.Authentication.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context,
            ITokenService tokenService,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _tokenService = tokenService;
            _configuration = configuration;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException($"User with email '{request.Email}' already exists.");
            }

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                Name = request.Name
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new ArgumentException($"Failed to register user: {errors}");
            }

            // Ensure 'User' role exists and assign
            if (!await _roleManager.RoleExistsAsync("User"))
            {
                await _roleManager.CreateAsync(new IdentityRole("User"));
            }
            await _userManager.AddToRoleAsync(user, "User");

            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = _tokenService.GenerateAccessToken(user, roles);
            var refreshToken = _tokenService.GenerateRefreshToken(user.Id);

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync(ct);

            var expirationMinutes = int.TryParse(_configuration["Jwt:AccessTokenExpirationMinutes"], out var exp) ? exp : 60;

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes),
                Email = user.Email,
                Roles = roles
            };
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = _tokenService.GenerateAccessToken(user, roles);
            var refreshToken = _tokenService.GenerateRefreshToken(user.Id);

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync(ct);

            var expirationMinutes = int.TryParse(_configuration["Jwt:AccessTokenExpirationMinutes"], out var exp) ? exp : 60;

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes),
                Email = user.Email,
                Roles = roles
            };
        }

        public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken ct = default)
        {
            var existingRefreshToken = await _context.RefreshTokens
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Token == request.RefreshToken, ct);

            if (existingRefreshToken == null || !existingRefreshToken.IsActive)
            {
                throw new UnauthorizedAccessException("Invalid or expired refresh token.");
            }

            var user = existingRefreshToken.User;
            if (user == null)
            {
                throw new UnauthorizedAccessException("User not found for this token.");
            }

            // Rotate refresh token: revoke current token
            existingRefreshToken.IsRevoked = true;
            existingRefreshToken.RevokedAt = DateTime.UtcNow;

            var roles = await _userManager.GetRolesAsync(user);
            var newAccessToken = _tokenService.GenerateAccessToken(user, roles);
            var newRefreshToken = _tokenService.GenerateRefreshToken(user.Id);

            _context.RefreshTokens.Add(newRefreshToken);
            await _context.SaveChangesAsync(ct);

            var expirationMinutes = int.TryParse(_configuration["Jwt:AccessTokenExpirationMinutes"], out var exp) ? exp : 60;

            return new AuthResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken.Token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes),
                Email = user.Email,
                Roles = roles
            };
        }

        public async Task<bool> RevokeRefreshTokenAsync(string userId, CancellationToken ct = default)
        {
            var activeTokens = await _context.RefreshTokens
                .Where(r => r.UserId == userId && !r.IsRevoked)
                .ToListAsync(ct);

            if (!activeTokens.Any())
            {
                return true;
            }

            foreach (var token in activeTokens)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<UserInfoResponse?> GetCurrentUserAsync(string userId, CancellationToken ct = default)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return null;
            }

            var roles = await _userManager.GetRolesAsync(user);

            return new UserInfoResponse
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                Name = user.Name,
                Roles = roles
            };
        }
    }
}
