using System.ComponentModel.DataAnnotations;

namespace SimpleProduct.Server.Authentication.Dtos
{
    public class RegisterRequest
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        public required string Password { get; set; }

        [MaxLength(100)]
        public string? Name { get; set; }
    }
}
