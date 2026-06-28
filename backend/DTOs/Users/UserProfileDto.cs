using System.Text.Json.Serialization;

namespace AutoServ.DTOs.Users
{
    public class UserProfileDto
    {
        public int UserId { get; set; }
        public string Email { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Phone { get; set; } = null!;

        [JsonPropertyName("avatarUrl")]
        public string? AvatarUrl { get; set; }

        [JsonPropertyName("carBrand")]
        public string? CarBrand { get; set; }

        [JsonPropertyName("carModel")]
        public string? CarModel { get; set; }

        [JsonPropertyName("carYear")]
        public int? CarYear { get; set; }

        [JsonPropertyName("carEngine")]
        public string? CarEngine { get; set; }

        [JsonPropertyName("carPlate")]
        public string? CarPlate { get; set; }

        [JsonPropertyName("carImageUrl")]
        public string? CarImageUrl { get; set; }
    }
}