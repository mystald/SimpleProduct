namespace SimpleProduct.Server.Authentication.Dtos
{
    public class AuthResponse
    {
        public required string AccessToken { get; set; }
        public required string RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string? Email { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
    }
}
