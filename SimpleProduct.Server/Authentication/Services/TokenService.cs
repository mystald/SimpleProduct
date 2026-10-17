using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SimpleProduct.Server.Authentication.Models;

namespace SimpleProduct.Server.Authentication.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateAccessToken(ApplicationUser user, IList<string> roles)
        {
            var keyString = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured.");
            var issuer = _configuration["Jwt:Issuer"] ?? "SimpleProduct.Server";
            var audience = _configuration["Jwt:Audience"] ?? "SimpleProduct.Client";
            var expirationMinutes = int.TryParse(_configuration["Jwt:AccessTokenExpirationMinutes"], out var exp) ? exp : 60;

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Name, user.UserName ?? user.Email ?? string.Empty)
            };

            if (!string.IsNullOrWhiteSpace(user.Name))
            {
                claims.Add(new Claim(JwtRegisteredClaimNames.Name, user.Name));
            }

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = credentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public RefreshToken GenerateRefreshToken(string userId)
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            var tokenString = Convert.ToBase64String(randomNumber);

            var expirationDays = int.TryParse(_configuration["Jwt:RefreshTokenExpirationDays"], out var exp) ? exp : 7;

            return new RefreshToken
            {
                Token = tokenString,
                UserId = userId,
                ExpiresAt = DateTime.UtcNow.AddDays(expirationDays),
                CreatedAt = DateTime.UtcNow
            };
        }

        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var keyString = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured.");
            var issuer = _configuration["Jwt:Issuer"] ?? "SimpleProduct.Server";
            var audience = _configuration["Jwt:Audience"] ?? "SimpleProduct.Client";

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString)),
                ValidateLifetime = false // Do not validate lifetime here to allow refreshing expired tokens
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }
    }
}
