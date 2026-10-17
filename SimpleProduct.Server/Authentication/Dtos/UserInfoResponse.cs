namespace SimpleProduct.Server.Authentication.Dtos
{
    public class UserInfoResponse
    {
        public required string Id { get; set; }
        public required string Email { get; set; }
        public string? Name { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
    }
}
