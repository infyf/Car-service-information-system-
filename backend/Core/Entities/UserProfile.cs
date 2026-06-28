namespace AutoServ.Core.Entities
{
    public class UserProfile
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? AvatarUrl { get; set; }
        public string? CarBrand { get; set; }
        public string? CarModel { get; set; }
        public int? CarYear { get; set; }
        public string? CarEngine { get; set; }
        public string? CarPlate { get; set; }
        public string? CarImageUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;
    }
}