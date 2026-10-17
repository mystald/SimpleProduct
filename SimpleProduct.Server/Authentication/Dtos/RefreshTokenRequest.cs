using System.ComponentModel.DataAnnotations;

namespace SimpleProduct.Server.Authentication.Dtos
{
    public class RefreshTokenRequest
    {
        [Required]
        public required string RefreshToken { get; set; }
    }
}
