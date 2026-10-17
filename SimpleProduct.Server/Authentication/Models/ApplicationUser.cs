using Microsoft.AspNetCore.Identity;

namespace SimpleProduct.Server.Authentication.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? Name { get; set; }
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
