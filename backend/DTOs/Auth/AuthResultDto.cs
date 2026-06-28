namespace AutoServ.DTOs.Auth
{
    public class AuthResultDto
    {
        public string Token { get; set; } = null!;
        public UserDto User { get; set; } = null!;
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? Phone { get; set; }
        public string Role { get; set; } = null!;
    }
}
